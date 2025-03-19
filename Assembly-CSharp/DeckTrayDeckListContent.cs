using System;
using System.Collections;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.MaterialService.Extensions;
using Hearthstone;
using Hearthstone.Core;
using Hearthstone.Util;
using PegasusShared;
using UnityEngine;

[CustomEditClass]
public class DeckTrayDeckListContent : DeckTrayReorderableContent
{
	public delegate void BusyWithDeck(bool busy);

	public delegate void DeckCountChanged(int deckCount);

	[CustomEditField(Sections = "Deck Tray Settings")]
	public Transform m_deckEditTopPos;

	[CustomEditField(Sections = "Deck Tray Settings")]
	public Transform m_traySectionStartPos;

	[CustomEditField(Sections = "Deck Tray Settings")]
	public GameObject m_deckInfoTooltipBone;

	[CustomEditField(Sections = "Deck Tray Settings")]
	public GameObject m_deckOptionsBone;

	[CustomEditField(Sections = "Prefabs")]
	public TraySection m_traySectionPrefab;

	[CustomEditField(Sections = "Prefabs")]
	public DeckTray m_deckTray;

	[CustomEditField(Sections = "Prefabs", T = EditType.GAME_OBJECT)]
	public string m_deckInfoActorPrefab;

	[CustomEditField(Sections = "Prefabs", T = EditType.GAME_OBJECT)]
	public string m_deckOptionsPrefab;

	[CustomEditField(Sections = "Deck Button Settings")]
	public ParticleSystem m_deleteDeckPoof;

	[CustomEditField(Sections = "Deck Button Settings")]
	public Vector3 m_deleteDeckPoofVisualOffset;

	[SerializeField]
	private Vector3 m_deckButtonOffset;

	[CustomEditField(Sections = "Deck Button Settings")]
	public GameObject m_newDeckButtonContainer;

	[CustomEditField(Sections = "Deck Button Settings")]
	public CollectionDeckTrayButton m_newDeckButton;

	protected const float TIME_BETWEEN_TRAY_DOOR_ANIMS = 0.015f;

	protected const int MAX_NUM_DECKBOXES_AVAILABLE = 27;

	protected const int NUM_DECKBOXES_TO_DISPLAY = 29;

	protected CollectionDeckInfo m_deckInfoTooltip;

	protected List<TraySection> m_traySections = new List<TraySection>();

	protected TraySection m_editingTraySection;

	protected int m_centeringDeckList = -1;

	protected DeckOptionsMenu m_deckOptionsMenu;

	protected bool m_initialized;

	protected bool m_animatingExit;

	protected bool m_doneEntering;

	private bool m_wasTouchModeEnabled;

	protected string m_previousDeckName;

	protected const float DELETE_DECK_ANIM_TIME = 0.5f;

	protected bool m_initializedDeckHeroes;

	protected bool m_deletingDecks;

	protected bool m_waitingToDeleteDeck;

	protected List<CollectionDeck> m_decksToDelete = new List<CollectionDeck>();

	protected TraySection m_newlyCreatedTraySection;

	private List<DeckCountChanged> m_deckCountChangedListeners = new List<DeckCountChanged>();

	private List<BusyWithDeck> m_busyWithDeckListeners = new List<BusyWithDeck>();

	private static FormatType s_PreHeroPickerFormat = FormatType.FT_STANDARD;

	public static FormatType s_HeroPickerFormat = FormatType.FT_STANDARD;

	private ScreenEffectsHandle m_screenEffectsHandle;

	private const float TRAY_MATERIAL_Y_OFFSET = -0.0825f;

	[CustomEditField(Sections = "Deck Button Settings")]
	public Vector3 DeckButtonOffset
	{
		get
		{
			return m_deckButtonOffset;
		}
		set
		{
			m_deckButtonOffset = value;
			UpdateNewDeckButton();
		}
	}

	public bool IsShowingDeckOptions
	{
		get
		{
			if (m_deckOptionsMenu != null)
			{
				return m_deckOptionsMenu.IsShown;
			}
			return false;
		}
	}

	public bool CanDragToReorderDecks
	{
		get
		{
			NetCache.NetCacheFeatures features = NetCache.Get()?.GetNetObject<NetCache.NetCacheFeatures>();
			if (features != null && !features.Collection.DeckReordering)
			{
				return false;
			}
			if (!CollectionManagerDisplay.IsSpecialOneDeckMode())
			{
				return !m_animatingExit;
			}
			return false;
		}
	}

	protected void Update()
	{
		UpdateDragToReorder();
		if (m_wasTouchModeEnabled != UniversalInputManager.Get().IsTouchMode())
		{
			m_wasTouchModeEnabled = UniversalInputManager.Get().IsTouchMode();
			if (UniversalInputManager.Get().IsTouchMode() && m_deckInfoTooltip != null)
			{
				HideDeckInfo();
			}
		}
	}

