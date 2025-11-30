using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using UnityEngine;
using UnityEditor;

namespace DoodleStudio95 {

// Source UnityEditor.Rendering.PostProcessing
internal class BaseEditor<T> : Editor
	where T : MonoBehaviour
{
	protected T m_Target
	{
		get { return (T)target; }
	}

	protected SerializedProperty FindProperty<TValue>(Expression<Func<T, TValue>> expr)
	{
		return serializedObject.FindProperty(EditorUtils.GetFieldPath(expr));
	}
}

internal class BasePropertyDrawer<T> : PropertyDrawer
{
	protected SerializedProperty FindProperty<TValue>(SerializedProperty property, Expression<Func<T, TValue>> expr)
	{
		return property.FindPropertyRelative(EditorUtils.GetFieldPath(expr));
	}
}

internal class MultiPropertyDrawer<T> : BasePropertyDrawer<T>
{
	// override internal float GetPropertyHeight(SerializedProperty property, GUIContent label) {
	// 	return lastHeight;
	// }
	protected float y;
	protected float lastHeight;
	protected Rect rect;
	protected void BeginLayout(Rect r) { rect = r; y = r.y; }
	protected void PropertyFieldLayout(SerializedProperty property, GUIContent label = null) {
		rect.height = EditorGUI.GetPropertyHeight(property);
		if (label != null)
			EditorGUI.PropertyField(rect, property, label);
		else
			EditorGUI.PropertyField(rect, property);
		rect.y += rect.height; // Advance for the next property
	}
	protected Rect EndLayout() { 
		var r = new Rect(rect.x, y, rect.width, rect.y + rect.height - y);
		lastHeight = r.height;
		return r;
	}
}

internal static class EditorUtils {

	///
	/// Call a method that's not available
	///  eg. InvokeMethod(typeof(AudioImporter), "UnityEditor.AudioUtil", "PlayClip", ...)
	/// https://forum.unity3d.com/threads/way-to-play-audio-in-editor-using-an-editor-script.132042/
	internal static object InvokeMethod(System.Type assemblyType, string className, string methodName, System.Type[] parameterTypes, object[] parameters) {
		Assembly unityEditorAssembly = assemblyType.Assembly;
		System.Type audioUtilClass = unityEditorAssembly.GetType(className);
		MethodInfo method = audioUtilClass.GetMethod(methodName, 
			BindingFlags.Static | BindingFlags.Public, null, parameterTypes, null);
		return method.Invoke(null, parameters);
	}
	
	internal static object InvokeMethod(System.Type assemblyType, string className, string methodName, System.Type parameterType, object parameter) {
		return InvokeMethod(assemblyType, className, methodName, new System.Type[]{parameterType}, new object[]{parameter});
	}

	internal static void DrawSpriteBorder(Rect r, Color color, int imgwidth, int imgheight, Vector4 border, float lineWidth = 1) {
		float w = imgwidth;
		float h = imgheight;
		DrawDottedLine(new Rect (r.xMin + (border.x / w) * r.width, r.y , lineWidth, r.height), color);
		DrawDottedLine(new Rect (r.xMax - (border.z / w) * r.width, r.y, lineWidth, r.height), color);
		DrawDottedLine(new Rect (r.x, r.yMin + (border.w / h) * r.height, r.width, lineWidth), color, false);
		DrawDottedLine(new Rect (r.x, r.yMax - (border.y / h) * r.height, r.width, lineWidth), color, false);
	}

	static void DrawDottedLine(Rect rect, Color color, bool horizontal = true, int separation = 8) {
		for(int y = 0; y < Mathf.CeilToInt((horizontal ? rect.height : rect.width) / separation); y ++) {
			if (y % 2 == 0) {
				if (horizontal)
					UnityEditor.EditorGUI.DrawRect (new Rect (rect.x, rect.y + y * separation, rect.width, separation), color);
				else
					UnityEditor.EditorGUI.DrawRect (new Rect (rect.x + y * separation, rect.y, separation, rect.height), color);
			}
		}
	}
	
	// Source UnityEditor.Rendering.PostProcessing
	// Returns a string path from an expression - mostly used to retrieve serialized properties
	// without hardcoding the field path. Safer, and allows for proper refactoring.
	internal static string GetFieldPath<TType, TValue>(Expression<Func<TType, TValue>> expr)
	{
			MemberExpression me;
			switch (expr.Body.NodeType)
			{
					case ExpressionType.MemberAccess:
							me = expr.Body as MemberExpression;
							break;
					default:
							throw new InvalidOperationException();
			}

			var members = new List<string>();
			while (me != null)
			{
					members.Add(me.Member.Name);
					me = me.Expression as MemberExpression;
			}

			var sb = new StringBuilder();
			for (int i = members.Count - 1; i >= 0; i--)
			{
					sb.Append(members[i]);
					if (i > 0) sb.Append('.');
			}

			return sb.ToString();
	}

