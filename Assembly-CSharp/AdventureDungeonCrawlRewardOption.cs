using System;
using System.Collections;
using System.Collections.Generic;
using Hearthstone.DungeonCrawl;
using PegasusShared;
using UnityEngine;

[CustomEditClass]
public class AdventureDungeonCrawlRewardOption : MonoBehaviour
{
	[Serializable]
	public class AdventureDungeonCrawlRewardOptionStyleOverride
	{
		public DungeonRunVisualStyle VisualStyle;

		public GameObject SlamDustEffect;
	}

	public delegate void OptionChosenCallback();

	public struct OptionData
	{
		public readonly AdventureDungeonCrawlPlayMat.OptionType optionType;

		public readonly List<long> options;

		public readonly int index;

		public OptionData(AdventureDungeonCrawlPlayMat.OptionType optionType, List<long> options, int index)
		{
			this.optionType = optionType;
			this.options = new List<long>(options);
			this.index = index;
		}
	}

	private struct OnFullDefLoadedData
	{
		public OptionData optionData;

		public DefLoader.DisposableFullDef fullDef;
	}

	public UberText m_optionName;

	public GameObject m_lootCrate;

	public AdventureDungeonCrawlDeckTray m_deckTray;

	public UIBButton m_chooseButton;

	public Transform m_bigCardBone;

	public float m_treasureOutroAnimDelay = 0.5f;

	[CustomEditField(Sections = "Animations")]
	public PlayMakerFSM m_lootCrateFSM;

	[CustomEditField(Sections = "Animations")]
	public string m_lootCrateDropAnimName;

	[CustomEditField(Sections = "Animations")]
	public string m_lootCrateSummonAnimName;

	[CustomEditField(Sections = "Animations")]
	public string m_lootCrateBurnAnimName;

	[CustomEditField(Sections = "Animations")]
	public string m_lootCrateAnimDoneStateName;

	[CustomEditField(Sections = "SFX", T = EditType.SOUND_PREFAB)]
	public string m_treasureCardAppearsSFX;

	[CustomEditField(Sections = "SFX", T = EditType.SOUND_PREFAB)]
	public string m_treasureCardSelectedSFX;

	[CustomEditField(Sections = "SFX", T = EditType.SOUND_PREFAB)]
	public string m_treasureCardDissipateWhenNotSelectedSFX;

	[CustomEditField(Sections = "Particles")]
	public PlayNewParticles m_particleScript;

	[CustomEditField(Sections = "Shrines")]
	public GameObject ShrineClassBanner;

	[CustomEditField(Sections = "Shrines")]
	public UberText ShrineClassBannerText;

	[CustomEditField(Sections = "Shrines")]
	public float ShrineClassBannerScalePercent;

	[CustomEditField(Sections = "Shrines")]
	public Vector3 ShrineCardPositionOffset;

	public List<AdventureDungeonCrawlRewardOptionStyleOverride> m_rewardOptionStyle;

	protected IDungeonCrawlData m_dungeonRunData;

	public const float LEFT_MOST_BIG_CARD_X_POS = 0.27f;

	private OptionData m_optionData;

	private Actor m_bigCardActor;

	private const TAG_RARITY TREASURE_CARD_RARITY = TAG_RARITY.RARE;

	private OptionChosenCallback m_optionChosenCallback;

	private bool m_outroSpellIsPlaying;

	public void Initalize(IDungeonCrawlData data)
	{
		m_dungeonRunData = data;
		SetRewardOptionVisualStyle();
	}

	private void Start()
	{
		m_chooseButton.AddEventListener(UIEventType.RELEASE, delegate
		{
			OptionChosen();
		});
	}

