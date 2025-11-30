// #define USE_OPTIMIZED_LINE_DRAWING
// #define DEBUG_VIEW

using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using System.Linq;


namespace DoodleStudio95 {

using SaveInfo = DoodleAnimationFileUtils.SaveInfo;
using PlaybackMode = DoodleAnimationFile.PlaybackMode;
using FramesPerSecond = DoodleAnimationFile.FramesPerSecond;
using Line = DrawUtils.Line;
using PatternMode = DoodleAnimationFile.PatternMode;

[System.Serializable]
public class DrawWindow : EditorWindow {

  static string[] FillToolNames = new string[]{ "Instant", "Random", "Sideways", "Growing"};
  static string[] FillToolIcons = new string[]{ "tool_bucket_normal.png", "tool_bucket_random.png", "tool_bucket_slow.png", "tool_bucket_growing.png"};

  static KeyCode[] colorkeys = new KeyCode[] {
    KeyCode.Alpha1,
    KeyCode.Alpha2,
    KeyCode.Alpha3,
    KeyCode.Alpha4,
    KeyCode.Alpha5,
  };

  static List<FloodFillOperation.Type> FloodTypes = new List<FloodFillOperation.Type>(){
    FloodFillOperation.Type.Normal,
    FloodFillOperation.Type.Random,
    FloodFillOperation.Type.RightToLeftSlow,
    FloodFillOperation.Type.Growing
  };

  static List<GrabMode> GrabModes = new List<GrabMode>() { GrabMode.Normal, GrabMode.Smear};//, GrabMode.Blurry };
  static string[] GrabModeNames = new string[]{ "None", "Smear"};//, "Blurry" };
  static string[] GrabModeIcons = new string[]{ "grab.png", "grab_smooth.png"};//, "grab_smoothbilinear.png" };
  
  static List<PatternMode> PatternModes = new List<PatternMode>() { PatternMode.Disabled, PatternMode.HorizontalAndVertical, PatternMode.Horizontal, PatternMode.Vertical };
  static string[] PatternModeNames = new string[]{ "Off", "Horizontal and Vertical", "Horizontal", "Vertical" };
  static string[] PatternModeIcons = new string[]{ "patternmode.png", "patternmode.png", "patternmode_horizontal.png", "patternmode_vertical.png" };

  static List<SymmetryMode> SymmetryModes = new List<SymmetryMode>() { SymmetryMode.None, SymmetryMode.Horizontal, SymmetryMode.Vertical, SymmetryMode.Fourways, SymmetryMode.PlayingCard, SymmetryMode.Radial };
  static string[] SymmetryModeNames = new string[]{ "Off", "Horizontal", "Vertical", "Four Ways", "Playing Card", "Radial" };
  static string[] SymmetryModeIcons = new string[]{ "symmetryx.png", "symmetryx.png", "symmetryy.png", "symmetryxy.png", "symmetrycard.png", "symmetryradial.png" };
  
  static List<OnionSkinMode> OnionModes = new List<OnionSkinMode>() { OnionSkinMode.None, OnionSkinMode.ThreeFramesForwardBack };
  // static List<OnionSkinMode> OnionModes = new List<OnionSkinMode>() { OnionSkinMode.None, OnionSkinMode.ThreeFramesForwardBack, OnionSkinMode.PreviousAndNext, OnionSkinMode.Previous };
  static string[] OnionModeNames = new string[]{ "Off", "On" };
  // static string[] OnionModeNames = new string[]{ "Off", "Three Frames", "Previous and Next", "Previous" };

  public enum AlphaMode { None, PaintOnlyBehind, PaintOnlyInside, IgnoreStrokes }
  static List<AlphaMode> AlphaModes = new List<AlphaMode>() { AlphaMode.None, AlphaMode.PaintOnlyBehind, AlphaMode.PaintOnlyInside, AlphaMode.IgnoreStrokes };
  static string[] AlphaModeNames = new string[]{ "Off", "Paint Only Behind", "Paint Only Inside", "Ignore Strokes" };
  static string[] AlphaModeIcons = new string[]{ "color_behindonly.png", "color_behindonly.png", "color_behindonly.png", "color_behindonly.png" };

  #if DEBUG_VIEW
  static List<string> debugText = new List<string>();
  #endif

  static List<PlaybackMode> playbackModes = new List<PlaybackMode>() { PlaybackMode.Loop, PlaybackMode.LoopBackAndForth };
  static string[] playbackModeIcons = new string[] { "loop.png", "loopbf.png" };

  static List<int> speeds = new List<int>(){(int)FramesPerSecond.Slow, (int)FramesPerSecond.Normal, (int)FramesPerSecond.Fast};

  enum ReferenceImageMode { Off, Transparent, Opaque }
  static List<ReferenceImageMode> ReferenceImageModes = new List<ReferenceImageMode>() { 
    ReferenceImageMode.Off, ReferenceImageMode.Transparent, ReferenceImageMode.Opaque };
  static string[] ReferenceImageNames = new string[] { "off", "transparent", "on" };

  static string[] BrushSizeTooltips = new string[] {
    "Tiny\nFor details",
    "Small\nFor most strokes",
    "Large\nFor thick lines",
    "Very Large\nEven bigger",
    "Excessive\nIt's not that big",
  };
  internal static Color COLOR_FILEACTION = new Color(0.73f, .98f, .98f);
  internal static Color COLOR_ACTIVE = new Color(.93f, .93f, .25f);
  internal static Color COLOR_DISABLED = new Color(1, 1, 1, 0.5f);
  internal static Color COLOR_PLAY = new Color(.2f, .95f, .2f);
  internal static Color COLOR_SELECTEDTOOL = Color.red;
  internal static Color COLOR_SELECTEDTOOLBG = new Color(215f/255f,215f/255f,173f/255f);
  internal static Color COLOR_DARK_CHECKERBOARD = new Color(.22f,.22f,.22f);
  internal static Color COLOR_FRAME_THUMBNAIL_SINGLE = new Color(.85f, .85f, .85f, .85f);
  internal static Color COLOR_FRAME_THUMBNAIL_ALL = new Color(.85f, .65f, .65f, .85f);
  internal static Color COLOR_TIMELINE_BACKGROUND = new Color(214/255f,203f/255f,119f/255f,1);

  internal static DrawWindow m_Instance;

  [MenuItem ("Window/Doodle Studio 95!")]
  [MenuItem ("Tools/Doodle Studio 95/Open Drawing Window", false, 0)]
  static void Open() {
    EditorWindow.GetWindow<DrawWindow> ("Doodle!", true);
  }

  public enum Tool {
    Color,
    Fill,
    Eraser,
    Replaser,
    Grab,
    Jumble
  }

  public enum SymmetryMode {
    None,
    Horizontal,
    Vertical,
    Fourways,
    PlayingCard,
    Radial
  }

  public enum OnionSkinMode {
    None,
    ThreeFramesForwardBack,
    Previous,
    PreviousAndNext
  }

  float BrushSize { 
    get {
      if (m_CurrentTool == Tool.Eraser) 
        return m_EraserSize; 
      else if (m_CurrentTool == Tool.Grab)
        return m_BrushSize * 2;
      return m_BrushSize; 
    } set {
      if (m_CurrentTool == Tool.Eraser) 
        m_EraserSize = value; 
      m_BrushSize = value; 
    }
  }
  
  // Image properties
  [SerializeField] int m_TextureWidth;
  [SerializeField] int m_TextureHeight;
  [SerializeField] FilterMode m_FilterMode = FilterMode.Point;

  // Timeline
  [SerializeField] List<DrawWindowKeyframe> m_KeyFrames;
  [SerializeField] bool m_Playing = false;
  [SerializeField] bool m_RecordingModeOn = false;//
  [SerializeField] int m_PlayheadPosition = 0;

  // Serialized tool settings
  [SerializeField] Tool m_CurrentTool = Tool.Color;
  [SerializeField] OnionSkinMode m_OnionSkinMode = OnionSkinMode.None;
  bool OnionSkinOn { get { return m_OnionSkinMode != OnionSkinMode.None; } }
  [SerializeField] bool m_SizeByVelocity = false;
  [SerializeField] int m_FramesPerSecond = (int)FramesPerSecond.Normal;
  [SerializeField] int m_CurrentPalette = 0;
  [SerializeField] List<FloodFillOperation> m_FloodFillOps = new List<FloodFillOperation> ();
  [SerializeField] FloodFillOperation.Type m_FloodFillType = FloodFillOperation.Type.Normal;
  [SerializeField] internal bool m_FloodFillRainbow = false;
  [SerializeField] AlphaMode m_AlphaMode = AlphaMode.None;
  [SerializeField] PlaybackMode m_PlaybackMode = PlaybackMode.Loop;
  [SerializeField] SymmetryMode m_SymmetryMode = SymmetryMode.None;
  [SerializeField] float m_Zoom = 1;
  [SerializeField] float m_BrushSize;
  [SerializeField] float m_EraserSize;
  [SerializeField] Color m_BrushColor;
  [SerializeField] float m_Replaser_BorderSize = 6;
  [SerializeField] Vector4 m_SpriteBorder = Vector4.zero; // the sprite border is used for 9-slicing // X=left, Y=bottom, Z=right, W=top.
  [SerializeField] bool m_DarkCheckerboard = false;
  [SerializeField] bool m_JumbleALot = false;
  [SerializeField] ReferenceImageMode m_RefImageMode = ReferenceImageMode.Off;
  
  enum GrabMode { Normal, Smear, Blurry, Glitch }
  [SerializeField] GrabMode m_Grab_Mode = GrabMode.Normal;

  [SerializeField] bool m_ShowSceneViewGizmo = true;

  [SerializeField] PatternMode m_PatternMode;
  bool PatternModeOn { get { return m_PatternMode != PatternMode.Disabled; } }

  [SerializeField] bool UseSceneViewDrawing { get { return DrawPrefs.Instance.m_SceneViewDrawing; } }
  [SerializeField] bool UseSoundRecorder { get { return DrawPrefs.Instance.m_SoundRecorder; } }

  // Dynamic fields
  [SerializeField] SerializableTexture2D m_EmptyTexture;
  [SerializeField] Vector2 m_ScrollPos;
   // Was Recording Mode on and user started drawing? we need to make new frames as playhead advances
  [SerializeField] bool m_MakeNewFrames = false;
  [SerializeField] bool m_UnsavedChangesPresent = false;
  [SerializeField] string m_CurrentPopup = ""; // "New" or "Custom" window
  [SerializeField] string m_NewName = ""; // name of new animation
  [SerializeField] int m_ReferenceImageObjectPickerID;
  
  // Library assets
  [SerializeField] string m_CurrentOpenAssetGUID;

  
  // Private
  float m_PlayTime = 0;
  int m_PlaybackDirection = 1;
  int m_PlayheadPositionBeforePlayback = 0;
  double m_LastPlayEditorTime;
  float m_DeltaTime;
  Vector2? m_LastDrawnPoint;
  GameObject m_SceneViewPreviewObject;
  Plane m_SceneViewPreviewPlane;
  Renderer m_PreviewSpriteRenderer;
  Material m_PreviewSpriteMaterial;
  Vector3 m_SceneViewPreviewObject_LastPosition = Vector3.zero;
  Quaternion m_SceneViewPreviewObject_LastRotation = Quaternion.identity;
  Vector3 m_SceneViewPreviewObject_LastScale = Vector3.zero;
  bool m_NeedsUIRedraw = false;
  int m_DraggingBorder = -1;
  Texture2D m_GrabImage;

  InputRect m_InputRectWindowDrawArea;
  InputRect m_InputRectWindowSpriteBorder;
  InputRect m_InputRectWindowTexture;
  InputRect m_InputRectSceneView;
  InputRect m_InputRectTopbar;

  double _drawCounter = 0;
  // true when user pressed mouse inside drawing area and hasn't released or left the area yet
  bool m_Drawing = false;
  // user clicked "DRAW" on scene view and we're now capturing the camera to let them draw on the quad
  bool m_SceneViewDrawModeOn = false;
  
  bool m_DraggingTimeline = false;
  Vector2 m_LastMousePosition;
  float m_VelocityDelta;
  float m_BgScroll = 0;
  GUIStyle m_TooltipStyle;
  string m_TooltipText;
  Rect m_TooltipPos;
  bool m_GuiEnabled;
  MaterialPropertyBlock _propBlock;

  // choosing a new tool assigns it here and applies it to m_CurrentTool after EventType.Draw to avoid changing the layout b/w events
  Tool _queued_tool; 

  string OpenAssetPath { get { return AssetDatabase.GUIDToAssetPath(m_CurrentOpenAssetGUID); } }
  bool DrawOnAllKeyframes { get { 
    return m_KeyFrames != null && m_KeyFrames.Count > 1 && 
      !m_RecordingModeOn &&
      Event.current != null && Event.current.alt; } }

  // Static
  static bool m_IsMouseDown = false;   // Mouse was down inside of the Window and hasn't been released yet
  static List<Vector2> _v2list = new List<Vector2> (); // Frequently updated, stored here to avoid allocation
  static List<Rect> _patternRects = new List<Rect>();
  static Rect[] _layoutRects;
  static List<Rect> _singleRect = new List<Rect>();
  static List<DrawWindowKeyframe> _singleKeyframe = new List<DrawWindowKeyframe>();

  internal bool ShowSceneViewGizmo { set { m_ShowSceneViewGizmo = value; } }
  internal void OnNewReferenceImageSet(bool hadImagePreviously, bool hasImageNow) { 
    if (!hadImagePreviously && hasImageNow && m_RefImageMode == ReferenceImageMode.Off) 
      m_RefImageMode = ReferenceImageMode.Transparent; 
    else if (hadImagePreviously && !hasImageNow)
      m_RefImageMode = ReferenceImageMode.Off;
  }

  #region Playback

  DrawWindowKeyframe GetKeyFrameAt(int Position, bool Repeat = false) { 
    if (Repeat)
      Position = (int)Mathf.Repeat (Position, GetAnimationLength ());
    int keyframeI = 0;
    if (m_KeyFrames.Count > 0) {
      int lastpos = 0;
      for (int i = 0; i < m_KeyFrames.Count; i++) {
        if (m_KeyFrames[i] == null) {
          Debug.LogWarning("null keyframe " + i);
          continue;
        }
        if (lastpos + m_KeyFrames [i].m_Length > Position) {
          keyframeI = i;
          break;
        }
        lastpos += m_KeyFrames [i].m_Length;
      }
    } else {
      AddKeyFrame ();
    }
    return m_KeyFrames [keyframeI]; 
  }

  DrawWindowKeyframe GetCurrentKeyFrame() {
    return GetKeyFrameAt (m_PlayheadPosition);
  }

  Texture2D GetCurrentTexture() { 
    return GetCurrentKeyFrame ().Texture.texture; 
  }

  void SetPlayhead(int Position, bool CheckLength = false, bool Loop = false) {
    if (CheckLength) {
      int len = GetAnimationLength ();
      if (Loop)
        Position = (int)Mathf.Repeat (Position, len);
      else
        Position = Mathf.Clamp (Position, 0, len - 1);
    }
    if(m_Playing && m_PlaybackMode == PlaybackMode.Loop)
      m_BgScroll += Mathf.Clamp01(Mathf.Abs(Position - m_PlayheadPosition));
    else
      m_BgScroll += Mathf.Clamp(Position - m_PlayheadPosition, -1, 1);
    
    m_PlayheadPosition = Position;
    //m_BgScroll = Position;

    UpdatePreview();
    
    m_NeedsUIRedraw = true;
  }

  void UpdatePreview() {
    if (m_PreviewSpriteMaterial == null || m_PreviewSpriteRenderer == null)
      return;
    if (_propBlock == null) _propBlock = new MaterialPropertyBlock();
    if (m_PreviewSpriteMaterial.HasProperty("_MainTex")) {
      _propBlock.SetTexture ("_MainTex", GetCurrentTexture());
    } else if (m_PreviewSpriteMaterial.HasProperty("_Layer0")) { // Layered preview
      _propBlock.SetTexture ("_Layer0", GetCurrentKeyFrame ().Texture.texture);
      // for (int i = 0; i < 10; i++)
      //   _propBlock.SetTexture ("_Layer" + i, i < DoodleAnimationFile.LAYERS ? GetCurrentKeyFrame ().Texture [i].texture : EmptyFrame.texture);
    }
    m_PreviewSpriteRenderer.SetPropertyBlock(_propBlock);
  }

  void SetPlayhead(DrawWindowKeyframe Frame) {
    int framei = m_KeyFrames.IndexOf (Frame);
    int pos = 0;
    if (framei > 0) {
      for (int i = 0; i < framei; i++) {
        pos += m_KeyFrames [i].m_Length;
      }
    }
    SetPlayhead (pos);
  }

  internal void SetPlayback(bool bPlaying) {
    bool wasPlaying = m_Playing;
    m_Playing = bPlaying;
    if (m_Playing) {
      m_LastPlayEditorTime = EditorApplication.timeSinceStartup;
      m_PlaybackDirection = 1;
      m_PlayTime = m_PlayheadPosition;
    }
    if (!m_Playing && m_RecordingModeOn)
      m_RecordingModeOn = m_MakeNewFrames = false;
    
    if (UseSoundRecorder) {
      if (SoundRecorder.IsRecording()) {
        if (wasPlaying && !m_Playing)
          SoundRecorder.StopRecording();
      } else {
        if (!wasPlaying && m_Playing)
          SoundRecorder.PlayPreview(m_PlayheadPosition / (float)GetAnimationLength());
        else if (wasPlaying && !m_Playing)
          SoundRecorder.StopPreview();
      }
    }

    if (!wasPlaying)
      m_PlayheadPositionBeforePlayback = m_PlayheadPosition;
    else
      SetPlayhead (m_PlayheadPositionBeforePlayback); // go back to where we were before playing
  }

  #endregion

  int GetAnimationLength() { 
    var l = 0; 
    foreach (var f in m_KeyFrames)
      l += f.m_Length;
    return l;
  }

  SerializableTexture2D EmptyFrame { 
    get {
      bool create = m_EmptyTexture == null || !m_EmptyTexture.IsValid || m_EmptyTexture.width != m_TextureWidth || m_EmptyTexture.height != m_TextureHeight;

      if (create) {
        if (m_EmptyTexture == null) {
          m_EmptyTexture = SerializableTexture2D.New(m_TextureWidth, m_TextureHeight, TextureFormat.ARGB32, m_FilterMode);
        } else {
          m_EmptyTexture.Create(m_TextureWidth, m_TextureHeight, TextureFormat.ARGB32, m_FilterMode);
        }
      }
      return m_EmptyTexture;
    }
  }

  #region Unity

  void Awake() {
    //Debug.Log("Awake");
    // Editor is open
    hideFlags = HideFlags.HideAndDontSave;
    titleContent = new GUIContent("Doodle!", StaticResources.GetEditorSprite("windowicons.png").Frames[Random.Range(0, StaticResources.GetEditorSprite("windowicons.png").Frames.Length - 1)]);

    
    // DrawPrefs.Load();

    // m_KeyFrames may not be null if we're being unserialized by unity after
    // another editor window gets maximized.
    if (m_KeyFrames == null) 
    {
      m_CurrentOpenAssetGUID = null;
      m_NeedsUIRedraw = false;
      m_Zoom = 1;
      m_BrushSize = DrawPrefs.Instance.m_BrushSize3;
      m_EraserSize = DrawPrefs.Instance.m_BrushSize5;
    
      m_Drawing = false;
  
      if(m_FilterMode == FilterMode.Point)
        New (DrawPrefs.Instance.m_Preset_Square_Chunky, true);
      else
        New (DrawPrefs.Instance.m_Preset_Square_Smooth, true);
    }
  }

