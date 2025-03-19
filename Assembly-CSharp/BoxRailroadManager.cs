using System.Collections;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Core;
using Blizzard.T5.Logging;
using Hearthstone.Progression;
using PegasusUtil;
using UnityEngine;

public class BoxRailroadManager : MonoBehaviour
{
	public enum RailroadState
	{
		NONE,
		JOURNAL_DISK_CLOSED,
		JOURNAL_DISK_OPEN,
		JOURNAL_OR_HEARTHSTONE,
		PACK_OPENING,
		COMPLETE
	}

	public enum ButtonRailroadState
	{
		ENABLED,
		DISABLED,
		HIDDEN
	}

	[SerializeField]
	private GameObject m_journalArrowBone;

	[SerializeField]
	private GameObject m_journalArrowBoneMobile;

	[SerializeField]
	private GameObject m_journalArrowBoneMobileWide;

	[SerializeField]
	private GameObject m_packOpeningArrowBone;

	[SerializeField]
	private GameObject m_packOpeningArrowBoneMobile;

	[SerializeField]
	private GameObject m_packOpeningArrowBoneMobileWide;

	private Dictionary<RailroadState, Dictionary<Box.ButtonType, ButtonRailroadState>> m_railroadButtonStates;

	private Dictionary<RailroadState, Dictionary<Box.State, Box.State>> m_railroadBoxStates;

	private RailroadState m_currentRailroadState;

	private Box.State m_overriddenBoxState;

	protected static Blizzard.T5.Core.ILogger m_logger;

	private RailroadState m_showingBoxTutorialsForState;

	private Coroutine m_boxTutorialCoroutine;

	private Notification m_arrowNotification;

	private Notification m_dialogueNotification;

	private bool m_isRailroadStateReady;

	private const float APPRENTICE_TRACK_LINK_DELAY_DEFAULT = 0.7f;

	private const float ARROW_POPUP_DELAY_DEFAULT = 1f;

	private const float ARROW_POPUP_DELAY_AFTER_DIALOGUE = 2f;

	public bool IsRailroadStateReady => m_isRailroadStateReady;

	private void Awake()
	{
		InitializeRailroadButtonStates();
		InitializeRailroadBoxStates();
		UpdateRailroadState();
		RewardTrackManager rewardTrackManager = RewardTrackManager.Get();
		if (rewardTrackManager != null)
		{
			rewardTrackManager.OnRewardTracksReceived += OnRewardTracksReceived;
		}
		if (Network.Get() != null)
		{
			Network.Get().RegisterNetHandler(SkipApprenticeResponse.PacketID.ID, OnSkipApprenticeResponse);
		}
		m_logger = LogSystem.Get().GetLogger("Apprentice");
	}

	private void OnDestroy()
	{
		RewardTrackManager rewardTrackManager = RewardTrackManager.Get();
		if (rewardTrackManager != null)
		{
			rewardTrackManager.OnRewardTracksReceived -= OnRewardTracksReceived;
		}
		Network.Get()?.RemoveNetHandler(SkipApprenticeResponse.PacketID.ID, OnSkipApprenticeResponse);
		if (m_boxTutorialCoroutine != null)
		{
			StopCoroutine(m_boxTutorialCoroutine);
		}
	}

	private void OnRewardTracksReceived()
	{
		UpdateRailroadingOnBox();
	}

	private void OnSkipApprenticeResponse()
	{
		UpdateRailroadingOnBox();
	}

