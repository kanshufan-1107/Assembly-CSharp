using System;
using Assets;
using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

namespace Hearthstone;

[RequireComponent(typeof(WidgetTemplate))]
public class LettuceVillageZonePortal : MonoBehaviour
{
	public enum BountySetDifficultyModes
	{
		Normal,
		Heroic,
		Mythic
	}

	public const string HIDE_ZONE_PORTAL = "HIDE_ZONE_PORTAL_ON_SELECTION";

	public const string NAVIGATE_TO_MAP = "NAVIGATE_TO_MAP";

	public const string MODE_SELECTED = "MODE_SELECTED";

	public const string HEROIC_FANFARE_COMPLETE = "HEROIC_STOPPED";

	public const string MYTHIC_FANFARE_COMPLETE = "MYTHIC_STOPPED";

	public const string SHOWING_ZONE_PORTAL = "SHOWING_ZONE_PORTAL";

	public const string HEROIC_FANFARE_TRIGGER = "HEROIC";

	public const string MYTHIC_FANFARE_TRIGGER = "MYTHIC";

	public const int AHUNE_BOUNTY_ID = 78;

	private bool m_isFanfaring;

	public AsyncReference m_ChooserTrayReference;

	public AsyncReference m_ChooseButtonReference;

	public VisualController m_VisualController;

	public GameObject m_PopupRoot;

	private MercenaryVillageZonePortalDataModel m_dataModel;

	private LettuceLobbyChooserTray m_lettuceLobbyChooserTray;

	private PlayButton m_chooseButton;

	private void Awake()
	{
		GetComponent<WidgetTemplate>().RegisterEventListener(delegate(string eventName)
		{
			switch (eventName)
			{
			case "NAVIGATE_TO_MAP":
				NavigateToMap();
				break;
			case "HEROIC_STOPPED":
				OnHeroicFanfareComplete();
				break;
			case "MYTHIC_STOPPED":
				OnMythicFanfareComplete();
				break;
			case "SHOWING_ZONE_PORTAL":
				OnShowZonePortal();
				break;
			}
		});
	}

	private void Start()
	{
		m_ChooserTrayReference.RegisterReadyListener<LettuceLobbyChooserTray>(OnChooserTrayReady);
		m_ChooseButtonReference.RegisterReadyListener<PlayButton>(OnChooseButtonReady);
		if (m_VisualController != null)
		{
			WidgetUtils.BindorCreateDataModel(GetComponent<Widget>(), 307, ref m_dataModel);
		}
	}

	public void SetDataModel(MercenaryVillageZonePortalDataModel dataModel)
	{
		m_dataModel = dataModel;
	}

	public void OnChooseButtonReady(PlayButton buttonController)
	{
		if (buttonController == null)
		{
			Error.AddDevWarning("UI Error!", "PlayButton could not be found! You will not be able to click 'Play'!");
			return;
		}
		m_chooseButton = buttonController;
		m_chooseButton.AddEventListener(UIEventType.RELEASE, ChooseButtonRelease);
		m_chooseButton.Disable(keepLabelTextVisible: true);
	}

	public void OnChooserTrayReady(LettuceLobbyChooserTray lettuceLobbyChooserTray)
	{
		if (lettuceLobbyChooserTray == null)
		{
			Error.AddDevWarning("UI Error!", "LettuceLobbyChooserTray could not be found!");
			return;
		}
		m_lettuceLobbyChooserTray = lettuceLobbyChooserTray;
		m_lettuceLobbyChooserTray.OnModeSelected += OnModeSelected;
	}

	public void ChooseButtonRelease(UIEvent e)
	{
		SendEventUpwardStateAction.SendEventUpward(base.gameObject, "HIDE_ZONE_PORTAL_ON_SELECTION");
	}

