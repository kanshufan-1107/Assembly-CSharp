using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Assets;
using Blizzard.T5.AssetManager;
using Blizzard.T5.Core;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using Hearthstone;
using Hearthstone.Core;
using UnityEngine;

public class SoundManager : IService, IHasFixedUpdate, IHasUpdate
{
	public delegate void LoadedCallback(AudioSource source, object userData);

	public enum LimitMaxOutOption
	{
		SKIP_NEWEST,
		STOP_OLDEST
	}

	public class SoundOptions
	{
		public bool InstanceLimited { get; set; }

		public float InstanceTimeLimit { get; set; }

		public int MaxInstancesOfThisSound { get; set; } = 1;

		public LimitMaxOutOption LimitMaxingOutOption { get; set; }

		public bool RandomStartTime { get; set; }
	}

	private class ExtensionMapping
	{
		public AudioSource Source;

		public SourceExtension Extension;
	}

	private class SoundLoadContext
	{
		public GameObject m_parent;

		public float m_volume;

		public SceneMgr.Mode m_sceneMode;

		public bool m_haveCallback;

		public LoadedCallback m_callback;

		public object m_userData;

		public void Init(GameObject parent, float volume, LoadedCallback callback, object userData)
		{
			m_parent = parent;
			m_volume = volume;
			Init(callback, userData);
		}

		public void Init(LoadedCallback callback, object userData)
		{
			m_sceneMode = (ServiceManager.TryGet<SceneMgr>(out var sceneMgr) ? sceneMgr.GetMode() : SceneMgr.Mode.INVALID);
			m_haveCallback = callback != null;
			m_callback = callback;
			m_userData = userData;
		}
	}

	private class SourceExtension
	{
		public int m_id;

		public float m_codeVolume = 1f;

		public float m_sourceVolume = 1f;

		public float m_defVolume = 1f;

		public float m_codePitch = 1f;

		public float m_sourcePitch = 1f;

		public float m_defPitch = 1f;

		public AudioClip m_sourceClip;

		public bool m_paused;

		public bool m_ducking;

		public string m_bundleName;
	}

	private class BundleInfo
	{
		private AssetReference m_assetRef;

		private List<AudioSource> m_refs = new List<AudioSource>();

		private bool m_garbageCollect;

		public string GetAssetRef()
		{
			return m_assetRef;
		}

		public void SetAssetRef(AssetReference assetRef)
		{
			m_assetRef = assetRef;
		}

		public List<AudioSource> GetRefs()
		{
			return m_refs;
		}

		public void AddRef(AudioSource instance)
		{
			m_garbageCollect = false;
			m_refs.Add(instance);
		}

		public bool RemoveRef(AudioSource instance)
		{
			return m_refs.Remove(instance);
		}

		public bool CanGarbageCollect()
		{
			if (!m_garbageCollect)
			{
				return false;
			}
			if (m_refs.Count > 0)
			{
				return false;
			}
			return true;
		}

		public void EnableGarbageCollect(bool enable)
		{
			m_garbageCollect = enable;
		}
	}

	private enum DuckMode
	{
		IDLE,
		BEGINNING,
		HOLD,
		RESTORING
	}

	private class DuckState
	{
		private object m_trigger;

		private Global.SoundCategory m_triggerCategory;

		private SoundDuckedCategoryDef m_duckedDef;

		private DuckMode m_mode;

		private string m_tweenName;

		private float m_volume = 1f;

		public void SetTrigger(object trigger)
		{
			m_trigger = trigger;
			AudioSource sourceTrigger = trigger as AudioSource;
			if (sourceTrigger != null)
			{
				m_triggerCategory = Get().GetCategory(sourceTrigger);
			}
		}

		public bool IsTrigger(object trigger)
		{
			return m_trigger == trigger;
		}

		public bool IsTriggerAlive()
		{
			return GeneralUtils.IsObjectAlive(m_trigger);
		}

		public Global.SoundCategory GetTriggerCategory()
		{
			return m_triggerCategory;
		}

		public SoundDuckedCategoryDef GetDuckedDef()
		{
			return m_duckedDef;
		}

		public void SetDuckedDef(SoundDuckedCategoryDef def)
		{
			m_duckedDef = def;
		}

		public DuckMode GetMode()
		{
			return m_mode;
		}

		public void SetMode(DuckMode mode)
		{
			m_mode = mode;
		}

		public string GetTweenName()
		{
			return m_tweenName;
		}

		public void SetTweenName(string name)
		{
			m_tweenName = name;
		}

		public float GetVolume()
		{
			return m_volume;
		}

		public void SetVolume(float volume)
		{
			m_volume = volume;
		}
	}

	private static SoundManager s_instance;

	private SoundConfig m_config;

	private List<AudioSource> m_generatedSources = new List<AudioSource>();

	private List<ExtensionMapping> m_extensionMappings = new List<ExtensionMapping>();

	private Map<Global.SoundCategory, List<AudioSource>> m_sourcesByCategory = new Map<Global.SoundCategory, List<AudioSource>>();

	private Map<string, List<AudioSource>> m_sourcesByClipName = new Map<string, List<AudioSource>>();

	private Map<string, BundleInfo> m_bundleInfos = new Map<string, BundleInfo>();

	private Map<Global.SoundCategory, List<DuckState>> m_duckStates = new Map<Global.SoundCategory, List<DuckState>>();

	private uint m_nextDuckStateTweenId;

	private List<AudioSource> m_inactiveSources = new List<AudioSource>();

	private List<string> m_bundleInfosToRemove = new List<string>();

	private GameObject m_sceneObject;

	private Map<string, int> activeLimitedSounds = new Map<string, int>();

	private List<MusicTrack> m_musicTracks = new List<MusicTrack>();

	private List<MusicTrack> m_ambienceTracks = new List<MusicTrack>();

	private bool m_musicIsAboutToPlay;

	private bool m_ambienceIsAboutToPlay;

	private AudioSource m_currentMusicTrack;

	private AudioSource m_currentAmbienceTrack;

	private List<AudioSource> m_fadingTracks = new List<AudioSource>();

	private float m_musicTrackStartTime;

	private int m_musicTrackIndex;

	private int m_nextMusicTrackIndex;

	private int m_ambienceTrackIndex;

	private bool m_isMasterEnabled;

	private bool m_isMusicEnabled;

	private bool m_mute;

	private int m_nextSourceId = 1;

	private uint m_frame;

	private List<Coroutine> m_fadingTracksIn = new List<Coroutine>();

	public static readonly AssetReference FallbackSound = new AssetReference("tavern_crowd_play_reaction_very_positive_2.wav:07343a9a2cec38942b8fdbbafa9165d7");

	public Action OnMusicStarted;

	private GameObject SceneObject
	{
		get
		{
			if (m_sceneObject == null)
			{
				m_sceneObject = new GameObject("SoundManagerSceneObject", typeof(HSDontDestroyOnLoad));
			}
			return m_sceneObject;
		}
	}

	public IEnumerator<IAsyncJobResult> Initialize(ServiceLocator serviceLocator)
	{
		InstantiatePrefab instantiateSoundConfigPrefab = new InstantiatePrefab("SoundConfig.prefab:cd41c731c777d4f468b79ffa365a9f94");
		yield return instantiateSoundConfigPrefab;
		SetConfig(instantiateSoundConfigPrefab.InstantiatedPrefab.GetComponent<SoundConfig>());
		SetMonoSoundOption(Options.Get().GetBool(Option.SOUND_MONO_ENABLED));
		Options.Get().RegisterChangedListener(Option.SOUND, OnMasterEnabledOptionChanged);
		Options.Get().RegisterChangedListener(Option.SOUND_VOLUME, OnMasterVolumeOptionChanged);
		Options.Get().RegisterChangedListener(Option.MUSIC, OnEnabledOptionChanged);
		Options.Get().RegisterChangedListener(Option.MUSIC_VOLUME, OnVolumeOptionChanged);
		Options.Get().RegisterChangedListener(Option.DIALOG_VOLUME, OnVolumeOptionChanged);
		Options.Get().RegisterChangedListener(Option.AMBIENCE_VOLUME, OnVolumeOptionChanged);
		Options.Get().RegisterChangedListener(Option.SOUND_EFFECT_VOLUME, OnVolumeOptionChanged);
		Options.Get().RegisterChangedListener(Option.BACKGROUND_SOUND, OnBackgroundSoundOptionChanged);
		Options.Get().RegisterChangedListener(Option.SOUND_MONO_ENABLED, OnMonoSoundOptionChanged);
		m_isMasterEnabled = Options.Get().GetBool(Option.SOUND);
		m_isMusicEnabled = Options.Get().GetBool(Option.MUSIC);
		SetMasterVolumeExponential();
		UpdateAppMute();
		HearthstoneApplication hearthstoneApplication = HearthstoneApplication.Get();
		if (hearthstoneApplication != null)
		{
			hearthstoneApplication.AddFocusChangedListener(OnAppFocusChanged);
		}
		AudioSettings.OnAudioConfigurationChanged += OnAudioConfigurationChanged;
		yield return new ServiceSoftDependency(typeof(SceneMgr), serviceLocator);
		if (serviceLocator.TryGetService<SceneMgr>(out var sceneMgr))
		{
			sceneMgr.RegisterSceneLoadedEvent(OnSceneLoaded);
		}
	}

