using UnityEngine;

namespace DoodleStudio95 {
///
/// A texture that can be serialized/deserialized when, e.g. entering/exiting Play Mode 
///
[System.Serializable]
internal class SerializableTexture2D : ISerializationCallbackReceiver {

  const int THUMBNAIL_SIZE = 150;
  static bool THUMBNAIL_FILTERING = false; // Enable if you want filtering on thumbnails, for whatever reason

  internal string name = "SerializableTexture2D";
	[HideInInspector] private Color32[] _pixels;
  [SerializeField] private byte[] _pixelsCompressed;
	[SerializeField]
	private int _width;
	[SerializeField]
	private int _height;
  [SerializeField]
  private TextureFormat _format;
  [SerializeField]
  private FilterMode _filterMode;

	private Texture2D _texture; // Will be null on deserialization
  private Rect _rect;

  // Backwards Compatibility
  [SerializeField] private byte[] _compressedTexture = null;

  #region static

  internal static SerializableTexture2D New(int Width, int Height, TextureFormat Format = TextureFormat.ARGB32, FilterMode FilterMode = FilterMode.Trilinear, bool FillBuffer = true) {
    //var t = CreateInstance<SerializableTexture2D>();
    var t = new SerializableTexture2D();
    t.Create(Width, Height, Format, FilterMode, FillBuffer);
    return t;
  }
  internal static SerializableTexture2D New(Texture2D SourceTexture) {
    //var t = CreateInstance<SerializableTexture2D>();
    Debug.Assert(SourceTexture != null);
    var t = new SerializableTexture2D();
    t.Create(SourceTexture);
    return t;
  }
  internal static SerializableTexture2D New(SerializableTexture2D SourceTexture) {
   //var t = CreateInstance<SerializableTexture2D>();
    Debug.Assert(SourceTexture != null);
    var t = new SerializableTexture2D();
    t.Create(SourceTexture);
    return t;
  }

  #endregion

  #region Getters

  internal float aspect { get { return (float)height / (float)width; } }
  internal bool IsValid {
    get {
      if (_width <= 0 || _height <= 0)
        return false;
      return true;
    }
  }
  internal Rect rect {
    get {
      if (_rect.width != width || _rect.height != height) {
        _rect = new Rect(0,0,width,height);
      }
      return _rect;
    }
  }
	internal Texture2D texture {
		get {
			if (IsValid && _texture == null) {
        UncompressIfNeeded();
        // Backwards compatibility, load compressed pixels if they exist
        if ((_pixels == null || _pixels.Length == 0) && 
          _compressedTexture != null && _compressedTexture.Length > 0) {
          var tempTex = new Texture2D(_width, _height, _format, false, false);
          tempTex.LoadImage(_compressedTexture);
          tempTex.Apply();
          _pixels = tempTex.GetPixels32();
          DrawUtils.SafeDestroy(tempTex);
        }
        if (_pixels == null || _pixels.Length == 0)
          FillPixelsTransparent(true);
        // Load from serialized pixels
        //Debug.Log("Creating texture2D for " + name + " from stored pixels");
        _texture = new Texture2D(_width, _height, _format, false, false);
        _texture.filterMode = _filterMode;
        UpdateTextureFromBuffer();
        Apply(true);
        return _texture;
			}
      if (_texture == null) {
        Debug.Log("Not a valid texture");
      }
			return _texture;
		}
	}
  internal void EnsurePixelsArray()
  {
    UncompressIfNeeded();
			if ((_pixels == null || _pixels.Length == 0) && texture != null)
				_pixels = texture.GetPixels32 ();
  }
	internal Color32[] pixels {
		get {
      UncompressIfNeeded();
			if ((_pixels == null || _pixels.Length == 0) && texture != null)
				_pixels = texture.GetPixels32 ();
      if (_pixels == null || _pixels.Length == 0) {
        Debug.LogError("Couldn't get pixels");
      }
			return _pixels;
		}
		set {
			_pixels = value;
		}
	}
	internal int width { get { return _width; } }
	internal int height { get { return _height; } }
	internal Vector2 size { get { return new Vector2(width,height); } }
  internal TextureFormat format { get { return _format; } }
  internal FilterMode filterMode { get { return _filterMode; } set { _filterMode = value;} }

  #endregion

  #region serialization

  public void OnBeforeSerialize() {
    _pixelsCompressed = CompressionTools.Compress(CompressionTools.Color32ArrayToByteArray(_pixels));
  }

  bool needsUncompress; // we'll set this to true when unserializing if we need to uncompress data

  void UncompressIfNeeded()
  {
    if (needsUncompress)
    {
      needsUncompress = false;
      if (_pixelsCompressed != null && _pixelsCompressed.Length > 0) {
        CompressionTools.ByteArrayToColor32Array(CompressionTools.Decompress(_pixelsCompressed), ref _pixels);
        _pixelsCompressed = null;
      }
    }
  }

  public void OnAfterDeserialize() {
    if (_pixelsCompressed == null)
      _pixels = null;
    else if (_pixelsCompressed.Length == 0)
      System.Array.Resize(ref _pixels, 0);
    else
    {
      // We'll uncompress later when someone calls .texture or
      // .EnsurePixelArray(); it's an error to touch the data now for a
      // mysterious Unity deserialization reason.
      needsUncompress = true; 
    }
  }

  #endregion

  #region internal

