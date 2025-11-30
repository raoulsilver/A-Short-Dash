using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace DoodleStudio95 {
using FramesPerSecond = DoodleAnimationFile.FramesPerSecond;
internal class DrawPrefs : ScriptableObject {

	internal const int VERSION = 09+25+2025;

	/// START USER-EDITABLE
	
	[Header("Settings")]
	[Tooltip("Where to save the animations, relative to the Assets folder")]
	public string m_SaveFolder;
	[Tooltip("When Symmetry is set to radial, this is the amount of times the brush repeats around the center of the image.")]
	internal int m_RadialSymmetryRepetitions = 4;
	[Tooltip("Adds the Grab tool to the tool bar in the Drawing Window. Use Grab to smear parts of the image.")]
	public bool m_ShowGrabTool;

	[Header("Presets")]
	public NewImageParams m_Preset_Character_Chunky;
	public NewImageParams m_Preset_Character_Smooth;
	public NewImageParams m_Preset_Background_Chunky;
	public NewImageParams m_Preset_Background_Smooth;
	public NewImageParams m_Preset_Square_Chunky;
	public NewImageParams m_Preset_Square_Smooth;
	public NewImageParams m_Preset_PlayingCard_Chunky;
	public NewImageParams m_Preset_PlayingCard_Smooth;
	public NewImageParams m_Preset_UI_Chunky;
	public NewImageParams m_Preset_UI_Smooth;

	[Header("Brush height")]
	[Tooltip("Brush sizes for each of the size levels. The values are from 0 to 1 where 1 is the total height of the animation file.")]
	public float m_BrushSize1;
	[Tooltip("Brush sizes for each of the size levels. The values are from 0 to 1 where 1 is the total height of the animation file.")]
	public float m_BrushSize2;
	[Tooltip("Brush sizes for each of the size levels. The values are from 0 to 1 where 1 is the total height of the animation file.")]
	public float m_BrushSize3;
	[Tooltip("Brush sizes for each of the size levels. The values are from 0 to 1 where 1 is the total height of the animation file.")]
	public float m_BrushSize4;
	[Tooltip("Brush sizes for each of the size levels. The values are from 0 to 1 where 1 is the total height of the animation file.")]
	public float m_BrushSize5;

	[Header("Colors")]
	public List<ColorPalette> m_Palettes = new List<ColorPalette>();

	[Header("Beta")]
	[Tooltip("Lets you draw directly on the scene view.")]
	public bool m_SceneViewDrawing;
	[Tooltip("Adds the Jumble tool to the tool bar in the Drawing Window. Use Jumble to randomly move pixels around your brush and create gradient effects.")]
	public bool m_ShowJumbleTool;
	[Tooltip("Choose an image to show behind your drawing to use as a reference.")]
	public Texture2D m_ReferenceImage;

	[Header("Experimental! Use at your own risk!")]
	[Tooltip("Lets you record a sound with your microphone and associate it with your animation automatically.")]
	public bool m_SoundRecorder;
	[Tooltip("Choose a texture to use as a brush.")]
	public Texture2D m_CustomBrush;
	[Tooltip("Type of compression used to store the animations. Use this if you need smaller filesizes.")]
	[HideInInspector] public Compression m_Compression = Compression.None;
	
	// Hidden
	[Tooltip("Uses the alpha of this texture repeatedly to stamp patterns on the image.")]
	[HideInInspector] public Texture2D m_CustomPattern;

	// Tweak these for performance
	internal static bool CHECK_DRAW_FREQUENCY = true;
	internal static double DRAW_FREQUENCY = 0.002f;
	internal static float LINE_TO_FREQUENCY = 1.0f;

	internal static bool GENERATE_FUNNY_NAMES = false;

	/// END USER-EDITABLE

	static string PATH = ROOT + "/Doodle Studio Preferences.asset";

	static DrawPrefs _loadedPrefs;
	public static DrawPrefs Instance {
		get {
			if (_loadedPrefs == null) {
				DrawPrefs prefs = AssetDatabase.LoadAssetAtPath<DrawPrefs>(PATH);
				if (prefs == null) {
					prefs = CreateInstance<DrawPrefs>();
					AssetDatabase.CreateAsset(prefs, PATH);
					AssetDatabase.SaveAssets();
					AssetDatabase.Refresh();
				}
				_loadedPrefs = prefs;
			}
			return _loadedPrefs;
		}
	}

	internal const string ROOT = "Assets/Doodle Studio 95"; // The path to the DrawDrawDraw folder
	
	// This is useful for differentiating the animations from your other assets, but feel free to remove it
	internal static bool ADD_ONION_ICON_TO_THUMBNAILS = true; 
	
	// when loading a Sprite or Texture sheet instead of an animation file, 
	// enable this to replace the original file instead of creating a new animation
	internal static bool REPLACE_LOADED_TEXTURES = true; 
	
	internal const byte FLOOD_FILL_THRESHOLD = 160; // how far into a pixel's alpha value should we paint? (ie alpha value under which it's considered a transparent pixel)
	internal static float PATTERN_REPETITIONS = 10;
	internal static bool SHOW_DEBUG_INPUTRECTS = false;

	[SerializeField, HideInInspector] internal bool inspector_preview_on = true;

	void Reset() {
		m_SaveFolder = "Textures/Drawn";
		m_Palettes = new List<ColorPalette>(){
			// MSPAIN
			ColorPalette.ColorPaletteStr("mspain", new List<string>(){
			"000000","ffffff","c0c0c0","ea3f34","fff732","80f200","77fafc","0049fb",
			"e660ff","fff987","7df37e","a2fbfc","798dfc","e94887","ef8751","808080",
			"751b14","827c14","3d7900","387d7e","001f7e","722b80","817d43","183e3f",
			"208bfc","0d457e","6a4efc","784315"
			}),
			// Poop
			ColorPalette.ColorPaletteStr("poop", new List<string>() {
				"b18f62","786144","875b2f","6f4317","5b3b1c","5b442e","65584c","906c5a","926945"
			})
		};

		m_Preset_Character_Chunky = new NewImageParams(150, 200, (int)FramesPerSecond.Normal, FilterMode.Point);
		m_Preset_Character_Smooth = new NewImageParams(384, 512, (int)FramesPerSecond.Normal, FilterMode.Trilinear);
		m_Preset_Background_Chunky = new NewImageParams(400, 200, (int)FramesPerSecond.Normal, FilterMode.Point);
		m_Preset_Background_Smooth = new NewImageParams(400 * 2, 200 * 2, (int)FramesPerSecond.Normal, FilterMode.Trilinear);
		m_Preset_Square_Chunky = new NewImageParams(200, 200, (int)FramesPerSecond.Normal, FilterMode.Point);
		m_Preset_Square_Smooth = new NewImageParams(200 * 2, 200 * 2, (int)FramesPerSecond.Normal, FilterMode.Trilinear);
		m_Preset_PlayingCard_Chunky = new NewImageParams((int)(63.5f * 3), (int)(88.9f * 3), (int)FramesPerSecond.Normal, FilterMode.Point, Vector4.zero, DrawWindow.SymmetryMode.PlayingCard);
		m_Preset_PlayingCard_Smooth = new NewImageParams((int)(63.5f * 8), (int)(88.9f * 8),(int)FramesPerSecond.Normal, FilterMode.Trilinear, Vector4.zero, DrawWindow.SymmetryMode.PlayingCard);
		m_Preset_UI_Chunky = new NewImageParams(256, 256, (int)FramesPerSecond.Normal, FilterMode.Point, new Vector4(85, 85, 85, 85), DrawWindow.SymmetryMode.Fourways);
		m_Preset_UI_Smooth = new NewImageParams(256 * 2, 256 * 2, (int)FramesPerSecond.Normal, FilterMode.Trilinear, new Vector4(85 *2, 85 * 2, 85 * 2, 85 * 2), DrawWindow.SymmetryMode.Fourways);

		m_BrushSize1 = 0;
		m_BrushSize2 = 0.007f;
		m_BrushSize3 = 0.016f;
		m_BrushSize4 = 0.045f;
		m_BrushSize5 = 0.12f;

		// Experimental
		m_SceneViewDrawing = true;
		m_SoundRecorder = false;
		m_ShowGrabTool = true;
		m_ShowJumbleTool = false;
	}

	public enum Quality {
		Chunky = 0, // FilterMode.Point
		Smooth = 2 // FilterMode.Trilinear
	}

	public enum Compression {
		None,
		DXT5,
	}

	[System.Serializable]
	public struct NewImageParams {
		public int width;
		public int height;
		public FilterMode filterMode;
		public Vector4 border;
		public int framesPerSecond;
		public DrawWindow.SymmetryMode symmetryMode;
		public DoodleAnimationFile.PatternMode patternMode;
		public NewImageParams(int width = 128, int height = 128, int fps = (int)FramesPerSecond.Normal, 
			FilterMode filterMode = FilterMode.Trilinear, Vector4? border = null, 
			DrawWindow.SymmetryMode symmetryMode = DrawWindow.SymmetryMode.None,
			DoodleAnimationFile.PatternMode patternMode = DoodleAnimationFile.PatternMode.Disabled) {
			this.width = width;
			this.height = height;
			this.filterMode = filterMode;
			this.framesPerSecond = fps;
			this.border = border.GetValueOrDefault();
			this.symmetryMode = symmetryMode;
			this.patternMode = patternMode;
		}
	}

	[System.Serializable]
	public class ColorPalette {
		public string name;
		public List<Color> colors;
		public ColorPalette(string name = "Palette", List<Color> colors = null) {
			this.name = name;
			this.colors = new List<Color>();
			if (colors != null) {
				foreach(var c in colors) {
					this.colors.Add(c);
				}
			}
		}
		public static ColorPalette ColorPaletteStr(string name = "Palette", List<string> colors = null) {
			var cp = new ColorPalette();
			if (colors != null) {
				foreach(var s in colors) {
					cp.colors.Add(DrawUtils.ColorFromString(s));
				}
			}
			return cp;
		}
		public void Add(Color color) { colors.Add(color); }
		public void Remove(Color color) { if (colors.Contains(color)) colors.Remove(color); }
	}

	public void Save() { 
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
	}

}

[CustomEditor(typeof(DrawPrefs))]
public class DrawPrefsEditor : Editor {
	override public void OnInspectorGUI() {
		var t = target as DrawPrefs;
		bool wasRecorder = t.m_SoundRecorder;
		bool wasDrawScene = t.m_SceneViewDrawing;
		var prevRefImage = t.m_ReferenceImage;
		
		EditorGUI.BeginChangeCheck();
		base.OnInspectorGUI();

		EditorGUILayout.Space();
		if (GUILayout.Button("Reset to Defaults", EditorStyles.miniButton) &&
			EditorUtility.DisplayDialog("Reset Doodle Studio 95 preferences?", 
				"Reset Doodle Studio 95! preferences to default?\nThis operation cannot be undone.", "Yes", "No")) {
			string path = AssetDatabase.GetAssetPath(target);
			AssetDatabase.DeleteAsset(path);
			var obj = CreateInstance<DrawPrefs>();
			AssetDatabase.CreateAsset(obj, path);
			AssetDatabase.Refresh();
			Selection.objects = new Object[]{ obj };
		}

		if (EditorGUI.EndChangeCheck()) {
			if (t.m_SoundRecorder != wasRecorder) SoundRecorder.Reset();
			if (t.m_CustomBrush && !EditorUtils.GetReadable(t.m_CustomBrush)) {
				Debug.LogErrorFormat("Can't set {0} as a custom brush because it's not readable. Set it to readable in the texture's import settings.", t.m_CustomBrush.name);
				t.m_CustomBrush = null;
			}
			if (t.m_CustomPattern && !EditorUtils.GetReadable(t.m_CustomPattern)) {
				Debug.LogErrorFormat("Can't set {0} as a custom pattern because it's not readable. Set it to readable in the texture's import settings.", t.m_CustomPattern.name);
				t.m_CustomPattern = null;
			}
			if (prevRefImage != t.m_ReferenceImage) {
				if (DrawWindow.m_Instance)
					DrawWindow.m_Instance.OnNewReferenceImageSet(prevRefImage, t.m_ReferenceImage);				
			}
			if (!wasDrawScene && t.m_SceneViewDrawing) {
				if (DrawWindow.m_Instance) {
					DrawWindow.m_Instance.ShowSceneViewGizmo = true;
				}
			}
			t.m_RadialSymmetryRepetitions = Mathf.Clamp(t.m_RadialSymmetryRepetitions, 2, 50);
		}
		EditorGUILayout.LabelField("Version: " + DrawPrefs.VERSION, EditorStyles.miniBoldLabel);
	}
 }

}