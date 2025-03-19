using System;
using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core;
using Hearthstone.DataModels;
using Hearthstone.Progression;
using Hearthstone.UI;
using PegasusShared;
using UnityEngine;

public class TutorialProgressScreen : MonoBehaviour
{
	[SerializeField]
	private List<WidgetInstance> m_bossCoinWidgets = new List<WidgetInstance>();

	[SerializeField]
	private WidgetInstance m_playButton;

	[SerializeField]
	private VisualController m_playButtonVisualController;

	[SerializeField]
	private VisualController m_cardRewardBroadcaster;

	[SerializeField]
	private VisualController m_backerVisualController;

	[SerializeField]
	private VisualController m_rewardChestVisualController;

	[SerializeField]
	private VisualController m_widgetLoadedVisualController;

	[SerializeField]
	private VisualController m_popupRootBroadcaster;

	[SerializeField]
	private Clickable m_rewardChestClickable;

	private const string CODE_LOCKED = "CODE_LOCKED";

	private const string CODE_UNLOCKED = "CODE_UNLOCKED";

	private const string CODE_UNLOCK_ANIMATION = "CODE_UNLOCK_ANIMATION";

	private const string CODE_COMPLETION_ANIMATION = "CODE_COMPLETION_ANIMATION";

	private const string CODE_COMPLETE = "CODE_COMPLETE";

	private const string CODE_SHOW_CARDS = "CODE_SHOW_CARDS";

	private const string CODE_SHOW_LID_OPEN = "SHOW_LID_OPEN";

	private const string CODE_SHOW_READY_TO_OPEN = "SHOW_READY_TO_OPEN";

	private const string CODE_PLAY_PRESSED = "CODE_PLAY_PRESSED";

	private const string CODE_GAME = "CODE_GAME";

	private const string CODE_BOX = "CODE_BOX";

	private const string CODE_WIDGET_LOADED = "WIDGET_LOADED";

	private static TutorialProgressScreen s_instance;

	private WidgetTemplate m_widget;

	private List<AdventureBossCoin> m_bossCoins = new List<AdventureBossCoin>();

	private HeroCoin.CoinPressCallback m_coinPressCallback;

	private EndGameScreen m_endGameScreen;

	private bool m_widgetReady;

	private ScenarioDbId m_nextScenarioId;

	private readonly Map<TutorialProgress, ScenarioDbId> m_progressToNextMissionIdMap = new Map<TutorialProgress, ScenarioDbId>
	{
		{
			TutorialProgress.NOTHING_COMPLETE,
			ScenarioDbId.TUTORIAL_REXXAR
		},
		{
			TutorialProgress.REXXAR_COMPLETE,
			ScenarioDbId.TUTORIAL_GARROSH
		},
		{
			TutorialProgress.GARROSH_COMPLETE,
			ScenarioDbId.TUTORIAL_LICH_KING
		}
	};

	private readonly List<string> m_coinMaterials = new List<string> { "Tut1_CoinPortrait.mat:64bdb7b3a9f7d884da7b886d4ad8d3cd", "Tut2_CoinPortrait.mat:2413103366053e648963b69878899c17", "Tut3_CoinPortrait.mat:b1cef4bf388ca5c41bf550a0f8cca64f" };

	private List<ScenarioDbfRecord> m_sortedMissionRecords = new List<ScenarioDbfRecord>();

	private const float START_SCALE_VAL = 0.5f;

	private Vector3 START_SCALE = new Vector3(0.5f, 0.5f, 0.5f);

	private Vector3 FINAL_SCALE = new PlatformDependentValue<Vector3>(PlatformCategory.Screen)
	{
		PC = new Vector3(0.15f, 0.15f, 0.15f),
		Phone = new Vector3(0.15f, 0.15f, 0.15f)
	};

	private Vector3 FINAL_SCALE_OVER_BOX = new PlatformDependentValue<Vector3>(PlatformCategory.Screen)
	{
		PC = new Vector3(2f, 2f, 2f),
		Phone = new Vector3(1.5f, 1.5f, 1.5f)
	};

	private PlatformDependentValue<Vector3> FINAL_POS = new PlatformDependentValue<Vector3>(PlatformCategory.Screen)
	{
		PC = new Vector3(-8f, 5f, -5f),
		Phone = new Vector3(-8f, 5f, -4.58f)
	};

	private PlatformDependentValue<Vector3> FINAL_POS_OVER_BOX = new PlatformDependentValue<Vector3>(PlatformCategory.Screen)
	{
		PC = new Vector3(0f, 24.5f, -0.2f),
		Phone = new Vector3(0f, 21f, -2.06f)
	};

