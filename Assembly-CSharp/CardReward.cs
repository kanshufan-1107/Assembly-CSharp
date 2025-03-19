using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets;
using Blizzard.T5.Core;
using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

public class CardReward : Reward
{
	[Serializable]
	public class TransformOverrideSet
	{
		[Serializable]
		public struct TransformOverride
		{
			public enum TransformType
			{
				Position,
				Rotation,
				Scale
			}

			public Transform Transform;

			public TransformType Type;

			public Vector3 Value;
		}

		public TAG_PREMIUM PremiumLevel;

		public int FrameId = -1;

		public List<TransformOverride> TransformOverrides = new List<TransformOverride>();
	}

	public GameObject m_nonHeroCardsRoot;

	public GameObject m_heroCardRoot;

	public GameObject m_cardParent;

	public GameObject m_duplicateCardParent;

	public CardRewardCount m_cardCount;

	public GameObject m_godRays;

	public bool m_showCardCount = true;

	public bool m_RotateIn = true;

	public GameObject m_classUnlockScroll;

	public UberText m_classUnlockScrollTitle;

	public UberText m_classUnlockScrollDescription;

	public Transform m_classUnlockScrollHeroBone;

	public Widget m_classUnlockScrollDeckWidget;

	public float m_classUnlockScrollRaysScale;

	public float m_classUnlockScrollRaysOffset;

	public ShopMultiClassIcons m_shopMultiClassIcons;

	public List<TransformOverrideSet> m_transformOverrideSets = new List<TransformOverrideSet>();

	private static readonly Map<TAG_CARDTYPE, Vector3> CARD_SCALE = new Map<TAG_CARDTYPE, Vector3>
	{
		{
			TAG_CARDTYPE.SPELL,
			new Vector3(1f, 1f, 1f)
		},
		{
			TAG_CARDTYPE.MINION,
			new Vector3(1f, 1f, 1f)
		},
		{
			TAG_CARDTYPE.WEAPON,
			new Vector3(1f, 0.5f, 1f)
		},
		{
			TAG_CARDTYPE.HERO,
			new Vector3(1f, 1f, 1f)
		},
		{
			TAG_CARDTYPE.LOCATION,
			new Vector3(1f, 1f, 1f)
		}
	};

	private List<Actor> m_actors = new List<Actor>();

	private GameObject m_goToRotate;

	private CardSoundSpell m_emote;

	private bool m_isHeroUnlock;

	private PopupRoot m_popupRoot;

	public void MakeActorsUnlit()
	{
		foreach (Actor actor in m_actors)
		{
			actor.SetUnlit();
		}
	}

	public void LayerPopupAboveBlur()
	{
		if (!(m_popupRoot != null))
		{
			m_popupRoot = UIContext.GetRoot().ShowPopup(base.gameObject);
		}
	}

	public void DismissPopupFromBlur()
	{
		if (!(m_popupRoot == null))
		{
			UIContext.GetRoot().DismissPopup(base.gameObject);
			m_popupRoot = null;
		}
	}

	protected override void InitData()
	{
		SetData(new CardRewardData(), updateVisuals: false);
	}