	public void SetRewardData(OptionData optionData)
	{
		m_optionData = optionData;
		EnableInteraction();
		if (m_bigCardActor != null)
		{
			m_bigCardActor.Destroy();
			m_bigCardActor = null;
		}
		if (OptionTypeIsTreasure(optionData.optionType))
		{
			m_lootCrate.gameObject.SetActive(value: false);
			long dbId = GetTreasureDatabaseID(optionData);
			if (dbId == 0L)
			{
				Log.Adventures.PrintWarning("Treasure choice has no dbId!");
				return;
			}
			string cardId = GameUtils.TranslateDbIdToCardId((int)dbId);
			if (cardId == null)
			{
				Log.Adventures.PrintWarning("AdventureDungeonCrawlRewardOption.SetRewardData() - No cardId for dbId {0}!", dbId);
			}
			else
			{
				DefLoader.Get().LoadFullDef(cardId, OnTreasureFullDefLoaded, optionData);
			}
		}
		else if (optionData.optionType == AdventureDungeonCrawlPlayMat.OptionType.LOOT)
		{
			SetLootCrateContents(optionData);
		}
	}

	public Actor GetActorFromCardId(string cardId)
	{
		if (m_deckTray != null)
		{
			DeckTrayDeckTileVisual visual = m_deckTray.GetCardsContent().GetCardTileVisual(cardId);
			if (visual != null)
			{
				return visual.GetActor();
			}
		}
		if (m_bigCardActor != null && m_bigCardActor.GetEntityDef() != null && m_bigCardActor.GetEntityDef().GetCardId() == cardId)
		{
			return m_bigCardActor;
		}
		return null;
	}

	public int GetTreasureDatabaseID()
	{
		return GetTreasureDatabaseID(m_optionData);
	}

	public static int GetTreasureDatabaseID(OptionData optionData)
	{
		if (!OptionTypeIsTreasure(optionData.optionType))
		{
			return 0;
		}
		if (optionData.options.Count < 1)
		{
			return 0;
		}
		return (int)optionData.options[0];
	}

	private void OnTreasureFullDefLoaded(string cardID, DefLoader.DisposableFullDef def, object userData)
	{
		OnFullDefLoadedData onFullDefLoadedData = default(OnFullDefLoadedData);
		onFullDefLoadedData.optionData = (OptionData)userData;
		onFullDefLoadedData.fullDef = def;
		OnFullDefLoadedData callbackData = onFullDefLoadedData;
		AssetLoader.Get().InstantiatePrefab(ActorNames.GetHandActor(def.EntityDef, TAG_PREMIUM.NORMAL), OnTreasureActorLoaded, callbackData, AssetLoadingOptions.IgnorePrefabPosition);
	}