	public Type[] GetDependencies()
	{
		return new Type[1] { typeof(IAssetLoader) };
	}

	public void Shutdown()
	{
		AudioSettings.OnAudioConfigurationChanged -= OnAudioConfigurationChanged;
		s_instance = null;
	}

	public void Update()
	{
		m_frame = (m_frame + 1) & 0xFFFFFFFFu;
		UpdateMusicAndAmbience();
	}

	public void FixedUpdate()
	{
		UpdateSources();
	}

	public float GetSecondsBetweenUpdates()
	{
		return 1f;
	}

	public static SoundManager Get()
	{
		if (s_instance == null)
		{
			s_instance = ServiceManager.Get<SoundManager>();
		}
		return s_instance;
	}

	public SoundConfig GetConfig()
	{
		return m_config;
	}

	public void SetConfig(SoundConfig config)
	{
		m_config = config;
	}

	public bool IsInitialized()
	{
		return m_config != null;
	}

	public GameObject GetPlaceholderSound()
	{
		AudioSource source = GetPlaceholderSource();
		if (source == null)
		{
			return null;
		}
		return source.gameObject;
	}

	public AudioSource GetPlaceholderSource()
	{
		if (m_config == null)
		{
			return null;
		}
		if (HearthstoneApplication.IsInternal())
		{
			return m_config.m_PlaceholderSound;
		}
		return null;
	}

	public SoundDef GetSoundDef(AudioSource source)
	{
		return source.gameObject.GetComponent<SoundDef>();
	}

	private void SetMasterVolumeExponential()
	{
		AudioListener.volume = Mathf.Pow(Mathf.Clamp01(Options.Get().GetFloat(Option.SOUND_VOLUME)), 1.75f);
	}

	private void SetMonoSoundOption(bool enabled)
	{
		AudioSpeakerMode newSpeakerMode = (enabled ? AudioSpeakerMode.Mono : AudioSpeakerMode.Stereo);
		if (newSpeakerMode == AudioSettings.GetConfiguration().speakerMode)
		{
			return;
		}
		AudioSource[] array = UnityEngine.Object.FindObjectsOfType<AudioSource>();
		List<(AudioSource, float, AudioClip)> sourcesToReplay = new List<(AudioSource, float, AudioClip)>();
		AudioSource[] array2 = array;
		foreach (AudioSource audioSource in array2)
		{
			if (IsPlaying(audioSource))
			{
				sourcesToReplay.Add((audioSource, audioSource.time, audioSource.clip));
			}
		}
		AudioConfiguration audioConfig = AudioSettings.GetConfiguration();
		audioConfig.speakerMode = newSpeakerMode;
		AudioSettings.Reset(audioConfig);
		SetMasterVolumeExponential();
		foreach (var audioSource2 in sourcesToReplay)
		{
			Play(audioSource2.Item1, null, audioSource2.Item3);
			audioSource2.Item1.time = audioSource2.Item2;
		}
	}

	public bool Play(AudioSource source, SoundDef oneShotDef = null, AudioClip oneShotClip = null, SoundOptions options = null)
	{
		return PlayImpl(source, oneShotDef, oneShotClip, options);
	}

	public void RegisterVideoSoundSource(AudioSource source, SoundDef def = null)
	{
		RegisterExtensionForVideoSource(source, def);
	}

	public bool PlayOneShot(AudioSource source, SoundDef oneShotDef, float volume = 1f, SoundOptions options = null)
	{
		if (!PlayImpl(source, oneShotDef, null, options))
		{
			return false;
		}
		if (IsActive(source))
		{
			SetVolume(source, volume);
		}
		return true;
	}

	public bool IsPlaying(AudioSource source)
	{
		if (source == null)
		{
			return false;
		}
		return source.isPlaying;
	}

	public bool Pause(AudioSource source)
	{
		if (source == null)
		{
			return false;
		}
		if (IsPaused(source))
		{
			return false;
		}
		SourceExtension ext = RegisterExtension(source);
		if (ext == null)
		{
			return false;
		}
		ext.m_paused = true;
		UpdateSource(source, ext);
		source.Pause();
		return true;
	}

	public bool IsPaused(AudioSource source)
	{
		if (source == null)
		{
			return false;
		}
		return GetExtension(source)?.m_paused ?? false;
	}

	public bool Stop(AudioSource source)
	{
		if (source == null)
		{
			return false;
		}
		if (!IsActive(source))
		{
			return false;
		}
		source.Stop();
		FinishSource(source);
		return true;
	}

	public void Destroy(AudioSource source)
	{
		if (!(source == null))
		{
			FinishSource(source);
		}
	}

	public void DestroyAll(Global.SoundCategory category)
	{
		List<AudioSource> soundsToDestroy = new List<AudioSource>();
		for (int i = 0; i < m_generatedSources.Count; i++)
		{
			AudioSource genSource = m_generatedSources[i];
			SoundDef soundDef = genSource.GetComponent<SoundDef>();
			if (soundDef.m_Category == category && !soundDef.m_persistPastGameEnd)
			{
				soundsToDestroy.Add(genSource);
			}
		}
		foreach (AudioSource destroyMe in soundsToDestroy)
		{
			Destroy(destroyMe);
		}
	}

	public bool IsActive(AudioSource source)
	{
		if (source == null)
		{
			return false;
		}
		if (IsPlaying(source))
		{
			return true;
		}
		if (IsPaused(source))
		{
			return true;
		}
		return false;
	}

	public bool IsPlaybackFinished(AudioSource source)
	{
		if (source == null || source.clip == null)
		{
			return false;
		}
		return source.timeSamples >= source.clip.samples;
	}

	public float GetVolume(AudioSource source)
	{
		if (source == null)
		{
			return 1f;
		}
		return RegisterExtension(source)?.m_codeVolume ?? 1f;
	}

	public void SetVolume(AudioSource source, float volume)
	{
		if (!(source == null))
		{
			SourceExtension ext = RegisterExtension(source);
			if (ext != null)
			{
				ext.m_codeVolume = volume;
				UpdateVolume(source, ext);
			}
		}
	}

	public void SetPitch(AudioSource source, float pitch)
	{
		if (!(source == null))
		{
			SourceExtension ext = RegisterExtension(source);
			if (ext != null)
			{
				ext.m_codePitch = pitch;
				UpdatePitch(source, ext);
			}
		}
	}

	public Global.SoundCategory GetCategory(AudioSource source)
	{
		if (source == null)
		{
			return Global.SoundCategory.NONE;
		}
		return GetDefFromSource(source).m_Category;
	}

	public void Set3d(AudioSource source, bool enable)
	{
		if (!(source == null))
		{
			source.spatialBlend = (enable ? 1f : 0f);
		}
	}

	public AudioSource GetCurrentMusicTrack()
	{
		return m_currentMusicTrack;
	}

	public bool Load(AssetReference assetRef)
	{
		if (!AssetLoader.Get().IsAssetAvailable(assetRef))
		{
			return false;
		}
		return SoundLoader.LoadSound(assetRef, OnLoadSoundLoaded);
	}

	public void LoadAndPlay(AssetReference assetRef)
	{
		LoadAndPlay(assetRef, null, 1f, null, null);
	}

	public void LoadAndPlay(AssetReference assetRef, GameObject parent)
	{
		LoadAndPlay(assetRef, parent, 1f, null, null);
	}

	public void LoadAndPlay(AssetReference assetRef, GameObject parent, float volume)
	{
		LoadAndPlay(assetRef, parent, volume, null, null);
	}

	public void LoadAndPlay(AssetReference assetRef, GameObject parent, float volume, LoadedCallback callback)
	{
		LoadAndPlay(assetRef, parent, volume, callback, null);
	}

	public void LoadAndPlay(AssetReference assetRef, GameObject parent, float volume, LoadedCallback callback, object callbackData)
	{
		if (string.IsNullOrEmpty(assetRef))
		{
			Log.Sound.PrintWarning("Missing assetref for LoadAndPlay().");
			callback?.Invoke(null, callbackData);
		}
		else
		{
			SoundLoadContext context = new SoundLoadContext();
			context.Init(parent, volume, callback, callbackData);
			SoundLoader.LoadSound(assetRef, OnLoadAndPlaySoundLoaded, context, GetPlaceholderSound());
		}
	}

	public void PlayPreloaded(AudioSource source)
	{
		PlayPreloaded(source, null);
	}

	public void PlayPreloaded(AudioSource source, float volume)
	{
		PlayPreloaded(source, null, volume);
	}

	public void PlayPreloaded(AudioSource source, GameObject parentObject)
	{
		PlayPreloaded(source, parentObject, 1f);
	}

	public void PlayPreloaded(AudioSource source, GameObject parentObject, float volume)
	{
		if (source == null)
		{
			Debug.LogError("Preloaded audio source is null! Cannot play!");
			return;
		}
		SourceExtension ext = RegisterExtension(source);
		if (ext != null)
		{
			ext.m_codeVolume = volume;
		}
		InitSourceTransform(source, parentObject);
		m_generatedSources.Add(source);
		Play(source);
	}

