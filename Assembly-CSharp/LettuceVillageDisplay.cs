using Assets;
using Hearthstone;
using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

[CustomEditClass]
[RequireComponent(typeof(WidgetTemplate))]
public class LettuceVillageDisplay : AbsSceneDisplay
{
	public class LettuceSceneTransitionPayload
	{
		public LettuceBountySetDbfRecord m_SelectedBountySet;

		public LettuceBounty.MercenariesBountyDifficulty m_DifficultyMode;

		public LettuceBountyDbfRecord m_SelectedBounty;

		public int m_MythicLevel;

		public long m_TeamId;

		public long m_CoOpPartnerTeamId;
	}

	public class ZoneTransitionInfo
	{
		public SceneMgr.Mode mode;

		public LettuceSceneTransitionPayload transitionPayload;
	}

	private const string PLAY_INTRO_ANIMATION = "PLAY_INTRO_ANIMATION";

	public const string UPDATE_VILLAGE_BUILDINGS = "UPDATE_VILLAGE_BUILDINGS";

	private bool m_mapReceived;

	private WidgetTemplate m_widget;

	public AsyncReference m_popupManagerReference;

	public AsyncReference m_LettuceVillagePC;

	public AsyncReference m_LettuceVillagePhone;

	private LettuceVillagePopupManager m_VillagePopupManager;

	private LettuceVillage m_Village;

	private VisualController m_VillageController;

	private bool m_villagePopupManagerFinishedLoading;

	private bool m_lettuceVillageFinishedChangingStates;

	private void Awake()
	{
		m_widget = GetComponent<WidgetTemplate>();
		m_widget.RegisterEventListener(HandleEvent);
	}

	public override void Start()
	{
		base.Start();
		NetCache.Get().RegisterUpdatedListener(typeof(NetCache.NetCacheLettuceMap), OnLettuceMapNetCacheUpdated);
		InitializeLettuceMap();
		SetupVillage();
	}

	public void OnDestroy()
	{
		NetCache.Get()?.RemoveUpdatedListener(typeof(NetCache.NetCacheLettuceMap), OnLettuceMapNetCacheUpdated);
	}

	private void OnVillagePopupManagerReady(LettuceVillagePopupManager popupManager)
	{
		if (popupManager == null)
		{
			Debug.LogError("Failed to load village popup manager");
			return;
		}
		m_VillagePopupManager = popupManager;
		m_villagePopupManagerFinishedLoading = true;
		if (m_Village != null)
		{
			m_Village.SetUIReferences(this, m_VillagePopupManager);
		}
	}

	private void OnVillageReady(VisualController controller)
	{
		m_clickBlocker.SetActive(value: true);
		SetVillageController(controller);
		LettuceBountyDbfRecord bountyRecord = (m_sceneTransitionPayload as LettuceSceneTransitionPayload)?.m_SelectedBounty;
		if (SceneMgr.Get().GetPrevMode() == SceneMgr.Mode.HUB || (SceneMgr.Get().GetPrevMode() == SceneMgr.Mode.LETTUCE_MAP && LettuceVillageDataUtil.IsBountyTutorial(bountyRecord)))
		{
			controller.SetState("PLAY_INTRO_ANIMATION");
		}
		else
		{
			m_Village.OnVillageEntered();
		}
	}

	private void SetVillageController(VisualController controller)
	{
		if (m_VillageController != null)
		{
			if (controller != m_VillageController)
			{
				Debug.LogErrorFormat("Both {0} and {1} are active! (only one is allowed)", m_VillageController.transform.parent.name, controller.transform.parent.name);
			}
			return;
		}
		m_VillageController = controller;
		m_Village = controller.GetComponent<LettuceVillage>();
		if (m_villagePopupManagerFinishedLoading)
		{
			m_Village.SetUIReferences(this, m_VillagePopupManager);
		}
		m_VillageController.GetComponent<Widget>().RegisterDoneChangingStatesListener(OnVillageWidgetDoneChangingStates);
	}