	public bool TryNavigatingDirectlyToMap()
	{
		NetCache.NetCacheLettuceMap cachedLettuceMap = NetCache.Get().GetNetObject<NetCache.NetCacheLettuceMap>();
		if (cachedLettuceMap?.Map?.Active != true)
		{
			return false;
		}
		LettuceBountyDbfRecord bountyRecord = GameDbf.LettuceBounty.GetRecord((int)cachedLettuceMap.Map.BountyId);
		if (bountyRecord == null)
		{
			return false;
		}
		LettuceBountySetDbfRecord bountySetRecord = GameDbf.LettuceBountySet.GetRecord(bountyRecord.BountySetId);
		if (bountySetRecord == null)
		{
			return false;
		}
		LettuceVillageDisplay.LettuceSceneTransitionPayload sceneTransitionPayload = new LettuceVillageDisplay.LettuceSceneTransitionPayload
		{
			m_SelectedBountySet = bountySetRecord,
			m_DifficultyMode = bountyRecord.DifficultyMode
		};
		if (bountySetRecord.IsTutorial)
		{
			sceneTransitionPayload.m_SelectedBounty = GameDbf.LettuceBounty.GetRecord((LettuceBountyDbfRecord r) => r.BountySetId == bountySetRecord.ID);
		}
		EventDataModel eventData = new EventDataModel();
		eventData.Payload = new LettuceVillageDisplay.ZoneTransitionInfo
		{
			mode = SceneMgr.Mode.LETTUCE_MAP,
			transitionPayload = sceneTransitionPayload
		};
		SendEventUpwardStateAction.SendEventUpward(base.gameObject, "TRANSITION_SCENE", eventData);
		return true;
	}

	private void NavigateToMap()
	{
		if (m_lettuceLobbyChooserTray == null)
		{
			return;
		}
		LettuceLobbyChooserTray.SelectedOptionInfo selectedOption = m_lettuceLobbyChooserTray.GetSelectedModeInfo();
		if (selectedOption == null)
		{
			Debug.LogErrorFormat("No sub button selected!");
			return;
		}
		m_chooseButton.Disable(keepLabelTextVisible: true);
		LettuceVillageDisplay.LettuceSceneTransitionPayload sceneTransitionPayload = new LettuceVillageDisplay.LettuceSceneTransitionPayload
		{
			m_SelectedBountySet = selectedOption.BountySetRecord,
			m_DifficultyMode = selectedOption.Difficulty
		};
		if (selectedOption.BountySetRecord != null && selectedOption.BountySetRecord.IsTutorial)
		{
			sceneTransitionPayload.m_SelectedBounty = GameDbf.LettuceBounty.GetRecord((LettuceBountyDbfRecord r) => r.BountySetId == selectedOption.BountySetRecord.ID);
		}
		SceneMgr.Mode nextMode = selectedOption.Mode;
		if (nextMode == SceneMgr.Mode.LETTUCE_BOUNTY_BOARD)
		{
			NetCache.NetCacheLettuceMap cachedLettuceMap = NetCache.Get().GetNetObject<NetCache.NetCacheLettuceMap>();
			if (cachedLettuceMap != null && cachedLettuceMap.Map != null && cachedLettuceMap.Map.Active)
			{
				nextMode = SceneMgr.Mode.LETTUCE_MAP;
			}
		}
		LettuceVillageDisplay.ZoneTransitionInfo transitionInfo = new LettuceVillageDisplay.ZoneTransitionInfo
		{
			mode = nextMode,
			transitionPayload = sceneTransitionPayload
		};
		EventDataModel eventData = new EventDataModel();
		eventData.Payload = transitionInfo;
		SendEventUpwardStateAction.SendEventUpward(base.gameObject, "TRANSITION_SCENE", eventData);
	}

	private void OnShowZonePortal()
	{
		CheckForAndMaybeShowFanfare();
	}

