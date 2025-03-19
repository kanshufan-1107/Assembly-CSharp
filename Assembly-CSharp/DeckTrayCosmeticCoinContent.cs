using System;
using System.Collections;
using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

public class DeckTrayCosmeticCoinContent : DeckTrayContent
{
	private class AnimatedCosmeticCoin
	{
		public Actor Actor;

		public GameObject GameObject;

		public Vector3 OriginalScale;

		public Vector3 OriginalPosition;
	}

	[Header("Positioning")]
	[SerializeField]
	private GameObject m_root;

	[SerializeField]
	private Vector3 m_trayHiddenOffset;

	[SerializeField]
	private GameObject m_cosmeticCoinContainer;

	[Header("Animation")]
	[SerializeField]
	private iTween.EaseType m_traySlideSlideInAnimation = iTween.EaseType.easeOutBounce;

	[SerializeField]
	private iTween.EaseType m_traySlideSlideOutAnimation;

	[SerializeField]
	private float m_traySlideAnimationTime = 0.25f;

	[SerializeField]
	private SpellType m_removalSpellType;

	[Header("Sound")]
	[SerializeField]
	private WeakAssetReference m_appearanceSound;

	[SerializeField]
	private WeakAssetReference m_socketSound;

	[SerializeField]
	private WeakAssetReference m_addSound;

	[SerializeField]
	private WeakAssetReference m_unsocketSound;

	private const string ADD_CARD_TO_DECK_SOUND = "collection_manager_card_add_to_deck_instant.prefab:06df359c4026d7e47b06a4174f33e3ef";

	private Widget m_rootWidget;

	private DeckDataModel m_deckDataModel;

	private CollectionDeck m_currentDeck;

	private Actor m_cosmeticCoinActor;

	private Vector3 m_originalLocalPosition;

	private bool m_animating;

	private bool m_waitingToLoadCoinDef;

	private bool m_shouldUpdateLimitToFavoritesSetting;

	private const string CODE_CHECKBOX_TOGGLED = "CODE_CHECKBOX_TOGGLED";

	private const string CODE_TRAY_CLICKED = "CODE_TRAY_CLICKED";

	private const string CODE_TRAY_DRAG_START = "CODE_TRAY_DRAG_START";

	private AnimatedCosmeticCoin m_animData;

