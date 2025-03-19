using System;
using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.MaterialService.Extensions;
using Hearthstone;
using Hearthstone.Core;
using Hearthstone.DataModels;
using Hearthstone.Util;
using PegasusLettuce;
using UnityEngine;

[CustomEditClass]
public class DeckTrayTeamListContent : DeckTrayReorderableContent
{
	public delegate void BusyWithTeam(bool busy);

	public delegate void TeamCountChanged(int teamCount);

	[CustomEditField(Sections = "Team Tray Settings")]
	public Transform m_teamEditTopPos;

	[CustomEditField(Sections = "Team Tray Settings")]
	public Transform m_traySectionStartPos;

	[CustomEditField(Sections = "Team Tray Settings")]
	public GameObject m_teamInfoTooltipBone;

	[CustomEditField(Sections = "Team Tray Settings")]
	public GameObject m_teamOptionsBone;

	[CustomEditField(Sections = "Prefabs", T = EditType.GAME_OBJECT)]
	public string m_teamOptionsPrefab;

	[CustomEditField(Sections = "Prefabs")]
	public TraySection m_traySectionPrefab;

	[CustomEditField(Sections = "Prefabs")]
	public DeckTray m_deckTray;

	[CustomEditField(Sections = "Prefabs", T = EditType.GAME_OBJECT)]
	public string m_teamInfoActorPrefab;

	[CustomEditField(Sections = "Team Button Settings")]
	public ParticleSystem m_deleteTeamPoof;

	[CustomEditField(Sections = "Team Button Settings")]
	public Vector3 m_deleteTeamPoofVisualOffset;

	[SerializeField]
	private Vector3 m_teamButtonOffset;

	[CustomEditField(Sections = "Team Button Settings")]
	public GameObject m_newTeamButtonContainer;

	[CustomEditField(Sections = "Team Button Settings")]
	public CollectionDeckTrayButton m_newTeamButton;

	protected const float TIME_BETWEEN_TRAY_DOOR_ANIMS = 0.015f;

	protected const int MAX_NUM_DECKBOXES_AVAILABLE = 9;

	protected const int NUM_DECKBOXES_TO_DISPLAY = 11;

	protected static readonly Vector3 DELETE_DECKBOX_POSITION_OFFSET = Vector3.down;

	protected CollectionTeamInfo m_teamInfoTooltip;

	protected List<TraySection> m_traySections = new List<TraySection>();

	protected TraySection m_editingTraySection;

	protected int m_centeringTeamList = -1;

	protected TeamOptionsMenu m_teamOptionsMenu;

	protected bool m_initialized;

	protected bool m_animatingExit;

	protected bool m_doneEntering;

	private bool m_wasTouchModeEnabled;

	private List<TeamCountChanged> m_teamCountChangedListeners = new List<TeamCountChanged>();

	private List<BusyWithTeam> m_busyWithTeamListeners = new List<BusyWithTeam>();

	protected string m_previousTeamName;

	protected const float DELETE_TEAM_ANIM_TIME = 0.5f;

	protected bool m_deletingTeams;

	protected bool m_waitingToDeleteTeam;

	protected List<LettuceTeam> m_teamsToDelete = new List<LettuceTeam>();

	protected TraySection m_newlyCreatedTraySection;

	private ScreenEffectsHandle m_screenEffectsHandle;

	private const float TRAY_MATERIAL_Y_OFFSET = -0.0825f;

	[CustomEditField(Sections = "Team Button Settings")]
	public Vector3 TeamButtonOffset
	{
		get
		{
			return m_teamButtonOffset;
		}
		set
		{
			m_teamButtonOffset = value;
			UpdateNewTeamButton();
		}
	}

