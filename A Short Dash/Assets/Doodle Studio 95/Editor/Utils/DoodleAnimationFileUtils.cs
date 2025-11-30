using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.U2D.Sprites;

namespace DoodleStudio95 {
using FramesPerSecond = DoodleAnimationFile.FramesPerSecond;
using PlaybackMode = DoodleAnimationFile.PlaybackMode;
using Compression = DrawPrefs.Compression;
using PatternMode = DoodleAnimationFile.PatternMode;
internal static class DoodleAnimationFileUtils {

	// internal string AssetPath { get{ return UnityEditor.AssetDatabase.GetAssetPath(this); } }

	internal static bool IsOldVersion(DoodleAnimationFile file) {
		foreach(var f in file.frames) {
			if (
				(f.m_Layers != null && f.m_Layers.Length > 0) ||
				(f.m_Sprites != null && f.m_Sprites.Length > 0) ||
				(f.m_Textures != null && f.m_Textures.Length > 0) || 
				f.Sprite == null
			)
				return true;
		}
		return false;
	}

	internal static void Resave(this DoodleAnimationFile file) {
		var tempKeyframes = new List<DrawWindowKeyframe>();
		foreach(var kf in file.frames) {
			tempKeyframes.Add(new DrawWindowKeyframe(kf));
		}
		file.version = DrawPrefs.VERSION;
		file.ClearSubAssets();
		file.CreateAllSpritesAndTextures(tempKeyframes);
		file.SaveSubAssets();
		AssetDatabase.Refresh();
		EditorUtility.SetDirty(file);
		AssetDatabase.SaveAssets();
	}
	internal static void CreateAllSpritesAndTextures(this DoodleAnimationFile file, List<DrawWindowKeyframe> EditableKeyframes) {
    file.frames = new List<DoodleAnimationFileKeyframe>();
		float i = 0;
		UnityEditor.EditorUtility.DisplayProgressBar("Saving", "Saving keyfames...", 0);
		foreach(var k in EditableKeyframes) {
			UnityEditor.EditorUtility.DisplayProgressBar("Saving", "Saving keyfames...", i / (float)EditableKeyframes.Count);

			try {
				// Stored keyframe has no serialized data, only length, a texture and a sprite
				var newkf = new DoodleAnimationFileKeyframe(k.m_Length);
				var tex = DrawUtils.GetTextureCopy(k.resultTexture);
				tex.filterMode = file.filterMode;
				tex.wrapMode = file.patternMode == PatternMode.Disabled ? TextureWrapMode.Clamp : TextureWrapMode.Repeat;

				// Compress
				var c = DrawPrefs.Instance.m_Compression;
				if (c != Compression.None) {
					if (c == Compression.DXT5)
						EditorUtility.CompressTexture(tex, TextureFormat.DXT5, (int)UnityEditor.TextureCompressionQuality.Best);
				}
				
				var s = Sprite.Create(
					tex, 
					new Rect(0, 0, k.width, k.height), 
					Vector2.one * .5f, 100, 0,
					SpriteMeshType.FullRect, 
					file.spriteBorder
				);
				s.name = tex.name = i + " (File)";
				tex.hideFlags = s.hideFlags = HideFlags.HideInHierarchy;

				newkf.Texture = tex;
				newkf.Sprite = s;

				file.frames.Add(newkf);
				i++;
			} catch(System.Exception e) {
				Debug.LogException(e);
			}
		}
		UnityEditor.EditorUtility.ClearProgressBar();
		file._timeline = null;
	}

  // Cleanup all the assets in case we're replacing a file
  internal static void ClearSubAssets(this DoodleAnimationFile file) {
    if (file.frames == null)
      return;
    foreach(var k in file.frames) {
      if (k.Sprite != null) 
				Object.DestroyImmediate(k.Sprite, true);
      
      if (k.Texture != null) 
				Object.DestroyImmediate(k.Texture, true);
    }
  }
  internal static void SaveSubAssets(this DoodleAnimationFile file) {
		foreach(var k in file.frames) {
      if (k.Texture) AssetDatabase.AddObjectToAsset(k.Texture, file);
      if (k.Sprite) AssetDatabase.AddObjectToAsset(k.Sprite, file);
    }
    
  }