  void OnEnable() {
    m_Instance = this;

    this.wantsMouseEnterLeaveWindow = false; // mosue leave window is unreliable 5.6, called in middle of window
    this.wantsMouseMove = true;

    StaticResources.Unload();

    var palette = DrawPrefs.Instance.m_Palettes[m_CurrentPalette];
    m_BrushColor = palette.colors[0];

    Undo.undoRedoPerformed -= OnUndoRedoPerformed;
    Undo.undoRedoPerformed += OnUndoRedoPerformed;
    
    #if UNITY_2019_1_OR_NEWER
    SceneView.duringSceneGui -= OnSceneGUI; // Remove if previously been assigned.
    SceneView.duringSceneGui += OnSceneGUI;
    #else
    SceneView.onSceneGUIDelegate -= OnSceneGUI;
    SceneView.onSceneGUIDelegate += OnSceneGUI;
    #endif

    EditorApplication.update -= OnApplicationUpdate;
    EditorApplication.update += OnApplicationUpdate;
    
    EditorApplication.playModeStateChanged -= OnPlayModeChanged;
    EditorApplication.playModeStateChanged += OnPlayModeChanged;

    _queued_tool = m_CurrentTool;
    _propBlock = new MaterialPropertyBlock();

    m_InputRectWindowDrawArea = new InputRect();
    m_InputRectWindowSpriteBorder = new InputRect();
    m_InputRectWindowTexture = new InputRect();
    m_InputRectSceneView = new InputRect();
    m_InputRectTopbar = new InputRect();

    if (m_RefImageMode != ReferenceImageMode.Off && DrawPrefs.Instance.m_ReferenceImage == null)
      m_RefImageMode = ReferenceImageMode.Off;

    if (UseSoundRecorder)
      SoundRecorder.Reset();
  }

  void OnDisable() {
    m_Instance = null;
    SetSceneViewDrawing(false);
    //Debug.Log ("OnDisable");
    Undo.undoRedoPerformed -= OnUndoRedoPerformed;
    #if UNITY_2019_1_OR_NEWER
    SceneView.duringSceneGui -= OnSceneGUI;
    #else
    SceneView.onSceneGUIDelegate -= OnSceneGUI;
    #endif
    EditorApplication.update -= OnApplicationUpdate;
    EditorApplication.playModeStateChanged -= OnPlayModeChanged;
    StaticResources.Unload();
  }

  void OnDestroy() {
    //Debug.Log ("On destroy");
    DestroyPreview ();
    /*
  DestroyPreview();
  foreach(var k in m_KeyFrames) {
    DestroyImmediate(k.m_Texture);
  }
  */
    m_CurrentOpenAssetGUID = null;
    foreach(var k in m_KeyFrames) {
      k.OnDestroy();
    }
    EmptyFrame.OnDestroy();
  }

  void OnUndoRedoPerformed() {
    m_NeedsUIRedraw = true;
    foreach (var k in m_KeyFrames) {
      Debug.Assert(k.Texture.texture != null);
      if (k.Texture.texture == null) continue;
      // Update local buffers to actual texture
      k.Texture.pixels = k.Texture.texture.GetPixels32 ();
      k.Texture.Apply (true);
    }
    m_FloodFillOps.Clear();
  }

  void OnLostFocus() {
    if (!m_SceneViewDrawModeOn)
      SetPlayback(false);
  }

  void OnPlayModeChanged(PlayModeStateChange change) {
    if (change != PlayModeStateChange.ExitingEditMode)
      return;
    DestroyPreview();
  }

  #endregion

  #region  MouseState
  internal enum MouseState { Idle, Hovered, Pressed, MouseUp }
  internal static MouseState GetStateAtCoords(Rect rect) {
    if (!GUI.enabled)
      return MouseState.Idle;
    var e = Event.current;
    bool hover = rect.Contains(e.mousePosition);
    bool pressed = hover && m_IsMouseDown;
    bool mouseup = hover && e.type == EventType.MouseUp && e.button == 0;
    if (mouseup) {
      return MouseState.MouseUp;
    } else if (hover && !pressed) {
      return MouseState.Hovered;
    } else if (pressed) {
      return MouseState.Pressed;
    } else 
    return MouseState.Idle;
  }
  #endregion

  #region Drawing

  /// <summary>
  /// Creates a texture and a preview object
  /// </summary>
  void New(bool ignoreUnsavedChanges = false) {
    if (!ignoreUnsavedChanges && m_UnsavedChangesPresent && !EditorUtility.DisplayDialog ("New?", "You have unsaved changes. Are you sure you want to make a new animation?", "OK", "Cancel"))
      return;
    /* 
    foreach (var k in m_KeyFrames) {
      foreach (var l in k.m_Layers)
        DestroyImmediate (l.texture);
    }
    m_KeyFrames.Clear ();
    */
    m_CurrentOpenAssetGUID = null;
    m_Playing = m_RecordingModeOn = m_MakeNewFrames = false;
    m_CurrentPopup = "";
    m_Zoom = 1;

    if (m_EmptyTexture != null) m_EmptyTexture.OnDestroy();
    m_EmptyTexture = null;

    m_FloodFillOps.Clear();
    if (m_KeyFrames != null) {
      foreach(var k in m_KeyFrames) {
        if (k != null)
          k.OnDestroy();
      }
    }
    m_KeyFrames = new List<DrawWindowKeyframe> ();
    AddKeyFrame ();
    SetPlayhead (0);

    SetSceneViewDrawing(false);
    DestroyPreview();
    m_SceneViewPreviewObject_LastScale = Vector3.zero; // Set it to zero so it gets reset to the new image's size
    CreatePreview();

    if (UseSoundRecorder)
      SoundRecorder.Reset();
    
    m_NeedsUIRedraw = false;
    m_UnsavedChangesPresent = false;
  }
  void New(DrawPrefs.NewImageParams Params, bool ignoreUnsavedChanges = false) {
    if (!ignoreUnsavedChanges && m_UnsavedChangesPresent && !EditorUtility.DisplayDialog ("New?", "You have unsaved changes. Are you sure you want to make a new animation?", "OK", "Cancel"))
      return;
    m_TextureWidth = Params.width;
    m_TextureHeight = Params.height;
    m_FilterMode = (FilterMode)Params.filterMode;
    m_SpriteBorder = Params.border;
    m_FramesPerSecond = Params.framesPerSecond;
    m_SymmetryMode = Params.symmetryMode;
    m_PatternMode = Params.patternMode;
    New(true);
  }

  void CreatePreview() {
    if (!UseSceneViewDrawing || !m_ShowSceneViewGizmo || Application.isPlaying)
      return;
    var keyframe = GetCurrentKeyFrame ();
    if (keyframe == null || !keyframe.Texture.IsValid)
      return;
    var previewObj = GameObject.Find ("(Draw Preview)");
    if (previewObj == null) {
      previewObj = GameObject.CreatePrimitive (PrimitiveType.Quad);
      previewObj.name = "(Draw Preview)";
      previewObj.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;// | HideFlags.HideInHierarchy;
      DrawUtils.SafeDestroy(previewObj.GetComponent<Collider>());
    }

    // Set scale to image if it's been reset
    if (Mathf.Approximately(m_SceneViewPreviewObject_LastScale.magnitude, 0))
      m_SceneViewPreviewObject_LastScale = previewObj.transform.localScale  = new Vector3 (
          (float)m_TextureWidth / 100f, (float)m_TextureHeight / 100f, 1);
        
    previewObj.transform.position = m_SceneViewPreviewObject_LastPosition;  
    previewObj.transform.rotation = m_SceneViewPreviewObject_LastRotation;  
    previewObj.transform.localScale = m_SceneViewPreviewObject_LastScale;
    m_SceneViewPreviewObject = previewObj; 

    if (m_PreviewSpriteMaterial != null) {
      DestroyImmediate(m_PreviewSpriteMaterial);
      m_PreviewSpriteMaterial = null;
    }
    if (m_PreviewSpriteMaterial == null) {
      var shader = Shader.Find ("Doodle Studio 95/Shadow Casting Sprite");
      if (shader == null) shader = Shader.Find ("Hidden/DrawPreview");
      Debug.Assert(shader != null);
      m_PreviewSpriteMaterial = new Material (shader);
    }
    m_PreviewSpriteRenderer = previewObj.GetComponent<Renderer>(); 
    m_PreviewSpriteRenderer.sharedMaterial = m_PreviewSpriteMaterial;

    m_SceneViewPreviewPlane = new Plane(previewObj.transform.forward, previewObj.transform.position);
    
    UpdatePreview();
    
   //Selection.objects = new Object[]{q};
   //if (SceneView.lastActiveSceneView != null) {
   //SceneView.lastActiveSceneView.AlignViewToObject(q.transform);
   //SceneView.lastActiveSceneView.FrameSelected();
   //}
  }

  /// <summary>
  /// Deletes the preview
  /// </summary>
  void DestroyPreview() {
    if (m_SceneViewPreviewObject != null) {
      m_SceneViewPreviewObject_LastPosition = m_SceneViewPreviewObject.transform.position;
      m_SceneViewPreviewObject_LastRotation = m_SceneViewPreviewObject.transform.rotation;
      m_SceneViewPreviewObject_LastScale = m_SceneViewPreviewObject.transform.localScale;
      DestroyImmediate (m_SceneViewPreviewObject);
      m_SceneViewPreviewObject = null;
    }
    if (m_PreviewSpriteMaterial != null) {
      DestroyImmediate(m_PreviewSpriteMaterial);
      m_PreviewSpriteMaterial = null;
    }
  }

  internal void Load(string assetPath) {
    if (m_KeyFrames != null && m_KeyFrames.Count > 0 && m_UnsavedChangesPresent) {
      if (!EditorUtility.DisplayDialog ("Load?", "You have unsaved changes. Are you sure you want to load a new animation?", "OK", "Cancel"))
        return;
    }
    EditorUtility.DisplayProgressBar ("Loading...", "Loading...", 0);

    // Reset everything
    New ();
    
    var animationFile = AssetDatabase.LoadAssetAtPath<DoodleAnimationFile>(assetPath);

    // Attempt to load from sprites (Legacy)
    if (animationFile == null) {
      var tempFile = DoodleAnimationFileUtils.FromTexture(assetPath);
      if (tempFile != null) {
        animationFile = tempFile;
        Debug.Assert(animationFile != null);
      } else {
        Debug.LogWarning("Error loading texture " + assetPath);
      }
    }

    if (animationFile != null) {
      m_KeyFrames = new List<DrawWindowKeyframe> ();
      foreach(var k in animationFile.frames) {
        if (k != null) {
          m_KeyFrames.Add(new DrawWindowKeyframe(k));
        } else {
          Debug.LogWarning("Null keyframe!", animationFile);
        }
      }
      if (animationFile.frames == null || animationFile.frames.Count == 0) {
        Debug.LogWarning("This animation doesn't have keyframes!", animationFile);
        New(true);
        EditorUtility.ClearProgressBar ();
        return;
      } else {
        m_PlaybackMode = animationFile.playbackMode;
        m_FramesPerSecond = (int)animationFile.framesPerSecond;
        m_TextureWidth = animationFile.width;
        m_TextureHeight = animationFile.height;
        m_SpriteBorder = animationFile.spriteBorder;
        m_FilterMode = animationFile.filterMode;
        m_DarkCheckerboard = animationFile.darkCheckerboard;
        m_PatternMode = animationFile.patternMode;
      }
      
    }
    EditorUtility.ClearProgressBar ();

    SetPlayhead (Mathf.Max (0, GetAnimationLength () - 1));
    m_CurrentOpenAssetGUID = AssetDatabase.AssetPathToGUID(assetPath);

    m_UnsavedChangesPresent = false;
  }