	public bool IsShowingTeamOptions
	{
		get
		{
			if (m_teamOptionsMenu != null)
			{
				return m_teamOptionsMenu.IsShown;
			}
			return false;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		CollectionManager.Get();
		HearthstoneApplication hearthstoneApplication = HearthstoneApplication.Get();
		if (hearthstoneApplication != null)
		{
			hearthstoneApplication.WillReset += WillReset;
		}
		m_screenEffectsHandle = new ScreenEffectsHandle(this);
	}

	protected void Update()
	{
		UpdateDragToReorder();
		if (m_wasTouchModeEnabled != UniversalInputManager.Get().IsTouchMode())
		{
			m_wasTouchModeEnabled = UniversalInputManager.Get().IsTouchMode();
			if (UniversalInputManager.Get().IsTouchMode() && m_teamInfoTooltip != null)
			{
				HideTeamInfo();
			}
		}
	}

	protected override void OnDestroy()
	{
		CollectionManager.Get().RemoveTeamDeletedListener(OnTeamDeleted);
		HearthstoneApplication hearthstoneApplication = HearthstoneApplication.Get();
		if (hearthstoneApplication != null)
		{
			hearthstoneApplication.WillReset -= WillReset;
		}
		if (Box.Get() != null)
		{
			Box.Get().RemoveTransitionFinishedListener(OnBoxTransitionFinished);
		}
		base.OnDestroy();
	}

	protected void Initialize()
	{
		if (m_initialized)
		{
			return;
		}
		m_newTeamButton.AddEventListener(UIEventType.RELEASE, delegate
		{
			OnNewTeamButtonPress();
		});
		CollectionManager.Get().RegisterTeamDeletedListener(OnTeamDeleted);
		GameObject go = AssetLoader.Get().InstantiatePrefab(m_teamInfoActorPrefab, AssetLoadingOptions.IgnorePrefabPosition);
		if (go == null)
		{
			Debug.LogError($"Unable to load actor {m_teamInfoActorPrefab}: null", base.gameObject);
			return;
		}
		m_teamInfoTooltip = go.GetComponent<CollectionTeamInfo>();
		if (m_teamInfoTooltip == null)
		{
			Debug.LogError($"Actor {m_teamInfoActorPrefab} does not contain CollectionDeckInfo component.", base.gameObject);
			return;
		}
		GameUtils.SetParent(m_teamInfoTooltip, m_teamInfoTooltipBone);
		m_teamInfoTooltip.RegisterHideListener(HideTeamInfoListener);
		go = AssetLoader.Get().InstantiatePrefab(m_teamOptionsPrefab);
		m_teamOptionsMenu = go.GetComponent<TeamOptionsMenu>();
		GameUtils.SetParent(m_teamOptionsMenu.gameObject, m_teamOptionsBone);
		m_teamOptionsMenu.SetTeamInfo(m_teamInfoTooltip);
		HideTeamInfo();
		CreateTraySections();
		m_initialized = true;
	}

	protected void HideTeamInfoListener()
	{
		if (m_editingTraySection != null)
		{
			LayerUtils.SetLayer(m_editingTraySection.m_deckBox.gameObject, GameLayer.Default);
			LayerUtils.SetLayer(m_teamOptionsMenu.gameObject, GameLayer.Default);
			m_editingTraySection.m_deckBox.HideRenameVisuals();
		}
		m_screenEffectsHandle.StopEffect(0.25f);
		UniversalInputManager inputManager = UniversalInputManager.Get();
		if (inputManager != null && inputManager.IsTouchMode())
		{
			if (m_editingTraySection != null)
			{
				m_editingTraySection.m_deckBox.SetHighlightState(ActorStateType.NONE);
				m_editingTraySection.m_deckBox.ShowDeckName();
			}
			if (inputManager.IsTextInputActive())
			{
				inputManager.CancelTextInput(base.gameObject);
			}
		}
		m_teamOptionsMenu.Hide();
	}

	protected virtual void ShowTeamInfo()
	{
		if (!UniversalInputManager.Get().IsTouchMode() && m_editingTraySection != null)
		{
			m_editingTraySection.m_deckBox.ShowRenameVisuals();
		}
		LayerUtils.SetLayer(m_editingTraySection.m_deckBox.gameObject, GameLayer.IgnoreFullScreenEffects);
		LayerUtils.SetLayer(m_teamInfoTooltip.gameObject, GameLayer.IgnoreFullScreenEffects);
		LayerUtils.SetLayer(m_teamOptionsMenu.gameObject, GameLayer.IgnoreFullScreenEffects);
		ScreenEffectParameters screenEffectParameters = ScreenEffectParameters.DesaturatePerspective;
		screenEffectParameters.Time = 0.25f;
		m_screenEffectsHandle.StartEffect(screenEffectParameters);
		m_teamInfoTooltip.Show();
		m_teamOptionsMenu.Show();
	}

	protected void HideTeamInfo()
	{
		m_teamInfoTooltip.Hide();
	}

	private void WillReset()
	{
		Processor.CancelScheduledCallback(BeginAnimation);
		Processor.CancelScheduledCallback(EndAnimation);
	}

	private void OnNewTeamButtonPress()
	{
		if (IsModeActive() && !base.IsTouchDragging)
		{
			SoundManager.Get().LoadAndPlay("Hub_Click.prefab:cc2cf2b5507827149b13d12210c0f323");
			StartCoroutine(StartCreateNewTeam());
		}
	}

	protected IEnumerator StartCreateNewTeam()
	{
		ShowNewTeamButton(newTeamButtonActive: false, null, null, disableOnHide: false);
		CreateNewTeam();
		if (CollectionManager.Get().GetCollectibleDisplay() != null)
		{
			CollectionManager.Get().GetCollectibleDisplay().EnableInput(enable: true);
		}
		CollectionDeckTray deckTray = CollectionDeckTray.Get();
		while (deckTray != null && deckTray.IsUpdatingTrayMode())
		{
			yield return null;
		}
		if (deckTray != null)
		{
			deckTray.m_doneButton.SetEnabled(enabled: true);
		}
	}

	protected void EndCreateNewTeam(bool newTeam)
	{
		ShowNewTeamButton(newTeamButtonActive: true, delegate
		{
			if (newTeam)
			{
				UpdateAllTrays(immediate: true);
			}
		});
	}

	private void DeleteQueuedTeams(bool force = false)
	{
		if (m_teamsToDelete.Count == 0 || (!IsModeActive() && !force))
		{
			return;
		}
		foreach (LettuceTeam team in m_teamsToDelete)
		{
			Network.Get().DeleteTeam(team.ID);
			CollectionManager.Get().AddPendingTeamDelete(team.ID);
			if (!Network.IsLoggedIn() || team.ID <= 0)
			{
				CollectionManager.Get().OnTeamDeletedWhileOffline(team.ID);
			}
		}
		m_teamsToDelete.Clear();
	}

	private void OnTeamDeleted(LettuceTeam removedTeam)
	{
		if (removedTeam != null)
		{
			m_waitingToDeleteTeam = false;
			long teamID = removedTeam.ID;
			StartCoroutine(DeleteTeamAnimation(teamID));
		}
	}

	public override void OnEditingTeamChanged(LettuceTeam newTeam, LettuceTeam oldTeam, bool isNewTeam)
	{
		if (newTeam != null && m_teamInfoTooltip != null && m_teamOptionsMenu != null)
		{
			m_teamOptionsMenu.SetTeam(newTeam);
		}
		if (isNewTeam && newTeam != null)
		{
			InitializeSortOrderFromTraysIfNeeded();
		}
		if (IsModeActive())
		{
			InitializeTraysFromTeams();
		}
		if (isNewTeam && newTeam != null)
		{
			CollectionUtils.PopulateMercenariesTeamDataModel(new LettuceTeamDataModel(), newTeam);
			m_newlyCreatedTraySection = GetExistingTrayFromTeam(newTeam);
			if (m_newlyCreatedTraySection != null)
			{
				m_centeringTeamList = m_newlyCreatedTraySection.m_deckBox.GetPositionIndex();
			}
			newTeam.SendChanges();
		}
		List<LettuceMercenary> updates = null;
		if (newTeam == null && oldTeam != null)
		{
			updates = oldTeam.GetMercs();
		}
		else if (newTeam != null)
		{
			updates = newTeam.GetMercs();
		}
		if (updates == null)
		{
			return;
		}
		LettuceCollectionDisplay lcd = CollectionManager.Get().GetCollectibleDisplay() as LettuceCollectionDisplay;
		if (!(lcd != null))
		{
			return;
		}
		LettuceCollectionPageManager lcpm = lcd.GetPageManager() as LettuceCollectionPageManager;
		if (!(lcpm != null))
		{
			return;
		}
		foreach (LettuceMercenary teamMerc in updates)
		{
			lcpm.UpdatePageMercenary(MercenaryFactory.CreateMercenaryDataModelWithCoin(teamMerc));
			lcpm.UpdateCurrentPageCardLocks(playSound: false);
		}
	}

	public void UpdateEditingTeamBoxVisual(string mercCardId, TAG_PREMIUM? premiumOverride = null)
	{
		if (!(m_editingTraySection == null))
		{
			m_editingTraySection.m_deckBox.SetHeroCardPremiumOverride(premiumOverride);
			m_editingTraySection.m_deckBox.SetHeroCardID(mercCardId, null);
		}
	}

	private IEnumerator DeleteTeamAnimation(long teamID, Action callback = null)
	{
		while (m_deletingTeams)
		{
			yield return null;
		}
		int delIndex = 0;
		TraySection delTraySection = null;
		TraySection newTeamButtonTrayLocation = m_traySections[0];
		for (int i = 0; i < m_traySections.Count; i++)
		{
			TraySection traySection = m_traySections[i];
			long existingTeamID = traySection.m_deckBox.GetDeckID();
			if (existingTeamID == teamID)
			{
				delIndex = i;
				delTraySection = traySection;
			}
			else if (existingTeamID == -1)
			{
				break;
			}
			newTeamButtonTrayLocation = traySection;
		}
		if (delTraySection == null)
		{
			Debug.LogWarning("Unable to delete team with ID {0}. Not found in tray sections.", base.gameObject);
			yield break;
		}
		FireBusyWithTeamEvent(busy: true);
		m_deletingTeams = true;
		FireTeamCountChangedEvent();
		m_traySections.RemoveAt(delIndex);
		GetIdealNewTeamButtonLocalPosition(newTeamButtonTrayLocation, out var newTeamBtnPos, out var newTeamBtnActive);
		Vector3 prevTraySectionPosition = delTraySection.transform.localPosition;
		SoundManager.Get().LoadAndPlay("collection_manager_delete_deck.prefab:5ca16bec63041b741a4fb33706ed9cb1", base.gameObject);
		m_deleteTeamPoof.transform.position = delTraySection.m_deckBox.transform.position + m_deleteTeamPoofVisualOffset;
		m_deleteTeamPoof.Play(withChildren: true);
		delTraySection.ClearDeckInfo();
		delTraySection.gameObject.SetActive(value: false);
		List<GameObject> animatingTraySections = new List<GameObject>();
		Action<object> onAnimationsComplete = delegate(object obj)
		{
			GameObject item = (GameObject)obj;
			animatingTraySections.Remove(item);
		};
		Vector3 delTraySectionPosition = Vector3.zero;
		for (int j = delIndex; j < m_traySections.Count; j++)
		{
			TraySection traySection2 = m_traySections[j];
			Vector3 traySectionPos = traySection2.transform.localPosition;
			iTween.MoveTo(traySection2.gameObject, iTween.Hash("position", prevTraySectionPosition, "islocal", true, "time", 0.5f, "easetype", iTween.EaseType.easeOutBounce, "oncomplete", onAnimationsComplete, "oncompleteparams", traySection2.gameObject, "name", "position"));
			animatingTraySections.Add(traySection2.gameObject);
			if (j <= 7)
			{
				delTraySectionPosition = traySectionPos;
			}
			prevTraySectionPosition = traySectionPos;
		}
		if (delIndex == 8)
		{
			delTraySectionPosition = delTraySection.transform.localPosition;
		}
		m_traySections.Insert(8, delTraySection);
		m_newTeamButton.SetIsUsable(CanShowNewTeamButton());
		delTraySection.gameObject.SetActive(value: true);
		delTraySection.HideDeckBox(immediate: true);
		delTraySection.transform.localPosition = DELETE_DECKBOX_POSITION_OFFSET + delTraySectionPosition;
		if (m_newTeamButton.gameObject.activeSelf)
		{
			iTween.MoveTo(m_newTeamButtonContainer, iTween.Hash("position", newTeamBtnPos, "islocal", true, "time", 0.5f, "easetype", iTween.EaseType.easeOutBounce, "oncomplete", onAnimationsComplete, "oncompleteparams", m_newTeamButtonContainer, "name", "position"));
			animatingTraySections.Add(m_newTeamButtonContainer);
		}
		else
		{
			m_newTeamButtonContainer.transform.localPosition = newTeamBtnPos;
		}
		while (animatingTraySections.Count > 0)
		{
			animatingTraySections.RemoveAll((GameObject obj) => obj == null || !obj.activeInHierarchy);
			yield return null;
		}
		delTraySection.transform.localPosition = delTraySectionPosition;
		if (!CollectionManager.Get().IsInEditTeamMode())
		{
			ShowNewTeamButton(newTeamBtnActive);
		}
		FireBusyWithTeamEvent(busy: false);
		callback?.Invoke();
		m_deletingTeams = false;
	}

	private void UpdateNewTeamButton(TraySection setNewTeamButtonPosition = null)
	{
		bool updated = UpdateNewTeamButtonPosition(setNewTeamButtonPosition);
		ShowNewTeamButton(updated && CanShowNewTeamButton());
	}

	private bool UpdateNewTeamButtonPosition(TraySection setNewTeamButtonPosition = null)
	{
		bool newTeamButtonActive = false;
		GetIdealNewTeamButtonLocalPosition(setNewTeamButtonPosition, out var newPos, out newTeamButtonActive);
		m_newTeamButtonContainer.transform.localPosition = newPos;
		return newTeamButtonActive;
	}

	private void GetIdealNewTeamButtonLocalPosition(TraySection setNewTeamButtonPosition, out Vector3 outPosition, out bool outActive)
	{
		TraySection lastUnusedTray = GetLastUnusedTraySection();
		TraySection newTeamButtonPosition = ((setNewTeamButtonPosition == null) ? lastUnusedTray : setNewTeamButtonPosition);
		outActive = lastUnusedTray != null;
		outPosition = ((newTeamButtonPosition != null) ? newTeamButtonPosition.transform.localPosition : m_traySectionStartPos.localPosition) + m_teamButtonOffset;
	}

	public void ShowNewTeamButton(bool newTeamButtonActive, CollectionDeckTrayButton.DelOnAnimationFinished callback = null)
	{
		ShowNewTeamButton(newTeamButtonActive, null, callback);
	}

	public void ShowNewTeamButton(bool newTeamButtonActive, float? speed, CollectionDeckTrayButton.DelOnAnimationFinished callback = null, bool disableOnHide = true)
	{
		if (m_newTeamButton.IsPoppedUp() != newTeamButtonActive)
		{
			if (newTeamButtonActive)
			{
				m_newTeamButton.gameObject.SetActive(value: true);
				m_newTeamButton.PlayPopUpAnimation(delegate
				{
					if (callback != null)
					{
						callback(this);
					}
				}, null, speed);
				return;
			}
			m_newTeamButton.PlayPopDownAnimation(delegate
			{
				if (disableOnHide)
				{
					m_newTeamButton.gameObject.SetActive(value: false);
				}
				callback?.Invoke(this);
			}, null, speed);
		}
		else if (callback != null)
		{
			callback(this);
		}
	}

	public bool IsDoneEntering()
	{
		return m_doneEntering;
	}

	public IEnumerator ShowTrayDoors(bool show)
	{
		foreach (TraySection traySection in m_traySections)
		{
			traySection.EnableDoors(show);
			traySection.ShowDoor(show);
		}
		yield return null;
	}

	public override bool AnimateContentEntranceStart()
	{
		Initialize();
		long editTeamID = -1L;
		if (m_editingTraySection != null)
		{
			editTeamID = m_editingTraySection.m_deckBox.GetDeckID();
		}
		InitializeTraysFromTeams();
		SwapEditTrayIfNeeded(editTeamID);
		bool immediate = SceneMgr.Get().IsInTavernBrawlMode();
		UpdateAllTrays(immediate, initializeTrays: false);
		if (m_editingTraySection != null)
		{
			FinishRenamingEditingTeam();
			m_editingTraySection.MoveDeckBoxBackToOriginalPosition(0.25f, delegate
			{
				m_editingTraySection = null;
			});
		}
		m_newTeamButton.SetIsUsable(CanShowNewTeamButton());
		FireBusyWithTeamEvent(busy: true);
		FireTeamCountChangedEvent();
		CollectionManager.Get().DoneEditing();
		return true;
	}

	public override bool AnimateContentEntranceEnd()
	{
		if (m_editingTraySection != null)
		{
			return false;
		}
		m_newTeamButton.SetEnabled(enabled: true);
		FireBusyWithTeamEvent(busy: false);
		DeleteQueuedTeams(force: true);
		return true;
	}

	public override bool AnimateContentExitStart()
	{
		m_animatingExit = true;
		FireBusyWithTeamEvent(busy: true);
		float? btnSpeed = null;
		if (SceneMgr.Get().IsInTavernBrawlMode())
		{
			btnSpeed = 500f;
		}
		if (m_newlyCreatedTraySection == null)
		{
			ShowNewTeamButton(newTeamButtonActive: false, btnSpeed);
		}
		Processor.ScheduleCallback(0.5f, realTime: false, BeginAnimation);
		return true;
	}

	public override bool AnimateContentExitEnd()
	{
		return !m_animatingExit;
	}

	public override bool PreAnimateContentExit()
	{
		if (m_scrollbar == null)
		{
			return true;
		}
		if (m_centeringTeamList != -1 && m_editingTraySection != null)
		{
			BoxCollider collider = m_editingTraySection.m_deckBox.GetComponent<BoxCollider>();
			if (m_scrollbar.ScrollObjectIntoView(m_editingTraySection.m_deckBox.gameObject, collider.center.y, collider.size.y / 2f, delegate
			{
				m_animatingExit = false;
			}, iTween.EaseType.linear, m_scrollbar.m_ScrollTweenTime, blockInputWhileScrolling: true))
			{
				m_animatingExit = true;
				m_centeringTeamList = -1;
			}
		}
		StartCoroutine(ShowTrayDoors(show: false));
		return !m_animatingExit;
	}

	public override bool PreAnimateContentEntrance()
	{
		m_doneEntering = false;
		StartCoroutine(ShowTrayDoors(show: false));
		return true;
	}

	private void BeginAnimation(object userData)
	{
		float animationWaitTime = 0.5f;
		foreach (TraySection traySection in m_traySections)
		{
			if (m_editingTraySection != traySection)
			{
				traySection.HideDeckBox();
			}
		}
		if (m_newlyCreatedTraySection != null)
		{
			TraySection animateTraySection = m_newlyCreatedTraySection;
			UpdateNewTeamButtonPosition(animateTraySection);
			ShowNewTeamButton(newTeamButtonActive: true, delegate
			{
				m_newTeamButton.FlipHalfOverAndHide(0.1f, delegate
				{
					animateTraySection.ShowDeckBox(immediate: true);
					animateTraySection.FlipDeckBoxHalfOverToShow(0.1f, delegate
					{
						animateTraySection.MoveDeckBoxToEditPosition(m_teamEditTopPos.position, 0.25f);
					});
				});
			});
			m_editingTraySection = m_newlyCreatedTraySection;
			m_newlyCreatedTraySection = null;
			animationWaitTime += 0.7f;
		}
		else if (m_editingTraySection != null)
		{
			m_editingTraySection.MoveDeckBoxToEditPosition(m_teamEditTopPos.position, 0.25f);
		}
		Processor.ScheduleCallback(animationWaitTime, realTime: false, EndAnimation);
	}

	private void EndAnimation(object userData)
	{
		m_animatingExit = false;
		FireBusyWithTeamEvent(busy: false);
	}

	private LettuceTeam UpdateRenamingEditingTeam(string newTeamName)
	{
		return UpdateRenamingEditingTeam_Internal(newTeamName, shouldValidateTeamName: false);
	}

	private LettuceTeam FinishUpdateRenamingEditingTeam(string newTeamName, bool shouldValidateDeckName)
	{
		return UpdateRenamingEditingTeam_Internal(newTeamName, shouldValidateDeckName);
	}

	private LettuceTeam UpdateRenamingEditingTeam_Internal(string newTeamName, bool shouldValidateTeamName)
	{
		LettuceTeam editTeam = CollectionManager.Get().GetEditingTeam();
		if (editTeam != null && !string.IsNullOrEmpty(newTeamName))
		{
			editTeam.Name = newTeamName;
			if (shouldValidateTeamName && RegionUtils.IsCNLegalRegion)
			{
				editTeam.SendTeamRenameChange();
				CollectionManager.Get().SendTeamNameChangedEvent();
			}
		}
		return editTeam;
	}

	private void OnUnfocusedDuringRename()
	{
		LettuceTeam editTeam = CollectionManager.Get().GetEditingTeam();
		string teamName = m_previousTeamName;
		if (editTeam != null)
		{
			teamName = editTeam.Name;
		}
		if (!teamName.Equals(m_previousTeamName, StringComparison.InvariantCultureIgnoreCase))
		{
			FinishRenamingEditingTeam(teamName, shouldValidateTeamName: true);
		}
	}

	private void FinishRenamingEditingTeam(string newTeamName = null, bool shouldValidateTeamName = false)
	{
		if (!(m_editingTraySection == null))
		{
			string sanitizedTeamName = TextUtils.StripHTMLTags(newTeamName);
			CollectionDeckBoxVisual editDeckBox = m_editingTraySection.m_deckBox;
			LettuceTeam editTeam = FinishUpdateRenamingEditingTeam(sanitizedTeamName, shouldValidateTeamName);
			if (editTeam != null && m_editingTraySection != null)
			{
				editDeckBox.SetDeckName(editTeam.Name);
			}
			if (UniversalInputManager.Get() != null && UniversalInputManager.Get().IsTextInputActive())
			{
				UniversalInputManager.Get().CancelTextInput(base.gameObject);
			}
			editDeckBox.ShowDeckName();
		}
	}

	private void OnDrawGizmos()
	{
		if (!(m_editingTraySection == null))
		{
			Bounds textBounds = m_editingTraySection.m_deckBox.GetDeckNameText().GetBounds();
			Gizmos.DrawWireSphere(textBounds.min, 0.1f);
			Gizmos.DrawWireSphere(textBounds.max, 0.1f);
		}
	}

	public void RegisterTeamCountUpdated(TeamCountChanged dlg)
	{
		m_teamCountChangedListeners.Add(dlg);
	}

	public void UnregisterTeamCountUpdated(TeamCountChanged dlg)
	{
		m_teamCountChangedListeners.Remove(dlg);
	}

	public void RegisterBusyWithTeam(BusyWithTeam dlg)
	{
		m_busyWithTeamListeners.Add(dlg);
	}

	public void UnregisterBusyWithTeam(BusyWithTeam dlg)
	{
		m_busyWithTeamListeners.Remove(dlg);
	}

	public virtual void HideTraySectionsNotInBounds(Bounds bounds)
	{
		int numTraysHidden = 0;
		foreach (TraySection traySection in m_traySections)
		{
			if (traySection.HideIfNotInBounds(bounds))
			{
				numTraysHidden++;
			}
		}
		Log.DeckTray.Print("Hid {0} tray sections that were not visible.", numTraysHidden);
		UIBScrollableItem scrollableItem = m_newTeamButtonContainer.GetComponent<UIBScrollableItem>();
		if (scrollableItem == null)
		{
			Debug.LogWarning("UIBScrollableItem not found on m_newDeckButtonContainer! This button may not be hidden properly while exiting Collection Manager!");
			return;
		}
		Bounds scrollItemBounds = default(Bounds);
		scrollableItem.GetWorldBounds(out var min, out var max);
		scrollItemBounds.SetMinMax(min, max);
		if (!bounds.Intersects(scrollItemBounds))
		{
			Log.DeckTray.Print("Hiding the New Deck button because it's out of the visible scroll area.");
			m_newTeamButton.gameObject.SetActive(value: false);
		}
	}

	public void CreateNewTeam(string customTeamName = null, string pastedTeamHashString = null)
	{
		PegasusLettuce.LettuceTeam.Type teamType = PegasusLettuce.LettuceTeam.Type.TYPE_SOLO;
		string teamName = customTeamName;
		if (string.IsNullOrEmpty(teamName))
		{
			teamName = CollectionManager.Get().AutoGenerateTeamName();
		}
		CollectionManager.Get().SendCreateTeam(teamName, teamType, pastedTeamHashString);
	}

	public void CreateNewTeamCancelled()
	{
		EndCreateNewTeam(newTeam: false);
	}

	public bool IsWaitingToDeleteTeam()
	{
		return m_waitingToDeleteTeam;
	}

	public int NumTeamsToDelete()
	{
		return m_teamsToDelete.Count;
	}

	public bool IsDeletingTeams()
	{
		return m_deletingTeams;
	}

	public void DeleteTeam(long teamID)
	{
		LettuceTeam team = CollectionManager.Get().GetTeam(teamID);
		if (team == null)
		{
			Log.All.PrintError("Unable to delete team id={0} - not found in cache.", teamID);
			return;
		}
		if (Network.IsLoggedIn() && teamID <= 0)
		{
			Log.Offline.PrintDebug("DeleteTeam() - Attempting to delete fake team while online.");
		}
		team.MarkBeingDeleted();
		m_teamsToDelete.Add(team);
		if (PlatformSettings.IsMobile())
		{
			LettuceCollectionDisplay lcd = CollectionManager.Get().GetCollectibleDisplay() as LettuceCollectionDisplay;
			if (lcd != null && lcd.IsMercenaryDetailsDisplayActive())
			{
				Navigation.GoBack();
			}
			Navigation.GoBack();
		}
		DeleteQueuedTeams();
	}

	public void DeleteEditingTeam()
	{
		LettuceTeam team = CollectionManager.Get().GetEditingTeam();
		if (team == null)
		{
			Debug.LogWarning("No team currently being edited!");
			return;
		}
		m_waitingToDeleteTeam = true;
		DeleteTeam(team.ID);
	}

	public void CancelRenameEditingTeam()
	{
		FinishRenamingEditingTeam();
	}

	public Vector3 GetNewTeamButtonPosition()
	{
		return m_newTeamButton.transform.localPosition;
	}

	public void UpdateTeamName(string teamName = null)
	{
		if (teamName == null)
		{
			LettuceTeam editingTeam = CollectionManager.Get().GetEditingTeam();
			if (editingTeam == null)
			{
				return;
			}
			teamName = editingTeam.Name;
		}
		FinishRenamingEditingTeam(teamName);
	}

	public void RenameCurrentlyEditingTeam()
	{
		if (m_editingTraySection == null)
		{
			Debug.LogWarning("Unable to rename team. No team currently being edited.", base.gameObject);
			return;
		}
		CollectionDeckBoxVisual editDeckBox = m_editingTraySection.m_deckBox;
		editDeckBox.HideDeckName();
		Camera camera = Box.Get().GetCamera();
		Bounds textBounds = editDeckBox.GetDeckNameText().GetBounds();
		Rect rect = CameraUtils.CreateGUIViewportRect(camera, textBounds.min, textBounds.max);
		Font currentFont = editDeckBox.GetDeckNameText().GetLocalizedFont();
		m_previousTeamName = editDeckBox.GetDeckNameText().Text;
		UniversalInputManager.TextInputParams inputParams = new UniversalInputManager.TextInputParams
		{
			m_owner = base.gameObject,
			m_rect = rect,
			m_updatedCallback = delegate(string newName)
			{
				UpdateRenamingEditingTeam(newName);
			},
			m_completedCallback = delegate(string newName)
			{
				FinishRenamingEditingTeam(newName, shouldValidateTeamName: true);
			},
			m_canceledCallback = delegate
			{
				FinishRenamingEditingTeam(m_previousTeamName, shouldValidateTeamName: true);
			},
			m_unfocusedCallback = delegate
			{
				OnUnfocusedDuringRename();
			},
			m_maxCharacters = CollectionDeck.DefaultMaxDeckNameCharacters,
			m_font = currentFont,
			m_text = editDeckBox.GetDeckNameText().Text
		};
		UniversalInputManager.Get().UseTextInput(inputParams);
	}

	protected void CreateTraySections()
	{
		Vector3 trayScale = m_traySectionStartPos.localScale;
		Vector3 trayRotation = m_traySectionStartPos.localEulerAngles;
		for (int i = 0; i < 11; i++)
		{
			TraySection traySection = (TraySection)GameUtils.Instantiate(m_traySectionPrefab, base.gameObject);
			traySection.m_deckBox.SetPositionIndex(i);
			traySection.transform.localScale = trayScale;
			traySection.transform.localEulerAngles = trayRotation;
			traySection.EnableDoors(show: false);
			CollectionDeckBoxVisual deckBox = traySection.m_deckBox;
			deckBox.AddEventListener(UIEventType.ROLLOVER, delegate
			{
				OnDeckBoxVisualOver(deckBox);
			});
			deckBox.AddEventListener(UIEventType.ROLLOUT, delegate
			{
				OnDeckBoxVisualOut(deckBox);
			});
			deckBox.AddEventListener(UIEventType.TAP, delegate
			{
				OnDeckBoxVisualRelease(traySection);
			});
			deckBox.StoreOriginalButtonPositionAndRotation();
			deckBox.HideBanner();
			m_traySections.Add(traySection);
		}
		RefreshTraySectionPositions(animateToNewPositions: false);
		if (!UniversalInputManager.UsePhoneUI)
		{
			HideTraySectionsNotInBounds(m_deckTray.m_scrollbar.m_ScrollBounds.bounds);
			Box.Get().AddTransitionFinishedListener(OnBoxTransitionFinished);
		}
	}

	private void OnBoxTransitionFinished(object userData)
	{
		Box.Get().RemoveTransitionFinishedListener(OnBoxTransitionFinished);
		foreach (TraySection traySection in m_traySections)
		{
			traySection.gameObject.SetActive(value: true);
		}
	}

	protected TraySection GetExistingTrayFromTeam(LettuceTeam team)
	{
		return GetExistingTrayFromTeam(team.ID);
	}

	private TraySection GetExistingTrayFromTeam(long teamID)
	{
		foreach (TraySection traySection in m_traySections)
		{
			if (traySection.m_deckBox.GetDeckID() == teamID)
			{
				return traySection;
			}
		}
		return null;
	}

	public TraySection GetEditingTraySection()
	{
		return m_editingTraySection;
	}

	protected void InitializeTraysFromTeams()
	{
		InitializeSortOrderFromTraysIfNeeded();
		UpdateTeamTrayVisuals();
	}

	protected void InitializeSortOrderFromTraysIfNeeded()
	{
		foreach (LettuceTeam team2 in CollectionManager.Get().GetTeams())
		{
			if (team2.SortOrder != 0)
			{
				return;
			}
		}
		int sortIndex = 0;
		for (int i = 0; i < m_traySections.Count; i++)
		{
			TraySection traySection = m_traySections[i];
			LettuceTeam team = CollectionManager.Get().GetTeam(traySection.m_deckBox.GetDeckID());
			if (team != null)
			{
				if (team.SortOrder != sortIndex)
				{
					team.SortOrder = (uint)sortIndex;
				}
				sortIndex++;
			}
		}
	}

	protected void UpdateAllTrays(bool immediate = false, bool initializeTrays = true)
	{
		if (initializeTrays)
		{
			InitializeTraysFromTeams();
		}
		List<TraySection> showTraySections = new List<TraySection>();
		foreach (TraySection traySection in m_traySections)
		{
			if (traySection.m_deckBox.GetDeckID() == -1 && !traySection.m_deckBox.IsLocked())
			{
				traySection.HideDeckBox(immediate);
			}
			else if (m_editingTraySection != traySection && !traySection.IsOpen())
			{
				showTraySections.Add(traySection);
			}
		}
		StartCoroutine(UpdateAllTraysAnimation(showTraySections, immediate));
	}

	protected virtual IEnumerator UpdateAllTraysAnimation(List<TraySection> showTraySections, bool immediate)
	{
		foreach (TraySection showTraySection in showTraySections)
		{
			showTraySection.ShowDeckBox(immediate);
			if (!immediate)
			{
				yield return new WaitForSeconds(0.015f);
			}
		}
		UpdateNewTeamButton();
		m_doneEntering = true;
	}

	public TraySection GetLastUnusedTraySection()
	{
		int index = 0;
		foreach (TraySection traySection in m_traySections)
		{
			if (index >= 9)
			{
				break;
			}
			if (traySection.m_deckBox.GetDeckID() == -1)
			{
				return traySection;
			}
			index++;
		}
		return null;
	}

	public TraySection GetLastUsedTraySection()
	{
		int index = 0;
		TraySection prev = null;
		foreach (TraySection traySection in m_traySections)
		{
			if (index >= 9)
			{
				break;
			}
			if (traySection.m_deckBox.GetDeckID() == -1)
			{
				return prev;
			}
			prev = traySection;
			index++;
		}
		return prev;
	}

	public TraySection GetTraySection(int index)
	{
		if (index >= 0 && index < m_traySections.Count)
		{
			return m_traySections[index];
		}
		return null;
	}

	public bool CanShowNewTeamButton()
	{
		if (CollectionManager.Get().GetTeams().Count < 9)
		{
			return CollectionDeckTray.Get().GetCurrentContentType() == DeckTray.DeckContentTypes.Teams;
		}
		return false;
	}

	public void SetEditingTraySection(int index)
	{
		m_editingTraySection = m_traySections[index];
		m_centeringTeamList = m_editingTraySection.m_deckBox.GetPositionIndex();
	}

	protected bool IsEditingTeam()
	{
		return CollectionManager.Get().GetEditingTeam() != null;
	}

	protected virtual void OnDeckBoxVisualOver(CollectionDeckBoxVisual deckBox)
	{
		if (!deckBox.IsLocked() && !UniversalInputManager.Get().IsTouchMode())
		{
			if (IsEditingTeam() && m_teamInfoTooltip != null)
			{
				ShowTeamInfo();
			}
			else if (!UniversalInputManager.Get().IsTouchMode() && IsModeTryingOrActive() && base.DraggingDeckBox == null)
			{
				deckBox.ShowDeleteButton(show: true);
			}
		}
	}

	private void OnDeckBoxVisualOut(CollectionDeckBoxVisual deckBox)
	{
		if (deckBox.IsLocked())
		{
			return;
		}
		if (UniversalInputManager.Get().IsTouchMode())
		{
			if (m_teamInfoTooltip != null && m_teamInfoTooltip.IsShown())
			{
				deckBox.SetHighlightState(ActorStateType.HIGHLIGHT_MOUSE_OVER);
			}
		}
		else if (!UniversalInputManager.Get().InputIsOver(deckBox.m_deleteButton.gameObject))
		{
			deckBox.ShowDeleteButton(show: false);
		}
	}

	protected void OnDeckBoxVisualRelease(TraySection traySection)
	{
		CollectionDeckBoxVisual deckBox = traySection.m_deckBox;
		if (deckBox.IsLocked())
		{
			return;
		}
		deckBox.enabled = true;
		if (base.IsTouchDragging || m_deckTray.IsUpdatingTrayMode())
		{
			return;
		}
		long teamID = deckBox.GetDeckID();
		LettuceTeam team = CollectionManager.Get().GetTeam(teamID);
		if (team != null)
		{
			if (team.IsBeingDeleted())
			{
				Log.DeckTray.Print($"DeckTrayTeamListContent.OnDeckBoxVisualRelease(): cannot edit team {team}; it is being deleted");
				return;
			}
			if (team.IsSavingChanges())
			{
				Log.DeckTray.PrintWarning("DeckTrayTeamListContent.OnDeckBoxVisualRelease(): cannot edit team {0}; waiting for changes to be saved", team);
				return;
			}
		}
		if (IsEditingTeam())
		{
			if (!UniversalInputManager.Get().IsTouchMode())
			{
				RenameCurrentlyEditingTeam();
			}
			else if (m_teamInfoTooltip != null && !m_teamInfoTooltip.IsShown())
			{
				ShowTeamInfo();
			}
		}
		else if (IsModeActive())
		{
			m_editingTraySection = traySection;
			m_centeringTeamList = m_editingTraySection.m_deckBox.GetPositionIndex();
			m_newTeamButton.SetEnabled(enabled: false);
			LettuceCollectionDisplay lcd = CollectionManager.Get().GetCollectibleDisplay() as LettuceCollectionDisplay;
			if (lcd != null)
			{
				lcd.RequestContentsToShowTeam(teamID);
			}
			Options.Get().SetBool(Option.HAS_STARTED_A_DECK, val: true);
		}
	}

	protected void FireTeamCountChangedEvent()
	{
		TeamCountChanged[] array = m_teamCountChangedListeners.ToArray();
		int count = CollectionManager.Get().GetTeams().Count;
		TeamCountChanged[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i](count);
		}
	}