	private bool IS_TESTING;

	private ScreenEffectsHandle m_screenEffectsHandle;

	private void Awake()
	{
		s_instance = this;
		m_widget = GetComponent<WidgetTemplate>();
		if (m_widget != null)
		{
			m_widget.RegisterReadyListener(OnWidgetReady);
		}
		m_screenEffectsHandle = new ScreenEffectsHandle(this);
		ScreenEffectParameters screenEffectParameters = ScreenEffectParameters.VignettePerspective;
		screenEffectParameters.Time = 0.5f;
		screenEffectParameters.EaseType = iTween.EaseType.easeInOutQuad;
		screenEffectParameters.Vignette = new VignetteParameters(1f);
		m_screenEffectsHandle.StartEffect(screenEffectParameters);
		NetCache.NetCacheProfileProgress profileProgress = NetCache.Get().GetNetObject<NetCache.NetCacheProfileProgress>();
		SetLessonState("TUTORIAL_" + (int)profileProgress.CampaignProgress);
		if (m_playButton != null)
		{
			m_playButton.RegisterReadyListener(delegate
			{
				PlayButton componentInChildren = m_playButton.GetComponentInChildren<PlayButton>();
				if (componentInChildren != null)
				{
					componentInChildren.AddEventListener(UIEventType.RELEASE, OnPlayButton);
				}
			});
		}
		InitMissionRecords();
	}

	private void OnDestroy()
	{
		NetCache netCache = NetCache.Get();
		if (netCache != null)
		{
			netCache.UnregisterNetCacheHandler(UpdateProgressOnStart);
			netCache.UnregisterNetCacheHandler(UpdateProgressOnComplete);
		}
		BnetBar.Get()?.Undim();
		s_instance = null;
	}

	public static TutorialProgressScreen Get()
	{
		return s_instance;
	}

	public void StartTutorialProgress()
	{
		StartCoroutine(StartTutorialProgressOnWidgetReady());
	}

	private IEnumerator StartTutorialProgressOnWidgetReady()
	{
		while (!m_widgetReady)
		{
			yield return null;
		}
		if (SceneMgr.Get().GetMode() == SceneMgr.Mode.GAMEPLAY)
		{
			if (GameState.Get().GetFriendlySidePlayer().GetTag<TAG_PLAYSTATE>(GAME_TAG.PLAYSTATE) == TAG_PLAYSTATE.WON)
			{
				GameState.Get().GetOpposingSidePlayer().GetHeroCard()
					.GetActorSpell(SpellType.ENDGAME_LOSE_ENEMY)
					.ActivateState(SpellStateType.DEATH);
			}
			BroadcastPopupRootVisualControllerMessage("CODE_GAME");
			Gameplay.Get().RemoveGamePlayNameBannerPhone();
		}
		else
		{
			BroadcastPopupRootVisualControllerMessage("CODE_BOX");
		}
		LoadAllTutorialHeroEntities();
	}

	public void SetCoinPressCallback(HeroCoin.CoinPressCallback callback)
	{
		if (callback != null)
		{
			m_coinPressCallback = delegate
			{
				Hide();
				callback();
			};
		}
	}

	public void SetEndGameScreen(EndGameScreen endGameScreen)
	{
		m_endGameScreen = endGameScreen;
	}

	private void OnWidgetReady(object unused)
	{
		if (m_widgetLoadedVisualController != null && m_widgetLoadedVisualController.HasState("WIDGET_LOADED"))
		{
			m_widgetLoadedVisualController.SetState("WIDGET_LOADED");
		}
		if (NetCache.Get().GetNetObject<NetCache.NetCacheProfileProgress>().CampaignProgress == TutorialProgress.LICH_KING_COMPLETE)
		{
			m_playButton.RegisterReadyListener(delegate
			{
				PlayButton componentInChildren = m_playButton.GetComponentInChildren<PlayButton>();
				if (componentInChildren != null)
				{
					componentInChildren.Disable();
				}
			});
		}
		if (m_rewardChestVisualController != null && m_rewardChestClickable != null)
		{
			if (HasEverOpenedRewardChest())
			{
				m_rewardChestVisualController.SetState("SHOW_LID_OPEN");
			}
			else if (NetCache.Get().GetNetObject<NetCache.NetCacheProfileProgress>().CampaignProgress == TutorialProgress.LICH_KING_COMPLETE)
			{
				m_rewardChestVisualController.SetState("SHOW_READY_TO_OPEN");
				m_rewardChestClickable.enabled = true;
				m_rewardChestClickable.AddEventListener(UIEventType.RELEASE, OnRewardChest);
			}
		}
		m_widgetReady = true;
	}

