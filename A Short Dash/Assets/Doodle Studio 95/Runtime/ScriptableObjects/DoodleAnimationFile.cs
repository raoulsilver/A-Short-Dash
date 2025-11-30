using System.Collections.Generic;
using UnityEngine;

// Lets us access internal from Editor scripts
using System.Runtime.CompilerServices;
[assembly:InternalsVisibleTo("Assembly-CSharp-Editor")]
[assembly:InternalsVisibleTo("DoodleStudio95Editor")]

namespace DoodleStudio95 {
///
///	Stores metadata for an animation
///
[System.Serializable]
public class DoodleAnimationFile : ScriptableObject {

	internal static int SPRITESHEET_MAX_SIZE = 8192;

	public enum PlaybackMode {
		Once,
		Loop,
		LoopBackAndForth,
		SingleFrame
	}

	public enum FramesPerSecond {
		Slow = 6,
		Normal = 8,
		Fast = 16,
	}

  public enum PatternMode { 
		Disabled = 0,
		HorizontalAndVertical = 11,
		Horizontal = 20,
		Vertical = 21
	}

	// [HideInInspector]
	[SerializeField] internal List<DoodleAnimationFileKeyframe> frames = new List<DoodleAnimationFileKeyframe>();

	[SerializeField] internal PlaybackMode playbackMode = PlaybackMode.Loop;
	[SerializeField] internal FramesPerSecond framesPerSecond = FramesPerSecond.Normal;
	[SerializeField] internal Vector4 spriteBorder = Vector4.zero;
	[SerializeField] internal FilterMode filterMode = FilterMode.Trilinear;
	[SerializeField] internal PatternMode patternMode = PatternMode.Disabled;

	[SerializeField] internal bool darkCheckerboard = false;
	[SerializeField] internal string spriteSheetAssetGUID; // deprecated
	[SerializeField] internal List<AudioClip> sounds = new List<AudioClip>();

	[SerializeField] internal int version = 0;

	[System.NonSerialized] internal List<int> _timeline;
	public List<int> Timeline { get { 
		if (_timeline == null || _timeline.Count == 0) {
			_timeline = new List<int>();
			int i = 0;
			foreach (var f in frames) {
				for (var j = 0; j < f.m_Length; ++j)  {
					_timeline.Add(i);
				}
				i++;
			}
		}
		return _timeline;
	}}

	[System.NonSerialized] private int _width = 1;
	[System.NonSerialized] private int _height = 1;
	public int width { get {
		if (_width <= 1 && FirstFrameTexture)
			_width = FirstFrameTexture.width;
		if (_width <= 1) Debug.LogError("Wrong width!");
		return _width;
	}}
	public int height { get {
		if (_height <= 1 && FirstFrameTexture)
			_height = FirstFrameTexture.height;
		return _height;
	}}
	
	internal DoodleAnimationFileKeyframe FirstFrame {
		get {
			if (frames.Count > 0 && frames[0] != null) 
				return frames[0];
			return null;
		}
	}
	internal Texture2D FirstFrameTexture {
		get {
			if (frames.Count > 0 && frames[0] != null) 
				return frames[0].Texture;
			return null;
		}
	}

	// Public
	public int Length { get { return Timeline.Count; } }
	public PlaybackMode PlayBackMode { get { return playbackMode; } }
	public FramesPerSecond FPS { get { return framesPerSecond; } }
	public Vector4 SpriteBorder { get { return spriteBorder; } }
	public FilterMode FilterMode { get { return filterMode ; } }

	public DrawUtils.AtlasInfo GenerateSpritesheet() {
		var rects = new List<Rect>();
		return DrawUtils.CreateAtlas(ref rects, this);
	}

	public DoodleAnimationFileKeyframe GetFrameAt(out int frameI, double Time, float Speed = 1) {
		return GetFrameAt(out frameI, Time, Speed, playbackMode, (int)framesPerSecond);
	}
	public DoodleAnimationFileKeyframe GetFrameAt(out int frameI, double Time, float Speed, PlaybackMode PlaybackMode, int FramesPerSecond) {
		frameI = 0;
		if (frames.Count == 0 || Timeline.Count == 0)
			return null;
		float t = (float)Time * (int)FramesPerSecond * Speed;
		float doubleT = Mathf.Floor(t % (Timeline.Count * 2));
		// if loop, repeat i by number of frames
		if (PlaybackMode == PlaybackMode.Loop) {
			frameI = (int)Mathf.Repeat(t, Timeline.Count);
		} else if (PlaybackMode == PlaybackMode.Once) {
			frameI = (int)Mathf.Clamp(doubleT, 0, Timeline.Count);
		} else if (PlaybackMode == PlaybackMode.LoopBackAndForth) {
			if (doubleT < Timeline.Count) {
				frameI = (int)Mathf.Clamp(doubleT, 0, Timeline.Count);
				//Debug.Log(t + " newT: " + doubleT + ", direction: " + 1 + " frameI " + frameI);
			} else {
				frameI = Mathf.Abs(Timeline.Count * 2 - (int)doubleT);
				//Debug.Log(t + " newT: " + doubleT + ", direction: " + (-1) + " frameI " + frameI);
			}
		}
		frameI = Mathf.Clamp(frameI, 0, Timeline.Count - 1);
		if (Timeline.Count < frameI || frames[Timeline[frameI]] == null)
			return null;
		return frames[Timeline[frameI]];
	}

