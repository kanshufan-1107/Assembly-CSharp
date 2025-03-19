using System.Collections;
using Assets;
using Hearthstone.Core.Streaming;
using Hearthstone.Streaming;
using Hearthstone.UI;
using UnityEngine;

public class BoxScrollButton : BoxMenuButton
{
	private const string ANIMATION_POPUP = "TavernBrawl_ButtonPopup";

	private const string ANIMATION_POPDOWN = "TavernBrawl_ButtonPopdown";

	private const string ANIMATION_DEACTIVATE = "TavernBrawl_ButtonDeactivate";

	public NetCache.NetCacheFeatures.CacheGames.FeatureFlags m_feature;

	public float m_hoverDelay = 0.5f;

	public Animator m_animator;

	public WeakAssetReference m_popupSound;

	public WeakAssetReference m_popdownSound;

	[SerializeField]
	private GameObject m_downloadIcon;

	[SerializeField]
	private WidgetInstance m_downloadProgressWidgetRef;

	private bool m_isPoppedUp;

	private Coroutine m_coroutine;

	private DownloadTags.Content m_moduleTag;

	private BoxDownloadProgress m_downloadProgress;

	private Coroutine m_downloadProgressCoroutine;

	private IGameDownloadManager DownloadManager => GameDownloadManagerProvider.Get();

	protected override void Awake()
	{
		base.Awake();
		m_moduleTag = GetDownloadTagForFeature();
		if (m_moduleTag != 0)
		{
			DownloadManager.RegisterModuleInstallationStateChangeListener(OnModuleDownloadStateChange, invokeImmediately: false);
		}
		if (m_moduleTag != 0 && !m_downloadProgressWidgetRef.gameObject.activeSelf && m_downloadProgressCoroutine == null)
		{
			m_downloadProgressCoroutine = StartCoroutine(InitializeDownloadProgress());
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (m_moduleTag != 0)
		{
			DownloadManager.UnregisterModuleInstallationStateChangeListener(OnModuleDownloadStateChange);
		}
	}

	public void UpdateButton()
	{
		if (CanDownloadMode())
		{
			bool moduleDownloadRequired = !DownloadManager.IsModuleReadyToPlay(m_moduleTag);
			bool isDownloadingModule = DownloadManager.IsModuleDownloading(m_moduleTag);
			UpdateDownloadButtonIcon(moduleDownloadRequired && !isDownloadingModule);
			UpdateDownloadProgress(isDownloadingModule);
		}
	}

	public override void TriggerOver()
	{
		Box box = Box.Get();
		if (!(box == null) && box.IsInStateWithButtons())
		{
			if (IsFeatureActive())
			{
				base.TriggerOver();
			}
			else
			{
				m_coroutine = StartCoroutine(DoPopup());
			}
		}
	}

	private IEnumerator DoPopup()
	{
		if (!UniversalInputManager.Get().IsTouchMode())
		{
			yield return new WaitForSeconds(m_hoverDelay);
		}
		m_isPoppedUp = true;
		SoundManager.Get().LoadAndPlay(m_popupSound.AssetString);
		m_animator.Play("TavernBrawl_ButtonPopup");
	}

	public override void TriggerOut()
	{
		if (IsFeatureActive())
		{
			base.TriggerOut();
			return;
		}
		if (m_coroutine != null)
		{
			StopCoroutine(m_coroutine);
			m_coroutine = null;
		}
		if (m_isPoppedUp)
		{
			SoundManager.Get().LoadAndPlay(m_popdownSound.AssetString);
			m_animator.Play("TavernBrawl_ButtonPopdown");
			m_isPoppedUp = false;
		}
	}

	public override void TriggerPress()
	{
		if (IsFeatureActive())
		{
			base.TriggerPress();
		}
	}

	public override void TriggerRelease()
	{
		if (IsFeatureActive())
		{
			base.TriggerRelease();
		}
	}

	public void SetDisabledVisuals()
	{
		m_animator.Play("TavernBrawl_ButtonDeactivate", -1, 1f);
	}

	public bool IsFeatureActive()
	{
		NetCache.NetCacheFeatures features = NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>();
		if (features != null && features.Games != null)
		{
			return features.Games.GetFeatureFlag(m_feature);
		}
		return false;
	}

	public bool CanDownloadMode()
	{
		if (m_feature == NetCache.NetCacheFeatures.CacheGames.FeatureFlags.Battlegrounds)
		{
			if (m_moduleTag != 0 && IsFeatureActive())
			{
				return GameModeUtils.HasUnlockedMode(Global.UnlockableGameMode.BATTLEGROUNDS);
			}
			return false;
		}
		return false;
	}

	private IEnumerator InitializeDownloadProgress()
	{
		while (!DownloadManager.IsReadyToPlay)
		{
			yield return null;
		}
		m_downloadProgressWidgetRef.gameObject.SetActive(value: true);
		m_downloadProgressWidgetRef.Initialize();
		m_downloadProgressWidgetRef.RegisterReadyListener(delegate
		{
			m_downloadProgress = m_downloadProgressWidgetRef.gameObject.GetComponentInChildren<BoxDownloadProgress>();
			m_downloadProgress.Init(m_moduleTag);
			UpdateDownloadProgress(DownloadManager.IsModuleDownloading(m_moduleTag));
		});
		m_downloadProgressCoroutine = null;
	}

	private void UpdateDownloadButtonIcon(bool showDownloadButton)
	{
		if (!CanDownloadMode())
		{
			showDownloadButton = false;
		}
		if (m_downloadIcon != null)
		{
			m_downloadIcon.SetActive(showDownloadButton);
		}
	}

	private void UpdateDownloadProgress(bool showDownloadProgress)
	{
		if (!CanDownloadMode())
		{
			showDownloadProgress = false;
		}
		if (m_downloadProgress != null)
		{
			m_downloadProgress.gameObject.SetActive(showDownloadProgress);
		}
	}

	private void OnModuleDownloadStateChange(DownloadTags.Content moduleTag, ModuleState state)
	{
		if (moduleTag == m_moduleTag)
		{
			UpdateDownloadButtonIcon(state < ModuleState.Downloading);
			UpdateDownloadProgress(state == ModuleState.Queued || state == ModuleState.Downloading);
			if (m_downloadProgress != null)
			{
				m_downloadProgress.UpdateDownloadStateChange(state);
			}
		}
	}

	private DownloadTags.Content GetDownloadTagForFeature()
	{
		if (m_feature == NetCache.NetCacheFeatures.CacheGames.FeatureFlags.Battlegrounds)
		{
			return DownloadTags.Content.Bgs;
		}
		return DownloadTags.Content.Unknown;
	}
}