	private void OnPlayButton(UIEvent e)
	{
		if (m_playButtonVisualController != null && m_playButtonVisualController.HasState("CODE_PLAY_PRESSED"))
		{
			m_playButtonVisualController.SetState("CODE_PLAY_PRESSED");
		}
		GameMgr.Get().FindGame(GameType.GT_TUTORIAL, FormatType.FT_WILD, (int)m_nextScenarioId, 0, 0L, null, null, restoreSavedGameState: false, null, null, 0L);
		PlayButton playButton = m_playButton.GetComponentInChildren<PlayButton>();
		if (playButton != null)
		{
			playButton.Disable();
		}
	}

	private void OnRewardChest(UIEvent e)
	{
		if (NetCache.Get().GetNetObject<NetCache.NetCacheProfileProgress>().CampaignProgress != TutorialProgress.LICH_KING_COMPLETE)
		{
			return;
		}
		RewardScrollDataModel dataModel = new RewardScrollDataModel
		{
			DisplayName = GameStrings.Get("GLUE_CLASS_UNLOCKED_CLASS"),
			Description = GameStrings.Get("GLOBAL_CLASS_MAGE"),
			RewardList = new RewardListDataModel
			{
				Items = new DataModelList<RewardItemDataModel>
				{
					new RewardItemDataModel
					{
						Quantity = 1,
						ItemType = RewardItemType.HERO_SKIN,
						Card = new CardDataModel
						{
							CardId = "HERO_08"
						}
					}
				}
			}
		};
		Widget rewardWidget = RewardScroll.ShowFakeRewardWidget(dataModel);
		rewardWidget.RegisterReadyListener(delegate
		{
			Clickable componentInChildren = rewardWidget.GetComponentInChildren<Clickable>();
			if (componentInChildren != null)
			{
				componentInChildren.AddEventListener(UIEventType.RELEASE, OnRewardDismissed);
			}
		});
		SetHasEverOpenedRewardChest();
		if (m_rewardChestVisualController != null && m_rewardChestClickable != null)
		{
			m_rewardChestVisualController.SetState("SHOW_LID_OPEN");
			m_rewardChestClickable.Active = false;
		}
		Hide();
	}

	private void OnRewardDismissed(UIEvent e)
	{
		if (m_endGameScreen != null)
		{
			LoadingScreen.Get().SetFadeColor(Color.white);
			m_endGameScreen.NavigateToHubForFirstTime();
		}
	}

	private void InitMissionRecords()
	{
		foreach (ScenarioDbfRecord missionRecord in GameDbf.Scenario.GetRecords())
		{
			if (missionRecord.AdventureId == 1)
			{
				int missionDbId = missionRecord.ID;
				if (Enum.IsDefined(typeof(ScenarioDbId), missionDbId))
				{
					m_sortedMissionRecords.Add(missionRecord);
				}
			}
		}
		m_sortedMissionRecords.Sort(GameUtils.MissionSortComparison);
	}

	private void LoadAllTutorialHeroEntities()
	{
		for (int i = 0; i < m_sortedMissionRecords.Count; i++)
		{
			string cardId = GameUtils.GetMissionHeroCardId(m_sortedMissionRecords[i].ID);
			if (DefLoader.Get().GetEntityDef(cardId) == null)
			{
				Debug.LogError($"TutorialProgress.OnTutorialHeroEntityDefLoaded() - failed to load {cardId}");
			}
		}
		StartCoroutine(Initialize());
	}

	private IEnumerator Initialize()
	{
		if (m_bossCoinWidgets.Count != m_sortedMissionRecords.Count)
		{
			Debug.LogError("Count of boss coins and mission records does not match!");
			yield break;
		}
		foreach (WidgetInstance widget in m_bossCoinWidgets)
		{
			while (!widget.IsReady)
			{
				yield return null;
			}
			AdventureBossCoin coin = widget.GetComponentInChildren<AdventureBossCoin>();
			if (coin != null)
			{
				m_bossCoins.Add(coin);
			}
		}
		SetupCoins();
		Show();
	}

