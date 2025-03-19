using System;
using System.Collections;
using System.Collections.Generic;
using Hearthstone.Core;
using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

[CustomEditClass]
public class DeckTrayMercListContent : DeckTrayReorderableContent
{
	public delegate void MercCountChanged(int mercCount);

	public AsyncReference m_mercLoadoutDisplay;

	[CustomEditField(Sections = "Scroll Settings")]
	public BoxCollider m_LockedScrollBounds;

	[CustomEditField(Sections = "Scroll Settings")]
	public float m_scrollHeightExtraBuffer = 20f;

	[CustomEditField(Sections = "Other Objects")]
	public GameObject m_teamCompleteHighlight;

	[CustomEditField(Sections = "Other Objects")]
	public Transform m_bigCardTopPosition;

	[CustomEditField(Sections = "Other Objects")]
	public Transform m_bigCardBottomPosition;

	[CustomEditField(Sections = "Other Objects")]
	public PlayMakerFSM m_doneButtonPlayMaker;

	[Range(-1f, 1f)]
	[Tooltip("Sensitivity of dragging towards the left direction that the merc would be removed from the list content")]
	[CustomEditField(Sections = "Interaction Settings")]
	public float m_dragRemoveDirectionSensitivity = 0.9f;

	private VisualController m_mercLoadoutVisualController;

	private LettuceTeamDataModel m_selectedTeamDataModel;

	private bool m_mercLoadoutDisplayFinishedLoading;

	private const string ADD_CARD_TO_TEAM_SOUND = "collection_manager_card_add_to_deck_instant.prefab:06df359c4026d7e47b06a4174f33e3ef";

	private const float CARD_MOVEMENT_TIME = 0.3f;

	private const string DECK_INCOMPLETE_STATE = "Deck_incomplete Idle";

	private const string DECK_INCOMPLETE_EVENT = "Deck_incomplete";

	private const string DECK_COMPLETE_STATE = "Deck_complete Idle";

	private const string DECK_COMPLETE_EVENT = "Deck_complete";

	private Vector3 m_originalLocalPosition;

	private List<MercCountChanged> m_mercCountChangedListeners = new List<MercCountChanged>();

	private bool m_animating;

	private bool m_hasFinishedEntering;

	private bool m_hasFinishedExiting = true;

	public Listable MercListable { get; private set; }

	public bool IsReorderingAllowed { get; private set; }

