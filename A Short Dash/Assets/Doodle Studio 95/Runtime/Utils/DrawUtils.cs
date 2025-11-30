using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.IO;

// Lets us access internal variables from Editor scripts
using System.Runtime.CompilerServices;
[assembly:InternalsVisibleTo("Assembly-CSharp-Editor")]
[assembly:InternalsVisibleTo("DoodleStudio95Editor")]

namespace DoodleStudio95 {
public static class DrawUtils {
		
		internal static Color32 TRANSPARENCY_COLOR = new Color32(0,0,0,0);

		public static int GetParameterInName(string Input, string ParameterName) {
			var matches = Regex.Matches(Input, ParameterName + "_([0-9]+)");
			if (matches.Count > 0) {
				try {
					return System.Convert.ToInt32(matches[0].Groups[1].Value);
				} catch (System.FormatException) {
					return -1;
				}
			}
			//Debug.LogWarningFormat("No parameter {0} in {1}", ParameterName, Input);
			return -1;
		}

		internal static void EnsureDirectoryExistsForFile(string filename) {
			var directoryName = Path.GetDirectoryName(filename);
			if (!Directory.Exists(directoryName)) {
				Directory.CreateDirectory(directoryName);
			}
		}

		internal static bool IsImageAsset(string assetPath) {
			if (string.IsNullOrEmpty(assetPath))
				return false;
			var extension = Path.GetExtension(assetPath).ToLower();
			return new string[] {".png", ".jpg", ".asset", ".jpeg", ".psd", ".bmp", ".tga", ".tif" }.Contains(extension);
		}

		internal static bool TexturedButton(Texture2D Texture, string Tooltip = "", int Width = 30, int Height = 30, GUIStyle Style = null) {
			return GUILayout.Button(
				new GUIContent(Texture, Tooltip), 
				Style,
				new GUILayoutOption[] { GUILayout.Width(Width), GUILayout.Height(Height) }
			);
		}

		static System.Random _R = new System.Random();
		internal static void Shuffle<T>(T[] array)
		{
				int n = array.Length;
				while (n > 1) {
					n--;
					int k = _R.Next(n + 1);
					T val = array[k];
					array[k] = array[n];
					array[n] = val;
				}
		}
		
		internal static Color ColorFromString(string str) {
			Color c = Color.magenta;
			if (string.IsNullOrEmpty(str) || !ColorUtility.TryParseHtmlString ("#" + str.ToUpper (), out c))
				Debug.LogErrorFormat("Error converting color {0}", str);
			return c;
		}

		internal static int nextPerfectSquare(uint number) {
			int c = (int)Mathf.Sqrt ((float)number) + 1;
			return c * c;
		}

		internal static int keyframeLengthForSprite(Sprite sprite) {
			return DrawUtils.GetParameterInName(sprite.name, "len");
		}

		internal delegate void CopyEditorTexturePropertiesFunc(Texture2D dest, Texture2D source);
		internal static CopyEditorTexturePropertiesFunc CopyEditorTextureProperties;

