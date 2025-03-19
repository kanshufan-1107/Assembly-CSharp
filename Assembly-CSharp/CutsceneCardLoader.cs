using System;
using UnityEngine;

public class CutsceneCardLoader : MonoBehaviour
{
	[Serializable]
	public class ActorConfig
	{
		public bool ShouldHideAttackObject = true;

		public bool ShouldHideHealthObject = true;

		public bool ShouldHideArmorObject = true;

		public bool ShouldHideManaCostObject = true;
	}

	[SerializeField]
	private Card m_card;

	[Header("Template Prefabs")]
	[SerializeField]
	private GameObject m_heroCardInPlayPrefab;

	[SerializeField]
	private GameObject m_minionCardInPlayPrefab;

	[SerializeField]
	private GameObject m_heroPowerCardInPlayPrefab;

	[SerializeField]
	[Header("Actor Settings")]
	private ActorConfig m_actorConfig;

	private GameObject m_loadedCardTemplate;

	private Player.Side m_cardSide;

	private Actor m_actor;

	private string m_cardId;

	public string GetLoadedCardId
	{
		get
		{
			if (!(m_actor != null))
			{
				return string.Empty;
			}
			return m_cardId;
		}
	}

	public Actor GetActor()
	{
		return m_actor;
	}

	private void OnDestroy()
	{
		Dispose();
	}

	public void Dispose()
	{
		if (m_actor != null)
		{
			m_actor.Destroy();
		}
		m_actor = null;
		m_cardId = string.Empty;
		m_cardSide = Player.Side.NEUTRAL;
		if (m_loadedCardTemplate != null)
		{
			UnityEngine.Object.Destroy(m_loadedCardTemplate);
		}
		m_loadedCardTemplate = null;
	}

	public void SetAndLoadCard(string cardId, CutsceneSceneDef.CardType cardType, Player.Side side = Player.Side.NEUTRAL, Actor heroActor = null)
	{
		m_cardId = cardId;
		m_cardSide = side;
		if (!(m_card == null))
		{
			switch (cardType)
			{
			case CutsceneSceneDef.CardType.HERO:
				LoadHero();
				break;
			case CutsceneSceneDef.CardType.ALTERNATE_FORM:
				LoadHero(isAlternateForm: true);
				break;
			case CutsceneSceneDef.CardType.MINION:
				LoadMinion(heroActor);
				break;
			case CutsceneSceneDef.CardType.HERO_POWER:
				LoadHeroPower(heroActor);
				break;
			default:
				Log.CosmeticPreview.PrintWarning(string.Format("{0} failed to load card due to unhandled card type: {1}", "CutsceneCardLoader", cardType));
				return;
			}
			ApplyActorConfig(m_actor, m_actorConfig);
		}
	}

	public void SetHeroScale(Vector3 scale)
	{
		base.gameObject.transform.localScale = scale;
	}

	private void LoadMinion(Actor heroActor)
	{
		if (heroActor == null)
		{
			Log.CosmeticPreview.PrintError("Failed to create minion as hero Actor was null!");
		}
		else if (TryCreateActorFromCardType(TAG_CARDTYPE.MINION))
		{
			CutsceneCustomEntity entity = new CutsceneCustomEntity();
			entity.Init(m_card, heroActor.GetCard(), m_cardSide);
			ConfigEntityAndSetForActorCard(entity, TAG_CARDTYPE.MINION);
		}
	}

	private void LoadHero(bool isAlternateForm = false)
	{
		if (!TryCreateActorFromCardType(TAG_CARDTYPE.HERO))
		{
			return;
		}
		CutsceneCustomEntity entity = new CutsceneCustomEntity();
		entity.Init(m_card, m_card, m_cardSide);
		ConfigEntityAndSetForActorCard(entity, TAG_CARDTYPE.HERO);
		using (DefLoader.DisposableCardDef cardDef = entity.ShareDisposableCardDef())
		{
			GameObject cardDefObject = cardDef.CardDef?.gameObject;
			if (cardDefObject != null && cardDefObject.TryGetComponent<HeroCardSwitcher>(out var heroCardSwitcher))
			{
				heroCardSwitcher.DoReplacementForCard(m_card, m_cardSide);
			}
			else
			{
				m_card.SetCardDef(cardDef, updateActor: false);
			}
		}
		if (isAlternateForm)
		{
			return;
		}
		CutsceneBoard board = CutsceneBoard.Get();
		if (board != null)
		{
			if (m_cardSide == Player.Side.FRIENDLY)
			{
				board.SetFriendlyHeroActor(m_actor);
			}
			else
			{
				board.SetOpponentHeroActor(m_actor);
			}
			board.UpdateCustomHeroTray(m_cardSide);
		}
		CustomHeroFrameBehaviour frame = GetComponentInChildren<CustomHeroFrameBehaviour>();
		if (frame != null)
		{
			frame.UpdateFrame();
		}
	}

