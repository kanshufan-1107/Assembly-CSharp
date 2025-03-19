using System;
using System.Collections;
using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

public class DeckTrayHeroSkinContent : DeckTrayContent
{
	private class AnimatedHeroSkin
	{
		public Actor Actor;

		public GameObject GameObject;

		public Vector3 OriginalScale;

		public Vector3 OriginalPosition;
	}

	[SerializeField]
	private UberText m_currentHeroSkinName;

	[SerializeField]
	[Header("Positioning")]
	private GameObject m_root;

	[SerializeField]
	private Vector3 m_trayHiddenOffset;

	[SerializeField]
	private GameObject m_heroSkinContainer;

	[SerializeField]
	private Vector3 m_missingCardEffectScale;

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

	[SerializeField]
	private WeakAssetReference m_pickUpSound;

	private const string ADD_CARD_TO_DECK_SOUND = "collection_manager_card_add_to_deck_instant.prefab:06df359c4026d7e47b06a4174f33e3ef";

	private Widget m_rootWidget;

	private DeckDataModel m_deckDataModel;

	private string m_currentHeroCardId;

	private CollectionDeck m_currentDeck;

	private Actor m_heroSkinActor;

	private Vector3 m_originalLocalPosition;

	private bool m_animating;

	private bool m_waitingToLoadHeroDef;

	private bool m_shouldUpdateLimitToFavoritesSetting;

	private const string CODE_CHECKBOX_TOGGLED = "CODE_CHECKBOX_TOGGLED";

	private const string CODE_TRAY_CLICKED = "CODE_TRAY_CLICKED";

	private const string CODE_TRAY_DRAG_START = "CODE_TRAY_DRAG_START";

	private AnimatedHeroSkin m_animData;

