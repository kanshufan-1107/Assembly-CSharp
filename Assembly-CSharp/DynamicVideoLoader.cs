using System.Collections;
using Blizzard.T5.AssetManager;
using Blizzard.T5.MaterialService.Extensions;
using Hearthstone.UI.Core;
using HutongGames.PlayMaker;
using UnityEngine;
using UnityEngine.Video;

public class DynamicVideoLoader : MonoBehaviour
{
	public VideoPlayer VideoPlayer;

	[UnityEngine.Tooltip("Optional texture to display while video is loading for non-mobile devices")]
	public Texture InitialTexture;

	[UnityEngine.Tooltip("Optional texture to display while video is loading for mobile devices")]
	public Texture InitialTexturePhone;

	[UnityEngine.Tooltip("Render texture that the video player draws to.")]
	public Texture VideoPlaybackTexture;

	[UnityEngine.Tooltip("Renderer to display the video and optional textures on.")]
	public Renderer MainDisplayRenderer;

	public PlayMakerFSM OptionalFSM;

	[UnityEngine.Tooltip("If defined, OptionalFSM will be set to this state when the video starts playing.")]
	public string OptionalFSMState = "Playing";

	[UnityEngine.Tooltip("Will automatically trigger the VideoPlayer if it has already configured (e.g. configured -> Ui hidden -> Ui shown)")]
	public bool ReTriggerPlayOnEnable;

	public AudioSource AudioSource;

	public SoundDef SoundDef;

	private string m_currentVideoLocation;

	private string m_currentFallbackTextureLocation;

	private bool m_requiresFallbackTexture;

	private Texture m_fallbackTexture;

	private IEnumerator m_textureSwapCoroutine;

	private bool m_hasConfiguredVideoPlayer;

	[Overridable]
	public string VideoLocation
	{
		get
		{
			return m_currentVideoLocation;
		}
		set
		{
			if (value != m_currentVideoLocation)
			{
				m_currentVideoLocation = value;
				SetInitialTexture();
				if (!string.IsNullOrEmpty(m_currentVideoLocation))
				{
					AssetLoader.Get().LoadAsset<VideoClip>(m_currentVideoLocation, OnVideoLoaded, m_currentVideoLocation);
				}
			}
		}
	}

	[Overridable]
	public string FallbackTextureLocation
	{
		get
		{
			return m_currentFallbackTextureLocation;
		}
		set
		{
			if (value != m_currentFallbackTextureLocation)
			{
				m_currentFallbackTextureLocation = value;
				if (!string.IsNullOrEmpty(m_currentFallbackTextureLocation))
				{
					AssetLoader.Get().LoadAsset<Texture>(m_currentFallbackTextureLocation, OnFallbackTextureLoaded);
				}
			}
		}
	}

	private void OnEnable()
	{
		if (ReTriggerPlayOnEnable && m_hasConfiguredVideoPlayer && !(VideoPlayer == null) && !string.IsNullOrEmpty(m_currentVideoLocation) && !(VideoPlayer.clip == null) && !VideoPlayer.isPlaying)
		{
			VideoPlayer.Play();
		}
	}

	public void OnClosed()
	{
		m_hasConfiguredVideoPlayer = false;
		m_currentVideoLocation = "";
		m_currentFallbackTextureLocation = "";
		m_fallbackTexture = null;
		m_requiresFallbackTexture = false;
		if (VideoPlayer != null)
		{
			VideoPlayer.clip = null;
		}
		if (MainDisplayRenderer != null && VideoPlaybackTexture != null)
		{
			MainDisplayRenderer.GetMaterial().mainTexture = VideoPlaybackTexture;
			RenderTexture playbackAsRT = VideoPlaybackTexture as RenderTexture;
			if (playbackAsRT != null)
			{
				RenderTexture active = RenderTexture.active;
				RenderTexture.active = playbackAsRT;
				GL.Clear(clearDepth: true, clearColor: true, Color.black);
				RenderTexture.active = active;
			}
		}
		if (m_textureSwapCoroutine != null)
		{
			StopCoroutine(m_textureSwapCoroutine);
		}
	}