	public AudioSource PlayClip(SoundPlayClipArgs args, bool createNewSource = true, SoundOptions options = null)
	{
		if (args == null || (args.m_def == null && args.m_forcedAudioClip == null))
		{
			Debug.LogWarningFormat("PlayClip: using placeholder sound for audio clip: {0}", (args != null) ? args.ToString() : "");
			return PlayImpl(null, null);
		}
		AudioSource source = null;
		if (createNewSource)
		{
			source = GenerateAudioSource(args.m_templateSource, args.m_def);
		}
		else
		{
			source = args.m_def.GetComponent<AudioSource>();
			if (source != null)
			{
				m_generatedSources.Add(source);
			}
			else
			{
				Log.Asset.PrintWarning("PlayClip: Loaded sound asset missing AudioSource. Generating new one...");
				source = GenerateAudioSource(args.m_templateSource, args.m_def);
			}
		}
		if (args.m_forcedAudioClip != null)
		{
			source.clip = args.m_forcedAudioClip;
		}
		if (args.m_volume.HasValue)
		{
			source.volume = args.m_volume.Value;
		}
		if (args.m_pitch.HasValue)
		{
			source.pitch = args.m_pitch.Value;
		}
		if (args.m_spatialBlend.HasValue)
		{
			source.spatialBlend = args.m_spatialBlend.Value;
		}
		if (args.m_category.HasValue)
		{
			source.GetComponent<SoundDef>().m_Category = args.m_category.Value;
		}
		InitSourceTransform(source, args.m_parentObject);
		if (args.m_forcedAudioClip != null)
		{
			if (Play(source, null, args.m_forcedAudioClip))
			{
				return source;
			}
		}
		else if (Play(source, args.m_def, null, options))
		{
			return source;
		}
		FinishGeneratedSource(source);
		return null;
	}

	public bool LoadAndPlayClip(AssetReference assetRef, SoundPlayClipArgs args)
	{
		if (string.IsNullOrEmpty(assetRef))
		{
			Log.Sound.PrintError("LoadAndPlayClip: Missing asset AssetReference!");
			return false;
		}
		if (!AssetLoader.Get().IsAssetAvailable(assetRef))
		{
			return false;
		}
		if (args == null)
		{
			Log.Sound.PrintWarning("LoadAndPlayClip: Missing SoundPlayClipArgs. Using default...");
			args = new SoundPlayClipArgs
			{
				m_category = Global.SoundCategory.FX
			};
		}
		return SoundLoader.LoadSound(assetRef, OnLoadAndPlayClipLoaded, args);
	}