	protected void FireBusyWithTeamEvent(bool busy)
	{
		BusyWithTeam[] array = m_busyWithTeamListeners.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i](busy);
		}
	}

	private int GetTotalDeckBoxesInUse()
	{
		int deckBoxesInUse = 0;
		foreach (TraySection traySection in m_traySections)
		{
			if (traySection.m_deckBox.GetDeckID() > -1)
			{
				deckBoxesInUse++;
			}
		}
		return deckBoxesInUse;
	}

	public int UpdateTeamTrayVisuals(bool suppressFX = false)
	{
		List<LettuceTeam> teams = CollectionManager.Get().GetTeams();
		int numDeckBoxes = teams.Count;
		CollectionManager.SortTeams(teams);
		for (int index = 0; index < numDeckBoxes && index < m_traySections.Count; index++)
		{
			if (index < teams.Count)
			{
				LettuceTeam team = teams[index];
				m_traySections[index].m_deckBox.AssignFromMercenariesTeam(team, suppressFX);
				m_traySections[index].m_deckBox.ShowNotificationButton(team.DoesContainDisabledMerc());
			}
			m_traySections[index].m_deckBox.SetIsLocked(index >= teams.Count);
		}
		return teams.Count;
	}

	public void OnTeamContentsUpdated(long teamID)
	{
		foreach (TraySection traySection in m_traySections)
		{
			if (traySection.m_deckBox != null)
			{
				LettuceTeam team = CollectionManager.Get().GetTeam(traySection.m_deckBox.GetDeckID());
				if (team != null)
				{
					traySection.m_deckBox.AssignFromMercenariesTeam(team);
				}
			}
		}
	}

	protected void SwapEditTrayIfNeeded(long editTeamID)
	{
		if (editTeamID < 0)
		{
			return;
		}
		TraySection realSection = null;
		foreach (TraySection tray in m_traySections)
		{
			if (tray.m_deckBox.GetDeckID() == editTeamID)
			{
				realSection = tray;
				break;
			}
		}
		if (!(realSection == m_editingTraySection))
		{
			m_deckTray.TryEnableScrollbar();
			float scroll = (float)realSection.m_deckBox.GetPositionIndex() / (float)(GetTotalDeckBoxesInUse() - 1);
			m_scrollbar.SetScrollImmediate(scroll);
			m_deckTray.SaveScrollbarPosition(DeckTray.DeckContentTypes.Decks);
			m_editingTraySection.m_deckBox.transform.localScale = CollectionDeckBoxVisual.SCALED_DOWN_LOCAL_SCALE;
			Vector3 editTrayHomePosition = Vector3.zero;
			editTrayHomePosition.y = 1.273138f;
			m_editingTraySection.m_deckBox.transform.localPosition = editTrayHomePosition;
			m_editingTraySection.m_deckBox.Hide();
			m_editingTraySection.m_deckBox.EnableButtonAnimation();
			realSection.m_deckBox.transform.localScale = CollectionDeckBoxVisual.SCALED_UP_LOCAL_SCALE;
			realSection.m_deckBox.transform.parent = null;
			realSection.m_deckBox.transform.position = m_teamEditTopPos.position;
			realSection.ShowDeckBoxNoAnim();
			realSection.m_deckBox.SetEnabled(enabled: true);
			m_editingTraySection = realSection;
		}
	}

	public override void StopDragToReorder()
	{
		if (m_draggingDeckBox != null)
		{
			foreach (LettuceTeam team in CollectionManager.Get().GetTeams())
			{
				team.SendTeamOrderChanges();
			}
			m_draggingDeckBox.OnStopDragToReorder();
		}
		m_draggingDeckBox = null;
		m_scrollbar.Pause(pause: false);
		m_scrollbar.PauseUpdateScrollHeight(pause: false);
	}

	protected override void UpdateDragToReorder()
	{
		if (m_draggingDeckBox == null)
		{
			return;
		}
		if (!InputCollection.GetMouseButton(0))
		{
			StopDragToReorder();
			return;
		}
		int startIndex = m_traySections.FindIndex((TraySection section) => section.m_deckBox == m_draggingDeckBox);
		if (startIndex < 0)
		{
			return;
		}
		TraySection draggingTraySection = m_traySections[startIndex];
		if (draggingTraySection == null)
		{
			return;
		}
		Ray cursorRay = Camera.main.ScreenPointToRay(InputCollection.GetMousePosition());
		Vector3 normal = -Camera.main.transform.forward;
		if (!new Plane(normal, m_traySectionStartPos.position).Raycast(cursorRay, out var cursorRayDistance))
		{
			return;
		}
		Vector3 point = cursorRay.GetPoint(cursorRayDistance);
		Vector3 buttonSize = TransformUtil.ComputeSetPointBounds(m_traySections[0], includeInactive: false).size;
		float itemsStartZ = m_traySectionStartPos.position.z;
		int newIndex = Mathf.FloorToInt((0f - (point.z - itemsStartZ)) / buttonSize.z);
		int numDeckBoxes = CollectionManager.Get().GetTeams().Count;
		if (numDeckBoxes < 1)
		{
			return;
		}
		float num = m_scrollbar.m_ScrollBounds.bounds.min.z - itemsStartZ;
		int minNewIndex = Mathf.FloorToInt((0f - (m_scrollbar.m_ScrollBounds.bounds.max.z - itemsStartZ)) / buttonSize.z) - 1;
		int maxNewIndex = Mathf.FloorToInt((0f - num) / buttonSize.z) + 1;
		minNewIndex = Mathf.Clamp(minNewIndex, 0, numDeckBoxes - 1);
		maxNewIndex = Mathf.Clamp(maxNewIndex, 0, numDeckBoxes - 1);
		newIndex = Mathf.Clamp(newIndex, minNewIndex, maxNewIndex);
		if (newIndex >= m_traySections.Count || newIndex == startIndex)
		{
			return;
		}
		float scrollEaseDuration = 1f;
		TraySection ensureOnScreenTraySection = m_traySections[newIndex];
		Bounds ensureOnScreenBounds = TransformUtil.ComputeSetPointBounds(ensureOnScreenTraySection.gameObject, includeInactive: false);
		if (!m_scrollbar.ScrollObjectIntoView(ensureOnScreenTraySection.gameObject, ensureOnScreenBounds.center.z - ensureOnScreenTraySection.gameObject.transform.position.z, ensureOnScreenBounds.extents.z * 1.25f, null, iTween.EaseType.linear, scrollEaseDuration, blockInputWhileScrolling: true))
		{
			m_scrollbar.StopScroll();
		}
		m_traySections.RemoveAt(startIndex);
		m_traySections.Insert(newIndex, draggingTraySection);
		for (int i = 0; i < numDeckBoxes; i++)
		{
			TraySection traySection = m_traySections[i];
			traySection.m_deckBox.SetPositionIndex(i);
			LettuceTeam team = CollectionManager.Get().GetTeam(traySection.m_deckBox.GetDeckID());
			if (team != null)
			{
				team.SortOrder = (uint)i;
			}
		}
		RefreshTraySectionPositions(animateToNewPositions: true);
	}

	private void RefreshTraySectionPositions(bool animateToNewPositions)
	{
		Vector3 nextLocalPosition = m_traySectionStartPos.localPosition;
		Vector3 prevOriginToBottomCenter = Vector3.zero;
		Transform parentTransform = m_traySectionStartPos.parent;
		for (int i = 0; i < 11; i++)
		{
			TraySection traySection = m_traySections[i];
			Bounds startingWorldBounds = TransformUtil.ComputeSetPointBounds(traySection.gameObject, includeInactive: false);
			Vector3 startingWorldPos = traySection.transform.position;
			if (i > 0)
			{
				Vector3 topCenterToOrigin = startingWorldPos - TransformUtil.ComputeWorldPoint(startingWorldBounds, TransformUtil.GetUnitAnchor(Anchor.FRONT));
				Vector3 worldIncrement = prevOriginToBottomCenter + topCenterToOrigin;
				Vector3 localIncrement = ((parentTransform != null) ? parentTransform.InverseTransformVector(worldIncrement) : worldIncrement);
				nextLocalPosition += localIncrement;
			}
			if (animateToNewPositions)
			{
				Hashtable args = iTweenManager.Get().GetTweenHashTable();
				args.Add("position", nextLocalPosition);
				args.Add("islocal", true);
				args.Add("time", 0.25f);
				args.Add("easetype", iTween.EaseType.easeOutCubic);
				iTween.MoveTo(traySection.gameObject, args);
			}
			else
			{
				traySection.gameObject.transform.localPosition = nextLocalPosition;
			}
			Material deckTrayMaterial = null;
			foreach (Material material in traySection.m_door.GetComponent<Renderer>().GetMaterials())
			{
				if (material.name.Equals("DeckTray", StringComparison.OrdinalIgnoreCase) || material.name.Equals("DeckTray (Instance)", StringComparison.OrdinalIgnoreCase))
				{
					deckTrayMaterial = material;
					break;
				}
			}
			Vector2 textureOffset = new Vector2(0f, -0.0825f * (float)i);
			traySection.GetComponent<Renderer>().GetMaterial().mainTextureOffset = textureOffset;
			if (deckTrayMaterial != null)
			{
				deckTrayMaterial.mainTextureOffset = textureOffset;
			}
			prevOriginToBottomCenter = TransformUtil.ComputeWorldPoint(startingWorldBounds, TransformUtil.GetUnitAnchor(Anchor.BACK)) - startingWorldPos;
		}
	}

	public bool UpdateDeckBoxWithNewId(long oldId, long newId)
	{
		foreach (TraySection traySection in m_traySections)
		{
			if (traySection.m_deckBox.GetDeckID() == oldId)
			{
				traySection.m_deckBox.SetDeckID(newId);
				return true;
			}
		}
		return false;
	}

	public void RefreshMissingCardIndicators()
	{
		foreach (TraySection traySection in m_traySections)
		{
			traySection.m_deckBox.UpdateInvalidCardCountIndicator();
		}
	}
}
