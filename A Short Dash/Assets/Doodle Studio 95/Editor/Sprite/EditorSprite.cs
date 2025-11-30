using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace DoodleStudio95 {
[System.Serializable]
internal class EditorSprite {

	internal string assetPath;
	internal string parentWindow = "";

	Texture2D texture;
	Sprite[] sprites;
	Texture2D[] frames;
	GUIStyle[] frameStyles;
	int[] timeline;
	float m_PlayTime;
	double m_LastPlayEditorTime;
	int m_FramesPerSecond = 8;

	internal Texture2D[] Frames { get { return frames; }}
	internal bool HasBorder { get { return !Mathf.Approximately(sprites[0].border.magnitude, 0); } }
	internal bool IsValid { get { return frames != null && frames.Length > 0 && frames[0] != null; } }

	internal EditorSprite(string path) {
		this.assetPath = path;
		Load();
	}

	void Load() {
    texture = AssetDatabase.LoadAssetAtPath<Texture2D> (assetPath);
		if (texture == null) {
			Debug.LogError("Could not load texture " + assetPath);
			return;
		}
		bool wasReadable = EditorUtils.GetReadable(texture);
		if (!wasReadable) Debug.LogWarning("Editor Sprite texture " + assetPath + " not readable, setting it now otherwise this'll slow things down");
		EditorUtils.SetReadable(texture, true);
		
		sprites = EditorUtils.GetOrderedSprites(assetPath);	

		frames = new Texture2D[sprites.Length];
		frameStyles = new GUIStyle[sprites.Length];
		var tml = new List<int>();
		for(int i = 0; i < sprites.Length; i++) {
			frames[i] = DrawUtils.GetTextureCopy(texture, sprites[i].rect);

			frameStyles[i] = new GUIStyle();
			frameStyles[i].border = new RectOffset((int)sprites[i].border.x, (int)sprites[i].border.y, (int)sprites[i].border.z, (int)sprites[i].border.w);
			frameStyles[i].normal.background = frames[i];
			frameStyles[i].stretchWidth = true;
			frameStyles[i].stretchHeight = true;
			for(int j = 0; j < Mathf.Max(DrawUtils.GetParameterInName(sprites[i].name, "len"), 1); j++)
				tml.Add(i);
		}
		timeline = tml.ToArray();

		m_FramesPerSecond = DrawUtils.GetParameterInName(sprites[0].name, "fps");
		if (m_FramesPerSecond == -1) m_FramesPerSecond = 8;

		//DrawUtils.SetReadable(texture, wasReadable);
	}

	internal void DrawFrame(int frameNumber, Rect rect, bool useBorder = false, bool keepAspect = false, bool centered = false) {
		if (sprites == null || sprites.Length == 0 || sprites[0] == null)
			return;
		frameNumber = (int)Mathf.Clamp(frameNumber, 0, sprites.Length - 1);
		var sprite = sprites[frameNumber];

		if (centered) {
			rect.x -= rect.width * .5f;
			rect.y -= rect.height * .5f;
		}
		if (keepAspect) {
			var center = rect.center;
			if (sprite.rect.width > sprite.rect.height) {
				rect.width = rect.height * (sprite.rect.width / sprite.rect.height);
			} else {
				rect.height = rect.width * (sprite.rect.height / sprite.rect.width);				
			}
			rect.center = center;
		}
		if (useBorder) {
			GUI.Box(rect, "", frameStyles[frameNumber]);
		} else {
			if (frames != null && frames.Length > 0 && frameNumber < frames.Length && frames[frameNumber] != null)
				GUI.DrawTexture(rect, frames[frameNumber]);
			/* 
			// Note: This will ignore filtering mode!			 
			GUI.DrawTextureWithTexCoords(rect, texture, 
				new Rect(
					sprite.rect.x / texture.width,
					sprite.rect.y / texture.height,
					sprite.rect.width / texture.width,
					sprite.rect.height / texture.height
				),
				true
			);
			*/
		}
	}
	internal void DrawFrame(int frameNumber, float X, float Y, bool centered = false) { 
		DrawFrame(frameNumber, new Rect(X, Y, sprites[frameNumber].rect.width, sprites[frameNumber].rect.height), false, true, centered); 
	}

	internal void DrawAnimated(Rect rect,  bool useBorder = false, bool keepAspect = false, bool centered = false, int startFrame = 0, int endFrame = -1) {
		if (endFrame == -1) endFrame = timeline.Length;
		float delta = (float)(EditorApplication.timeSinceStartup - m_LastPlayEditorTime) * m_FramesPerSecond;
		float newPlayTime = m_PlayTime + delta;
		m_LastPlayEditorTime = EditorApplication.timeSinceStartup;
		m_PlayTime = newPlayTime;
		float t = m_PlayTime + rect.x * rect.y * 10 * (timeline.Length - 1); // randomize based on position
		int frameNumber = timeline[Mathf.Clamp(startFrame + (int)Mathf.Repeat(Mathf.FloorToInt(t), (endFrame - startFrame)), 0, timeline.Length - 1)];
		DrawFrame(frameNumber, rect, useBorder, keepAspect, centered);
	}
	internal void DrawAnimated(float X, float Y, bool centered = false, int startFrame = 0, int endFrame = -1) { 
		DrawAnimated(new Rect(X, Y, sprites[0].rect.width, sprites[0].rect.height), false, true, centered, startFrame, endFrame); 
	}

	internal bool DrawAsButton(Rect rect, string tooltip = "", bool useBorder = false, bool keepAspect = false, bool centered = false, 
			int ForceFrame = -1, bool firstFrameIsPressedFrame = true) {
		
		var state = DrawWindow.GetStateAtCoords(rect);
		
		bool showTooltip = state == DrawWindow.MouseState.Hovered || state == DrawWindow.MouseState.Pressed;
		if (showTooltip && !string.IsNullOrEmpty(tooltip) && DrawWindow.m_Instance != null)
				DrawWindow.m_Instance.RequestTooltip(tooltip, rect);

		if (ForceFrame >= 0) {
			DrawFrame(ForceFrame, rect, useBorder, keepAspect);
		} else {
			switch(state) {
				case DrawWindow.MouseState.Hovered:
				DrawAnimated(rect, useBorder, keepAspect, centered, (firstFrameIsPressedFrame ? 1 : 0));
				break;
				case DrawWindow.MouseState.Pressed:
				case DrawWindow.MouseState.MouseUp:
				DrawFrame(0, rect, useBorder, keepAspect);
				break;
				case DrawWindow.MouseState.Idle:
				DrawFrame((firstFrameIsPressedFrame ? 1 : 0) + Mathf.FloorToInt((sprites.Length - 2) * 
					Mathf.Repeat((Mathf.Sin(rect.x * 8) + Mathf.Sin(rect.y * 5)) * 0.5f + 0.5f, 1.0f))// shake things up with a bit of randomness
					, rect, useBorder, keepAspect);
				break;
			}
		}

		// Workaround for bad button behavior in 2017
		GUI.color = Color.clear;//new Color(0,0,0,0.5f);
		var pressed = GUI.Button(rect, "");
		GUI.color = Color.white;
		return pressed;
		
		// return state == DrawWindow.MouseState.MouseUp;
	}

	internal bool DrawAsButtonWithLabelLayout(string label, string tooltip = "", float width = 0, float height = 60, int fontSize = 18, Color? color = null, bool useBorder = true) {
		Rect r = Rect.zero;
		if (width > 0)
			r = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.objectField, GUILayout.Width(width), GUILayout.Height(height));
		else
			r = GUILayoutUtility.GetRect(0, EditorGUIUtility.currentViewWidth, 0, height);
		GUI.color = color != null ? color.GetValueOrDefault() : Color.white;
		var but = DrawAsButton(r, tooltip, useBorder);
		EditorSprite.DrawLabel(new Rect(r.x, r.y, r.width * .9f, r.height), label, Color.black, fontSize);
		GUI.color = Color.white;
		return but;
	}

	// static
	internal static bool DrawCompoundButton(Rect rect, EditorSprite backgroundSprite, EditorSprite faceSprite, string tooltip = "", 
			float faceScale = 1, bool useBorder = false) {
		if (faceSprite == null) {
			Debug.LogError("No face sprite present");
			return backgroundSprite.DrawAsButton(rect, tooltip, useBorder);
		}
		var state = DrawWindow.GetStateAtCoords(rect);
		bool showTooltip = state == DrawWindow.MouseState.Hovered || state == DrawWindow.MouseState.Pressed;
		if (!GUI.enabled)
			GUI.color = DrawWindow.COLOR_DISABLED;
		bool pressed = backgroundSprite.DrawAsButton(rect, null, useBorder, false, false, -1, true);
		int border = (int)(rect.width * 0.255f);
		if (rect.width > rect.height) border = (int)(rect.height * 0.165f);
		float offset =  - rect.width * (state == DrawWindow.MouseState.Pressed ? .0f : .03f);
		var r = new Rect(rect.x + border + offset, rect.y + border + offset, rect.width - border * 2, rect.height - border * 2);
		r = r.ScaleCentered(faceScale);
		if (state == DrawWindow.MouseState.Hovered) {
			faceSprite.DrawAnimated(r, false, true);
		} else {
			faceSprite.DrawFrame(0, r, false, true);
		}
		if (!GUI.enabled)
			GUI.color = new Color(GUI.color.r,GUI.color.g,GUI.color.b,1.0f);
		if (showTooltip && GUI.enabled && !string.IsNullOrEmpty(tooltip) && DrawWindow.m_Instance != null)
				DrawWindow.m_Instance.RequestTooltip(tooltip, rect);
		return pressed;
	}
	internal static bool DrawCompoundButton(Rect rect, string background, string face, string tooltip = "", float faceScale = 1, bool useBorder = false) {
		return DrawCompoundButton(rect, StaticResources.GetEditorSprite(background), StaticResources.GetEditorSprite(face), tooltip, faceScale, useBorder);
	}
	internal static bool DrawCompoundButton(Rect rect, string face, string tooltip = "") {
		return DrawCompoundButton(rect, StaticResources.GetEditorSprite("buttonframe_big.png"), StaticResources.GetEditorSprite(face), tooltip);
	}
	internal static bool DrawCompoundButtonLayout(Rect rect, EditorSprite backgroundSprite, EditorSprite faceSprite, string tooltip = "", float faceScale = 1, bool useBorder = false) {
		var r = EditorGUILayout.BeginHorizontal(GUILayout.Width(rect.width), GUILayout.Height(rect.height));
		EditorGUILayout.Space();
		EditorGUILayout.EndHorizontal();
		rect.x = r.x;
		rect.y = r.y;
		bool b = DrawCompoundButton(rect, backgroundSprite, faceSprite, tooltip, faceScale, useBorder);
		return b;
	}
	internal static bool DrawCompoundButtonLayout(Rect rect, string background, string face, string tooltip = "", float faceScale = 1, bool useBorder = false) {
		return DrawCompoundButtonLayout(rect, StaticResources.GetEditorSprite(background), StaticResources.GetEditorSprite(face), tooltip, faceScale, useBorder);
	}
	internal static void DrawLabel(Rect rect, string Text, Color color, int fontSize = 20, bool showBackground = false) {
		var lines = Text.Split('\n');
		var labelStyle = new GUIStyle();
    labelStyle.richText = true;
    labelStyle.font = StaticResources.GetFont();
    labelStyle.alignment = TextAnchor.MiddleCenter;
    labelStyle.fontStyle = FontStyle.Normal;
    labelStyle.fontSize = fontSize;
    labelStyle.normal.textColor = Color.white;

    float lineHeight = fontSize;
    var finalSize = Vector2.zero;
    for(int i = 0; i < lines.Length; i++) {
      var size = labelStyle.CalcSize(new GUIContent(lines[i]));
      finalSize.x = Mathf.Max(finalSize.x, size.x);
      finalSize.y += size.y;
    }
		if (showBackground) {
			GUI.color = new Color(0,0,0,0.5f);
			StaticResources.GetEditorSprite("bg_square.png").DrawAnimated(new Rect(rect.x + 3, rect.y + 3, rect.width, rect.height), true);
			GUI.color = new Color(55f/255f,55f/255f,55f/255f);
			StaticResources.GetEditorSprite("bg_square.png").DrawAnimated(rect, true);
			GUI.color = Color.white;
		}
    lineHeight = (rect.height * .6f) / (float)lines.Length;
    GUI.color = color;
    for(int i = 0; i < lines.Length; i++) {
      GUI.Label(new Rect(rect.x, rect.y + rect.height * .2f + lineHeight * i - 3, rect.width, lineHeight), lines[i], labelStyle);
    }
		GUI.color = Color.white;
	}

	internal void SetFiltering(FilterMode mode) {
		foreach(var f in frames) {
			f.filterMode = mode;
		}
	}

}
}