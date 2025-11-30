using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

namespace DoodleStudio95 {
[ExecuteInEditMode()]
public class DoodleAnimator : MonoBehaviour, ISerializationCallbackReceiver {

	const string PREVIEW_MATERIAL_SUFFIX = " (Doodle)";

	[System.Serializable]
	public struct Settings {
		public bool randomizeOnStart;
		public bool playOnStart;

		public bool overrideFileSettings;
		public DoodleAnimationFile.PlaybackMode customPlaybackMode;
		public int customFramesPerSecond;
		public Vector4 customBorder;
    public float customPixelsPerUnit;
    public FilterMode customFilterMode;
    public TextureWrapMode wrapMode;
		public int startFrame; // Range of animation start 

		public bool useUnscaledTime;

		public static Settings DEFAULT { get { return new Settings() {
				randomizeOnStart = true,
				playOnStart = true,
				overrideFileSettings = false,
				customPlaybackMode = DoodleAnimationFile.PlaybackMode.Loop,
				customFramesPerSecond = (int)DoodleAnimationFile.FramesPerSecond.Normal,
				customBorder = Vector4.zero,
				customPixelsPerUnit = 100,
				customFilterMode = FilterMode.Trilinear,
				wrapMode = TextureWrapMode.Clamp,
				startFrame = 0,
				useUnscaledTime = false
			};
		}}

		internal static Settings FromFile(DoodleAnimationFile file) {
			var s = DEFAULT;
			if (file != null) {
				s.customFramesPerSecond = (int)file.framesPerSecond;
				s.customPlaybackMode = file.playbackMode;
				s.customBorder = file.spriteBorder;
				if (file.FirstFrameTexture) {
					s.customFilterMode = file.FirstFrameTexture.filterMode;
					s.wrapMode = file.FirstFrameTexture.wrapMode;
				}
			}
			return s;
		}
		internal static Settings FromFile(DoodleAnimationFile file, Settings existing) {
			var s = existing;
			if (file != null) {
				s.customFramesPerSecond = (int)file.framesPerSecond;
				s.customPlaybackMode = file.playbackMode;
				s.customBorder = file.spriteBorder;
				if (file.FirstFrameTexture) {
					s.customFilterMode = file.FirstFrameTexture.filterMode;
					s.wrapMode = file.FirstFrameTexture.wrapMode;
				}
			}
			return s;
		}
	}

	#region Legacy code to add backwards compatibility

	[SerializeField, HideInInspector] DoodleAnimationState[] m_States;
	[System.Serializable]
	class DoodleAnimationState {
		public DoodleAnimationFile file = null;
		public float speed = 1.0f;
		public bool randomizeOnStart = false;
		public bool playOnStart = true;

		public bool overrideFileSettings = false;
		public int customPlaybackMode = 0;
		public int customFramesPerSecond = 8;
		public bool customSliced = false;
    public int customPixelsPerUnit = 100;
    public FilterMode filterMode = FilterMode.Trilinear;
    public TextureWrapMode wrapMode = TextureWrapMode.Clamp;
		public int startFrame = 0;
	}

	public void OnBeforeSerialize() { Upgrade(); }
	public void OnAfterDeserialize() { Upgrade(); }
	void Upgrade() {
		if (m_States != null && m_States.Length > 0) {
			var s = m_States[0];
			speed = s.speed;
			m_Settings = new Settings() {
				randomizeOnStart = s.randomizeOnStart,
				playOnStart = s.playOnStart,
				overrideFileSettings = s.overrideFileSettings,
				customPlaybackMode = (DoodleAnimationFile.PlaybackMode)s.customPlaybackMode,
				customFramesPerSecond = s.customFramesPerSecond,
				customBorder = File ? File.spriteBorder : Vector4.zero,
				customPixelsPerUnit = s.customPixelsPerUnit,
				customFilterMode = s.filterMode,
				wrapMode = s.wrapMode,
				startFrame = s.startFrame
			};
			m_File = s.file;
			
			// clean up
			m_States = null;
		}
	}