	private void SetInitialTexture()
	{
		if (VideoPlaybackTexture == null)
		{
			Error.AddDevFatal("Dynamic video loader configured without video playback texture.");
			return;
		}
		if (MainDisplayRenderer == null)
		{
			Error.AddDevFatal("Dynamic video loader configured without main display renderer.");
			return;
		}
		if (PlatformSettings.Screen == ScreenCategory.Phone && InitialTexturePhone != null)
		{
			MainDisplayRenderer.GetMaterial().mainTexture = InitialTexturePhone;
		}
		else if (InitialTexture != null)
		{
			MainDisplayRenderer.GetMaterial().mainTexture = InitialTexture;
		}
		m_hasConfiguredVideoPlayer = false;
	}

	private void OnFallbackTextureLoaded(AssetReference assetRef, AssetHandle<Texture> asset, object callbackData)
	{
		if (string.IsNullOrEmpty(m_currentVideoLocation) || string.IsNullOrEmpty(m_currentFallbackTextureLocation))
		{
			return;
		}
		if (VideoPlayer == null)
		{
			Error.AddDevFatal("Dynamic video loader configured without connection to a video playback component.");
		}
		else if (!(VideoPlayer.clip != null))
		{
			if (VideoPlaybackTexture == null)
			{
				Error.AddDevFatal("Dynamic video loader configured without video playback texture.");
			}
			else if (MainDisplayRenderer == null)
			{
				Error.AddDevFatal("Dynamic video loader configured without main display renderer.");
			}
			else if (m_requiresFallbackTexture)
			{
				MainDisplayRenderer.GetMaterial().mainTexture = asset.Asset;
			}
			else
			{
				m_fallbackTexture = asset.Asset;
			}
		}
	}

	private void OnVideoLoaded(AssetReference assetRef, AssetHandle<VideoClip> asset, object assetId)
	{
		if (string.IsNullOrEmpty(m_currentVideoLocation))
		{
			return;
		}
		if (VideoPlayer == null)
		{
			Error.AddDevFatal("Dynamic video loader configured without connection to a video playback component.");
		}
		else if (VideoPlaybackTexture == null)
		{
			Error.AddDevFatal("Dynamic video loader configured without video playback texture.");
		}
		else if (MainDisplayRenderer == null)
		{
			Error.AddDevFatal("Dynamic video loader configured without main display renderer.");
		}
		else if (asset == null)
		{
			Error.AddDevWarning("Missing Asset", "Dynamic video loader failed to load video at " + assetId.ToString());
			if (m_fallbackTexture != null)
			{
				MainDisplayRenderer.GetMaterial().mainTexture = m_fallbackTexture;
			}
			else
			{
				m_requiresFallbackTexture = true;
			}
		}
		else
		{
			VideoPlayer.clip = asset.Asset;
			VideoPlayer.prepareCompleted += OnReadyToPlay;
			VideoPlayer.Prepare();
		}
	}

	private void OnReadyToPlay(VideoPlayer source)
	{
		if (string.IsNullOrEmpty(m_currentVideoLocation))
		{
			return;
		}
		VideoPlayer.started += ShowVideo;
		SoundManager.Get().RegisterVideoSoundSource(AudioSource, SoundDef);
		VideoPlayer.SetTargetAudioSource(0, AudioSource);
		VideoPlayer.Play();
		if (OptionalFSM != null && !string.IsNullOrEmpty(OptionalFSMState))
		{
			FsmState[] states = OptionalFSM.FsmStates;
			for (int stateIdx = 0; stateIdx < states.Length; stateIdx++)
			{
				if (states[stateIdx].Name == OptionalFSMState)
				{
					OptionalFSM.SetState(OptionalFSMState);
					break;
				}
			}
		}
		VideoPlayer.prepareCompleted -= OnReadyToPlay;
	}

	private void ShowVideo(VideoPlayer player)
	{
		m_textureSwapCoroutine = SwapTexturesWhenReady();
		StartCoroutine(m_textureSwapCoroutine);
		VideoPlayer.started -= ShowVideo;
	}

	private IEnumerator SwapTexturesWhenReady()
	{
		while (VideoPlayer.frame < 1)
		{
			yield return null;
		}
		yield return null;
		MainDisplayRenderer.GetMaterial().mainTexture = VideoPlaybackTexture;
		m_hasConfiguredVideoPlayer = true;
		m_textureSwapCoroutine = null;
	}
}
