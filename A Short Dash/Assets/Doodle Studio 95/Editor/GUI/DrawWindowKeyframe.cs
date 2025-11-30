using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace DoodleStudio95 {

// A keyframe used in the DrawWindow, only during edit time.
// Is then converted to a DoodleKeyframe stored in a DoodleAnimationFile
[System.Serializable]
internal class DrawWindowKeyframe {

	[SerializeField] private SerializableTexture2D m_Texture;
	[SerializeField] internal int m_Length;

	internal SerializableTexture2D Texture { get { 
		if (m_Texture != null && m_Texture.IsValid) 
			return m_Texture;
		return null;
	}}

	internal int width { get { 
		if (Texture != null)
			return Texture.width;
		Debug.LogError("No texture!");
		return 1; 
	} }
	internal int height { get { 
		if (Texture != null)
			return Texture.height;
		Debug.LogError("No texture!");
		return 1;
	} }

	internal Texture2D resultTexture { get {
		if (Texture != null)
			return Texture.texture;
		return null;
	}}

	#region Constructors

	internal DrawWindowKeyframe(int Length = 1) {
		m_Length = Length;
	}
	
	internal DrawWindowKeyframe(DoodleAnimationFileKeyframe AssetKeyframe) {
		m_Length = AssetKeyframe.m_Length;
		
		if (AssetKeyframe.Texture)
			SetTexture(DrawUtils.GetTextureCopy(AssetKeyframe.Texture));
		else
			Debug.LogError("No texture to edit! ", AssetKeyframe.Texture);
	}
	
	internal DrawWindowKeyframe(Sprite sprite) {
		m_Length = DrawUtils.keyframeLengthForSprite (sprite);
		if (m_Length == -1) 
			m_Length = 1;
		SetTexture (DrawUtils.GetTextureCopy (sprite.texture, sprite.rect));
	}

	#endregion

	internal void OnDestroy() {
		if (m_Texture != null)
			m_Texture.OnDestroy();
	}

	internal void SetTexture(Texture2D texture) {

		if (m_Texture == null)
			m_Texture = EditorUtils.MakeSerializableTextureWithReadableCheck(texture);
		else
			m_Texture.Create(texture);
	}
	internal void SetTexture(SerializableTexture2D texture) {
		if (m_Texture == null)
			m_Texture = SerializableTexture2D.New(texture);
		else
			m_Texture.Create(texture);
	}


	internal DrawWindowKeyframe Copy() {
		var ki = new DrawWindowKeyframe(m_Length);
		if (Texture != null)
			ki.SetTexture(Texture);
		return ki;
	}
}
}