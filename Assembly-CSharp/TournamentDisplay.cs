using System;
using System.Collections.Generic;
using Blizzard.T5.Core;
using PegasusShared;
using PegasusUtil;
using UnityEngine;

public class TournamentDisplay : MonoBehaviour
{
	public delegate void DelMedalChanged(NetCache.NetCacheMedalInfo medalInfo);

	public TextMesh m_modeName;

	public Vector3_MobileOverride m_deckPickerPosition;

	public Vector3 m_SetRotationOnscreenPosition = new Vector3(27.051f, 1.7f, -22.4f);

	public Vector3 m_SetRotationOffscreenPosition = new Vector3(-60f, 1.7f, -22.4f);

	public Vector3 m_SetRotationOffscreenDuringTransition = new Vector3(-260f, 1.7f, -22.4f);

	public float m_SetRotationSideInTime = 1f;

	private static TournamentDisplay s_instance;

	private bool m_allInitialized;

	private bool m_netCacheReturned;

	private bool m_deckPickerTrayLoaded;

	private DeckPickerTrayDisplay m_deckPickerTray;

	private GameObject m_deckPickerTrayGO;

	private NetCache.NetCacheMedalInfo m_currentMedalInfo;

	private List<DelMedalChanged> m_medalChangedListeners = new List<DelMedalChanged>();

	public bool SlidingInForSetRotation { get; private set; }

	private void Awake()
	{
		AssetLoader.Get().InstantiatePrefab(UniversalInputManager.UsePhoneUI ? "DeckPickerTray_phone.prefab:a30124f640b5b92459bf820a4e3b1ca7" : "DeckPickerTray.prefab:3e13b59cdca14074bbce2b7d903ed895", DeckPickerTrayLoaded, null, AssetLoadingOptions.IgnorePrefabPosition);
		s_instance = this;
	}

	private void OnDestroy()
	{
		s_instance = null;
		UserAttentionManager.StopBlocking(UserAttentionBlocker.SET_ROTATION_INTRO);
		UnregisterListeners();
	}

	private void Start()
	{
		MusicManager.Get().StartPlaylist(MusicPlaylistType.UI_Tournament);
		NetCache.Get().RegisterScreenTourneys(UpdateTourneyPage, NetCache.DefaultErrorHandler);
	}

	private void Update()
	{
		if (!m_allInitialized && m_netCacheReturned && m_deckPickerTrayLoaded)
		{
			Log.PlayModeInvestigation.PrintInfo($"TournamentDisplay.Update() called. m_allInitialized={m_allInitialized}, m_netCacheReturned={m_netCacheReturned}, m_deckPickerTrayLoaded={m_deckPickerTrayLoaded}");
			if (SetRotationManager.Get().ShouldShowSetRotationIntro())
			{
				Log.PlayModeInvestigation.PrintInfo("TournamentDisplay.Update() -- ShouldShowSetRotationIntro() = true");
				m_deckPickerTrayGO.transform.localPosition = m_SetRotationOffscreenDuringTransition;
				SetupSetRotation();
				Log.PlayModeInvestigation.PrintInfo("TournamentDisplay.Update() -- SetupSetRotation() is complete");
			}
			m_deckPickerTray.InitAssets();
			m_allInitialized = true;
		}
	}

	public void UpdateHeaderText()
	{
		if (m_deckPickerTray == null)
		{
			return;
		}
		if (!GameUtils.HasCompletedApprentice())
		{
			m_deckPickerTray.SetHeaderText(GameStrings.Get("GLUE_PLAY_STANDARD"));
			return;
		}
		if (!Options.GetInRankedPlayMode())
		{
			string casualHeaderText = GameStrings.Get("GLUE_PLAY_CASUAL");
			m_deckPickerTray.SetHeaderText(casualHeaderText);
			return;
		}
		Map<FormatType, string> obj = new Map<FormatType, string>
		{
			{
				FormatType.FT_WILD,
				"GLUE_PLAY_WILD"
			},
			{
				FormatType.FT_STANDARD,
				"GLUE_PLAY_STANDARD"
			},
			{
				FormatType.FT_CLASSIC,
				"GLUE_PLAY_CLASSIC"
			},
			{
				FormatType.FT_TWIST,
				"GLUE_PLAY_TWIST"
			}
		};
		FormatType currentFormatType = Options.GetFormatType();
		string headerText;
		if (!obj.TryGetValue(currentFormatType, out var glueText))
		{
			Debug.LogError("TournamentDisplay.UpdateHeaderText called in unsupported format type: " + currentFormatType);
			headerText = "UNSUPPORTED HEADER TEXT " + currentFormatType;
		}
		else
		{
			headerText = GameStrings.Get(glueText);
		}
		m_deckPickerTray.SetHeaderText(headerText);
	}