	// Load
	internal static DoodleAnimationFile FromTexture(string assetPath) {
		DoodleAnimationFile file = ScriptableObject.CreateInstance<DoodleAnimationFile>();
		// Attempt to load from sprites (Legacy)
		var sprites = EditorUtils.GetOrderedSprites(assetPath);

		var texture = AssetDatabase.LoadAssetAtPath<Texture2D> (assetPath);

		// Store settings to restore them later
		var settings = TextureImporter.GetAtPath(assetPath) as TextureImporter;
		bool prevReadable = settings.isReadable;
		int prevMaxSize = settings.maxTextureSize;
		TextureImporterNPOTScale prevNpot = settings.npotScale;
		
		// Make sure we're loading the highest quality and we can read it
		if (!prevReadable || prevMaxSize < DoodleAnimationFile.SPRITESHEET_MAX_SIZE) {
			settings.isReadable = true;
			settings.maxTextureSize = DoodleAnimationFile.SPRITESHEET_MAX_SIZE;
			settings.npotScale = TextureImporterNPOTScale.None;
			AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport);
		}
		file.filterMode = texture.filterMode;

		if (sprites.Length > 0) {
			file.frames = new List<DoodleAnimationFileKeyframe> ();
			for (int i = 0; i < sprites.Length; i++) {
				EditorUtility.DisplayProgressBar ("Loading...", "Loading...", (float)i / (float)sprites.Length);
				file.frames.Add (new DoodleAnimationFileKeyframe(sprites [i]));
			}

			// Try and get encoded info
			int fps = DrawUtils.GetParameterInName (sprites[0].name, "fps");
			if (fps != -1)
				file.framesPerSecond = (FramesPerSecond)fps;  
			else
				file.framesPerSecond = FramesPerSecond.Normal;
			int playbackmode = DrawUtils.GetParameterInName(sprites[0].name, "playbackmode");
			if (playbackmode != -1)
				file.playbackMode = (PlaybackMode)playbackmode;
			else
				file.playbackMode = PlaybackMode.Loop;

			file.spriteBorder = sprites [0].border;
		} else {
			// No sprites, use the texture as the first frame
			var kf = new DoodleAnimationFileKeyframe(1, DrawUtils.GetTextureCopy (texture));
			file.frames = new List<DoodleAnimationFileKeyframe> () { kf };
			file.spriteBorder = Vector4.zero;
			file.playbackMode = PlaybackMode.Loop;
			file.framesPerSecond = FramesPerSecond.Normal;
		}