	#endregion

	#region Runtime

	/// Can it show a preview, does it have any images at all?
	internal bool HasValidImages { get { return m_File != null && m_File.frames.Count > 0; } }

	public int GetFrameAt(double time, bool reverse = false) {
		if (!HasValidImages)
			return 0;
		if (this.PlaybackMode == DoodleAnimationFile.PlaybackMode.SingleFrame) {
			return m_Settings.startFrame;
		}
		int f = 0;
		m_File.GetFrameAt(out f, time, speed, PlaybackMode, FramesPerSecond);
		if (reverse)
			f = Mathf.Abs(m_File.Timeline.Count - f);
		return f;
	}

	public List<DoodleAnimationFileKeyframe> Keyframes { get {
		if (!HasValidImages) 
			return null;
		return File.frames;
	}}

	private List<Sprite> _sprites;
	// Sprites that we created manually, separated so we differentiate them from the ones coming from the file
	private List<Sprite> _generatedSprites = new List<Sprite>(); 
	public List<Sprite> Sprites { get { 
		if (!HasValidImages)
			return null;
		if (_sprites == null || _sprites.Count == 0 || _sprites.Count != Textures.Count) {
			_sprites = new List<Sprite>();
			
			for(int i = 0; i < Keyframes.Count; i++) {
				DoodleAnimationFileKeyframe k = Keyframes[i];
				Sprite s = k.Sprite;

				bool needRuntimeSprite = DoesSpriteNeedRegen(s);
				bool needRuntimeTexture = DoesTextureNeedRegen(k.Texture);

				// Create a runtime sprite only if we really need to
				if (needRuntimeSprite || needRuntimeTexture) {
					s = Sprite.Create(
						Textures[i], 
						new Rect(0, 0, File.width, File.height), 
						Vector2.one * .5f, PixelsPerUnit, 0, 
						SpriteMeshType.FullRect,// Sliced ? SpriteMeshType.FullRect : SpriteMeshType.Tight, 
						SpriteBorder
					);
					s.name = i + " (runtime)";
					s.hideFlags = HideFlags.HideAndDontSave;
					_generatedSprites.Add(s);
				}

				_sprites.Add(s);
			}
		}
		return _sprites;
	}}

	internal List<Texture2D> _textures;
	internal List<Texture2D> _generatedTextures = new List<Texture2D>(); 
	public List<Texture2D> Textures { get { 
		if (!HasValidImages)
			return null;
		if (_textures == null || _textures.Count == 0 || _textures.Count != Keyframes.Count) {
			_textures = new List<Texture2D>();
			
			for(int i = 0; i < Keyframes.Count; i++) {
				DoodleAnimationFileKeyframe k = Keyframes[i];
				Texture2D t = k.Texture;
				Debug.Assert(t != null);

				bool needRuntimeTexture = DoesTextureNeedRegen(t);

				if (needRuntimeTexture) {
					t = DrawUtils.GetTextureCopy(k.Texture);
					t.wrapMode = m_Settings.wrapMode;
					t.filterMode = m_Settings.customFilterMode;
					t.name = i + " (runtime)";
					t.hideFlags = HideFlags.HideAndDontSave;
					_generatedTextures.Add(t);
				}

				_textures.Add(t);
			}
		}
		return _textures;
	}}

	private Texture2D _runtimeSpriteSheet;
	private DrawUtils.AtlasInfo _runtimeSpriteSheetInfo;
	public Texture2D GenerateSpritesheet() { 
		if (_runtimeSpriteSheet == null) {
			var rects = new List<Rect>();
			_runtimeSpriteSheetInfo = DrawUtils.CreateAtlas(ref rects, File);
			_runtimeSpriteSheet = _runtimeSpriteSheetInfo.texture;
		}
		return _runtimeSpriteSheet;
	}
	public DrawUtils.AtlasInfo SpriteSheetInfo { get {
		Debug.Assert(GenerateSpritesheet() != null);
		return _runtimeSpriteSheetInfo;
	}}