  internal void Create(int Width, int Height, TextureFormat Format = TextureFormat.ARGB32, FilterMode FilterMode = FilterMode.Trilinear, bool FillBuffer = true) {
    if (_texture != null) {
      MonoBehaviour.DestroyImmediate(_texture);
      _texture = null;
    }
    if (Width == 0 || Height == 0) {
      Debug.LogWarningFormat("Wrong size when creating texture! Size:{0}x{1}", Width, Height);
    }
    _width = Mathf.Clamp(Width, 1, int.MaxValue);
    _height = Mathf.Clamp(Height, 1, int.MaxValue);
    _format = Format;
    _filterMode = FilterMode;
    FillPixelsTransparent(FillBuffer);
  }
  void FillPixelsTransparent(bool fill)
  {
    _pixels = new Color32[_width  * _height];
    if (fill) {
      for(int i = 0; i < _pixels.Length; i++) {
        _pixels[i] = DrawUtils.TRANSPARENCY_COLOR;
      }
    }
  }
  internal void Create(Texture2D SourceTexture) {
    if(SourceTexture == null) {
      Debug.LogWarning("Wrong data to create texture");
      return;
    }
    Create(SourceTexture.width, SourceTexture.height, SourceTexture.format, SourceTexture.filterMode, false);
    _pixels = SourceTexture.GetPixels32();
    _texture = DrawUtils.GetTextureCopy(SourceTexture);
  }
  internal void Create(SerializableTexture2D SourceTexture) {
    Create(SourceTexture.width, SourceTexture.height, SourceTexture.format, SourceTexture.filterMode, false);
    //Debug.Log(SourceTexture.pixels.Length + ", ");
    if (SourceTexture.IsValid) {
      if (SourceTexture._pixels != null && SourceTexture._pixels.Length > 0) {
        System.Array.Copy(SourceTexture.pixels, _pixels, _pixels.Length);
      }
    } else {
      Debug.LogWarning("Couldn't copy texture " + SourceTexture.name);
    }
  }

  internal void OnDestroy() {
    if (_texture != null) {
      DrawUtils.SafeDestroy(_texture);
      _texture = null;
    }
  }

	internal Color32 GetPixelFastNoBuffer(int x, int y) {
		return _pixels [y * width + x];
	}

	internal Color32 GetPixelFast(int x, int y) {
		return pixels [y * width + x];
	}

	internal void SetPixelFastNoBuffer(int x, int y, Color32 Color) { 
		_pixels [y * width + x] = Color; 
  }

	internal void SetPixelFast(int x, int y, Color32 Color, bool UpdateFromBuffer = true) { 
		_pixels [y * width + x] = Color; 
		if (UpdateFromBuffer)
			this.UpdateTextureFromBuffer ();
	}

	internal void UpdateTextureFromBuffer() {
		texture.SetPixels32 (pixels);
	}

	internal void Apply(bool UpdateFromBuffer = false) {
		if (UpdateFromBuffer)
			UpdateTextureFromBuffer();
		texture.Apply ();
	}

  // Resize image and store it for preview in the editor, to make large animations faster to preview
  internal SerializableTexture2D GetThumbnail() {
    float destWidth = Mathf.RoundToInt(Mathf.Min(width, THUMBNAIL_SIZE));
    float destHeight = Mathf.RoundToInt(destWidth * ((float)height / (float)width));

    if (destWidth >= width)
      return null;

    var tx = SerializableTexture2D.New((int)destWidth, (int)destHeight, format, FilterMode.Trilinear, false);
    tx.name = "Thumbnail";

    // Average-based resizing from http://blog.collectivemass.com/2014/03/resizing-textures-in-unity/
    int xLength = (int)(destWidth * destHeight);
    Vector2 vPixelSize = new Vector2((float)width / destWidth, (float)height / destHeight);

    //*** Loop through destination pixels and process
    Vector2 vCenter = new Vector2();
    for(int i = 0; i < xLength; i++){
      float xX = (float)i % destWidth;
      float xY = Mathf.Floor((float)i / destWidth);
      
      vCenter.x = (xX / destWidth) * width;
      vCenter.y = (xY / destHeight) * height;
       
      if (THUMBNAIL_FILTERING) {
        // Average filter, slower but nicer
        //*** Calculate grid around point
        int xXFrom = (int)Mathf.Max(Mathf.Floor(vCenter.x - (vPixelSize.x * 0.5f)), 0);
        int xXTo = (int)Mathf.Min(Mathf.Ceil(vCenter.x + (vPixelSize.x * 0.5f)), width);
        int xYFrom = (int)Mathf.Max(Mathf.Floor(vCenter.y - (vPixelSize.y * 0.5f)), 0);
        int xYTo = (int)Mathf.Min(Mathf.Ceil(vCenter.y + (vPixelSize.y * 0.5f)), height);

        //*** Loop and accumulate
        Color oColorTemp = new Color();
        float xGridCount = 0;
        for(int iy = xYFrom; iy < xYTo; iy++){
            for(int ix = xXFrom; ix < xXTo; ix++){
                oColorTemp += GetPixelFast(ix, iy);
                xGridCount++;
            }
        }
        tx.pixels[i] = oColorTemp / (float)xGridCount; // Average Color
      } else {
        // Nearest neighbour 
        vCenter.x = Mathf.Round(vCenter.x);
        vCenter.y = Mathf.Round(vCenter.y);
        int xSourceIndex = (int)((vCenter.y * width) + vCenter.x);
        tx.pixels[i] = pixels[xSourceIndex];
      }
      
    }
    return tx;
  }

  #endregion
}
}