	public event Action<string> OnCoinChanged;

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
		LoadCosmeticCoinActor();
	}

	private void WidgetReadyListener(object unused)
	{
		m_deckDataModel = new DeckDataModel();
		m_deckDataModel.RandomCoinFavoritesOnly = true;
		m_rootWidget.BindDataModel(m_deckDataModel);
	}

	private void WidgetEventListener(string eventName)
	{
		switch (eventName)
		{
		case "CODE_CHECKBOX_TOGGLED":
		{
			bool shouldEnable = !m_deckDataModel.RandomCoinFavoritesOnly;
			ToggleFavoritesOnly(shouldEnable);
			break;
		}
		case "CODE_TRAY_CLICKED":
			RemoveCosmeticCoin();
			break;
		case "CODE_TRAY_DRAG_START":
			GrabCosmeticCoin();
			break;
		}
	}

	private void ToggleFavoritesOnly(bool enabled)
	{
		m_currentDeck.RandomCoinUseFavorite = enabled;
		UpdateDatamodel();
		this.OnCoinChanged?.Invoke(string.Empty);
		m_shouldUpdateLimitToFavoritesSetting = true;
	}

	public void UpdateCosmeticCoin(int coinId, bool assigning, Actor baseActor = null)
	{
		if (m_currentDeck == null)
		{
			return;
		}
		ToggleSparkleEffects(enabled: false);
		if (assigning)
		{
			if (!string.IsNullOrEmpty(m_socketSound.AssetString))
			{
				SoundManager.Get().LoadAndPlay(m_socketSound.AssetString);
			}
			ShowSocketFX();
			if (m_currentDeck.CosmeticCoinID.HasValue && m_currentDeck.CosmeticCoinID.Value == coinId)
			{
				return;
			}
			m_currentDeck.CosmeticCoinID = coinId;
		}
		if (baseActor != null)
		{
			using DefLoader.DisposableCardDef cardDef = baseActor.ShareDisposableCardDef();
			UpdateCosmeticCoinVisual(baseActor.GetEntityDef(), cardDef, assigning);
		}
		else
		{
			m_waitingToLoadCoinDef = true;
			string coinCard = CosmeticCoinManager.Get().GetCoinCardFromCoinId(coinId);
			DefLoader.Get().LoadFullDef(coinCard, delegate(string cardID, DefLoader.DisposableFullDef fullDef, object callbackData)
			{
				using (fullDef)
				{
					m_waitingToLoadCoinDef = false;
					UpdateCosmeticCoinVisual(fullDef.EntityDef, fullDef.DisposableCardDef, assigning);
				}
			});
		}
		UpdateCosmeticCoinObject();
		UpdateDatamodel();
	}

	private void GrabCosmeticCoin()
	{
		if (m_cosmeticCoinActor != null && CollectionInputMgr.Get().GrabCosmeticCoinFromSlot(m_cosmeticCoinActor))
		{
			m_currentDeck.CosmeticCoinID = null;
			UpdateDatamodel();
			UpdateCosmeticCoinObject();
			ToggleSparkleEffects(enabled: true);
			this.OnCoinChanged?.Invoke(string.Empty);
		}
	}

	public void ToggleSparkleEffects(bool enabled)
	{
		if (m_deckDataModel != null)
		{
			m_deckDataModel.DraggingDeckAssignment = enabled;
		}
	}

	private void RemoveCosmeticCoin()
	{
		if (!(m_cosmeticCoinActor != null) || !m_currentDeck.CosmeticCoinID.HasValue)
		{
			return;
		}
		Spell unsocketSpell = m_cosmeticCoinActor.GetSpell(m_removalSpellType);
		m_currentDeck.CosmeticCoinID = null;
		UpdateDatamodel();
		this.OnCoinChanged?.Invoke(string.Empty);
		if (!string.IsNullOrEmpty(m_unsocketSound.AssetString))
		{
			SoundManager.Get().LoadAndPlay(m_unsocketSound.AssetString, base.gameObject);
		}
		if (unsocketSpell == null)
		{
			UpdateCosmeticCoinObject();
			return;
		}
		unsocketSpell.AddFinishedCallback(delegate
		{
			UpdateCosmeticCoinObject();
		});
		unsocketSpell.ActivateState(SpellStateType.BIRTH);
	}

	private void LoadCosmeticCoinActor()
	{
		string actorPath = ActorNames.GetHandActor(TAG_CARDTYPE.SPELL, TAG_PREMIUM.NORMAL);
		AssetLoader.Get().InstantiatePrefab(actorPath, delegate(AssetReference assetRef, GameObject go, object callbackData)
		{
			if (go == null)
			{
				Debug.LogWarning($"DeckTrayCosmeticCoinContent.LoadCosmeticCoinActor - FAILED to load \"{assetRef}\"");
			}
			else
			{
				Actor component = go.GetComponent<Actor>();
				if (component == null)
				{
					Debug.LogWarning($"HandActorCache.OnActorLoaded() - ERROR \"{assetRef}\" has no Actor component");
				}
				else
				{
					GameUtils.SetParent(component, m_cosmeticCoinContainer);
					m_cosmeticCoinActor = component;
				}
			}
		}, null, AssetLoadingOptions.IgnorePrefabPosition);
	}

	private void UpdateCosmeticCoinVisual(EntityDef entityDef, DefLoader.DisposableCardDef cardDef, bool assigning)
	{
		if (m_cosmeticCoinActor == null)
		{
			Debug.LogError("Cosmetic coin object not loaded yet! Cannot set coin!");
			return;
		}
		m_cosmeticCoinActor.SetEntityDef(entityDef);
		m_cosmeticCoinActor.SetCardDef(cardDef);
		GameObject cosmeticCoinRoot = m_cosmeticCoinActor.GetRootObject();
		if (cosmeticCoinRoot != null && !cosmeticCoinRoot.activeSelf)
		{
			cosmeticCoinRoot.SetActive(value: true);
		}
		m_cosmeticCoinActor.UpdateAllComponents();
		if (assigning)
		{
			this.OnCoinChanged?.Invoke(entityDef.GetCardId());
		}
		if (assigning && cardDef?.CardDef != null)
		{
			GameUtils.LoadCardDefEmoteSound(cardDef.CardDef.m_EmoteDefs, EmoteType.PICKED, OnPickEmoteLoaded);
		}
		ShowSocketFX();
	}

	private void OnPickEmoteLoaded(CardSoundSpell spell)
	{
		if (!(spell == null))
		{
			spell.AddFinishedCallback(OnPickEmoteFinished);
			spell.Reactivate();
		}
	}

	private void OnPickEmoteFinished(Spell spell, object userData)
	{
		UnityEngine.Object.Destroy(spell.gameObject);
	}

	private void ShowSocketFX()
	{
		Spell socketSpell = m_cosmeticCoinActor.GetSpell(SpellType.DEATHREVERSE);
		if (socketSpell != null)
		{
			socketSpell.ActivateState(SpellStateType.BIRTH);
		}
	}

	private void UpdateCosmeticCoinObject()
	{
		Spell unsocketSpell = m_cosmeticCoinActor.GetSpell(m_removalSpellType);
		SpellManager.Get().ReleaseSpell(unsocketSpell, reset: true);
		m_cosmeticCoinActor.UpdateAllComponents();
		m_cosmeticCoinActor.gameObject.transform.position = m_cosmeticCoinContainer.transform.position;
		m_cosmeticCoinActor.gameObject.SetActive(m_currentDeck.CosmeticCoinID.HasValue);
	}

	private void UpdateDatamodel()
	{
		m_deckDataModel.RandomCoinFavoritesOnly = m_currentDeck.RandomCoinUseFavorite;
		if (m_currentDeck.CosmeticCoinID.HasValue)
		{
			if (m_deckDataModel.CosmeticCoin == null)
			{
				m_deckDataModel.CosmeticCoin = new CardDataModel();
			}
			CosmeticCoinDbfRecord coinRecord = GameDbf.CosmeticCoin.GetRecord(m_currentDeck.CosmeticCoinID.Value);
			if (coinRecord == null)
			{
				Log.CoinManager.PrintWarning("UpdateDatamodel: Coin record not found.");
				return;
			}
			m_deckDataModel.CosmeticCoin.Name = coinRecord.Name;
			string coinCardId = CosmeticCoinManager.Get().GetCoinCardFromCoinId(m_currentDeck.CosmeticCoinID.Value);
			m_deckDataModel.CosmeticCoin.CardId = coinCardId;
		}
		else
		{
			m_deckDataModel.CosmeticCoin = null;
		}
	}

	private void SaveRandomCoinSelectionPreference()
	{
		if (m_shouldUpdateLimitToFavoritesSetting)
		{
			bool num = GameUtils.IsGSDFlagSet(GameSaveKeyId.COLLECTION_MANAGER, GameSaveKeySubkeyId.COLLECTION_MANAGER_RANDOM_COSMETIC_COIN_USE_ALL_OWNED);
			bool deckIsUsingRandomOwned = !m_deckDataModel.RandomCoinFavoritesOnly;
			if (num != deckIsUsingRandomOwned)
			{
				GameUtils.SetGSDFlag(GameSaveKeyId.COLLECTION_MANAGER, GameSaveKeySubkeyId.COLLECTION_MANAGER_RANDOM_COSMETIC_COIN_USE_ALL_OWNED, deckIsUsingRandomOwned);
			}
			m_shouldUpdateLimitToFavoritesSetting = false;
		}
	}

	public void AnimateCoinAssignmentFromPageVisual(Actor actor)
	{
		GameObject go = actor.gameObject;
		AnimatedCosmeticCoin animData = new AnimatedCosmeticCoin();
		animData.Actor = actor;
		animData.GameObject = go;
		animData.OriginalScale = go.transform.localScale;
		animData.OriginalPosition = go.transform.position;
		m_animData = animData;
		go.transform.position = new Vector3(go.transform.position.x, go.transform.position.y + 0.5f, go.transform.position.z);
		go.transform.localScale = m_cosmeticCoinContainer.transform.lossyScale;
		Hashtable args = iTweenManager.Get().GetTweenHashTable();
		args.Add("from", 0f);
		args.Add("to", 1f);
		args.Add("time", 0.6f);
		args.Add("easetype", iTween.EaseType.easeOutCubic);
		args.Add("onupdate", "AnimateNewCosmeticCoinUpdate");
		args.Add("onupdatetarget", base.gameObject);
		args.Add("oncomplete", "AnimateNewCosmeticCoinFinished");
		args.Add("oncompleteparams", animData);
		args.Add("oncompletetarget", base.gameObject);
		iTween.ValueTo(go, args);
		SoundManager.Get().LoadAndPlay(m_addSound.AssetString);
	}

	private void AnimateNewCosmeticCoinFinished()
	{
		m_cosmeticCoinActor.gameObject.SetActive(value: true);
		Actor actor = m_animData.Actor;
		string coinCard = actor.GetEntityDef().GetCardId();
		int coinId = CosmeticCoinManager.Get().GetCoinIdFromCoinCard(coinCard);
		UpdateCosmeticCoin(coinId, assigning: true, actor);
		UnityEngine.Object.Destroy(m_animData.GameObject);
		m_animData = null;
	}

	private void AnimateNewCosmeticCoinUpdate(float val)
	{
		GameObject go = m_animData.GameObject;
		Vector3 start = m_animData.OriginalPosition;
		Vector3 end = m_cosmeticCoinContainer.transform.position;
		if (val <= 0.85f)
		{
			val /= 0.85f;
			go.transform.position = new Vector3(Mathf.Lerp(start.x, end.x, val), Mathf.Lerp(start.y, end.y, val) + Mathf.Sin(val * (float)Math.PI) * 15f + val * 4f, Mathf.Lerp(start.z, end.z, val));
		}
		else
		{
			m_cosmeticCoinActor.gameObject.SetActive(value: false);
			val = (val - 0.85f) / 0.14999998f;
			go.transform.position = new Vector3(end.x, end.y + Mathf.Lerp(4f, 0f, val), end.z);
		}
	}

	public void AnimateInCosmeticCoin(Actor actor)
	{
		if (m_animData == null)
		{
			Actor actorCopy = actor.Clone();
			actorCopy.SetCardDefFromActor(actor);
			AnimateCoinAssignmentFromPageVisual(actorCopy);
		}
	}

	public override bool PreAnimateContentEntrance()
	{
		CollectionDeck currDeck = CollectionManager.Get().GetEditedDeck();
		if (currDeck == null)
		{
			return true;
		}
		m_currentDeck = currDeck;
		if (m_currentDeck.CosmeticCoinID.HasValue)
		{
			UpdateCosmeticCoin(m_currentDeck.CosmeticCoinID.Value, assigning: false);
		}
		else
		{
			UpdateDatamodel();
			UpdateCosmeticCoinObject();
		}
		return true;
	}

	public override bool AnimateContentEntranceStart()
	{
		if (m_waitingToLoadCoinDef)
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
		m_animating = true;
		iTween.MoveFrom(base.gameObject, iTween.Hash("position", m_originalLocalPosition + m_trayHiddenOffset, "islocal", true, "time", m_traySlideAnimationTime, "easetype", m_traySlideSlideInAnimation, "oncomplete", (Action<object>)delegate
		{
			m_animating = false;
		}));
		return true;
	}

	public override bool AnimateContentEntranceEnd()
	{
		return !m_animating;
	}

	public override bool AnimateContentExitStart()
	{
		SaveRandomCoinSelectionPreference();
		base.transform.localPosition = m_originalLocalPosition;
		m_animating = true;
		iTween.MoveTo(base.gameObject, iTween.Hash("position", m_originalLocalPosition + m_trayHiddenOffset, "islocal", true, "time", m_traySlideAnimationTime, "easetype", m_traySlideSlideOutAnimation, "oncomplete", (Action<object>)delegate
		{
			m_animating = false;
			m_root.SetActive(value: false);
		}));
		return true;
	}

	public override bool AnimateContentExitEnd()
	{
		return !m_animating;
	}
}
