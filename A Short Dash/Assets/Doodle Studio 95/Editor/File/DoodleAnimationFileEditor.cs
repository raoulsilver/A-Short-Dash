using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace DoodleStudio95 {

// [CustomPropertyDrawer(typeof(DoodleAnimationFile))]
// internal class DoodleAnimationFilePropertyDrawer : PropertyDrawer {
// 	const int EDIT_BUTTON_WIDTH = 45;
// 	const int EDIT_BUTTON_HEIGHT = 24;

// 	override internal float GetPropertyHeight(SerializedProperty property, GUIContent label) {
// 		return EDIT_BUTTON_HEIGHT;
// 		return base.GetPropertyHeight(property, label) + 0;
// 	}
// 	override internal void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
// 		// EditorGUI.PropertyField(position, property, label);

// 		Rect r = new Rect(position.x, position.y, position.width - EDIT_BUTTON_WIDTH, EDIT_BUTTON_HEIGHT);
// 		using (var l = new EditorGUI.PropertyScope(r, new GUIContent(property.displayName), property)) {
// 			// if (label != null) EditorGUIUtility.labelWidth = r.width * .23f;
// 			EditorGUI.PropertyField(r, property, l.content);
// 			EditorGUIUtility.labelWidth = 0; //Reset
// 		}
// 		// return;
// 		// Edit button		
// 		GUI.color = DrawWindow.COLOR_FILEACTION;
// 		var r_edit = new Rect(position.xMax - EDIT_BUTTON_WIDTH, position.y, EDIT_BUTTON_WIDTH, position.height);
// 		var but = StaticResources.GetEditorSprite("buttonframe_smaller.png").DrawAsButton(r_edit);
// 		EditorSprite.DrawLabel(new Rect(r_edit.x, r_edit.y + r_edit.height * .025f, r_edit.width * .9f, r_edit.height * 1f), 
// 			"Edit", Color.black, 11);
// 		if (but) {
// 			EditorWindow.FocusWindowIfItsOpen(typeof(DrawWindow));
// 			var window = (DrawWindow)EditorWindow.GetWindow(typeof(DrawWindow), false);
// 			window.Load(AssetDatabase.GetAssetPath(property.objectReferenceValue));
// 		}
// 		GUI.color = Color.white;
// 		GUI.enabled = true;
// 	}
// }

[CanEditMultipleObjects()]
[CustomEditor(typeof(DoodleAnimationFile))]
internal class DoodleAnimationFileEditor : Editor {

	// [MenuItem("Tools/Doodle Studio 95/Convert selected animation files to Sprite sheets")]		
	[MenuItem("Assets/Doodle Studio 95/Convert to Sprite Sheet", false, 0)]
  static void ConvertToSpriteSheet() { 
		List<Object> newSelection = new List<Object>();
		foreach(var obj in Selection.objects) {
			if (obj is DoodleAnimationFile) {
				newSelection.Add((obj as DoodleAnimationFile).SaveAsSpritesheet());
			}
		}
		Selection.objects = newSelection.ToArray();
	}

	[MenuItem("Assets/Doodle Studio 95/Convert to GIF", false, 0)]
  static void ConvertToGIF() { 
		float t = 0;
		EditorUtility.DisplayProgressBar("Converting to GIF", "Converting...", t);
		foreach(var obj in Selection.objects) {
			if (obj is DoodleAnimationFile) {
				EditorUtility.DisplayProgressBar("Converting to GIF", "Converting " + obj.name, t);
				(obj as DoodleAnimationFile).SaveAsGif();
			}
			t += 1f / Selection.objects.Length;
		}
		EditorUtility.ClearProgressBar();
	}
  [MenuItem("Assets/Doodle Studio 95/Convert to Sprite Sheet", true),
		MenuItem("Assets/Doodle Studio 95/Convert to GIF", true),
		MenuItem("Assets/Doodle Studio 95/Update animation to latest version", true)
	]
  static bool ConvertValidate() { 
		if (Selection.objects.Length > 1) {
			return true;
		}
    foreach(var s in Selection.objects) {
			var ap = AssetDatabase.GetAssetPath(s);
      if (!string.IsNullOrEmpty(ap) && AssetDatabase.LoadAssetAtPath<DoodleAnimationFile>(ap) != null)
        return true;
    }
    return false;
  }

	// [MenuItem("Tools/Doodle Studio 95/Convert selected textures to Doodle Animation File")]
	[MenuItem("Assets/Doodle Studio 95/Convert to Animation File", false, 0)]
	static void ConvertToAnimationFile() {  
		List<Object> newSelection = new List<Object>();
		float t = 0;
		EditorUtility.DisplayProgressBar("Converting to Animation File", "Converting...", t);
		foreach(var obj in Selection.objects) {
			if (obj is Texture2D) {
				EditorUtility.DisplayProgressBar("Converting to Animation File", "Converting " + obj.name, t);
				string path = AssetDatabase.GetAssetPath(obj);
				var file = DoodleAnimationFileUtils.FromTexture(path);
				// TODO: ensure they're being saved
				AssetDatabase.CreateAsset(file, path + ".asset");
				file.Resave();
				newSelection.Add(file);
			}
			t += 1f / Selection.objects.Length;
		}
		EditorUtility.ClearProgressBar();
		Selection.objects = newSelection.ToArray();
	}
	[MenuItem("Assets/Doodle Studio 95/Convert to Animation File", true)]
  static bool ConvertValidateTexture() { 
		if (Selection.objects.Length > 1) {
			return true;
		}
    foreach(var s in Selection.objects) {
			var ap = AssetDatabase.GetAssetPath(s);
      if (!string.IsNullOrEmpty(ap) && AssetDatabase.LoadAssetAtPath<Texture2D>(ap) != null)
        return true;
    }
    return false;
  }

	[MenuItem("Assets/Doodle Studio 95/Update animation to latest version", false, 1)]
  static void UpdateToNewVersion() { 
		float t = 0;
		EditorUtility.DisplayProgressBar("Resaving", "Converting...", t);
		foreach(var obj in Selection.objects) {
			if (obj is DoodleAnimationFile) {
				DoodleAnimationFile anim = obj as DoodleAnimationFile;
				// if (!anim.NeedsUpdate)
				// 	continue;
				EditorUtility.DisplayProgressBar("Resaving", "Converting " + obj.name, t);
				try {
					anim.Resave();
				} catch (System.Exception e) {
					Debug.LogError("Error saving " + obj.name);
					Debug.LogException(e);
				}
			}
			t += 1f / Selection.objects.Length;
		}
		EditorUtility.ClearProgressBar();
	}

	[MenuItem("Tools/Doodle Studio 95/Rename selected game objects to their animation's name")]
	static void BatchRenameToAnimationFile() {
		foreach (var obj in Selection.gameObjects) {
			if (obj.GetComponent<DoodleAnimator>())
				obj.name = obj.GetComponent<DoodleAnimator>().File.name;
		}
	}

	public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height) {
    var t = target as DoodleAnimationFile;
		if (t.frames == null) {
			Debug.LogError("No frames to make a preview from. Did you call SaveKeyframes()?", this);
			return null;
		}
		if (t.frames.Count > 0 && t.FirstFrameTexture) {
			var texture = t.FirstFrameTexture;
			float aspect = (float)texture.height / (float)texture.width;
			Texture2D cache = new Texture2D(width, Mathf.CeilToInt(height * aspect), TextureFormat.ARGB32, false);
			// EditorUtility.CopySerialized(texture, cache);

			// Make a checkerboard pattern for transparency
			Color cs = t.darkCheckerboard ? new Color(0.22f, 0.22f, 0.22f) : new Color (0.92f, 0.92f, 0.92f);
			Color ct = t.darkCheckerboard ? new Color(.19f, .19f, .19f) : Color.white;
			int realwidth = cache.width; // output texture size is different from requested size for some reason
			int realheight = cache.height;
			int size = Mathf.CeilToInt(realwidth / 4f);
      int hsize = Mathf.FloorToInt((float)size / 2f);
			for (int xx = 0; xx < realwidth; xx++) {
				for (int yy = 0; yy < realheight; yy++) {
					var p = texture.GetPixelBilinear(xx / (float)realwidth, yy / (float)realheight);
					if (p.a > 0) {
						cache.SetPixel(xx, yy, p);
						continue;
					}
					bool solid = false;
					if (yy % size < hsize) {
						if (xx % size >= hsize)
							solid = true;
					} else if (xx % size < hsize) {
						solid = true;
					}
					cache.SetPixel(xx, yy, solid ? cs : ct);
				}
			}

			// Stamp a watermark/logo on the thumbnail to show it's an animation
			if (DrawPrefs.ADD_ONION_ICON_TO_THUMBNAILS) {
				float wsize = Mathf.Min(realwidth, realheight) * .27f;
				DrawUtils.Stamp(StaticResources.GetTexture2D("previewlogo.png"), cache, new Rect(realwidth - wsize - realwidth * 0.04f, realheight - wsize - height * 0.05f, wsize, wsize));
			}

			DrawUtils.Stamp(StaticResources.GetTexture2D("bg_thinborder_transparent.png"), cache, new Rect(0, 0, realwidth, realheight));

			cache.Apply();

			return cache;
		}
		return null;
	}

	static double _lastT = 0;
	int previewFrame = 0;

	// SerializedProperty propPlaybackMode;
	// SerializedProperty propFramesPerSecond;
	// SerializedProperty propBorder;
	// SerializedProperty propFilterMode;
	SerializedProperty propSound;

	void OnEnable() {
		// Workaround for weird bug being called when target is null, TODO: fix
		if (target == null) {
			return;
		}
		EditorApplication.update += OnUpdate;

		// propPlaybackMode = serializedObject.FindProperty("playbackMode");
		// propFramesPerSecond = serializedObject.FindProperty("framesPerSecond");
		// propBorder = serializedObject.FindProperty("spriteBorder");
		// propFilterMode = serializedObject.FindProperty("filterMode");
		propSound = serializedObject.FindProperty("sounds");
	}
	void OnDisable() {
		EditorApplication.update -= OnUpdate;

		// Workaround for weird bug being called when target is null, TODO: fix
		if (target == null)
			return;
	}
	internal void OnUpdate() {
		if(Application.isPlaying || !DoodleAnimationFilePreview.m_Play)
			return;
    var t = target as DoodleAnimationFile;
		int prevI = previewFrame;
		_lastT = EditorApplication.timeSinceStartup;
		previewFrame = (int)Mathf.Repeat((float)_lastT * (int)t.framesPerSecond, t.Length);
		if (previewFrame != prevI)
			Repaint();
	}

  override public void OnInspectorGUI() {
		GUI.enabled = !Application.isPlaying;
		var t = (target as DoodleAnimationFile);

		if (!serializedObject.isEditingMultipleObjects && 
			StaticResources.GetEditorSprite("buttonframe_smaller.png").DrawAsButtonWithLabelLayout("Edit", "", 0, 50, 18, DrawWindow.COLOR_FILEACTION)) {
			DrawWindow.OpenAndLoad(t);
		}
		using (new GUILayout.HorizontalScope()) {
			if (StaticResources.GetEditorSprite("buttonframe_smaller.png").DrawAsButtonWithLabelLayout("Add to scene...", "", 0, 35, 12)) {
				var menu = new GenericMenu();
				menu.AddItem(new GUIContent("Add as Sprite"), false, delegate() { EditorUtils.SelectAndFrame(t.MakeSprite()); });
				menu.AddItem(new GUIContent("Add as Sprite (Shadow Casting)"), false, delegate() { EditorUtils.SelectAndFrame(t.Make3DSprite()); });
				menu.AddItem(new GUIContent("Add as UI Image"), false, delegate() { EditorUtils.SelectAndFrame(t.MakeUISprite()); });
				menu.AddItem(new GUIContent("Add as Particles"), false, delegate() { EditorUtils.SelectAndFrame(t.MakeParticles().gameObject); }); 
				menu.ShowAsContext();
				Event.current.Use();
			}
			if (StaticResources.GetEditorSprite("buttonframe_smaller.png").DrawAsButtonWithLabelLayout("Convert to...", "", 0, 35, 12)) {
				var menu = new GenericMenu();
				menu.AddItem(new GUIContent("Convert to Sprite Sheet"), false, delegate() { t.SaveAsSpritesheet(); }); 
				menu.AddItem(new GUIContent("Convert to GIF"), false, delegate(){ t.SaveAsGif(); });
				menu.ShowAsContext();
				Event.current.Use();
			}
		}
		GUI.enabled = true;
		serializedObject.Update();
		// EditorGUILayout.PropertyField(propPlaybackMode);
		// EditorGUILayout.PropertyField(propFramesPerSecond);
		// EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
		// EditorGUILayout.PropertyField(propBorder);

		/* 
		EditorGUI.BeginChangeCheck();
		propBorder.vector4Value = EditorGUILayout.Vector4Field(
			new GUIContent(propBorder.displayName, "Edits the left, bottom, right and top edges of the resulting Sprite border, in pixels. Use this to add 9-slicing to UI elements."), 
			propBorder.vector4Value);
		if (EditorGUI.EndChangeCheck()) {
			// TODO: regenerate sprites
			// t.ClearSubAssets();
			// t.CreateAllSpritesAndTextures(t.frames);
			// t.SaveSubAssets();
		}
		*/
		// EditorGUILayout.PropertyField(propFilterMode);

		if (propSound != null) {
			for(int i = 0; i < propSound.arraySize; i++) {
				EditorGUILayout.ObjectField("Sound #" + (i+1), 
					propSound.GetArrayElementAtIndex(i).objectReferenceValue, 
					typeof(AudioClip), false);
			}
		}

		if (!serializedObject.isEditingMultipleObjects) {
			if (Event.current.alt)
				EditorGUILayout.LabelField("Saved with version " + t.version, EditorStyles.miniLabel);

			if (DoodleAnimationFileUtils.IsOldVersion(t)) {
				EditorGUILayout.HelpBox("This animation was saved with an older version of Doodle Studio. \nClick Update to make it load faster!", MessageType.Warning);
				GUI.color = Color.yellow;
				if (GUILayout.Button("Update"))
					t.Resave();
				GUI.color = Color.white;
			}
			if (t.frames == null || t.frames.Count == 0 || t.frames[0].Texture == null)
				EditorGUILayout.HelpBox("This animation is empty! Oh no!", MessageType.Error);
			
		}

		serializedObject.ApplyModifiedProperties();
  }

}
}