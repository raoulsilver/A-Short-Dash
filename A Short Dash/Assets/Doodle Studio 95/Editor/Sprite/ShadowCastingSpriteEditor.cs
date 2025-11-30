using System.Collections;
using UnityEngine;
using UnityEditor;

namespace DoodleStudio95 {
[CustomEditor(typeof(ShadowCastingSprite)), CanEditMultipleObjects()]
public class ShadowCastingSpriteEditor : Editor {

	SpriteRenderer renderer;
	void OnEnable() {
		renderer = (target as ShadowCastingSprite).GetComponent<SpriteRenderer>();
	}

	override public void OnInspectorGUI() {
		var t = target as ShadowCastingSprite;
		EditorGUI.BeginChangeCheck();
		base.OnInspectorGUI();
		if(EditorGUI.EndChangeCheck()) {
			t.SetMode();
		}
		if (t.castShadows != UnityEngine.Rendering.ShadowCastingMode.Off && renderer.sharedMaterial.shader.name.Contains("Sprites/Default")) {
			EditorGUILayout.HelpBox("The default sprite material won't cast shadows.\nChange the Material in the Sprite Renderer.", MessageType.Error);
		}
	}
}
}