	protected override void Awake()
	{
		base.Awake();
		CollectionManager collectionManager = CollectionManager.Get();
		collectionManager.RegisterFavoriteHeroChangedListener(OnFavoriteHeroChanged);
		collectionManager.RegisterOnUIHeroOverrideCardRemovedListener(OnUIHeroOverrideCardRemoved);
		HearthstoneApplication hearthstoneApplication = HearthstoneApplication.Get();
		if (hearthstoneApplication != null)
		{
			hearthstoneApplication.WillReset += WillReset;
		}
		m_screenEffectsHandle = new ScreenEffectsHandle(this);
	}

	protected override void OnDestroy()
	{
		CollectionManager collectionManager = CollectionManager.Get();
		collectionManager.RemoveFavoriteHeroChangedListener(OnFavoriteHeroChanged);
		collectionManager.RemoveDeckDeletedListener(OnDeckDeleted);
		collectionManager.RemoveOnUIHeroOverrideCardRemovedListener(OnUIHeroOverrideCardRemoved);
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

	private void WillReset()
	{
		Processor.CancelScheduledCallback(BeginAnimation);
		Processor.CancelScheduledCallback(EndAnimation);
	}

	private void OnNewDeckButtonPress()
	{
		if (IsModeActive() && !base.IsTouchDragging)
		{
			SoundManager.Get().LoadAndPlay("Hub_Click.prefab:cc2cf2b5507827149b13d12210c0f323");
			StartCreateNewDeck();
		}
	}

	protected void StartCreateNewDeck()
	{
		PresenceMgr.Get().SetStatus(Global.PresenceStatus.DECKEDITOR);
		ShowNewDeckButton(newDeckButtonActive: false);
		CollectionManagerDisplay cmd = CollectionManager.Get().GetCollectibleDisplay() as CollectionManagerDisplay;
		if (!(cmd != null))
		{
			return;
		}
		FormatType ft = cmd.CurrentSetFilterFormatType;
		if (ft == FormatType.FT_UNKNOWN)
		{
			RankMgr.LogMessage("Options.GetFormatType() = FT_UNKOWN", "StartCreateNewDeck", "D:\\p4Workspace\\32.0.0\\Pegasus\\Client\\Assets\\Shared\\Scripts\\Game\\DeckTrayDeckListContent.cs", 201);
			return;
		}
		s_PreHeroPickerFormat = ft;
		if (Options.GetInRankedPlayMode())
		{
			s_HeroPickerFormat = ft;
		}
		else
		{
			s_HeroPickerFormat = Options.Get().GetEnum<FormatType>(Option.FORMAT_TYPE_LAST_PLAYED);
		}
		cmd.EnterSelectNewDeckHeroMode();
	}

	protected void EndCreateNewDeck(bool newDeck)
	{
		Options.SetFormatType(s_PreHeroPickerFormat);
		CollectionManagerDisplay cmd = CollectionManager.Get().GetCollectibleDisplay() as CollectionManagerDisplay;
		if (cmd != null)
		{
			cmd.ExitSelectNewDeckHeroMode();
		}
		ShowNewDeckButton(newDeckButtonActive: true, delegate
		{
			if (newDeck)
			{
				UpdateAllTrays(immediate: true, updateVisuals: true);
			}
		});
	}

	private void DeleteQueuedDecks(bool force = false)
	{
		if (m_decksToDelete.Count == 0 || (!IsModeActive() && !force))
		{
			return;
		}
		foreach (CollectionDeck deck in m_decksToDelete)
		{
			Network.Get().DeleteDeck(deck.ID, deck.Type);
			CollectionManager.Get().AddPendingDeckDelete(deck.ID);
			if (!Network.IsLoggedIn() || deck.ID <= 0)
			{
				CollectionManager.Get().OnDeckDeletedWhileOffline(deck.ID);
			}
		}
		m_decksToDelete.Clear();
	}

	private void OnDeckDeleted(CollectionDeck removedDeck)
	{
		if (removedDeck != null)
		{
			m_waitingToDeleteDeck = false;
			long deckID = removedDeck.ID;
			StartCoroutine(DeleteDeckAnimation(deckID));
		}
	}

	private void OnFavoriteHeroChanged(TAG_CLASS heroClass, NetCache.CardDefinition favoriteHero, bool isFavorite, object userData)
	{
		UpdateDeckTrayVisuals(rerollAllHeroes: false, heroClass);
	}

	private void OnUIHeroOverrideCardRemoved()
	{
		UpdateDeckTrayVisuals();
	}

	private IEnumerator DeleteDeckAnimation(long deckID, Action callback = null)
	{
		while (m_deletingDecks)
		{
			yield return null;
		}
		int delIndex = 0;
		TraySection delTraySection = null;
		TraySection newDeckButtonTrayLocation = m_traySections[0];
		for (int i = 0; i < m_traySections.Count; i++)
		{
			TraySection traySection = m_traySections[i];
			long existingDeckID = traySection.m_deckBox.GetDeckID();
			if (existingDeckID == deckID)
			{
				delIndex = i;
				delTraySection = traySection;
			}
			else if (existingDeckID == -1)
			{
				break;
			}
			newDeckButtonTrayLocation = traySection;
		}
		if (delTraySection == null)
		{
			Debug.LogWarning("Unable to delete deck with ID {0}. Not found in tray sections.", base.gameObject);
			yield break;
		}
		FireBusyWithDeckEvent(busy: true);
		m_deletingDecks = true;
		FireDeckCountChangedEvent();
		m_traySections.RemoveAt(delIndex);
		GetIdealNewDeckButtonLocalPosition(newDeckButtonTrayLocation, out var newDeckBtnPos, out var newDeckBtnActive);
		Vector3 prevTraySectionPosition = delTraySection.transform.localPosition;
		if (HeroPickerDisplay.Get() == null || !HeroPickerDisplay.Get().IsShown())
		{
			SoundManager.Get().LoadAndPlay("collection_manager_delete_deck.prefab:5ca16bec63041b741a4fb33706ed9cb1", base.gameObject);
			m_deleteDeckPoof.transform.position = delTraySection.m_deckBox.transform.position + m_deleteDeckPoofVisualOffset;
			m_deleteDeckPoof.Play(withChildren: true);
		}
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
			if (j <= 25)
			{
				delTraySectionPosition = traySectionPos;
			}
			prevTraySectionPosition = traySectionPos;
		}
		if (delIndex == 26)
		{
			delTraySectionPosition = delTraySection.transform.localPosition;
		}
		m_traySections.Insert(26, delTraySection);
		m_newDeckButton.SetIsUsable(CanShowNewDeckButton());
		delTraySection.gameObject.SetActive(value: true);
		delTraySection.HideDeckBox(immediate: true);
		delTraySection.transform.localPosition = delTraySectionPosition;
		if (m_newDeckButton.gameObject.activeSelf)
		{
			iTween.MoveTo(m_newDeckButtonContainer, iTween.Hash("position", newDeckBtnPos, "islocal", true, "time", 0.5f, "easetype", iTween.EaseType.easeOutBounce, "oncomplete", onAnimationsComplete, "oncompleteparams", m_newDeckButtonContainer, "name", "position"));
			animatingTraySections.Add(m_newDeckButtonContainer);
		}
		else
		{
			m_newDeckButtonContainer.transform.localPosition = newDeckBtnPos;
		}
		while (animatingTraySections.Count > 0)
		{
			animatingTraySections.RemoveAll((GameObject obj) => obj == null || !obj.activeInHierarchy);
			yield return null;
		}
		if (!CollectionManager.Get().IsInEditMode())
		{
			ShowNewDeckButton(newDeckBtnActive);
		}
		FireBusyWithDeckEvent(busy: false);
		callback?.Invoke();
		m_deletingDecks = false;
	}