	// Returns true if the sprite needs to be regenerated at runtime
	// because of user settings
	bool DoesSpriteNeedRegen(Sprite s) {
		if (s == null)
			return true;
		if (m_Settings.overrideFileSettings) {
			return 
				!s.border.Equals(m_Settings.customBorder) ||
				!Mathf.Approximately(s.pixelsPerUnit, m_Settings.customPixelsPerUnit);
			// s.texture.wrapMode != m_Settings.wrapMode ||
			// s.texture.filterMode != m_Settings.customFilterMode
		}
		return false;
	}
	bool DoesTextureNeedRegen(Texture2D t) {
		if (t == null)
			return true;
		if (m_Settings.overrideFileSettings) {
			return t.wrapMode != m_Settings.wrapMode ||
				t.filterMode != m_Settings.customFilterMode;
		}
		return false;
	}

	internal void UnloadRenderers() {
		if (spriteRenderer)
			spriteRenderer.sprite = null;
		if (genericRenderer != null) 
			genericRenderer.SetPropertyBlock(null);

		if (uiImageRenderer != null) {
			uiImageRenderer.sprite = null;
			uiImageRenderer.SetAllDirty();
		}
		if (uiRawImageRenderer != null) 
			uiRawImageRenderer.texture = null;
	}

	internal void UnloadResources() {
		UnloadRenderers();
		if (_generatedTextures != null) {
			foreach(var t in _generatedTextures)
				DrawUtils.SafeDestroy(t);
		}
		_generatedTextures.Clear();
		if (_generatedSprites != null) {
			foreach(var s in _generatedSprites)
				DrawUtils.SafeDestroy(s);
		}
		_generatedSprites.Clear();
		_sprites = null;
		_textures = null;
		if (_runtimeSpriteSheet != null)
			DrawUtils.SafeDestroy(_runtimeSpriteSheet);
		_runtimeSpriteSheet = null;
	}

	#endregion

	
	#region Getters

	public DoodleAnimationFile File { get { return m_File; } set { ChangeAnimation(value); } }
	public int FramesPerSecond { get { return m_Settings.overrideFileSettings ? m_Settings.customFramesPerSecond : (int)m_File.framesPerSecond; } }
	public DoodleAnimationFile.PlaybackMode PlaybackMode { get { return m_Settings.overrideFileSettings ? m_Settings.customPlaybackMode : m_File.playbackMode; } }
	public bool HasBorder { get { return SpriteBorder.magnitude != 0; } }
	public Vector4 SpriteBorder { get { return m_Settings.overrideFileSettings ? m_Settings.customBorder : m_File.spriteBorder; } }
	public float PixelsPerUnit { get { 
		if (!m_Settings.overrideFileSettings && Sprites != null && Sprites.Count > 0)
			return Sprites[0].pixelsPerUnit;
		
		return m_Settings.customPixelsPerUnit;
	}}
	
	internal float SettingTime { get { 
		return m_Settings.overrideFileSettings && m_Settings.useUnscaledTime ? Time.unscaledTime : Time.time;
	} }
	internal float SettingDeltaTime { get { 
		return m_Settings.overrideFileSettings && m_Settings.useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
	} }

	private List<int> Timeline { get {
		if (m_File == null)
			return null;
		return m_File.Timeline;
	}}

	public int CurrentFrame { get { return Mathf.FloorToInt(m_CurrentFrame); } }
	public float CurrentTimeInSeconds { get { 
		return (m_CurrentFrame * speed) / FramesPerSecond;
	} }
	public float CurrentAnimationLengthInSeconds { get { 
		return ((Timeline.Count - 1) * speed) / FramesPerSecond; 
	} }
  public bool Playing { get { return m_Playing; } }

	#endregion

	[SerializeField] private DoodleAnimationFile m_File;	
	[Range(0,4)] public float speed = 1;
	public Settings m_Settings = Settings.DEFAULT;

	#region Renderers