	private void DeckPickerTrayLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		m_deckPickerTrayGO = go;
		m_deckPickerTray = go.GetComponent<DeckPickerTrayDisplay>();
		m_deckPickerTray.transform.parent = base.transform;
		m_deckPickerTray.transform.localPosition = m_deckPickerPosition;
		m_deckPickerTrayLoaded = true;
		UpdateHeaderText();
	}

	public void SetRotationSlideIn()
	{
		SlidingInForSetRotation = true;
		m_deckPickerTrayGO.transform.localPosition = m_SetRotationOffscreenPosition;
		iTween.MoveTo(m_deckPickerTrayGO, iTween.Hash("position", m_SetRotationOnscreenPosition, "delay", 0f, "time", m_SetRotationSideInTime, "islocal", true, "easetype", iTween.EaseType.easeOutBounce, "oncomplete", (Action<object>)delegate
		{
			SlidingInForSetRotation = false;
		}));
	}

	private void UpdateTourneyPage()
	{
		if (!NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>().Games.Tournament)
		{
			if (!SceneMgr.Get().IsModeRequested(SceneMgr.Mode.HUB))
			{
				SceneMgr.Get().SetNextMode(SceneMgr.Mode.HUB);
				Error.AddWarningLoc("GLOBAL_FEATURE_DISABLED_TITLE", "GLOBAL_FEATURE_DISABLED_MESSAGE_PLAY");
			}
			return;
		}
		NetCache.NetCacheMedalInfo medalInfo = NetCache.Get().GetNetObject<NetCache.NetCacheMedalInfo>();
		bool medalChanged = false;
		if (m_currentMedalInfo != null)
		{
			foreach (FormatType enumValue in Enum.GetValues(typeof(FormatType)))
			{
				if (enumValue != 0)
				{
					MedalInfoData medalInfoData = medalInfo.GetMedalInfoData(enumValue);
					MedalInfoData currentMedalInfoData = m_currentMedalInfo.GetMedalInfoData(enumValue);
					if (medalInfoData == null || currentMedalInfoData == null || medalInfoData.LeagueId != currentMedalInfoData.LeagueId || medalInfoData.StarLevel != currentMedalInfoData.StarLevel || medalInfoData.Stars != currentMedalInfoData.Stars || medalInfoData.StarsPerWin != currentMedalInfoData.StarsPerWin)
					{
						medalChanged = true;
						break;
					}
				}
			}
		}
		m_currentMedalInfo = medalInfo;
		if (medalChanged)
		{
			DelMedalChanged[] array = m_medalChangedListeners.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				array[i](m_currentMedalInfo);
			}
		}
		m_netCacheReturned = true;
	}

	private void UnregisterListeners()
	{
		if (NetCache.Get() != null)
		{
			NetCache.Get().UnregisterNetCacheHandler(UpdateTourneyPage);
		}
	}

	public static TournamentDisplay Get()
	{
		return s_instance;
	}

	public void SceneUnload()
	{
		UnregisterListeners();
	}

	public NetCache.NetCacheMedalInfo GetCurrentMedalInfo()
	{
		return m_currentMedalInfo;
	}

	public void RegisterMedalChangedListener(DelMedalChanged listener)
	{
		if (!m_medalChangedListeners.Contains(listener))
		{
			m_medalChangedListeners.Add(listener);
		}
	}

	public void RemoveMedalChangedListener(DelMedalChanged listener)
	{
		m_medalChangedListeners.Remove(listener);
	}

	private void SetupSetRotation()
	{
		AssetLoader.Get().InstantiatePrefab("TheBox_TheClock.prefab:d922114c10efb5e4d8ab76d57913eff3");
	}
}