	private void OnTreasureActorLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		OnFullDefLoadedData callbackDataObject = (OnFullDefLoadedData)callbackData;
		using (callbackDataObject.fullDef)
		{
			if (go == null)
			{
				Debug.LogWarning($"AdventureDungeonCrawlRewardOption.OnActorLoaded() - FAILED to load actor \"{assetRef}\"");
				return;
			}
			Actor actor = go.GetComponent<Actor>();
			if (actor == null)
			{
				Debug.LogWarning($"AdventureDungeonCrawlRewardOption.OnActorLoaded() - ERROR actor \"{assetRef}\" has no Actor component");
				return;
			}
			actor.SetPremium(TAG_PREMIUM.NORMAL);
			actor.SetEntityDef(callbackDataObject.fullDef.EntityDef);
			actor.SetCardDef(callbackDataObject.fullDef.DisposableCardDef);
			actor.UpdateAllComponents();
			actor.ContactShadow(visible: true);
			actor.transform.parent = m_bigCardBone;
			actor.transform.localPosition = Vector3.zero;
			actor.transform.localScale = Vector3.one;
			if (callbackDataObject.optionData.optionType == AdventureDungeonCrawlPlayMat.OptionType.SHRINE_TREASURE)
			{
				ShrineClassBanner.SetActive(value: true);
				GameUtils.SetParent(ShrineClassBanner, actor.GetCardTypeBannerAnchor(), withRotation: true);
				ShrineClassBannerText.Text = GameStrings.GetClassName(callbackDataObject.fullDef.EntityDef.GetClass());
				actor.transform.localScale = Vector3.one * ShrineClassBannerScalePercent;
				actor.transform.localPosition += ShrineCardPositionOffset;
			}
			actor.Hide();
			CardSelectionHandler cardSelectionHandler = actor.GetCollider().gameObject.AddComponent<CardSelectionHandler>();
			cardSelectionHandler.SetActor(actor);
			cardSelectionHandler.SetChoiceNum(m_optionData.index + 1);
			cardSelectionHandler.SetChosenCallback(OptionChosen);
			m_bigCardActor = actor;
		}
	}

	public OptionData GetOptionData()
	{
		return m_optionData;
	}

	public void SetOptionChosenCallback(OptionChosenCallback callback)
	{
		m_optionChosenCallback = callback;
	}

	public bool IsInitialized()
	{
		if (OptionTypeIsTreasure(m_optionData.optionType))
		{
			return m_bigCardActor != null;
		}
		return true;
	}

	public void PlayIntro()
	{
		if (OptionTypeIsTreasure(m_optionData.optionType))
		{
			if (m_bigCardActor == null)
			{
				Log.Adventures.PrintError("AdventureDungeonCrawlRewardOption.PlayIntro() - attempting to play intro for TREASURE, but m_bigCardActor is null!");
				return;
			}
			m_bigCardActor.Show();
			m_bigCardActor.ActivateSpellBirthState(SpellType.SUMMON_IN_FORGE);
			m_bigCardActor.ActivateSpellBirthState(DraftDisplay.GetSpellTypeForRarity(TAG_RARITY.RARE));
			if (!string.IsNullOrEmpty(m_treasureCardAppearsSFX))
			{
				SoundManager.Get().LoadAndPlay(m_treasureCardAppearsSFX);
			}
		}
		else if (m_optionData.optionType == AdventureDungeonCrawlPlayMat.OptionType.LOOT)
		{
			m_lootCrateFSM.SendEvent(m_lootCrateDropAnimName);
		}
	}

	public bool IntroIsPlaying()
	{
		if (OptionTypeIsTreasure(m_optionData.optionType))
		{
			return false;
		}
		if (m_optionData.optionType == AdventureDungeonCrawlPlayMat.OptionType.LOOT)
		{
			return m_lootCrateFSM.ActiveStateName != m_lootCrateAnimDoneStateName;
		}
		return false;
	}

	private void EnableInteraction()
	{
		if (m_bigCardActor != null && m_bigCardActor.GetCollider() != null)
		{
			m_bigCardActor.GetCollider().enabled = true;
		}
		m_chooseButton.SetEnabled(enabled: true);
	}

	public void DisableInteraction()
	{
		if (m_bigCardActor != null && m_bigCardActor.GetCollider() != null)
		{
			m_bigCardActor.GetCollider().enabled = false;
		}
		m_chooseButton.SetEnabled(enabled: false);
	}

	public void PlayOutro(bool thisOptionSelected)
	{
		if (OptionTypeIsTreasure(m_optionData.optionType))
		{
			if (m_bigCardActor == null)
			{
				Log.Adventures.PrintWarning("AdventureDungeonCrawlRewardOption.PlayIntro() - attempting to play outro for TREASURE, but m_bigCardActor is null!");
				return;
			}
			m_outroSpellIsPlaying = true;
			Spell raritySpell = m_bigCardActor.GetSpell(DraftDisplay.GetSpellTypeForRarity(TAG_RARITY.RARE));
			if (thisOptionSelected)
			{
				m_bigCardActor.GetSpell(SpellType.SUMMON_OUT_FORGE).AddFinishedCallback(OutroSpellFinished, m_bigCardActor);
				m_bigCardActor.ActivateSpellBirthState(SpellType.SUMMON_OUT_FORGE);
				if (raritySpell != null)
				{
					raritySpell.ActivateState(SpellStateType.DEATH);
				}
				if (!string.IsNullOrEmpty(m_treasureCardSelectedSFX))
				{
					SoundManager.Get().LoadAndPlay(m_treasureCardSelectedSFX);
				}
				return;
			}
			Spell burnSpell = m_bigCardActor.GetSpell(SpellType.BURN);
			if (burnSpell != null)
			{
				burnSpell.AddFinishedCallback(OutroSpellFinished, m_bigCardActor);
				m_bigCardActor.ActivateSpellBirthState(SpellType.BURN);
			}
			else
			{
				OutroSpellFinished(null, m_bigCardActor);
			}
			if (raritySpell != null)
			{
				raritySpell.ActivateState(SpellStateType.DEATH);
			}
			if (!string.IsNullOrEmpty(m_treasureCardDissipateWhenNotSelectedSFX))
			{
				SoundManager.Get().LoadAndPlay(m_treasureCardDissipateWhenNotSelectedSFX);
			}
		}
		else if (m_optionData.optionType == AdventureDungeonCrawlPlayMat.OptionType.LOOT)
		{
			m_lootCrateFSM.SendEvent(thisOptionSelected ? m_lootCrateSummonAnimName : m_lootCrateBurnAnimName);
		}
	}

	private void OutroSpellFinished(Spell spell, object actorObject)
	{
		Actor actor = (Actor)actorObject;
		StartCoroutine(WaitForAnimToFinishThenDestroy(actor.gameObject));
	}

	private IEnumerator WaitForAnimToFinishThenDestroy(GameObject gameObjectToDestroy)
	{
		yield return new WaitForSeconds(m_treasureOutroAnimDelay);
		m_outroSpellIsPlaying = false;
		yield return new WaitForSeconds(5f);
		UnityEngine.Object.Destroy(gameObjectToDestroy);
	}

	public bool OutroIsPlaying()
	{
		if (OptionTypeIsTreasure(m_optionData.optionType))
		{
			return m_outroSpellIsPlaying;
		}
		if (m_optionData.optionType == AdventureDungeonCrawlPlayMat.OptionType.LOOT)
		{
			return m_lootCrateFSM.ActiveStateName != m_lootCrateAnimDoneStateName;
		}
		return false;
	}

	private void OptionChosen()
	{
		if (m_optionChosenCallback != null)
		{
			m_optionChosenCallback();
		}
	}

	private void SetRewardOptionVisualStyle()
	{
		DungeonRunVisualStyle visualStyle = m_dungeonRunData.VisualStyle;
		foreach (AdventureDungeonCrawlRewardOptionStyleOverride style in m_rewardOptionStyle)
		{
			if (visualStyle == style.VisualStyle)
			{
				if (m_particleScript != null)
				{
					m_particleScript.m_Target = style.SlamDustEffect;
				}
				break;
			}
		}
	}

	private static bool OptionTypeIsTreasure(AdventureDungeonCrawlPlayMat.OptionType optionType)
	{
		if (optionType != AdventureDungeonCrawlPlayMat.OptionType.SHRINE_TREASURE)
		{
			return optionType == AdventureDungeonCrawlPlayMat.OptionType.TREASURE;
		}
		return true;
	}

	private void SetLootCrateContents(OptionData optionData)
	{
		m_lootCrate.gameObject.SetActive(value: true);
		CollectionDeck rewardOptionCardSet = new CollectionDeck
		{
			Type = DeckType.CLIENT_ONLY_DECK,
			FormatType = FormatType.FT_WILD,
			HeroCardID = "None"
		};
		for (int i = 0; i < optionData.options.Count; i++)
		{
			long dbId = optionData.options[i];
			if (i == 0)
			{
				if (dbId == 0L)
				{
					m_optionName.Text = "";
					continue;
				}
				EntityDef groupTitleDef = DefLoader.Get().GetEntityDef((int)dbId);
				m_optionName.Text = groupTitleDef.GetName();
				continue;
			}
			string cardId = GameUtils.TranslateDbIdToCardId((int)dbId);
			if (string.IsNullOrEmpty(cardId))
			{
				Log.Adventures.PrintWarning("AdventureDungeonCrawlRewardOption.SetRewardData() - No cardId for dbId {0}!", dbId);
			}
			else
			{
				rewardOptionCardSet.InsertSlotWithCard(cardId, TAG_PREMIUM.NORMAL, owned: false, 1);
			}
		}
		m_deckTray.SetDungeonCrawlDeck(rewardOptionCardSet, playGlowAnimation: false);
	}
}