  [System.NonSerialized, HideInInspector] internal Renderer genericRenderer;
	[System.NonSerialized, HideInInspector] internal SpriteRenderer spriteRenderer;
	[System.NonSerialized, HideInInspector] internal Image uiImageRenderer;
	[System.NonSerialized, HideInInspector] internal RawImage uiRawImageRenderer;
	[System.NonSerialized, HideInInspector] internal AudioSource audioSource;
	[System.NonSerialized, HideInInspector] internal ParticleSystem particles;
	[System.NonSerialized, HideInInspector] internal ParticleSystemRenderer pRenderer;

	#endregion

	#region Private

	float m_CurrentFrame = 0;
	int _startFrame = 0;
	bool m_Playing = false;
	int m_PlaybackDirection = 1;
	MaterialPropertyBlock _propBlock;

  #endregion
	
	#region Unity

	void OnEnable() {
    genericRenderer = GetComponent<Renderer>();
		spriteRenderer = GetComponent<SpriteRenderer>();
		uiImageRenderer = GetComponent<Image>();
		uiRawImageRenderer = GetComponent<RawImage>();
		audioSource = GetComponent<AudioSource>();
		particles = GetComponent<ParticleSystem>();
		
		_propBlock = new MaterialPropertyBlock();

    if (spriteRenderer != null) spriteRenderer.sprite = null;
		if (uiImageRenderer != null) uiImageRenderer.sprite = null;
		if (uiRawImageRenderer != null) uiRawImageRenderer.texture = null;

		UnloadResources();

		if (particles != null) {
			pRenderer = particles.GetComponent<ParticleSystemRenderer>();
			SetupParticles();
		}
		
		// Update preview
		if (!Application.isPlaying) {
			SetFrame(0);	
		} else {
			m_CurrentFrame = 0;
			Stop();
			if (m_Settings.playOnStart) {
				Play();
			} else {
				GoToAndPause(m_Settings.startFrame);
			}
		}
		
	}
	void OnDisable() {
		UnloadResources();
	}

	void OnDestroy() {
		UpdateRenderers();
		UnloadResources();
	}

	void Update() {
		if (Application.isPlaying && Keyframes != null && Timeline.Count > 0) {
			if (m_Playing) {
				m_CurrentFrame += GetSafeDeltaTime() * FramesPerSecond * speed * m_PlaybackDirection;
				bool playheadPassedLength = m_CurrentFrame >= Timeline.Count - 1;
				if (PlaybackMode == DoodleAnimationFile.PlaybackMode.Loop) {
					if (playheadPassedLength) {
						while(m_CurrentFrame >= Timeline.Count - 1) {
							m_CurrentFrame -= Timeline.Count;
						}
						if (m_File.GetSound(0) && audioSource) {
							audioSource.time = Mathf.Clamp(CurrentTimeInSeconds, 0, audioSource.clip.length);
							audioSource.pitch = speed;
							audioSource.Play();
						}
					}
				} else if (PlaybackMode == DoodleAnimationFile.PlaybackMode.LoopBackAndForth) {
					if (playheadPassedLength || CurrentFrame < 0) {
						int prevFrame = CurrentFrame;
						m_PlaybackDirection *= -1;
						m_CurrentFrame += m_PlaybackDirection;
						while(m_CurrentFrame > Timeline.Count) {
							m_CurrentFrame -= Timeline.Count;
						}
						if (m_File.GetSound(0) && audioSource) {
							audioSource.time = Mathf.Clamp(CurrentTimeInSeconds, 0, audioSource.clip.length);
							audioSource.pitch = speed * (float)m_PlaybackDirection;
							audioSource.Play();
						}
					}
				} else if (PlaybackMode == DoodleAnimationFile.PlaybackMode.Once) {
					if (playheadPassedLength) {
							Stop();
					}
				}
				m_CurrentFrame = Mathf.Clamp(m_CurrentFrame, 0, Timeline.Count - 1);
				m_CurrentFrame = GetFrameAt(_startFrame + SettingTime);
				SetFrame();
			}
		}
	}

	#endregion