	protected override void OnDataSet(bool updateVisuals)
	{
		if (!updateVisuals)
		{
			return;
		}
		if (!(base.Data is CardRewardData cardRewardData))
		{
			Debug.LogWarning($"CardReward.OnDataSet() - data {base.Data} is not CardRewardData");
			return;
		}
		if (string.IsNullOrEmpty(cardRewardData.CardID))
		{
			Debug.LogWarning($"CardReward.OnDataSet() - data {cardRewardData} has invalid cardID");
			return;
		}
		SetReady(ready: false);
		EntityDef entityDef = DefLoader.Get().GetEntityDef(cardRewardData.CardID);
		if (entityDef.IsHeroSkin())
		{
			string assetRef;
			if (cardRewardData.Premium == TAG_PREMIUM.GOLDEN)
			{
				assetRef = "Card_Play_Hero.prefab:42cbbd2c4969afb46b3887bb628de19d";
				SetUpGoldenHeroAchieves();
				return;
			}
			CardHero.HeroType? heroType = GameUtils.GetHeroType(cardRewardData);
			if (heroType == CardHero.HeroType.VANILLA)
			{
				m_isHeroUnlock = true;
				assetRef = "Card_Play_Hero.prefab:42cbbd2c4969afb46b3887bb628de19d";
				SetClassRewardScroll();
				AssetLoader.Get().InstantiatePrefab(assetRef, OnVanillaHeroActorLoaded, entityDef, AssetLoadingOptions.IgnorePrefabPosition);
				m_cardCount.Hide();
				return;
			}
			switch (heroType)
			{
			case CardHero.HeroType.BATTLEGROUNDS_HERO:
				assetRef = "Card_Play_Bacon_Hero.prefab:227eb40f91281fa429c48c8a730c982f";
				SetBattlegroundsHeroRewardText();
				break;
			case CardHero.HeroType.BATTLEGROUNDS_GUIDE:
				assetRef = "Card_Play_Bacon_Guide.prefab:6cf6c56b1ef6f4c4db7210533b95f4ac";
				SetBattlegroundsGuideRewardText();
				break;
			default:
				assetRef = "Card_Play_Hero.prefab:42cbbd2c4969afb46b3887bb628de19d";
				break;
			}
			AssetLoader.Get().InstantiatePrefab(assetRef, OnHeroActorLoaded, entityDef, AssetLoadingOptions.IgnorePrefabPosition);
			m_goToRotate = m_heroCardRoot;
			m_cardCount.Hide();
		}
		else
		{
			if ((bool)UniversalInputManager.UsePhoneUI || !m_showCardCount)
			{
				m_cardCount.Hide();
			}
			if (cardRewardData.OriginData == 26)
			{
				SetSpecificCardRewardText(entityDef);
			}
			else
			{
				SetGenericRandomCardRewardText(entityDef);
			}
			AssetLoader.Get().InstantiatePrefab(ActorNames.GetHandActor(entityDef, cardRewardData.Premium), OnActorLoaded, entityDef, AssetLoadingOptions.IgnorePrefabPosition);
			m_goToRotate = m_nonHeroCardsRoot;
		}
	}

	protected override void ShowReward(bool updateCacheValues)
	{
		CardRewardData cardRewardData = base.Data as CardRewardData;
		InitRewardText();
		EntityDef cardDef = DefLoader.Get().GetEntityDef(cardRewardData.CardID);
		if (((cardRewardData.FixedReward != null && cardRewardData.FixedReward.UseQuestToast) || !GameUtils.IsVanillaHero(cardRewardData.CardID)) && cardDef.IsHeroSkin() && m_rewardBanner != null)
		{
			m_rewardBanner.gameObject.SetActive(value: false);
		}
		TAG_PREMIUM premium = cardRewardData.Premium;
		int frameId = ActorNames.GetSignatureFrameId(cardDef.GetCardId());
		TransformOverrideSet overrideSet = m_transformOverrideSets.FirstOrDefault(delegate(TransformOverrideSet o)
		{
			if (o.PremiumLevel != premium)
			{
				return false;
			}
			return o.PremiumLevel != TAG_PREMIUM.SIGNATURE || o.FrameId == frameId;
		});
		if (overrideSet != null)
		{
			foreach (TransformOverrideSet.TransformOverride transformOverride in overrideSet.TransformOverrides)
			{
				SetTransformValue(transformOverride.Transform, transformOverride.Type, transformOverride.Value);
			}
		}
		if (!m_showCardCount || cardDef.GetRarity() == TAG_RARITY.LEGENDARY)
		{
			m_cardCount.Hide();
		}
		if (m_isHeroUnlock)
		{
			LayerPopupAboveBlur();
		}
		m_root.SetActive(value: true);
		if (m_RotateIn && m_goToRotate != null)
		{
			m_goToRotate.transform.localEulerAngles = new Vector3(0f, 0f, 180f);
			Hashtable args = iTweenManager.Get().GetTweenHashTable();
			args.Add("amount", new Vector3(0f, 0f, 540f));
			args.Add("time", 1.5f);
			args.Add("easetype", iTween.EaseType.easeOutElastic);
			args.Add("space", Space.Self);
			iTween.RotateAdd(m_goToRotate.gameObject, args);
		}
		SoundManager.Get().LoadAndPlay("game_end_reward.prefab:6c28275a79f151a478d49afc04533e72");
		PlayHeroEmote();
	}