	public event Action<string> OnHeroChanged;

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
		LoadHeroSkinActor();
	}

	private void WidgetReadyListener(object unused)
	{
		m_deckDataModel = new DeckDataModel();
		m_deckDataModel.RandomHeroFavoritesOnly = true;
		m_rootWidget.BindDataModel(m_deckDataModel);
	}

	private void WidgetEventListener(string eventName)
	{
		switch (eventName)
		{
		case "CODE_CHECKBOX_TOGGLED":
		{
			bool enabled = !m_deckDataModel.RandomHeroFavoritesOnly;
			ToggleFavoritesOnly(enabled);
			break;
		}
		case "CODE_TRAY_CLICKED":
			RemoveHeroSkin();
			break;
		case "CODE_TRAY_DRAG_START":
			GrabHeroSkin();
			break;
		}
	}

	private void ToggleFavoritesOnly(bool enabled)
	{
		m_currentDeck.RandomHeroUseFavorite = enabled;
		UpdateDatamodel();
		this.OnHeroChanged?.Invoke(string.Empty);
		m_shouldUpdateLimitToFavoritesSetting = true;
	}

	public void UpdateHeroSkin(string cardId, TAG_PREMIUM premium, bool assigning, Actor baseActor = null)
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
			if (m_currentDeck.HeroCardID == cardId && m_currentDeck.HeroOverridden)
			{
				ShowSocketFX();
				return;
			}
			m_currentDeck.HeroOverridden = true;
			m_currentDeck.HeroCardID = cardId;
		}
		if (baseActor != null)
		{
			using DefLoader.DisposableCardDef cardDef = baseActor.ShareDisposableCardDef();
			UpdateHeroSkinVisual(baseActor.GetEntityDef(), cardDef, baseActor.GetPremium(), assigning);
		}
		else
		{
			m_waitingToLoadHeroDef = true;
			DefLoader.Get().LoadFullDef(cardId, delegate(string cardID, DefLoader.DisposableFullDef fullDef, object callbackData)
			{
				using (fullDef)
				{
					m_waitingToLoadHeroDef = false;
					UpdateHeroSkinVisual(fullDef.EntityDef, fullDef.DisposableCardDef, premium, assigning);
				}
			});
		}
		UpdateHeroSkinObject();
		UpdateDatamodel();
	}

	private void GrabHeroSkin()
	{
		if (m_heroSkinActor != null && m_currentDeck.HeroOverridden && CollectionInputMgr.Get().GrabHeroSkinFromSlot(m_heroSkinActor))
		{
			m_currentDeck.HeroOverridden = false;
			UpdateDatamodel();
			UpdateHeroSkinObject();
			ToggleSparkleEffects(enabled: true);
			this.OnHeroChanged?.Invoke(string.Empty);
		}
	}

	public void ToggleSparkleEffects(bool enabled)
	{
		if (m_deckDataModel != null)
		{
			m_deckDataModel.DraggingDeckAssignment = enabled;
		}
	}

	private void RemoveHeroSkin()
	{
		if (!(m_heroSkinActor != null) || !m_currentDeck.HeroOverridden)
		{
			return;
		}
		Spell unsocketSpell = m_heroSkinActor.GetSpell(m_removalSpellType);
		m_currentDeck.HeroOverridden = false;
		UpdateDatamodel();
		this.OnHeroChanged?.Invoke(string.Empty);
		if (!string.IsNullOrEmpty(m_unsocketSound.AssetString))
		{
			SoundManager.Get().LoadAndPlay(m_unsocketSound.AssetString, base.gameObject);
		}
		if (unsocketSpell == null)
		{
			UpdateHeroSkinObject();
			return;
		}
		unsocketSpell.AddFinishedCallback(delegate
		{
			UpdateHeroSkinObject();
		});
		unsocketSpell.ActivateState(SpellStateType.BIRTH);
	}

	private void LoadHeroSkinActor()
	{
		string actorPath = ActorNames.GetHeroSkinOrHandActor(TAG_CARDTYPE.HERO, TAG_PREMIUM.NORMAL);
		AssetLoader.Get().InstantiatePrefab(actorPath, delegate(AssetReference assetRef, GameObject go, object callbackData)
		{
			if (go == null)
			{
				Debug.LogWarning($"DeckTrayHeroSkinContent.LoadHeroSkinActor - FAILED to load \"{assetRef}\"");
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
					GameUtils.SetParent(component, m_heroSkinContainer);
					m_heroSkinActor = component;
					CollectionHeroSkin component2 = m_heroSkinActor.GetComponent<CollectionHeroSkin>();
					if (component2 != null)
					{
						component2.ShowName = false;
					}
				}
			}
		}, null, AssetLoadingOptions.IgnorePrefabPosition);
	}

	private void UpdateHeroSkinVisual(EntityDef entityDef, DefLoader.DisposableCardDef cardDef, TAG_PREMIUM premium, bool assigning)
	{
		if (m_heroSkinActor == null)
		{
			Debug.LogError("Hero skin object not loaded yet! Cannot set portrait!");
			return;
		}
		m_heroSkinActor.SetEntityDef(entityDef);
		m_heroSkinActor.SetCardDef(cardDef);
		m_heroSkinActor.SetPremium(premium);
		GameObject heroSkinRoot = m_heroSkinActor.GetRootObject();
		if (heroSkinRoot != null && !heroSkinRoot.activeSelf)
		{
			heroSkinRoot.SetActive(value: true);
		}
		m_heroSkinActor.UpdateAllComponents();
		CollectionHeroSkin heroSkin = m_heroSkinActor.GetComponent<CollectionHeroSkin>();
		if (heroSkin != null)
		{
			heroSkin.SetClass(entityDef);
		}
		if (assigning)
		{
			this.OnHeroChanged?.Invoke(entityDef.GetCardId());
		}
		if (assigning && cardDef?.CardDef != null)
		{
			GameUtils.LoadCardDefEmoteSound(cardDef.CardDef.m_EmoteDefs, EmoteType.PICKED, OnPickEmoteLoaded);
		}
		if (m_currentHeroSkinName != null)
		{
			m_currentHeroSkinName.Text = entityDef.GetName();
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
		CollectionHeroSkin heroSkin = m_heroSkinActor.GetComponent<CollectionHeroSkin>();
		if (heroSkin != null)
		{
			heroSkin.ShowSocketFX();
		}
	}

	private void UpdateHeroSkinObject()
	{
		Spell unsocketSpell = m_heroSkinActor.GetSpell(m_removalSpellType);
		SpellManager.Get().ReleaseSpell(unsocketSpell, reset: true);
		m_heroSkinActor.UpdateAllComponents();
		m_heroSkinActor.gameObject.transform.position = m_heroSkinContainer.transform.position;
		m_heroSkinActor.gameObject.SetActive(m_currentDeck.HeroOverridden);
	}

	private void UpdateDatamodel()
	{
		m_deckDataModel.HeroOverride = m_currentDeck.HeroOverridden;
		m_deckDataModel.RandomHeroFavoritesOnly = m_currentDeck.RandomHeroUseFavorite;
		if (m_currentDeck.HeroOverridden)
		{
			int heroId = GameUtils.TranslateCardIdToDbId(m_currentDeck.HeroCardID);
			CardDbfRecord hero = GameDbf.Card.GetRecord(heroId);
			if (m_deckDataModel.Hero == null)
			{
				m_deckDataModel.Hero = new HeroDataModel();
			}
			m_deckDataModel.Hero.CardID = heroId;
			m_deckDataModel.Hero.Name = hero.Name;
		}
		else
		{
			m_deckDataModel.Hero = null;
		}
	}

	private void SaveRandomHeroSelectionPreference()
	{
		if (m_shouldUpdateLimitToFavoritesSetting)
		{
			bool num = GameUtils.IsGSDFlagSet(GameSaveKeyId.COLLECTION_MANAGER, GameSaveKeySubkeyId.COLLECTION_MANAGER_RANDOM_HERO_USE_ALL_OWNED);
			bool deckIsUsingRandomOwned = !m_deckDataModel.RandomHeroFavoritesOnly;
			if (num != deckIsUsingRandomOwned)
			{
				GameUtils.SetGSDFlag(GameSaveKeyId.COLLECTION_MANAGER, GameSaveKeySubkeyId.COLLECTION_MANAGER_RANDOM_HERO_USE_ALL_OWNED, deckIsUsingRandomOwned);
			}
			m_shouldUpdateLimitToFavoritesSetting = false;
		}
	}

	public void AnimateHeroAssignmentFromPageVisual(Actor actor)
	{
		GameObject go = actor.gameObject;
		AnimatedHeroSkin animData = new AnimatedHeroSkin();
		animData.Actor = actor;
		animData.GameObject = go;
		animData.OriginalScale = go.transform.localScale;
		animData.OriginalPosition = go.transform.position;
		m_animData = animData;
		go.transform.position = new Vector3(go.transform.position.x, go.transform.position.y + 0.5f, go.transform.position.z);
		go.transform.localScale = m_heroSkinContainer.transform.lossyScale;
		Hashtable args = iTweenManager.Get().GetTweenHashTable();
		args.Add("from", 0f);
		args.Add("to", 1f);
		args.Add("time", 0.6f);
		args.Add("easetype", iTween.EaseType.easeOutCubic);
		args.Add("onupdate", "AnimateNewHeroSkinUpdate");
		args.Add("onupdatetarget", base.gameObject);
		args.Add("oncomplete", "AnimateNewHeroSkinFinished");
		args.Add("oncompleteparams", animData);
		args.Add("oncompletetarget", base.gameObject);
		iTween.ValueTo(go, args);
		CollectionHeroSkin heroSkin = actor.gameObject.GetComponent<CollectionHeroSkin>();
		if (heroSkin != null)
		{
			heroSkin.ShowSocketFX();
		}
		SoundManager.Get().LoadAndPlay(m_addSound.AssetString);
	}

	private void AnimateNewHeroSkinFinished()
	{
		m_heroSkinActor.gameObject.SetActive(value: true);
		Actor actor = m_animData.Actor;
		UpdateHeroSkin(actor.GetEntityDef().GetCardId(), actor.GetPremium(), assigning: true, actor);
		UnityEngine.Object.Destroy(m_animData.GameObject);
		m_animData = null;
	}

	private void AnimateNewHeroSkinUpdate(float val)
	{
		GameObject go = m_animData.GameObject;
		Vector3 start = m_animData.OriginalPosition;
		Vector3 end = m_heroSkinContainer.transform.position;
		if (val <= 0.85f)
		{
			val /= 0.85f;
			go.transform.position = new Vector3(Mathf.Lerp(start.x, end.x, val), Mathf.Lerp(start.y, end.y, val) + Mathf.Sin(val * (float)Math.PI) * 15f + val * 4f, Mathf.Lerp(start.z, end.z, val));
		}
		else
		{
			m_heroSkinActor.gameObject.SetActive(value: false);
			val = (val - 0.85f) / 0.14999998f;
			go.transform.position = new Vector3(end.x, end.y + Mathf.Lerp(4f, 0f, val), end.z);
		}
	}

	public void AnimateInHeroSkin(Actor actor)
	{
		if (m_animData == null)
		{
			Actor actorCopy = actor.Clone();
			actorCopy.SetCardDefFromActor(actor);
			CollectionHeroSkin heroSkin = actorCopy.GetComponent<CollectionHeroSkin>();
			if (heroSkin != null)
			{
				heroSkin.ShowFavoriteBanner(show: false);
				heroSkin.ShowName = false;
			}
			AnimateHeroAssignmentFromPageVisual(actorCopy);
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
		TAG_PREMIUM heroPremium = CollectionManager.Get().GetHeroPremium(currDeck.GetClass());
		UpdateHeroSkin(currDeck.HeroCardID, heroPremium, assigning: false);
		return true;
	}

	public override bool AnimateContentEntranceStart()
	{
		if (m_waitingToLoadHeroDef)
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
		SaveRandomHeroSelectionPreference();
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