	public LettuceTeamDataModel SelectedTeamDataModel
	{
		get
		{
			if (m_selectedTeamDataModel == null)
			{
				if (m_mercLoadoutVisualController == null)
				{
					return null;
				}
				Widget owner = m_mercLoadoutVisualController.Owner;
				if (!owner.GetDataModel(217, out var dataModel))
				{
					dataModel = new LettuceTeamDataModel();
					owner.BindDataModel(dataModel);
					m_selectedTeamDataModel = dataModel as LettuceTeamDataModel;
				}
			}
			return m_selectedTeamDataModel;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		m_mercLoadoutDisplay.RegisterReadyListener<VisualController>(OnMercLoadoutDisplayReady);
		StartCoroutine(InitializeWhenReady());
		m_originalLocalPosition = base.transform.localPosition;
		m_hasFinishedEntering = false;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	public void OnMercLoadoutDisplayReady(VisualController visualController)
	{
		Widget vcWidget = visualController.GetComponent<Widget>();
		vcWidget.RegisterEventListener(MercLoadoutEventListener);
		m_mercLoadoutVisualController = visualController;
		MercListable = vcWidget.gameObject.GetComponentInChildren<Listable>(includeInactive: true);
		m_mercLoadoutDisplayFinishedLoading = true;
	}

	public override bool AnimateContentEntranceStart()
	{
		if (!IsContentLoaded())
		{
			return false;
		}
		ResetReoderingState();
		m_animating = true;
		m_hasFinishedEntering = false;
		Action<object> setup = delegate
		{
			UpdateTeamCompleteHighlight();
			m_animating = false;
		};
		LettuceTeam currentTeam = CollectionManager.Get().GetEditingTeam();
		if (currentTeam != null)
		{
			base.transform.localPosition = GetOffscreenLocalPosition();
			iTween.StopByName(base.gameObject, "position");
			iTween.MoveTo(base.gameObject, iTween.Hash("position", m_originalLocalPosition, "islocal", true, "time", 0.3f, "easetype", iTween.EaseType.easeOutQuad, "oncomplete", setup, "name", "position"));
			if (currentTeam.GetMercCount() > 0)
			{
				SoundManager.Get().LoadAndPlay("collection_manager_new_deck_moves_up_tray.prefab:13650cd587089e14d9a297c8de6057f1", base.gameObject);
			}
			UpdateMercList(updateHighlight: false);
		}
		else
		{
			setup(null);
		}
		return true;
	}

	public override bool PreAnimateContentEntrance()
	{
		LettuceTeam editingTeam = CollectionManager.Get().GetEditingTeam();
		CollectionUtils.PopulateMercenariesTeamDataModel(SelectedTeamDataModel, editingTeam);
		if (m_scrollbar != null)
		{
			m_scrollbar.m_HeightMode = UIBScrollable.HeightMode.UseHeightCallback;
			m_scrollbar.SetScrollHeightCallback(ScrollHeightCallback);
		}
		return true;
	}

	public override bool AnimateContentEntranceEnd()
	{
		if (m_animating)
		{
			return false;
		}
		m_hasFinishedEntering = true;
		FireMercCountChangedEvent();
		return true;
	}

	public override bool PreAnimateContentExit()
	{
		if (m_scrollbar != null)
		{
			m_scrollbar.m_HeightMode = UIBScrollable.HeightMode.UseScrollableItem;
			m_scrollbar.SetScrollHeightCallback(null);
		}
		return base.PreAnimateContentExit();
	}

	public override bool AnimateContentExitStart()
	{
		if (m_animating)
		{
			return false;
		}
		m_animating = true;
		m_hasFinishedExiting = false;
		m_teamCompleteHighlight.SetActive(value: false);
		iTween.StopByName(base.gameObject, "position");
		iTween.MoveTo(base.gameObject, iTween.Hash("position", GetOffscreenLocalPosition(), "islocal", true, "time", 0.3f, "easetype", iTween.EaseType.easeInQuad, "name", "position"));
		SoundManager.Get().LoadAndPlay("panel_slide_off_deck_creation_screen.prefab:b0d25fc984ec05d4fbea7480b611e5ad", base.gameObject);
		Processor.ScheduleCallback(0.5f, realTime: false, delegate
		{
			m_animating = false;
		});
		return true;
	}

	public override bool AnimateContentExitEnd()
	{
		m_hasFinishedExiting = true;
		return !m_animating;
	}

	public bool HasFinishedEntering()
	{
		return m_hasFinishedEntering;
	}

	public bool HasFinishedExiting()
	{
		return m_hasFinishedExiting;
	}

	public override void OnEditingTeamChanged(LettuceTeam newTeam, LettuceTeam oldTeam, bool isNewTeam)
	{
		base.OnEditingTeamChanged(newTeam, oldTeam, isNewTeam);
	}

	public override bool IsContentLoaded()
	{
		return m_mercLoadoutDisplayFinishedLoading;
	}

	public GameObject GetMercVisual(string cardID)
	{
		return null;
	}

	public override void Show(bool showAll = false)
	{
		base.Show(showAll);
	}

	public override void Hide(bool hideAll = false)
	{
		base.Hide(hideAll);
	}

	public bool AddMerc(EntityDef cardEntityDef, bool playSound, Actor animateFromActor = null, bool updateVisuals = true, int index = -1, LettuceMercenary.Loadout loadout = null)
	{
		if (!IsModeActive())
		{
			return false;
		}
		if (cardEntityDef == null)
		{
			Debug.LogError("Trying to add card EntityDef that is null.");
			return false;
		}
		string cardID = cardEntityDef.GetCardId();
		LettuceTeam team = CollectionManager.Get().GetEditingTeam();
		if (team == null)
		{
			return false;
		}
		if (team.GetMercCount() == CollectionManager.Get().GetTeamSize())
		{
			GameplayErrorManager.Get().DisplayMessage(GameStrings.Get("GLUE_LETTUCE_COLLECTION_ON_ADD_FULL_TEAM_ERROR"));
			return false;
		}
		if (team.IsMercInTeam(cardID))
		{
			return false;
		}
		if (!team.AddMerc(cardID, index, loadout))
		{
			Log.Lettuce.PrintWarning("DecktrayMercListContent.AddMerc({0}): team.AddMerc failed!", cardID);
			return false;
		}
		if (updateVisuals)
		{
			UpdateMercList(cardEntityDef, updateHighlight: true, animateFromActor);
			LettuceCollectionDisplay obj = CollectionManager.Get().GetCollectibleDisplay() as LettuceCollectionDisplay;
			obj.UpdateCurrentPageCardLocks(playSound: true);
			CollectionDeckTray.Get().GetTeamsContent().UpdateTeamTrayVisuals();
			if (obj.IsMercenaryDetailsDisplayActive())
			{
				ChangeCurrentlySelectedMercenary(CollectionManager.Get().GetMercenary(cardID).ID, selected: true);
			}
		}
		if (playSound)
		{
			SoundManager.Get().LoadAndPlay("collection_manager_place_card_in_deck.prefab:df069ffaea9dfb24b96accc95bc434a7", base.gameObject);
		}
		ResetReoderingState();
		return true;
	}

	public void RemoveMerc(int mercID, bool playSound, bool updateVisuals = true)
	{
		if (!IsModeActive())
		{
			return;
		}
		LettuceTeam team = CollectionManager.Get().GetEditingTeam();
		if (team == null)
		{
			return;
		}
		if (!team.RemoveMerc(mercID))
		{
			Log.Lettuce.PrintWarning("DeckTrayMercListContent.RemoveMerc - attempted to remove merc ({0}) that is not in team.", mercID);
			return;
		}
		if (playSound)
		{
			SoundManager.Get().LoadAndPlay("collection_manager_card_remove_from_deck_instant.prefab:bcee588ddfc73844ea3a24beb63bc53f", base.gameObject);
		}
		if (!updateVisuals)
		{
			return;
		}
		UpdateMercList();
		CollectionManager.Get().GetCollectibleDisplay().UpdateCurrentPageCardLocks(playSound: true);
		CollectionDeckTray.Get().GetTeamsContent().UpdateTeamTrayVisuals();
		LettuceCollectionDisplay lcd = CollectionManager.Get().GetCollectibleDisplay() as LettuceCollectionDisplay;
		if (!(lcd != null))
		{
			return;
		}
		LettuceCollectionPageManager lcpm = lcd.GetPageManager() as LettuceCollectionPageManager;
		if (lcpm != null)
		{
			LettuceMercenary merc = CollectionManager.Get().GetMercenary(mercID);
			if (merc != null)
			{
				lcpm.UpdatePageMercenary(MercenaryFactory.CreateMercenaryDataModelWithCoin(merc));
			}
		}
	}

	[ContextMenu("Update Card List")]
	public void UpdateMercList()
	{
		UpdateMercList(updateHighlight: true);
	}

	public void UpdateMercList(bool updateHighlight, Actor animateFromActor = null, Action onCompleteCallback = null)
	{
		UpdateMercList(string.Empty, updateHighlight, animateFromActor, onCompleteCallback);
	}

	public void UpdateMercList(EntityDef justChangedCardEntityDef, bool updateHighlight = true, Actor animateFromActor = null, Action onCompleteCallback = null)
	{
		UpdateMercList((justChangedCardEntityDef != null) ? justChangedCardEntityDef.GetCardId() : string.Empty, updateHighlight, animateFromActor, onCompleteCallback);
	}

	public void UpdateMercList(string justChangedCardID, bool updateHighlight = true, Actor animateFromActor = null, Action onCompleteCallback = null)
	{
		LettuceTeam editingTeam = CollectionManager.Get().GetEditingTeam();
		if (editingTeam != null)
		{
			CollectionUtils.PopulateMercenariesTeamDataModel(SelectedTeamDataModel, editingTeam);
			m_mercLoadoutVisualController.SetState("SHOW");
			FireMercCountChangedEvent();
			if (m_scrollbar != null)
			{
				m_scrollbar.UpdateScroll();
			}
			if (updateHighlight)
			{
				UpdateTeamCompleteHighlight();
			}
		}
	}

	public LettuceMercenaryDataModel GetMercenaryDataModel(int mercId)
	{
		foreach (LettuceMercenaryDataModel mercData in SelectedTeamDataModel.MercenaryList)
		{
			if (mercData.MercenaryId == mercId)
			{
				return mercData;
			}
		}
		return null;
	}

	public void ChangeCurrentlySelectedMercenary(int mercId, bool selected)
	{
		foreach (LettuceMercenaryDataModel mercData in SelectedTeamDataModel.MercenaryList)
		{
			if (mercData.MercenaryId == mercId)
			{
				mercData.MercenarySelected = selected;
			}
			else
			{
				mercData.MercenarySelected = false;
			}
		}
	}

	public void ChangeMercenaryArtVariation(int mercId, LettuceMercenary.ArtVariation artVariation)
	{
		foreach (LettuceMercenaryDataModel mercData in SelectedTeamDataModel.MercenaryList)
		{
			if (mercData.MercenaryId == mercId)
			{
				CollectionUtils.PopulateMercenaryCardDataModel(mercData, artVariation);
				break;
			}
		}
	}

	public void UpdateTeamCompleteHighlight()
	{
		CollectionManager cm = CollectionManager.Get();
		LettuceTeam editingTeam = cm.GetEditingTeam();
		if (editingTeam == null)
		{
			return;
		}
		bool teamFull = editingTeam.GetMercCount() == cm.GetTeamSize();
		m_teamCompleteHighlight.SetActive(teamFull);
		if (!(m_doneButtonPlayMaker != null))
		{
			return;
		}
		string activeStateName = m_doneButtonPlayMaker.ActiveStateName;
		if (!(activeStateName == "Deck_incomplete Idle"))
		{
			if (activeStateName == "Deck_complete Idle" && !teamFull)
			{
				m_doneButtonPlayMaker.SendEvent("Deck_incomplete");
			}
		}
		else if (teamFull)
		{
			m_doneButtonPlayMaker.SendEvent("Deck_complete");
		}
	}

	public override void StartDragToReorder(IDraggableCollectionVisual draggingDeckBox)
	{
		base.StartDragToReorder(draggingDeckBox);
		AllowMercReordering();
	}

	public override void StopDragToReorder()
	{
		base.StopDragToReorder();
		IsReorderingAllowed = false;
	}

	private void AllowMercReordering()
	{
		m_scrollbar.StopScroll();
		IsReorderingAllowed = true;
	}

	private void MercLoadoutEventListener(string eventName)
	{
		if (!(eventName == "MERC_LOADOUT_RELEASED"))
		{
			if (eventName == "TEAM_MERC_drag_started")
			{
				if (IsReorderingAllowed)
				{
					OnMercDragStarted();
				}
				else if (Vector3.Dot(Vector3.left, PegUI.Get().GetDragDelta().normalized) > m_dragRemoveDirectionSensitivity)
				{
					AllowMercReordering();
					OnMercDragStarted();
				}
			}
			else
			{
				LettuceCollectionDisplay lcd = CollectionManager.Get().GetCollectibleDisplay() as LettuceCollectionDisplay;
				if (lcd == null)
				{
					Log.Lettuce.PrintWarning("DeckTrayMercListContent.MercLoadoutEventListener - LettuceCollectionDisplay is null!");
				}
				else
				{
					lcd.HandleTileHoverEvents(eventName, m_mercLoadoutVisualController);
				}
			}
		}
		else
		{
			OnMercReleased();
		}
	}

	private void HideHoverCards()
	{
		LettuceCollectionDisplay lcd = CollectionManager.Get().GetCollectibleDisplay() as LettuceCollectionDisplay;
		if (lcd == null)
		{
			Log.Lettuce.PrintWarning("DeckTrayMercListContent.HideHoverCards - LettuceCollectionDisplay is null!");
		}
		else
		{
			lcd.HideHoverCards();
		}
	}

	private void OnMercReleased()
	{
		ResetReoderingState();
		LettuceCollectionDisplay lcd = CollectionManager.Get().GetCollectibleDisplay() as LettuceCollectionDisplay;
		if (lcd == null)
		{
			Log.Lettuce.PrintWarning("DeckTrayMercListContent.OnMercReleased - LettuceCollectionDisplay is null!");
			return;
		}
		lcd.HideHoverCards();
		LettuceMercenaryDataModel mercData = WidgetUtils.GetEventDataModel(m_mercLoadoutVisualController).Payload as LettuceMercenaryDataModel;
		LettuceMercenary merc = CollectionManager.Get().GetMercenary(mercData.MercenaryId);
		if (mercData.IsDisabled)
		{
			RemoveMerc(mercData.MercenaryId, playSound: true);
		}
		else if (lcd.IsMercenaryDetailsDisplayActive() && lcd.GetMercenaryDetailsDisplay().GetCurrentlyDisplayedMercenary().ID == merc.ID)
		{
			lcd.GetMercenaryDetailsDisplay().HideHelpPopups();
			Navigation.GoBack();
		}
		else
		{
			lcd.ShowMercenaryDetailsDisplay(merc);
		}
	}

	private void OnMercDragStarted()
	{
		HideHoverCards();
		LettuceMercenaryDataModel mercData = WidgetUtils.GetEventDataModel(m_mercLoadoutVisualController).Payload as LettuceMercenaryDataModel;
		if (CanPickupCard() && CollectionInputMgr.Get().GrabMercenariesModeCard(mercData, CollectionUtils.MercenariesModeCardType.Mercenary))
		{
			m_draggingDeckBox = null;
			RemoveMerc(mercData.MercenaryId, playSound: true);
		}
	}

	private IEnumerator InitializeWhenReady()
	{
		while (!IsContentLoaded())
		{
			yield return null;
		}
	}

	private bool CanPickupCard()
	{
		if (ShouldIgnoreAllInput())
		{
			return false;
		}
		if (CollectionDeckTray.Get().GetCurrentContentType() != DeckTray.DeckContentTypes.Mercs)
		{
			return false;
		}
		if (!CollectionDeckTray.Get().CanPickupCard())
		{
			return false;
		}
		return true;
	}

	private bool ShouldIgnoreAllInput()
	{
		if (CollectionInputMgr.Get() != null && CollectionInputMgr.Get().IsDraggingScrollbar())
		{
			return true;
		}
		if (CraftingManager.Get() != null && CraftingManager.Get().IsCardShowing())
		{
			return true;
		}
		if (CollectionManager.Get().GetCollectibleDisplay().GetPageManager()
			.ArePagesTurning())
		{
			return true;
		}
		return false;
	}

	private float ScrollHeightCallback()
	{
		return 0f;
	}

	private Vector3 GetOffscreenLocalPosition()
	{
		Vector3 newLocalPos = m_originalLocalPosition;
		CollectionManager.Get().GetEditingTeam()?.GetMercCount();
		newLocalPos.z -= 100f;
		return newLocalPos;
	}

	private void ResetReoderingState()
	{
		IsReorderingAllowed = false;
		m_draggingDeckBox = null;
	}

	public void RegisterMercCountUpdated(MercCountChanged dlg)
	{
		m_mercCountChangedListeners.Add(dlg);
	}

	public void UnregisterMercCountUpdated(MercCountChanged dlg)
	{
		m_mercCountChangedListeners.Remove(dlg);
	}

	private void FireMercCountChangedEvent()
	{
		MercCountChanged[] array = m_mercCountChangedListeners.ToArray();
		LettuceTeam currentTeam = CollectionManager.Get().GetEditingTeam();
		int currentMercCount = 0;
		if (currentTeam != null)
		{
			currentMercCount = currentTeam.GetMercCount();
		}
		MercCountChanged[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i](currentMercCount);
		}
	}
}