	private void OnLoadSoundLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		if (assetRef == null)
		{
			Debug.LogErrorFormat("SoundManager.OnLoadSoundLoaded() - ERROR Tried to load null assetRef!", assetRef, go);
			return;
		}
		if (go == null)
		{
			Debug.LogErrorFormat("SoundManager.OnLoadSoundLoaded() - ERROR assetRef=\"{0}\" go=\"{1}\" failed to load", assetRef, go);
			return;
		}
		AudioSource source = go.GetComponent<AudioSource>();
		if (source == null)
		{
			UnityEngine.Object.DestroyImmediate(go);
			Debug.LogErrorFormat("SoundManager.OnLoadSoundLoaded() - ERROR assetRef=\"{0}\" has no AudioSource", assetRef);
			return;
		}
		RegisterSourceBundle(assetRef, source);
		source.volume = 0f;
		source.Play();
		source.Stop();
		UnregisterSourceBundle(assetRef.ToString(), source);
		UnityEngine.Object.DestroyImmediate(source.gameObject);
	}

	private void OnLoadAndPlaySoundLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		if (assetRef == null)
		{
			Log.Sound.PrintError("SoundManager.OnLoadAndPlaySoundLoaded() - ERROR Tried to load null assetRef!");
			return;
		}
		if (go == null)
		{
			Log.Sound.PrintError($"SoundManager.OnLoadAndPlaySoundLoaded() - ERROR \"{assetRef}\" failed to load");
			return;
		}
		AudioSource source = go.GetComponent<AudioSource>();
		if (source == null)
		{
			UnityEngine.Object.DestroyImmediate(go);
			Log.Sound.PrintError($"SoundManager.OnLoadAndPlaySoundLoaded() - ERROR \"{assetRef}\" has no AudioSource");
			return;
		}
		SoundLoadContext context = (SoundLoadContext)callbackData;
		if (context.m_sceneMode != SceneMgr.Mode.FATAL_ERROR && SceneMgr.Get() != null && SceneMgr.Get().IsModeRequested(SceneMgr.Mode.FATAL_ERROR))
		{
			UnityEngine.Object.DestroyImmediate(go);
			return;
		}
		if (RegisterSourceBundle(assetRef, source) == null)
		{
			Log.Sound.PrintWarning($"Failed to load and play sound name={assetRef}, go={go.name} (this may be due to it not yet being downloaded)");
			return;
		}
		if (context.m_haveCallback && !GeneralUtils.IsCallbackValid(context.m_callback))
		{
			UnityEngine.Object.DestroyImmediate(go);
			UnregisterSourceBundle(SceneObject.name, source);
			return;
		}
		m_generatedSources.Add(source);
		RegisterExtension(source).m_codeVolume = context.m_volume;
		InitSourceTransform(source, context.m_parent);
		Play(source);
		if (context.m_callback != null)
		{
			context.m_callback(source, context.m_userData);
		}
	}

	private void OnLoadAndPlayClipLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		if (assetRef == null)
		{
			Log.Sound.PrintError("SoundManager.OnLoadAndPlayClipLoaded() - ERROR Tried to load null assetRef!");
			return;
		}
		if (go == null)
		{
			Log.Sound.PrintError("LoadAndPlayClip: Sound asset \"{0}\" failed to load", assetRef);
			return;
		}
		SoundDef def = go.GetComponent<SoundDef>();
		if (def == null)
		{
			Log.Sound.PrintError("LoadAndPlayClip: SoundDef missing from asset! Aborting playing \"{0}\"", assetRef);
			UnityEngine.Object.DestroyImmediate(go);
		}
		else
		{
			SoundPlayClipArgs args = (SoundPlayClipArgs)callbackData;
			args.m_def = def;
			PlayClip(args, createNewSource: false);
		}
	}

	public void AddMusicTracks(List<MusicTrack> tracks)
	{
		AddTracks(tracks, m_musicTracks);
	}

	public void AddAmbienceTracks(List<MusicTrack> tracks)
	{
		AddTracks(tracks, m_ambienceTracks);
	}

	public List<MusicTrack> GetCurrentMusicTracks()
	{
		return m_musicTracks;
	}

	public List<MusicTrack> GetCurrentAmbienceTracks()
	{
		return m_ambienceTracks;
	}

	public int GetCurrentMusicTrackIndex()
	{
		return m_musicTrackIndex;
	}

	public void SetCurrentMusicTrackIndex(int idx)
	{
		if (m_musicTrackIndex != idx)
		{
			m_musicIsAboutToPlay = PlayMusicTrack(idx);
		}
	}

	public void SetCurrentMusicTrackTime(float time)
	{
		if ((bool)m_currentMusicTrack)
		{
			m_currentMusicTrack.time = time;
		}
		else
		{
			m_musicTrackStartTime = time;
		}
	}

	public void StopCurrentMusicTrack()
	{
		if (m_currentMusicTrack != null)
		{
			FadeTrackOut(m_currentMusicTrack);
			ChangeCurrentMusicTrack(null);
		}
	}

	public void StopCurrentAmbienceTrack()
	{
		if (m_currentAmbienceTrack != null)
		{
			FadeTrackOut(m_currentAmbienceTrack);
			ChangeCurrentAmbienceTrack(null);
		}
	}

	public void NukeMusicAndAmbiencePlaylists()
	{
		m_musicTracks.Clear();
		m_ambienceTracks.Clear();
		m_nextMusicTrackIndex = 0;
		m_musicTrackIndex = 0;
		m_ambienceTrackIndex = 0;
	}

	public void NukePlaylistsAndStopPlayingCurrentTracks()
	{
		NukeMusicAndAmbiencePlaylists();
		StopCurrentMusicTrack();
		StopCurrentAmbienceTrack();
	}

	public void NukeMusicAndStopPlayingCurrentTrack()
	{
		m_musicTracks.Clear();
		m_nextMusicTrackIndex = 0;
		m_musicTrackIndex = 0;
		StopCurrentMusicTrack();
	}

	public void NukeAmbienceAndStopPlayingCurrentTrack()
	{
		m_ambienceTracks.Clear();
		m_ambienceTrackIndex = 0;
		StopCurrentAmbienceTrack();
	}

	public void ImmediatelyKillMusicAndAmbience()
	{
		NukeMusicAndAmbiencePlaylists();
		FinishAllFadingTracks();
		FinishMusicAndAmbiance();
	}

	private void FinishMusicAndAmbiance()
	{
		if (m_currentMusicTrack != null)
		{
			FinishSource(m_currentMusicTrack);
			ChangeCurrentMusicTrack(null);
		}
		if (m_currentAmbienceTrack != null)
		{
			FinishSource(m_currentAmbienceTrack);
			ChangeCurrentAmbienceTrack(null);
		}
	}

	private void FinishAllFadingTracks()
	{
		AudioSource[] array = m_fadingTracks.ToArray();
		foreach (AudioSource track in array)
		{
			if (!(track == null))
			{
				FinishSource(track);
			}
		}
		m_fadingTracks.Clear();
	}

	private void AddTracks(List<MusicTrack> sourceTracks, List<MusicTrack> destTracks)
	{
		foreach (MusicTrack track in sourceTracks)
		{
			destTracks.Add(track);
		}
	}

	private void OnMusicLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		if (assetRef == null)
		{
			Log.Sound.PrintError("SoundManager.OnMusicLoaded() - ERROR Tried to load null assetRef!");
			return;
		}
		if (go == null)
		{
			Log.Sound.PrintError($"SoundManager.OnMusicLoaded() - ERROR \"{assetRef}\" failed to load");
			return;
		}
		AudioSource source = go.GetComponent<AudioSource>();
		if (source == null)
		{
			Log.Sound.PrintError("SoundManager.OnMusicLoaded() - ERROR \"" + SceneObject.name + "\" has no AudioSource");
			return;
		}
		RegisterSourceBundle(assetRef, source);
		MusicTrack track = (MusicTrack)callbackData;
		if (m_musicTrackIndex >= m_musicTracks.Count || m_musicTracks[m_musicTrackIndex] != track)
		{
			UnregisterSourceBundle(assetRef, source);
			UnityEngine.Object.DestroyImmediate(go);
		}
		else
		{
			m_generatedSources.Add(source);
			source.transform.parent = SceneObject.transform;
			source.volume *= track.m_volume;
			source.time = m_musicTrackStartTime;
			m_musicTrackStartTime = 0f;
			ChangeCurrentMusicTrack(source);
			Play(source);
			if (OnMusicStarted != null)
			{
				OnMusicStarted();
			}
		}
		m_musicIsAboutToPlay = false;
	}

	private void OnAmbienceLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		if (assetRef == null)
		{
			Log.Sound.PrintError("SoundManager.OnAmbienceLoaded() - ERROR Tried to load null assetRef!");
			return;
		}
		if (go == null)
		{
			Log.Sound.PrintError($"SoundManager.OnAmbienceLoaded() - ERROR \"{assetRef}\" failed to load");
			return;
		}
		AudioSource source = go.GetComponent<AudioSource>();
		if (source == null)
		{
			Log.Sound.PrintError("SoundManager.OnAmbienceLoaded() - ERROR \"" + SceneObject.name + "\" has no AudioSource");
			return;
		}
		RegisterSourceBundle(assetRef, source);
		MusicTrack track = (MusicTrack)callbackData;
		if (!m_ambienceTracks.Contains(track))
		{
			UnregisterSourceBundle(assetRef, source);
			UnityEngine.Object.DestroyImmediate(go);
		}
		else
		{
			m_generatedSources.Add(source);
			source.transform.parent = SceneObject.transform;
			source.volume *= track.m_volume;
			ChangeCurrentAmbienceTrack(source);
			m_fadingTracksIn.Add(Processor.RunCoroutine(FadeTrackIn(source)));
			Play(source);
		}
		m_ambienceIsAboutToPlay = false;
	}

	private void ChangeCurrentMusicTrack(AudioSource source)
	{
		m_currentMusicTrack = source;
	}

	private void ChangeCurrentAmbienceTrack(AudioSource source)
	{
		m_currentAmbienceTrack = source;
	}

	private void UpdateMusicAndAmbience()
	{
		if (!IsMusicEnabled())
		{
			return;
		}
		if (!m_musicIsAboutToPlay)
		{
			if (m_currentMusicTrack != null)
			{
				if (!IsPlaying(m_currentMusicTrack))
				{
					Processor.RunCoroutine(PlayMusicInSeconds(m_config.m_SecondsBetweenMusicTracks));
				}
			}
			else
			{
				m_musicIsAboutToPlay = PlayNextMusic();
			}
		}
		if (m_ambienceIsAboutToPlay)
		{
			return;
		}
		if (m_currentAmbienceTrack != null)
		{
			if (!IsPlaying(m_currentAmbienceTrack))
			{
				Processor.RunCoroutine(PlayAmbienceInSeconds(0f));
			}
		}
		else
		{
			m_ambienceIsAboutToPlay = PlayNextAmbience();
		}
	}

	private IEnumerator PlayMusicInSeconds(float seconds)
	{
		m_musicIsAboutToPlay = true;
		yield return new WaitForSeconds(seconds);
		m_musicIsAboutToPlay = PlayNextMusic();
	}

	private bool PlayNextMusic()
	{
		if (!IsMusicEnabled())
		{
			return false;
		}
		if (m_musicTracks.Count <= 0)
		{
			return false;
		}
		return PlayMusicTrack(m_nextMusicTrackIndex);
	}

	private bool PlayMusicTrack(int index)
	{
		if (index < 0 || index >= m_musicTracks.Count)
		{
			return false;
		}
		m_musicTrackIndex = index;
		MusicTrack track = m_musicTracks[m_musicTrackIndex];
		m_nextMusicTrackIndex = (index + 1) % m_musicTracks.Count;
		if (track == null)
		{
			return false;
		}
		if (m_currentMusicTrack != null)
		{
			FadeTrackOut(m_currentMusicTrack);
			ChangeCurrentMusicTrack(null);
		}
		return SoundLoader.LoadSound(AssetLoader.Get().IsAssetAvailable(track.m_name) ? track.m_name : track.m_fallback, OnMusicLoaded, track, GetPlaceholderSound());
	}

	private bool IsMusicEnabled()
	{
		if (!SoundUtils.IsDeviceBackgroundMusicPlaying() && m_isMasterEnabled)
		{
			return m_isMusicEnabled;
		}
		return false;
	}

	private IEnumerator PlayAmbienceInSeconds(float seconds)
	{
		m_ambienceIsAboutToPlay = true;
		yield return new WaitForSeconds(seconds);
		m_ambienceIsAboutToPlay = PlayNextAmbience();
	}

	private bool PlayNextAmbience()
	{
		if (!IsMusicEnabled())
		{
			return false;
		}
		if (m_ambienceTracks.Count <= 0)
		{
			return false;
		}
		MusicTrack track = m_ambienceTracks[m_ambienceTrackIndex];
		m_ambienceTrackIndex = (m_ambienceTrackIndex + 1) % m_ambienceTracks.Count;
		if (track == null)
		{
			return false;
		}
		string trackAssetPath = (AssetLoader.Get().IsAssetAvailable(track.m_name) ? track.m_name : track.m_fallback);
		foreach (Coroutine fadeTrackCoroutine in m_fadingTracksIn)
		{
			if (fadeTrackCoroutine != null)
			{
				Processor.CancelCoroutine(fadeTrackCoroutine);
			}
		}
		m_fadingTracksIn.Clear();
		return SoundLoader.LoadSound(trackAssetPath, OnAmbienceLoaded, track, GetPlaceholderSound());
	}

	private void FadeTrackOut(AudioSource source)
	{
		FadeTrackOut(source, 1f);
	}

	public void FadeTrackOut(AudioSource source, float duration)
	{
		if (!IsActive(source))
		{
			FinishSource(source);
		}
		else
		{
			Processor.RunCoroutine(FadeTrack(source, 0f, duration));
		}
	}

	private IEnumerator FadeTrackIn(AudioSource source)
	{
		SourceExtension ext = GetExtension(source);
		if (ext == null)
		{
			Log.Sound.PrintWarning("Unable to find extension for sound {0}", source.name);
			yield break;
		}
		float targetVolume = GetVolume(source);
		float currTime = 0f;
		float targetVolumeTime = 1f;
		ext.m_codeVolume = 0f;
		UpdateVolume(source, ext);
		while (ext.m_codeVolume < targetVolume)
		{
			currTime += Time.deltaTime;
			ext.m_codeVolume = Mathf.Lerp(0f, targetVolume, Mathf.Clamp01(currTime / targetVolumeTime));
			UpdateVolume(source, ext);
			yield return null;
			if (source == null || !IsActive(source))
			{
				break;
			}
		}
	}

	private IEnumerator FadeTrack(AudioSource source, float targetVolume, float duration)
	{
		m_fadingTracks.Add(source);
		SourceExtension ext = GetExtension(source);
		while (!Mathf.Approximately(ext.m_codeVolume, targetVolume))
		{
			ext.m_codeVolume = Mathf.Lerp(ext.m_codeVolume, targetVolume, Time.deltaTime / duration);
			UpdateVolume(source, ext);
			yield return null;
			if (source == null || !IsActive(source))
			{
				yield break;
			}
		}
		FinishSource(source);
	}

	private SourceExtension RegisterExtension(AudioSource source, SoundDef oneShotDef = null, AudioClip oneShotClip = null, bool aboutToPlay = false)
	{
		SoundDef def = oneShotDef;
		if (def == null)
		{
			def = GetDefFromSource(source);
		}
		SourceExtension ext = GetExtension(source);
		if (ext == null)
		{
			AssetHandle<AudioClip> loadedClip = null;
			AudioClip clip = ((oneShotClip == null) ? LoadClipForPlayback(ref loadedClip, source, def) : oneShotClip);
			ServiceManager.Get<DisposablesCleaner>()?.Attach(source, loadedClip);
			if (clip == null || (aboutToPlay && ProcessClipLimits(clip)))
			{
				return null;
			}
			ext = RegisterSourceExtensionCommon(source, def);
			ext.m_sourceClip = source.clip;
			InitNewClipOnSource(source, def, ext, clip);
		}
		else if (aboutToPlay)
		{
			AudioClip clip2;
			if (oneShotClip == null)
			{
				AssetHandle<AudioClip> loadedClip2 = null;
				clip2 = LoadClipForPlayback(ref loadedClip2, source, def);
				ServiceManager.Get<DisposablesCleaner>()?.Attach(source, loadedClip2);
			}
			else
			{
				clip2 = oneShotClip;
			}
			if (!CanPlayClipOnExistingSource(source, clip2))
			{
				if (IsActive(source))
				{
					Stop(source);
				}
				else
				{
					FinishSource(source);
				}
				return null;
			}
			if (source.clip != clip2)
			{
				if (source.clip != null)
				{
					UnregisterSourceByClip(source);
				}
				InitNewClipOnSource(source, def, ext, clip2);
			}
		}
		return ext;
	}

	private SourceExtension RegisterExtensionForVideoSource(AudioSource source, SoundDef def = null)
	{
		if (def == null)
		{
			def = GetDefFromSource(source);
		}
		return RegisterSourceExtensionCommon(source, def);
	}

	private SourceExtension RegisterSourceExtensionCommon(AudioSource source, SoundDef def)
	{
		SourceExtension ext = new SourceExtension();
		ext.m_sourceVolume = source.volume;
		ext.m_sourcePitch = source.pitch;
		ext.m_id = GetNextSourceId();
		AddExtensionMapping(source, ext);
		Global.SoundCategory cat = GetCategory(source);
		if (cat == Global.SoundCategory.NONE)
		{
			cat = def.m_Category;
		}
		RegisterSourceByCategory(source, cat);
		ext.m_defVolume = SoundUtils.GetRandomVolumeFromDef(def);
		ext.m_defPitch = SoundUtils.GetRandomPitchFromDef(def);
		return ext;
	}

	public AudioClip LoadClipForPlayback(ref AssetHandle<AudioClip> clipHandle, AudioSource source, SoundDef oneShotDef)
	{
		string clipAsset = null;
		SoundDef def = oneShotDef;
		if (oneShotDef == null)
		{
			def = source.GetComponent<SoundDef>();
		}
		if (def != null)
		{
			clipAsset = SoundUtils.GetRandomClipFromDef(def);
			if (clipAsset == null)
			{
				clipAsset = def.m_AudioClip;
				if (string.IsNullOrEmpty(clipAsset))
				{
					if (source.clip != null)
					{
						return source.clip;
					}
					string sourcePath = "";
					if (HearthstoneApplication.IsInternal())
					{
						sourcePath = " " + DebugUtils.GetHierarchyPathAndType(source);
					}
					Error.AddDevFatal("{0} has no AudioClip. Top-level parent is {1}{2}.", source.gameObject.name, GameObjectUtils.FindTopParent(source), sourcePath);
					return null;
				}
			}
		}
		if (clipAsset == null || def == null)
		{
			Error.AddDevFatal("DetermineClipForPlayback: failed to GET AudioClip clipAsset={0}, gameObject={2}, soundDef={3}", clipAsset, source.gameObject.name, def);
			return null;
		}
		SoundLoader.LoadAudioClipWithFallback(ref clipHandle, source, clipAsset);
		return clipHandle?.Asset;
	}

	private bool CanPlayClipOnExistingSource(AudioSource source, AudioClip clip)
	{
		if (clip == null)
		{
			return false;
		}
		if ((!IsActive(source) || source.clip != clip) && ProcessClipLimits(clip))
		{
			return false;
		}
		return true;
	}

	private void InitNewClipOnSource(AudioSource source, SoundDef def, SourceExtension ext, AudioClip clip)
	{
		ext.m_defVolume = SoundUtils.GetRandomVolumeFromDef(def);
		ext.m_defPitch = SoundUtils.GetRandomPitchFromDef(def);
		source.clip = clip;
		RegisterSourceByClip(source, clip);
	}

	private void UnregisterExtension(AudioSource source, SourceExtension ext)
	{
		source.volume = ext.m_sourceVolume;
		source.pitch = ext.m_sourcePitch;
		source.clip = ext.m_sourceClip;
		RemoveExtensionMapping(source);
	}

	private void UpdateSource(AudioSource source, SourceExtension ext)
	{
		UpdateMute(source);
		UpdateVolume(source, ext);
		UpdatePitch(source, ext);
	}

	private void UpdateMute(AudioSource source)
	{
		bool categoryEnabled = IsCategoryEnabled(source);
		UpdateMute(source, categoryEnabled);
	}

	private void UpdateMute(AudioSource source, bool categoryEnabled)
	{
		source.mute = m_mute || !categoryEnabled;
	}

	private void UpdateCategoryMute(Global.SoundCategory cat)
	{
		if (m_sourcesByCategory.TryGetValue(cat, out var sources))
		{
			bool categoryEnabled = IsCategoryEnabled(cat);
			for (int i = 0; i < sources.Count; i++)
			{
				AudioSource source = sources[i];
				UpdateMute(source, categoryEnabled);
			}
		}
	}

	private void UpdateAllMutes()
	{
		foreach (ExtensionMapping mapping in m_extensionMappings)
		{
			UpdateMute(mapping.Source);
		}
	}

	private void UpdateVolume(AudioSource source, SourceExtension ext)
	{
		float categoryVolume = GetCategoryVolume(source);
		float duckingVolume = GetDuckingVolume(source);
		UpdateVolume(source, ext, categoryVolume, duckingVolume);
	}

	private void UpdateVolume(AudioSource source, SourceExtension ext, float categoryVolume, float duckingVolume)
	{
		source.volume = ext.m_codeVolume * ext.m_sourceVolume * ext.m_defVolume * categoryVolume * duckingVolume;
	}

	public void UpdateCategoryVolume(Global.SoundCategory cat)
	{
		if (!m_sourcesByCategory.TryGetValue(cat, out var sources))
		{
			return;
		}
		float categoryVolume = SoundUtils.GetCategoryVolume(cat);
		for (int i = 0; i < sources.Count; i++)
		{
			AudioSource source = sources[i];
			if (!(source == null))
			{
				SourceExtension ext = GetExtension(source);
				float duckingVolume = GetDuckingVolume(source);
				UpdateVolume(source, ext, categoryVolume, duckingVolume);
			}
		}
	}

	private void UpdateAllCategoryVolumes()
	{
		foreach (Global.SoundCategory cat in m_sourcesByCategory.Keys)
		{
			UpdateCategoryVolume(cat);
		}
	}

	private void UpdatePitch(AudioSource source, SourceExtension ext)
	{
		source.pitch = ext.m_codePitch * ext.m_sourcePitch * ext.m_defPitch;
	}

	private void OnMasterEnabledOptionChanged(Option option, object prevValue, bool existed, object userData)
	{
		m_isMasterEnabled = Options.Get().GetBool(option);
		UpdateAllMutes();
	}

	private void OnMasterVolumeOptionChanged(Option option, object prevValue, bool existed, object userData)
	{
		SetMasterVolumeExponential();
	}

	private void OnEnabledOptionChanged(Option option, object prevValue, bool existed, object userData)
	{
		m_isMusicEnabled = Options.Get().GetBool(option);
		foreach (KeyValuePair<Global.SoundCategory, Option> pair in SoundDataTables.s_categoryEnabledOptionMap)
		{
			Global.SoundCategory cat = pair.Key;
			if (pair.Value == option)
			{
				UpdateCategoryMute(cat);
			}
		}
	}

	private void OnVolumeOptionChanged(Option option, object prevValue, bool existed, object userData)
	{
		foreach (KeyValuePair<Global.SoundCategory, Option> pair in SoundDataTables.s_categoryVolumeOptionMap)
		{
			Global.SoundCategory cat = pair.Key;
			if (pair.Value == option)
			{
				UpdateCategoryVolume(cat);
			}
		}
	}

	private void OnBackgroundSoundOptionChanged(Option option, object prevValue, bool existed, object userData)
	{
		UpdateAppMute();
	}

	private void OnMonoSoundOptionChanged(Option option, object prevValue, bool existed, object userData)
	{
		SetMonoSoundOption(Options.Get().GetBool(option));
	}

	private void OnAudioConfigurationChanged(bool deviceWasChanged)
	{
		if (deviceWasChanged)
		{
			Log.Sound.Print("OnAudioConfigurationChanged - Audio device was changed.");
		}
		else
		{
			Log.Sound.Print("OnAudioConfigurationChanged - Audio system was reset.");
		}
		UpdateAllCategoryVolumes();
		SetMasterVolumeExponential();
	}

	private void RegisterSourceByCategory(AudioSource source, Global.SoundCategory cat)
	{
		if (!m_sourcesByCategory.TryGetValue(cat, out var sources))
		{
			sources = new List<AudioSource>();
			m_sourcesByCategory.Add(cat, sources);
			sources.Add(source);
		}
		else if (!sources.Contains(source))
		{
			sources.Add(source);
		}
	}

	private void UnregisterSourceByCategory(AudioSource source)
	{
		Global.SoundCategory cat = GetCategory(source);
		if (!m_sourcesByCategory.TryGetValue(cat, out var sources))
		{
			Debug.LogWarning($"SoundManager.UnregisterSourceByCategory() - {GetSourceId(source)} is untracked. category={cat}");
		}
		else
		{
			sources.Remove(source);
		}
	}

	private bool IsCategoryEnabled(AudioSource source)
	{
		SoundDef def = source.GetComponent<SoundDef>();
		return IsCategoryEnabled(def.m_Category);
	}

	private bool IsCategoryEnabled(Global.SoundCategory cat)
	{
		if (SoundUtils.IsMusicCategory(cat) && SoundUtils.IsDeviceBackgroundMusicPlaying())
		{
			return false;
		}
		if (!m_isMasterEnabled)
		{
			return false;
		}
		Option opt = SoundUtils.GetCategoryEnabledOption(cat);
		return opt switch
		{
			Option.INVALID => true, 
			Option.MUSIC => m_isMusicEnabled, 
			_ => Options.Get().GetBool(opt), 
		};
	}

	private float GetCategoryVolume(AudioSource source)
	{
		return SoundUtils.GetCategoryVolume(source.GetComponent<SoundDef>().m_Category);
	}

	private bool IsCategoryAudible(Global.SoundCategory cat)
	{
		if (SoundUtils.GetCategoryVolume(cat) <= Mathf.Epsilon)
		{
			return false;
		}
		return IsCategoryEnabled(cat);
	}

	private void RegisterSourceByClip(AudioSource source, AudioClip clip)
	{
		if (!m_sourcesByClipName.TryGetValue(clip.name, out var sources))
		{
			sources = new List<AudioSource>();
			m_sourcesByClipName.Add(clip.name, sources);
			sources.Add(source);
		}
		else if (!sources.Contains(source))
		{
			sources.Add(source);
		}
	}

	private void UnregisterSourceByClip(AudioSource source)
	{
		AudioClip clip = source.clip;
		if (clip == null)
		{
			Debug.LogWarning($"SoundManager.UnregisterSourceByClip() - id {GetSourceId(source)} (source {source}) is untracked");
			return;
		}
		if (!m_sourcesByClipName.TryGetValue(clip.name, out var sources))
		{
			Debug.LogError($"SoundManager.UnregisterSourceByClip() - id {GetSourceId(source)} (source {source}) is untracked. clip={clip}");
			return;
		}
		sources.Remove(source);
		if (sources.Count == 0)
		{
			m_sourcesByClipName.Remove(clip.name);
		}
	}

	private bool ProcessClipLimits(AudioClip clip)
	{
		if (m_config == null || m_config.m_PlaybackLimitDefs == null)
		{
			return false;
		}
		string clipName = clip.name;
		bool limited = false;
		AudioSource lowerPrioritySource = null;
		foreach (SoundPlaybackLimitDef currPlaybackDef in m_config.m_PlaybackLimitDefs)
		{
			SoundPlaybackLimitClipDef clipDef = FindClipDefInPlaybackDef(clipName, currPlaybackDef);
			if (clipDef == null)
			{
				continue;
			}
			int lowestPriority = clipDef.m_Priority;
			float minUnitTimePassed = 2f;
			int limitCount = 0;
			foreach (SoundPlaybackLimitClipDef currClipDef in currPlaybackDef.m_ClipDefs)
			{
				string currClipName = currClipDef.LegacyName;
				if (!m_sourcesByClipName.TryGetValue(currClipName, out var sources))
				{
					continue;
				}
				int currPriority = currClipDef.m_Priority;
				foreach (AudioSource source in sources)
				{
					if (!IsPlaying(source))
					{
						continue;
					}
					float unitTimePassed = source.time / source.clip.length;
					if (unitTimePassed <= currClipDef.m_ExclusivePlaybackThreshold)
					{
						limitCount++;
						if (currPriority < lowestPriority && unitTimePassed < minUnitTimePassed)
						{
							lowerPrioritySource = source;
							lowestPriority = currPriority;
							minUnitTimePassed = unitTimePassed;
						}
					}
				}
			}
			if (limitCount >= currPlaybackDef.m_Limit)
			{
				limited = true;
				break;
			}
		}
		if (!limited)
		{
			return false;
		}
		if (lowerPrioritySource == null)
		{
			return true;
		}
		Stop(lowerPrioritySource);
		return false;
	}

	private SoundPlaybackLimitClipDef FindClipDefInPlaybackDef(string clipName, SoundPlaybackLimitDef def)
	{
		if (def.m_ClipDefs == null)
		{
			return null;
		}
		foreach (SoundPlaybackLimitClipDef clipDef in def.m_ClipDefs)
		{
			string defClipName = clipDef.LegacyName;
			if (clipName == defClipName)
			{
				return clipDef;
			}
		}
		return null;
	}

	public bool StartDucking(SoundDucker ducker)
	{
		if (ducker == null)
		{
			return false;
		}
		if (ducker.m_DuckedCategoryDefs == null)
		{
			return false;
		}
		if (ducker.m_DuckedCategoryDefs.Count == 0)
		{
			return false;
		}
		RegisterForDucking(ducker, ducker.GetDuckedCategoryDefs());
		return true;
	}

	public void StopDucking(SoundDucker ducker)
	{
		if (!(ducker == null) && ducker.m_DuckedCategoryDefs != null && ducker.m_DuckedCategoryDefs.Count != 0)
		{
			UnregisterForDucking(ducker, ducker.GetDuckedCategoryDefs());
		}
	}

	public void SetIgnoreDucking(AudioSource source, bool enable)
	{
		if (!(source == null))
		{
			SoundDef def = source.GetComponent<SoundDef>();
			if (!(def == null))
			{
				def.m_IgnoreDucking = enable;
			}
		}
	}

	private void RegisterSourceForDucking(AudioSource source, SourceExtension ext)
	{
		SoundDuckingDef duckingDef = FindDuckingDefForSource(source);
		if (duckingDef != null)
		{
			RegisterForDucking(source, duckingDef.m_DuckedCategoryDefs);
			ext.m_ducking = true;
		}
	}

	private void RegisterForDucking(object trigger, List<SoundDuckedCategoryDef> defs)
	{
		foreach (SoundDuckedCategoryDef duckedCatDef in defs)
		{
			DuckState state = RegisterDuckState(trigger, duckedCatDef);
			ChangeDuckState(state, DuckMode.BEGINNING);
		}
	}

	private DuckState RegisterDuckState(object trigger, SoundDuckedCategoryDef duckedCatDef)
	{
		Global.SoundCategory duckedCat = duckedCatDef.m_Category;
		DuckState state;
		if (m_duckStates.TryGetValue(duckedCat, out var states))
		{
			state = states.Find((DuckState currState) => currState.IsTrigger(trigger));
			if (state != null)
			{
				return state;
			}
		}
		else
		{
			states = new List<DuckState>();
			m_duckStates.Add(duckedCat, states);
		}
		state = new DuckState();
		states.Add(state);
		state.SetTrigger(trigger);
		state.SetDuckedDef(duckedCatDef);
		return state;
	}

	private void UnregisterSourceForDucking(AudioSource source, SourceExtension ext)
	{
		if (ext.m_ducking)
		{
			SoundDuckingDef duckingDef = FindDuckingDefForSource(source);
			if (duckingDef != null)
			{
				UnregisterForDucking(source, duckingDef.m_DuckedCategoryDefs);
			}
		}
	}

	private void UnregisterForDucking(object trigger, List<SoundDuckedCategoryDef> defs)
	{
		foreach (SoundDuckedCategoryDef def in defs)
		{
			Global.SoundCategory duckedCat = def.m_Category;
			if (!m_duckStates.TryGetValue(duckedCat, out var states))
			{
				Debug.LogError(string.Format("SoundManager.UnregisterForDucking() - {0} ducks {1}, but no DuckStates were found for {1}", trigger, duckedCat));
				continue;
			}
			DuckState state = states.Find((DuckState currState) => currState.IsTrigger(trigger));
			if (state != null)
			{
				ChangeDuckState(state, DuckMode.RESTORING);
			}
		}
	}

	private uint GetNextDuckStateTweenId()
	{
		m_nextDuckStateTweenId = (m_nextDuckStateTweenId + 1) & 0xFFFFFFFFu;
		return m_nextDuckStateTweenId;
	}

	private void ChangeDuckState(DuckState state, DuckMode mode)
	{
		string tweenName = state.GetTweenName();
		if (tweenName != null)
		{
			iTween.StopByName(SceneObject, tweenName);
		}
		state.SetMode(mode);
		state.SetTweenName(null);
		switch (mode)
		{
		case DuckMode.BEGINNING:
			AnimateBeginningDuckState(state);
			break;
		case DuckMode.RESTORING:
			AnimateRestoringDuckState(state);
			break;
		}
	}

	private void AnimateBeginningDuckState(DuckState state)
	{
		string tweenName = $"DuckState Begin id={GetNextDuckStateTweenId()}";
		state.SetTweenName(tweenName);
		SoundDuckedCategoryDef duckedDef = state.GetDuckedDef();
		Action<object> onDuckStateUpdate = delegate(object amount)
		{
			float volume = (float)amount;
			state.SetVolume(volume);
			UpdateCategoryVolume(duckedDef.m_Category);
		};
		Action<object> onDuckStateBeginningComplete = delegate(object e)
		{
			OnDuckStateBeginningComplete(e);
		};
		Hashtable args = iTweenManager.Get().GetTweenHashTable();
		args.Add("name", tweenName);
		args.Add("time", duckedDef.m_BeginSec);
		args.Add("easetype", duckedDef.m_BeginEaseType);
		args.Add("from", state.GetVolume());
		args.Add("to", duckedDef.m_Volume);
		args.Add("onupdate", onDuckStateUpdate);
		args.Add("oncomplete", onDuckStateBeginningComplete);
		args.Add("oncompleteparams", state);
		iTween.ValueTo(SceneObject, args);
	}

	private void OnDuckStateBeginningComplete(object arg)
	{
		if (arg is DuckState state)
		{
			state.SetMode(DuckMode.HOLD);
			state.SetTweenName(null);
		}
	}

	private void AnimateRestoringDuckState(DuckState state)
	{
		string tweenName = $"DuckState Finish id={GetNextDuckStateTweenId()}";
		state.SetTweenName(tweenName);
		SoundDuckedCategoryDef duckedDef = state.GetDuckedDef();
		Action<object> onDuckStateUpdate = delegate(object amount)
		{
			float volume = (float)amount;
			state.SetVolume(volume);
			UpdateCategoryVolume(duckedDef.m_Category);
		};
		Action<object> onDuckStateRestoringComplete = delegate(object e)
		{
			OnDuckStateRestoringComplete(e);
		};
		Hashtable args = iTweenManager.Get().GetTweenHashTable();
		args.Add("name", tweenName);
		args.Add("time", duckedDef.m_RestoreSec);
		args.Add("easetype", duckedDef.m_RestoreEaseType);
		args.Add("from", state.GetVolume());
		args.Add("to", 1f);
		args.Add("onupdate", onDuckStateUpdate);
		args.Add("oncomplete", onDuckStateRestoringComplete);
		args.Add("oncompleteparams", state);
		iTween.ValueTo(SceneObject, args);
	}

	private void OnDuckStateRestoringComplete(object arg)
	{
		if (!(arg is DuckState state))
		{
			return;
		}
		Global.SoundCategory cat = state.GetDuckedDef().m_Category;
		List<DuckState> states = m_duckStates[cat];
		for (int i = 0; i < states.Count; i++)
		{
			if (states[i] == state)
			{
				states.RemoveAt(i);
				if (states.Count == 0)
				{
					m_duckStates.Remove(cat);
				}
				break;
			}
		}
	}

	private SoundDuckingDef FindDuckingDefForSource(AudioSource source)
	{
		Global.SoundCategory cat = GetCategory(source);
		return FindDuckingDefForCategory(cat);
	}

	private SoundDuckingDef FindDuckingDefForCategory(Global.SoundCategory cat)
	{
		if (m_config == null || m_config.m_DuckingDefs == null)
		{
			return null;
		}
		foreach (SoundDuckingDef def in m_config.m_DuckingDefs)
		{
			if (cat == def.m_TriggerCategory)
			{
				return def;
			}
		}
		return null;
	}

	private float GetDuckingVolume(AudioSource source)
	{
		if (source == null)
		{
			return 1f;
		}
		SoundDef def = source.GetComponent<SoundDef>();
		if (def.m_IgnoreDucking)
		{
			return 1f;
		}
		return GetDuckingVolume(def.m_Category);
	}

	private float GetDuckingVolume(Global.SoundCategory cat)
	{
		if (!m_duckStates.TryGetValue(cat, out var states))
		{
			return 1f;
		}
		float minVolume = 1f;
		foreach (DuckState state in states)
		{
			Global.SoundCategory triggerCat = state.GetTriggerCategory();
			if (triggerCat == Global.SoundCategory.NONE || IsCategoryAudible(triggerCat))
			{
				float volume = state.GetVolume();
				if (minVolume > volume)
				{
					minVolume = volume;
				}
			}
		}
		return minVolume;
	}

	private bool SourceIsRegisteredForDucking(AudioSource source)
	{
		SoundDuckingDef duckingDef = FindDuckingDefForSource(source);
		if (duckingDef == null)
		{
			return false;
		}
		foreach (SoundDuckedCategoryDef duckedCategoryDef in duckingDef.m_DuckedCategoryDefs)
		{
			Global.SoundCategory duckedCat = duckedCategoryDef.m_Category;
			if (m_duckStates.TryGetValue(duckedCat, out var states))
			{
				if (states.Find((DuckState currState) => currState.IsTrigger(source)) == null)
				{
					return false;
				}
				continue;
			}
			return false;
		}
		return true;
	}

	private int GetNextSourceId()
	{
		int nextSourceId = m_nextSourceId;
		m_nextSourceId = ((m_nextSourceId == int.MaxValue) ? 1 : (m_nextSourceId + 1));
		return nextSourceId;
	}

	private int GetSourceId(AudioSource source)
	{
		return GetExtension(source)?.m_id ?? 0;
	}

	private AudioSource PlayImpl(AudioSource source, SoundDef oneShotDef, AudioClip oneShotClip = null, SoundOptions additionalSettings = null)
	{
		if (source == null)
		{
			AudioSource fallbackSource = GetPlaceholderSource();
			if (fallbackSource == null)
			{
				Error.AddDevFatal("SoundManager.Play() - source is null and fallback is null");
				return null;
			}
			Debug.LogWarningFormat("Using placeholder sound for source={0}, oneShotDef={1}, oneShotClip={2}", source, oneShotDef, oneShotClip);
			source = UnityEngine.Object.Instantiate(fallbackSource);
			m_generatedSources.Add(source);
		}
		SourceExtension ext = RegisterExtension(source, oneShotDef, oneShotClip, aboutToPlay: true);
		if (ext == null)
		{
			return null;
		}
		if (!SourceIsRegisteredForDucking(source))
		{
			RegisterSourceForDucking(source, ext);
		}
		UpdateSource(source, ext);
		if (additionalSettings == null)
		{
			SoundDef soundDef = oneShotDef;
			if (soundDef == null)
			{
				soundDef = source.GetComponent<SoundDef>();
			}
			if (soundDef != null)
			{
				additionalSettings = new SoundOptions
				{
					InstanceLimited = soundDef.m_InstanceLimited,
					InstanceTimeLimit = soundDef.m_InstanceLimitedDuration,
					MaxInstancesOfThisSound = soundDef.m_InstanceLimitMaximum,
					LimitMaxingOutOption = soundDef.m_LimitMaxOutOption
				};
			}
		}
		if (additionalSettings != null && additionalSettings.InstanceLimited)
		{
			if (activeLimitedSounds.TryGetValue(source.gameObject.name, out var activeInstances))
			{
				int maxInstances = additionalSettings.MaxInstancesOfThisSound;
				if (maxInstances < 1)
				{
					maxInstances = 1;
				}
				if (activeInstances >= maxInstances)
				{
					switch ((int)additionalSettings.LimitMaxingOutOption)
					{
					case 0:
						return null;
					case 1:
						break;
					default:
						Log.Presence.PrintWarning("Unknown Sound MaxOut Option: {0}", additionalSettings.LimitMaxingOutOption);
						return null;
					}
					FinishFirstGeneratedSourceByName(source.gameObject.name);
				}
				else
				{
					activeLimitedSounds.Remove(source.gameObject.name);
					activeLimitedSounds.Add(source.gameObject.name, activeInstances + 1);
				}
			}
			else
			{
				activeLimitedSounds.Add(source.gameObject.name, 1);
			}
			float time = additionalSettings.InstanceTimeLimit;
			if (time <= 0f)
			{
				time = source.clip.length;
			}
			HearthstoneApplication.Get().StartCoroutine(EnableInstanceLimitedSound(source.gameObject.name, time));
		}
		if (additionalSettings != null && additionalSettings.RandomStartTime && source?.clip != null)
		{
			source.time = UnityEngine.Random.value * source.clip.length;
		}
		source.Play();
		return source;
	}

	private IEnumerator EnableInstanceLimitedSound(string sound, float time)
	{
		if (!activeLimitedSounds.ContainsKey(sound))
		{
			yield break;
		}
		while (time > 0f)
		{
			time -= Time.deltaTime;
			yield return null;
		}
		if (activeLimitedSounds.TryGetValue(sound, out var activeInstances))
		{
			activeLimitedSounds.Remove(sound);
			activeInstances--;
			if (activeInstances > 0)
			{
				activeLimitedSounds.Add(sound, activeInstances);
			}
		}
	}

	private SoundDef GetDefFromSource(AudioSource source)
	{
		SoundDef def = source.GetComponent<SoundDef>();
		if (def == null)
		{
			Log.Sound.Print("SoundUtils.GetDefFromSource() - source={0} has no def. adding new def.", source);
			def = source.gameObject.AddComponent<SoundDef>();
		}
		return def;
	}

	private void OnAppFocusChanged(bool focus, object userData)
	{
		UpdateAppMute();
	}

	private void UpdateAppMute()
	{
		UpdateMusicAndSources();
		if (HearthstoneApplication.Get() != null)
		{
			m_mute = !HearthstoneApplication.Get().HasFocus() && !Options.Get().GetBool(Option.BACKGROUND_SOUND);
		}
		UpdateAllMutes();
	}

	private void OnSceneLoaded(SceneMgr.Mode mode, PegasusScene scene, object userData)
	{
		GarbageCollectBundles();
	}

	private AudioSource GenerateAudioSource(AudioSource templateSource, SoundDef def)
	{
		string defName = ((def != null) ? Path.GetFileNameWithoutExtension(def.m_AudioClip) : "CreatedSound");
		string goName = $"Audio Object - {defName}";
		AudioSource source;
		if ((bool)templateSource)
		{
			GameObject gameObject = new GameObject(goName);
			SoundUtils.AddAudioSourceComponents(gameObject);
			source = gameObject.GetComponent<AudioSource>();
			SoundUtils.CopyAudioSource(templateSource, source);
		}
		else if ((bool)m_config.m_PlayClipTemplate)
		{
			GameObject gameObject2 = UnityEngine.Object.Instantiate(m_config.m_PlayClipTemplate.gameObject);
			gameObject2.name = goName;
			source = gameObject2.GetComponent<AudioSource>();
		}
		else
		{
			GameObject gameObject3 = new GameObject(goName);
			SoundUtils.AddAudioSourceComponents(gameObject3);
			source = gameObject3.GetComponent<AudioSource>();
		}
		m_generatedSources.Add(source);
		return source;
	}

	private void InitSourceTransform(AudioSource source, GameObject parentObject)
	{
		if (!(source == null) && !(source.gameObject == null) && !(source.transform == null))
		{
			source.transform.parent = SceneObject.transform;
			if (parentObject == null || parentObject.transform == null)
			{
				source.transform.position = Vector3.zero;
			}
			else
			{
				source.transform.position = parentObject.transform.position;
			}
		}
	}

	private void FinishSource(AudioSource source)
	{
		if (m_currentMusicTrack == source)
		{
			ChangeCurrentMusicTrack(null);
		}
		else if (m_currentAmbienceTrack == source)
		{
			ChangeCurrentAmbienceTrack(null);
		}
		for (int i = 0; i < m_fadingTracks.Count; i++)
		{
			if (m_fadingTracks[i] == source)
			{
				m_fadingTracks.RemoveAt(i);
				break;
			}
		}
		UnregisterSourceByCategory(source);
		UnregisterSourceByClip(source);
		SourceExtension ext = GetExtension(source);
		if (ext != null)
		{
			UnregisterSourceForDucking(source, ext);
			UnregisterSourceBundle(source, ext);
			UnregisterExtension(source, ext);
		}
		FinishGeneratedSource(source);
	}

	private void FinishGeneratedSource(AudioSource source)
	{
		for (int i = 0; i < m_generatedSources.Count; i++)
		{
			if (m_generatedSources[i] == source)
			{
				UnityEngine.Object.Destroy(source.gameObject);
				m_generatedSources.RemoveAt(i);
				break;
			}
		}
	}

	private void FinishFirstGeneratedSourceByName(string sourceName)
	{
		for (int i = 0; i < m_generatedSources.Count; i++)
		{
			AudioSource genSource = m_generatedSources[i];
			if (genSource.gameObject.name == sourceName)
			{
				UnityEngine.Object.Destroy(genSource.gameObject);
				m_generatedSources.RemoveAt(i);
				break;
			}
		}
	}

	private BundleInfo RegisterSourceBundle(AssetReference assetRef, AudioSource source)
	{
		if (!m_bundleInfos.TryGetValue(assetRef, out var info))
		{
			info = new BundleInfo();
			info.SetAssetRef(assetRef);
			m_bundleInfos.Add(assetRef, info);
		}
		if (source != null)
		{
			info.AddRef(source);
			SourceExtension ext = RegisterExtension(source);
			if (ext == null)
			{
				return null;
			}
			ext.m_bundleName = assetRef;
		}
		return info;
	}

	private void UnregisterSourceBundle(AudioSource source, SourceExtension ext)
	{
		if (ext.m_bundleName != null)
		{
			UnregisterSourceBundle(ext.m_bundleName, source);
		}
	}

	private void UnregisterSourceBundle(string name, AudioSource source)
	{
		if (m_bundleInfos.TryGetValue(name, out var info) && info.RemoveRef(source) && info.CanGarbageCollect())
		{
			m_bundleInfos.Remove(name);
			UnloadSoundBundle(name);
		}
	}

	private void UnloadSoundBundle(AssetReference assetRef)
	{
	}

	private void GarbageCollectBundles()
	{
		Map<string, BundleInfo> bundleInfos = new Map<string, BundleInfo>();
		foreach (KeyValuePair<string, BundleInfo> pair in m_bundleInfos)
		{
			string assetRef = pair.Key;
			BundleInfo info = pair.Value;
			info.EnableGarbageCollect(enable: true);
			if (info.CanGarbageCollect())
			{
				UnloadSoundBundle(assetRef);
			}
			else
			{
				bundleInfos.Add(assetRef, info);
			}
		}
		m_bundleInfos = bundleInfos;
	}

	private void UpdateMusicAndSources()
	{
		UpdateMusicAndAmbience();
		UpdateSources();
	}

	private void UpdateSources()
	{
		UpdateSourceExtensionMappings();
		UpdateSourcesByCategory();
		UpdateSourcesByClipName();
		UpdateSourceBundles();
		UpdateGeneratedSources();
		UpdateDuckStates();
	}

	private void UpdateSourceExtensionMappings()
	{
		int i = 0;
		while (i < m_extensionMappings.Count)
		{
			AudioSource source = m_extensionMappings[i].Source;
			if (source == null)
			{
				m_extensionMappings.RemoveAt(i);
				continue;
			}
			if (!IsActive(source))
			{
				m_inactiveSources.Add(source);
			}
			i++;
		}
		CleanInactiveSources();
	}

	private void CleanUpSourceList(List<AudioSource> sources)
	{
		if (sources == null)
		{
			return;
		}
		int i = 0;
		while (i < sources.Count)
		{
			if (sources[i] == null)
			{
				sources.RemoveAt(i);
			}
			else
			{
				i++;
			}
		}
	}

	private void UpdateSourcesByCategory()
	{
		foreach (KeyValuePair<Global.SoundCategory, List<AudioSource>> item in m_sourcesByCategory)
		{
			CleanUpSourceList(item.Value);
		}
	}

	private void UpdateSourcesByClipName()
	{
		foreach (KeyValuePair<string, List<AudioSource>> item in m_sourcesByClipName)
		{
			CleanUpSourceList(item.Value);
		}
	}

	private void UpdateSourceBundles()
	{
		m_bundleInfosToRemove.Clear();
		foreach (KeyValuePair<string, BundleInfo> bundleInfo2 in m_bundleInfos)
		{
			BundleInfo bundleInfo = bundleInfo2.Value;
			List<AudioSource> sources = bundleInfo.GetRefs();
			int i = 0;
			bool removed = false;
			while (i < sources.Count)
			{
				if (sources[i] == null)
				{
					removed = true;
					sources.RemoveAt(i);
				}
				else
				{
					i++;
				}
			}
			if (removed)
			{
				string assetRef = bundleInfo.GetAssetRef();
				if (bundleInfo.CanGarbageCollect())
				{
					m_bundleInfosToRemove.Add(assetRef);
				}
			}
		}
		for (int ii = 0; ii < m_bundleInfosToRemove.Count; ii++)
		{
			string bundleName = m_bundleInfosToRemove[ii];
			m_bundleInfos.Remove(bundleName);
			UnloadSoundBundle(bundleName);
		}
	}

	private void UpdateGeneratedSources()
	{
		CleanUpSourceList(m_generatedSources);
	}

	private void UpdateDuckStates()
	{
		foreach (KeyValuePair<Global.SoundCategory, List<DuckState>> duckState in m_duckStates)
		{
			foreach (DuckState state in duckState.Value)
			{
				if (!state.IsTriggerAlive() && state.GetMode() != DuckMode.RESTORING)
				{
					ChangeDuckState(state, DuckMode.RESTORING);
				}
			}
		}
	}

	private void CleanInactiveSources()
	{
		foreach (AudioSource source in m_inactiveSources)
		{
			FinishSource(source);
		}
		m_inactiveSources.Clear();
	}

	private void AddExtensionMapping(AudioSource source, SourceExtension extension)
	{
		if (!(source == null) && extension != null)
		{
			ExtensionMapping mapping = new ExtensionMapping();
			mapping.Source = source;
			mapping.Extension = extension;
			m_extensionMappings.Add(mapping);
		}
	}

	private void RemoveExtensionMapping(AudioSource source)
	{
		for (int i = 0; i < m_extensionMappings.Count; i++)
		{
			if (m_extensionMappings[i].Source == source)
			{
				m_extensionMappings.RemoveAt(i);
				break;
			}
		}
	}

	private SourceExtension GetExtension(AudioSource source)
	{
		for (int i = 0; i < m_extensionMappings.Count; i++)
		{
			ExtensionMapping mapping = m_extensionMappings[i];
			if (mapping.Source == source)
			{
				return mapping.Extension;
			}
		}
		return null;
	}
}
