using System;
using System.Collections;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.AssetManager;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using Hearthstone.Core;
using UnityEngine;
using UnityEngine.Video;

public class Cinematic : IService, IHasUpdate
{
	private static readonly AssetReference Hearthstone_Tavern_Abridged_Video = new AssetReference("Hearthstone_Tavern_Abridged.mp4:869a4aa0259d3084bae21724d07be067");

	private static readonly AssetReference Hearthstone_Tavern_Abridged_Logo_Video = new AssetReference("Hearthstone_Tavern_Abridged_Logo.mp4:baa9ec8f9f37cfa4bad60ac6525d28ff");

	private static readonly AssetReference Hearthstone_Tavern_Abridged_Audio = new AssetReference("Hearthstone_Tavern_Abridged_Audio.wav:f89a884079f1645598bb19565f5915ef");

	private AssetHandle<AudioClip> m_movieAudio;

	private AudioSource m_audioSource;

	private Camera m_camera;

	private SoundDucker m_soundDucker;

	private bool m_canceled;

	private int m_previousTargetFrameRate;

	private Action m_callback;

	private VideoPlayer m_mainPlayer;

	private VideoPlayer m_logoPlayer;

	private GameObject m_sceneObject;

	private float m_playBeginTime;

	public bool IsPlaying { get; protected set; }

	private GameObject SceneObject
	{
		get
		{
			if (m_sceneObject == null)
			{
				m_sceneObject = new GameObject("CinematicSceneObject", typeof(HSDontDestroyOnLoad));
			}
			return m_sceneObject;
		}
	}

	public IEnumerator<IAsyncJobResult> Initialize(ServiceLocator serviceLocator)
	{
		m_soundDucker = SceneObject.AddComponent<SoundDucker>();
		m_soundDucker.m_GlobalDuckDef = new SoundDuckedCategoryDef();
		m_soundDucker.m_GlobalDuckDef.m_Volume = 0f;
		m_soundDucker.m_GlobalDuckDef.m_RestoreSec = 1.5f;
		m_soundDucker.m_GlobalDuckDef.m_BeginSec = 1.5f;
		yield break;
	}

	public Type[] GetDependencies()
	{
		return null;
	}

	public void Shutdown()
	{
		AssetHandle.SafeDispose(ref m_movieAudio);
	}

	public void Play(Action callback)
	{
		m_callback = callback;
		Options.Get().SetBool(Option.HAS_SEEN_NEW_CINEMATIC, val: true);
		m_canceled = false;
		IsPlaying = true;
		Processor.RunCoroutine(AwaitReadinessThenPlay());
	}

	private VideoPlayer CreatePlayer()
	{
		VideoPlayer videoPlayer = SceneObject.AddComponent<VideoPlayer>();
		videoPlayer.isLooping = false;
		videoPlayer.playOnAwake = false;
		videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
		return videoPlayer;
	}

	private void OnPlayBegin()
	{
		TelemetryManager.Client().SendCinematic(begin: true, -1f);
		m_playBeginTime = Time.realtimeSinceStartup;
		m_previousTargetFrameRate = Application.targetFrameRate;
		Application.targetFrameRate = 0;
		m_mainPlayer = CreatePlayer();
		m_logoPlayer = CreatePlayer();
		m_mainPlayer.renderMode = VideoRenderMode.CameraNearPlane;
		m_logoPlayer.renderMode = VideoRenderMode.CameraFarPlane;
		m_mainPlayer.loopPointReached += OnMainVideoComplete;
		m_logoPlayer.loopPointReached += OnLogoVideoComplete;
		BnetBar.Get().gameObject.SetActive(value: false);
		PegCursor.Get().Hide();
		CreateCamera();
	}

	private void OnPlayEnd(bool canceled)
	{
		Application.targetFrameRate = m_previousTargetFrameRate;
		if (BnetBar.Get() != null)
		{
			BnetBar.Get().gameObject.SetActive(value: true);
			BnetBar.Get().UpdateLayout();
		}
		if (PegCursor.Get() != null)
		{
			PegCursor.Get().Show();
		}
		if (SocialToastMgr.Get() != null)
		{
			SocialToastMgr.Get().Reset();
		}
		if (m_camera != null)
		{
			UnityEngine.Object.Destroy(m_camera.gameObject);
		}
		if (SoundManager.Get() != null)
		{
			SoundManager.Get().Stop(m_audioSource);
		}
		if (m_soundDucker != null)
		{
			m_soundDucker.StopDucking();
		}
		AssetHandle.SafeDispose(ref m_movieAudio);
		if (m_audioSource != null)
		{
			m_audioSource.Stop();
		}
		m_canceled = true;
		IsPlaying = false;
		if (m_callback != null)
		{
			m_callback();
			m_callback = null;
		}
		float duration = -1f;
		if (canceled)
		{
			duration = Time.realtimeSinceStartup - m_playBeginTime;
		}
		TelemetryManager.Client().SendCinematic(begin: false, duration);
		Processor.RunCoroutine(WaitOneFrameThenTeardownPlayer());
	}

