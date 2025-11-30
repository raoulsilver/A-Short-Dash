using UnityEngine;

namespace DoodleStudio95 {
[System.Serializable]
public class DoodleAnimationFileKeyframe {

	internal string name = "Keyframe";

	[SerializeField] private Texture2D _texture;	
	[SerializeField] private Sprite _sprite;	

	// Deprecated
	[SerializeField] internal readonly Texture2D[] m_Textures;
	[SerializeField] internal readonly Sprite[] m_Sprites;
	[SerializeField] internal readonly SerializableTexture2D[] m_Layers;

	[SerializeField] internal int m_Length;

	public Texture2D Texture { get {
			if(_texture == null && m_Textures != null && m_Textures.Length > 0)
				_texture = m_Textures[0];
			// Backwards compatibility, try and load legacy version
			if(_texture == null && m_Layers != null && m_Layers.Length > 0 && m_Layers[0].IsValid)
				_texture = m_Layers[0].texture;
			if (_texture)
				return _texture;
			return null; 
		} 
		set {
			_texture = value;
		}
	} 

	public Sprite Sprite { get {
			if(!_sprite && m_Sprites != null && m_Sprites.Length > 0)
				_sprite = m_Sprites[0];
			if (_sprite)
				return _sprite;
			return  null;
		}
		set
		{
			_sprite = value;
		}
	}

	#region internal 

	internal DoodleAnimationFileKeyframe(int Length, Texture2D Texture = null, bool CreateSprite = false) {
		m_Length = Length;
		if (Texture) {
			this.Texture = Texture;
			if (CreateSprite)
				this.CreateSprite();
		}
	}

	// TODO: only do this in DrawWindowKeyframe
	internal DoodleAnimationFileKeyframe(Sprite sprite) {
		m_Length = DrawUtils.keyframeLengthForSprite (sprite);
		if (m_Length == -1) 
			m_Length = 1;
		Texture = DrawUtils.GetTextureCopy (sprite.texture, sprite.rect);
	}

	internal Sprite CreateSprite(Vector4 spriteBorder) {
		var s = Sprite.Create(
				Texture, 
				new Rect(0, 0, Texture.width, Texture.height), 
				Vector2.one * .5f, 100, 0,
				SpriteMeshType.FullRect, 
				spriteBorder
		);
		Sprite = s;
		return s;
	}
	internal Sprite CreateSprite() { return CreateSprite(Vector4.zero); }

	#endregion
	
}
}