	internal static void SelectAndFrame(UnityEngine.Object obj) {
		Selection.objects = new UnityEngine.Object[]{ obj };
    if (SceneView.lastActiveSceneView)
      SceneView.lastActiveSceneView.FrameSelected();
	}

	internal static SerializableTexture2D MakeSerializableTextureWithReadableCheck(Texture2D SourceTexture) {
		if (!string.IsNullOrEmpty(AssetDatabase.GetAssetPath(SourceTexture)) && 
      !GetReadable(SourceTexture)) {
      SetReadable(SourceTexture, true);
      SerializableTexture2D.New(SourceTexture);
      SetReadable(SourceTexture, false);
		}
    return SerializableTexture2D.New(SourceTexture);
	}

	/// Set an asset as readable, returns true if it had to be reimported
		internal static bool SetReadable(Texture2D Texture, bool bReadable = true) {
			if (Texture == null)
				return false;
			var assetPath = AssetDatabase.GetAssetPath(Texture);
			var settings = TextureImporter.GetAtPath(assetPath) as TextureImporter;
			if (settings.isReadable != bReadable) {
				settings.isReadable = bReadable;
				AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
				return true;
			}
			return false;
		}
		internal static bool GetReadable(Texture2D Texture) {
			if (Texture == null)
				return false;
			var assetPath = AssetDatabase.GetAssetPath(Texture);
			var settings = TextureImporter.GetAtPath(assetPath) as TextureImporter;
			if (settings == null)
				return false;
			return settings.isReadable;
		}

		internal static bool ApproximatelyColor32(Color32 C1, Color32 C2) {
			return Mathf.Approximately(C1.r, C2.r) && 
				Mathf.Approximately(C1.g, C2.g) && 
				Mathf.Approximately(C1.b, C2.b) && 
				Mathf.Approximately(C1.a, C2.a);
		}

		internal static float EaseElasticOut(float t) {
			var p = 0.3f;
    	return Mathf.Pow(2,-10*t) * Mathf.Sin((t-p/4)*(2*Mathf.PI)/p) + 1;
		}

		internal static Sprite[] GetOrderedSprites(string assetPath) {
			var sprites = AssetDatabase.LoadAllAssetsAtPath(assetPath).OfType<Sprite>().ToArray();
   		sprites = sprites.OrderBy(x => DrawUtils.GetParameterInName(x.name, "frame")).ToArray();
			return sprites;
		}
		internal static Sprite[] GetOrderedSprites(UnityEngine.Object obj) { return GetOrderedSprites(AssetDatabase.GetAssetPath(obj)); }

		internal static Vector2 Rotate(this Vector2 v, float degrees) {
			float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
			float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);
			
			float tx = v.x;
			float ty = v.y;
			v.x = (cos * tx) - (sin * ty);
			v.y = (sin * tx) + (cos * ty);
			return v;
		}
}

///
/// A property drawer for drawing multiple fields
///		Usage, override GetFields() { return new string[5] { "size", "file", "myProperty" }; } and it'll take care of everything
///
internal class MultiPropertyDrawer : PropertyDrawer {

	float ControlHeight { get { return EditorGUIUtility.singleLineHeight; } }

	virtual internal int GetPadding() { return 5; }
	virtual internal string[] GetFields() { return new string[0]; }

	internal Rect GetTotalRect(Rect position) { return new Rect(
		position.x, position.y, 
		position.width, ControlHeight * GetFields().Length + GetPadding() * 2); 
	}

	override public float GetPropertyHeight(SerializedProperty property, GUIContent label) {
		float h = 0;
		var fields = GetFields();
		for(int i = 0; i < fields.Length; i++) {
			SerializedProperty prop = property.FindPropertyRelative(fields[i]);
			h += EditorGUI.GetPropertyHeight(prop);
		}
		return h + GetPadding() * 2;
		// return base.GetPropertyHeight(property, label) * 3;
		// return ControlHeight * GetFields().Length + GetPadding() * 2;
	}

	internal void NextRect(ref Rect position, float propertyHeight = -1) {
		if (propertyHeight == -1) propertyHeight = ControlHeight;
		position.y += propertyHeight;
	}

	override public void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		GUI.Box(GetTotalRect(position), "", EditorStyles.helpBox);
		float padding = GetPadding();
		position = position.ScaleCentered(padding,padding);
		Rect r = new Rect(position.x, position.y + padding * .5f, position.width - padding, position.height);
		EditorGUI.BeginProperty(position, label, property);
		EditorGUI.BeginChangeCheck();
		var fields = GetFields();
		for(int i = 0; i < fields.Length; i++) {
			SerializedProperty prop = property.FindPropertyRelative(fields[i]);
			r.height = EditorGUI.GetPropertyHeight(prop);
			EditorGUI.PropertyField(r, prop);
			NextRect(ref r, EditorGUI.GetPropertyHeight(prop));
		}
			EditorGUI.EndProperty();
		if (EditorGUI.EndChangeCheck())
			property.serializedObject.ApplyModifiedProperties();
	}

}
}