  // https://forum.unity.com/threads/tutorial-how-to-to-show-specific-folder-content-in-the-project-window-via-editor-scripting.508247/
  string GetProjectWindowCurrentFolder()
  {

    // https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/ProjectBrowser.cs
    // TODO: try UnityEditor.ProjectBrowser.GetAllProjectBrowsers()


    // Find the internal ProjectBrowser class in the editor assembly.
    System.Reflection.Assembly editorAssembly = typeof(Editor).Assembly;
    System.Type projectBrowserType = editorAssembly.GetType("UnityEditor.ProjectBrowser");

    // object lastBrowser = projectBrowserType.GetField(
    //   "s_LastInteractedProjectBrowser", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static
    // ).GetValue(editorAssembly);
 
    // This is the internal method, which performs the desired action.
    // Should only be called if the project window is in two column mode.
    System.Reflection.MethodInfo showFolderContents = projectBrowserType.GetMethod(
        "ShowFolderContents", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
 
    // Find any open project browser windows.
    Object[] projectBrowserInstances = Resources.FindObjectsOfTypeAll(projectBrowserType);
 
    if (projectBrowserInstances.Length > 0)
    {
        for (int i = 0; i < projectBrowserInstances.Length; i++)
        {
          // Sadly, there is no method to check for the view mode.
          // We can use the serialized object to find the private property.
          SerializedObject serializedObject = new SerializedObject(projectBrowserInstances[i]);
          bool inTwoColumnMode = serializedObject.FindProperty("m_ViewMode").enumValueIndex == 1;
          
          if (inTwoColumnMode)
            return serializedObject.FindProperty("m_SelectedPath").stringValue;
        }
    }
    return DrawPrefs.Instance.m_SaveFolder;
  }

  /// <summary>
  /// Saves the texture and returns the AssetDatabase path to the generated asset
  /// </summary>
  void Save(out SaveInfo outInfo, bool SaveAs = false, bool SaveSpritesheet = false) {
    var containerPath = DrawPrefs.Instance.m_SaveFolder;
    
    // Debug.Log(GetProjectWindowCurrentFolder());

    var now = System.DateTime.Now;
    string filename = "";
    if (!string.IsNullOrEmpty(m_NewName))
      filename = m_NewName;
    else if (DrawPrefs.GENERATE_FUNNY_NAMES)
      filename = NameGenerator.GetName();
    else
      filename = string.Format ("Animation_{0}-{1}-{2}_{3}-{4}-{5}", now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
    
    var sep = Path.DirectorySeparatorChar;
    var openAssetPath = OpenAssetPath;

    var openAnimationFile = AssetDatabase.LoadAssetAtPath<DoodleAnimationFile>(openAssetPath);
    var openTexture = openAnimationFile == null ? AssetDatabase.LoadAssetAtPath<Texture2D>(openAssetPath) : null;

    bool newFile = string.IsNullOrEmpty(m_CurrentOpenAssetGUID) || SaveAs;
    
    if (!newFile) {
      /*if (!EditorUtility.DisplayDialog ("Replace existing animation?", string.Format ("Save over {0}.png?\nCan't be undone.", m_CurrentOpenAssetPath.name), "Replace", "Cancel")) {
        outInfo = new SaveInfo ();
        return;
      }
      */
      filename = Path.GetFileNameWithoutExtension(openAssetPath);
      containerPath = Path.GetDirectoryName(openAssetPath);
      if (containerPath.StartsWith("Assets" + sep))
        containerPath = containerPath.Remove(0, "Assets".Length + 1);
    } else {
      // Ensure we're not replacing an existing file
      while(
        File.Exists(string.Format ("{0}"+sep+"{1}"+sep+"{2}.png", Application.dataPath, containerPath, filename)) ||
        File.Exists(string.Format ("{0}"+sep+"{1}"+sep+"{2}.asset", Application.dataPath, containerPath, filename))
      ) {
          filename += "1";
      }
    }
  
    Directory.CreateDirectory (Path.GetDirectoryName (string.Format ("{0}"+sep+"{1}", Application.dataPath, containerPath)));

    //Debug.Log("Saving to " + folder);

    bool saveSpriteSheet = SaveSpritesheet;
    if (DrawPrefs.REPLACE_LOADED_TEXTURES) {
      saveSpriteSheet = openTexture != null;
    }

    DoodleAnimationFile asset = null;
    SaveInfo saveinfo = new SaveInfo();

    Selection.objects = new Object[]{};

    if (UseSoundRecorder && SoundRecorder.HasRecordings()) {
      SoundRecorder.Save(string.Format("Asset"+sep+"{0}", containerPath), filename, false);
      List<AudioClip> soundAssets = new List<AudioClip>();
      foreach(SoundRecorder.Sound sound in SoundRecorder.Sounds) {
        if (string.IsNullOrEmpty(sound.m_SavedGUID)) {
          continue;
        }
        soundAssets.Add(AssetDatabase.LoadAssetAtPath<AudioClip>(AssetDatabase.GUIDToAssetPath(sound.m_SavedGUID)));
      }
      saveinfo.soundAssets = soundAssets;
    }
    
    // Save animation file
    var animationFileAssetPath = string.Format("Assets"+sep+"{0}"+sep+"{1}.asset", containerPath, filename);
    bool replacingAnimationFile = !newFile && openAnimationFile != null;

    // 1. Get the the asset we need to edit
    if (!replacingAnimationFile) {
      asset = ScriptableObject.CreateInstance<DoodleAnimationFile>();
      openAnimationFile = asset;
    } else {
      animationFileAssetPath = openAssetPath;
      asset = openAnimationFile;
    }

    // 2. Assign all the asset's properties    
    asset.version = DrawPrefs.VERSION;
    asset.framesPerSecond = (FramesPerSecond)m_FramesPerSecond;
    asset.playbackMode = m_PlaybackMode;
    asset.spriteBorder = m_SpriteBorder;
    asset.sounds = saveinfo.soundAssets;
    asset.filterMode = m_FilterMode;
    asset.darkCheckerboard = m_DarkCheckerboard;
    asset.patternMode = m_PatternMode;

    // 3. Create or save the asset on disk
    if (!replacingAnimationFile) {
      // Recreate all the sprites and store them in the asset
      //     Needs to be called before CreateAsset() so a static preview is generated
      asset.CreateAllSpritesAndTextures(m_KeyFrames); 

      DrawUtils.EnsureDirectoryExistsForFile(animationFileAssetPath);
      AssetDatabase.CreateAsset(asset, animationFileAssetPath);

      // Needs to be called after CreateAsset() so the sub assets have an object to be stored at 
      asset.SaveSubAssets(); 
    } else {
      // Destroy all the sprites inside the existing asset
      asset.ClearSubAssets();
      // Recreate all the sprites and store them in the asset
      asset.CreateAllSpritesAndTextures(m_KeyFrames);
      // Save everything
      asset.SaveSubAssets();
    }

    AssetDatabase.Refresh();
    EditorUtility.SetDirty(asset);
    // AssetDatabase.ImportAsset(animationFileAssetPath, ImportAssetOptions.ForceSynchronousImport);

    var selected = Selection.objects;
    System.Array.Resize(ref selected, selected.Length + 1);
    selected[selected.Length - 1] = AssetDatabase.LoadAssetAtPath<Object>(animationFileAssetPath);
    Selection.objects = selected;

    saveinfo.animationFileGUID = AssetDatabase.AssetPathToGUID(animationFileAssetPath);

    if (!replacingAnimationFile)
      m_CurrentOpenAssetGUID = saveinfo.animationFileGUID;

    if (saveSpriteSheet) {
      var spriteSheet = openAnimationFile.SaveAsSpritesheet();
      if (spriteSheet) {
        System.Array.Resize(ref selected, selected.Length + 1);
        selected[selected.Length - 1] = spriteSheet;
        Selection.objects = selected;
      }
    }

    AssetDatabase.SaveAssets();
		// #if UNITY_2018_1_OR_NEWER
    // #else
    // EditorApplication.projectWindowChanged.Invoke();
    // #endif

    //StopDrawing();

    /*
  if (!AssetDatabase.IsValidFolder(folder)) {
   AssetDatabase.CreateFolder("Assets", "Textures");
   AssetDatabase.CreateFolder("Assets/Textures", "Drawn");
  }
  AssetDatabase.CreateAsset(m_Texture, folder + "texture" + Time.time + ".png");*/

    m_NeedsUIRedraw = false;
    m_UnsavedChangesPresent = false;

    outInfo = saveinfo;
  }
  internal void Save() { SaveInfo si; Save(out si); }
  void SaveAs() { SaveInfo si; Save(out si, true); }

  void Stamp(SerializableTexture2D Layer, float X, float Y, Texture2D Brush, bool ReplaceAlpha = true, float Size = 1, Color? TintColor = null, float InsideRadius = -1, bool checkSymmetry = true) {
    // get dest rect
    var destrect = new Rect (Vector2.zero, Vector2.one * Layer.texture.height * BrushSize * Size);
    destrect.center = new Vector2 (X, Y);
    destrect.width = Mathf.Max (destrect.width, 1);
    destrect.height = Mathf.Max (destrect.height, 1);

    Color tint = TintColor.GetValueOrDefault();
    if (TintColor != null) {
      // Dilate: Avoid black borders by setting transparent pixels' color
      for (int xx = -3; xx < destrect.width + 3; xx++) {
        for (int yy = -3; yy < destrect.height + 3; yy++) {
          var destx = (int)destrect.x + xx;
          var desty = (int)destrect.y + yy;
          SetTransparentColor(Layer, destx, desty, tint);
        }
      }
    }
    
    var center = new Vector2(destrect.width * .5f, destrect.height * .5f);
    for (int xx = 0; xx < destrect.width; xx++) {
      for (int yy = 0; yy < destrect.height; yy++) {
        if (InsideRadius > 0) {
          var pt = new Vector2(xx, yy);
          if (Vector2.Distance(pt, center) > InsideRadius)
            continue;
        }
        var destx = (int)destrect.x + xx;
        var desty = (int)destrect.y + yy;
        if (!IsPixelDrawable(Layer, ref destx, ref desty))
          continue;
        var brushColor = Brush.GetPixelBilinear ((float)xx / destrect.width, (float)yy / destrect.height);

        var targetColor = TintColor != null ? tint : brushColor;
        var prevColor = Layer.GetPixelFastNoBuffer (destx, desty);
        var newColor = ReplaceAlpha ? targetColor : Color.Lerp (prevColor, targetColor, brushColor.a);
        SetPixel (Layer, destx, desty, newColor, checkSymmetry);
      }
    }
  }

  /// <summary>
  /// Draw into the texture
  /// </summary>
  void DrawAt(SerializableTexture2D Layer, float X, float Y, Color BrushColor, float Size = 1) {
    var texture = Layer.texture;
    if (texture == null)
      return;
    if (Mathf.Approximately (Size, 0))
      return;
    //Debug.Log("Draw on " + Position.x + ", " + Position.y);
    int x = Mathf.FloorToInt (X);
    int y = Mathf.FloorToInt (Y);
    Layer.EnsurePixelsArray();

    if (m_CurrentTool == Tool.Color && DrawPrefs.Instance.m_CustomBrush) {
      Stamp(Layer, X, Y, DrawPrefs.Instance.m_CustomBrush, false, Size, BrushColor, -1, false);
      foreach(var p in _v2list)
        Stamp(Layer, p.x, p.y, DrawPrefs.Instance.m_CustomBrush, false, Size, BrushColor, -1, false);
    } else {
      if (m_CurrentTool != Tool.Eraser) {
        // Dilate: Avoid black borders by setting transparent pixels' color   // TODO: only do it for pixels outside the circle
        var dilateSize = Vector2.one * Mathf.Ceil(texture.height * BrushSize * Size + 5);
        SetTransparentColor(Layer, new Rect(x - dilateSize.x * .5f, y - dilateSize.y * .5f, dilateSize.x, dilateSize.y), BrushColor);
        foreach(var p in _v2list)
          SetTransparentColor(Layer, new Rect(p.x - dilateSize.x * .5f, p.y - dilateSize.y * .5f, dilateSize.x, dilateSize.y), BrushColor);
      }

      DrawCircle (Layer, x, y, texture.height * 0.5f * BrushSize * Size, BrushColor);
    }
    //texture.Apply();  // moved to Layer.Apply()
    m_LastDrawnPoint = new Vector2 (x, y);
  }

  void SetTransparentColor(SerializableTexture2D Layer, int x, int y, Color color) {
    if (!IsPixelDrawable(Layer, ref x, ref y))
      return;
    var prevColor = Layer.GetPixelFastNoBuffer (x, y);
    if (prevColor.a < 1) {
      SetPixel (Layer, x, y, 
        new Color(color.r, color.g, color.b, prevColor.a),
        false // don't do this on the symmetry points, so the previous color check is correct
      );
    }
  }

  // Sets the color of a rect's pixels while keeping the alpha value intact, to avoid black borders
  void SetTransparentColor(SerializableTexture2D Layer, Rect destrect, Color color) {
    destrect.width = Mathf.Max (destrect.width, 1);
    destrect.height = Mathf.Max (destrect.height, 1);
    for (int xx = 0; xx < destrect.width; xx++) {
      for (int yy = 0; yy < destrect.height; yy++) {
        var destx = (int)destrect.x + xx;
        var desty = (int)destrect.y + yy;
        SetTransparentColor(Layer, destx, desty, color);
      }
    }
  }

  void DrawCircle(SerializableTexture2D Layer, int centerx, int centery, float radius, Color BrushColor) {
    int x, y, px, nx, py, ny, d;
    Layer.EnsurePixelsArray();
    for (x = 0; x <= radius; x++) {
      d = (int)Mathf.Ceil (Mathf.Sqrt (radius * radius - x * x));
      for (y = 0; y <= d; y++) {
        px = centerx + x;
        nx = centerx - x;
        py = centery + y;
        ny = centery - y;

        SetPixel (Layer, px, py, BrushColor);
        SetPixel (Layer, nx, py, BrushColor);

        SetPixel (Layer, px, ny, BrushColor);
        SetPixel (Layer, nx, ny, BrushColor);

      }
    }
    Layer.UpdateTextureFromBuffer ();
  }

  bool IsPixelDrawable(SerializableTexture2D Layer, ref int x, ref int y) {
    if (PatternModeOn) {
      x = (int)Mathf.Repeat(x, Layer.width);
      if (m_PatternMode == PatternMode.Horizontal && !Layer.rect.Contains(x, y))
        return false;
      y = (int)Mathf.Repeat(y, Layer.height);
      return true;
    }
    if (!Layer.rect.Contains(x, y))
      return false;
    return true;
  }

  // this'll get called thousands of times!
  internal void SetPixel(SerializableTexture2D Layer, int x, int y, Color32 color, bool checkSymmetry = true) {
    if (!IsPixelDrawable(Layer, ref x, ref y))
      return;
    
    if (m_CurrentTool == Tool.Eraser) {
      color = Layer.GetPixelFastNoBuffer(x, y);
      color.a = 0;
    }
    if (checkSymmetry) {
      if (m_SymmetryMode == SymmetryMode.Horizontal || m_SymmetryMode == SymmetryMode.Fourways)
        SetPixel (Layer, Mathf.Abs (Layer.width - 1 - x), y, color, false);
      if (m_SymmetryMode == SymmetryMode.Vertical || m_SymmetryMode == SymmetryMode.Fourways || m_SymmetryMode == SymmetryMode.PlayingCard)
        SetPixel (Layer, m_SymmetryMode == SymmetryMode.PlayingCard ? Mathf.Abs(x - Layer.width) : x, Mathf.Abs (Layer.height - 1 - y), color, false);
      if (m_SymmetryMode == SymmetryMode.Fourways)
        SetPixel (Layer, Mathf.Abs (Layer.width - 1 - x), Mathf.Abs (Layer.height - 1 - y), color, false);
      if (m_SymmetryMode == SymmetryMode.Radial) {
        int s = DrawPrefs.Instance.m_RadialSymmetryRepetitions;
        for(int i = 1; i < s; i++) {
          var v = new Vector2(x, y);
          v -= new Vector2(Layer.width * .5f, Layer.height * .5f);
          v = v.Rotate(((float)i / (float)s) * 360f);
          v += new Vector2(Layer.width * .5f, Layer.height * .5f);
          var xx = Mathf.RoundToInt(v.x);
          var yy = Mathf.RoundToInt(v.y);
          // Now we make sure the resulting point is repeated if in pattern mode to avoid clipping
          IsPixelDrawable(Layer, ref xx, ref yy);
          SetPixel(Layer, xx, yy, color, false);
        }
      }
    }

    if(m_CurrentTool == Tool.Color)
    {
      bool skip = false;
      switch (m_AlphaMode)
      {
        case AlphaMode.PaintOnlyBehind:
          if (Layer.GetPixelFastNoBuffer(x, y).a > 0)
            skip = true;;
        break;
        case AlphaMode.PaintOnlyInside:
          if (Layer.GetPixelFastNoBuffer(x, y).a < 0.01f)
            skip = true;;
        break;
        case AlphaMode.IgnoreStrokes:
          var col = Layer.GetPixelFastNoBuffer(x, y);
          if (col.a > 0.5 && col.r * col.g * col.b < 0.025f)
            skip = true;
        break;
      }
      if (skip)
        return;
    }


    if ((m_CurrentTool == Tool.Color || m_CurrentTool == Tool.Fill) && DrawPrefs.Instance.m_CustomPattern) {
      int w = DrawPrefs.Instance.m_CustomPattern.width;
      int h = DrawPrefs.Instance.m_CustomPattern.height;
      float a = (float)w / (float)h;
      var pc = DrawPrefs.Instance.m_CustomPattern.GetPixel(
        (int)Mathf.Repeat(((float)x / (float)Layer.width) * w * a * DrawPrefs.PATTERN_REPETITIONS, w),
        (int)Mathf.Repeat(((float)y / (float)Layer.height) * h * DrawPrefs.PATTERN_REPETITIONS, h)
      );
      if (pc.a > 0 && new Color(pc.r, pc.g, pc.b, pc.a).grayscale < 1) // TODO: optimize
        color = pc;
    }
    Layer.SetPixelFastNoBuffer (x, y, color);
    
    m_UnsavedChangesPresent = true;
  }

  void LineTo(SerializableTexture2D Layer, float fromX, float fromY, float toX, float toY, Color BrushColor, float Size = 1) {
    Layer.EnsurePixelsArray();
    
    var from = new Vector2(fromX, fromY);
    var to = new Vector2 (toX, toY);
    if ((to - from).magnitude >= 1) {
      #if USE_OPTIMIZED_LINE_DRAWING
      PlotLineWidth(Layer, (int)fromX, (int)fromY, (int)toX, (int)toY, Size * BrushSize * Layer.height, BrushColor);
      #else
      var dir = (to - from).normalized;
      while ((to - from).magnitude > DrawPrefs.LINE_TO_FREQUENCY) {
        from += dir * 1.0f;
        DrawAt (Layer, from.x, from.y, BrushColor, Size);
      }
      #endif
    }
    DrawAt (Layer, toX, toY, BrushColor, Size);
    Layer.UpdateTextureFromBuffer();
  }

  void setPixelColor(SerializableTexture2D Layer, Color color, int x, int y, float alpha)
  {
    color.a *= 1.0f-alpha/255f;
    SetPixel(Layer, x, y, color);
  }

  /////

  void PlotLineWidth(SerializableTexture2D Layer, int x0, int y0, int x1, int y1, float wd, Color color)
  { 
    int dx = Mathf.Abs(x1-x0), sx = x0 < x1 ? 1 : -1; 
    int dy = Mathf.Abs(y1-y0), sy = y0 < y1 ? 1 : -1; 
    int err = dx-dy, e2, x2, y2;
    float ed = dx+dy == 0 ? 1 : Mathf.Sqrt((float)dx*dx+(float)dy*dy);
    
    for (wd = (wd+1)/2; ; ) {
        setPixelColor(Layer, color, x0,y0, Mathf.Max(0,255*(Mathf.Abs(err-dx+dy)/ed-wd+1)));
        e2 = err; x2 = x0;
        if (2*e2 >= -dx) {
          for (e2 += dy, y2 = y0; e2 < ed*wd && (y1 != y2 || dx > dy); e2 += dx)
              setPixelColor(Layer, color, x0, y2 += sy, Mathf.Max(0,255*(Mathf.Abs(e2)/ed-wd+1)));
          if (x0 == x1) break;
          e2 = err; err -= dy; x0 += sx; 
        } 
        if (2*e2 <= dy) {
          for (e2 = dx-e2; e2 < ed*wd && (x1 != x2 || dx < dy); e2 += dy)
              setPixelColor(Layer, color, x2 += sx, y0, Mathf.Max(0,255*(Mathf.Abs(e2)/ed-wd+1)));
          if (y0 == y1) break;
          err += dx; y0 += sy; 
        }
    }
  }


  void JumbleAt(SerializableTexture2D Layer, float X, float Y, float Size = 1) {
    bool RANDOMLY_ERASE = false;
    // get dest rect
    var destrect = new Rect (Vector2.zero, Vector2.one * Layer.height * BrushSize * Size);
    destrect.center = new Vector2 (X, Y);
    destrect.width = Mathf.Max (destrect.width, 1);
    destrect.height = Mathf.Max (destrect.height, 1);
    float intensity = m_JumbleALot ? 4 : 1;
    Layer.EnsurePixelsArray();
    for(int i = 0; i < 20 * intensity; i++) {
      float fromSize = 1.0f * intensity;
      var targetFrom = destrect.center + Random.insideUnitCircle * destrect.width * .5f * fromSize;
      if (!PatternModeOn && !Layer.rect.Contains(targetFrom.x, targetFrom.y))
        continue;
      float toSize = 0.15f * intensity;
      var targetTo = targetFrom + Random.insideUnitCircle * destrect.width * .5f * fromSize * toSize;
      if (!PatternModeOn && !Layer.rect.Contains(targetTo.x, targetTo.y))
        continue;
      
      if (PatternModeOn) { 
        targetFrom = targetFrom.Repeat(Layer.size); 
        targetTo = targetTo.Repeat(Layer.size); 
      }
      var colorTo = Layer.GetPixelFastNoBuffer ((int)targetTo.x, (int)targetTo.y);
      if (RANDOMLY_ERASE && Random.value < 0.2f) {
        Layer.SetPixelFastNoBuffer((int)targetTo.x, (int)targetTo.y, DrawUtils.TRANSPARENCY_COLOR);
      } else {
        Layer.SetPixelFastNoBuffer((int)targetTo.x, (int)targetTo.y, Layer.GetPixelFastNoBuffer((int)targetFrom.x, (int)targetFrom.y));
      }
      Layer.SetPixelFastNoBuffer((int)targetFrom.x, (int)targetFrom.y, colorTo);
      
    }
    Layer.UpdateTextureFromBuffer();
  }

  void UpdateGrabImage(SerializableTexture2D Layer, float X, float Y, float Size = 1) {
    float radius = Layer.height * 0.5f * BrushSize * Size;
    int size = Mathf.CeilToInt(radius * 2);
    var r = new Rect(0,0, size, size);
    r.center = new Vector2(X, Y);
    if (!PatternModeOn)
      r = r.CropToFit(Layer.rect);
    // Debug.Log(r);
    m_GrabImage = DrawUtils.GetTextureCopy(Layer.texture, r, PatternModeOn, false);
    var center = new Vector2(radius, radius);
    for(int x = 0; x < m_GrabImage.width; x++) {
      for(int y = 0; y < m_GrabImage.height; y++) {
        var pt = new Vector2(x, y);
        if (Vector2.Distance(pt, center) > (radius + 2))
          m_GrabImage.SetPixel(x, y, Color.clear);
      }
    }
    m_GrabImage.Apply();
  }

  void Grab(SerializableTexture2D Layer, float FromX, float FromY, float ToX, float ToY, float Size = 1) {
    int deltax = Mathf.RoundToInt(ToX - FromX);
    int deltay = Mathf.RoundToInt(ToY - FromY);
    if (deltax == 0 && deltay == 0)
      return;

    float radius = Layer.height * 0.5f * BrushSize * Size;
    Layer.EnsurePixelsArray();

    if (m_GrabImage == null)
      UpdateGrabImage(Layer, FromX, FromY, Size);

    var from = new Vector2(FromX, FromY);
    var to = new Vector2(ToX, ToY);
    var dir = (to - from).normalized;
      int x, y, px, py;
    switch(m_Grab_Mode) {
      case GrabMode.Normal:
        while ((to - from).magnitude > 2) {
          from += dir * 1.0f;
          Stamp(Layer, from.x, from.y, m_GrabImage, true, Size, null, radius, false);
        }
        Stamp(Layer, ToX, ToY, m_GrabImage, true, Size, null, radius, false);
      break;

      case GrabMode.Smear:
      case GrabMode.Blurry:
      var copy = DrawUtils.GetTextureCopy(Layer.texture, null, false);//PatternModeOn);
      for(x = 0; x < radius * 2; x++) {
        for(y = 0; y < radius * 2; y++) {
          var p = new Vector2(x - radius, y - radius);
          float d = Vector2.Distance(Vector2.zero, p) / radius;
          if (d > 1)
            continue;
          px = Mathf.RoundToInt(from.x + p.x);
          py = Mathf.RoundToInt(from.y + p.y);
          int tx = px + deltax;
          int ty = py + deltay;
          if (PatternModeOn) {
            px = (int)Mathf.Repeat(px, Layer.width);
            py = (int)Mathf.Repeat(py, Layer.height);
            tx = (int)Mathf.Repeat(tx, Layer.width);
            ty = (int)Mathf.Repeat(ty, Layer.height);
          } else if (!Layer.rect.Contains(px, py) || !Layer.rect.Contains(tx, ty)) {
            continue;
          }
          
          Layer.SetPixelFastNoBuffer(tx, ty,
            // m_GrabImage.GetPixel(x, y) // Linear
            m_Grab_Mode == GrabMode.Blurry ? 
              copy.GetPixelBilinear(
                Mathf.Repeat((tx - Mathf.SmoothStep(0, (ToX - FromX), 1-d)) / (float)Layer.width, Layer.width),
                Mathf.Repeat((ty - Mathf.SmoothStep(0, (ToY - FromY), 1-d)) / (float)Layer.height, Layer.height)
              )
            :
            copy.GetPixel(
              (int)Mathf.Repeat(Mathf.RoundToInt(tx - Mathf.Lerp(0, (ToX - FromX), (1-d)*(1-d))), Layer.width),
              (int)Mathf.Repeat(Mathf.RoundToInt(ty - Mathf.Lerp(0, (ToY - FromY), (1-d)*(1-d))), Layer.height)
            )
            // Color.Lerp(Color.black, Color.white, 1-d) // visualize intensity
          );
        }
      }
      break;
    }

    Layer.UpdateTextureFromBuffer();
  }

  void Fill(SerializableTexture2D Layer, float X, float Y, Color BrushColor) {
    Layer.EnsurePixelsArray();
    bool repeatX = m_PatternMode != PatternMode.Disabled && m_PatternMode != PatternMode.Vertical;
    bool repeatY = m_PatternMode != PatternMode.Disabled && m_PatternMode != PatternMode.Horizontal;
    if (PatternModeOn) {
      if (repeatX)
        X = (int)Mathf.Repeat(X, Layer.width);
      if (repeatY)
        Y = (int)Mathf.Repeat(Y, Layer.height);
    } else if (!Layer.rect.Contains(X, Y)) {
      return;
    }
    int x = Mathf.FloorToInt (X);
    int y = Mathf.FloorToInt (Y);
  
    var col = Layer.GetPixelFastNoBuffer (x, y);
    if (BrushColor.Equals(col))
      return;
  
    //RecursiveFloodFill(Layer, x, y, col, BrushColor);
    //Layer.Apply(true);
    m_FloodFillOps.Add (new FloodFillOperation (Layer, x, y, col, BrushColor, m_FloodFillType, repeatX, repeatY));
  }

 

  #endregion

  #region SceneView

  void OnSceneGUI(SceneView sceneView) {
    if ((!UseSceneViewDrawing || !m_ShowSceneViewGizmo) && m_SceneViewPreviewObject) {
      DestroyPreview();
    } else if (UseSceneViewDrawing && m_ShowSceneViewGizmo && !m_SceneViewPreviewObject) {
      CreatePreview();
    }

    var keyframe = (m_KeyFrames != null && m_KeyFrames.Count > 0) ? GetCurrentKeyFrame () : null;
    SerializableTexture2D frameTex = null;
    if (keyframe != null && keyframe.Texture != null)
      frameTex = keyframe.Texture;
    if (frameTex != null && !frameTex.IsValid) {
      // Image is no longer valid, perhaps it was deleted
      frameTex = null;
    }

    var vp = sceneView.camera.ScreenToViewportPoint(Event.current.mousePosition);

    if (UseSceneViewDrawing) {
      
      // UI
      Handles.BeginGUI();
      GUILayout.BeginArea(new Rect(5, 5, 60, 90));
      var origColor = GUI.color;
      GUI.color = m_SceneViewDrawModeOn ? Color.red : Color.white;
      if (GUILayout.Button(m_SceneViewDrawModeOn ? "Stop" : "Draw", StaticResources.GetStyle("drawbutton"), GUILayout.ExpandWidth(true), GUILayout.Height(30))) {
        m_ShowSceneViewGizmo = true;
        if (m_SceneViewPreviewObject == null)
          CreatePreview();
        SetSceneViewDrawing(sceneView, !m_SceneViewDrawModeOn);
      }

      // GUI.color = m_ShowSceneViewGizmo ? COLOR_ACTIVE : Color.white;
      // var r = new Rect(5, 35, 30, 30);
      // if (StaticResources.GetEditorSprite("buttonframe_rounded.png").DrawAsButton(r, "", false, false, false, -1, true)) {
      //   m_ShowSceneViewGizmo = !m_ShowSceneViewGizmo;
      //   DestroyPreview();
      // }
      // GUI.DrawTexture(r.ScaleCentered(0.5f), StaticResources.GetTexture2D("sceneviewdraw.png"));
      // GUI.color = Color.white;
      
      GUI.color = origColor;
      GUILayout.EndArea();
      Handles.EndGUI();

      // Input
      if (m_SceneViewPreviewObject != null) {
        
        Handles.matrix = m_SceneViewPreviewObject.transform.localToWorldMatrix;
        Handles.color = Color.black;
        Handles.DrawDottedLines(new Vector3[]{new Vector3(-0.5f,-0.5f,0),new Vector3(0.5f,-0.5f,0),new Vector3(0.5f,0.5f,0),new Vector3(-0.5f,0.5f,0)},
          new int[]{0,1,1,2,2,3,3,0}, 2);
        Handles.color = COLOR_ACTIVE;
        Handles.DrawWireCube(Vector3.zero, new Vector3(1.05f,1.05f,0.0f));
        HandleUtility.Repaint();

        Event e = Event.current;

        if (m_SceneViewDrawModeOn) {
          
          // Update our input handler
          bool inside = vp.x >= 0 && vp.x <= 1 && vp.y >= 0 && vp.y <= 1;
          m_InputRectSceneView.Update(inside);

          if (DrawPrefs.SHOW_DEBUG_INPUTRECTS) {
            Handles.BeginGUI();
            m_InputRectSceneView.DebugDraw(new Rect(0,100,100,30));
            Handles.EndGUI();
          }

          bool drawAllowed = m_InputRectSceneView.MouseOver ? ProcessInput(frameTex, m_InputRectSceneView) : false;
          if (m_InputRectSceneView.MousePressing && drawAllowed) {
            
            var mouseRay = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            float hitDistance = 0;
            m_SceneViewPreviewPlane.SetNormalAndPosition(m_SceneViewPreviewObject.transform.forward, m_SceneViewPreviewObject.transform.position);
            bool hit = m_SceneViewPreviewPlane.Raycast(mouseRay, out hitDistance);
            if (hit) {
              Vector2 coord = m_SceneViewPreviewObject.transform.InverseTransformPoint(mouseRay.GetPoint(hitDistance)) + Vector3.one * .5f;
              var texturePos = new Vector2( coord.x * keyframe.width, coord.y * keyframe.height);
              OnDrawPointHit(texturePos, keyframe);
            }
          }
          if (Event.current.isMouse) {
            // Capture input
            int controlID = GUIUtility.GetControlID (FocusType.Passive);
            GUIUtility.hotControl = controlID;
            e.Use();
            Tools.current = UnityEditor.Tool.None;
          }
          
        } 
      }
    }
  }

  void SetSceneViewDrawing(bool bDrawing) {
    foreach (var sv in SceneView.sceneViews) {
      SetSceneViewDrawing(sv as SceneView, bDrawing);
    }
  }
  void SetSceneViewDrawing(SceneView sceneView, bool bDrawing) {
    if (sceneView == null)
      return;
    m_SceneViewDrawModeOn = bDrawing;
    if (m_SceneViewDrawModeOn && m_SceneViewPreviewObject) {
      var objs = Selection.objects;
      Selection.activeObject = m_SceneViewPreviewObject;
      sceneView.FrameSelected();
      Selection.objects = objs;
    }
  }

  void OnApplicationUpdate() {
    
    // Check the preview object should exist
    if (DrawPrefs.Instance.m_SceneViewDrawing) { 

      // Check for material state, since we can't have a consistent callback when user creates a new scene
      bool sceneChanged = m_PreviewSpriteMaterial == null;
      
      // Workaround for preview object in weird state when user creates new scene, recreate the object
      if (sceneChanged && m_SceneViewPreviewObject != null && m_PreviewSpriteMaterial == null)
        DestroyPreview();
      
      if (m_SceneViewPreviewObject == null)
        CreatePreview();

      if (sceneChanged)
        SetSceneViewDrawing(false);
      
    } else {
      if (m_SceneViewPreviewObject != null)
        DestroyPreview();
    }

    
  }

  #endregion

  void Update() {
    m_DeltaTime = (float)(EditorApplication.timeSinceStartup - m_LastPlayEditorTime);
    if (m_Playing) {
      //int lastFrame = Mathf.FloorToInt (m_PlayTime);
      float delta = m_DeltaTime * m_FramesPerSecond;
      float newPlayTime = m_PlayTime + delta;

      int totalFrameLength = GetAnimationLength ();
    
      if (m_MakeNewFrames) {
        while (GetAnimationLength() <= Mathf.FloorToInt (newPlayTime))
          AddKeyFrame (true);
      } else {
        bool playheadPassedLength = newPlayTime >= totalFrameLength;
        if (UseSoundRecorder && playheadPassedLength && SoundRecorder.IsRecording()) {
          SoundRecorder.StopRecording();
        }
        if (m_PlaybackMode == PlaybackMode.Loop) {
          if (playheadPassedLength) {
            newPlayTime -= totalFrameLength;
            if (UseSoundRecorder)
              SoundRecorder.PlayPreview(newPlayTime / totalFrameLength);
          }
        } else if (m_PlaybackMode == PlaybackMode.LoopBackAndForth) {
          newPlayTime = m_PlayTime + delta * m_PlaybackDirection;
          if (playheadPassedLength) {
            newPlayTime = totalFrameLength + totalFrameLength - newPlayTime - 1;
            m_PlaybackDirection = -1;
            if (UseSoundRecorder)
              SoundRecorder.PlayPreview(newPlayTime / totalFrameLength, true);
          } else if (newPlayTime < 0) {
            newPlayTime = Mathf.Abs (newPlayTime) + 1;
            m_PlaybackDirection = 1;
            if (UseSoundRecorder)
              SoundRecorder.PlayPreview(newPlayTime / totalFrameLength);
          }
        } else if (m_PlaybackMode == PlaybackMode.Once && playheadPassedLength) {
          SetPlayback(false);
          newPlayTime = 0;
        }
      }
      m_PlayTime = newPlayTime;
      int currentFrame = Mathf.FloorToInt (Mathf.Clamp (m_PlayTime, 0, totalFrameLength - 1));

      /*
    int currentFrameAbs = Mathf.FloorToInt((float)playbackTime * (float)m_FramesPerSecond); 
    int currentFrame = (int)Mathf.Repeat(currentFrameAbs, totalFrameLength);
    
    if (m_MakeNewFrames) {
      while(m_KeyFrames.Count <= currentFrameAbs)
        AddKeyframe();
      currentFrame = currentFrameAbs;
    } else {
      if (m_PlaybackMode == PlaybackMode.LoopBackAndForth) {
        bool forward = Mathf.Repeat(currentFrameAbs, totalFrameLength * 2) <= totalFrameLength;
        if (!forward)
          currentFrame = totalFrameLength - 1 - currentFrame;
        Debug.LogFormat("currentFrameAbs {0} len {1} mod {2} result {3}", currentFrameAbs, totalFrameLength, Mathf.Repeat(currentFrameAbs, totalFrameLength * 2), currentFrame);
      } else if (m_PlaybackMode == PlaybackMode.Once && currentFrameAbs >= totalFrameLength) {
        currentFrameAbs = 0;
        m_Playing = false;
      }
    }
    */
      SetPlayhead (currentFrame);

    }

    if (!string.IsNullOrEmpty(m_CurrentPopup)) {
      var window = StaticResources.GetWindow(m_CurrentPopup);
      if (window != null && window.Elements.Count > 0) {
        window.AnimationTime += m_DeltaTime;
      }
    }

    if (m_FloodFillOps.Count > 0) {
      // Advance the flood fills
      for (int i = 0; i < m_FloodFillOps.Count; i++) {
        if (m_FloodFillOps [i].Advance ()) {
          m_UnsavedChangesPresent = true;
        } else {
          m_FloodFillOps.RemoveAt (i--);
        }
      }
    }
    
    m_LastPlayEditorTime = EditorApplication.timeSinceStartup;

    if (mouseOverWindow == this) {
      m_NeedsUIRedraw = true; // TODO: don't do this unless we moved
    }
    if (m_NeedsUIRedraw)
      Repaint ();
    
    
  }
  #region GUI

  void OnGUI() {

    // Check for reference image object picker
    // https://answers.unity.com/questions/554012/how-do-i-use-editorguiutilityshowobjectpicker-c.html
    if (Event.current != null && Event.current.commandName == "ObjectSelectorUpdated" && 
      EditorGUIUtility.GetObjectPickerControlID() == m_ReferenceImageObjectPickerID) {
      var obj = EditorGUIUtility.GetObjectPickerObject();
      bool hadImg = DrawPrefs.Instance.m_ReferenceImage != null;
      DrawPrefs.Instance.m_ReferenceImage = obj && obj is Texture2D ? obj as Texture2D : null;
      OnNewReferenceImageSet(hadImg, DrawPrefs.Instance.m_ReferenceImage);
    }

    EditorGUI.BeginChangeCheck ();

    #if DEBUG_VIEW
    debugText.Clear();
    debugText.Add("Drawing " + m_Drawing);
    debugText.Add("mouse down " + m_IsMouseDown);
    debugText.Add("zoom " + m_Zoom);
    debugText.Add("scroll " + m_ScrollPos);
    debugText.Add("last point " + m_LastDrawnPoint);
    #endif

    // vars
    var windowrect = new Rect (0, 0, position.width, position.height);
    var keyframe = (m_KeyFrames != null && m_KeyFrames.Count > 0) ? GetCurrentKeyFrame () : null;
    SerializableTexture2D frameTex = null;
    if (keyframe != null && keyframe.Texture != null)
      frameTex = keyframe.Texture;
    if (frameTex != null && !frameTex.IsValid) {
      // Image is no longer valid, perhaps it was deleted
      Debug.Log("Layer is no longer valid");
      frameTex = null;
    }

    //Debug.Log(keyframe.m_Layers + ", " + keyframe.m_Layers.Length + ", " + m_CurrentLayer);
    //Debug.Assert(layer != null);
    int totalFrameLength = GetAnimationLength ();
  
    var keyCode = Event.current != null && Event.current.type == EventType.KeyDown ? Event.current.keyCode : KeyCode.None;
    if (!string.IsNullOrEmpty(m_CurrentPopup))
      keyCode = KeyCode.None;
    bool keyNextFrame = keyCode == KeyCode.Period || keyCode == KeyCode.RightArrow;
    bool keyPrevFrame = keyCode == KeyCode.Comma || keyCode == KeyCode.LeftArrow;
    bool keyAddFrame = keyCode == KeyCode.F5;
    bool keyAddKeyFrame = keyCode == KeyCode.F6;
    bool keyPlay = keyCode == KeyCode.Space;
    bool keyRec = keyCode == KeyCode.R;
    bool keyOnionSkin = keyCode == KeyCode.Tab;
    bool keyToolColor = keyCode == KeyCode.B;
    bool keyToolEraser = keyCode == KeyCode.E;
    bool keyToolFill = keyCode == KeyCode.G;
    if(keyNextFrame || keyPrevFrame || keyAddFrame || keyAddKeyFrame ||
      keyPlay || keyRec || keyOnionSkin || keyToolColor || keyToolEraser || keyToolFill ||
      System.Array.IndexOf(colorkeys, keyCode) != -1) {
      Event.current.Use();
    }

    // Set up
    if (Event.current.type == EventType.MouseDown && Event.current.button == 0) {
      m_IsMouseDown = true;
    }
    else if (Event.current.type == EventType.MouseUp && Event.current.button == 0 || Event.current.type == EventType.DragExited) {
      m_IsMouseDown = false;
    }
    
    // Stop flood fills
    if (m_FloodFillType != FloodFillOperation.Type.Normal && !m_IsMouseDown && !Event.current.shift) {
      m_FloodFillOps.Clear ();
    }
    #if DEBUG_VIEW
    debugText.Add("open asset: " + OpenAssetPath);
    debugText.Add("Flood fill operations : " + m_FloodFillOps.Count);
    debugText.Add("Current Popup : " + m_CurrentPopup);
    if (frameTex != null)
      debugText.Add(string.Format("Image : {0}x{1}, {2}(actual:{3}, aip:{4}",
        m_TextureWidth, m_TextureHeight, m_FilterMode, frameTex.filterMode, frameTex.texture.alphaIsTransparency
      ));
    #endif
    
    if (keyOnionSkin)
      CycleOnionSkin();

    m_GuiEnabled = string.IsNullOrEmpty(m_CurrentPopup);
    GUI.enabled = m_GuiEnabled;
  
    // Whole thing
    var r_whole = windowrect;
    var r_sidebar = new Rect(r_whole.x, r_whole.y, 180, r_whole.height);
    var r_timeline = new Rect(r_sidebar.xMax + 10, r_whole.y, r_whole.width - r_sidebar.width - 5, 40);
    var r_playback = new Rect(r_timeline.x, r_timeline.yMax, r_timeline.width, 40);
    var r_drawarea = new Rect(r_timeline.x, r_playback.yMax, r_timeline.width, r_whole.height - r_playback.yMax);

    // background
    {
      var bg = StaticResources.GetTexture2D("bg_main.png");
      float t = m_BgScroll * 0.03f;
      GUI.DrawTextureWithTexCoords(r_whole, bg, new Rect(-t, 0, 3.0f * ((float)r_whole.width / (float)r_whole.height), 3.0f), false);

    }

    if (windowrect.width > 300) { // Right side    
    // Draw Area
    {
      // avoid scrollbars when zoomed in
      m_InputRectWindowDrawArea.Update(Mathf.Approximately(m_Zoom, 1) ? r_drawarea : new Rect(r_drawarea.x, r_drawarea.y, r_drawarea.width - 18, r_drawarea.height - 18), "m_InputRectWindowDrawArea");
      bool drawAllowed = m_InputRectWindowDrawArea.MouseOver ? ProcessInput(frameTex, m_InputRectWindowDrawArea) : false;

      var r_zoomed_drawarea = new Rect(0, 0, r_drawarea.width * m_Zoom, r_drawarea.height * m_Zoom);

      m_ScrollPos = GUI.BeginScrollView (new Rect (r_drawarea.x, r_drawarea.y, r_drawarea.width, r_drawarea.height), m_ScrollPos, r_zoomed_drawarea);
    
      // Input
      #if DEBUG_VIEW
      debugText.Add("velocity " + m_VelocityDelta);
      debugText.Add("Keyframe " + keyframe);
      if (frameTex != null) {
        debugText.Add("layer " + frameTex);
        debugText.Add("layer valid " + (frameTex != null && frameTex.IsValid));
        debugText.Add("layer.pixels " + frameTex.pixels);
        debugText.Add("layer size " + frameTex.width);
        debugText.Add("layer texture " + frameTex.texture);
      }
      #endif
      if (keyframe != null && frameTex.texture != null) {
        {
          float aspect = (float)keyframe.width / (float)keyframe.height;
          r_zoomed_drawarea = r_zoomed_drawarea.ScaleCentered(45,45);
          //r_zoomed_drawarea.x += 10;
          //r_zoomed_drawarea.width -= 5;
          Rect r_texture_image = new Rect(0, 0, keyframe.width, keyframe.height).ScaleToFit(r_zoomed_drawarea);//
          if (PatternModeOn)
            r_texture_image = r_texture_image.ScaleCentered(0.33f);

          // Draw the border markers before drawing, since it includes an input check that stops drawing if we're dragging them
          if (!PatternModeOn)
            DrawSpriteBorderMarkers(r_texture_image, keyframe, ref drawAllowed);


          m_InputRectWindowTexture.Update(r_texture_image, "m_InputRectWindowTexture");
          m_InputRectWindowTexture.DebugDraw(r_texture_image, "m_InputRectWindowTexture");
          if (m_InputRectWindowDrawArea.MouseOver) {
            // Zoom
            if (Event.current.type == EventType.ScrollWheel) {
              m_Zoom = Mathf.Clamp(m_Zoom - Event.current.delta.y * 0.1f, 1, 20);
              CenterCanvas(r_drawarea);
            }
            // Drawing
            if (m_InputRectWindowDrawArea.MousePressing && drawAllowed) {
              var texturePos = new Vector2 (
                (Event.current.mousePosition.x - r_texture_image.x) / r_texture_image.width,
                1.0f - (Event.current.mousePosition.y - r_texture_image.y) / r_texture_image.height);
              texturePos.x *= keyframe.width;
              texturePos.y *= keyframe.height;

              OnDrawPointHit(texturePos, keyframe);
            }
          }
          #if DEBUG_VIEW
          debugText.Add("drawAllowed " + drawAllowed);
          #endif

          if (PatternModeOn) {
            int rx = m_PatternMode != PatternMode.Vertical ? 3 : 0;
            int ry = m_PatternMode != PatternMode.Horizontal ? 3 : 0;
            var rects = _patternRects;
            
            rects.Clear();
            for (int x = -rx; x <= rx; x++) {
              for (int y = -ry; y <= ry; y++) {
                rects.Add(new Rect(
                  r_texture_image.x + r_texture_image.width * x,
                  r_texture_image.y + r_texture_image.height * y,
                  r_texture_image.width, r_texture_image.height
                ));
              }
            }
            DrawCanvas(rects, keyframe, totalFrameLength, false);
            for (int x = -rx; x <= rx; x++) {
              for (int y = -ry; y <= ry; y++) {
                DrawCursor(r_texture_image, keyframe, new Vector2(r_texture_image.width * x, r_texture_image.height * y));
              }
            }
          } else {
            DrawCanvas(r_texture_image, keyframe, totalFrameLength);
            DrawCursor(r_texture_image, keyframe);
          }
        }
    
      }
      //EditorGUI.LabelField(new Rect(r_main.x,r_main.y,100,EditorGUIUtility.singleLineHeight), "r main size " + r_main.width + ", " + r_main.height);
      //EditorGUI.LabelField(new Rect(r_main.x,r_main.y + EditorGUIUtility.singleLineHeight,200,EditorGUIUtility.singleLineHeight), "r tex size " + r_texture.x + ", " + r_texture.y + " : " + r_texture.width + ", " + r_texture.height);

      GUI.EndScrollView (false);

      //GUI.EndGroup ();
    } // End r_drawarea

    // top menu
    {
      m_InputRectTopbar.Update(new Rect(r_timeline.x, r_timeline.y, r_timeline.width, r_timeline.height + r_playback.height), "m_InputRectTopbar");

      r_timeline = r_timeline.ScaleCentered(14,1);
      r_playback = r_playback.ScaleCentered(14,1);

      GUI.color = COLOR_TIMELINE_BACKGROUND;
      StaticResources.GetEditorSprite("bg_timeline.png").DrawFrame(0, r_timeline.Offset(0,0,0,7), true, false);
      GUI.color = Color.white;

      GUI.enabled = m_GuiEnabled && keyframe != null;
      {
          // Timeline drag scrub operation
          m_DraggingTimeline = Event.current.isMouse && !m_Drawing && m_InputRectTopbar.MousePressing && r_timeline.Contains (Event.current.mousePosition);
          
          r_timeline = r_timeline.ScaleCentered(10,10);
          
          int frames = 10;
          if (totalFrameLength > frames) frames = 30;
          if (totalFrameLength > frames) frames = 60;
          if (totalFrameLength > frames) frames = 120;

          float framewidth = r_timeline.width / (float)frames;
    
          for (int i = 0; i < frames; i++) {
            GUI.color = new Color (0.6f, 0.6f, 0.6f, 0.13f);
            var r = new Rect (r_timeline.x + (float)i * framewidth, r_timeline.y, framewidth, r_timeline.y + r_timeline.height);
            //GUI.Box(r, "");
            //GUI.DrawTexture(r, StaticResources.GetTexture2D("bg_thinborder.png"), ScaleMode.StretchToFill);
            StaticResources.GetEditorSprite("bg_thinborder.png").DrawFrame(0, r, true);
            GUI.color = Color.white;

            if (Event.current.mousePosition.x > r.x && Event.current.mousePosition.x < r.x + r.width) {
              if (m_DraggingTimeline) {
                // Scrub
                SetPlayback (false);
                //var scrubframe = Mathf.FloorToInt(((Event.current.mousePosition.x - r_frames.x) / r_frames.width) * framewidth);
                //Debug.Log(scrubframe);
                SetPlayhead (i, true);
              } else if (Event.current.button == 1 && i < GetAnimationLength()) {
                // Right click menu
                var k = GetKeyFrameAt(i);
                int keyframeI = m_KeyFrames.IndexOf(k);
                SetPlayhead(i);
                if (Event.current.type == EventType.MouseUp) {
                  var menu = new GenericMenu();
                  menu.AddItem(new GUIContent("Longer"), false, () => k.m_Length++);
                  if(k.m_Length > 1) 
                    menu.AddItem(new GUIContent("Shorter"), false, () => k.m_Length--); 
                  else
                    menu.AddDisabledItem(new GUIContent("Shorter"));
                  menu.AddSeparator("");
                  if (keyframeI > 0) {
                    menu.AddItem(new GUIContent("Move Left"), false, () => { 
                      m_KeyFrames.RemoveAt(keyframeI);
                      m_KeyFrames.Insert(keyframeI - 1, k); 
                      SetPlayhead(k);
                    } );
                  } else { menu.AddDisabledItem(new GUIContent("Move Left")); }
                  if (keyframeI < m_KeyFrames.Count - 1) {
                    menu.AddItem(new GUIContent("Move Right"), false, () => { 
                      m_KeyFrames.RemoveAt(keyframeI);
                      m_KeyFrames.Insert(keyframeI + 1, k); 
                      SetPlayhead(k);
                    } ); 
                  } else { menu.AddDisabledItem(new GUIContent("Move Right")); }
                  menu.AddSeparator("");
                  
                  menu.AddItem(new GUIContent("Duplicate"), false, () => DuplicateKeyFrame(k));
                  menu.AddSeparator("");
                  menu.AddItem(new GUIContent("Delete"), false, () => RemoveKeyFrame(k)); 
                  menu.ShowAsContext();
                  Event.current.Use();
                }
              }
            }
          }
          float lastx = r_timeline.x;
          for (int i = 0; i < frames; i++) {
            // keyframes
            var frame = m_KeyFrames.Count > i ? m_KeyFrames [i] : null;
            Rect r = new Rect (lastx, r_timeline.y, framewidth * (frame != null ? frame.m_Length : 1), r_timeline.y + r_timeline.height);
            if (frame != null) {
              GUI.color = DrawOnAllKeyframes ? COLOR_FRAME_THUMBNAIL_ALL : COLOR_FRAME_THUMBNAIL_SINGLE;
              if (m_KeyFrames [i] == keyframe)
                GUI.color *= 0.85f;
              StaticResources.GetEditorSprite("bg_thinborder.png").DrawFrame(0, r, true);
              GUI.color = Color.white;
              if (frame != null && frame.Texture != null && frame.Texture.texture != null) { // Geez
                r = r.ScaleCentered(0.75f);
                GUI.DrawTexture (r, frame.Texture.texture, ScaleMode.ScaleToFit, true);
              }
            }
            lastx += framewidth * (frame != null ? frame.m_Length : 1);
          }
    
          // playhead
          GUI.color = new Color (0,0,0, 0.8f);
          GUI.Box (new Rect (r_timeline.x + framewidth * m_PlayheadPosition + framewidth * 0.5f - 1, r_timeline.y - 3, 4, r_timeline.y + r_timeline.height + 6), "");
          
          GUI.color = Color.white;
        } // end Timeline
        GUI.enabled = m_GuiEnabled;
        
        float butwidth = r_playback.height * .74f;
        {
          var rects = DrawPlaybackButtons(new Rect(r_playback.x, r_playback.y, butwidth * 5, r_playback.height), 5);

          GUI.color = m_RecordingModeOn ? Color.red : new Color (0.8f, 0, 0);
          if (EditorSprite.DrawCompoundButton(rects[0], 
            "buttonframe_big.png","rec.png", "Record\nAdd frames while drawing") || 
              (GUI.enabled && keyRec)) {
            m_RecordingModeOn = !m_RecordingModeOn;
            //if (!m_Playing)
            //  SetPlayback (true);
          }
          GUI.color = Color.white;

          //GUI.enabled = m_PlayheadPosition - 1 >= 0;
          if (GUI.enabled && keyPrevFrame)
            SetPlayhead (m_PlayheadPosition - 1, true, true);

          GUI.color = COLOR_PLAY;
          if (EditorSprite.DrawCompoundButton(rects[1], 
            "buttonframe_big.png", m_Playing ? "play_pause.png" : "play.png", 
              m_Playing ? "Pause" : "Play") || 
              (GUI.enabled && keyPlay)) {
            SetPlayback (!m_Playing);
            }
          GUI.color = Color.white;
      
          GUI.enabled = m_GuiEnabled && totalFrameLength > 1;
          if (EditorSprite.DrawCompoundButton(rects[2], 
              "buttonframe_big.png", "ff.png", "Advance One Frame") || 
              (GUI.enabled && keyNextFrame)) {
            SetPlayhead (m_PlayheadPosition + 1, true, true);
            SetPlayback(false);
          }
          GUI.enabled = m_GuiEnabled;

          GUI.color = COLOR_ACTIVE;
          {
            if (EditorSprite.DrawCompoundButton(new Rect(0,0,1,1).ScaleToFit(rects[3]).ScaleCentered(4,4), 
                "buttonframe_rounded.png", playbackModeIcons[playbackModes.IndexOf(m_PlaybackMode)], 
                m_PlaybackMode == PlaybackMode.Loop ? "Playback\nLoop" : (m_PlaybackMode == PlaybackMode.LoopBackAndForth ? "Playback\nBack and Forth" : "Playback\nOnce")
                )) {
              m_RecordingModeOn = m_MakeNewFrames = false;
              m_PlaybackMode = (PlaybackMode)playbackModes[(int)Mathf.Repeat(playbackModes.IndexOf(m_PlaybackMode) + 1, playbackModes.Count)];
            }
          }
          GUI.color = Color.white;

          GUI.color = COLOR_ACTIVE;
          {
            string[] icons = new string[]{"play_speed_slow.png", "play_speed_normal.png", "play_speed_fast.png"};
            string[] names = new string[]{ "Speed\nSlow", "Speed\nNormal", "Speed\nFast"};
            int i = Mathf.Clamp(speeds.IndexOf(m_FramesPerSecond), 0, icons.Length - 1);
            if (EditorSprite.DrawCompoundButton(new Rect(0,0,1,1).ScaleToFit(rects[4]).ScaleCentered(4,4), "buttonframe_rounded.png", icons[i], 
                names[i]
                )) {
              m_FramesPerSecond = (int)speeds[(int)Mathf.Repeat(i + 1, speeds.Count)];
            }
          }
          GUI.color = Color.white;
        } // End playback buttons

        // Sound recorder
        if (UseSoundRecorder) {
          var rects = DrawPlaybackButtons(new Rect(r_playback.center.x - butwidth * 1, r_playback.y, butwidth * 2, r_playback.height), 2);
        
          GUI.color = SoundRecorder.IsRecording() ? COLOR_ACTIVE : Color.white;
          if (EditorSprite.DrawCompoundButton(
            rects[0],
            "buttonframe_big.png","button_mic.png", "Record Sound") && m_KeyFrames.Count > 1) {
            if (!SoundRecorder.IsRecording()) {
              SetPlayhead(0);
              SetPlayback(true);
              SoundRecorder.StartRecording(totalFrameLength / (float)m_FramesPerSecond);
            } else {
              SoundRecorder.StopRecording();
            }
          }
          GUI.color = Color.white;

          GUI.color = SoundRecorder.CurrentSound.HasRecording() ? Color.green : (SoundRecorder.IsRecording() ? COLOR_ACTIVE : Color.white);
          if (StaticResources.GetEditorSprite("recording.png").DrawAsButton(rects[1].ScaleCentered(0.4f), "", false, false, false, 0, false))
            SoundRecorder.PlayPreview(0, true);
          
          GUI.color = Color.white;
          
        }
        
        
        // Frames buttons
        {
          var rects = DrawPlaybackButtons(new Rect(r_playback.xMax - butwidth * 5, r_playback.y, butwidth * 5, r_playback.height), 5);
          if (EditorSprite.DrawCompoundButton(rects[0], "buttonframe_big.png", "frame_add.png", "Add frame") || 
              (GUI.enabled && keyAddKeyFrame)) { 
            //Undo.RegisterCompleteObjectUndo(this, "Add Keyframe");
            keyframe = AddKeyFrame (false);
            frameTex = keyframe.Texture;
            SetPlayhead (keyframe);
          }
          if (EditorSprite.DrawCompoundButton(rects[1], "buttonframe_big.png", "frame_remove.png", "Remove frame")) { 
            RemoveKeyFrame (keyframe);
            keyframe = GetCurrentKeyFrame ();
            frameTex = keyframe.Texture;
          }
          if (EditorSprite.DrawCompoundButton(rects[2], "buttonframe_big.png", "frame_addtime.png", "Longer") || 
              (GUI.enabled && keyAddFrame))
            keyframe.m_Length++;  
          
          GUI.enabled = m_GuiEnabled && keyframe != null && keyframe.m_Length > 1;
          if (EditorSprite.DrawCompoundButton(rects[3], "buttonframe_big.png", "frame_removetime.png", "Shorter"))
            keyframe.m_Length--; 
          
          GUI.enabled = m_GuiEnabled;
          if (EditorSprite.DrawCompoundButton(rects[4], "buttonframe_big.png", "frame_duplicate.png", "Duplicate Frame"))
            DuplicateKeyFrame ();
          
          GUI.enabled = m_GuiEnabled;
        } // End frames buttons
        
      }// end top menu
    } // end right side

    // Sidebar (Left side)
    GUI.color = new Color(194/255f,193f/255f,129f/255f,1);
    GUI.DrawTextureWithTexCoords(
      new Rect(r_sidebar.x, r_sidebar.y, r_sidebar.width + 15, r_sidebar.height), 
      StaticResources.GetTexture2D("bg_sidebar.png"), 
      new Rect(0, 0, 1.0f, 0.8f * ((float)r_sidebar.height / (float)r_sidebar.width)), 
      true
    );

    
    GUI.color = Color.white;
    {
      float lastY = r_sidebar.y;
      var r_firstbuttons = new Rect(r_sidebar.x, lastY, r_sidebar.width, 40);
      r_firstbuttons.x += 5;
      r_firstbuttons.GetEvenRectsHorizontalNoAlloc(ref _layoutRects, 3, new Vector2(0, 3));
      var rects_firstbuttons = _layoutRects;
      {
        GUI.color = COLOR_FILEACTION;
        
        // New
        if (EditorSprite.DrawCompoundButton(rects_firstbuttons[0], 
          "buttonframe_smaller.png", "text_new.png", "New", 0.87f)) {
          m_NewName = "";
          OpenPopup("New");
        }
        
        // Save
        //GUI.enabled = m_KeyFrames.Count > 0 && m_UnsavedChangesPresent;
        GUI.color = COLOR_FILEACTION;          
        if (EditorSprite.DrawCompoundButton(rects_firstbuttons[1],
          "buttonframe_smaller.png", "text_save.png", "Save", 0.87f)) {
          if (!string.IsNullOrEmpty(m_CurrentOpenAssetGUID) && AssetDatabase.LoadAssetAtPath<DoodleAnimationFile>(OpenAssetPath) != null) {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Save (Replace)"), false, Save);
            menu.AddItem(new GUIContent("Save As..."), false, SaveAs); 
            menu.AddItem(new GUIContent("Save As Sprite Sheet"), false, delegate() {
              Save();
              DoodleAnimationFile asset = GetFileAsset();
              asset.SaveAsSpritesheet();
            }); 
            menu.AddItem(new GUIContent("Save as GIF"), false, delegate(){
              Save();
              DoodleAnimationFile asset = GetFileAsset();
              asset.SaveAsGif();
            }); 
            menu.ShowAsContext();
            Event.current.Use();
          } else {
            Save();
          }

        }
        // MAKE button
        GUI.enabled = m_GuiEnabled && m_KeyFrames.Count > 0;
        GUI.color = COLOR_FILEACTION;          
        if (EditorSprite.DrawCompoundButton(rects_firstbuttons[2], 
          "buttonframe_smaller.png", "text_make.png", "Add to scene", 0.87f)) {
          var menu = new GenericMenu();
          menu.AddItem(new GUIContent("Add as Sprite"), false, delegate() {
            SetSceneViewDrawing(false);
            EditorUtils.SelectAndFrame( GetFileAsset().MakeSprite(
              m_SceneViewPreviewObject ? m_SceneViewPreviewObject.transform.position : Vector3.zero,
              m_SceneViewPreviewObject ? m_SceneViewPreviewObject.transform.rotation : Quaternion.identity,
              m_SceneViewPreviewObject ? m_SceneViewPreviewObject.transform.localScale * .25f : Vector3.one
            ));
          });
          menu.AddItem(new GUIContent("Add as Sprite (Shadow Casting)"), false, delegate() {
            var obj = GetFileAsset();
            Vector3 scale = Vector3.one;
            if (m_SceneViewPreviewObject) {
              scale = new Vector3(
                1 * m_SceneViewPreviewObject.transform.localScale.x / ((float)obj.width / 100f),
                1 * m_SceneViewPreviewObject.transform.localScale.y / ((float)obj.height / 100f),
                1
              );
            }
            
            GameObject spriteObj = obj.Make3DSprite(
              m_SceneViewPreviewObject ? m_SceneViewPreviewObject.transform.position : Vector3.zero,
              m_SceneViewPreviewObject ? m_SceneViewPreviewObject.transform.rotation : Quaternion.identity,
              m_SceneViewPreviewObject ? scale : Vector3.one
            );

            if (!m_SceneViewPreviewObject)
              spriteObj.transform.Translate(Vector3.up * spriteObj.GetComponent<SpriteRenderer>().bounds.extents.y);
            
            SetSceneViewDrawing(false);
            EditorUtils.SelectAndFrame(spriteObj);
          });
          menu.AddItem(new GUIContent("Add as UI Image"), false, delegate() { 
            SetSceneViewDrawing(false);
            EditorUtils.SelectAndFrame(GetFileAsset().MakeUISprite()); 
          });
          menu.AddItem(new GUIContent("Add as Particles"), false, delegate() { 
            SetSceneViewDrawing(false);
            EditorUtils.SelectAndFrame(GetFileAsset().MakeParticles().gameObject); 
          }); 
          menu.ShowAsContext();
          Event.current.Use();
        }

        GUI.enabled = m_GuiEnabled;
        GUI.color = Color.white;
        
      }
      lastY = r_firstbuttons.yMax;
      // End first buttons

      // Tools
      // draw bg
      int tools = 3;
      if (DrawPrefs.Instance.m_ShowJumbleTool)  tools++;
      if (DrawPrefs.Instance.m_ShowGrabTool)    tools++;
      var r_tools = new Rect(r_sidebar.x,lastY,r_sidebar.width, Mathf.Lerp(55, 40, (tools-3) / 2f));
      {
        var rects = r_tools.GetEvenRectsHorizontal(tools, new Vector2(5, 5), true);
        int butsize = 50;
        if (ToolButton(rects[0], Tool.Color, "tool_ink.png", "Draw", butsize, butsize) || keyToolColor) {
          _queued_tool = Tool.Color;
        }
        if (ToolButton(rects[1], Tool.Eraser, "tool_eraser.png", "Erase", butsize, butsize) || keyToolEraser) {
          _queued_tool = Tool.Eraser;
        }
        if (ToolButton(rects[2], Tool.Fill, "tool_bucket.png", "Paint", butsize, butsize) || keyToolFill) {
          _queued_tool = Tool.Fill;
        }
        if (DrawPrefs.Instance.m_ShowJumbleTool &&
          ToolButton(rects[3], Tool.Jumble, "tool_jumble.png", "Jumble", butsize, butsize)) {
          _queued_tool = Tool.Jumble;
        }
        if (DrawPrefs.Instance.m_ShowGrabTool && ToolButton(rects[tools-1], Tool.Grab, "grab.png", "Grab", 20, 20))
          _queued_tool = Tool.Grab;
      }
      lastY = r_tools.yMax;
      // End Tool buttons

      // Tool options
    
      var r_toolsettings = new Rect(r_tools.x, lastY + 10, r_tools.width, 100);
      {
        float settingsY = 0;
        GUI.color = COLOR_SELECTEDTOOLBG;
        StaticResources.GetEditorSprite("bg_rounded.png").DrawFrame(0, new Rect(
          r_toolsettings.x,
          r_toolsettings.y - 8, 
          r_toolsettings.width, 
          r_toolsettings.height + 8
          ), true, false);
        GUI.color = Color.white;

        r_toolsettings = r_toolsettings.ScaleCentered(10,10);

        // Brush size
        if (m_CurrentTool == Tool.Color || m_CurrentTool == Tool.Eraser || m_CurrentTool == Tool.Jumble || m_CurrentTool == Tool.Grab) {
          var r_size = new Rect(r_toolsettings.x, r_toolsettings.y + settingsY, r_toolsettings.width, r_toolsettings.height * .4f);
          {
            GUI.color = new Color(.7f,.7f,.7f,1);
            StaticResources.GetEditorSprite("bg_rounded.png").DrawFrame(0, r_size, true, false);
            
            Rect rr = new Rect();
            float[] brushSizes = new float[]{ 
              DrawPrefs.Instance.m_BrushSize1,
              DrawPrefs.Instance.m_BrushSize2,
              DrawPrefs.Instance.m_BrushSize3,
              DrawPrefs.Instance.m_BrushSize4,
              DrawPrefs.Instance.m_BrushSize5
            };
            var rects_sizes = r_size.GetEvenRectsHorizontal(brushSizes.Length, new Vector2(0,0), true);
            for (int i = 0; i < brushSizes.Length; i++) {
              float ix = (float)(i + 1) / (float)(brushSizes.Length + 1);
              rr = rects_sizes[i];
              rr = rr.ScaleCentered(ix * .9f);
              
              GUI.color = new Color(1,1,1,0.4f);
              //GUI.DrawTexture(rects_sizes[i], StaticResources.GetTexture2D("brushpreview.png"));              
              GUI.color = Color.white;       
              
              bool hover = rects_sizes[i].Contains(Event.current.mousePosition);
              bool pressed = hover && Event.current.type == EventType.MouseDown;
              GUI.color = Mathf.Approximately (BrushSize, brushSizes [i]) ? COLOR_ACTIVE : Color.white;
              if (hover) {
                StaticResources.GetEditorSprite("size.png").DrawAnimated(rr);              
                RequestTooltip(BrushSizeTooltips[i], rects_sizes[i]);
              } else {
                StaticResources.GetEditorSprite("size.png").DrawFrame(0, rr);
              }
              if (pressed || keyCode == colorkeys [i])
                BrushSize = brushSizes [i];
              if (Mathf.Approximately (BrushSize, brushSizes [i])) {
                if (i < brushSizes.Length - 1 && keyCode == KeyCode.UpArrow) {
                  BrushSize = brushSizes[i+1];
                } else if (i > 0 && keyCode == KeyCode.DownArrow) {
                  BrushSize = brushSizes[i-1];
                }
              }
            }
            GUI.color = Color.white;
            
          }
          settingsY += r_size.height;
        } // End brush size

        // Additional settings
        var r_additionalsettings = new Rect(r_toolsettings.x, r_toolsettings.y + settingsY, 
          r_toolsettings.width, 
          r_toolsettings.height * .6f);
        var rects_additionalsettings = r_additionalsettings.GetEvenRectsHorizontal(3, new Vector2(5, 0), true);
        {
          int size = (int)r_additionalsettings.height;
          if (m_CurrentTool == Tool.Color) {
            GUI.color = m_SizeByVelocity ? COLOR_ACTIVE : Color.white;
            if (ImgButton (rects_additionalsettings[0], "color_sizebyvelocity.png", "buttonframe_rounded.png", true, "Ink brush"))
              m_SizeByVelocity = !m_SizeByVelocity;
            GUI.color = Color.white;

            GUI.color = m_AlphaMode != AlphaMode.None ? COLOR_ACTIVE : Color.white;
            int i = Mathf.Clamp(AlphaModes.IndexOf(m_AlphaMode), 0, AlphaModes.Count);
            if (ImgButton (rects_additionalsettings[1], "color_behindonly.png", "buttonframe_rounded.png", true, "Mask\n" + AlphaModeNames[i]))
              m_AlphaMode = AlphaModes[(int)Mathf.Repeat(i + 1, AlphaModes.Count)];
            GUI.color = Color.white;
          }
          // bucket tools
          if (m_CurrentTool == Tool.Fill) {
            int i = Mathf.Clamp(FloodTypes.IndexOf(m_FloodFillType), 0, FloodTypes.Count);
            GUI.color = m_FloodFillType != FloodFillOperation.Type.Normal ? COLOR_ACTIVE : Color.white;
            if (ImgButton (rects_additionalsettings[0], FillToolIcons[i], "buttonframe_rounded.png", true, "Effect\n" + FillToolNames[i]))
              m_FloodFillType = FloodTypes[(int)Mathf.Repeat(i + 1, FloodTypes.Count)];
            GUI.color = Color.white;
            GUI.color = m_FloodFillRainbow ? COLOR_ACTIVE : Color.white;
            if (ImgButton (rects_additionalsettings[1], "tool_bucket_rainbow.png", "buttonframe_rounded.png", true, "Rainbow"))
              m_FloodFillRainbow = !m_FloodFillRainbow;
            GUI.color = Color.white;
          }

          if (m_CurrentTool == Tool.Jumble) {
            GUI.color = m_JumbleALot ? COLOR_ACTIVE : Color.white;
            if (ImgButton(rects_additionalsettings[0], "tool_jumble_alot.png", "buttonframe_rounded.png", true, "Jumble a LOT"))
              m_JumbleALot = !m_JumbleALot;
            GUI.color = Color.white;
          }

          if (m_CurrentTool == Tool.Grab) {
              int i = Mathf.Clamp(GrabModes.IndexOf(m_Grab_Mode), 0, GrabModes.Count);
              GUI.color = m_Grab_Mode != GrabMode.Normal ? COLOR_ACTIVE : Color.white;
              if (ImgButton (rects_additionalsettings[0], GrabModeIcons[i], "buttonframe_rounded.png", true, "Effect\n" + GrabModeNames[i]))
                m_Grab_Mode = GrabModes[(int)Mathf.Repeat(i + 1, GrabModes.Count)];
              GUI.color = Color.white;
            }
        } // End tool additional settings

      } // end r_toolsettings
      lastY = r_toolsettings.yMax;
      lastY += 5;
    
      // Colors
      {
        m_CurrentPalette = Mathf.Clamp(m_CurrentPalette, 0, DrawPrefs.Instance.m_Palettes.Count - 1);
        var palette = DrawPrefs.Instance.m_Palettes[m_CurrentPalette];
        if (palette.colors == null) {
          palette.colors = new List<Color>();
          DrawPrefs.Instance.Save();
        }
        var currentcolor = m_BrushColor;

        // Swatches
        float swatchsize = 18f;
        float swatchboxmargin = 4;
        int swatchcolumns = 5;
        int maxcolors = 36;
        int swatchitems = Mathf.Clamp(palette.colors.Count, 1, maxcolors);
        int swatchlines = Mathf.Clamp(Mathf.CeilToInt((float)swatchitems / (float)swatchcolumns), 6, 6);
        var r_colors = new Rect(r_sidebar.x, lastY + 5, r_sidebar.width, (swatchsize - 0.5f) * swatchlines + swatchboxmargin * 2);
        r_colors.height = Mathf.Max(r_colors.height, 75);
        r_colors = r_colors.ScaleCentered(5,1);
        {
          GUI.color = new Color(1,1,1,0.3f);
          StaticResources.GetEditorSprite("bg_thinborder.png").DrawFrame(0, r_colors, true);
          GUI.color = Color.white;

          var r_picker = new Rect(r_colors.x + swatchboxmargin, r_colors.y + swatchboxmargin, 55, 32);
      		// #if UNITY_2018_1_OR_NEWER
          m_BrushColor = EditorGUI.ColorField (r_picker, new GUIContent(""), m_BrushColor, true, true, false);
          // #else
          // m_BrushColor = EditorGUI.ColorField (r_picker, new GUIContent(""), m_BrushColor, true, true, false, null);
          // #endif      
          GUI.enabled = m_GuiEnabled && DrawPrefs.Instance.m_Palettes.Count > 1;
          float swatch_r = 14;
          var r_nextpalette = new Rect(r_colors.xMax - swatchboxmargin - swatch_r, r_colors.yMin + swatchboxmargin, swatch_r, swatch_r);
          if (r_nextpalette.Contains(Event.current.mousePosition))
            RequestTooltip("Next Palette", r_nextpalette);
          var gs = new GUIStyle(GUI.skin.box);
          gs.padding = new RectOffset(0,0,0,0);
          gs.margin = new RectOffset(0,0,0,0);
          gs.fontSize = 10;
          if (GUI.Button(r_nextpalette, ">", gs)) {
            m_CurrentPalette = (int)Mathf.Repeat(m_CurrentPalette + 1, DrawPrefs.Instance.m_Palettes.Count);
          }
          var r_addpalette = r_nextpalette;
          r_addpalette.y += r_addpalette.height + 4;
          if (GUI.Button(r_addpalette, "+", gs)) {
            DrawPrefs.Instance.m_Palettes.Add(new DrawPrefs.ColorPalette());
            m_CurrentPalette = DrawPrefs.Instance.m_Palettes.Count - 1;
            DrawPrefs.Instance.Save();
          }
          GUI.enabled = m_GuiEnabled;
          
          // Swatches
          Color col;
          Color highlightedCol = m_BrushColor;
          int x = 0, y = 0;
          gs = new GUIStyle (GUI.skin.box);
          gs.fontSize = 10;
          {
            for(int i = 0; i < swatchlines * swatchcolumns; i++) {
              var butrect = new Rect (
                r_colors.xMax - swatchboxmargin - swatchcolumns * swatchsize - swatch_r + r_colors.x + (swatchsize-1) * x, 
                swatchboxmargin * 1 + r_colors.y + (swatchsize-1) * y, 
                swatchsize, swatchsize);
              
              GUI.color = new Color(1,1,1,0.2f);
              StaticResources.GetEditorSprite("bg_thinborder.png").DrawFrame(0, butrect, true);
              if (i >= palette.colors.Count && GUI.Button(butrect, "?") && !palette.colors.Contains(m_BrushColor))
              {
                palette.colors.Add(m_BrushColor);
                DrawPrefs.Instance.Save();
              }

              GUI.color = Color.white;
              if (i < palette.colors.Count) {
                col = palette.colors[i];
                GUI.color = col;
                
                if (StaticResources.GetEditorSprite("bg_thinborder.png").DrawAsButton(butrect, "", false, false, false, 0))
                  m_BrushColor = col;
                if (butrect.Contains(Event.current.mousePosition))
                  highlightedCol = col;

                GUI.color = Color.white;
              }
              x++;
              if (x >= swatchcolumns) {
                x = 0;
                y++;
              }
            }
          }

          // Color turtle
          var r_turtle = new Rect(r_picker.xMin, r_picker.yMax + swatchboxmargin * 2, r_picker.width, 32);
          {
            var sprite = StaticResources.GetTexture2D("colorpreview3.png");
            float aspect = (float)sprite.width / (float)sprite.height;
            float margin = 1.6f;
            float width =  r_turtle.height * aspect * margin;
            float height = r_turtle.height * margin;
            var r = new Rect(0,0,width,height).ScaleToFit(r_turtle).ScaleCentered(-8);
            // var turtlerect = new Rect(r.xMin + 130 - width, r.y + (r.height - height) * .5f, width, height);
            GUI.color = m_DarkCheckerboard ? Color.black : Color.white;
            GUI.DrawTexture(r, StaticResources.GetTexture2D("colorpreview3.png"), ScaleMode.StretchToFill, true);
            // GUI.color = Color.white;
            // GUI.DrawTexture(r, StaticResources.GetTexture2D("colorpreview2.png"), ScaleMode.StretchToFill, true);
            GUI.color = highlightedCol;// m_BrushColor;
            GUI.DrawTexture(r, StaticResources.GetTexture2D("colorpreview1.png"), ScaleMode.StretchToFill, true);
            GUI.color = Color.white;
          }

          // Dark checkerboard
          {
            // GUI.color = m_DarkCheckerboard ? COLOR_ACTIVE : Color.white;
            float size = 30;
            if (EditorSprite.DrawCompoundButton(new Rect(r_turtle.center.x - size * .5f, r_turtle.yMax + swatchboxmargin - 3,size,size), "buttonframe_rounded.png", "checkerboard.png", "Background\n" + (m_DarkCheckerboard ? "Dark" : "Light"))) {
              m_DarkCheckerboard = !m_DarkCheckerboard;
            }
            // GUI.color = Color.white;
          }
          
          
        } // End swatches
        lastY = r_colors.yMax;

      } // End colors
      
      // Tools
      var r_extratools = new Rect(r_sidebar.x, lastY + 10, r_sidebar.width, 60);
      {
        var rects_extratools = r_extratools.GetEvenRectsHorizontal(3, Vector2.zero, true);
        {
          GUI.color = OnionSkinOn ? COLOR_ACTIVE : Color.white;
          int i = Mathf.Clamp(OnionModes.IndexOf(m_OnionSkinMode), 0, OnionModes.Count);
          if (ImgButton (rects_extratools[0], "onionskin.png", "buttonframe_rounded.png", true, "Onion Skin\n" + OnionModeNames[i]))
            CycleOnionSkin();
          GUI.color = Color.white;
        }
        {
        GUI.color = m_SymmetryMode != SymmetryMode.None ? COLOR_ACTIVE : Color.white;
          int i = Mathf.Clamp(SymmetryModes.IndexOf(m_SymmetryMode), 0, SymmetryModes.Count);
          if (ImgButton (rects_extratools[1], SymmetryModeIcons[i], "buttonframe_rounded.png", true, "Pattern\n" + SymmetryModeNames[i]))
            m_SymmetryMode = SymmetryModes[(int)Mathf.Repeat(i + 1, SymmetryModes.Count)];
        }
        {
          GUI.color = PatternModeOn ? COLOR_ACTIVE : Color.white;
          int i = Mathf.Clamp(PatternModes.IndexOf(m_PatternMode), 0, PatternModes.Count);
          if (ImgButton (rects_extratools[2], PatternModeIcons[i], "buttonframe_rounded.png", true, "Pattern\n" + PatternModeNames[i])) {
            m_PatternMode = PatternModes[(int)Mathf.Repeat(i + 1, PatternModes.Count)];
            m_Zoom = PatternModeOn ? 2.25f : 1.0f;
            CenterCanvas(r_drawarea);
          }
          GUI.color = Color.white;
        }
        
        
      } // end extra tools 1
      lastY = r_extratools.yMax;
      
      
      var r_footer = new Rect(r_sidebar.x, lastY, r_sidebar.width, 45);
      {
        var rects_footer = r_footer.GetEvenRectsHorizontal(4, new Vector2(5, 0), true);
        if (DrawPrefs.Instance.m_SceneViewDrawing) {
          GUI.color = m_ShowSceneViewGizmo ? COLOR_ACTIVE : Color.white;
          if (ImgButton (rects_footer[0], "sceneviewdraw.png", "buttonframe_rounded.png", true, "SCENE PREVIEW\n" + (m_ShowSceneViewGizmo ? "on" : "off"))) {
            m_ShowSceneViewGizmo = !m_ShowSceneViewGizmo;
            DestroyPreview();
          }
          GUI.color = Color.white;
        }

        int i = Mathf.Clamp(ReferenceImageModes.IndexOf(m_RefImageMode), 0, ReferenceImageModes.Count);
        GUI.color = DrawPrefs.Instance.m_ReferenceImage && m_RefImageMode != ReferenceImageMode.Off ? COLOR_ACTIVE : Color.white;
        if (ImgButton (rects_footer[1], "refimage.png", "buttonframe_rounded.png", true, 
          "REFERENCE IMAGE\n" + "(Alt + click to select an image)\n" + ReferenceImageNames[i])) {
          if (!DrawPrefs.Instance.m_ReferenceImage || Event.current.alt) {
            EditorGUIUtility.ShowObjectPicker<Texture2D>(DrawPrefs.Instance.m_ReferenceImage, false, "", 0);
            m_ReferenceImageObjectPickerID = EditorGUIUtility.GetObjectPickerControlID();
          } else if (DrawPrefs.Instance.m_ReferenceImage) {
            m_RefImageMode = ReferenceImageModes[(int)Mathf.Repeat(i + 1, ReferenceImageModes.Count)];
          }
        }
        GUI.color = Color.white;

        GUI.color = Color.white;
        if (ImgButton (rects_footer[2], "flipx.png", "buttonframe_rounded.png", true, (Event.current.alt ? "Flip\n(All Frames)" : "Flip\nHold Alt to flip All Frames")))
            Flip (Event.current.alt, false);

        if (ImgButton (rects_footer[3], "settings.png", "buttonframe_rounded.png", true, "Settings")) {
          Selection.objects = new Object[]{DrawPrefs.Instance};
          EditorGUIUtility.PingObject(DrawPrefs.Instance);
        }


        // if (ImgButton (rects_footer[1], "settings.png", "buttonframe_rounded.png", true, "test")) {
        //   var size = Vector2.one * 100;
        //   DrawUtils.DebugBoundLines(new Line(new Vector2(50,50), new Vector2(120,50)), size);
        //   DrawUtils.DebugBoundLines(new Line(new Vector2(-50,50), new Vector2(50,50)), size);
        //   DrawUtils.DebugBoundLines(new Line(new Vector2(50,50), new Vector2(-50,50)), size);
        //   DrawUtils.DebugBoundLines(new Line(new Vector2(50,50), new Vector2(-120,50)), size);
        // }
      } // end footer
      lastY = r_footer.yMax;

      
      GUI.backgroundColor = Color.white;
      
    } // End sidebar (Left side)

    #if DEBUG_VIEW
    GUI.Label(new Rect(r_drawarea.x, r_drawarea.y + r_drawarea.height - 12 - 12 * debugText.Count, 800, 400), string.Join("\n",debugText.ToArray()));
    #endif

    // popups//
    if (!string.IsNullOrEmpty(m_CurrentPopup)) {
      m_GuiEnabled = true;
      GUI.enabled = m_GuiEnabled;

      var window = StaticResources.GetWindow(m_CurrentPopup);
      if (window.Elements.Count > 0 && window.AnimationTime > 0){
        window.Position = new Vector2(r_whole.center.x, r_whole.center.y);
        window.Scale = (r_whole.height * .8f) / window.bounds.size.y;
        //window.Scale *= DrawUtils.EaseElasticOut(Mathf.Clamp01(window.AnimationTime / 2.0f));

        // Draw an animated background
        GUI.color = new Color(1,1,1,Mathf.Lerp(0.8f, 0.0f, 1 - Mathf.Clamp01(window.AnimationTime / .4f)));
        GUI.DrawTexture(r_whole, StaticResources.GetTexture2D("bg_main.png"));
        var bg = StaticResources.GetTexture2D("bg_popupvisible.png");
        var oldMatrix = GUI.matrix;
        GUIUtility.RotateAroundPivot(window.AnimationTime * 7, window.Position);
        GUIUtility.ScaleAroundPivot(Vector2.one * 1.5f, window.Position);
        GUI.color = new Color(1,1,1,Mathf.Lerp(0.2f, 0.0f, 1 - Mathf.Clamp01(window.AnimationTime / .4f)));
        GUI.DrawTexture(r_whole, bg, ScaleMode.ScaleToFit, true);
        //GUI.DrawTextureWithTexCoords(r_whole, bg, new Rect(-t, 0, 3.0f * ((float)r_whole.width / (float)r_whole.height), 3.0f), false);
        GUI.matrix = oldMatrix;
        GUIUtility.RotateAroundPivot(-45 -window.AnimationTime * 6, window.Position);
        GUIUtility.ScaleAroundPivot(Vector2.one * 1.7f, window.Position);
        GUI.color = new Color(1,1,1,Mathf.Lerp(0.2f, 0.0f, 1 - Mathf.Clamp01(window.AnimationTime / .4f)));
        GUI.DrawTexture(r_whole, bg, ScaleMode.ScaleToFit, true);
        //GUI.DrawTextureWithTexCoords(r_whole, bg, new Rect(-t, 0, 3.0f * ((float)r_whole.width / (float)r_whole.height), 3.0f), false);
        GUI.matrix = oldMatrix;
        GUI.color = Color.white;
        
        // Animate the window
        GUIUtility.RotateAroundPivot(Mathf.Lerp(Mathf.Sin(window.AnimationTime * 6) * 25, 0, Mathf.Clamp01(window.AnimationTime / .5f)), window.Position);      
        GUIUtility.ScaleAroundPivot(Vector2.one * (1-Mathf.Pow(1-Mathf.Clamp01(window.AnimationTime / .5f), 3)), window.Position);

        // Draw everything that's not interactive or animated
        window.DrawStaticElements();

        // new 
        bool chunky = m_FilterMode == FilterMode.Point;
        if (m_CurrentPopup == "New") {
          m_NewName = GUI.TextField(window.GetElement("text_name").GetRect(window), m_NewName, 999, StaticResources.style_nameTextfield);
          GUI.TextField(window.GetElement("text_version").GetRect(window), "version " + DrawPrefs.VERSION, 999, StaticResources.style_versionTextfield);

          if (window.DrawElementAsButton("button_character", "Tall\nGood for characters", -1, false, chunky)) {
            New(chunky ? DrawPrefs.Instance.m_Preset_Character_Chunky : DrawPrefs.Instance.m_Preset_Character_Smooth);
          }
          if (window.DrawElementAsButton("button_landscape", "Wide\nFor backgrounds and landscapes", -1, false, chunky)) {
            New(chunky ? DrawPrefs.Instance.m_Preset_Background_Chunky : DrawPrefs.Instance.m_Preset_Background_Smooth);
          }
          if (window.DrawElementAsButton("button_item", "Square\nFor details, items and particles", -1, false, chunky)) {
            New(chunky ? DrawPrefs.Instance.m_Preset_Square_Chunky : DrawPrefs.Instance.m_Preset_Square_Smooth);
          }
          if (window.DrawElementAsButton("button_ui", "UI Frame\nSliced in 9 parts for you already", -1, false, chunky)) {
            New(chunky ? DrawPrefs.Instance.m_Preset_UI_Chunky : DrawPrefs.Instance.m_Preset_UI_Smooth);
          }//
          if (window.DrawElementAsButton("button_card", "Playing Card", -1, false, chunky)) {
            New(chunky ? DrawPrefs.Instance.m_Preset_PlayingCard_Chunky : DrawPrefs.Instance.m_Preset_PlayingCard_Smooth);
          }//
          if (window.DrawElementAsButton("button_quality", "" + (chunky ? "Chunky\nSmall + Point filtering" : "Smooth\nSlower but more detailed"), chunky ? 0 : 1, chunky)) {
            m_FilterMode = chunky ? FilterMode.Trilinear : FilterMode.Point;
          }
          if (chunky) 
            window.DrawElement("quality_chunky");
          else 
            window.DrawElement("quality_smooth");
          
          if (window.DrawElementAsButton("button_custom", "CUSTOMATIC 2000\nInput your coordinates", -1, true, chunky)) {
            // new custom
            OpenPopup("NewCustom");
          }
          if (window.DrawElementAsCompoundButton("button_close")) {
            ClosePopup();
          }//
        } else if (m_CurrentPopup == "NewCustom") {
          window.DrawElement("beepboop1", true);
          window.DrawElement("beepboop2", true);
          if (window.DrawElementAsButton("button_quality", "Filter Mode\n" + (chunky ? "Point" : "Trilinear"), chunky ? 0 : 1)) {
            m_FilterMode = m_FilterMode == FilterMode.Point ? FilterMode.Trilinear : FilterMode.Point;
          }

          var gs = StaticResources.style_resolutionTextfield;
          m_TextureWidth = EditorGUI.IntField(window.GetElement("text_width").GetRect(window), m_TextureWidth, gs);
          var sn = GUI.TextField(window.GetElement("text_width").GetRect(window), m_TextureWidth.ToString(), 4, gs);
          int.TryParse(sn, out m_TextureWidth);
          m_TextureWidth = Mathf.Clamp(m_TextureWidth, 1, 8192);

          sn = GUI.TextField(window.GetElement("text_height").GetRect(window), m_TextureHeight.ToString(), 4, gs);
          int.TryParse(sn, out m_TextureHeight);
          m_TextureHeight = Mathf.Clamp(m_TextureHeight, 1, 8192);

          // Custom resolution dialog
          if (window.DrawElementAsCompoundButton("button_makecustom", "", true)) {
            if ((m_TextureWidth < 2048 && m_TextureHeight < 2048) || 
              EditorUtility.DisplayDialog("Make huge new animation?", "That's.. umm.. a REALLY big animation... are you sure?", "YES I NEED PIXELS", "Cancel")) {
              ClosePopup();
              m_SpriteBorder = Vector4.zero; 
              New(new DrawPrefs.NewImageParams(m_TextureWidth, m_TextureHeight, m_FramesPerSecond, m_FilterMode, m_SpriteBorder));
            }
          }//
          if (window.DrawElementAsCompoundButton("button_close")) {
            OpenPopup("New");
          }//
        }
        GUI.matrix = oldMatrix;
    }
      
        /* 
      var gs = new GUIStyle();
      GUI.Button(r_whole, "", gs);

      var r_popup = new Rect(0,0,Mathf.Max(400, Mathf.Min(800, r_whole.width * .8f)), Mathf.Min(600, r_whole.height * .8f));
      r_popup.center = r_whole.center;
      GUI.color = new Color(0,0,0,0.4f);
      StaticResources.GetEditorSprite("bg_popup.png").DrawFrame(0, new Rect(r_popup.x + 5, r_popup.y + 4, r_popup.width, r_popup.height), true);
      GUI.color = Color.white;
      StaticResources.GetEditorSprite("bg_popup.png").DrawFrame(0, r_popup, true);

      StaticResources.GetEditorSprite("text_new.png").DrawFrame(0, new Rect(r_popup.center.x, r_popup.yMin - 20, r_popup.width, 90), false, true, true);
      
      GUI.color = COLOR_SELECTEDTOOL;
      if (EditorSprite.DrawCompoundButton(new Rect(r_popup.xMax - 120, r_popup.yMin + 10, 70, 70), "buttonframe_big.png", "close.png", 1)) {
        m_Popup_New = false;
      }
      GUI.color = Color.white;

      StaticResources.GetEditorSprite("new_character.png").DrawAsButton(new Rect(r_popup.center.x - 400, r_popup.center.y, 200, 200), false, true, true);
      StaticResources.GetEditorSprite("new_landscape.png").DrawAsButton(new Rect(r_popup.center.x - 200, r_popup.center.y, 200, 200), false, true, true);
      StaticResources.GetEditorSprite("new_detail.png").DrawAsButton(new Rect(r_popup.center.x, r_popup.center.y, 200, 200), false, true, true);
      StaticResources.GetEditorSprite("new_ui.png").DrawAsButton(new Rect(r_popup.center.x + 200, r_popup.center.y, 200, 200), false, true, true);
      StaticResources.GetEditorSprite("new_custom.png").DrawAsButton(new Rect(r_popup.center.x + 400, r_popup.center.y, 200, 200), false, true, true);

      EditorSprite.DrawCompoundButton(new Rect(r_popup.center.x - 200, r_popup.yMin + 50, 300, 200), "buttonframe_big.png", "new_character.png", 2);
      EditorSprite.DrawCompoundButton(new Rect(r_popup.center.x + 200, r_popup.yMin + 50, 300, 200), "buttonframe_big.png", "new_character.png", 2);
      */

    } // end new popup
    

    // Tooltip
    DrawTooltip(r_whole);
    m_TooltipText = "";
    // End whole thing

    //// Drag and drop action to load
    {
      var firstDraggedObject = DragAndDrop.objectReferences.Length > 0 ? DragAndDrop.objectReferences [0] : null;
      if (firstDraggedObject != null) {
        string path = null;

        var assetPath = AssetDatabase.GetAssetPath (firstDraggedObject);
        if (string.IsNullOrEmpty (assetPath)) {
          // Scene game object
          var animator = (firstDraggedObject as GameObject).GetComponent<DoodleAnimator> ();
          if (animator && animator.File) 
            path = AssetDatabase.GetAssetPath (animator.File);

        } else {
          if(DrawUtils.IsImageAsset(assetPath)) {
            if (
              AssetDatabase.LoadAssetAtPath<DoodleAnimationFile> (assetPath) != null ||
              AssetDatabase.LoadAssetAtPath<Texture2D> (assetPath) != null || 
              AssetDatabase.LoadAllAssetsAtPath (assetPath).OfType<Sprite> ().ToArray ().Length > 0
            )
              path = assetPath;
          } else {
            // Extension unsupported
          }
        }

        DragAndDrop.visualMode = DragAndDropVisualMode.Link;

        bool inside = windowrect.Contains (Event.current.mousePosition);
        bool valid = !string.IsNullOrEmpty (path);
      
        GUI.color = new Color (1, 1, 1, 0.9f);
        GUI.Box (windowrect, "");
        GUI.color = Color.white;
        GUI.DrawTexture (windowrect, StaticResources.GetTexture2D("load_bg.png"), ScaleMode.StretchToFill);

        StaticResources.GetEditorSprite("load.png").DrawFrame(
          inside && valid ? 1 : (inside && !valid ? 2 : 0), 
          new Rect (windowrect.center.x - 150, windowrect.center.y - 150, 300, 300)
        );
      
        if (inside && !valid)
          DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
      
        if (inside) {
          if (Event.current.type == EventType.DragPerform) {
            // Load
            if (!string.IsNullOrEmpty (path)) {
              Load (path);

              if (firstDraggedObject is GameObject) {
                var animator = (firstDraggedObject as GameObject).GetComponent<DoodleAnimator> ();
                if (animator) {
                  m_PlaybackMode = animator.m_Settings.customPlaybackMode;
                  if (m_PlaybackMode == PlaybackMode.Once) m_PlaybackMode = PlaybackMode.Loop; // playing once is boring
                  m_FramesPerSecond = (int)animator.m_Settings.customFramesPerSecond;
                }
              }
            }
          }
        }

        m_NeedsUIRedraw = true;
      }
    } //// End drag and drop action

    #if DEBUG_VIEW
    if (Event.current.type == EventType.Repaint && m_GrabImage != null) {
      GUI.DrawTexture(new Rect(0, 0, m_GrabImage.width, m_GrabImage.height), m_GrabImage);
    }
    #endif

    if (Event.current.type == EventType.MouseUp)
      _drawCounter = 0;

    // Update GUI
    if (EditorGUI.EndChangeCheck () || keyCode != KeyCode.None) {
      m_NeedsUIRedraw = true;
    }

    GUI.skin = null;

    if (Event.current.type != EventType.Layout)
      m_VelocityDelta = Mathf.Clamp01(Mathf.Lerp (m_VelocityDelta, 0, 1.8f * Time.deltaTime * (1 + BrushSize * 2.5f)));
    if (Event.current.type == EventType.MouseDown)
      m_VelocityDelta = 0;
    if (Event.current.type == EventType.MouseDrag)
      m_VelocityDelta = Mathf.Clamp01(m_VelocityDelta + (Event.current.mousePosition - m_LastMousePosition).magnitude * Time.deltaTime * 0.005f);
    // Apply changed variables
    if (Event.current.type == EventType.Repaint) {
      m_CurrentTool = _queued_tool;
    }
    #if DEBUG_VIEW
    debugText.Add(m_LastMousePosition.ToString());
    #endif

    if (Event.current.type != EventType.Layout)
      m_LastMousePosition = Event.current.mousePosition;
  }

  void DrawCanvas(Rect rect, DrawWindowKeyframe keyframe, int totalFrameLength) {
    _singleRect.Clear();
    _singleRect.Add(rect);
    DrawCanvas(_singleRect, keyframe, totalFrameLength);
  }
  void DrawCanvas(List<Rect> rects, DrawWindowKeyframe keyframe, int totalFrameLength, bool drawSpriteBorder = true) {
    float aspect = keyframe.width / (float)keyframe.height;

    // Draw outside border
    GUI.color = m_RecordingModeOn ? Color.red : Color.black;
    foreach(var rect in rects) {
      Rect r_border = rect.ScaleCentered(-20,-20);
      if (m_Playing && totalFrameLength > 1)
        StaticResources.GetEditorSprite("bg_drawareaborder.png").DrawAnimated(r_border, true);
      else
        StaticResources.GetEditorSprite("bg_drawareaborder.png").DrawFrame(0, r_border, true);
    }
    GUI.color = Color.white;

    foreach(var rect in rects) {
      // draw frame border
      if (rects.Count > 1) {
        Rect r_border = rect.ScaleCentered(-20,-20);
        GUI.color = new Color(0,0,0,.15f);
        StaticResources.GetEditorSprite("bg_drawareaborder.png").DrawFrame(0, r_border, true);
        GUI.color = Color.white;
      }

      float gridSize = Mathf.CeilToInt(rect.width / m_Zoom / 20f);
      if (m_Zoom > 2) gridSize = m_TextureWidth / 10f;
      GUI.color = m_DarkCheckerboard ? COLOR_DARK_CHECKERBOARD : Color.white;
      GUI.DrawTextureWithTexCoords(rect, StaticResources.GetTexture2D("transparency.png"), new Rect(0, 0, gridSize * aspect, gridSize));
      GUI.color = Color.white;
    }

    foreach(var rect in rects) {

      if (DrawPrefs.Instance.m_ReferenceImage && m_RefImageMode != ReferenceImageMode.Off) {
        GUI.color = new Color(1,1,1,m_RefImageMode == ReferenceImageMode.Transparent ? 0.5f : 1);
        GUI.DrawTexture(rect, DrawPrefs.Instance.m_ReferenceImage, ScaleMode.ScaleToFit);
        GUI.color = Color.white;
      }

      if (DrawOnAllKeyframes) {
        // float[] alphas = new float[]{0.05f, 0.1f, 0.2f, 0.3f, 0.4f, 0.6f};
        for(int i = 0; i < m_KeyFrames.Count; i++) {
          var c = Color.white;
          if (i != m_PlayheadPosition)
            c.a = 0.2f;
            //c.a = alphas[(int)Mathf.Lerp(1, alphas.Length - 1, 1 - Mathf.Clamp01(Mathf.Abs(i - m_PlayheadPosition) / 5))];
          
          // c.a = i != m_PlayheadPosition ? 0.05f : 1f;
          GUI.color = c;
          GUI.DrawTexture (rect, m_KeyFrames[i].Texture.texture, ScaleMode.StretchToFill, true);
        }
        GUI.color = Color.white;
      } else {
        bool drawOnionSkin = !m_Playing && !m_DraggingTimeline && OnionSkinOn && m_KeyFrames.Count > 1;
        // Onion Skin prev frames
        if (drawOnionSkin) {
          if (m_KeyFrames.Count > 2) {
            GUI.color = new Color (1, 1, 1, 0.05f);
            var prevprevFrame = GetKeyFrameAt (m_PlayheadPosition - 2, true);
            if (prevprevFrame != null) {
              GUI.DrawTexture (rect, prevprevFrame.Texture.texture, ScaleMode.StretchToFill, true);
            }
          }
          GUI.color = new Color (1, 1, 1, 0.4f);
          var prevFrame = GetKeyFrameAt (m_PlayheadPosition - 1, true);
          if (prevFrame != null) {
            GUI.DrawTexture (rect, prevFrame.Texture.texture, ScaleMode.StretchToFill, true);
          }
          GUI.color = Color.white;
        } // end Onion Skin

        // It's all for this
        GUI.color = Color.white;
        GUI.DrawTexture (rect, keyframe.Texture.texture, ScaleMode.StretchToFill, true);

        // Onion Skin next frame
        if (drawOnionSkin) {
          GUI.color = new Color (1, 1, 1, 0.2f);
          var nextFrame = GetKeyFrameAt (m_PlayheadPosition < m_KeyFrames.Count - 1 ? m_PlayheadPosition + 1 : 0);
          if (nextFrame != null) {
            GUI.DrawTexture (rect, nextFrame.Texture.texture, ScaleMode.StretchToFill, true);
          }
          GUI.color = Color.white;
        }
      }

      

      // Draw sprite border values (used for slicing)
      if (drawSpriteBorder && m_SpriteBorder != Vector4.zero) {
        float w = rect.width;
        float h = rect.height;
        var c = new Color(0,0,0,.25f);
        if (m_DarkCheckerboard)
          c = new Color(1,1,1,.25f);
        EditorUtils.DrawSpriteBorder(rect, c, keyframe.width, keyframe.height, m_SpriteBorder);
      }
    }
  }

  void DrawCursor(Rect rect, DrawWindowKeyframe keyframe, Vector2? offset = null) {
    // Draw cursor
    if (m_InputRectWindowDrawArea.MouseOver && m_CurrentTool != Tool.Fill) {
      float size = BrushSize * keyframe.height; // DrawCircle radius
      size *= rect.height / keyframe.height;
      var r_brush = new Rect (Vector2.zero, Vector2.one * size);
      r_brush.center = Event.current.mousePosition + offset.GetValueOrDefault();
      GUI.color = m_BrushColor;//new Color(0,0,0,0.5f); 
      var brushTexture = StaticResources.GetTexture2D("brushpreview.psd");
      if (m_CurrentTool == Tool.Color && DrawPrefs.Instance.m_CustomBrush)
        brushTexture = DrawPrefs.Instance.m_CustomBrush;
      GUI.DrawTexture (r_brush, brushTexture, ScaleMode.StretchToFill, true);
      if (m_CurrentTool == Tool.Replaser) {
        r_brush.width *= m_Replaser_BorderSize;
        r_brush.height *= m_Replaser_BorderSize;
        GUI.color = new Color(1,1,1,0.5f);
        GUI.DrawTexture (r_brush, brushTexture, ScaleMode.StretchToFill, true);
        GUI.color = Color.white;
      }
      if (m_SymmetryMode != SymmetryMode.None) {
        UpdateReflectedPoints (_v2list, r_brush.center.x, r_brush.center.y, rect.center.x, rect.center.y, m_SymmetryMode);
        foreach (var p in _v2list) {
          r_brush.center = p;
          GUI.DrawTexture (r_brush, brushTexture, ScaleMode.StretchToFill, true);
        }
      }
  
      GUI.color = Color.white;
    }
  }

  void DrawSpriteBorderMarkers(Rect rect, DrawWindowKeyframe keyframe, ref bool drawAllowed) {
    // Sprite border
    Rect r_spriteborder = rect.ScaleCentered(-40,-40);
    m_InputRectWindowSpriteBorder.Update(r_spriteborder, "m_InputRectWindowSpriteBorder");
    {
      Vector4 ratios = new Vector4(
        m_SpriteBorder.x / keyframe.width, // Left
        1 - m_SpriteBorder.y / keyframe.height, // Bottom
        1 - m_SpriteBorder.z / keyframe.width, // Right
        m_SpriteBorder.w / keyframe.height // Top
      );
      float w = 18;
      float hw = w * .5f;
      Rect r_l = new Rect(rect.x + rect.width * ratios.x - hw, rect.yMax, w, w);
      Rect r_b = new Rect(rect.xMax, rect.y + rect.height * ratios.y - hw, w, w);
      Rect r_r = new Rect(rect.x + rect.width * ratios.z - hw, rect.yMax, w, w);
      Rect r_t = new Rect(rect.xMax, rect.y + rect.height * ratios.w - hw, w, w);
      var tex = StaticResources.GetTexture2D("border_grab.png");
      var prevMatrix = GUI.matrix;
      // Marker textures
      GUIUtility.RotateAroundPivot(180, r_l.center);
      GUI.DrawTexture(r_l, tex, ScaleMode.ScaleToFit);
      GUI.matrix = prevMatrix;
      GUIUtility.RotateAroundPivot(180, r_r.center);
      GUI.DrawTexture(r_r, tex, ScaleMode.ScaleToFit);
      GUI.matrix = prevMatrix;
      GUIUtility.RotateAroundPivot(90, r_b.center);
      GUI.DrawTexture(r_b, tex, ScaleMode.ScaleToFit);
      GUI.matrix = prevMatrix;
      GUIUtility.RotateAroundPivot(90, r_t.center);
      GUI.DrawTexture(r_t, tex, ScaleMode.ScaleToFit);
      GUI.matrix = prevMatrix;
      GUI.color = Color.white;
      // Tooltips
      // if (Event.current.type == EventType.Repaint) {
      //   var tooltipr = new Rect(r_whole.xMin, r_whole.yMax, 0,0);
      //   if (r_l.Contains(Event.current.mousePosition))
      //     RequestTooltip("Left border\nHold shift for symmetry", tooltipr);
      //   else if (r_b.Contains(Event.current.mousePosition))
      //     RequestTooltip("Bottom border\nHold shift for symmetry", tooltipr);
      //   else if (r_r.Contains(Event.current.mousePosition))
      //     RequestTooltip("Right border\nHold shift for symmetry", tooltipr);
      //   else if (r_t.Contains(Event.current.mousePosition))
      //     RequestTooltip("Top border\nHold shift for symmetry", tooltipr);
      // }
      // Input
      if (Event.current.isMouse) {
        // On Press
        if (m_DraggingBorder == -1 && m_InputRectWindowSpriteBorder.State == InputRect.MouseState.StartedPressing) {
          if (r_l.Contains(Event.current.mousePosition))
            m_DraggingBorder = 0;
          else if (r_b.Contains(Event.current.mousePosition))
            m_DraggingBorder = 1;
          else if (r_r.Contains(Event.current.mousePosition))
            m_DraggingBorder = 2;
          else if (r_t.Contains(Event.current.mousePosition))
            m_DraggingBorder = 3;
          if (m_DraggingBorder != -1)
            Event.current.Use();
        } else if (!m_InputRectWindowDrawArea.MousePressing) {
          m_DraggingBorder = -1;
        }
        // While dragging
        if (m_DraggingBorder >= 0) {
          if (m_DraggingBorder == 0) {
            float l = (Event.current.mousePosition.x - rect.x) / rect.width;
            m_SpriteBorder.x = Mathf.Clamp01(l) * keyframe.width;
            m_SpriteBorder.z = Mathf.Clamp(m_SpriteBorder.z, 0, Mathf.Abs(keyframe.width - m_SpriteBorder.x));
            if (Event.current.shift)
              m_SpriteBorder = Vector4.one * Mathf.Clamp(m_SpriteBorder.x, 0, keyframe.width * .5f);
          } else if (m_DraggingBorder == 1) {
            float l = 1 - (Event.current.mousePosition.y - rect.y) / rect.height;
            m_SpriteBorder.y = Mathf.Clamp01(l) * keyframe.height;
            m_SpriteBorder.w = Mathf.Clamp(m_SpriteBorder.w, 0, Mathf.Abs(keyframe.height - m_SpriteBorder.y));
            if (Event.current.shift) 
              m_SpriteBorder = Vector4.one * Mathf.Clamp(m_SpriteBorder.y, 0, keyframe.width * .5f);
          } else if (m_DraggingBorder == 2) {
            float l = 1 - (Event.current.mousePosition.x - rect.x) / rect.width;
            m_SpriteBorder.z = Mathf.Clamp01(l) * keyframe.width;
            m_SpriteBorder.x = Mathf.Clamp(m_SpriteBorder.x, 0, Mathf.Abs(keyframe.width - m_SpriteBorder.z));
            if (Event.current.shift) 
              m_SpriteBorder = Vector4.one * Mathf.Clamp(m_SpriteBorder.z, 0, keyframe.width * .5f);
          } else if (m_DraggingBorder == 3) {
            float l = (Event.current.mousePosition.y - rect.y) / rect.height;
            m_SpriteBorder.w = Mathf.Clamp01(l) * keyframe.height;
            m_SpriteBorder.y = Mathf.Clamp(m_SpriteBorder.y, 0, Mathf.Abs(keyframe.height - m_SpriteBorder.w));
            if (Event.current.shift) 
              m_SpriteBorder = Vector4.one * Mathf.Clamp(m_SpriteBorder.w, 0, keyframe.width * .5f);
          }
        }
      }
      if (m_DraggingBorder >= 0)
        drawAllowed = false;
    }
  }

  void CenterCanvas(Rect r_drawarea) {
    var diff = new Vector2 (r_drawarea.width, r_drawarea.height) * m_Zoom - new Vector2 (r_drawarea.width, r_drawarea.height);
    m_ScrollPos = new Vector2(
      diff.x * .5f,
      diff.y * .5f
    );
  }

  // Returns if drawing is allowed 
  bool ProcessInput(SerializableTexture2D layer, InputRect input) {
    if (Event.current.type == EventType.Layout)
      return false;

    bool draw = false;
    
    switch(input.State) {
      case InputRect.MouseState.StartedPressing:
        OnStartedDrawing(layer);
      break;
      case InputRect.MouseState.StoppedPressing:
      case InputRect.MouseState.Idle:
        if (m_Drawing)
          OnStoppedDrawing();
      break;
    }
    draw = m_Drawing && input.MousePressing;
    
    if (DrawPrefs.CHECK_DRAW_FREQUENCY && EditorApplication.timeSinceStartup < _drawCounter)
      return false;

    draw = draw && (Event.current.isMouse || Event.current.type == EventType.Repaint);
    
    return draw;
  }

  void OnStartedDrawing(SerializableTexture2D layer) {
    m_Drawing = true;
    //Debug.Log("mouse hit on " + Event.current.mousePosition + ", screen " + Screen.width + ", " + Screen.height);
    if (DrawOnAllKeyframes) {
      foreach(var k in m_KeyFrames)
        Undo.RegisterCompleteObjectUndo (k.Texture.texture, "Draw");
        
    } else {
      Undo.RegisterCompleteObjectUndo (layer.texture, "Draw");
    }
    m_LastDrawnPoint = null;
    m_VelocityDelta = 0;
    // REC
    if (m_RecordingModeOn && m_CurrentTool != Tool.Fill) {
      m_MakeNewFrames = true;
      if (!m_Playing) SetPlayback(true);
      m_PlayTime = Mathf.Repeat (m_PlayTime, GetAnimationLength ());
      m_LastPlayEditorTime = EditorApplication.timeSinceStartup;
    }
  }
  void OnStoppedDrawing() {
    //Debug.Log("mouse hit on " + Event.current.mousePosition + ", screen " + Screen.width + ", " + Screen.height);
    m_LastDrawnPoint = null;
    m_RecordingModeOn = m_MakeNewFrames = false;
    if (!Event.current.shift && m_FloodFillOps.Count > 0 && m_FloodFillType != FloodFillOperation.Type.Normal) {
      m_FloodFillOps.Clear ();
    }
    if (m_GrabImage != null) {
      DestroyImmediate(m_GrabImage);
      m_GrabImage = null;
    }
    m_Drawing = false;
  }

  void OnDrawPointHit(Vector2 texturePos, DrawWindowKeyframe keyframe) {
    m_NeedsUIRedraw = true;
    _drawCounter = EditorApplication.timeSinceStartup + DrawPrefs.DRAW_FREQUENCY;
    
    UpdateReflectedPoints (_v2list, texturePos.x, texturePos.y, (float)keyframe.width * .5f, (float)keyframe.height * .5f, m_SymmetryMode);
    
    var lastPoint = m_LastDrawnPoint != null ? m_LastDrawnPoint.GetValueOrDefault() : texturePos;
    DrawLine(keyframe, lastPoint, texturePos);
  }

  void DrawLine(DrawWindowKeyframe keyframe, Vector2 from, Vector2 to, bool setLastDrawnPoint = true) {
    var brushSize = m_SizeByVelocity ? m_VelocityDelta * 2 : 1.0f;
    _singleKeyframe.Clear();
    if (!DrawOnAllKeyframes) _singleKeyframe.Add(keyframe);
    foreach(DrawWindowKeyframe k in DrawOnAllKeyframes ? m_KeyFrames : _singleKeyframe) {
      if (m_CurrentTool == Tool.Color) {
        LineTo (k.Texture, from.x, from.y, to.x, to.y, m_BrushColor, brushSize);
        k.Texture.Apply ();
      } else if (m_CurrentTool == Tool.Eraser) {
        LineTo (k.Texture, from.x, from.y, to.x, to.y, DrawUtils.TRANSPARENCY_COLOR);            
        k.Texture.Apply ();
      } else if (m_CurrentTool == Tool.Fill && Event.current.type == EventType.MouseDown) {
        Fill (k.Texture, to.x, to.y, m_BrushColor);
        if (m_FloodFillType != FloodFillOperation.Type.Normal) {
          foreach (var p in _v2list)
            Fill (k.Texture, p.x, p.y, m_BrushColor);
        }
        //k.Texture.Apply();
      } else if (m_CurrentTool == Tool.Replaser) {
        LineTo (k.Texture, from.x, from.y, to.x, to.y, new Color(0,0,0,0), m_Replaser_BorderSize); // TODO: integrate this into DrawAt to avoid double drawing
        LineTo (k.Texture, from.x, from.y, to.x, to.y, m_BrushColor, brushSize);
        k.Texture.Apply ();
      } else if (m_CurrentTool == Tool.Jumble) {
        JumbleAt(k.Texture, to.x, to.y, brushSize);
        foreach (var p in _v2list)
          JumbleAt(k.Texture, p.x, p.y, brushSize);
        k.Texture.Apply();
      } else if (m_CurrentTool == Tool.Grab) {
        Grab(k.Texture, from.x, from.y, to.x, to.y, brushSize);
        k.Texture.Apply();
      }
    }
    if (setLastDrawnPoint)
      m_LastDrawnPoint = new Vector2(to.x, to.y);
    
  }

  #endregion

  DrawWindowKeyframe AddKeyFrame(bool AddLast = false) {
    var ki = new DrawWindowKeyframe(1);
    ki.SetTexture(EmptyFrame);
    //Undo.RegisterCreatedObjectUndo(ki, "New keyframe"); // TODO: undo when creating a new keyframe
    m_UnsavedChangesPresent = true;
    if (m_KeyFrames.Count <= 1 || AddLast) {
      m_KeyFrames.Add (ki);
    } else {
      int i = m_KeyFrames.IndexOf (GetCurrentKeyFrame ()) + 1;
      m_KeyFrames.Insert (i, ki);
    }
    return ki;
  }

  void RemoveKeyFrame(DrawWindowKeyframe keyframe) {
    for(int i = 0; i < m_FloodFillOps.Count; i++) {
      if (keyframe.Texture == m_FloodFillOps[i].Target) {
        m_FloodFillOps.RemoveAt(i);
        i--;
      }
    }
    int ki = m_KeyFrames.IndexOf (keyframe);
    //Undo.RegisterCompleteObjectUndo(this, "Remove Keyframe");
    m_KeyFrames.Remove (keyframe); 
    keyframe.OnDestroy();
    if (m_KeyFrames.Count == 0)
      AddKeyFrame ();
    ki = Mathf.Clamp (ki - 1, 0, m_KeyFrames.Count - 1);
    SetPlayhead (m_KeyFrames [ki]); 
  }

  void DuplicateKeyFrame(DrawWindowKeyframe keyframe) {
    var newKeyFrame = new DrawWindowKeyframe(keyframe.m_Length);
    newKeyFrame.SetTexture(keyframe.Texture);
    m_KeyFrames.Insert (m_KeyFrames.IndexOf (keyframe) + 1, newKeyFrame);
    SetPlayhead (newKeyFrame);
    m_UnsavedChangesPresent = true;
  }
  void DuplicateKeyFrame() { DuplicateKeyFrame(GetCurrentKeyFrame ()); }

  void Flip(bool bAll = false, bool bVertical = false) {
    if (bVertical && !EditorUtility.DisplayDialog ("Flip Vertically", "Flipping vertically takes a while BECAUSE COMPUTERS.\nFlip Vertically?", "Yes", "No"))
      return;
    var current = GetCurrentKeyFrame ();
    int cols = current.width;
    int rows = current.height;
    var scratch = new Color32[cols];

    if (bVertical) {
      m_SpriteBorder.y = m_TextureHeight - m_SpriteBorder.y;
      m_SpriteBorder.w = m_TextureHeight - m_SpriteBorder.w;
    } else {
      m_SpriteBorder.x = m_TextureWidth - m_SpriteBorder.x;
      m_SpriteBorder.z = m_TextureWidth - m_SpriteBorder.z;
    }
    _singleKeyframe.Clear();
    if (!bAll)
      _singleKeyframe.Add(current);
    foreach (var keyframe in bAll ? m_KeyFrames : _singleKeyframe) {
      var l = keyframe.Texture;
      l.EnsurePixelsArray();
      Debug.Assert(l.width == cols && l.height == rows);
      if (!bVertical) {
        for (int ri = 0; ri < rows; ++ri) {
          System.Array.Copy (l.pixels, ri * cols, scratch, 0, cols);
          System.Array.Reverse (scratch);
          System.Array.Copy (scratch, 0, l.pixels, ri * cols, cols);
        }
        l.Apply (true);
      } else {
        // vertical flip
        for (int ci = 0; ci < cols; ++ci) {
          for (int ri = 0; ri < rows / 2; ++ri) {
            var mirroredIndex = rows - 1 - ri;
            //Color32 temp = l.pixels[ci * l.width + mirroredIndex]; //l.GetPixelFast(ci, mirroredIndex);
            Color32 temp = l.GetPixelFastNoBuffer (ci, mirroredIndex);
            l.SetPixelFastNoBuffer (ci, mirroredIndex, l.GetPixelFastNoBuffer (ci, ri));
            l.SetPixelFastNoBuffer (ci, ri, temp);
          }
        }
        l.UpdateTextureFromBuffer();
        l.Apply ();
      }
    }
    m_UnsavedChangesPresent = true;
  }

  // Utils
  bool ToolButton(Rect rect, Tool Tool, string filename, string Tooltip = "", int Width = 50, int Height = 50) {
    if (GUI.enabled && m_CurrentTool == Tool) {
      GUI.color = COLOR_SELECTEDTOOLBG;
      StaticResources.GetEditorSprite("bg_rounded.png").DrawFrame(0, 
        new Rect(rect.x - 4, rect.y - 4, rect.width + 8, rect.height + 30),
        true, false);
      GUI.color = COLOR_SELECTEDTOOL;
    } else {
      GUI.color = Color.white;
    }
    
    var b = ImgButton(rect, filename, "buttonframe_rounded.png", GUI.enabled, Tooltip, 1.2f);
    GUI.color = Color.white;    
    return b;
  }

  bool ImgButton(string filename, string border, bool Enabled = true, string Tooltip = "", int Width = 50, int Height = 50, float FaceScale = 1) {
    GUI.enabled = m_GuiEnabled && Enabled; 
    var b = EditorSprite.DrawCompoundButtonLayout(new Rect(0,0,Width,Height), 
      StaticResources.GetEditorSprite(border),
      StaticResources.GetEditorSprite(filename),
      Tooltip,
      FaceScale
    );
    
    GUI.enabled = m_GuiEnabled;
    return b;
  }
  bool ImgButton(string filename, bool Enabled = true, string Tooltip = "", int Width = 50, int Height = 50) {
    return ImgButton(filename, "buttonframe_big.png", Enabled, Tooltip, Width, Height);
  }
  bool ImgButton(Rect rect, string filename, string border, bool Enabled = true, string Tooltip = "", float FaceScale = 1) {
    GUI.enabled = m_GuiEnabled && Enabled; 
    var b = EditorSprite.DrawCompoundButton(rect, 
      StaticResources.GetEditorSprite(border),
      StaticResources.GetEditorSprite(filename),
      Tooltip,
      FaceScale
    );
    GUI.enabled = m_GuiEnabled;
    return b;
  }

  Rect[] DrawPlaybackButtons(Rect r, int amount) {
    GUI.color = COLOR_TIMELINE_BACKGROUND;
    StaticResources.GetEditorSprite("bg_timeline.png").DrawFrame(0, r.Offset(-4,0,4,0), true, false);
    GUI.color = Color.white;

    return r.ScaleCentered(8,4).GetEvenRectsHorizontal(amount, new Vector2(-3,0), false);

  }

  void UpdateReflectedPoints(List<Vector2> points, float x, float y, float PivotX, float PivotY, SymmetryMode Mode) {
    points.Clear ();
    if (Mode == SymmetryMode.None)
      return;
    var hs = new Vector2 (PivotX, PivotY);
    var p = new Vector2 (x, y);
    p -= hs;
    bool addHorizontal = Mode == SymmetryMode.Horizontal || Mode == SymmetryMode.Fourways;
    bool addVertical = Mode == SymmetryMode.Vertical || Mode == SymmetryMode.Fourways || Mode == SymmetryMode.PlayingCard;
    bool radial = Mode == SymmetryMode.Radial;
    if (addHorizontal)
      points.Add (new Vector2 (-p.x, p.y) + hs);
    if (addVertical) {
      points.Add (new Vector2 (p.x * (Mode == SymmetryMode.PlayingCard ? -1 : 1), -p.y) + hs);
    }
    if (addHorizontal && addVertical)
      points.Add (new Vector2 (-p.x, -p.y) + hs);
    if (radial) {
      int s = DrawPrefs.Instance.m_RadialSymmetryRepetitions;;
      for(int i = 1; i < s; i++) {
        var v = new Vector2(p.x, p.y);
        // v -= hs;
        // v -= new Vector2(Layer.width * .5f, Layer.height * .5f);
        v = v.Rotate(((float)i / (float)s) * 360f);
        v += hs;
        // Now we make sure the resulting point is repeated if in pattern mode to avoid clipping
        // IsPixelDrawable(Layer, ref xx, ref yy);
        points.Add(v);
      }
    }
  }

  DoodleAnimationFile GetFileAsset() {
    if (string.IsNullOrEmpty(m_CurrentOpenAssetGUID) || AssetDatabase.LoadAssetAtPath<DoodleAnimationFile>(OpenAssetPath) == null || m_UnsavedChangesPresent) {
      SaveInfo saveInfo;
      Save (out saveInfo);
      return AssetDatabase.LoadAssetAtPath<DoodleAnimationFile>(AssetDatabase.GUIDToAssetPath(saveInfo.animationFileGUID));
    } else if (AssetDatabase.LoadAssetAtPath<DoodleAnimationFile>(OpenAssetPath) != null) {
      return AssetDatabase.LoadAssetAtPath<DoodleAnimationFile>(OpenAssetPath);
    }
    return null;
  }

  // Request a tooltip, doesn't draw it right away so tooltip goes on top of everything
  internal void RequestTooltip(string text, Rect pos) {
    if (!GUI.enabled) return;
    m_TooltipText = text;
    m_TooltipPos = pos;
  }
  void DrawTooltip(Rect container) {
    if (string.IsNullOrEmpty(m_TooltipText)) return;
    m_TooltipText = m_TooltipText.ToLower();
    string[] lines = m_TooltipText.Split('\n');

    // the rect of the actual visible window
    var windowRect = new Rect(container.x, container.y, position.width, position.height);
  
    m_TooltipStyle = new GUIStyle();
    m_TooltipStyle.richText = true;
    m_TooltipStyle.font = StaticResources.GetFont();
    m_TooltipStyle.alignment = TextAnchor.MiddleCenter;
    m_TooltipStyle.fontStyle = FontStyle.Normal;
    m_TooltipStyle.fontSize = 20;
    m_TooltipStyle.normal.textColor = Color.white;
    var titleStyle = new GUIStyle(m_TooltipStyle);
    titleStyle.fontSize = 20;
    var subtitleStyle = new GUIStyle(m_TooltipStyle);
    subtitleStyle.fontSize = 15;

    float lineHeight = titleStyle.fontSize;
    var finalSize = Vector2.zero;
    for(int i = 0; i < lines.Length; i++) {
      var size = (i == 0 ? titleStyle : subtitleStyle).CalcSize(new GUIContent(lines[i]));
      finalSize.x = Mathf.Max(finalSize.x, size.x);
      finalSize.y += size.y;
    }

    var tooltipRect = new Rect(0,0,finalSize.x * 1.05f + 65, finalSize.y + 30);
    tooltipRect.x = m_TooltipPos.center.x - tooltipRect.width * .5f;
    tooltipRect.y = m_TooltipPos.yMax - 3;
    tooltipRect.x = Mathf.Clamp(tooltipRect.x, windowRect.xMin - 8, windowRect.xMax - tooltipRect.width + 8);
    if (tooltipRect.yMax > windowRect.yMax) 
      tooltipRect.y = m_TooltipPos.yMin - tooltipRect.height + 3; // flip to top side if too far down

    GUI.color = new Color(0,0,0,0.5f);
    StaticResources.GetEditorSprite("bg_square.png").DrawAnimated(new Rect(tooltipRect.x + 3, tooltipRect.y + 3, tooltipRect.width, tooltipRect.height), true);
    GUI.color = new Color(55f/255f,55f/255f,55f/255f);
    StaticResources.GetEditorSprite("bg_square.png").DrawAnimated(tooltipRect, true);
    GUI.color = Color.white;
    lineHeight = (tooltipRect.height * .6f) / (float)lines.Length;
    for(int i = 0; i < lines.Length; i++) {
      GUI.color = new Color(1,1,1,0.5f);
      //GUI.Box(new Rect(tooltipRect.x, tooltipRect.y + tooltipRect.height * .2f + lineHeight * i - 3, tooltipRect.width, lineHeight), "");
      GUI.color = Color.white;
      GUI.Label(new Rect(tooltipRect.x, tooltipRect.y + tooltipRect.height * .2f + lineHeight * i - 3, tooltipRect.width, lineHeight), lines[i], (i == 0 ? titleStyle : subtitleStyle));
    }
  }

  void CycleOnionSkin() {
    int i = Mathf.Clamp(OnionModes.IndexOf(m_OnionSkinMode), 0, OnionModes.Count);
    m_OnionSkinMode = OnionModes[(int)Mathf.Repeat(i + 1, OnionModes.Count)];
  }

  void OpenPopup(string Popup) {
    m_CurrentPopup = Popup;
    var window = StaticResources.GetWindow(m_CurrentPopup);
    if (window != null) {
      window.AnimationTime = 0;
    } else {
      Debug.LogWarning("Couldn't open popup " + Popup);
      m_CurrentPopup = "";
    }
  }
  void ClosePopup() { m_CurrentPopup = ""; } 


  internal static void OpenAndLoad(DoodleAnimationFile file) {
    EditorWindow.FocusWindowIfItsOpen(typeof(DrawWindow));
    var window = (DrawWindow)EditorWindow.GetWindow(typeof(DrawWindow), false);
    window.Load(AssetDatabase.GetAssetPath(file));
    window.SetPlayback(true);
  }
   
}
}
