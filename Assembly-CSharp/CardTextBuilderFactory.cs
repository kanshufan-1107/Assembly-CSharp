using Assets;

public class CardTextBuilderFactory
{
	public static CardTextBuilder Create(Assets.Card.CardTextBuilderType type)
	{
		switch (type)
		{
		case Assets.Card.CardTextBuilderType.JADE_GOLEM:
			return new JadeGolemCardTextBuilder();
		case Assets.Card.CardTextBuilderType.JADE_GOLEM_TRIGGER:
			return new JadeGolemTriggerCardTextBuilder();
		case Assets.Card.CardTextBuilderType.MODULAR_ENTITY:
			return new ModularEntityCardTextBuilder();
		case Assets.Card.CardTextBuilderType.KAZAKUS_POTION_EFFECT:
			return new KazakusPotionEffectCardTextBuilder();
		case Assets.Card.CardTextBuilderType.PRIMORDIAL_WAND:
			return new PrimordialWandCardTextBuilder();
		case Assets.Card.CardTextBuilderType.ALTERNATE_CARD_TEXT:
			return new AlternateCardTextCardTextBuilder();
		case Assets.Card.CardTextBuilderType.SCRIPT_DATA_NUM_1:
			return new ScriptDataNum1CardTextBuilder();
		case Assets.Card.CardTextBuilderType.GALAKROND_COUNTER:
			return new GalakrondCounterCardTextBuilder();
		case Assets.Card.CardTextBuilderType.DECORATE:
			return new DecorateCardTextBuilder();
		case Assets.Card.CardTextBuilderType.GAMEPLAY_STRING:
			return new GameplayStringTextBuilder();
		case Assets.Card.CardTextBuilderType.ZOMBEAST:
			return new ZombeastCardTextBuilder();
		case Assets.Card.CardTextBuilderType.ZOMBEAST_ENCHANTMENT:
			return new ZombeastEnchantmentCardTextBuilder();
		case Assets.Card.CardTextBuilderType.HIDDEN_CHOICE:
			return new HiddenChoiceCardTextBuilder();
		case Assets.Card.CardTextBuilderType.INVESTIGATE:
			return new InvestigateCardTextBuilder();
		case Assets.Card.CardTextBuilderType.REFERENCE_CREATOR_ENTITY:
			return new ReferenceCreatorEntityCardTextBuilder();
		case Assets.Card.CardTextBuilderType.REFERENCE_SCRIPT_DATA_NUM_1_ENTITY:
			return new ReferenceScriptDataNum1EntityCardTextBuilder();
		case Assets.Card.CardTextBuilderType.REFERENCE_SCRIPT_DATA_NUM_1_NUM_2_ENTITY:
			return new ReferenceScriptDataNum1Num2EntityCardTextBuilder();
		case Assets.Card.CardTextBuilderType.UNDATAKAH_ENCHANT:
			return new UndatakahCardTextBuilder();
		case Assets.Card.CardTextBuilderType.PLAYER_TAG_THRESHOLD:
			return new PlayerTagThresholdCardTextBuilder();
		case Assets.Card.CardTextBuilderType.ENTITY_TAG_THRESHOLD:
			return new EntityTagThresholdCardTextBuilder();
		case Assets.Card.CardTextBuilderType.DRUSTVAR_HORROR:
			return new DrustvarHorrorTargetingTextBuilder();
		case Assets.Card.CardTextBuilderType.SPELL_DAMAGE_ONLY:
			return new SpellDamageOnlyCardTextBuilder();
		case Assets.Card.CardTextBuilderType.HIDDEN_ENTITY:
			return new HiddenEntityCardTextBuilder();
		case Assets.Card.CardTextBuilderType.SCORE_VALUE_COUNT_DOWN:
			return new ScoreValueCountDownCardTextBuilder();
		case Assets.Card.CardTextBuilderType.SCRIPT_DATA_NUM_1_NUM_2:
			return new ScriptDataNum1Num2CardTextBuilder();
		case Assets.Card.CardTextBuilderType.POWERED_UP:
			return new PoweredUpTargetingTextBuilder();
		case Assets.Card.CardTextBuilderType.MULTIPLE_ALT_TEXT_SCRIPT_DATA_NUMS:
			return new MultiAltTextScriptDataNumsCardTextBuilder();
		case Assets.Card.CardTextBuilderType.REFERENCE_SCRIPT_DATA_NUM_1_ENTITY_POWER:
			return new ReferenceScriptDataNum1EntityPower();
		case Assets.Card.CardTextBuilderType.REFERENCE_SCRIPT_DATA_NUM_1_NUM_2_ENTITY_POWER:
			return new ReferenceScriptDataNum1Num2EntityPower();
		case Assets.Card.CardTextBuilderType.REFERENCE_SCRIPT_DATA_NUM_1_CARD_DBID:
			return new ReferenceScriptDataNum1CardDBIDCardTextBuilder();
		case Assets.Card.CardTextBuilderType.MULTIPLE_ENTITY_NAMES:
			return new MultipleEntityNamesCardTextBuilder();
		case Assets.Card.CardTextBuilderType.REFERENCE_SCRIPT_DATA_NUM_CARD_RACE:
			return new ReferenceScriptDataNumCardRaceCardTextBuilder();
		case Assets.Card.CardTextBuilderType.BG_QUEST:
			return new BGQuestCardTextBuilder();
		case Assets.Card.CardTextBuilderType.MULTIPLE_ALT_TEXT_SCRIPT_DATA_NUMS_REF_SDN6_CARD_DBID:
		{
			MultiAltTextScriptDataNumsCardTextBuilder multiAltTextScriptDataNumsCardTextBuilder = new MultiAltTextScriptDataNumsCardTextBuilder();
			multiAltTextScriptDataNumsCardTextBuilder.SetTagRefType(GAME_TAG.TAG_SCRIPT_DATA_NUM_6, MultiAltTextScriptDataNumsCardTextBuilder.TagReferenceType.CardDBID);
			return multiAltTextScriptDataNumsCardTextBuilder;
		}
		case Assets.Card.CardTextBuilderType.ZILLIAX_DELUXE_3000:
			return new ZilliaxDeluxe3000CardTextBuilder();
		case Assets.Card.CardTextBuilderType.BATTLEGROUNDS_ZILLIAX:
			return new BattlegroundsZilliaxCardTextBuilder();
		case Assets.Card.CardTextBuilderType.SPELL_ABSORB:
			return new SpellAbsorbCardTextBuilder();
		case Assets.Card.CardTextBuilderType.ALT_TEXT_REFERENCE_SCRIPT_DATA_NUM_1_NUM_2_ENTITY_POWER:
			return new AltTextScriptDataNum1Num2EntityPowerCardTextBuilder();
		default:
			return new CardTextBuilder();
		}
	}
}