	public void CheckForAndMaybeShowFanfare()
	{
		if (m_isFanfaring)
		{
			return;
		}
		if (LettuceVillageDataUtil.IsDifficultyUnlocked(LettuceBounty.MercenariesBountyDifficulty.HEROIC))
		{
			GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.MERCENARIES, GameSaveKeySubkeyId.MERCENARIES_HAS_SEEN_HEROIC_PVE_ZONE_FANFARE, out long value);
			if (value == 0L)
			{
				m_isFanfaring = true;
				SendEventUpwardStateAction.SendEventUpward(base.gameObject, "HEROIC");
				return;
			}
		}
		if (LettuceVillageDataUtil.IsDifficultyUnlocked(LettuceBounty.MercenariesBountyDifficulty.MYTHIC))
		{
			GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.MERCENARIES, GameSaveKeySubkeyId.MERCENARIES_HAS_SEEN_MYTHIC_PVE_ZONE_FANFARE, out long value2);
			if (value2 == 0L)
			{
				m_isFanfaring = true;
				SendEventUpwardStateAction.SendEventUpward(base.gameObject, "MYTHIC");
			}
		}
	}

	private void OnHeroicFanfareComplete()
	{
		m_isFanfaring = false;
		if (LettuceVillageDataUtil.IsDifficultyUnlocked(LettuceBounty.MercenariesBountyDifficulty.HEROIC))
		{
			GameUtils.SetGSDFlag(GameSaveKeyId.MERCENARIES, GameSaveKeySubkeyId.MERCENARIES_HAS_SEEN_HEROIC_PVE_ZONE_FANFARE, enableFlag: true);
			SendEventUpwardStateAction.SendEventUpward(base.gameObject, "UPDATE_VILLAGE_BUILDINGS");
		}
	}

	private void OnMythicFanfareComplete()
	{
		m_isFanfaring = false;
		if (LettuceVillageDataUtil.IsDifficultyUnlocked(LettuceBounty.MercenariesBountyDifficulty.MYTHIC))
		{
			GameUtils.SetGSDFlag(GameSaveKeyId.MERCENARIES, GameSaveKeySubkeyId.MERCENARIES_HAS_SEEN_MYTHIC_PVE_ZONE_FANFARE, enableFlag: true);
			SendEventUpwardStateAction.SendEventUpward(base.gameObject, "UPDATE_VILLAGE_BUILDINGS");
		}
	}

	private void SetLockedReasonOnDataModel(MercenariesDataUtil.MercenariesBountyLockedReason lockedReason, LettuceBountySetDbfRecord bountySetRecord, string optionalCustomText = null)
	{
		string reasonString = null;
		if (lockedReason == MercenariesDataUtil.MercenariesBountyLockedReason.COMING_SOON)
		{
			m_dataModel.IsSelectedModeComingSoon = true;
		}
		else
		{
			if (!string.IsNullOrEmpty(optionalCustomText))
			{
				m_dataModel.SelectedModeLockedReason = optionalCustomText;
				return;
			}
			m_dataModel.IsSelectedModeComingSoon = false;
			switch (lockedReason)
			{
			case MercenariesDataUtil.MercenariesBountyLockedReason.EVENT_NOT_STARTED:
			case MercenariesDataUtil.MercenariesBountyLockedReason.EVENT_NOT_COMPLETE:
				reasonString = "GLUE_LETTUCE_BOUNTY_LOCKED_REASON_EVENT_NOT_STARTED";
				break;
			case MercenariesDataUtil.MercenariesBountyLockedReason.EVENT_NOT_ACTIVE:
				reasonString = "GLUE_LETTUCE_BOUNTY_LOCKED_REASON_EVENT_NOT_ACTIVE";
				break;
			case MercenariesDataUtil.MercenariesBountyLockedReason.EVENT_ENDED:
				reasonString = "GLUE_LETTUCE_BOUNTY_LOCKED_REASON_EVENT_ENDED";
				break;
			case MercenariesDataUtil.MercenariesBountyLockedReason.PVE_BUILDING_NEEDS_UPGRADE:
				reasonString = "GLUE_LETTUCE_BOUNTY_LOCKED_REASON_BLDG_NEEDS_UPGRADE";
				break;
			case MercenariesDataUtil.MercenariesBountyLockedReason.PREVIOUS_ZONES_INCOMPLETE:
				reasonString = ((bountySetRecord.RequiredCompletedBounty != 78) ? "GLUE_LETTUCE_BOUNTY_LOCKED_REASON_PRV_ZONES_INCOMPLETE" : "GLUE_LETTUCE_BOUNTY_LOCKED_REASON_PRV_ZONES_INCOMPLETE_AHUNE");
				break;
			case MercenariesDataUtil.MercenariesBountyLockedReason.CURRENT_BOUNTY_UNFINISHED:
				reasonString = "GLUE_LETTUCE_COMPLETE_RUN";
				break;
			}
		}
		if (reasonString == null)
		{
			m_dataModel.SelectedModeLockedReason = "";
		}
		else
		{
			m_dataModel.SelectedModeLockedReason = GameStrings.Get(reasonString);
		}
	}

	public void OnModeSelected()
	{
		LettuceLobbyChooserTray.SelectedOptionInfo selectedOption = m_lettuceLobbyChooserTray.GetSelectedModeInfo();
		if (!selectedOption.Locked)
		{
			m_dataModel.IsSelectedModeLocked = false;
			m_dataModel.IsSelectedModeComingSoon = false;
			m_chooseButton.Enable();
		}
		else
		{
			m_dataModel.IsSelectedModeLocked = true;
			SetLockedReasonOnDataModel(selectedOption.LockedReason, selectedOption.BountySetRecord, selectedOption.CustomLockedText);
			m_chooseButton.Disable(keepLabelTextVisible: true);
		}
		LettuceBountySetDbfRecord record = selectedOption.BountySetRecord;
		m_dataModel.SelectedZoneName = record.Name;
		switch (selectedOption.Difficulty)
		{
		case LettuceBounty.MercenariesBountyDifficulty.HEROIC:
			m_dataModel.SelectedZoneDescription = record.DescriptionHeroic;
			m_dataModel.SelectedModeDifficulty = BountySetDifficultyModes.Heroic;
			break;
		case LettuceBounty.MercenariesBountyDifficulty.MYTHIC:
			m_dataModel.SelectedZoneDescription = record.DescriptionMythic;
			m_dataModel.SelectedModeDifficulty = BountySetDifficultyModes.Mythic;
			break;
		default:
			m_dataModel.SelectedZoneDescription = record.DescriptionNormal;
			m_dataModel.SelectedModeDifficulty = BountySetDifficultyModes.Normal;
			break;
		}
		if (!string.IsNullOrEmpty(record.ZoneArtTexture))
		{
			ObjectCallback callback = delegate(AssetReference assetRef, UnityEngine.Object obj, object callbackData)
			{
				m_dataModel.SelectedZoneTexture = obj as Texture;
				SendEventUpwardStateAction.SendEventUpward(base.gameObject, "MODE_SELECTED");
			};
			if (!AssetLoader.Get().LoadTexture(record.ZoneArtTexture, callback))
			{
				callback(record.ZoneArtTexture, null, null);
			}
		}
		BnetBar bnetBar = BnetBar.Get();
		NetCache.NetCacheMercenariesPlayerInfo playerInfo = NetCache.Get().GetNetObject<NetCache.NetCacheMercenariesPlayerInfo>();
		if (playerInfo != null && bnetBar != null && bnetBar.TryGetServerTimeUTC(out var serverNow))
		{
			TimeSpan difference = playerInfo.GeneratedBountyResetTime - serverNow;
			m_dataModel.BossRushDaysTillReset = difference.Days;
			m_dataModel.BossRushHoursTillReset = difference.Hours;
		}
	}
}