	// To make sure playback doesn't get screwed up when there's a long delta time
	// eg. a big scene got loaded, we limit how slow the animations can run
	float GetSafeDeltaTime() {
		float minFps = 10;
		return Mathf.Min(1f / minFps, SettingDeltaTime);
	}

	internal void SetupParticles() {
		if (!particles)
			return;
		if (!pRenderer) {
			Debug.LogWarning("No particles renderer");
			return;
		}
		if (!HasValidImages) {
			// pRenderer.GetPropertyBlock(_propBlock);
			// _propBlock.SetTexture("_MainTex", null);
			// pRenderer.SetPropertyBlock(_propBlock);
			return;
		}

		var texture = GenerateSpritesheet();
		
		var atlasInfo = SpriteSheetInfo;
		
		pRenderer.GetPropertyBlock(_propBlock);
		_propBlock.SetTexture("_MainTex", texture);
		pRenderer.SetPropertyBlock(_propBlock);

    var tsa = particles.textureSheetAnimation;
    tsa.enabled = atlasInfo.frames > 1;
    tsa.numTilesX = atlasInfo.framesX;
    tsa.numTilesY = atlasInfo.framesY;
    var fot = tsa.frameOverTime;
    fot.mode = ParticleSystemCurveMode.Curve;
    fot.curve = AnimationCurve.Linear(0,0,1,1);
    tsa.frameOverTime = fot;
    tsa.frameOverTimeMultiplier = (float)atlasInfo.frames / (float)(atlasInfo.framesX * atlasInfo.framesY);
    tsa.cycleCount = 1;
    float lifetime = (1f / (float)FramesPerSecond) * Keyframes.Count; // TODO: use frames length?
   	var m = particles.main;
		 m.startLifetime = lifetime / speed;
	}
	
	internal void UpdateRenderers() {
		// Update sprites when files changedxs
		UnloadResources();
		if (particles != null)
			SetupParticles();
		SetFrame();
	}
	public void SetFrame(int Frame = -1) {
		// if (!enabled)
		// 	return;
		if (!HasValidImages)
			return;
		if (Frame < 0) 
			Frame = CurrentFrame;
		if (Frame > Timeline.Count) {
			Debug.LogErrorFormat(this, "Can't find frame {0}", Frame);
			return;
		}
		Frame = Mathf.Clamp(Frame, 0, Timeline.Count - 1);

		if (!Application.isPlaying) {
			spriteRenderer = GetComponent<SpriteRenderer>();
			uiImageRenderer = GetComponent<Image>();//
			uiRawImageRenderer = GetComponent<RawImage>();//
      genericRenderer = GetComponent<Renderer>();
		}

		Sprite sprite = null;
		Texture2D texture = null;

		if (enabled && HasValidImages) { // Not sure if hiding the sprite when animator is disabled is what we want
			if (Frame < 0) 
				Frame = CurrentFrame;
			if (Frame > Timeline.Count) {
				Debug.LogErrorFormat(this, "Can't find frame {0}", Frame);
				return;//
			}
			Frame = Mathf.Clamp(Frame, 0, Timeline.Count - 1);

			// Only get sprites if we need to (They might get generated and make things slower)
			if (spriteRenderer || uiImageRenderer)
				sprite = Sprites[Timeline[Frame]];

			// Only get textures if we need to (They might get generated and make things slower)
			if (uiImageRenderer || uiRawImageRenderer || (genericRenderer != null && genericRenderer != pRenderer))
				texture = Textures[Timeline[Frame]];
		}

        if (spriteRenderer) {
                var prevSize = spriteRenderer.size;
                spriteRenderer.sprite = sprite;
                spriteRenderer.size = prevSize;
        }
        else if (genericRenderer && genericRenderer != pRenderer)
        {
            genericRenderer.GetPropertyBlock(_propBlock);
            if (texture) {
                _propBlock.SetTexture("_MainTex", texture);
                _propBlock.SetTexture("_BaseMap", texture);
                _propBlock.SetTexture("_EmissionMap", texture);
            }
            genericRenderer.SetPropertyBlock(_propBlock);
        }

		if (uiImageRenderer != null) {
			// uiImageRenderer.sprite = File.FirstFrame != null ? File.FirstFrame.Sprite : null;
			uiImageRenderer.sprite = sprite;
			uiImageRenderer.SetAllDirty();
		}
		if (uiRawImageRenderer != null) {
			uiRawImageRenderer.texture = texture;
		}
	}

