using System;
using System.Collections;
using Blizzard.T5.MaterialService.Extensions;
using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

public class DeckTrayCardBackContent : DeckTrayContent
{
	private class AnimatedCardBack
	{
		public int CardBackId;

		public GameObject GameObject;

		public Vector3 OriginalScale;

		public Vector3 OriginalPosition;
	}

	[SerializeField]
	[Header("Positioning")]
	private GameObject m_root;

	[SerializeField]
	private Vector3 m_trayHiddenOffset;

	[SerializeField]
	private GameObject m_cardBackContainer;

	[SerializeField]
	[Header("Animation")]
	private iTween.EaseType m_traySlideSlideInAnimation = iTween.EaseType.easeOutBounce;

	[SerializeField]
	private iTween.EaseType m_traySlideSlideOutAnimation;

	[SerializeField]
	private float m_traySlideAnimationTime = 0.25f;

	[SerializeField]
	private SpellType m_removalSpellType;

	[SerializeField]
	[Header("Sound")]
	private WeakAssetReference m_appearanceSound;

	[SerializeField]
	private WeakAssetReference m_socketSound;

	[SerializeField]
	private WeakAssetReference m_unsocketSound;

	[SerializeField]
	private WeakAssetReference m_pickUpSound;

	private Widget m_rootWidget;

	private DeckDataModel m_deckDataModel;

	private GameObject m_currentCardBack;

	private CollectionDeck m_currentDeck;

	private Vector3 m_originalLocalPosition;

	private bool m_animatingTray;

	private bool m_waitingToLoadCardback;

	private bool m_shouldUpdateLimitToFavoritesSetting;

	private Notification m_dragToRemoveNotification;

	private bool m_shouldShowDragToRemoveNotification;

	private Notification m_randomIsDefaultNotification;

	private bool m_shouldShowRandomIsDefaultNotification;

	private const string CODE_CHECKBOX_TOGGLED = "CODE_CHECKBOX_TOGGLED";

	private const string CODE_TRAY_CLICKED = "CODE_TRAY_CLICKED";

	private const string CODE_TRAY_DRAG_START = "CODE_TRAY_DRAG_START";

	private AnimatedCardBack m_animData;

