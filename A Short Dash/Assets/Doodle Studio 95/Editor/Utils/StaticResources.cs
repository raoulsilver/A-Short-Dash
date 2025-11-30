using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace DoodleStudio95 {
/// Loads resources from a folder and allows for easy access to cached UI assets
[InitializeOnLoad]
internal static class StaticResources {
	const string FONT_PATH = DrawPrefs.ROOT + "/Assets/Textures/Walter_Turncoat/WalterTurncoat.ttf";
	const string UI_DIRECTORY = DrawPrefs.ROOT + "/Assets/Textures";
	internal const string WINDOWS_DIRECTORY = DrawPrefs.ROOT + "/Assets/Windows";

	static Dictionary<string, Texture2D> loaded_textures = new Dictionary<string, Texture2D>();
	static Dictionary<string, EditorSprite> loaded_editorSprites = new Dictionary<string, EditorSprite>();
	static Dictionary<string, ParsedWindow> loaded_windows = new Dictionary<string, ParsedWindow>();
	static Dictionary<string, GUIStyle> loaded_styles = new Dictionary<string, GUIStyle>();
	static Font font;

	static Texture2D LoadTexture(string path) {
		var t = AssetDatabase.LoadAssetAtPath<Texture2D> (UI_DIRECTORY + "/" + path);
		if (t == null) 
			Debug.LogError("Cannot load " + path);
		else
			t.hideFlags = HideFlags.HideInInspector;
		return t;
	}
	static EditorSprite LoadEditorSprite(string path) {
		var s = new EditorSprite(UI_DIRECTORY + "/" + path);
		if (!s.IsValid) 
			Debug.LogError("Cannot load " + path);
		return s;
	}
	static ParsedWindow LoadWindow(string path) {
		var jsonWindow = JsonUtility.FromJson<ParsedWindow>(System.IO.File.ReadAllText(WINDOWS_DIRECTORY + "/" + path + ".json"));
		if (jsonWindow != null)
			return jsonWindow;
		Debug.LogWarning("Couldn't load json window" + path);
		return null;
		/*var window = AssetDatabase.LoadAssetAtPath<ParsedWindow>(WINDOWS_DIRECTORY + "/" + path + ".asset");
		if (window == null)
			Debug.LogWarning("Could not load window " + path);
		return window;*/
	}
	static GUIStyle LoadStyle(string id) {
		GUIStyle gs = null;
		if (id == "drawbutton") {
			gs = new GUIStyle(GUI.skin.button);
			gs.normal.background = gs.hover.background = gs.focused.background = gs.active.background = 
				GetEditorSprite("buttonframe_smaller.png").Frames[0];
			gs.normal.textColor = gs.hover.textColor = gs.focused.textColor = gs.active.textColor = 
				Color.black;
			gs.padding = new RectOffset(0,6,0,0);
			gs.font = GetFont();
			gs.fontSize = 10;
		}
		return gs;
	}

	#region internal
	internal static DoodleStudio95.EditorSprite GetEditorSprite(string filename) {
		if (!loaded_editorSprites.TryGetValue(filename, out var sprite))
		{
			if (loaded_editorSprites.ContainsKey(filename)) // texture went away after non serialized vars lost
				Unload();
			sprite = LoadEditorSprite(filename);
			loaded_editorSprites[filename] = sprite;
		}
		return sprite;
	}
	internal static Texture2D GetTexture2D(string filename) {
		if (!loaded_textures.TryGetValue(filename, out var texture))
		{
			if (loaded_textures.ContainsKey(filename)) // texture went away after non serialized vars lost
				Unload();
			texture = LoadTexture(filename);
			loaded_textures[filename] = texture;
		}
		return texture;
	}
	internal static ParsedWindow GetWindow(string filename) {
		if (!loaded_windows.TryGetValue(filename, out var window))
        {
			if (loaded_windows.ContainsKey(filename))
				Unload();
			loaded_windows[filename] = LoadWindow(filename);
		}
		return loaded_windows[filename];
	}
	internal static Font GetFont() {
		if (font == null) {
			font = AssetDatabase.LoadAssetAtPath<Font>(FONT_PATH);
			if (font == null) 
				Debug.LogError("Cannot load font");
		}
		return font;
	}
	internal static GUIStyle GetStyle(string id) {
		GUIStyle style = null;
		loaded_styles.TryGetValue(id, out style);
		if (style != null && style.normal.background == null) // Special case when textures inside were lost
			style = null;
		if (style == null) {
			if (loaded_styles.ContainsKey(id))
				Unload();
			style = LoadStyle(id);
			loaded_styles[id] = style;
		}
		return style;
	}
	internal static void Unload(bool ForceCollect = false) {
		loaded_textures.Clear();
		loaded_editorSprites.Clear();
		loaded_windows.Clear();
		font = null;
		if (ForceCollect)
			System.GC.Collect();
	}
	
	static StaticResources() {
		Unload();
	}
	
	#endregion

	#region Styles

	static GUIStyle _style_nameTextfield;
	internal static GUIStyle style_nameTextfield { get {
		if (_style_nameTextfield == null) {
			var gs = new GUIStyle();
			gs.richText = true;
			gs.font = StaticResources.GetFont();
			gs.alignment = TextAnchor.MiddleLeft;
			gs.fontStyle = FontStyle.Normal;
			gs.fontSize = 17;
			gs.normal.textColor = new Color(.4f,.4f,.4f);
			gs.normal.background = null;
			_style_nameTextfield = gs;
		}
		return _style_nameTextfield;
	}}

	static GUIStyle _style_versionTextfield;
	internal static GUIStyle style_versionTextfield { get {
		if (_style_versionTextfield == null) {
			var gs = new GUIStyle();
			gs.font = StaticResources.GetFont();
			gs.alignment = TextAnchor.MiddleLeft;
			gs.fontStyle = FontStyle.Normal;
			gs.fontSize = 8;
			gs.normal.textColor = DrawUtils.ColorFromString("906B48FF");
			gs.normal.background = null;
			_style_versionTextfield = gs;
		}
		return _style_versionTextfield;
	}}

	static GUIStyle _style_resolutionTextfield;
	internal static GUIStyle style_resolutionTextfield { get {
		if (_style_resolutionTextfield == null) {
			var gs = new GUIStyle();
			gs.richText = true;
			gs.font = StaticResources.GetFont();
			gs.alignment = TextAnchor.MiddleCenter;
			gs.fontStyle = FontStyle.Normal;
			gs.fontSize = 40;
			gs.normal.textColor = Color.black;
			gs.normal.background = null;
			_style_resolutionTextfield = gs;
		}
		return _style_resolutionTextfield;
	}}

	#endregion

}

}