	protected override void HideReward()
	{
		DismissPopupFromBlur();
		base.HideReward();
		m_root.SetActive(value: false);
	}

	protected override void OnDestroy()
	{
		DismissPopupFromBlur();
		base.OnDestroy();
	}

	private void OnFullDefLoaded(string cardID, DefLoader.DisposableFullDef fullDef, object callbackData)
	{
		using (fullDef)
		{
			if (fullDef == null)
			{
				Debug.LogWarning($"CardReward.OnFullDefLoaded() - fullDef for CardID {cardID} is null");
				return;
			}
			if (fullDef.EntityDef == null)
			{
				Debug.LogWarning($"CardReward.OnFullDefLoaded() - entityDef for CardID {cardID} is null");
				return;
			}
			if (fullDef.CardDef == null)
			{
				Debug.LogWarning($"CardReward.OnFullDefLoaded() - cardDef for CardID {cardID} is null");
				return;
			}
			foreach (Actor actor in m_actors)
			{
				FinishSettingUpActor(actor, fullDef.DisposableCardDef);
			}
			foreach (EmoteEntryDef emoteDef in fullDef.CardDef.m_EmoteDefs)
			{
				if (emoteDef.m_emoteType == EmoteType.START)
				{
					AssetLoader.Get().InstantiatePrefab(emoteDef.m_emoteSoundSpellPath, OnStartEmoteLoaded);
				}
			}
			SetReady(ready: true);
		}
	}