		/// Get a texture by copying the texture directly, or blitting if format is unsupported
		internal static Texture2D GetTextureCopy(Texture2D Source, Rect? Rect = null, bool Repeat = false, bool apply = true) {
			if (Source == null) {
				Debug.LogWarning("Tried to copy a null texture");
				return null;
			}
			float srcwidth = Source.width;
			float srcheight = Source.height;
			var rect = Rect != null ? Rect.GetValueOrDefault() : new Rect(0, 0, srcwidth, srcheight);
			Texture2D dest;

			// Slowest way if we need to wrap pixels around
			if (Repeat) {
				dest = new Texture2D((int)rect.width, (int)rect.height);
				dest.filterMode = Source.filterMode;
				if (CopyEditorTextureProperties != null)
					CopyEditorTextureProperties(dest, Source);
				for(int xx = 0; xx < rect.width; xx++) {
					for(int yy = 0; yy < rect.height; yy++) {
						float srcx = Mathf.Repeat(rect.x + xx, srcwidth);
						float srcy = Mathf.Repeat(rect.y + yy, srcheight);
						dest.SetPixel(xx, yy, Source.GetPixel((int)srcx, (int)srcy));
					}
				}
				if (apply)
					dest.Apply();
				return dest;
			}

			rect = rect.CropToFit(new Rect(0,0,Source.width,Source.height));

			// Fast way if source texture format is the same as the one we'll display at
			if (Source.format == TextureFormat.ARGB32) {
				dest = new Texture2D((int)rect.width, (int)rect.height, Source.format, false);
				Graphics.CopyTexture(Source, 0, 0, (int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height, dest, 0, 0, 0, 0);
				dest.filterMode = Source.filterMode;
				if (CopyEditorTextureProperties != null)
					CopyEditorTextureProperties(dest, Source);
				if (apply)
					dest.Apply();
				return dest;
			}
			
			// Slower, blitting. Note: texture asset must be readable, see SetReadable()
			dest = new Texture2D((int)rect.width, (int)rect.height);
			dest.filterMode = Source.filterMode;
			if (CopyEditorTextureProperties != null)
				CopyEditorTextureProperties(dest, Source);
			dest.SetPixels(Source.GetPixels((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height, 0));
			if (apply)
				dest.Apply();
			return dest;
		}

		internal static Texture2D GetEmptyTexture(int width, int height, FilterMode filterMode = FilterMode.Trilinear, TextureFormat format = TextureFormat.ARGB32) {
			var t = new Texture2D(width, height, format, true);
			t.filterMode = filterMode;
			var pixels = new Color[width * height];
			for(int i = 0; i < pixels.Length; i++) { pixels[i] = Color.clear; }
			t.SetPixels(pixels);
			t.Apply();
			return t;
		}

		internal static void Stamp(Texture2D Stamp, Texture2D Dest, Rect Rect) {
			//Rect.width = Mathf.Clamp((int)Rect.width, 0, Dest.width);
			//Rect.height = Mathf.Clamp((int)Rect.height, 0, Dest.height);
			Rect.y = -Rect.y - Dest.height - Rect.height; // Switch to bottom up coords
			for (float xx = 0; xx < Rect.width; xx++) {
				for (float yy = 0; yy < Rect.height; yy++) {
					var c = Stamp.GetPixelBilinear(xx / Rect.width, yy / Rect.height);
					if (c.a > 0) {
						int rx = (int)(Rect.x + xx);
						int ry = (int)(Rect.y + yy);
						Dest.SetPixel(rx, ry, Color.Lerp(Dest.GetPixel(rx, ry), c, c.a));
					}
				}
			}
		}

	internal delegate void DestroyCallbackFunc(Object obj);
	internal static DestroyCallbackFunc DestroyCallback;

	internal static void SafeDestroy(Object Obj)
	{
		if (Obj == null)
			return;
		if (DestroyCallback != null)
			DestroyCallback(Obj);
		else
			MonoBehaviour.Destroy(Obj);
	}
	public struct AtlasInfo {
		public Texture2D texture;
		public int framesX;
		public int framesY;
		public int frames;
	}
		public static AtlasInfo CreateAtlas(ref List<Rect> rects, DoodleAnimationFile file) {
			int gridWidth, gridHeight;
			var Keyframes = file.frames;
			if (Keyframes.Count > 1) {
				int nextSquare = DrawUtils.nextPerfectSquare ((uint)Keyframes.Count);
				gridWidth = (int)Mathf.Sqrt (nextSquare);
				gridHeight = Mathf.CeilToInt ((float)Keyframes.Count / (float)gridWidth);
			} else {
				gridWidth = 1;
				gridHeight = 1;
			}

			Texture2D atlas;
			{
				var width =  file.width * gridWidth;
				var height = file.height * gridHeight;
				atlas = new Texture2D (width, height, Keyframes[0].Texture.format, true);
			}

			int frameWidth = file.width;
			int frameHeight = file.height;

			var blankFrame = new Color[frameWidth * frameHeight];
			for (int i = 0; i < blankFrame.Length; i++) {
				blankFrame [i] = new Color32 (1, 1, 1, 0);
			}

			int x = 0;
			int y = atlas.height - frameHeight;
			int framesx = 0;
			int framesy = 0;
			int len = (atlas.width / frameWidth) * (atlas.height / frameHeight);
			for (var k = 0; k < len; ++k) {
				//EditorUtility.DisplayProgressBar ("Saving...", "Saving...", (float)k / (float)len);
				if (k < Keyframes.Count) {
					var keyframe = Keyframes[k];
					if (keyframe.Sprite != null) {
						Graphics.CopyTexture(keyframe.Texture, 0, 0, 0, 0, file.width, file.height, atlas, 0, 0, x, y);
					} else if (Keyframes [k].m_Layers != null && Keyframes [k].m_Layers.Length > 0) {
						// LEGACY CODE FOR USING SERIALIZED TEXTURES
						for (int li = 0; li < Keyframes [k].m_Layers.Length; li++) {
							var layer = Keyframes [k].m_Layers [li];
							if (li == 0) {
								Graphics.CopyTexture (layer.texture, 0, 0, 0, 0, file.width, file.height, atlas, 0, 0, x, y);
							} else {
								for (int xx = 0; xx < file.width; xx++) {
									for (int yy = 0; yy < file.height; yy++) {
										var prev = Keyframes [k].m_Layers [li - 1].GetPixelFast (xx, yy);
										//if (prev.a > 0) // TODO: reverse the order of blitting and do this to avoid unnecesary pixels drawn
										//continue;
										var current = layer.GetPixelFast (xx, yy);
										atlas.SetPixel (x + xx, y + yy, Color.Lerp (prev, current, current.a));
									}
								}
							}
						}
					}
					rects.Add (new Rect (x, y, frameWidth, frameHeight));
				} else {
					// fill empty frames with (0,0,0,0)
					atlas.SetPixels(x, y, frameWidth, frameHeight, blankFrame, 0);
				}
				x += frameWidth;
				if (y == 0)
					framesx++;
				if (x >= atlas.width) {
					y -= frameHeight;
					x = 0;
					framesy++;
				}
			}
			atlas.Apply ();
			atlas.name = "Atlas (Runtime";
    	//EditorUtility.ClearProgressBar ();

			AtlasInfo atlasInfo;
			atlasInfo.texture = atlas;
			atlasInfo.frames = Keyframes.Count;
			atlasInfo.framesX = framesx;
			atlasInfo.framesY = framesy;

			return atlasInfo;
		}

		public struct Line { 
			public Vector2 from; 
			public Vector2 to; 
			public Line(Vector2 from, Vector2 to) { this.from = from; this.to = to; }
		}

		// Breaks up a line into multiple lines that stay inside a rectangle
		public static List<Line> GetLinesInsideBounds(Line line, Vector2 size)
		{
			var lines = new List<Line>();
			bool done = false;
			var from = new Vector2(line.from.x, line.from.y);
			var to = new Vector2(line.to.x, line.to.y);
			int tries = 0;
			Vector2 intersectionPoint = Vector2.zero;

			Line[] sides = new Line[] {
				new Line(new Vector2(0,0), new Vector2(size.x,0)),
				new Line(new Vector2(size.x,0), new Vector2(size.x,size.y)),
				new Line(new Vector2(size.x,size.y), new Vector2(0,size.y)),
				new Line(new Vector2(0,size.y), new Vector2(0,0)),
			};
			Vector2? lastIntersectionPoint = null;
			while(!done) {
				// Get the cell number for each coordinate
				int cx1 = cell(from.x, size.x);
				int cx2 = cell(to.x, 	size.x);
				int cy1 = cell(from.y, size.y);
				int cy2 = cell(to.y, 	size.y);

				// Don't move more than one cell at a time
				cx2 = cx1 + Mathf.Clamp(cx2 - cx1, -1, 1);
				cy2 = cy1 + Mathf.Clamp(cy2 - cy1, -1, 1);
				
				// bool intersection = false;
				// if (cx1 != cx2 || cy1 != cy2) {
				// 	intersection = true;
				// 	// Get the point at the boundary
				// 	if (cx1 != cx2) {
				// 		if (cx2 >= 0) 
				// 			intersectionPoint.x = size.x * cx2;
				// 		else
				// 			intersectionPoint.x = size.x * (cx2 + 1);
				// 	}
				// 	if (cy1 != cy2) {
				// 		if (cy2 >= 0) 
				// 			intersectionPoint.y = size.y * cy2;
				// 		else
				// 			intersectionPoint.y = size.y * (cy2 + 1);
				// 	}
				// 	// HACK: Add a tiny difference so that the new intersection point falls on the next cell
				// 	intersectionPoint.x += Mathf.Clamp(cx2 - cx1, -1, 1) * 0.001f;
				// 	intersectionPoint.y += Mathf.Clamp(cy2 - cy1, -1, 1) * 0.001f;
				// 	// Ignore intersections that are equal to the last one
				// 	if (lastIntersectionPoint != null && Mathf.Approximately(Vector2.Distance(lastIntersectionPoint.GetValueOrDefault(), intersectionPoint) , 0))
				// 		intersection = false;
					
				// 	if (intersection)
				// 		Debug.Log("    * point in boundary from " + from + " (in cell " + cx1 +","+cy1+") to " + to + " (in cell "+cx2+","+cy2+") is " + intersectionPoint);
				// }

				// Faster method, but TODO move the sides to the right cell
				bool intersection = false;
				foreach(var s in sides) {
					var sideFrom = s.from + new Vector2(size.x * cx1, size.y * cy1);
					var sideTo = s.to + new Vector2(size.x * cx1, size.y * cy1);
					// Check for intersection for each side, bu
					if (Intersects(from, to, sideFrom, sideTo, out intersectionPoint) && 
						// ignore intersections that are equal to the last one
						(lastIntersectionPoint == null || Vector2.Distance(intersectionPoint, lastIntersectionPoint.GetValueOrDefault()) > 0)) {
						// HACK: Add a tiny difference so that the new intersection point falls on the next cell
						intersectionPoint.x += Mathf.Clamp(cx2 - cx1, -1, 1) * 0.001f;
						intersectionPoint.y += Mathf.Clamp(cy2 - cy1, -1, 1) * 0.001f;
						intersection = true;
						break;
					}
				}

				if (intersection) {
					Debug.Log("    * point in boundary from " + from + " to " + to + " is " + intersectionPoint);
					lines.Add(new Line(from, intersectionPoint));
					lastIntersectionPoint = intersectionPoint;
					// Next cycle we start from the last intersection
					from = intersectionPoint;
				} else {
					// No intersections, add the full line and we're done
					lines.Add(new Line(from, to));
					done = true;
				}

				// Debug, safety check
				tries++;
				if (tries > 50)
					done = true;
			}
			return lines;
		}
		static int cell(float p, float size) { return Mathf.FloorToInt(p / size); }

		public static void DebugBoundLines(Line line, Vector2 size) {
			Debug.Log("for line from " + line.from + " to " + line.to + ":");
			int i = 0;
			foreach(var l in DrawUtils.GetLinesInsideBounds(line, size)) {
				Debug.Log("   > line "+i+", from " + l.from + " to " + l.to);
				i++;
			}
		}

		// a1 is line1 start, a2 is line1 end, b1 is line2 start, b2 is line2 end
		static bool Intersects(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2, out Vector2 intersection)
		{
				intersection = Vector2.zero;

				Vector2 b = a2 - a1;
				Vector2 d = b2 - b1;
				float bDotDPerp = b.x * d.y - b.y * d.x;

				// if b dot d == 0, it means the lines are parallel so have infinite intersection points
				if (bDotDPerp == 0)
						return false;

				Vector2 c = b1 - a1;
				float t = (c.x * d.y - c.y * d.x) / bDotDPerp;
				if (t < 0 || t > 1)
						return false;

				float u = (c.x * b.y - c.y * b.y) / bDotDPerp;
				if (u < 0 || u > 1)
						return false;

				intersection = a1 + t * b;

				return true;
		}

		public static Vector2 Repeat(this Vector2 v, Vector2 size) {
			v.x = Mathf.Repeat(v.x, size.x);
			v.y = Mathf.Repeat(v.y, size.y);
			return v;
		}

	}
}