		// Restore settings
		if (!prevReadable || prevMaxSize < DoodleAnimationFile.SPRITESHEET_MAX_SIZE) {
			settings.isReadable = prevReadable;
			settings.maxTextureSize = prevMaxSize;
			settings.npotScale = prevNpot;
			AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport);
		}
		file.version = DrawPrefs.VERSION;
		return file;
	}

	// Save

	internal struct SaveInfo {
    internal string animationFileGUID;
    internal List<AudioClip> soundAssets;
    internal DrawUtils.AtlasInfo atlas;
  }

  internal static string NormalizePath(string path, bool isAsset = false) {
	var sep = Path.DirectorySeparatorChar;
	path = path.Replace('\\', sep);
	path = path.Replace('/', sep);
	if (isAsset && path.StartsWith("Assets" + sep + "Assets"))
		path = path.Remove(0, "Assets".Length + 1);
	return path;
  }
	
	internal static string GetAssetPath(this DoodleAnimationFile file) {
		return AssetDatabase.GetAssetPath(file);
	}
	internal static string GetFilePath(this DoodleAnimationFile file) {
		return AssetToFilePath(file.GetAssetPath());
	}

	static string LIBRARY_PATH = Application.dataPath.Substring(0, Application.dataPath.Length - "Assets/".Length);

	internal static string AssetToFilePath(string assetPath) {
		return Path.Combine(
			LIBRARY_PATH,
			assetPath
		);
	}
	// internal static string FileToAssetPath(string filePath) {
	// 	Debug.Log(Application.dataPath);
	// 	var fs = filePath.Split(Path.DirectorySeparatorChar);
	// 	Debug.Log(fs);
	// 	if (filePath.StartsWith(Application.dataPath))
	// 		filePath = filePath.Substring(Application.dataPath.Length, filePath.Length - Application.dataPath.Length);
	// 	return filePath;
	// }
	internal static string ReplaceExtension(string path, string newExt) {
		return Path.Combine(
			Path.GetDirectoryName(path),
			string.Format("{0}.{1}", Path.GetFileNameWithoutExtension(path), newExt)
		);
	}

	internal static Object SaveAsSpritesheet(this DoodleAnimationFile file) {
		Object obj = null;
		EditorUtility.DisplayProgressBar("Converting to Sprite Sheet", "Converting " + file.name + "...", 0);
		try {
			var outFilePath = ReplaceExtension( file.GetFilePath(), "png");
			var outAssetPath = ReplaceExtension( file.GetAssetPath(), "png");

			Debug.Log("outFilePath " + outFilePath);
			Debug.Log("outAssetPath " + outAssetPath);

			bool replacingTextureAsset = AssetDatabase.LoadAssetAtPath<Texture2D>(outAssetPath) != null;

			var rects = new List<Rect> ();
			var atlas = DrawUtils.CreateAtlas (ref rects, file);
			
			File.WriteAllBytes (outFilePath, atlas.texture.EncodeToPNG ());
			AssetDatabase.ImportAsset (outAssetPath, ImportAssetOptions.ForceSynchronousImport);

			var settings = TextureImporter.GetAtPath (outAssetPath) as TextureImporter;
			// Set default values on new files
			if (!replacingTextureAsset) {
                if (file.frames.Count > 1)
                    settings.spriteImportMode = SpriteImportMode.Multiple;
                else
                    settings.spriteImportMode = SpriteImportMode.Single;
				settings.alphaIsTransparency = true;
				settings.compressionQuality = 100;
				settings.textureType = TextureImporterType.Sprite;
				settings.textureCompression = TextureImporterCompression.CompressedHQ;
				settings.filterMode = file.filterMode;
				settings.mipmapEnabled = file.filterMode != FilterMode.Point;
				settings.maxTextureSize = DoodleAnimationFile.SPRITESHEET_MAX_SIZE;
			}
		
			// Create sprites, only if we're making a new file, or there're sprites already
			if (replacingTextureAsset || settings.textureType == TextureImporterType.Sprite) {
			
                var factory = new SpriteDataProviderFactories();
                factory.Init();
                var dataProvider = factory.GetSpriteEditorDataProviderFromObject(settings);
                dataProvider.InitSpriteEditorDataProvider();
                var spriteRects = dataProvider.GetSpriteRects();

				var rectIndex = 0;
				float t = 0;
				foreach (var kf in file.frames) {
					EditorUtility.DisplayProgressBar("Converting to Sprite Sheet", "Converting " + file.name + "...", t);
					// Use existing sprites if present
                    
                    System.Array.Resize(ref spriteRects, file.frames.Count);

					if (spriteRects[rectIndex] == null) {
						spriteRects[rectIndex] = new SpriteRect();
					}
                    spriteRects[rectIndex].pivot = Vector2.one * 0.5f;
					spriteRects[rectIndex].alignment = SpriteAlignment.Center;
					spriteRects[rectIndex].border = file.spriteBorder;
					spriteRects[rectIndex].name = "frame_" + rectIndex + "_len_" + kf.m_Length;
					if (rectIndex == 0) {
						spriteRects[rectIndex].name += "_fps_" + (int)file.framesPerSecond;
						spriteRects[rectIndex].name += "_playbackmode_" + (int)file.playbackMode;
					}
					spriteRects[rectIndex].rect = rects [rectIndex];
					rectIndex++;
					t += 1f / (float)file.frames.Count;
				}
				dataProvider.SetSpriteRects(spriteRects);
                dataProvider.Apply();

				if (replacingTextureAsset) {
					// For some reason Unity won't apply changes to the spritesheet unless the textureType is reset
					bool mipmaps = settings.mipmapEnabled;
					TextureWrapMode wrapMode = settings.wrapMode;
					TextureImporterType type = settings.textureType;
					settings.textureType = TextureImporterType.Default;
					settings.textureType = type;
					settings.spriteImportMode = SpriteImportMode.Multiple;
					settings.mipmapEnabled = mipmaps;
					settings.wrapMode = wrapMode;
				}
			}
			AssetDatabase.ImportAsset (outAssetPath, ImportAssetOptions.ForceSynchronousImport);
			obj = AssetDatabase.LoadAssetAtPath<Texture2D>(outAssetPath);
		} catch(System.Exception e) {
			Debug.LogException(e);
		}
		EditorUtility.ClearProgressBar();
		Selection.objects = new Object[]{obj};
		return obj;
	}

	public static void SaveAsGif(this DoodleAnimationFile file, string outputPath = null) {
		EditorUtility.DisplayProgressBar("Converting to GIF", "Converting " + file.name + " to GIF...", 0);
		try {
			var outFilePath = 	ReplaceExtension( file.GetFilePath(), "gif");
			var outAssetPath = 	ReplaceExtension( file.GetAssetPath(), "gif");

			// If the image uses a color close to this one it'll get treated as transparent
			// TODO: change the transparent color until it's not used in the non-transparent pixels
			Color32 transparentColor = new Color32(255,125,255,255);

			uGIF.GIFEncoder ge = new uGIF.GIFEncoder();
			ge.useGlobalColorTable = true;
			ge.repeat = file.playbackMode == PlaybackMode.Once ? 1 : 0;
			ge.FPS = (int)file.framesPerSecond;
			ge.transparent = transparentColor;
			
			System.IO.MemoryStream stream = new System.IO.MemoryStream ();
			ge.Start (stream);

			// Generate the palette from all the frames. This is slow.
			ge.SetPalette(file.GenerateSpritesheet().texture.GetPixels32());

			List<int> timeline = file.Timeline;
			if (file.playbackMode == PlaybackMode.LoopBackAndForth) {
				List<int> rv = new List<int>(timeline);
				rv.Reverse();
				rv.RemoveAt(0);
				timeline.AddRange(rv);
			}
			for(int i = 0; i < timeline.Count; i++) {
				EditorUtility.DisplayProgressBar("Converting to GIF", "Converting " + file.name + " to GIF...", (float)i / timeline.Count);
				Texture2D tex = Texture2D.Instantiate(file.frames[timeline[i]].Texture);
				tex.alphaIsTransparency = true;
				// TODO: resize if the GIF is too big
				uGIF.Image img = new uGIF.Image(tex);
				// Simplify transparency to a single color
				for(int p = 0; p < img.pixels.Length; p++) {
					if (img.pixels[p].a < 1) {
						img.pixels[p] = transparentColor;
					}
				}
				img.Flip ();
				ge.AddFrame (img);
			}
			ge.Finish ();
			byte[] bytes = stream.GetBuffer ();
			stream.Close ();

			System.IO.File.WriteAllBytes (outFilePath, bytes);

			UnityEditor.AssetDatabase.Refresh();
			System.GC.Collect();

			Selection.objects = new Object[]{
				AssetDatabase.LoadMainAssetAtPath( outAssetPath )
			};
		} catch(UnityException e) {
			Debug.LogException(e);
		}

		EditorUtility.ClearProgressBar();
	}

	// Make
	public static GameObject Make3DSprite(this DoodleAnimationFile file) {
		return file.Make3DSprite( Vector3.zero, Quaternion.identity, Vector3.one );
	}
	public static GameObject Make3DSprite(this DoodleAnimationFile file, Vector3 position, Quaternion rotation, Vector3 scale) {
		var mats = AssetDatabase.FindAssets("Shadow Casting Sprite t:material");
    if (mats.Length > 0) {
      return file.Make3DSprite(position, rotation, scale, 
				AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(mats[0]))
			);
    }
		return null;
	}
}
}