	private void UpdateNewDeckButton(TraySection setNewDeckButtonPosition = null)
	{
		bool updated = UpdateNewDeckButtonPosition(setNewDeckButtonPosition);
		ShowNewDeckButton(updated && CanShowNewDeckButton());
	}

	private bool UpdateNewDeckButtonPosition(TraySection setNewDeckButtonPosition = null)
	{
		bool newDeckButtonActive = false;
		GetIdealNewDeckButtonLocalPosition(setNewDeckButtonPosition, out var newPos, out newDeckButtonActive);
		m_newDeckButtonContainer.transform.localPosition = newPos;
		return newDeckButtonActive;
	}

	private void GetIdealNewDeckButtonLocalPosition(TraySection setNewDeckButtonPosition, out Vector3 outPosition, out bool outActive)
	{
		TraySection lastUnusedTray = GetLastUnusedTraySection();
		TraySection newDeckButtonPosition = ((setNewDeckButtonPosition == null) ? lastUnusedTray : setNewDeckButtonPosition);
		outActive = lastUnusedTray != null;
		outPosition = ((newDeckButtonPosition != null) ? newDeckButtonPosition.transform.localPosition : m_traySectionStartPos.localPosition) + m_deckButtonOffset;
	}

	public void ShowNewDeckButton(bool newDeckButtonActive, CollectionDeckTrayButton.DelOnAnimationFinished callback = null)
	{
		ShowNewDeckButton(newDeckButtonActive, null, callback);
	}

	public void ShowNewDeckButton(bool newDeckButtonActive, float? speed, CollectionDeckTrayButton.DelOnAnimationFinished callback = null)
	{
		if (m_newDeckButton.IsPoppedUp() != newDeckButtonActive)
		{
			if (newDeckButtonActive)
			{
				m_newDeckButton.gameObject.SetActive(value: true);
				m_newDeckButton.PlayPopUpAnimation(delegate
				{
					if (callback != null)
					{
						callback(this);
					}
				}, null, speed);
				return;
			}
			m_newDeckButton.PlayPopDownAnimation(delegate
			{
				m_newDeckButton.gameObject.SetActive(value: false);
				if (callback != null)
				{
					callback(this);
				}
			}, null, speed);
		}
		else if (callback != null)
		{
			callback(this);
		}
	}

