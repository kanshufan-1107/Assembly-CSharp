using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckBigCard : MonoBehaviour
{
	public delegate void OnBigCardShownHandler(Actor shownActor, EntityDef entityDef);

	public GameObject m_topPosition;

	public GameObject m_bottomPosition;

	public Material m_missingCardMaterial;

	public Material m_ghostCardMaterial;

	public Material m_invalidCardMaterial;

	public bool m_disableCollidersOnHeroPower;

	public bool m_flipHeroPowerHorizontalPosition;

	public bool m_showTooltipsForAdventure;

	public UberText m_createdByText;

	public Vector3 m_positionOffsetWithCreatorBanner;

	public float m_scaleMultiplierWithCreatorBanner = 1f;

	private HandActorCache m_actorCache = new HandActorCache();

	private bool m_actorCacheInit;

	private bool m_hideBigHeroPower;

	private bool m_shown;

	private EntityDef m_entityDef;

	private TAG_PREMIUM m_premium;

	private DefLoader.DisposableCardDef m_cardDef;

	private Actor m_shownActor;

	private Actor m_shownHeroPowerActor;

	private GhostCard.Type m_ghosted;

	private int m_firstShowFrame;

	private Vector3 m_defaultLocalPosition;

	private Vector3 m_defaultLocalScale;

	private static readonly PlatformDependentValue<Vector3> HERO_POWER_START_POSITION = new PlatformDependentValue<Vector3>(PlatformCategory.Screen)
	{
		PC = new Vector3(0f, -0.5f, 0f)
	};

	private static readonly PlatformDependentValue<Vector3> HERO_POWER_SIGNATURE_START_POSITION = new PlatformDependentValue<Vector3>(PlatformCategory.Screen)
	{
		PC = new Vector3(0f, -0.5f, 0f)
	};

	private static readonly PlatformDependentValue<Vector3> HERO_POWER_START_SCALE = new PlatformDependentValue<Vector3>(PlatformCategory.Screen)
	{
		PC = new Vector3(0f, 0f, 0f)
	};

	private static readonly PlatformDependentValue<Vector3> HERO_POWER_POSITION = new PlatformDependentValue<Vector3>(PlatformCategory.Screen)
	{
		PC = new Vector3(-2.11f, 0f, -0.12f),
		Phone = new Vector3(-2.05f, 0f, -0.12f)
	};

	private static readonly PlatformDependentValue<Vector3> SIGNATURE_HERO_POWER_POSITION = new PlatformDependentValue<Vector3>(PlatformCategory.Screen)
	{
		PC = new Vector3(-3.672f, 0f, -0.12f),
		Phone = new Vector3(-1.817f, 0f, -0.674f)
	};

	private static readonly PlatformDependentValue<Vector3> HERO_POWER_SCALE = new PlatformDependentValue<Vector3>(PlatformCategory.Screen)
	{
		PC = new Vector3(0.8117157f, 0.8117157f, 0.8117157f),
		Phone = new Vector3(0.8117157f, 0.8117157f, 0.8117157f)
	};

	private static readonly PlatformDependentValue<Vector3> SIGNATURE_HERO_POWER_SCALE = new PlatformDependentValue<Vector3>(PlatformCategory.Screen)
	{
		PC = new Vector3(0.8117157f, 0.8117157f, 0.8117157f),
		Phone = new Vector3(0.6f, 0.6f, 0.6f)
	};

	private static readonly float HERO_POWER_TWEEN_TIME = 0.5f;

	private static readonly int GHOST_CARD_RENDER_QUEUE = 72;

	public event OnBigCardShownHandler OnBigCardShown;

	private void Awake()
	{
		m_firstShowFrame = 0;
		m_defaultLocalScale = base.transform.localScale;
		m_defaultLocalPosition = base.transform.localPosition;
	}

	private void OnDestroy()
	{
		m_cardDef?.Dispose();
		m_cardDef = null;
	}

	public void Show(EntityDef entityDef, TAG_PREMIUM premium, DefLoader.DisposableCardDef cardDef, Vector3 sourcePosition, GhostCard.Type ghosted, float delay = 0f)
	{
		if (false)
		{
			int frame = Time.frameCount;
			if (m_firstShowFrame == 0)
			{
				m_firstShowFrame = frame;
			}
			else if (frame - m_firstShowFrame <= 1)
			{
				return;
			}
		}
		StopCoroutine("ShowWithDelayInternal");
		m_shown = true;
		m_entityDef = entityDef;
		m_premium = premium;
		m_cardDef?.Dispose();
		m_cardDef = cardDef?.Share();
		m_ghosted = ghosted;
		if (delay > 0f)
		{
			KeyValuePair<float, Action> args = new KeyValuePair<float, Action>(delay, delegate
			{
				Show(entityDef, premium, cardDef, sourcePosition, ghosted);
			});
			StartCoroutine("ShowWithDelayInternal", args);
			return;
		}
		if (!UniversalInputManager.UsePhoneUI)
		{
			float zMin = m_bottomPosition.transform.position.z;
			float zMax = m_topPosition.transform.position.z;
			float z = Mathf.Clamp(sourcePosition.z, zMin, zMax);
			TransformUtil.SetPosZ(base.transform, z);
			m_defaultLocalPosition = base.transform.localPosition;
		}
		if (!m_actorCacheInit)
		{
			m_actorCacheInit = true;
			m_actorCache.AddActorLoadedListener(OnActorLoaded);
			m_actorCache.Initialize();
		}
		if (!m_actorCache.IsInitializing())
		{
			Show(sourcePosition.z);
		}
	}

	public void Hide(EntityDef entityDef, TAG_PREMIUM premium)
	{
		if (m_entityDef == entityDef && m_premium == premium)
		{
			Hide();
		}
	}

	public void ForceHide()
	{
		Hide();
	}

	public void SetCreatorName(string creatorName)
	{
		if (!(m_createdByText == null))
		{
			if (string.IsNullOrEmpty(creatorName))
			{
				m_createdByText.Text = string.Empty;
				base.transform.localPosition = m_defaultLocalPosition;
				base.transform.localScale = m_defaultLocalScale;
			}
			else
			{
				m_createdByText.Text = GameStrings.Format("GAMEPLAY_HISTORY_CREATED_BY", creatorName);
				base.transform.localPosition = m_defaultLocalPosition + m_positionOffsetWithCreatorBanner;
				base.transform.localScale = m_defaultLocalScale * m_scaleMultiplierWithCreatorBanner;
			}
		}
	}

	public void OffsetByVector(Vector3 offset)
	{
		m_defaultLocalPosition += offset;
		base.transform.localPosition = m_defaultLocalPosition;
	}

	public void SetHideBigHeroPower(bool hide)
	{
		m_hideBigHeroPower = hide;
	}

	private IEnumerator ShowWithDelayInternal(KeyValuePair<float, Action> args)
	{
		yield return new WaitForSeconds(args.Key);
		args.Value();
	}

	private void OnActorLoaded(string assetName, Actor actor, object callbackData)
	{
		if (actor == null)
		{
			Debug.LogWarning($"DeckBigCard.OnActorLoaded() - FAILED to load {assetName}");
			return;
		}
		actor.TurnOffCollider();
		actor.Hide();
		actor.transform.parent = base.transform;
		TransformUtil.Identity(actor.transform);
		if (base.gameObject != null)
		{
			LayerUtils.SetLayer(actor, base.gameObject.layer);
		}
		if (m_actorCache != null && !m_actorCache.IsInitializing() && m_shown)
		{
			Show();
		}
	}

	private void Show(float sourceZ = 0f)
	{
		m_shownActor = m_actorCache.GetActor(m_entityDef, m_premium);
		if (m_shownActor == null)
		{
			return;
		}
		m_shownActor.SetEntityDef(m_entityDef);
		m_shownActor.SetPremium(m_premium);
		if (m_cardDef == null)
		{
			m_cardDef = DefLoader.Get().GetCardDef(m_entityDef.GetCardId(), new CardPortraitQuality(3, m_premium));
		}
		m_shownActor.SetCardDef(m_cardDef);
		m_shownActor.GhostCardEffect(m_ghosted, m_premium);
		if (m_shownActor.isGhostCard())
		{
			GhostCard component = m_shownActor.m_ghostCardGameObject.GetComponent<GhostCard>();
			component.SetRenderQueue(GHOST_CARD_RENDER_QUEUE);
			component.SetBigCard(isBigCard: true);
		}
		bool isShowingBigHeroPower = m_shownActor.GetEntityDef().IsHero() && !m_hideBigHeroPower;
		TooltipPanelManager.Orientation orientation = ((isShowingBigHeroPower && (bool)UniversalInputManager.UsePhoneUI) ? TooltipPanelManager.Orientation.LeftTop : TooltipPanelManager.Orientation.LeftMiddle);
		TooltipPanelManager.Get().UpdateTooltipForBigCard(m_entityDef, m_shownActor, orientation);
		if (m_showTooltipsForAdventure && !m_shownActor.GetEntityDef().IsHero())
		{
			TooltipPanelManager.Get().UpdateKeywordHelpForAdventure(m_shownActor.GetEntityDef(), m_shownActor);
		}
		m_shownActor.CreateBannedRibbon();
		m_shownActor.UpdateAllComponents();
		if (m_missingCardMaterial != null)
		{
			m_shownActor.SetMissingCardMaterial(m_missingCardMaterial);
		}
		m_shownActor.Show();
		CollectibleDisplay collectionManagerDisplay = CollectionManager.Get().GetCollectibleDisplay();
		bool inTemplateScreen = collectionManagerDisplay != null && collectionManagerDisplay.GetViewMode() == CollectionUtils.ViewMode.DECK_TEMPLATE;
		CollectionDeck deck = CollectionManager.Get().GetEditedDeck();
		if (deck != null && deck.Locked)
		{
			return;
		}
		if (m_ghosted != 0)
		{
			TooltipPanelManager.Orientation orientation2 = TooltipPanelManager.Orientation.LeftMiddle;
			if ((bool)UniversalInputManager.UsePhoneUI && inTemplateScreen)
			{
				orientation2 = ((sourceZ > 0f) ? TooltipPanelManager.Orientation.RightBottom : TooltipPanelManager.Orientation.RightTop);
			}
			TooltipPanelManager.Get().UpdateGhostCardHelpForCollectionManager(m_shownActor, m_ghosted, orientation2);
		}
		if (isShowingBigHeroPower)
		{
			string heroPowerId = GameUtils.GetHeroPowerCardIdFromHero(m_shownActor.GetEntityDef().GetCardId());
			ShowHeroPowerCard(heroPowerId, m_shownActor.GetPremium());
		}
		if (m_createdByText != null && !string.IsNullOrEmpty(m_createdByText.Text))
		{
			m_createdByText.gameObject.SetActive(value: true);
		}
		if (this.OnBigCardShown != null)
		{
			this.OnBigCardShown(m_shownActor, m_entityDef);
		}
	}

	private void Hide()
	{
		StopCoroutine("ShowWithDelayInternal");
		if (m_showTooltipsForAdventure)
		{
			TooltipPanelManager.Get().HideKeywordHelp();
		}
		if (m_createdByText != null)
		{
			m_createdByText.gameObject.SetActive(value: false);
		}
		m_shown = false;
		if (!(m_shownActor == null))
		{
			m_shownActor.Hide();
			m_shownActor = null;
			if (!m_hideBigHeroPower)
			{
				HideHeroPowerCard();
			}
			TooltipPanelManager.Get().HideTooltipPanels();
		}
	}

	private void ShowHeroPowerCard(string heroPowerCardId, TAG_PREMIUM premium)
	{
		AssetLoader.Get().InstantiatePrefab(ActorNames.GetNameWithPremiumType(ActorNames.ACTOR_ASSET.HISTORY_HERO_POWER, premium), delegate(AssetReference actorName, GameObject actorGameObject, object data)
		{
			OnHeroPowerActorLoaded(actorName, actorGameObject, heroPowerCardId, premium);
		}, heroPowerCardId, AssetLoadingOptions.IgnorePrefabPosition);
	}

	private void OnHeroPowerActorLoaded(AssetReference assetRef, GameObject actorGameObject, string heroPowerCardId, TAG_PREMIUM premium)
	{
		if (actorGameObject == null)
		{
			Debug.LogError($"CollectionDeckTray.OnDeckBigHeroPowerActorLoaded: Unable to load actor for hero power: {assetRef}");
			return;
		}
		if (m_shownHeroPowerActor != null)
		{
			UnityEngine.Object.Destroy(m_shownHeroPowerActor.gameObject);
		}
		m_shownHeroPowerActor = actorGameObject.GetComponent<Actor>();
		if (!m_shown)
		{
			HideHeroPowerCard();
			return;
		}
		m_shownHeroPowerActor.Show();
		if (m_disableCollidersOnHeroPower)
		{
			m_shownHeroPowerActor.TurnOffCollider();
		}
		using DefLoader.DisposableFullDef heroPowerDef = DefLoader.Get().GetFullDef(heroPowerCardId, m_shownHeroPowerActor.CardPortraitQuality);
		m_shownHeroPowerActor.SetCardDef(heroPowerDef.DisposableCardDef);
		m_shownHeroPowerActor.SetEntityDef(heroPowerDef.EntityDef);
		TAG_PREMIUM heroPowerPremium = ((premium == TAG_PREMIUM.SIGNATURE) ? TAG_PREMIUM.GOLDEN : premium);
		m_shownHeroPowerActor.SetPremium(heroPowerPremium);
		m_shownHeroPowerActor.GhostCardEffect(m_ghosted, heroPowerPremium);
		if (m_shownActor.isGhostCard())
		{
			m_shownHeroPowerActor.m_ghostCardGameObject.GetComponent<GhostCard>().SetRenderQueue(GHOST_CARD_RENDER_QUEUE);
		}
		m_shownHeroPowerActor.SetUnlit();
		m_shownHeroPowerActor.UpdateAllComponents();
		m_shownHeroPowerActor.gameObject.transform.parent = base.transform;
		m_shownHeroPowerActor.transform.localPosition = ((premium == TAG_PREMIUM.SIGNATURE) ? HERO_POWER_SIGNATURE_START_POSITION : HERO_POWER_START_POSITION);
		m_shownHeroPowerActor.transform.localScale = HERO_POWER_START_SCALE;
		Vector3 finalPosition = ((premium == TAG_PREMIUM.SIGNATURE) ? SIGNATURE_HERO_POWER_POSITION : HERO_POWER_POSITION);
		Vector3 finalScale = ((premium == TAG_PREMIUM.SIGNATURE) ? SIGNATURE_HERO_POWER_SCALE : HERO_POWER_SCALE);
		if (m_flipHeroPowerHorizontalPosition)
		{
			finalPosition.x = 0f - finalPosition.x;
		}
		iTween.MoveTo(m_shownHeroPowerActor.gameObject, iTween.Hash("position", finalPosition, "islocal", true, "time", HERO_POWER_TWEEN_TIME));
		iTween.ScaleTo(m_shownHeroPowerActor.gameObject, iTween.Hash("scale", finalScale, "islocal", true, "time", HERO_POWER_TWEEN_TIME));
	}

	private void HideHeroPowerCard()
	{
		if (m_shownHeroPowerActor != null)
		{
			UnityEngine.Object.Destroy(m_shownHeroPowerActor.gameObject);
		}
	}
}