	private void InitializeRailroadButtonStates()
	{
		m_railroadButtonStates = new Dictionary<RailroadState, Dictionary<Box.ButtonType, ButtonRailroadState>>();
		Dictionary<Box.ButtonType, ButtonRailroadState> railroadToJournalButtonConfig = new Dictionary<Box.ButtonType, ButtonRailroadState>
		{
			{
				Box.ButtonType.TRADITIONAL,
				ButtonRailroadState.DISABLED
			},
			{
				Box.ButtonType.BACON,
				ButtonRailroadState.DISABLED
			},
			{
				Box.ButtonType.TAVERN_BRAWL,
				ButtonRailroadState.DISABLED
			},
			{
				Box.ButtonType.GAME_MODES,
				ButtonRailroadState.DISABLED
			},
			{
				Box.ButtonType.STORE,
				ButtonRailroadState.HIDDEN
			},
			{
				Box.ButtonType.OPEN_PACKS,
				ButtonRailroadState.HIDDEN
			},
			{
				Box.ButtonType.COLLECTION,
				ButtonRailroadState.HIDDEN
			}
		};
		m_railroadButtonStates[RailroadState.JOURNAL_DISK_CLOSED] = railroadToJournalButtonConfig;
		m_railroadButtonStates[RailroadState.JOURNAL_DISK_OPEN] = railroadToJournalButtonConfig;
		m_railroadButtonStates[RailroadState.JOURNAL_OR_HEARTHSTONE] = new Dictionary<Box.ButtonType, ButtonRailroadState>
		{
			{
				Box.ButtonType.BACON,
				ButtonRailroadState.DISABLED
			},
			{
				Box.ButtonType.TAVERN_BRAWL,
				ButtonRailroadState.DISABLED
			},
			{
				Box.ButtonType.GAME_MODES,
				ButtonRailroadState.DISABLED
			},
			{
				Box.ButtonType.STORE,
				ButtonRailroadState.HIDDEN
			},
			{
				Box.ButtonType.OPEN_PACKS,
				ButtonRailroadState.HIDDEN
			},
			{
				Box.ButtonType.COLLECTION,
				ButtonRailroadState.HIDDEN
			}
		};
		m_railroadButtonStates[RailroadState.PACK_OPENING] = new Dictionary<Box.ButtonType, ButtonRailroadState>
		{
			{
				Box.ButtonType.TRADITIONAL,
				ButtonRailroadState.DISABLED
			},
			{
				Box.ButtonType.BACON,
				ButtonRailroadState.DISABLED
			},
			{
				Box.ButtonType.TAVERN_BRAWL,
				ButtonRailroadState.DISABLED
			},
			{
				Box.ButtonType.GAME_MODES,
				ButtonRailroadState.DISABLED
			},
			{
				Box.ButtonType.JOURNAL,
				ButtonRailroadState.DISABLED
			},
			{
				Box.ButtonType.STORE,
				ButtonRailroadState.HIDDEN
			},
			{
				Box.ButtonType.COLLECTION,
				ButtonRailroadState.HIDDEN
			}
		};
	}

	private void InitializeRailroadBoxStates()
	{
		m_railroadBoxStates = new Dictionary<RailroadState, Dictionary<Box.State, Box.State>>();
		m_railroadBoxStates[RailroadState.JOURNAL_DISK_CLOSED] = new Dictionary<Box.State, Box.State>
		{
			{
				Box.State.HUB,
				Box.State.APPRENTICE_OVERRIDE_CLOSED
			},
			{
				Box.State.HUB_WITH_DRAWER,
				Box.State.APPRENTICE_OVERRIDE_CLOSED
			}
		};
		m_railroadBoxStates[RailroadState.JOURNAL_OR_HEARTHSTONE] = new Dictionary<Box.State, Box.State> { 
		{
			Box.State.HUB_WITH_DRAWER,
			Box.State.APPRENTICE_OVERRIDE_HUB
		} };
		m_railroadBoxStates[RailroadState.JOURNAL_DISK_OPEN] = new Dictionary<Box.State, Box.State> { 
		{
			Box.State.HUB_WITH_DRAWER,
			Box.State.APPRENTICE_OVERRIDE_HUB
		} };
	}

	public void UpdateRailroadingOnBox()
	{
		UpdateRailroadState();
		if (!RankMgr.Get().IsWaitingForApprenticeComplete && Box.Get() != null)
		{
			Box.Get().UpdateStateForCurrentSceneMode();
		}
	}