	public List<DoodleAnimationFileKeyframe> GetFrames() {
		return frames;
	}

	public Texture2D[] GetTextures() {
		Texture2D[] textures = new Texture2D[Timeline.Count];
		for(int i = 0; i < Timeline.Count; i++) {
      textures[i] = frames[Timeline[i]].Texture;
		}
		return textures;
	}

	public AudioClip GetSound(int Index = 0) {
		if (sounds != null && Index < sounds.Count) {
			return sounds[Index];
		}
		return null;
	}

	#region Convert
	// Convert to...

	public GameObject MakeSprite(Vector3 position, Quaternion rotation, Vector3 scale) {
    // Create a sprite
    var spriteObj = new GameObject (name + " Sprite");
    spriteObj.transform.position = position;
    spriteObj.transform.rotation = rotation;
    spriteObj.transform.localScale = scale;
    var renderer = spriteObj.AddComponent<SpriteRenderer> ();
    
    var animator = spriteObj.AddComponent<DoodleAnimator> ();

    animator.ChangeAnimation(this);

		// Call SetFrame() so sprites are assigned, so we can edit the sprite renderer's size right after
		animator.SetFrame();
    if (spriteBorder.magnitude != 0) {
			renderer.size = new Vector2(
				scale.x * (width / (float)animator.PixelsPerUnit), 
				scale.y * (height / (float)animator.PixelsPerUnit)
			);
      renderer.drawMode = SpriteDrawMode.Sliced;
			spriteObj.transform.localScale = Vector3.one;
		} else if (patternMode != PatternMode.Disabled) {
			renderer.size = new Vector2(
				scale.x * (width / (float)animator.PixelsPerUnit), 
				scale.y * (height / (float)animator.PixelsPerUnit)
			);
			renderer.drawMode = SpriteDrawMode.Tiled;
			spriteObj.transform.localScale = Vector3.one;
		}
		
		return spriteObj;
  }
	public GameObject MakeSprite() { return MakeSprite(Vector3.zero, Quaternion.identity, Vector3.one); }

  public GameObject Make3DSprite(Vector3 position, Quaternion rotation, Vector3 scale, Material material) {
		var spriteObj = MakeSprite(position, rotation, scale);
		var renderer = spriteObj.GetComponent<SpriteRenderer>();
		renderer.sharedMaterial = material;
    
    var shc = spriteObj.AddComponent<ShadowCastingSprite>();
    shc.castShadows = UnityEngine.Rendering.ShadowCastingMode.On;
    
		return spriteObj;
  }

  public GameObject MakeUISprite() {
    // Create a sprite
    var spriteObj = new GameObject (name + " Sprite");
    var canvas = GameObject.FindFirstObjectByType<Canvas> ();
    if (!canvas) {
      canvas = new GameObject ("Canvas", new System.Type[] {
        typeof(Canvas),
        typeof(UnityEngine.UI.CanvasScaler),
        typeof(UnityEngine.UI.GraphicRaycaster)
      }).GetComponent<Canvas>();
      if (GameObject.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem> () == null)
        new GameObject ("EventSystem", new System.Type[]{ typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule) });
			canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			canvas.GetComponent<UnityEngine.UI.CanvasScaler>().uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
    }
    spriteObj.transform.SetParent (canvas.transform, true);
		RectTransform rt = spriteObj.AddComponent<RectTransform>();
    rt.localPosition = Vector3.zero;
    rt.localRotation = Quaternion.identity;
    rt.localScale = Vector3.one;
  
    var renderer = spriteObj.AddComponent<UnityEngine.UI.Image> ();
    
    var animator = spriteObj.AddComponent<DoodleAnimator> ();
    animator.ChangeAnimation(this);
    renderer.sprite = animator.Sprites[0];
    if (renderer.sprite.border != Vector4.zero)
      renderer.type = UnityEngine.UI.Image.Type.Sliced;
    renderer.SetNativeSize ();
		return spriteObj;
  }

  public ParticleSystem MakeParticles() {
    var obj = new GameObject (name + " Particles");
    var ps = obj.AddComponent<ParticleSystem> ();
		var s = Shader.Find("Particles/Alpha Blended Premultiply");
		if (s == null)
			s = Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply");
    var m = new Material(s);
    m.name = "Default Particle";
    ps.GetComponent<ParticleSystemRenderer>().sharedMaterial = m;
    var animator = obj.AddComponent<DoodleAnimator> ();
    animator.ChangeAnimation(this);
    // Force refresh of particle textures
    animator.enabled = false;
    animator.enabled = true;
		return ps;
  }
	#endregion
}
}