	void SetVisible(bool bVisible) {
		if (spriteRenderer)
			spriteRenderer.enabled = bVisible;
		if (uiImageRenderer)
			uiImageRenderer.enabled = bVisible;
		if (uiRawImageRenderer)
			uiRawImageRenderer.enabled = bVisible;
	}

	public void Show() { SetVisible(true); }
	public void Hide() { SetVisible(false); }

	public void ChangeAnimation(DoodleAnimationFile File) { 
		ChangeAnimation(File, Settings.FromFile(File, m_Settings), false);   // TODO: should there be a version of this that doesn't reset settings?
	} // single parameter version for unity UI
	public void ChangeAnimation(DoodleAnimationFile File, Settings settings, bool EnsureAudioSource = false) {
		UnloadResources();
		m_File = File;
		m_Settings = settings;
		if (File == null) {
			UpdateRenderers();
			return;
		}

		if (EnsureAudioSource && File && File.GetSound(0) && !audioSource) {
			audioSource = gameObject.GetComponent<AudioSource>();
			if (!audioSource) audioSource = gameObject.AddComponent<AudioSource>();
			audioSource.clip = File.GetSound(0);
			audioSource.playOnAwake = false;
			audioSource.loop = false;
		}
    if (Application.isPlaying && m_Settings.playOnStart)
	    GoToAndPlay(0);
		else
			GoToAndPause(m_Settings.startFrame);
	}

  public void Play(bool Reverse = false) {
		m_PlaybackDirection = 1;
		if (File == null) {
			// Debug.LogWarning("No animation to play", this);
			return;
		}
		if (PlaybackMode == DoodleAnimationFile.PlaybackMode.SingleFrame) {
			GoToAndPause(m_Settings.startFrame);
			return;
		}
		m_Playing = true;
		_startFrame = m_Settings.randomizeOnStart ? 
      Random.Range(m_Settings.startFrame, Timeline.Count - 1) :
      0;
		m_CurrentFrame = _startFrame;
    if (Reverse)
      m_CurrentFrame = Timeline.Count - 1;
		
		if (audioSource && m_File.GetSound(0)) {
			audioSource.clip = m_File.GetSound(0);
			audioSource.pitch = speed;
			audioSource.loop = false;
			audioSource.time = Mathf.Clamp(CurrentTimeInSeconds, 0, audioSource.clip.length);
			Debug.Log("Time " + audioSource.time + " time in secs " + CurrentTimeInSeconds + " lengtrh " + audioSource.clip.length);
			audioSource.Play();
		}
		Show();
		SetFrame();
	}
	public void Pause() {
		m_Playing = false;
	}
	public void Stop() {
		m_Playing = false;
		Hide();
	}
	public void GoToAndPlay(int Frame = 0) {
		m_CurrentFrame = Frame;
		Play();
	}
	public void GoToAndPause(int Frame = 0) {
		m_CurrentFrame = Frame;
		Show();
		Pause();
		SetFrame();
	}

  public System.Collections.IEnumerator PlayAndPauseAt(int StartFrame = 0, int EndFrame = -1) {
    Stop();
    Show();
    if (StartFrame < 0)
      StartFrame += m_File.Timeline.Count;
    if (EndFrame < 0)
      EndFrame += m_File.Timeline.Count;
    m_CurrentFrame = StartFrame;
    m_PlaybackDirection = EndFrame > StartFrame ? 1 : -1;
    SetFrame();
    while ((int)m_CurrentFrame != (int)EndFrame) {
      m_CurrentFrame += GetSafeDeltaTime() * FramesPerSecond * speed * m_PlaybackDirection;
      SetFrame();
      yield return null;
    }
    yield break;
  }
}
}