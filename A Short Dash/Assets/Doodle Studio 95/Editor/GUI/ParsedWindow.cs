using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.Assertions;

namespace DoodleStudio95 {
[System.Serializable]
public class ParsedWindow {
	[System.Serializable]
	public class ParsedWindowElement {
		public string name;
		[SerializeField] Rect rect;
		public float angle;
		public string mainTextureGUID;
		public string MainTextureFileName { get { return System.IO.Path.GetFileName(AssetDatabase.GUIDToAssetPath(mainTextureGUID)); } }
		public string childMainTextureGUID;
		public string ChildTextureFilename { get { return System.IO.Path.GetFileName(AssetDatabase.GUIDToAssetPath(childMainTextureGUID)); } }
		public Color color;
		public bool isStatic;
		public int drawOrder;

		public Rect localRect { get { return rect; } set { rect = value; } }
		public Rect GetRect(ParsedWindow ParentWindow) {
			var rect = new Rect(this.rect);
			rect.x *= ParentWindow.Scale;
			rect.y *= ParentWindow.Scale;
			rect.width *= ParentWindow.Scale;
			rect.height *= ParentWindow.Scale;

			rect.x += ParentWindow.Position.x;
			rect.y += ParentWindow.Position.y;
			return rect;
		}
	}
	public string Name;
	public List<ParsedWindowElement> Elements = new List<ParsedWindowElement>();
	public Bounds bounds = new Bounds();

	// Edited at runtime
	[SerializeField]
	public Vector2 Position = Vector2.zero;
	[SerializeField]
	public float Scale = 1;
	[SerializeField]
	public float AnimationTime = 0;

	Dictionary<string, ParsedWindowElement> _elementsDict;
	Dictionary<string, ParsedWindowElement> elementsDict { get {
		if (_elementsDict == null) {
			_elementsDict = new Dictionary<string, ParsedWindowElement>();
			foreach(var e in Elements) {
				_elementsDict.Add(e.name, e);
			}
		}
		return _elementsDict;
	}
	}

	public void Add(ParsedWindowElement Element) {
		Elements.Add(Element);
		var r = Element.localRect;
		bounds.Encapsulate(new Bounds(new Vector3(r.center.x, r.center.y, 0), new Vector3(r.width, r.height, 0)));
	}
	public ParsedWindowElement GetElement(string ElementName) {
		if (!elementsDict.ContainsKey(ElementName)) {
			Debug.Log("window " + Name + " doesn't contain element " + ElementName); //
			return new ParsedWindowElement();
		}
		return elementsDict[ElementName];
	}

	public void DrawElement(ParsedWindowElement element, bool Animated = false, int Frame = 0) {
		var rect = element.GetRect(this);

		var prevMatrix = GUI.matrix;
		GUIUtility.RotateAroundPivot(-element.angle, rect.center);
		var prevColor = GUI.color;
		GUI.color = element.color;
		var sprite = StaticResources.GetEditorSprite(element.MainTextureFileName);
		if (string.IsNullOrEmpty(element.MainTextureFileName) || sprite == null) {
			Debug.LogError("Element " + element.name + " has no texture");
		} else {
			if (!Animated) {
				sprite.DrawFrame(Frame, rect, sprite.HasBorder);
			} else {
				sprite.DrawAnimated(rect, sprite.HasBorder);
			}
		}
		GUI.color = prevColor;
		GUI.matrix = prevMatrix;
	}
	public void DrawElement(string ElementName, bool Animated = false, int Frame = 0) {
		DrawElement(GetElement(ElementName), Animated, Frame);
	}
	public bool DrawElementAsButton(string ElementName, string tooltip = "", int ForceFrame = -1, bool firstFrameIsPressedFrame = true, bool chunky = false) {
		var element = GetElement(ElementName);
		var rect = element.GetRect(this);

		var prevMatrix = GUI.matrix;
		GUIUtility.RotateAroundPivot(-element.angle, rect.center);
		var prevColor = GUI.color;
		GUI.color = element.color;
		var sprite = StaticResources.GetEditorSprite(element.MainTextureFileName);
		sprite.SetFiltering(chunky ? FilterMode.Point : FilterMode.Trilinear);
		bool value = sprite.DrawAsButton(rect, tooltip, false, false, false, ForceFrame, firstFrameIsPressedFrame);
		GUI.color = prevColor;
		GUI.matrix = prevMatrix;
		return value;
	}
	public bool DrawElementAsCompoundButton(string ElementName, string tooltip = "", bool useBorder = false) {
		var element = GetElement(ElementName);
		var rect = element.GetRect(this);

		var prevMatrix = GUI.matrix;
		GUIUtility.RotateAroundPivot(-element.angle, rect.center);
		var prevColor = GUI.color;
		GUI.color = element.color;
		bool b = EditorSprite.DrawCompoundButton(rect, element.MainTextureFileName, element.ChildTextureFilename, tooltip, 1, useBorder);
		GUI.color = prevColor;
		GUI.matrix = prevMatrix;
		return b;
	}

	public void DrawStaticElements() {
		foreach(var e in Elements) {
			if (e != null && e.isStatic) {
				DrawElement(e);
			}
		}
	}
}
}