	public bool WaitingForCardbackAnimation
	{
		get
		{
			if (m_animData == null)
			{
				return m_waitingToLoadCardback;
			}
			return true;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		m_rootWidget = GetComponent<WidgetTemplate>();
		if (m_rootWidget != null)
		{
			m_rootWidget.RegisterEventListener(WidgetEventListener);
			m_rootWidget.RegisterReadyListener(WidgetReadyListener);
		}
		m_originalLocalPosition = base.transform.localPosition;
		base.transform.localPosition = m_originalLocalPosition + m_trayHiddenOffset;
		m_root.SetActive(value: false);
		m_shouldShowRandomIsDefaultNotification = !GameUtils.IsGSDFlagSet(GameSaveKeyId.COLLECTION_MANAGER, GameSaveKeySubkeyId.COLLECTION_MANAGER_HAS_SEEN_UPDATED_CARD_BACK_DECK_TRAY_EMPTY);
		m_shouldShowDragToRemoveNotification = !GameUtils.IsGSDFlagSet(GameSaveKeyId.COLLECTION_MANAGER, GameSaveKeySubkeyId.COLLECTION_MANAGER_HAS_SEEN_UPDATED_CARD_BACK_DECK_TRAY_ASSIGNED);
	}

	private void WidgetReadyListener(object unused)
	{
		m_deckDataModel = new DeckDataModel();
		m_deckDataModel.RandomCardBackFavoritesOnly = true;
		m_rootWidget.BindDataModel(m_deckDataModel);
	}

	private void WidgetEventListener(string eventName)
	{
		switch (eventName)
		{
		case "CODE_CHECKBOX_TOGGLED":
		{
			bool enabled = !m_deckDataModel.RandomCardBackFavoritesOnly;
			ToggleFavoritesOnly(enabled);
			break;
		}
		case "CODE_TRAY_CLICKED":
			RemoveCardBack();
			break;
		case "CODE_TRAY_DRAG_START":
			GrabCardBack();
			break;
		}
	}

	private void ToggleFavoritesOnly(bool enabled)
	{
		int idOfTheRandomCardBack = CardBackManager.Get().TheRandomCardBackID;
		int? cardBackToUse = (enabled ? ((int?)null) : new int?(idOfTheRandomCardBack));
		if (m_currentDeck.CardBackID != cardBackToUse)
		{
			m_currentDeck.CardBackID = cardBackToUse;
		}
		UpdateDatamodel();
		m_shouldUpdateLimitToFavoritesSetting = true;
	}

	public bool DeckHasCardBackOverride()
	{
		int idOfTheRandomCardBack = CardBackManager.Get().TheRandomCardBackID;
		if (m_currentDeck.CardBackID.HasValue)
		{
			return m_currentDeck.CardBackID != idOfTheRandomCardBack;
		}
		return false;
	}

	public bool AnimateInCardBack(int cardBackId, GameObject original)
	{
		if (m_animData != null || m_waitingToLoadCardback)
		{
			return false;
		}
		m_waitingToLoadCardback = true;
		if (!CardBackManager.Get().LoadCardBackByIndex(cardBackId, delegate(CardBackManager.LoadCardBackData cardBackData)
		{
			m_waitingToLoadCardback = false;
			AnimateCardBackAssignmentFromPageVisual(cardBackData, original);
		}, "Card_Hidden.prefab:1a94649d257bc284ca6e2962f634a8b9"))
		{
			m_waitingToLoadCardback = false;
			Debug.LogError("Could not load CardBack " + cardBackId);
			return false;
		}
		return true;
	}

	public void UpdateCardBack(int cardBackId, bool assigning, GameObject obj = null)
	{
		if (m_currentDeck == null)
		{
			return;
		}
		if (assigning)
		{
			if (!string.IsNullOrEmpty(m_socketSound.AssetString))
			{
				SoundManager.Get().LoadAndPlay(m_socketSound.AssetString, base.gameObject);
			}
			m_currentDeck.CardBackID = cardBackId;
		}
		UpdateDatamodel();
		ShowTutorialIfNeeded();
		ToggleSparkleEffects(enabled: false);
		if (obj != null)
		{
			SetCardBackObject(obj, assigning);
			return;
		}
		m_waitingToLoadCardback = true;
		if (!CardBackManager.Get().LoadCardBackByIndex(cardBackId, delegate(CardBackManager.LoadCardBackData cardBackData)
		{
			m_waitingToLoadCardback = false;
			if (m_currentDeck == null || m_currentDeck.CardBackID != cardBackId)
			{
				UnityEngine.Object.Destroy(cardBackData.m_GameObject);
			}
			else
			{
				GameObject go = cardBackData.m_GameObject;
				SetCardBackObject(go, assigning);
			}
		}, "Card_Hidden.prefab:1a94649d257bc284ca6e2962f634a8b9"))
		{
			m_waitingToLoadCardback = false;
			Debug.LogWarning($"CardBackManager was unable to load card back ID: {cardBackId}");
		}
	}

	private void GrabCardBack()
	{
		if (m_currentCardBack != null)
		{
			Actor cbActor = m_currentCardBack.GetComponent<Actor>();
			int? deckCardBack = m_currentDeck.CardBackID;
			if (deckCardBack.HasValue && CollectionInputMgr.Get().GrabCardBackFromSlot(cbActor, deckCardBack.Value))
			{
				m_currentDeck.CardBackID = null;
				ClearCardBackGameObject();
				UpdateDatamodel();
				ShowTutorialIfNeeded();
				ToggleSparkleEffects(enabled: true);
			}
		}
	}

	public void ToggleSparkleEffects(bool enabled)
	{
		if (m_deckDataModel != null)
		{
			m_deckDataModel.DraggingDeckAssignment = enabled;
		}
	}

	private void RemoveCardBack()
	{
		if (!(m_currentCardBack != null) || !DeckHasCardBackOverride())
		{
			return;
		}
		Actor actor = m_currentCardBack.GetComponent<Actor>();
		Spell unsocketSpell = actor.GetSpell(m_removalSpellType);
		CardBackSummon cardBackSummon = unsocketSpell.gameObject.GetComponentInChildren<CardBackSummon>();
		CardBack cardBack = actor.GetComponentInChildren<CardBack>();
		if (cardBackSummon != null && cardBack != null)
		{
			cardBackSummon.UpdateEffectWithCardBack(cardBack);
		}
		m_currentDeck.CardBackID = null;
		UpdateDatamodel();
		ShowTutorialIfNeeded();
		if (!string.IsNullOrEmpty(m_unsocketSound.AssetString))
		{
			SoundManager.Get().LoadAndPlay(m_unsocketSound.AssetString, base.gameObject);
		}
		if (unsocketSpell == null)
		{
			ClearCardBackGameObject();
			return;
		}
		unsocketSpell.AddFinishedCallback(delegate(Spell spell, object userData)
		{
			SpellManager.Get().ReleaseSpell(spell, reset: true);
			ClearCardBackGameObject();
		});
		unsocketSpell.ActivateState(SpellStateType.BIRTH);
	}

	private void ClearCardBackGameObject()
	{
		if (m_currentCardBack != null)
		{
			UnityEngine.Object.Destroy(m_currentCardBack);
			m_currentCardBack = null;
		}
	}

	private void SetCardBackObject(GameObject go, bool assigning)
	{
		GameUtils.SetParent(go, m_cardBackContainer, withRotation: true);
		CollectionDraggableCardVisual heldCardVisual = CollectionInputMgr.Get()?.GetHeldCardVisual();
		if (heldCardVisual != null)
		{
			heldCardVisual.InitActorCache();
		}
		Actor actor = go.GetComponent<Actor>();
		if (actor == null)
		{
			UnityEngine.Object.Destroy(go);
			return;
		}
		if (assigning)
		{
			Spell socketSpell = actor.GetSpell(SpellType.DEATHREVERSE);
			if (socketSpell != null)
			{
				socketSpell.ActivateState(SpellStateType.BIRTH);
			}
		}
		if (m_currentCardBack != null)
		{
			UnityEngine.Object.Destroy(m_currentCardBack);
		}
		m_currentCardBack = go;
		GameObject cbMesh = actor.m_cardMesh;
		actor.SetCardbackUpdateIgnore(ignoreUpdate: true);
		actor.SetUnlit();
		actor.UpdateAllComponents();
		if (cbMesh != null)
		{
			Material cbMaterial = cbMesh.GetComponent<Renderer>().GetMaterial();
			if (cbMaterial.HasProperty("_SpecularIntensity"))
			{
				cbMaterial.SetFloat("_SpecularIntensity", 0f);
			}
		}
	}

	private void UpdateDatamodel()
	{
		if (DeckHasCardBackOverride())
		{
			int cardBackId = m_currentDeck.CardBackID.Value;
			CardBackDbfRecord cardBack = GameDbf.CardBack.GetRecord(cardBackId);
			if (m_deckDataModel.CardBack == null)
			{
				m_deckDataModel.CardBack = new CardBackDataModel();
			}
			m_deckDataModel.CardBack.CardBackId = cardBackId;
			m_deckDataModel.CardBack.Name = cardBack.Name;
		}
		else
		{
			bool randomBackIsAssigned = m_currentDeck.CardBackID.HasValue;
			m_deckDataModel.RandomCardBackFavoritesOnly = !randomBackIsAssigned;
			m_deckDataModel.CardBack = null;
		}
	}

	private void SaveRandomCardBackSelectionPreference()
	{
		if (m_shouldUpdateLimitToFavoritesSetting)
		{
			bool num = GameUtils.IsGSDFlagSet(GameSaveKeyId.COLLECTION_MANAGER, GameSaveKeySubkeyId.COLLECTION_MANAGER_RANDOM_CARD_BACK_USE_ALL_OWNED);
			bool deckIsUsingRandomOwned = !m_deckDataModel.RandomCardBackFavoritesOnly;
			if (num != deckIsUsingRandomOwned)
			{
				GameUtils.SetGSDFlag(GameSaveKeyId.COLLECTION_MANAGER, GameSaveKeySubkeyId.COLLECTION_MANAGER_RANDOM_CARD_BACK_USE_ALL_OWNED, deckIsUsingRandomOwned);
			}
			CardBackManager.Get().LoadRandomCardBackIntoFavoriteSlot(updateScene: true);
			m_shouldUpdateLimitToFavoritesSetting = false;
		}
	}

	public void AnimateCardBackAssignmentFromPageVisual(CardBackManager.LoadCardBackData cardBackData, GameObject original)
	{
		GameObject go = cardBackData.m_GameObject;
		go.GetComponent<Actor>().GetSpell(SpellType.DEATHREVERSE).Reactivate();
		AnimatedCardBack cardBack = new AnimatedCardBack();
		cardBack.CardBackId = cardBackData.m_CardBackIndex;
		cardBack.GameObject = go;
		cardBack.OriginalScale = go.transform.localScale;
		cardBack.OriginalPosition = original.transform.position;
		m_animData = cardBack;
		go.transform.position = new Vector3(original.transform.position.x, original.transform.position.y + 0.5f, original.transform.position.z);
		go.transform.localScale = m_cardBackContainer.transform.lossyScale;
		Hashtable args = iTweenManager.Get().GetTweenHashTable();
		args.Add("from", 0f);
		args.Add("to", 1f);
		args.Add("time", 0.6f);
		args.Add("easetype", iTween.EaseType.easeOutCubic);
		args.Add("onupdate", "AnimateNewCardUpdate");
		args.Add("onupdatetarget", base.gameObject);
		args.Add("oncomplete", "AnimateNewCardFinished");
		args.Add("oncompleteparams", cardBack);
		args.Add("oncompletetarget", base.gameObject);
		iTween.ValueTo(go, args);
		if (!string.IsNullOrEmpty(m_pickUpSound.AssetString))
		{
			SoundManager.Get().LoadAndPlay(m_pickUpSound.AssetString, base.gameObject);
		}
	}

	private void AnimateNewCardUpdate(float val)
	{
		GameObject go = m_animData.GameObject;
		Vector3 start = m_animData.OriginalPosition;
		Vector3 end = m_cardBackContainer.transform.position;
		if (val <= 0.85f)
		{
			val /= 0.85f;
			go.transform.position = new Vector3(Mathf.Lerp(start.x, end.x, val), Mathf.Lerp(start.y, end.y, val) + Mathf.Sin(val * (float)Math.PI) * 15f + val * 4f, Mathf.Lerp(start.z, end.z, val));
			return;
		}
		if (m_currentCardBack != null)
		{
			UnityEngine.Object.Destroy(m_currentCardBack);
			m_currentCardBack = null;
		}
		val = (val - 0.85f) / 0.14999998f;
		go.transform.position = new Vector3(end.x, end.y + Mathf.Lerp(4f, 0f, val), end.z);
	}

	private void AnimateNewCardFinished(AnimatedCardBack cardBack)
	{
		cardBack.GameObject.transform.localScale = cardBack.OriginalScale;
		UpdateCardBack(cardBack.CardBackId, assigning: true, cardBack.GameObject);
		m_animData = null;
	}

	public override bool PreAnimateContentEntrance()
	{
		m_currentDeck = CollectionManager.Get().GetEditedDeck();
		m_shouldUpdateLimitToFavoritesSetting = false;
		ClearCardBackGameObject();
		if (DeckHasCardBackOverride())
		{
			int cardBackToShow = m_currentDeck.CardBackID.Value;
			UpdateCardBack(cardBackToShow, assigning: false);
		}
		return true;
	}

	public override bool AnimateContentEntranceStart()
	{
		if (m_waitingToLoadCardback)
		{
			return false;
		}
		m_root.SetActive(value: true);
		UpdateDatamodel();
		if (!string.IsNullOrEmpty(m_appearanceSound.AssetString))
		{
			SoundManager.Get().LoadAndPlay(m_appearanceSound.AssetString, base.gameObject);
		}
		base.transform.localPosition = m_originalLocalPosition;
		m_animatingTray = true;
		iTween.MoveFrom(base.gameObject, iTween.Hash("position", m_originalLocalPosition + m_trayHiddenOffset, "islocal", true, "time", m_traySlideAnimationTime, "easetype", m_traySlideSlideInAnimation, "oncomplete", (Action<object>)delegate
		{
			m_animatingTray = false;
		}));
		return true;
	}

	public override bool AnimateContentEntranceEnd()
	{
		if (!m_animatingTray)
		{
			ShowTutorialIfNeeded();
		}
		return !m_animatingTray;
	}

	public override bool AnimateContentExitStart()
	{
		HideTutorials();
		SaveRandomCardBackSelectionPreference();
		base.transform.localPosition = m_originalLocalPosition;
		m_animatingTray = true;
		iTween.MoveTo(base.gameObject, iTween.Hash("position", m_originalLocalPosition + m_trayHiddenOffset, "islocal", true, "time", m_traySlideAnimationTime, "easetype", m_traySlideSlideOutAnimation, "oncomplete", (Action<object>)delegate
		{
			m_animatingTray = false;
			m_root.SetActive(value: false);
		}));
		return true;
	}

	public override bool AnimateContentExitEnd()
	{
		return !m_animatingTray;
	}

	public void ShowTutorialIfNeeded()
	{
		HideTutorials();
		bool overridden = DeckHasCardBackOverride();
		if (!(overridden ? m_shouldShowDragToRemoveNotification : m_shouldShowRandomIsDefaultNotification))
		{
			return;
		}
		Transform tutorialBone = (CollectionManager.Get().GetCollectibleDisplay() as CollectionManagerDisplay).m_cardBackDeckTrayTutorialBone;
		if (tutorialBone == null)
		{
			Debug.LogWarning("No bone for card back deck tray tutorials. Did you forget a connection in CollectionManagerDisplay?");
			return;
		}
		string textToShow = (overridden ? GameStrings.Get("GLUE_COLLECTION_TUTORIAL_UPDATED_CARD_BACK_DECK_TRAY_ASSIGNED") : GameStrings.Get("GLUE_COLLECTION_TUTORIAL_UPDATED_CARD_BACK_DECK_TRAY_EMPTY"));
		Notification notification = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, tutorialBone, textToShow);
		if (notification != null)
		{
			notification.ShowPopUpArrow(Notification.PopUpArrowDirection.Right);
			notification.PulseReminderEveryXSeconds(3f);
			if (overridden)
			{
				m_dragToRemoveNotification = notification;
				m_shouldShowDragToRemoveNotification = false;
				GameUtils.SetGSDFlag(GameSaveKeyId.COLLECTION_MANAGER, GameSaveKeySubkeyId.COLLECTION_MANAGER_HAS_SEEN_UPDATED_CARD_BACK_DECK_TRAY_ASSIGNED, enableFlag: true);
			}
			else
			{
				m_randomIsDefaultNotification = notification;
				m_shouldShowRandomIsDefaultNotification = false;
				GameUtils.SetGSDFlag(GameSaveKeyId.COLLECTION_MANAGER, GameSaveKeySubkeyId.COLLECTION_MANAGER_HAS_SEEN_UPDATED_CARD_BACK_DECK_TRAY_EMPTY, enableFlag: true);
			}
		}
	}

	public void HideTutorials()
	{
		if (m_dragToRemoveNotification != null)
		{
			NotificationManager.Get().DestroyNotificationNowWithNoAnim(m_dragToRemoveNotification);
		}
		if (m_randomIsDefaultNotification != null)
		{
			NotificationManager.Get().DestroyNotificationNowWithNoAnim(m_randomIsDefaultNotification);
		}
	}
}