	public override bool AnimateContentEntranceStart()
	{
		Initialize();
		long editDeckID = -1L;
		if (m_editingTraySection != null)
		{
			editDeckID = m_editingTraySection.m_deckBox.GetDeckID();
		}
		UpdateDeckTrayVisuals(!m_initializedDeckHeroes);
		m_initializedDeckHeroes = true;
		SwapEditTrayIfNeeded(editDeckID);
		bool immediate = CollectionManagerDisplay.IsSpecialOneDeckMode();
		UpdateAllTrays(immediate, updateVisuals: false);
		if (m_editingTraySection != null)
		{
			FinishRenamingEditingDeck(null, shouldValidateDeckName: false);
			m_editingTraySection.MoveDeckBoxBackToOriginalPosition(0.25f, delegate
			{
				m_editingTraySection = null;
			});
		}
		m_newDeckButton.SetIsUsable(CanShowNewDeckButton());
		FireBusyWithDeckEvent(busy: true);
		FireDeckCountChangedEvent();
		CollectionManager.Get().DoneEditing();
		return true;
	}

	public override bool AnimateContentEntranceEnd()
	{
		if (m_editingTraySection != null)
		{
			return false;
		}
		m_newDeckButton.SetEnabled(enabled: true);
		FireBusyWithDeckEvent(busy: false);
		DeleteQueuedDecks(force: true);
		return true;
	}

