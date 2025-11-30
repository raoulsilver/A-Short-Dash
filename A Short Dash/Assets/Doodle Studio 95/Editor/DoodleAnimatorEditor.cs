using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

namespace DoodleStudio95 {

using PlaybackMode = DoodleAnimationFile.PlaybackMode;
using Settings = DoodleAnimator.Settings;

// [CustomPropertyDrawer(typeof(DoodleAnimator.DoodleAnimationState))]
// internal class DoodleAnimationStatePropertyDrawer : MultiPropertyDrawer {
	
// 	bool PreviewAllowed { get { return !Application.isPlaying; } }// && !isPrefab && (target as MonoBehaviour).isActiveAndEnabled; } }
// 	float lastHeight = 0;

// 	override internal string[] GetFields() {
// 		return new string[] {
// 			"randomizeOnStart",
// 			"playOnStart",
// 			"speed"
// 		};
// 	}
// // }

// [CustomPropertyDrawer(typeof(DoodleAnimator.Settings)), CanEditMultipleObjects()]
// internal class AnimatorSettingsPropertyDrawer : MultiPropertyDrawer<DoodleAnimator.Settings> {
	
// 	bool PreviewAllowed { get { return !Application.isPlaying; } }// && !isPrefab && (target as MonoBehaviour).isActiveAndEnabled; } }

// 	override internal float GetPropertyHeight(SerializedProperty property, GUIContent label) {
// 		var h = EditorGUI.GetPropertyHeight(property, label);
// 		Debug.Log("height " + h);
// 		return h;
// 	}
// 	override internal void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
// 		// position.height = 500;
// 		GUI.Box(new Rect(position.x, position.y, position.width, position.height), "", EditorStyles.helpBox);

// 		var viewWidth = EditorGUIUtility.currentViewWidth - 0;
		
// 		BeginLayout(position);

// 		PropertyFieldLayout(FindProperty(property, x => x.playOnStart));
// 		PropertyFieldLayout(FindProperty(property, x => x.randomizeOnStart));
		
// 		var propOverride = FindProperty(property, x => x.overrideFileSettings);
// 		// propOverride.boolValue = EditorGUILayout.ToggleLeft(new GUIContent("Custom settings"), propOverride.boolValue);
// 		PropertyFieldLayout(propOverride);
// 		if (propOverride.boolValue) {
// 			EditorGUI.indentLevel++;
// 			var propPlaybackMode = FindProperty(property, x => x.customPlaybackMode);
// 			PropertyFieldLayout(propPlaybackMode,new GUIContent("Playback Mode"));
// 			if ((DoodleAnimationFile.PlaybackMode)propPlaybackMode.intValue == DoodleAnimationFile.PlaybackMode.SingleFrame) {
// 				var propStartFrame = FindProperty(property, x => x.startFrame);
// 				PropertyFieldLayout(FindProperty(property, x => x.startFrame)); // TODO: clamp
// 			}
// 			PropertyFieldLayout(FindProperty(property, x => x.customFramesPerSecond));

// 			PropertyFieldLayout(FindProperty(property, x => x.filterMode));
// 			PropertyFieldLayout(FindProperty(property, x => x.wrapMode));
// 			PropertyFieldLayout(FindProperty(property, x => x.customSliced));
// 			PropertyFieldLayout(FindProperty(property, x => x.customPixelsPerUnit));
// 			EditorGUI.indentLevel--;
// 		}

// 		// position = EndLayout(); // Sets the total height

// 		Debug.Log(position);
// 	}
// }


[InitializeOnLoad]
[CustomEditor(typeof(DoodleAnimator)), CanEditMultipleObjects()]
internal class DoodleAnimatorEditor : BaseEditor<DoodleAnimator> {

	int previewTimelineFrame = 0;

	bool isPrefab = false;
	Color _lastSpriteColor;
	bool _queued_preview;

	bool PreviewAllowed { get { return !Application.isPlaying && !isPrefab && 
		m_Target && m_Target.isActiveAndEnabled; } }

