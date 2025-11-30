using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace DoodleStudio95 {
[CustomPreview(typeof(DoodleAnimationFile))]
internal class DoodleAnimationFilePreview : ObjectPreview {

	internal static bool m_Play = true;

	override public bool HasPreviewGUI() { return true; }

	override public void OnPreviewSettings()
	{
		GUIStyle gs = EditorStyles.whiteMiniLabel;
		gs.alignment = TextAnchor.UpperCenter;
		if (GUILayout.Button(m_Play ? "[Preview animation ON]" : "[Preview animation OFF]", gs, GUILayout.Width(120))) {
			m_Play = !m_Play;
		}
	}

	override public void OnPreviewGUI(Rect r, GUIStyle background) {
    var t = target as DoodleAnimationFile;
		if (t.frames.Count > 0) {
			int frameI = 0;
			DoodleAnimationFileKeyframe preview = m_Play ? t.GetFrameAt(out frameI, EditorApplication.timeSinceStartup) : t.frames[0];
			if (preview != null && preview.Texture) {
				Rect imgr = new Rect(0,0,t.width,t.height).ScaleToFit(r);
				DrawThumbnail(imgr, preview.Texture, t.darkCheckerboard);
				GUI.color = new Color(1,0,0,0.5f);
				GUI.color = Color.white;
				if (t.spriteBorder != Vector4.zero) {
					EditorUtils.DrawSpriteBorder(imgr, new Color(0,0,0,0.25f), t.width, t.height, t.spriteBorder);
				}
			}
		}
	}
	internal void DrawThumbnail(Rect r, Texture2D texture, bool dark = false) {
		float aspect = r.width / r.height;
		GUI.color = dark ? DrawWindow.COLOR_DARK_CHECKERBOARD : Color.white;
		GUI.DrawTextureWithTexCoords(r, StaticResources.GetTexture2D("transparency.png"), new Rect(0, 0, 10 * aspect, 10));
		GUI.color = Color.white;
		GUI.DrawTexture (r, texture, ScaleMode.StretchToFill, true);
	}
}
}