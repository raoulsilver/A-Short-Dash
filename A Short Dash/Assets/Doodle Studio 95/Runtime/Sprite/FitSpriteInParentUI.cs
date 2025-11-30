using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DoodleStudio95 {
///
/// Utility function to fit a SpriteRenderer inside of a UI element while keeping its parent's bounds
///
[ExecuteInEditMode, RequireComponent(typeof(SpriteRenderer))]
public class FitSpriteInParentUI : MonoBehaviour {
	
	RectTransform _parent;
	SpriteRenderer _spr;

	void OnEnable() {
		_parent = transform.parent.GetComponent<RectTransform>();
		if (!_parent) {
			enabled = false;
			return;
		}
		_spr = GetComponent<SpriteRenderer>();
	}
	void Update () {
		FitSpriteInUI(_spr, _parent);
	}

	// Source: https://forum.unity3d.com/threads/overdraw-spriterenderer-in-ui.339912/
	static public void FitSpriteInUI(SpriteRenderer From, RectTransform To) {
		float pxWidth = To.rect.width;            //width  of the scaled UI-Object in pixel
		float pxHeight = To.rect.height;        //height of the scaled UI-Object in pixel

		if (float.IsNaN(pxHeight) || float.IsNaN(pxWidth)) {
			//unity hasn't not yet initialized (usually happens during start of the game)
			return;
		}
		float spriteX = From.sprite.bounds.size.x;
		float spriteY = From.sprite.bounds.size.y;
		
		float scaleX = pxWidth / spriteX;
		float scaleY = pxHeight / spriteY;

		#if UNITY_5_6_OR_NEWER
		if (From.drawMode != SpriteDrawMode.Simple) {
			// This is incorrect but will display SOMETHING.  TODO: Fit sliced sprites correctly 
			scaleX = scaleY = From.sprite.pixelsPerUnit * .5f;
			var s = From.size;
			s.x = (pxWidth / From.sprite.pixelsPerUnit) * 2;
			s.y = (pxHeight / From.sprite.pixelsPerUnit) * 2;
			From.size = s;
		}
		#endif
		From.transform.localScale = new Vector3(scaleX, scaleY, 1);
		var p = From.transform.localPosition;
		p.x = p.y = 0;
		From.transform.localPosition = p;
	}
}
}