	private void LoadHeroPower(Actor heroActor)
	{
		if (string.IsNullOrEmpty(m_cardId))
		{
			m_cardId = GameUtils.GetHeroPowerCardIdFromHero(heroActor.GetEntity().GetCardId());
		}
		if (TryCreateActorFromCardType(TAG_CARDTYPE.HERO_POWER))
		{
			CutsceneCustomEntity entity = new CutsceneCustomEntity();
			entity.Init(m_card, heroActor.GetCard(), m_cardSide);
			ConfigEntityAndSetForActorCard(entity, TAG_CARDTYPE.HERO_POWER);
			heroActor.GetEntity().GetController().SetHeroPower(entity);
		}
	}

	private bool TryCreateActorFromCardType(TAG_CARDTYPE type)
	{
		GameObject template = null;
		switch (type)
		{
		case TAG_CARDTYPE.HERO:
			template = m_heroCardInPlayPrefab;
			break;
		case TAG_CARDTYPE.MINION:
			template = m_minionCardInPlayPrefab;
			break;
		case TAG_CARDTYPE.HERO_POWER:
			template = m_heroPowerCardInPlayPrefab;
			break;
		default:
			Log.CosmeticPreview.PrintError(string.Format("{0} failed to create actor due to unhandled card type: {1}", "CutsceneCardLoader", type));
			return false;
		}
		if (template == null)
		{
			Log.CosmeticPreview.PrintError(string.Format("{0} failed to create actor due null template for card type: {1}", "CutsceneCardLoader", type));
			return false;
		}
		m_loadedCardTemplate = UnityEngine.Object.Instantiate(template, base.transform, worldPositionStays: false);
		if (m_loadedCardTemplate == null)
		{
			return false;
		}
		m_loadedCardTemplate.transform.position = Vector3.zero;
		m_actor = m_loadedCardTemplate.GetComponent<Actor>();
		return m_actor != null;
	}

	private void ConfigEntityAndSetForActorCard(CutsceneCustomEntity entity, TAG_CARDTYPE type)
	{
		if (GameUtils.TryGetCardTagRecords(m_cardId, out var tagRecords))
		{
			foreach (CardTagDbfRecord tagRec in tagRecords)
			{
				entity.SetTag(tagRec.TagId, tagRec.TagValue);
			}
		}
		entity.SetTag(GAME_TAG.CARDTYPE, type);
		entity.SetTag(GAME_TAG.CUTSCENE_CARD_TYPE, type);
		m_card.SetEntity(entity);
		m_card.SetActor(m_actor);
		entity.SetCard(m_card);
		entity.LoadCard(m_cardId);
		m_actor.SetEntity(entity);
		m_actor.SetCard(m_card);
		m_actor.SetCardDefFromEntity(entity);
	}

	private static void ApplyActorConfig(Actor actor, ActorConfig config)
	{
		if (!(actor == null) && config != null)
		{
			if (actor.m_attackObject != null)
			{
				actor.m_attackObject.SetActive(!config.ShouldHideAttackObject);
			}
			if (actor.m_healthObject != null)
			{
				actor.m_healthObject.SetActive(!config.ShouldHideHealthObject);
			}
			if (actor.m_armorObject != null)
			{
				actor.m_armorObject.SetActive(!config.ShouldHideArmorObject);
			}
			if (actor.m_manaObject != null && actor.m_costTextMesh != null)
			{
				actor.m_costTextMesh.gameObject.SetActive(!config.ShouldHideManaCostObject);
			}
		}
	}
}