	private void HandleEvent(string eventName)
	{
		if (!(eventName == "TRANSITION_SCENE"))
		{
			if (eventName == "UPDATE_VILLAGE_BUILDINGS")
			{
				m_Village.UpdateBuildingStates();
			}
			return;
		}
		object payload = m_widget.GetDataModel<EventDataModel>().Payload;
		if (payload is ZoneTransitionInfo)
		{
			ZoneTransitionInfo transitionInfo = payload as ZoneTransitionInfo;
			if (!Network.IsLoggedIn() && !CanNavigateToSceneWhileOffline(transitionInfo.mode))
			{
				DialogManager.Get().ShowReconnectHelperDialog();
			}
			else
			{
				SetNextModeAndHandleTransition(transitionInfo.mode, SceneMgr.TransitionHandlerType.NEXT_SCENE, transitionInfo.transitionPayload);
			}
		}
	}

	private void OnVillageWidgetDoneChangingStates(object widget)
	{
		m_lettuceVillageFinishedChangingStates = true;
	}

	public override bool IsFinishedLoading(out string failureMessage)
	{
		if (!m_villagePopupManagerFinishedLoading)
		{
			failureMessage = "LettuceDisplay - Village popup manager never loaded.";
			return false;
		}
		m_VillagePopupManager.VillageIsReady = false;
		if (m_Village == null)
		{
			failureMessage = "LettuceDisplay - Village never loaded";
			return false;
		}
		if (!m_Village.VillageIsReady)
		{
			failureMessage = "LettuceDisplay - Village never became ready";
			return false;
		}
		if (!CollectionManager.Get().IsLettuceLoaded())
		{
			failureMessage = "LettuceDisplay - Lettuce Collection Manager never loaded.";
			return false;
		}
		if (!LettuceVillageDataUtil.Initialized)
		{
			failureMessage = "LettuceDisplay - Village Data not initialized";
			return false;
		}
		if (!m_mapReceived)
		{
			failureMessage = "LettuceDisplay - Map not received";
			return false;
		}
		if (!m_lettuceVillageFinishedChangingStates)
		{
			failureMessage = "LettuceVillageDisplay - Village never finished changing states.";
			return false;
		}
		if (m_VillagePopupManager.GetTaskBoardManager() != null && !m_VillagePopupManager.GetTaskBoardManager().IsTaskBoardReady())
		{
			failureMessage = "LettuceVillageDisplay - Task board did not finish loading.";
			return false;
		}
		m_VillagePopupManager.VillageIsReady = true;
		failureMessage = string.Empty;
		return true;
	}

	public LettuceVillage GetVillage()
	{
		return m_Village;
	}

	public void ShowPvEZonePortal()
	{
		m_VillagePopupManager.Show(LettuceVillagePopupManager.PopupType.PVE);
	}

	protected override bool ShouldStartShown()
	{
		if (SceneMgr.Get().GetPrevMode() != SceneMgr.Mode.LETTUCE_BOUNTY_BOARD && SceneMgr.Get().GetPrevMode() != SceneMgr.Mode.LETTUCE_MAP && SceneMgr.Get().GetPrevMode() != SceneMgr.Mode.LETTUCE_COLLECTION)
		{
			return SceneMgr.Get().GetPrevMode() != SceneMgr.Mode.LETTUCE_PLAY;
		}
		return false;
	}

	private void SetupVillage()
	{
		m_popupManagerReference.RegisterReadyListener<LettuceVillagePopupManager>(OnVillagePopupManagerReady);
		m_LettuceVillagePC.RegisterReadyListener<VisualController>(OnVillageReady);
		m_LettuceVillagePhone.RegisterReadyListener<VisualController>(OnVillageReady);
	}

	private void InitializeLettuceMap()
	{
		if (Network.IsLoggedIn())
		{
			Network.Get().RequestLettuceMap();
		}
		else
		{
			m_mapReceived = true;
		}
	}

	private void OnLettuceMapNetCacheUpdated()
	{
		m_mapReceived = true;
	}

	private bool CanNavigateToSceneWhileOffline(SceneMgr.Mode nextMode)
	{
		if (nextMode == SceneMgr.Mode.LETTUCE_MAP)
		{
			NetCache.NetCacheLettuceMap cachedLettuceMap = NetCache.Get().GetNetObject<NetCache.NetCacheLettuceMap>();
			if (cachedLettuceMap != null && cachedLettuceMap.Map != null)
			{
				return cachedLettuceMap.Map.Active;
			}
			return false;
		}
		return true;
	}
}