	private void SetupCoins()
	{
		if (m_bossCoins.Count != m_coinMaterials.Count)
		{
			Debug.LogError("Count of boss coins and coin materials does not match!");
			return;
		}
		for (int i = 0; i < m_sortedMissionRecords.Count; i++)
		{
			AdventureBossCoin coin = m_bossCoins[i];
			if (coin != null)
			{
				AssetReference materialReference = AssetReference.CreateFromAssetString(m_coinMaterials[i]);
				if (materialReference == null)
				{
					Debug.LogError("Failed to load asset reference");
					return;
				}
				Material material = AssetLoader.Get().LoadAsset<Material>(materialReference);
				coin.SetPortraitMaterial(material);
				coin.gameObject.SetActive(value: true);
			}
		}
		LayerUtils.SetLayer(base.gameObject, GameLayer.IgnoreFullScreenEffects);
	}

	private void Show()
	{
		iTween.FadeTo(base.gameObject, 1f, 0.25f);
		bool isDuringGameplay = SceneMgr.Get().GetMode() == SceneMgr.Mode.GAMEPLAY;
		base.transform.position = (isDuringGameplay ? FINAL_POS : FINAL_POS_OVER_BOX);
		LayerUtils.SetLayer(base.gameObject, GameLayer.IgnoreFullScreenEffects);
		base.transform.localScale = START_SCALE;
		Hashtable args = iTweenManager.Get().GetTweenHashTable();
		args.Add("onstart", "OnScaleAnimStart");
		args.Add("scale", isDuringGameplay ? FINAL_SCALE : FINAL_SCALE_OVER_BOX);
		args.Add("time", 0.5f);
		args.Add("oncomplete", "OnScaleAnimComplete");
		args.Add("oncompletetarget", base.gameObject);
		iTween.ScaleTo(base.gameObject, args);
		BnetBar bnetBar = BnetBar.Get();
		if ((object)bnetBar != null)
		{
			bnetBar.HideSkipTutorialButton();
			bnetBar.Undim();
		}
		if (NetCache.Get().GetNetObject<NetCache.NetCacheProfileProgress>().CampaignProgress != TutorialProgress.LICH_KING_COMPLETE)
		{
			PlayButton playButton = m_playButton.GetComponentInChildren<PlayButton>();
			if (playButton != null)
			{
				playButton.Enable();
			}
		}
	}

	private void Hide()
	{
		Hashtable scaleArgs = iTweenManager.Get().GetTweenHashTable();
		scaleArgs.Add("scale", START_SCALE);
		scaleArgs.Add("time", 0.5f);
		scaleArgs.Add("oncomplete", "OnHideAnimComplete");
		scaleArgs.Add("oncompletetarget", base.gameObject);
		iTween.ScaleTo(base.gameObject, scaleArgs);
		scaleArgs = iTweenManager.Get().GetTweenHashTable();
		scaleArgs.Add("alpha", 0f);
		scaleArgs.Add("time", 0.25f);
		scaleArgs.Add("delay", 0.25f);
		iTween.FadeTo(base.gameObject, scaleArgs);
		if (GameMgr.Get().IsTraditionalTutorial() && !GameUtils.IsTraditionalTutorialComplete())
		{
			BnetBar.Get().ShowSkipTutorialButton();
		}
	}

	private void BroadcastCoinVisualControllerMessage(AdventureBossCoin coin, string message)
	{
		if (!(coin == null))
		{
			VisualController vc = coin.GetVisualControllerBroadcaster();
			if (!(vc == null) && vc.HasState(message))
			{
				vc.SetState(message);
			}
		}
	}

	private void BroadcastCardVisualControllerMessage(string message)
	{
		if (!(m_cardRewardBroadcaster == null) && m_cardRewardBroadcaster.HasState(message))
		{
			m_cardRewardBroadcaster.SetState(message);
		}
	}

	private void BroadcastPopupRootVisualControllerMessage(string message)
	{
		if (!(m_popupRootBroadcaster == null) && m_popupRootBroadcaster.HasState(message))
		{
			m_popupRootBroadcaster.SetState(message);
		}
	}

	private void SetLessonState(string state)
	{
		if (!(m_backerVisualController == null) && m_backerVisualController.HasState(state))
		{
			m_backerVisualController.SetState(state);
		}
	}

	private void OnScaleAnimStart()
	{
		if (IS_TESTING)
		{
			UpdateProgressOnStart();
		}
		else
		{
			NetCache.Get().RegisterTutorialEndGameScreen(UpdateProgressOnStart, NetCache.DefaultErrorHandler);
		}
	}

	private void OnScaleAnimComplete()
	{
		if (IS_TESTING)
		{
			UpdateProgressOnComplete();
		}
		else
		{
			NetCache.Get().RegisterTutorialEndGameScreen(UpdateProgressOnComplete, NetCache.DefaultErrorHandler);
		}
	}