	public override bool AnimateContentExitStart()
	{
		m_animatingExit = true;
		FireBusyWithDeckEvent(busy: true);
		float? btnSpeed = null;
		if (SceneMgr.Get().IsInTavernBrawlMode())
		{
			btnSpeed = 500f;
		}
		ShowNewDeckButton(newDeckButtonActive: false, btnSpeed);
		Processor.ScheduleCallback(0.5f, realTime: false, BeginAnimation);
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
			UpdateNewDeckButtonPosition(animateTraySection);
			ShowNewDeckButton(newDeckButtonActive: true, delegate
			{
				animateTraySection.ShowDeckBox(immediate: true, delegate
				{
					animateTraySection.m_deckBox.gameObject.SetActive(value: false);
					m_newDeckButton.FlipHalfOverAndHide(0.1f, delegate
					{
						animateTraySection.FlipDeckBoxHalfOverToShow(0.1f, delegate
						{
							animateTraySection.MoveDeckBoxToEditPosition(m_deckEditTopPos.position, 0.25f);
						});
					});
				});
			});
			m_editingTraySection = m_newlyCreatedTraySection;
			m_newlyCreatedTraySection = null;
			animationWaitTime += 0.7f;
		}
		else if (m_editingTraySection != null)
		{
			m_editingTraySection.MoveDeckBoxToEditPosition(m_deckEditTopPos.position, 0.25f);
		}
		Processor.ScheduleCallback(animationWaitTime, realTime: false, EndAnimation);
	}

	private void EndAnimation(object userData)
	{
		m_animatingExit = false;
		FireBusyWithDeckEvent(busy: false);
	}

	private CollectionDeck UpdateRenamingEditingDeck(string newDeckName)
	{
		return UpdateRenamingEditingDeck_Internal(newDeckName, shouldValidateDeckName: false);
	}

	private CollectionDeck FinishUpdateRenamingEditingDeck(string newDeckName, bool shouldValidateDeckName)
	{
		return UpdateRenamingEditingDeck_Internal(newDeckName, shouldValidateDeckName);
	}

	private CollectionDeck UpdateRenamingEditingDeck_Internal(string newDeckName, bool shouldValidateDeckName)
	{
		CollectionDeck editDeck = m_deckTray.GetCardsContent().GetEditingDeck();
		if (editDeck != null && !string.IsNullOrEmpty(newDeckName))
		{
			editDeck.Name = newDeckName;
			if (shouldValidateDeckName && RegionUtils.IsCNLegalRegion)
			{
				editDeck.SendDeckRenameChange(null, shouldValidateDeckName: true, DeckType.NORMAL_DECK, DeckSourceType.DECK_SOURCE_TYPE_NORMAL);
			}
		}
		return editDeck;
	}

	private void OnUnfocusedDuringRename()
	{
		CollectionDeck editingDeck = m_deckTray.GetCardsContent().GetEditingDeck();
		string deckName = m_previousDeckName;
		if (editingDeck != null)
		{
			deckName = editingDeck.Name;
		}
		FinishRenamingEditingDeck(deckName, shouldValidateDeckName: true);
	}

	private void FinishRenamingEditingDeck(string newDeckName, bool shouldValidateDeckName)
	{
		if (!(m_editingTraySection == null))
		{
			if (UniversalInputManager.Get() != null && UniversalInputManager.Get().IsTextInputActive())
			{
				UniversalInputManager.Get().CancelTextInput(base.gameObject);
			}
			string sanitizedDeckName = TextUtils.StripHTMLTags(newDeckName);
			CollectionDeckBoxVisual editDeckBox = m_editingTraySection.m_deckBox;
			CollectionDeck editDeck = FinishUpdateRenamingEditingDeck(sanitizedDeckName, shouldValidateDeckName);
			if (editDeck != null && m_editingTraySection != null)
			{
				editDeckBox.SetDeckName(editDeck.Name);
			}
			editDeckBox.ShowDeckName();
		}
	}

	public void CreateNewDeckFromUserSelection(TAG_CLASS heroClass, string heroCardID, string customDeckName = null, DeckSourceType deckSourceType = DeckSourceType.DECK_SOURCE_TYPE_NORMAL, string pastedDeckHashString = null)
	{
		bool num = SceneMgr.Get().IsInTavernBrawlMode();
		DeckType deckType = DeckType.NORMAL_DECK;
		string deckName = customDeckName;
		if (num)
		{
			deckName = GameStrings.Get("GLUE_COLLECTION_TAVERN_BRAWL_DECKNAME");
			deckType = DeckType.TAVERN_BRAWL_DECK;
		}
		else if (string.IsNullOrEmpty(deckName))
		{
			deckName = CollectionManager.Get().AutoGenerateDeckName(heroClass);
		}
		CollectionManager.Get().SendCreateDeck(deckType, deckName, heroCardID, deckSourceType, pastedDeckHashString);
		EndCreateNewDeck(newDeck: true);
	}

	public void CreateNewDeckCancelled()
	{
		EndCreateNewDeck(newDeck: false);
	}

	public bool IsWaitingToDeleteDeck()
	{
		return m_waitingToDeleteDeck;
	}

	public int NumDecksToDelete()
	{
		return m_decksToDelete.Count;
	}

	public bool IsDeletingDecks()
	{
		return m_deletingDecks;
	}

	public void DeleteDeck(long deckID)
	{
		CollectionDeck deck = CollectionManager.Get().GetDeck(deckID);
		if (deck == null)
		{
			Log.All.PrintError("Unable to delete deck id={0} - not found in cache.", deckID);
			return;
		}
		if (Network.IsLoggedIn() && deckID <= 0)
		{
			Log.Offline.PrintDebug("DeleteDeck() - Attempting to delete fake deck while online.");
		}
		deck.MarkBeingDeleted();
		m_decksToDelete.Add(deck);
		DeleteQueuedDecks();
	}

	public void DeleteEditingDeck()
	{
		CollectionDeck deck = CollectionManager.Get().GetEditedDeck();
		if (deck == null)
		{
			Debug.LogWarning("No deck currently being edited!");
			return;
		}
		m_waitingToDeleteDeck = true;
		DeleteDeck(deck.ID);
	}

	public void CancelRenameEditingDeck()
	{
		FinishRenamingEditingDeck(null, shouldValidateDeckName: false);
	}

	public Vector3 GetNewDeckButtonPosition()
	{
		return m_newDeckButton.transform.localPosition;
	}

	public void UpdateDeckName(string deckName, bool shouldValidateDeckName)
	{
		if (deckName == null)
		{
			CollectionDeck editingDeck = m_deckTray.GetCardsContent().GetEditingDeck();
			if (editingDeck == null)
			{
				return;
			}
			deckName = editingDeck.Name;
		}
		FinishRenamingEditingDeck(deckName, shouldValidateDeckName);
	}

	public void RenameCurrentlyEditingDeck()
	{
		if (m_editingTraySection == null)
		{
			Debug.LogWarning("Unable to rename deck. No deck currently being edited.", base.gameObject);
		}
		else if (!CollectionManagerDisplay.IsSpecialOneDeckMode())
		{
			CollectionDeckBoxVisual editDeckBox = m_editingTraySection.m_deckBox;
			editDeckBox.HideDeckName();
			Camera camera = Box.Get().GetCamera();
			Bounds textBounds = editDeckBox.GetDeckNameText().GetBounds();
			Rect rect = CameraUtils.CreateGUIViewportRect(camera, textBounds.min, textBounds.max);
			Font currentFont = editDeckBox.GetDeckNameText().GetLocalizedFont();
			m_previousDeckName = editDeckBox.GetDeckNameText().Text;
			UniversalInputManager.TextInputParams inputParams = new UniversalInputManager.TextInputParams
			{
				m_owner = base.gameObject,
				m_rect = rect,
				m_updatedCallback = delegate(string newName)
				{
					UpdateRenamingEditingDeck(newName);
				},
				m_completedCallback = delegate(string newName)
				{
					FinishRenamingEditingDeck(newName, shouldValidateDeckName: true);
				},
				m_canceledCallback = delegate
				{
					FinishRenamingEditingDeck(m_previousDeckName, shouldValidateDeckName: true);
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
		if (m_centeringDeckList != -1 && m_editingTraySection != null)
		{
			BoxCollider collider = m_editingTraySection.m_deckBox.GetComponent<BoxCollider>();
			if (m_scrollbar.ScrollObjectIntoView(m_editingTraySection.m_deckBox.gameObject, collider.center.y, collider.size.y / 2f, delegate
			{
				m_animatingExit = false;
			}, iTween.EaseType.linear, m_scrollbar.m_ScrollTweenTime, blockInputWhileScrolling: true))
			{
				m_animatingExit = true;
				m_centeringDeckList = -1;
			}
		}
		StartCoroutine(ShowTrayDoors(show: false));
		return !m_animatingExit;
	}

	public override bool PreAnimateContentEntrance()
	{
		m_doneEntering = false;
		StartCoroutine(ShowTrayDoors(show: true));
		return true;
	}

	public override void OnEditedDeckChanged(CollectionDeck newDeck, CollectionDeck oldDeck, bool isNewDeck)
	{
		if (newDeck != null && m_deckInfoTooltip != null)
		{
			m_deckInfoTooltip.SetDeck(newDeck);
			if (m_deckOptionsMenu != null)
			{
				m_deckOptionsMenu.SetDeck(newDeck);
			}
		}
		if (IsModeActive())
		{
			UpdateDeckTrayVisuals();
		}
		if (isNewDeck && newDeck != null)
		{
			m_newlyCreatedTraySection = GetExistingTrayFromDeck(newDeck);
			if (m_newlyCreatedTraySection != null)
			{
				m_centeringDeckList = m_newlyCreatedTraySection.m_deckBox.GetPositionIndex();
			}
		}
	}

	public void UpdateEditingDeckBoxVisual(string heroCardId, TAG_PREMIUM? premiumOverride = null)
	{
		if (!(m_editingTraySection == null))
		{
			m_editingTraySection.m_deckBox.SetHeroCardPremiumOverride(premiumOverride);
			if (heroCardId != string.Empty)
			{
				m_editingTraySection.m_deckBox.SetHeroCardID(heroCardId, null);
			}
			else
			{
				m_editingTraySection.m_deckBox.SetHeroCardIdFromDeck();
			}
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

	public void RegisterDeckCountUpdated(DeckCountChanged dlg)
	{
		m_deckCountChangedListeners.Add(dlg);
	}

	public void UnregisterDeckCountUpdated(DeckCountChanged dlg)
	{
		m_deckCountChangedListeners.Remove(dlg);
	}

	public void RegisterBusyWithDeck(BusyWithDeck dlg)
	{
		m_busyWithDeckListeners.Add(dlg);
	}

	public void UnregisterBusyWithDeck(BusyWithDeck dlg)
	{
		m_busyWithDeckListeners.Remove(dlg);
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
		UIBScrollableItem scrollableItem = m_newDeckButtonContainer.GetComponent<UIBScrollableItem>();
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
			m_newDeckButton.gameObject.SetActive(value: false);
		}
	}

	protected void Initialize()
	{
		if (m_initialized)
		{
			return;
		}
		m_newDeckButton.AddEventListener(UIEventType.RELEASE, delegate
		{
			OnNewDeckButtonPress();
		});
		CollectionManager.Get().RegisterDeckDeletedListener(OnDeckDeleted);
		GameObject go = AssetLoader.Get().InstantiatePrefab(m_deckInfoActorPrefab, AssetLoadingOptions.IgnorePrefabPosition);
		if (go == null)
		{
			Debug.LogError($"Unable to load actor {m_deckInfoActorPrefab}: null", base.gameObject);
			return;
		}
		m_deckInfoTooltip = go.GetComponent<CollectionDeckInfo>();
		if (m_deckInfoTooltip == null)
		{
			Debug.LogError($"Actor {m_deckInfoActorPrefab} does not contain CollectionDeckInfo component.", base.gameObject);
			return;
		}
		GameUtils.SetParent(m_deckInfoTooltip, m_deckInfoTooltipBone);
		m_deckInfoTooltip.RegisterHideListener(HideDeckInfoListener);
		go = AssetLoader.Get().InstantiatePrefab(m_deckOptionsPrefab);
		m_deckOptionsMenu = go.GetComponent<DeckOptionsMenu>();
		GameUtils.SetParent(m_deckOptionsMenu.gameObject, m_deckOptionsBone);
		m_deckOptionsMenu.SetDeckInfo(m_deckInfoTooltip);
		HideDeckInfo();
		CreateTraySections();
		m_initialized = true;
	}

	protected void HideDeckInfoListener()
	{
		if (m_editingTraySection != null)
		{
			LayerUtils.SetLayer(m_editingTraySection.m_deckBox.gameObject, GameLayer.Default);
			LayerUtils.SetLayer(m_deckOptionsMenu.gameObject, GameLayer.Default);
			m_editingTraySection.m_deckBox.HideRenameVisuals();
		}
		m_screenEffectsHandle.StopEffect();
		if (UniversalInputManager.Get().IsTouchMode())
		{
			if (m_editingTraySection != null)
			{
				m_editingTraySection.m_deckBox.SetHighlightState(ActorStateType.NONE);
				m_editingTraySection.m_deckBox.ShowDeckName();
			}
			FinishRenamingEditingDeck(null, shouldValidateDeckName: false);
		}
		m_deckOptionsMenu.Hide();
		if (m_editingTraySection != null)
		{
			m_editingTraySection.m_deckBox.UpdateColliderHeightForDeathKnight();
		}
	}

	protected virtual void ShowDeckInfo()
	{
		if (!UniversalInputManager.Get().IsTouchMode() && m_editingTraySection != null)
		{
			m_editingTraySection.m_deckBox.ShowRenameVisuals();
		}
		LayerUtils.SetLayer(m_editingTraySection.m_deckBox.gameObject, GameLayer.IgnoreFullScreenEffects);
		LayerUtils.SetLayer(m_deckInfoTooltip.gameObject, GameLayer.IgnoreFullScreenEffects);
		LayerUtils.SetLayer(m_deckOptionsMenu.gameObject, GameLayer.IgnoreFullScreenEffects);
		ScreenEffectParameters screenEffectParameters = ScreenEffectParameters.DesaturatePerspective;
		screenEffectParameters.Time = 0.25f;
		m_screenEffectsHandle.StartEffect(screenEffectParameters);
		m_deckInfoTooltip.UpdateManaCurve();
		if (CollectionManagerDisplay.ShouldShowDeckHeaderInfo())
		{
			m_deckInfoTooltip.Show();
		}
		if (CollectionManagerDisplay.ShouldShowDeckOptionsMenu())
		{
			m_deckOptionsMenu.Show();
		}
	}

	protected void HideDeckInfo()
	{
		m_deckInfoTooltip.Hide();
	}

	protected void CreateTraySections()
	{
		Vector3 trayScale = m_traySectionStartPos.localScale;
		Vector3 trayRotation = m_traySectionStartPos.localEulerAngles;
		for (int i = 0; i < 29; i++)
		{
			TraySection traySection = (TraySection)GameUtils.Instantiate(m_traySectionPrefab, base.gameObject);
			traySection.m_deckBox.SetPositionIndex(i);
			traySection.transform.localScale = trayScale;
			traySection.transform.localEulerAngles = trayRotation;
			traySection.EnableDoors(i < 27);
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
			deckBox.SetIsLocked(ShouldDeckBoxesBeLocked());
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

	protected TraySection GetExistingTrayFromDeck(CollectionDeck deck)
	{
		return GetExistingTrayFromDeck(deck.ID);
	}

	private TraySection GetExistingTrayFromDeck(long deckID)
	{
		foreach (TraySection traySection in m_traySections)
		{
			if (traySection.m_deckBox.GetDeckID() == deckID)
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

	protected void InitializeTraysFromDecks()
	{
		UpdateDeckTrayVisuals(rerollAllHeroes: true);
	}

	protected void UpdateAllTrays(bool immediate, bool updateVisuals)
	{
		if (updateVisuals)
		{
			UpdateDeckTrayVisuals();
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
		UpdateNewDeckButton();
		m_doneEntering = true;
	}

	public TraySection GetLastUnusedTraySection()
	{
		int index = 0;
		foreach (TraySection traySection in m_traySections)
		{
			if (index >= 27)
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
			if (index >= 27)
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

	public bool CanShowNewDeckButton()
	{
		if (CollectionManager.Get().GetDecks(DeckType.NORMAL_DECK).Count < 27 && !SceneMgr.Get().IsInTavernBrawlMode())
		{
			return GameUtils.IsTraditionalTutorialComplete();
		}
		return false;
	}

	public bool ShouldDeckBoxesBeLocked()
	{
		return !GameUtils.IsTraditionalTutorialComplete();
	}

	public void SetEditingTraySection(int index)
	{
		m_editingTraySection = m_traySections[index];
		m_centeringDeckList = m_editingTraySection.m_deckBox.GetPositionIndex();
	}

	protected bool IsEditingCards()
	{
		return CollectionManager.Get().GetEditedDeck() != null;
	}

	protected virtual void OnDeckBoxVisualOver(CollectionDeckBoxVisual deckBox)
	{
		if (!deckBox.IsLocked() && !UniversalInputManager.Get().IsTouchMode())
		{
			if (CollectionManager.Get().IsEditingDeathKnightDeck())
			{
				deckBox.ResetColliderHeight();
			}
			if (IsEditingCards() && m_deckInfoTooltip != null)
			{
				ShowDeckInfo();
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
			if (m_deckInfoTooltip != null && m_deckInfoTooltip.IsShown())
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
		if (!GameUtils.IsCardGameplayEventActive(deckBox.GetHeroCardID()))
		{
			DialogManager.Get().ShowClassUpcomingPopup();
			return;
		}
		deckBox.enabled = true;
		if (base.IsTouchDragging || m_deckTray.IsUpdatingTrayMode())
		{
			return;
		}
		long deckID = deckBox.GetDeckID();
		CollectionDeck deck = CollectionManager.Get().GetDeck(deckID);
		if (deck != null)
		{
			if (deck.IsBeingDeleted())
			{
				Log.DeckTray.Print($"CollectionDeckTrayDeckListContent.OnDeckBoxVisualRelease(): cannot edit deck {deck}; it is being deleted");
				return;
			}
			if (deck.IsSavingChanges())
			{
				Log.DeckTray.PrintWarning("CollectionDeckTrayDeckListContent.OnDeckBoxVisualRelease(): cannot edit deck {0}; waiting for changes to be saved", deck);
				return;
			}
		}
		if (IsEditingCards())
		{
			if (!UniversalInputManager.Get().IsTouchMode())
			{
				RenameCurrentlyEditingDeck();
			}
			else if (m_deckInfoTooltip != null && !m_deckInfoTooltip.IsShown())
			{
				ShowDeckInfo();
			}
		}
		else if (IsModeActive())
		{
			m_editingTraySection = traySection;
			m_centeringDeckList = m_editingTraySection.m_deckBox.GetPositionIndex();
			m_newDeckButton.SetEnabled(enabled: false);
			CollectionManagerDisplay cmd = CollectionManager.Get().GetCollectibleDisplay() as CollectionManagerDisplay;
			if (cmd != null)
			{
				cmd.RequestContentsToShowDeck(deckID);
				cmd.HideDeckHelpPopup();
				cmd.HideSetFilterTutorial();
			}
			Options.Get().SetBool(Option.HAS_STARTED_A_DECK, val: true);
		}
	}

	protected void FireDeckCountChangedEvent()
	{
		DeckCountChanged[] array = m_deckCountChangedListeners.ToArray();
		int count = CollectionManager.Get().GetDecks(DeckType.NORMAL_DECK).Count;
		DeckCountChanged[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i](count);
		}
	}

	protected void FireBusyWithDeckEvent(bool busy)
	{
		BusyWithDeck[] array = m_busyWithDeckListeners.ToArray();
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

	protected int UpdateDeckTrayVisuals()
	{
		return UpdateDeckTrayVisuals(rerollAllHeroes: false);
	}

	protected int UpdateDeckTrayVisuals(bool rerollAllHeroes, TAG_CLASS heroClassToReroll = TAG_CLASS.INVALID)
	{
		List<CollectionDeck> decks = GetActiveDecks();
		for (int index = 0; index < decks.Count && index < m_traySections.Count; index++)
		{
			if (index < decks.Count)
			{
				CollectionDeck deck = decks[index];
				bool rerollFavoriteHero = rerollAllHeroes || deck.GetClass() == heroClassToReroll;
				m_traySections[index].m_deckBox.AssignFromCollectionDeck(deck, rerollFavoriteHero);
			}
		}
		return decks.Count;
	}

	protected List<CollectionDeck> GetActiveDecks()
	{
		List<CollectionDeck> decks = null;
		if (SceneMgr.Get().IsInTavernBrawlMode())
		{
			decks = CollectionManager.Get().GetDecks(DeckType.TAVERN_BRAWL_DECK);
			int brawlLibraryItemId = TavernBrawlManager.Get().CurrentMission()?.SelectedBrawlLibraryItemId ?? 0;
			decks.RemoveAll((CollectionDeck deck) => deck.BrawlLibraryItemId != brawlLibraryItemId);
		}
		else
		{
			decks = CollectionManager.Get().GetDecks(DeckType.NORMAL_DECK);
		}
		return decks;
	}

	public void OnDeckContentsUpdated(long deckID)
	{
		foreach (TraySection traySection in m_traySections)
		{
			if (traySection.m_deckBox != null)
			{
				CollectionDeck deck = traySection.m_deckBox.GetCollectionDeck();
				if (deck != null)
				{
					traySection.m_deckBox.AssignFromCollectionDeck(deck, rerollFavoriteHero: false);
				}
			}
		}
	}

	protected void SwapEditTrayIfNeeded(long editDeckID)
	{
		if (editDeckID < 0)
		{
			return;
		}
		TraySection realSection = null;
		foreach (TraySection tray in m_traySections)
		{
			if (tray.m_deckBox.GetDeckID() == editDeckID)
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
			realSection.m_deckBox.transform.position = m_deckEditTopPos.position;
			realSection.ShowDeckBoxNoAnim();
			realSection.m_deckBox.SetEnabled(enabled: true);
			m_editingTraySection = realSection;
		}
	}

	protected override void UpdateDragToReorder()
	{
		if (m_draggingDeckBox == null)
		{
			return;
		}
		if (!InputCollection.GetMouseButton(0) || !CanDragToReorderDecks)
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
		int numDeckBoxes = CollectionManager.Get().GetDecks(DeckType.NORMAL_DECK).Count;
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
			CollectionDeck deck = traySection.m_deckBox.GetCollectionDeck();
			if (deck != null)
			{
				deck.SortOrder = i + -100;
			}
		}
		RefreshTraySectionPositions(animateToNewPositions: true);
	}

	private void RefreshTraySectionPositions(bool animateToNewPositions)
	{
		Vector3 nextLocalPosition = m_traySectionStartPos.localPosition;
		Vector3 prevOriginToBottomCenter = Vector3.zero;
		Transform parentTransform = m_traySectionStartPos.parent;
		for (int i = 0; i < 29; i++)
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
			UnityEngine.Vector2 textureOffset = new UnityEngine.Vector2(0f, -0.0825f * (float)i);
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