	private IEnumerator WaitOneFrameThenTeardownPlayer()
	{
		yield return null;
		UnityEngine.Object.Destroy(m_mainPlayer);
		UnityEngine.Object.Destroy(m_logoPlayer);
	}

	private IEnumerator AwaitReadinessThenPlay()
	{
		OnPlayBegin();
		AssetLoader.Get().LoadAsset<AudioClip>(Hearthstone_Tavern_Abridged_Audio, AudioLoaded);
		if (!AssetLoader.Get().LoadAsset<VideoClip>(Hearthstone_Tavern_Abridged_Video, MovieLoaded))
		{
			MovieLoaded(Hearthstone_Tavern_Abridged_Video, null, null);
		}
		if (!AssetLoader.Get().LoadAsset<VideoClip>(Hearthstone_Tavern_Abridged_Logo_Video, LogoLoaded))
		{
			LogoLoaded(Hearthstone_Tavern_Abridged_Video, null, null);
		}
		if (PlatformSettings.IsMobile())
		{
			while (m_movieAudio == null && !m_canceled)
			{
				yield return null;
			}
		}
		else
		{
			while ((!m_mainPlayer.isPrepared || m_movieAudio == null || !m_logoPlayer.isPrepared) && !m_canceled)
			{
				yield return null;
			}
		}
		if (!m_canceled)
		{
			m_mainPlayer.Play();
			while (!m_canceled && m_mainPlayer.frame < 1)
			{
				yield return null;
			}
		}
		if (!m_canceled)
		{
			m_mainPlayer.targetCamera = m_camera;
			m_logoPlayer.targetCamera = m_camera;
			PlaySound();
		}
	}

	public void Update()
	{
		if (InputCollection.GetAnyKey())
		{
			if (m_audioSource != null && m_audioSource.isPlaying)
			{
				m_audioSource.Stop();
			}
			if (m_mainPlayer != null && m_mainPlayer.isPlaying)
			{
				m_mainPlayer.Stop();
			}
			if (m_logoPlayer != null && m_logoPlayer.isPlaying)
			{
				m_logoPlayer.Stop();
			}
			if (IsPlaying)
			{
				OnPlayEnd(canceled: true);
			}
		}
	}

	private void PlaySound()
	{
		SoundPlayClipArgs args = new SoundPlayClipArgs();
		args.m_forcedAudioClip = m_movieAudio;
		args.m_volume = 1f;
		args.m_pitch = 1f;
		args.m_category = Global.SoundCategory.FX;
		args.m_parentObject = SceneObject;
		m_audioSource = SoundManager.Get().PlayClip(args);
		SoundManager.Get().Set3d(m_audioSource, enable: false);
		SoundManager.Get().SetIgnoreDucking(m_audioSource, enable: true);
		m_soundDucker.StartDucking();
	}

	private void OnMainVideoComplete(VideoPlayer _)
	{
		m_mainPlayer.renderMode = VideoRenderMode.CameraFarPlane;
		m_logoPlayer.renderMode = VideoRenderMode.CameraNearPlane;
		m_logoPlayer.Play();
	}

	private void OnLogoVideoComplete(VideoPlayer _)
	{
		OnPlayEnd(canceled: false);
	}

	private void CreateCamera()
	{
		GameObject cameraGO = new GameObject();
		cameraGO.transform.position = new Vector3(-9997.9f, -9998.9f, -9999.9f);
		m_camera = cameraGO.AddComponent<Camera>();
		m_camera.name = "Cinematic Background Camera";
		m_camera.clearFlags = CameraClearFlags.Color;
		m_camera.backgroundColor = Color.black;
		m_camera.depth = 1000f;
		m_camera.nearClipPlane = 0.01f;
		m_camera.farClipPlane = 0.02f;
		m_camera.allowHDR = false;
	}

	private void AudioLoaded(AssetReference assetRef, AssetHandle<AudioClip> asset, object callbackData)
	{
		using (asset)
		{
			if (asset == null)
			{
				Error.AddDevFatal("Failed to load Cinematic Audio Track!");
			}
			else if (!m_canceled)
			{
				AssetHandle.Set(ref m_movieAudio, asset);
			}
		}
	}

	private void MovieLoaded(AssetReference assetRef, AssetHandle<VideoClip> asset, object callbackData)
	{
		if (asset == null)
		{
			Error.AddDevFatal("Failed to load Cinematic movie!");
			m_canceled = true;
		}
		else if (!m_canceled)
		{
			m_mainPlayer.clip = asset.Asset;
			m_mainPlayer.Prepare();
		}
	}

	private void LogoLoaded(AssetReference assetRef, AssetHandle<VideoClip> asset, object callbackData)
	{
		if (asset == null)
		{
			Error.AddDevFatal("Failed to load Cinematic logo!");
			m_canceled = true;
		}
		else if (!m_canceled)
		{
			m_logoPlayer.clip = asset.Asset;
			m_logoPlayer.Prepare();
		}
	}
}