	private void OnHideAnimComplete()
	{
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void UpdateProgressOnStart()
	{
		int nextTutorialIndex = 1;
		if (IS_TESTING)
		{
			m_nextScenarioId = m_progressToNextMissionIdMap[TutorialProgress.REXXAR_COMPLETE];
		}
		else
		{
			NetCache.NetCacheProfileProgress profileProgress = NetCache.Get().GetNetObject<NetCache.NetCacheProfileProgress>();
			if (m_progressToNextMissionIdMap.ContainsKey(profileProgress.CampaignProgress))
			{
				m_nextScenarioId = m_progressToNextMissionIdMap[profileProgress.CampaignProgress];
				nextTutorialIndex = (int)profileProgress.CampaignProgress;
			}
			else
			{
				if (profileProgress.CampaignProgress != TutorialProgress.LICH_KING_COMPLETE)
				{
					return;
				}
				nextTutorialIndex = m_bossCoins.Count;
			}
		}
		for (int i = 0; i < m_bossCoins.Count; i++)
		{
			AdventureBossCoin coin = m_bossCoins[i];
			if (!(coin == null))
			{
				if (i == nextTutorialIndex - 1)
				{
					BroadcastCoinVisualControllerMessage(coin, "CODE_UNLOCKED");
				}
				else if (i < nextTutorialIndex)
				{
					BroadcastCoinVisualControllerMessage(coin, "CODE_COMPLETE");
				}
				else
				{
					BroadcastCoinVisualControllerMessage(coin, "CODE_LOCKED");
				}
			}
		}
	}

	private void UpdateProgressOnComplete()
	{
		bool isDuringGameplay = SceneMgr.Get().GetMode() == SceneMgr.Mode.GAMEPLAY;
		int nextTutorialIndex = 1;
		if (IS_TESTING)
		{
			m_nextScenarioId = m_progressToNextMissionIdMap[TutorialProgress.REXXAR_COMPLETE];
		}
		else
		{
			NetCache.NetCacheProfileProgress profileProgress = NetCache.Get().GetNetObject<NetCache.NetCacheProfileProgress>();
			if (m_progressToNextMissionIdMap.ContainsKey(profileProgress.CampaignProgress))
			{
				m_nextScenarioId = m_progressToNextMissionIdMap[profileProgress.CampaignProgress];
				nextTutorialIndex = (int)profileProgress.CampaignProgress;
			}
			else
			{
				if (profileProgress.CampaignProgress != TutorialProgress.LICH_KING_COMPLETE)
				{
					return;
				}
				nextTutorialIndex = m_bossCoins.Count;
			}
		}
		for (int i = 0; i < m_bossCoins.Count; i++)
		{
			AdventureBossCoin coin = m_bossCoins[i];
			if (coin == null)
			{
				continue;
			}
			if (i == nextTutorialIndex - 1)
			{
				if (isDuringGameplay)
				{
					BroadcastCoinVisualControllerMessage(coin, "CODE_COMPLETION_ANIMATION");
					if (nextTutorialIndex != m_bossCoins.Count)
					{
						BroadcastCardVisualControllerMessage("CODE_SHOW_CARDS_" + (i + 1));
					}
				}
				else
				{
					BroadcastCoinVisualControllerMessage(coin, "CODE_COMPLETE");
				}
			}
			else if (i == nextTutorialIndex)
			{
				if (isDuringGameplay)
				{
					BroadcastCoinVisualControllerMessage(coin, "CODE_UNLOCK_ANIMATION");
				}
				else
				{
					BroadcastCoinVisualControllerMessage(coin, "CODE_UNLOCKED");
				}
			}
		}
	}

	public static bool HasEverOpenedRewardChest()
	{
		if (!GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.PLAYER_FLAGS, GameSaveKeySubkeyId.PLAYER_FLAGS_HAS_OPENED_TRADITIONAL_TUTORIAL_REWARD_CHEST, out long hasOpenedRewardChest))
		{
			return false;
		}
		return hasOpenedRewardChest != 0;
	}

	public static void SetHasEverOpenedRewardChest()
	{
		GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.PLAYER_FLAGS, GameSaveKeySubkeyId.PLAYER_FLAGS_HAS_OPENED_TRADITIONAL_TUTORIAL_REWARD_CHEST, 1L));
	}

	private void ExitButtonPress(UIEvent e)
	{
		SceneMgr.Get().SetNextMode(SceneMgr.Mode.HUB);
		m_screenEffectsHandle.StopEffect(0.5f, iTween.EaseType.easeInOutQuad);
	}
}