	private void OnStartEmoteLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		if (!(go == null))
		{
			CardSoundSpell spell = go.GetComponent<CardSoundSpell>();
			if (!(spell == null))
			{
				m_emote = spell;
			}
		}
	}

	private void PlayHeroEmote()
	{
		if (!(m_emote == null))
		{
			m_emote.Reactivate();
		}
	}

	private void OnVanillaHeroActorLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		EntityDef entityDef = (EntityDef)callbackData;
		Actor actor = go.GetComponent<Actor>();
		actor.SetEntityDef(entityDef);
		actor.transform.parent = m_classUnlockScrollHeroBone.transform;
		actor.transform.localScale = Vector3.one;
		actor.transform.localPosition = Vector3.zero;
		actor.transform.localRotation = Quaternion.identity;
		actor.TurnOffCollider();
		if ((bool)actor.m_healthObject)
		{
			actor.m_healthObject.SetActive(value: false);
		}
		if (m_rewardBannerBone != null)
		{
			m_rewardBannerBone.SetActive(value: false);
		}
		if (m_godRays != null)
		{
			m_godRays.transform.localScale = Vector3.one * m_classUnlockScrollRaysScale;
			m_godRays.transform.localPosition = new Vector3(m_godRays.transform.localPosition.x, m_godRays.transform.localPosition.y, m_classUnlockScrollRaysOffset);
		}
		LayerUtils.SetLayer(actor.gameObject, GameLayer.IgnoreFullScreenEffects);
		m_actors.Add(actor);
		DefLoader.Get().LoadFullDef(entityDef.GetCardId(), OnFullDefLoaded, new CardPortraitQuality(3, TAG_PREMIUM.GOLDEN));
	}

	private void OnHeroActorLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		EntityDef entityDef = (EntityDef)callbackData;
		Actor actor = go.GetComponent<Actor>();
		actor.SetEntityDef(entityDef);
		actor.transform.parent = m_heroCardRoot.transform;
		actor.transform.localScale = Vector3.one;
		actor.transform.localPosition = Vector3.zero;
		actor.transform.localRotation = Quaternion.identity;
		actor.TurnOffCollider();
		if ((bool)actor.m_healthObject)
		{
			actor.m_healthObject.SetActive(value: false);
		}
		CardRewardData cardRewardData = base.Data as CardRewardData;
		if ((cardRewardData.FixedReward != null && cardRewardData.FixedReward.UseQuestToast) || !GameUtils.IsVanillaHero(cardRewardData.CardID))
		{
			PlatformDependentValue<Vector3> rewardScale = new PlatformDependentValue<Vector3>(PlatformCategory.Screen)
			{
				PC = new Vector3(1.35f, 1.35f, 1.35f),
				Phone = new Vector3(1.3f, 1.3f, 1.3f)
			};
			PlatformDependentValue<Vector3> rewardPos = new PlatformDependentValue<Vector3>(PlatformCategory.Screen)
			{
				PC = new Vector3(0f, 0f, -0.2f),
				Phone = new Vector3(0f, 0f, -0.3f)
			};
			if (actor.GetCustomFrameRequiresMetaCalibration())
			{
				rewardScale.PC = actor.GetCustomFrameMetaRewardCalibrationScalePC();
				rewardScale.Phone = actor.GetCustomFrameMetaRewardCalibrationScalePhone();
			}
			actor.transform.localScale = rewardScale;
			actor.transform.localPosition = rewardPos;
		}
		SetClassIconPosition(actor);
		StartCoroutine(SetMultiClassIcons());
		LayerUtils.SetLayer(actor.gameObject, GameLayer.IgnoreFullScreenEffects);
		m_actors.Add(actor);
		DefLoader.Get().LoadFullDef(entityDef.GetCardId(), OnFullDefLoaded, new CardPortraitQuality(3, TAG_PREMIUM.GOLDEN));
	}

	private void OnActorLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		EntityDef entityDef = (EntityDef)callbackData;
		if (go == null)
		{
			Log.MissingAssets.PrintWarning("CardReward.OnActorLoaded - Failed to load actor {0}", assetRef);
			return;
		}
		Actor actor = go.GetComponent<Actor>();
		if (actor == null)
		{
			Log.MissingAssets.PrintWarning("CardReward.OnActorLoaded - No actor found in {0}", assetRef);
			return;
		}
		StartSettingUpNonHeroActor(actor, entityDef, m_cardParent.transform);
		CardRewardData cardRewardData = base.Data as CardRewardData;
		m_cardCount.SetCount(cardRewardData.Count);
		if (cardRewardData.Count > 1)
		{
			Actor duplicateActor = UnityEngine.Object.Instantiate(actor);
			StartSettingUpNonHeroActor(duplicateActor, entityDef, m_duplicateCardParent.transform);
		}
		DefLoader.Get().LoadFullDef(entityDef.GetCardId(), OnFullDefLoaded, entityDef, new CardPortraitQuality(3, TAG_PREMIUM.GOLDEN));
	}

	private void StartSettingUpNonHeroActor(Actor actor, EntityDef entityDef, Transform parentTransform)
	{
		actor.SetEntityDef(entityDef);
		actor.transform.parent = parentTransform;
		TAG_CARDTYPE cardType = entityDef.GetCardType();
		if (!CARD_SCALE.ContainsKey(cardType))
		{
			Debug.LogWarning("CardReward - No CARD_SCALE exists for card type " + cardType);
			actor.transform.localScale = CARD_SCALE[TAG_CARDTYPE.MINION];
		}
		else
		{
			actor.transform.localScale = CARD_SCALE[cardType];
		}
		actor.transform.localPosition = Vector3.zero;
		actor.transform.localRotation = Quaternion.identity;
		actor.TurnOffCollider();
		if (base.Data.Origin != NetCache.ProfileNotice.NoticeOrigin.ACHIEVEMENT)
		{
			LayerUtils.SetLayer(actor.gameObject, GameLayer.IgnoreFullScreenEffects);
		}
		m_actors.Add(actor);
	}

	private void FinishSettingUpActor(Actor actor, DefLoader.DisposableCardDef cardDef)
	{
		CardRewardData cardRewardData = base.Data as CardRewardData;
		actor.SetCardDef(cardDef);
		actor.SetPremium(cardRewardData.Premium);
		actor.CreateBannedRibbon();
		actor.UpdateAllComponents();
	}

	private void SetClassRewardScroll()
	{
		CardRewardData cardRewardData = base.Data as CardRewardData;
		EntityDef entityDef = DefLoader.Get().GetEntityDef(cardRewardData.CardID);
		TAG_CLASS cardClass = entityDef.GetClass();
		string className = GameStrings.GetClassName(cardClass);
		m_classUnlockScroll.SetActive(value: true);
		m_classUnlockScrollTitle.Text = GameStrings.Format("GLUE_CLASS_UNLOCKED_CLASS");
		m_classUnlockScrollDescription.Text = GameStrings.Format("GLUE_CLASS_UNLOCK_DESCRIPTION", className);
		m_classUnlockScrollDeckWidget.RegisterReadyListener(delegate
		{
			DeckTemplateDbfRecord record2 = GameDbf.DeckTemplate.GetRecord((DeckTemplateDbfRecord record) => record.IsStarterDeck && record.ClassId == (int)cardClass && EventTimingManager.Get().IsEventActive(record.Event));
			if (record2 != null)
			{
				DeckPouchDataModel deckPouchDataModel = ShopDeckPouchDisplay.CreateDeckPouchDataModelFromDeckTemplate(record2, null);
				if (deckPouchDataModel != null)
				{
					m_classUnlockScrollDeckWidget.BindDataModel(deckPouchDataModel);
				}
			}
		});
	}

	private void SetUpGoldenHeroAchieves()
	{
		string rewardHeadline = GameStrings.Get("GLOBAL_REWARD_GOLDEN_HERO_HEADLINE");
		SetRewardText(rewardHeadline, string.Empty, string.Empty);
	}

	private void SetBattlegroundsHeroRewardText()
	{
		string rewardHeadline = GameStrings.Get("GLOBAL_REWARD_BATTLEGROUNDS_HERO_HEADLINE");
		SetRewardText(rewardHeadline, string.Empty, string.Empty);
	}

	private void SetBattlegroundsGuideRewardText()
	{
		string rewardHeadline = GameStrings.Get("GLOBAL_REWARD_BATTLEGROUNDS_GUIDE_HEADLINE");
		SetRewardText(rewardHeadline, string.Empty, string.Empty);
	}

	private void SetGenericRandomCardRewardText(EntityDef entityDef)
	{
		string rewardHeadline = entityDef.GetRarity() switch
		{
			TAG_RARITY.LEGENDARY => GameStrings.Get("GLUE_GENERIC_RANDOM_LEGENDARY_SCROLL_TITLE"), 
			TAG_RARITY.EPIC => GameStrings.Get("GLUE_GENERIC_RANDOM_EPIC_SCROLL_TITLE"), 
			TAG_RARITY.RARE => GameStrings.Get("GLUE_GENERIC_RANDOM_RARE_SCROLL_TITLE"), 
			_ => GameStrings.Get("GLUE_GENERIC_RANDOM_CARD_SCROLL_TITLE"), 
		};
		string description = GameStrings.Format("GLUE_GENERIC_RANDOM_CARD_SCROLL_DESC", entityDef.GetName());
		SetRewardText(rewardHeadline, description, string.Empty);
	}

	private void SetSpecificCardRewardText(EntityDef entityDef)
	{
		string rewardHeadline = entityDef.GetName();
		string description = GameStrings.Get("GLUE_GENERIC_SPECIFIC_CARD_SCROLL_DESC");
		SetRewardText(rewardHeadline, description, string.Empty);
	}

	private void InitRewardText()
	{
		CardRewardData cardRewardData = base.Data as CardRewardData;
		EntityDef entityDef = DefLoader.Get().GetEntityDef(cardRewardData.CardID);
		if (entityDef.IsHeroSkin())
		{
			return;
		}
		string rewardHeadline = GameStrings.Get("GLOBAL_REWARD_CARD_HEADLINE");
		string rewardDetails = string.Empty;
		string rewardSource = string.Empty;
		entityDef.GetCardSet();
		TAG_CLASS cardClass = entityDef.GetClass();
		string className = GameStrings.GetClassName(cardClass);
		if (GameMgr.Get().IsTraditionalTutorial())
		{
			rewardDetails = GameUtils.GetCurrentTutorialCardRewardDetails();
		}
		else if (entityDef.IsCoreCard())
		{
			int numCardsInSet = 17;
			if (cardClass == TAG_CLASS.NEUTRAL)
			{
				numCardsInSet = 80;
			}
			int numCardsOwnedInSet = CollectionManager.Get().GetCoreCardsIOwn(cardClass);
			if (cardRewardData.Premium == TAG_PREMIUM.GOLDEN)
			{
				rewardDetails = string.Empty;
			}
			else
			{
				if (numCardsInSet == numCardsOwnedInSet)
				{
					cardRewardData.InnKeeperLine = CardRewardData.InnKeeperTrigger.CORE_CLASS_SET_COMPLETE;
				}
				else if (numCardsOwnedInSet == 4)
				{
					cardRewardData.InnKeeperLine = CardRewardData.InnKeeperTrigger.SECOND_REWARD_EVER;
				}
				rewardDetails = GameStrings.Format("GLOBAL_REWARD_CORE_CARD_DETAILS", numCardsOwnedInSet, numCardsInSet, className);
			}
		}
		if (base.Data.Origin == NetCache.ProfileNotice.NoticeOrigin.LEVEL_UP)
		{
			TAG_CLASS heroClass = (TAG_CLASS)base.Data.OriginData;
			NetCache.HeroLevel level = GameUtils.GetHeroLevel(heroClass);
			rewardSource = GameStrings.Format("GLOBAL_REWARD_CARD_LEVEL_UP", level.CurrentLevel.Level.ToString(), GameStrings.GetClassName(heroClass));
		}
		else
		{
			rewardSource = string.Empty;
		}
		SetRewardText(rewardHeadline, rewardDetails, rewardSource);
	}

	private void SetTransformValue(Transform transform, TransformOverrideSet.TransformOverride.TransformType transformType, Vector3 value)
	{
		if (!(transform == null))
		{
			switch (transformType)
			{
			case TransformOverrideSet.TransformOverride.TransformType.Position:
				transform.localPosition = value;
				break;
			case TransformOverrideSet.TransformOverride.TransformType.Rotation:
				transform.localEulerAngles = value;
				break;
			case TransformOverrideSet.TransformOverride.TransformType.Scale:
				transform.localScale = value;
				break;
			}
		}
	}

	private void SetClassIconPosition(Actor actor)
	{
		if (!(actor == null))
		{
			if (RewardUtils.IsShopPremiumHeroSkin(actor.GetEntityDef()) && actor.m_diamondPortraitClassIconPos != null)
			{
				m_shopMultiClassIcons.transform.parent = actor.m_diamondPortraitClassIconPos.transform;
			}
			else if (actor.m_classIconPos != null)
			{
				m_shopMultiClassIcons.transform.parent = actor.m_classIconPos.transform;
			}
			m_shopMultiClassIcons.transform.localPosition = Vector3.zero;
		}
	}

	private IEnumerator SetMultiClassIcons()
	{
		if (base.Data is CardRewardData cardRewardData && !(m_shopMultiClassIcons == null))
		{
			m_shopMultiClassIcons.gameObject.SetActive(value: true);
			yield return new WaitUntil(() => m_shopMultiClassIcons.gameObject.activeInHierarchy);
			if (m_shopMultiClassIcons.Initialize(cardRewardData.CardID))
			{
				m_shopMultiClassIcons.ShowClassIcons();
			}
		}
	}
}