	public void UpdateRailroadState()
	{
		bool isGameSaveDataReady = (m_isRailroadStateReady = GameSaveDataManager.Get().IsDataReady(GameSaveKeyId.FTUE));
		if (GameUtils.HasCompletedApprentice() || !isGameSaveDataReady)
		{
			m_currentRailroadState = RailroadState.NONE;
			return;
		}
		if (GameUtils.ShouldSkipRailroading())
		{
			m_currentRailroadState = RailroadState.NONE;
			return;
		}
		RewardTrackManager rewardTrackManager = RewardTrackManager.Get();
		if (!rewardTrackManager.IsApprenticeTrackReady())
		{
			m_logger.Log(Blizzard.T5.Core.LogLevel.Warning, "[BoxRailRoadManager] Apprentice player missing Apprentice Track data, can't determind railroad state.");
			m_currentRailroadState = RailroadState.NONE;
			return;
		}
		Hearthstone.Progression.RewardTrack apprenticeTrack = rewardTrackManager.GetRewardTrack(Global.RewardTrackType.APPRENTICE);
		int apprenticeLevel = apprenticeTrack.TrackDataModel.Level;
		bool hasUnclaimedRewardForCurrentLevel = apprenticeTrack.HasUnclaimedRewardsForLevel(apprenticeLevel);
		bool hasSeenTavernGuide = false;
		if (GameSaveDataManager.Get().IsDataReady(GameSaveKeyId.FTUE))
		{
			GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.FTUE, GameSaveKeySubkeyId.FTUE_HAS_SEEN_TAVERN_GUIDE_INTRODUCTION, out long hasSeenTavernGuideFlag);
			hasSeenTavernGuide = hasSeenTavernGuideFlag == 1;
		}
		if (apprenticeLevel == 1 && !hasSeenTavernGuide)
		{
			m_currentRailroadState = RailroadState.JOURNAL_DISK_CLOSED;
		}
		else if (apprenticeLevel == 1)
		{
			m_currentRailroadState = RailroadState.JOURNAL_OR_HEARTHSTONE;
		}
		else if (apprenticeLevel == 2 && hasUnclaimedRewardForCurrentLevel)
		{
			m_currentRailroadState = RailroadState.JOURNAL_DISK_OPEN;
		}
		else if (apprenticeLevel == apprenticeTrack.TrackDataModel.LevelHardCap)
		{
			m_currentRailroadState = RailroadState.JOURNAL_DISK_CLOSED;
		}
		else
		{
			m_currentRailroadState = RailroadState.NONE;
		}
	}

	public ButtonRailroadState GetCurrentStateForButtonType(Box.ButtonType buttonType)
	{
		if (buttonType == Box.ButtonType.BACON && GameUtils.IsBattleGroundsTutorialComplete())
		{
			return ButtonRailroadState.ENABLED;
		}
		if (m_railroadButtonStates == null || !m_railroadButtonStates.ContainsKey(m_currentRailroadState))
		{
			return ButtonRailroadState.ENABLED;
		}
		Dictionary<Box.ButtonType, ButtonRailroadState> currentButtonStates = m_railroadButtonStates[m_currentRailroadState];
		if (!currentButtonStates.ContainsKey(buttonType))
		{
			return ButtonRailroadState.ENABLED;
		}
		return currentButtonStates[buttonType];
	}

	public bool ShouldShowCollectionButton()
	{
		return m_currentRailroadState == RailroadState.NONE;
	}

	public bool ShouldDisableButton(BoxMenuButton button)
	{
		return ShouldDisableButtonType(button.m_buttonType);
	}

	public bool ShouldDisableButtonType(Box.ButtonType buttonType)
	{
		ButtonRailroadState buttonState = GetCurrentStateForButtonType(buttonType);
		if (buttonState != ButtonRailroadState.DISABLED)
		{
			return buttonState == ButtonRailroadState.HIDDEN;
		}
		return true;
	}

	public bool ShouldHideButton(BoxMenuButton button)
	{
		return ShouldHideButtonType(button.m_buttonType);
	}

	public bool ShouldHideButtonType(Box.ButtonType buttonType)
	{
		return GetCurrentStateForButtonType(buttonType) == ButtonRailroadState.HIDDEN;
	}

	public Box.State GetBoxStateOverride(Box.State currentBoxState)
	{
		m_overriddenBoxState = currentBoxState;
		if (m_railroadBoxStates == null || !m_railroadBoxStates.ContainsKey(m_currentRailroadState))
		{
			return currentBoxState;
		}
		Dictionary<Box.State, Box.State> currentStateOverrides = m_railroadBoxStates[m_currentRailroadState];
		if (!currentStateOverrides.ContainsKey(currentBoxState))
		{
			return currentBoxState;
		}
		return currentStateOverrides[currentBoxState];
	}

	public Box.State GetOverriddenBoxState()
	{
		return m_overriddenBoxState;
	}

	public bool ShouldLinkToJournal()
	{
		if (m_currentRailroadState != RailroadState.JOURNAL_DISK_OPEN)
		{
			return m_currentRailroadState == RailroadState.JOURNAL_DISK_CLOSED;
		}
		return true;
	}

	public void ShowApprenticeTrack()
	{
		StartCoroutine(ShowApprenticeTrackOnDelay(0.7f));
	}

	private IEnumerator ShowApprenticeTrackOnDelay(float delay)
	{
		yield return new WaitForSecondsRealtime(delay);
		DeepLinkManager.ExecuteDeepLink(new string[2] { "journal", "ApprenticeTrack" }, DeepLinkManager.DeepLinkSource.RAILROAD_MANAGER, 0);
	}

	public bool ShouldSuppressPopups()
	{
		return m_currentRailroadState != RailroadState.NONE;
	}

	public void ToggleBoxTutorials(bool setEnabled)
	{
		if (setEnabled)
		{
			ShowBoxTutorialsIfNecessary();
			m_showingBoxTutorialsForState = m_currentRailroadState;
		}
		else
		{
			StopAllBoxTutorials();
			m_showingBoxTutorialsForState = RailroadState.NONE;
		}
	}

	private void ShowBoxTutorialsIfNecessary()
	{
		if (m_showingBoxTutorialsForState != m_currentRailroadState)
		{
			StopAllBoxTutorials();
			switch (m_currentRailroadState)
			{
			case RailroadState.JOURNAL_DISK_CLOSED:
			case RailroadState.JOURNAL_DISK_OPEN:
				ShowInitialJournalTutorialPopups();
				break;
			case RailroadState.PACK_OPENING:
				ShowPackOpeningTutorialPopups();
				break;
			case RailroadState.COMPLETE:
				ShowRailroadingCompletePopups();
				break;
			case RailroadState.NONE:
			case RailroadState.JOURNAL_OR_HEARTHSTONE:
				break;
			}
		}
	}

	private void StopAllBoxTutorials()
	{
		if (m_boxTutorialCoroutine != null)
		{
			StopCoroutine(m_boxTutorialCoroutine);
		}
		if (m_arrowNotification != null)
		{
			m_arrowNotification.PlayDeath();
		}
		if (m_dialogueNotification != null)
		{
			m_dialogueNotification.PlayDeath();
		}
	}

	private void CreateOrMoveArrow(Transform newTransform)
	{
		if (m_arrowNotification == null)
		{
			m_arrowNotification = NotificationManager.Get().CreateBouncingArrow(UserAttentionBlocker.NONE, newTransform.position, newTransform.localEulerAngles, addToList: false, newTransform.localScale.x);
			if (m_arrowNotification == null)
			{
				m_logger.Log(Blizzard.T5.Core.LogLevel.Error, "m_arrowNotification (bounceArrow) is null");
			}
		}
		else
		{
			m_arrowNotification.gameObject.SetActive(value: true);
			m_arrowNotification.transform.position = newTransform.position;
			m_arrowNotification.transform.localScale = newTransform.localScale;
			m_arrowNotification.transform.localEulerAngles = newTransform.localEulerAngles;
		}
	}

	private void ShowInitialJournalTutorialPopups()
	{
		GameSaveKeySubkeyId flagSubkey = GameSaveKeySubkeyId.FTUE_HAS_SEEN_POST_TUTORIAL_JOURNAL_DIALOGUE;
		GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.FTUE, flagSubkey, out long hasSeenPostTutorialDialogueFlag);
		bool num = hasSeenPostTutorialDialogueFlag == 1;
		if (!num)
		{
			ShowInnkeeperQuote("GLUE_PROGRESSION_APPRENTICE_TUTORIALS_INNKEEEPER_FIRST_REWARD", flagSubkey);
		}
		float arrowDelay = (num ? 1f : 2f);
		GameObject arrowBone = m_journalArrowBone;
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			arrowBone = m_journalArrowBoneMobile;
			arrowBone.transform.localPosition = TransformUtil.GetAspectRatioDependentPosition(m_journalArrowBoneMobile.transform.localPosition, m_journalArrowBoneMobile.transform.localPosition, m_journalArrowBoneMobileWide.transform.localPosition);
		}
		m_boxTutorialCoroutine = StartCoroutine(ShowArrowOnDelay(arrowBone, arrowDelay));
	}

	private void ShowPackOpeningTutorialPopups()
	{
		GameObject arrowBone = m_packOpeningArrowBone;
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			arrowBone = m_packOpeningArrowBoneMobile;
			arrowBone.transform.localPosition = TransformUtil.GetAspectRatioDependentPosition(m_packOpeningArrowBoneMobile.transform.localPosition, m_packOpeningArrowBoneMobile.transform.localPosition, m_packOpeningArrowBoneMobileWide.transform.localPosition);
		}
		m_boxTutorialCoroutine = StartCoroutine(ShowArrowOnDelay(arrowBone, 0.5f));
	}

	private void ShowRailroadingCompletePopups()
	{
		GameSaveKeySubkeyId flagSubkey = GameSaveKeySubkeyId.FTUE_HAS_SEEN_RAILROADING_COMPLETE_DIALOGUE;
		GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.FTUE, flagSubkey, out long hasSeenQuoteFlag);
		if (hasSeenQuoteFlag != 1)
		{
			ShowInnkeeperQuote("GLUE_PROGRESSION_APPRENTICE_TUTORIALS_INNKEEEPER_INTRO_COMPLETE", flagSubkey);
		}
	}

	private IEnumerator ShowArrowOnDelay(GameObject arrowBone, float delay)
	{
		yield return new WaitForSecondsRealtime(delay);
		CreateOrMoveArrow(arrowBone.transform);
	}

	private void ShowInnkeeperQuote(string quoteString, GameSaveKeySubkeyId flagSubkey)
	{
		m_dialogueNotification = NotificationManager.Get().CreateInnkeeperQuote(UserAttentionBlocker.NONE, GameStrings.Get(quoteString), null);
		if (!GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.FTUE, flagSubkey, 1L)))
		{
			m_logger.Log(Blizzard.T5.Core.LogLevel.Error, "[BoxRailroadManager] Failed to save flag for 'has seen journal introduction' subkey.");
			Debug.LogError("");
		}
	}
}