	SerializedProperty m_File;
	SerializedProperty m_Settings;
	SerializedProperty m_Speed;

	// Settings
	SerializedProperty m_RandomizeOnStart;
	SerializedProperty m_PlayOnStart;
	SerializedProperty m_OverrideFileSettings;
	SerializedProperty m_PlaybackMode;
	SerializedProperty m_FPS;
	SerializedProperty m_Border;
	SerializedProperty m_PPU;
	SerializedProperty m_FilterMode;
	SerializedProperty m_WrapMode;
	SerializedProperty m_StartFrame;
	SerializedProperty m_UseUnscaledTime;


	// Get a callback when project window changed to update existing animators in the scene if files changed
	static DoodleAnimatorEditor() {
		UnityEditor.EditorApplication.projectChanged += OnProjectWindowChanged;

		DrawUtils.DestroyCallback += EditorDestroy;
		DrawUtils.CopyEditorTextureProperties += CopyEditorTextureProperties;
	}

	static void EditorDestroy(Object obj)
	{
		if (Application.isPlaying)
			Destroy(obj);
		else
			DestroyImmediate(obj);
	}

	static void CopyEditorTextureProperties(Texture2D dest, Texture2D source)
	{
		dest.alphaIsTransparency = source.alphaIsTransparency;
	}

	static void OnProjectWindowChanged() {
		if (!Application.isPlaying) {
			foreach(var animator in FindObjectsByType<DoodleAnimator>(FindObjectsSortMode.None)) {
				animator.UpdateRenderers();
			}
		}
		SceneView.RepaintAll();
	}

	void OnEnable() {
		isPrefab = !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(target));
		_queued_preview = DrawPrefs.Instance.inspector_preview_on;

		m_File = serializedObject.FindProperty("m_File");
		m_Settings = FindProperty(x => x.m_Settings);
		m_Speed = FindProperty(x => x.speed);

		m_RandomizeOnStart = FindProperty(x => x.m_Settings.randomizeOnStart);
		m_PlayOnStart = FindProperty(x => x.m_Settings.playOnStart);
		m_OverrideFileSettings = FindProperty(x => x.m_Settings.overrideFileSettings);
		m_PlaybackMode = FindProperty(x => x.m_Settings.customPlaybackMode);
		m_FPS = FindProperty(x => x.m_Settings.customFramesPerSecond);
		m_Border = FindProperty(x => x.m_Settings.customBorder);
		m_PPU = FindProperty(x => x.m_Settings.customPixelsPerUnit);
		m_FilterMode = FindProperty(x => x.m_Settings.customFilterMode);
		m_WrapMode = FindProperty(x => x.m_Settings.wrapMode);
		m_StartFrame = FindProperty(x => x.m_Settings.startFrame);
		m_UseUnscaledTime = FindProperty(x => x.m_Settings.useUnscaledTime);
		
		EditorApplication.update += OnUpdate;
		Undo.undoRedoPerformed += OnUndoRedo;
	}
	void OnDisable() {
		EditorApplication.update -= OnUpdate;
		Undo.undoRedoPerformed -= OnUndoRedo;
		/* //
    var t = target as DoodleAnimator;
		if (t != null) // might've been destroyed when deleting the gameobject
			t.SetFrame(0);
		*/
	}

	void OnUndoRedo() {
		if (m_Target)
			m_Target.UpdateRenderers();
	}

	internal void OnUpdate() {
		var t = m_Target;
		if (t == null)  // was destroyed {
			return;

		if (_queued_preview != DrawPrefs.Instance.inspector_preview_on) {
			DrawPrefs.Instance.inspector_preview_on = _queued_preview;
			DrawPrefs.Instance.Save();
			Repaint();
		}
		
		if(!PreviewAllowed)
			return;

		// Special case for applying the color of the sprite renderer to our preview
		if (t.spriteRenderer) {
			if(!Color.Equals(_lastSpriteColor, t.spriteRenderer.color)) {
				_lastSpriteColor = t.spriteRenderer.color;
				t.SetFrame(previewTimelineFrame);
			}
		}

		if (t.File != null) {
			int prevFrame = previewTimelineFrame;
			previewTimelineFrame = t.GetFrameAt(EditorApplication.timeSinceStartup);
			if (previewTimelineFrame != prevFrame) {
				// Update image
				t.SetFrame(previewTimelineFrame);
				Repaint();
				if (!Application.isPlaying && t.uiImageRenderer && t.uiImageRenderer.enabled) {
					// // Workaround for UI preview not being updated unless this happens
					// t.uiImageRenderer.enabled = false;
					// t.uiImageRenderer.enabled = true;

				}
			}
		}
	}

	override public void OnInspectorGUI() {
		var t = m_Target;

		serializedObject.Update();

		if (t.particles != null) {
			EditorGUILayout.HelpBox("This will override some of your particles settings:\nTexture Sheet, material and Start Lifetime", MessageType.Info);
		}
			

		EditorGUILayout.Space();

		bool needsSpriteRegen = false;
		bool preview = PreviewAllowed && DrawPrefs.Instance.inspector_preview_on;
		
		var viewWidth = EditorGUIUtility.currentViewWidth - 40;
		
		// File
		var oldfile = m_File.objectReferenceValue;
		EditorGUI.BeginChangeCheck();
		
		// EditorGUILayout.Space();

		// // background
		using (var r_whole = new EditorGUILayout.VerticalScope()) {
			// var bg = StaticResources.GetTexture2D("bg_main.png");
			// var rect = r_whole.rect.ScaleCentered(-17,-20);
			// rect.x -= 5;
			// float time = 0f;//m_BgScroll * 0.03f;
			// GUI.DrawTextureWithTexCoords(rect, bg, new Rect(-time, 0, 3.0f * (rect.width / rect.height), 3.0f), false);
			// GUI.color = new Color(1,1,1,0.2f);
			// var s = new GUIStyle();
			// s.normal.background = StaticResources.GetTexture2D("bg_thinborder_transparent.png");
			// s.border = new RectOffset(40,40,40,40);
			// GUI.Box(rect, "", s);
			// GUI.color = Color.white;
		
		// }
		// File field with editr button
		int propHeight = 20;
		using(new EditorGUILayout.HorizontalScope(GUILayout.Height(propHeight))) {
			EditorGUIUtility.labelWidth = viewWidth * .23f;
			EditorGUILayout.PropertyField(m_File, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
			EditorGUIUtility.labelWidth = 0; // reset
			if (StaticResources.GetEditorSprite("buttonframe_smaller.png").DrawAsButtonWithLabelLayout("Edit", "", 45, propHeight, 12, DrawWindow.COLOR_FILEACTION, false)) {
				if (m_File.objectReferenceValue != null) {
					DrawWindow.OpenAndLoad(m_File.objectReferenceValue as DoodleAnimationFile);
				}
			}
			// Preview button
			// var r_preview = new Rect (r_header.x + r_header.width - 30, r_header.y, 30, r_header.height);
			GUI.enabled = t.uiImageRenderer == null;
			GUI.color = (GUI.enabled ? (preview ? DrawWindow.COLOR_ACTIVE : Color.white) : DrawWindow.COLOR_DISABLED);
			var r_preview = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.objectField, GUILayout.Width(propHeight), GUILayout.Height(propHeight));
			if (StaticResources.GetEditorSprite("buttonframe_rounded.png").DrawAsButton(r_preview)) {// && Event.current.type == EventType.MouseUp) {
				_queued_preview = !_queued_preview;
			}
			GUI.color = (GUI.enabled ? Color.white : DrawWindow.COLOR_DISABLED);
			StaticResources.GetEditorSprite("preview.png").DrawFrame(preview ? 0 : 1, r_preview.ScaleCentered(.7f));
			GUI.enabled = true;
			GUI.color = Color.white;
		}
		
		DoodleAnimationFile file = m_File.objectReferenceValue ? m_File.objectReferenceValue as DoodleAnimationFile : null;
		if (EditorGUI.EndChangeCheck() && oldfile != file) {
			t.ChangeAnimation(file, file ? Settings.FromFile(file, t.m_Settings) : t.m_Settings, true);
			needsSpriteRegen = true;
			m_Settings.FindPropertyRelative("startFrame").intValue = 0;
			m_StartFrame.intValue = Mathf.Clamp(m_StartFrame.intValue, 0, file ? file.Timeline.Count - 1 : 0);
		}
		
		if (file != null) {
			if (t.HasBorder && (
				(t.uiImageRenderer && 
					t.uiImageRenderer.type != UnityEngine.UI.Image.Type.Sliced &&
					t.uiImageRenderer.type != UnityEngine.UI.Image.Type.Tiled ) ||
				(t.spriteRenderer && t.spriteRenderer.drawMode == SpriteDrawMode.Simple))) {
				EditorGUILayout.HelpBox("The animation has a Sprite Border. \nSet the renderer mode to Sliced for best results.", MessageType.Warning);
			}

			// Preview
			if (t.HasValidImages && preview &&
				t.uiImageRenderer == null // UI image has its own preview
			) {
				var frame = file.frames [file.Timeline [Mathf.Clamp(previewTimelineFrame,0,file.Timeline.Count-1)]];
				var r = GUILayoutUtility.GetRect(viewWidth, Mathf.Clamp(viewWidth * ((float)file.height / (float)file.width), 10, 250));
				GUI.color = Color.black;
				StaticResources.GetEditorSprite("bg_drawareaborder.png").DrawAnimated(r, true);
				GUI.color = Color.white;

				r = r.ScaleCentered(20, 20);
				EditorGUI.DrawTextureTransparent(r, frame.Texture, ScaleMode.ScaleToFit);
			}

			// Settings
			using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox)) {
				EditorGUILayout.LabelField("Playback settings", EditorStyles.miniLabel);
				PropertyField(m_Speed, "Plays the animation slower or faster");
				PropertyField(m_PlayOnStart, "Play the animation at the start of the scene? If unchecked, only the first frame will show.");
				PropertyField(m_RandomizeOnStart, "Random first frame", "Randomize which frame is chosen at the start. Useful when you place several objects with the same animation.");
				
				EditorGUI.BeginChangeCheck();
				if (serializedObject.isEditingMultipleObjects) // Avoid overriding values if editing multiple objetcs
					PropertyField(m_OverrideFileSettings, "Use your own settings instead of the animation's");
				else
					m_OverrideFileSettings.boolValue = EditorGUILayout.ToggleLeft(
						new GUIContent(m_OverrideFileSettings.displayName, "Use your own settings instead of the animation's"), 
						m_OverrideFileSettings.boolValue);
				if (EditorGUI.EndChangeCheck())
					needsSpriteRegen = true;

				if (m_OverrideFileSettings.boolValue || serializedObject.isEditingMultipleObjects) {
					EditorGUI.indentLevel++;
					PropertyField(m_PlaybackMode, "Playback Mode", "Change how the animation is played");
					if (t.m_Settings.customPlaybackMode == PlaybackMode.SingleFrame) {
						if (file.Timeline.Count > 1) {
							EditorGUI.indentLevel++;
							EditorGUILayout.PropertyField(m_StartFrame);
							if (GUI.changed) {
								m_StartFrame.intValue = Mathf.Clamp(m_StartFrame.intValue, 0, file ? file.Timeline.Count - 1 : 0);
								t.SetFrame();
							}
							EditorGUI.indentLevel--;
						}
					} else {
						EditorGUI.indentLevel++;
						EditorGUILayout.PropertyField(m_FPS, new GUIContent("Frames Per Second"));
						if (GUI.changed)
								m_FPS.intValue = Mathf.Clamp(m_FPS.intValue, 1, int.MaxValue);
						EditorGUILayout.PropertyField(m_UseUnscaledTime, new GUIContent("Unscaled Time", "Ignores Time.timescale and plays the animation normally even if the game is slowed down, for example, in a pause menu."));
						EditorGUI.indentLevel--;
					}
					EditorGUILayout.LabelField("Texture settings", EditorStyles.boldLabel);
					// if (t.particles)
					// 	EditorGUILayout.HelpBox("These settings will have no effect on the particle system.", MessageType.Info);

					DrawPropertyWithRegenCheck(ref needsSpriteRegen, m_FilterMode, new GUIContent("Filter Mode", "What kind of filtering the final image will have. Use point to keep chunky pixels sharp, or Trilinear for smooth borders."));
					DrawPropertyWithRegenCheck(ref needsSpriteRegen, m_WrapMode);
					// if (GUILayout.Button("Reset to defaults", EditorStyles.miniButton)) {
					// 	t.m_Settings.customBorder = file.spriteBorder;
					// 	t.m_Settings.customPixelsPerUnit = DoodleAnimator.Settings.DEFAULT.customPixelsPerUnit;
					// }
					EditorGUILayout.LabelField("Sprite settings", EditorStyles.boldLabel);
					if (t.spriteRenderer || t.uiImageRenderer)
						EditorGUILayout.HelpBox("Changing these settings will make your scene's loading times much longer.", MessageType.Warning);

					EditorGUI.BeginChangeCheck();
					// DrawPropertyWithRegenCheck(ref needsSpriteRegen, m_Border, new GUIContent("Edits the left, bottom, right and top edges of the resulting Sprite border, in pixels. Use this to add 9-slicing to UI elements."));
					m_Border.vector4Value = EditorGUILayout.Vector4Field(
						new GUIContent(m_Border.displayName, "Edits the left, bottom, right and top edges of the resulting Sprite border, in pixels. Use this to add 9-slicing to UI elements."), 
						m_Border.vector4Value);
					if (EditorGUI.EndChangeCheck()) {
						if (t.File) {
							var v = m_Border.vector4Value;
							// v.x = Mathf.Clamp(v.x, 0, t.File.width - v.z);
							// v.z = Mathf.Clamp(v.z, 0, t.File.width - v.x);
							// v.y = Mathf.Clamp(v.y, 0, t.File.height - v.w);
							// v.w = Mathf.Clamp(v.w, 0, t.File.height - v.y);
							v.x = Mathf.Clamp(v.x, 0, t.File.width);
							v.z = Mathf.Clamp(v.z, 0, t.File.width);
							v.y = Mathf.Clamp(v.y, 0, t.File.height);
							v.w = Mathf.Clamp(v.w, 0, t.File.height);
							m_Border.vector4Value = v;
						}
						needsSpriteRegen = true;
					}
					
					DrawPropertyWithRegenCheck(ref needsSpriteRegen, m_PPU, new GUIContent("Pixels per Unit", "Use this to customize the size of the image when using it inside the Unity UI."));

					EditorGUI.indentLevel--;
				}
			}
			if (GUI.changed) {
				if (t.particles) {
					t.SetupParticles();
					t.particles.Simulate(0,true, true);
					t.particles.Play();
				}
			}
		}	
		}
		serializedObject.ApplyModifiedProperties();
		if (needsSpriteRegen) {
			t.UpdateRenderers(); // calls UnloadResources.
		}
		EditorGUILayout.Space();
	}

	void DrawPropertyWithRegenCheck(ref bool needsRegen, SerializedProperty prop, GUIContent guiContent) {
		EditorGUI.BeginChangeCheck();
		EditorGUILayout.PropertyField(prop, guiContent);
		if (EditorGUI.EndChangeCheck())
			needsRegen = true;
	}
	void DrawPropertyWithRegenCheck(ref bool needsRegen, SerializedProperty prop) {
		EditorGUI.BeginChangeCheck();
		EditorGUILayout.PropertyField(prop);
		if (EditorGUI.EndChangeCheck())
			needsRegen = true;
	}
	void PropertyField(SerializedProperty prop, string displayName, string tooltip) {
		EditorGUILayout.PropertyField(prop, new GUIContent(displayName, tooltip));
	}
	void PropertyField(SerializedProperty prop, string tooltip) {
		PropertyField(prop, prop.displayName, tooltip);
	}

}
}