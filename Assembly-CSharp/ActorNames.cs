using System.Collections.Generic;
using Assets;
using UnityEngine;

public class ActorNames
{
	public enum ACTOR_ASSET
	{
		HAND_MINION,
		HAND_SPELL,
		HAND_WEAPON,
		HAND_HERO,
		HAND_MERCENARY,
		HAND_LOCATION,
		PLAY_MINION,
		PLAY_WEAPON,
		PLAY_HERO,
		PLAY_HERO_POWER,
		PLAY_GAME_MODE_BUTTON,
		PLAY_MOVE_MINION_HOVER_TARGET,
		PLAY_MERCENARY,
		PLAY_LETTUCE_ABILITY_SPELL,
		PLAY_LETTUCE_ABILITY_MINION,
		PLAY_LETTUCE_EQUIPMENT,
		PLAY_BATTLEGROUND_HERO_BUDDY,
		PLAY_LOCATION,
		PLAY_BATTLEGROUND_QUEST_REWARD,
		PLAY_BATTLEGROUND_ANOMALY,
		PLAY_BATTLEGROUND_SPELL,
		PLAY_BATTLEGROUND_TRINKET,
		PLAY_BATTLEGROUND_CLICKABLE_BUTTON,
		HISTORY_HERO_POWER,
		HISTORY_HERO_POWER_OPPONENT,
		BIG_CARD_LETTUCE_ABILITY_SPELL,
		BIG_CARD_LETTUCE_ABILITY_MINION,
		BIG_CARD_LETTUCE_EQUIPMENT,
		BIG_CARD_BG_ANOMALY,
		BIG_CARD_BG_TRINKET,
		BIG_CARD_BG_CLICKABLE_CARD
	}

	public static readonly Dictionary<ACTOR_ASSET, string> s_actorAssets = new Dictionary<ACTOR_ASSET, string>
	{
		{
			ACTOR_ASSET.HAND_MINION,
			"Card_Hand_Ally.prefab:d00eb0f79080e0749993fe4619e9143d"
		},
		{
			ACTOR_ASSET.HAND_SPELL,
			"Card_Hand_Ability.prefab:3c3f5189f0d0b3745a1c1ca21d41efe0"
		},
		{
			ACTOR_ASSET.HAND_WEAPON,
			"Card_Hand_Weapon.prefab:30888a1fdca5c6c43abcc5d9dca55783"
		},
		{
			ACTOR_ASSET.HAND_HERO,
			"Card_Hand_Hero.prefab:a977c49edb5fb5d4c8dee4d2344d1395"
		},
		{
			ACTOR_ASSET.HAND_MERCENARY,
			"Card_Hand_Mercenary.prefab:f9e5a62d0cf1f4b4db131efcf1a082c0"
		},
		{
			ACTOR_ASSET.HAND_LOCATION,
			"Card_Hand_Location.prefab:bc312fcf691884a40967dae38d4d8b79"
		},
		{
			ACTOR_ASSET.PLAY_HERO,
			"Card_Play_Hero.prefab:42cbbd2c4969afb46b3887bb628de19d"
		},
		{
			ACTOR_ASSET.PLAY_MINION,
			"Card_Play_Ally.prefab:23b7de16184fa8042bf6b734e7ca4d60"
		},
		{
			ACTOR_ASSET.PLAY_WEAPON,
			"Card_Play_Weapon.prefab:71f767d4f10681a45ac853936d1db800"
		},
		{
			ACTOR_ASSET.PLAY_HERO_POWER,
			"Card_Play_HeroPower.prefab:a3794839abb947146903a26be13e09af"
		},
		{
			ACTOR_ASSET.PLAY_GAME_MODE_BUTTON,
			"Card_Play_GameModeButton.prefab:6d260d8912ac3f945a4177ba5882eaf2"
		},
		{
			ACTOR_ASSET.PLAY_MOVE_MINION_HOVER_TARGET,
			"Card_Play_MoveMinionHoverTarget.prefab:1f57541a9fdc77344810e84b76693bc4"
		},
		{
			ACTOR_ASSET.PLAY_BATTLEGROUND_HERO_BUDDY,
			"Card_Play_Bacon_CoinBasedBuddyMeter.prefab:e3040483cefb5af49b4ccdd611b01df2"
		},
		{
			ACTOR_ASSET.PLAY_MERCENARY,
			"Card_Play_Mercenary.prefab:7c4e1f3052ce6e545a018b7131dad5ad"
		},
		{
			ACTOR_ASSET.PLAY_LETTUCE_ABILITY_SPELL,
			"Card_Play_LettuceAbility.prefab:c580722c24bcdbd4d9125352d1275e69"
		},
		{
			ACTOR_ASSET.PLAY_LETTUCE_ABILITY_MINION,
			"Card_Play_LettuceAbility_Minion.prefab:9820a8900603e844fb08fcb5493f0334"
		},
		{
			ACTOR_ASSET.PLAY_LETTUCE_EQUIPMENT,
			"Card_Play_LettuceEquipment.prefab:029c966daebb81343ad4c07bc85deaad"
		},
		{
			ACTOR_ASSET.PLAY_LOCATION,
			"Card_Play_Location.prefab:f4ee385d6c5c2a54cb83c817676b3e96"
		},
		{
			ACTOR_ASSET.PLAY_BATTLEGROUND_QUEST_REWARD,
			"Card_Play_Bacon_HeroPower_Quests.prefab:7aeb77ca586b96449b626fc2284d8e7e"
		},
		{
			ACTOR_ASSET.PLAY_BATTLEGROUND_ANOMALY,
			"Card_Play_BG_Anomaly.prefab:50a17de7bc539234ba1986975ea293ac"
		},
		{
			ACTOR_ASSET.PLAY_BATTLEGROUND_SPELL,
			"Card_Play_BaconSpell.prefab:26b10f4aef5073f4f84dcb5058fe8440"
		},
		{
			ACTOR_ASSET.PLAY_BATTLEGROUND_TRINKET,
			"Card_Play_BattlegroundsTrinket.prefab:0fa31459018c057488eefd45fef95a9d"
		},
		{
			ACTOR_ASSET.PLAY_BATTLEGROUND_CLICKABLE_BUTTON,
			"Card_Play_BG_ClickableButton.prefab:aa16914e891d55d438164d5e1eb4241d"
		},
		{
			ACTOR_ASSET.HISTORY_HERO_POWER,
			"History_HeroPower.prefab:e73edf8ccea2b11429093f7a448eef53"
		},
		{
			ACTOR_ASSET.HISTORY_HERO_POWER_OPPONENT,
			"History_HeroPower_Opponent.prefab:a99d23d6e8630f94b96a8e096fffb16f"
		},
		{
			ACTOR_ASSET.BIG_CARD_LETTUCE_ABILITY_SPELL,
			"BigCard_LettuceAbility.prefab:53cf859e0a512a240b9a6b1f8ad524b1"
		},
		{
			ACTOR_ASSET.BIG_CARD_LETTUCE_ABILITY_MINION,
			"BigCard_LettuceAbility_Minion.prefab:cd2ed854b5e5ef542806802188fb40d5"
		},
		{
			ACTOR_ASSET.BIG_CARD_LETTUCE_EQUIPMENT,
			"BigCard_LettuceEquipment.prefab:2b360077e2dc4ec4299908d851b32a5b"
		},
		{
			ACTOR_ASSET.BIG_CARD_BG_ANOMALY,
			"Card_Hand_BG_Anomaly.prefab:7f3f43fd7e6cf3848b2502662d5ba14d"
		},
		{
			ACTOR_ASSET.BIG_CARD_BG_TRINKET,
			"Card_Hand_BG_Trinket.prefab:3c79ff39d8cdb154f86343c20ebf9c5a"
		}
	};

	public static readonly Dictionary<ACTOR_ASSET, string> s_premiumActorAssets = new Dictionary<ACTOR_ASSET, string>
	{
		{
			ACTOR_ASSET.HAND_MINION,
			"Card_Hand_Ally_Premium.prefab:b0f0a4abee3293540830967b829f2bec"
		},
		{
			ACTOR_ASSET.HAND_SPELL,
			"Card_Hand_Ability_Premium.prefab:5105f461bc4a48e4c8bf452b93cfd772"
		},
		{
			ACTOR_ASSET.HAND_WEAPON,
			"Card_Hand_Weapon_Premium.prefab:c7736007f7a350942bbe40e466ac357c"
		},
		{
			ACTOR_ASSET.HAND_HERO,
			"Card_Hand_Hero_Premium.prefab:aca669662daf766449cd351fe4691f8f"
		},
		{
			ACTOR_ASSET.HAND_MERCENARY,
			"Card_Hand_Mercenary_Tier_2.prefab:9c9adc8aa105ac24296f1e1538faf951"
		},
		{
			ACTOR_ASSET.HAND_LOCATION,
			"Card_Hand_Location_Premium.prefab:7de3ab2e9ed39f84fae0b6376494577b"
		},
		{
			ACTOR_ASSET.PLAY_MINION,
			"Card_Play_Ally_Premium.prefab:99bd268ec3a056d4795110a141c6fd75"
		},
		{
			ACTOR_ASSET.PLAY_WEAPON,
			"Card_Play_Weapon_Premium.prefab:66cbba9ed8f300c43834ab519327f094"
		},
		{
			ACTOR_ASSET.PLAY_HERO_POWER,
			"Card_Play_HeroPower_Premium.prefab:015ad985f9ec49e4db327d131fd79901"
		},
		{
			ACTOR_ASSET.PLAY_GAME_MODE_BUTTON,
			"Card_Play_GameModeButton.prefab:6d260d8912ac3f945a4177ba5882eaf2"
		},
		{
			ACTOR_ASSET.PLAY_MOVE_MINION_HOVER_TARGET,
			"Card_Play_MoveMinionHoverTarget.prefab:1f57541a9fdc77344810e84b76693bc4"
		},
		{
			ACTOR_ASSET.PLAY_BATTLEGROUND_HERO_BUDDY,
			"Card_Play_Bacon_CoinBasedBuddyMeter.prefab:e3040483cefb5af49b4ccdd611b01df2"
		},
		{
			ACTOR_ASSET.PLAY_MERCENARY,
			"Card_Play_Mercenary_Tier_2.prefab:c8dcb22e4703ddd4a8584b5dade8b924"
		},
		{
			ACTOR_ASSET.PLAY_LETTUCE_ABILITY_SPELL,
			"Card_Play_LettuceAbility.prefab:c580722c24bcdbd4d9125352d1275e69"
		},
		{
			ACTOR_ASSET.PLAY_LETTUCE_ABILITY_MINION,
			"Card_Play_LettuceAbility_Minion.prefab:9820a8900603e844fb08fcb5493f0334"
		},
		{
			ACTOR_ASSET.PLAY_LETTUCE_EQUIPMENT,
			"Card_Play_LettuceEquipment.prefab:029c966daebb81343ad4c07bc85deaad"
		},
		{
			ACTOR_ASSET.PLAY_LOCATION,
			"Card_Play_Location_Premium.prefab:b7fc72340ca46464699682c3a9758343"
		},
		{
			ACTOR_ASSET.PLAY_BATTLEGROUND_QUEST_REWARD,
			"Card_Play_Bacon_HeroPower_Quests.prefab:7aeb77ca586b96449b626fc2284d8e7e"
		},
		{
			ACTOR_ASSET.PLAY_BATTLEGROUND_ANOMALY,
			"Card_Play_BG_Anomaly.prefab:50a17de7bc539234ba1986975ea293ac"
		},
		{
			ACTOR_ASSET.PLAY_BATTLEGROUND_SPELL,
			"Card_Play_BaconSpell.prefab:26b10f4aef5073f4f84dcb5058fe8440"
		},
		{
			ACTOR_ASSET.PLAY_BATTLEGROUND_TRINKET,
			"Card_Play_BattlegroundsTrinket.prefab:0fa31459018c057488eefd45fef95a9d"
		},
		{
			ACTOR_ASSET.PLAY_BATTLEGROUND_CLICKABLE_BUTTON,
			"Card_Play_BG_ClickableButton.prefab:aa16914e891d55d438164d5e1eb4241d"
		},
		{
			ACTOR_ASSET.HISTORY_HERO_POWER,
			"History_HeroPower_Premium.prefab:081da807b95b8495e9f16825c5164787"
		},
		{
			ACTOR_ASSET.HISTORY_HERO_POWER_OPPONENT,
			"History_HeroPower_Opponent_Premium.prefab:82e1456f33aae4b3d9b2dac73aaa3ffa"
		},
		{
			ACTOR_ASSET.BIG_CARD_LETTUCE_ABILITY_SPELL,
			"BigCard_LettuceAbility.prefab:53cf859e0a512a240b9a6b1f8ad524b1"
		},
		{
			ACTOR_ASSET.BIG_CARD_LETTUCE_ABILITY_MINION,
			"BigCard_LettuceAbility_Minion.prefab:cd2ed854b5e5ef542806802188fb40d5"
		},
		{
			ACTOR_ASSET.BIG_CARD_LETTUCE_EQUIPMENT,
			"BigCard_LettuceEquipment.prefab:2b360077e2dc4ec4299908d851b32a5b"
		},
		{
			ACTOR_ASSET.BIG_CARD_BG_ANOMALY,
			"Card_Hand_BG_Anomaly.prefab:7f3f43fd7e6cf3848b2502662d5ba14d"
		},
		{
			ACTOR_ASSET.BIG_CARD_BG_TRINKET,
			"Card_Hand_BG_Trinket.prefab:3c79ff39d8cdb154f86343c20ebf9c5a"
		}
	};

	public static readonly Dictionary<ACTOR_ASSET, string> s_diamondActorAssets = new Dictionary<ACTOR_ASSET, string>
	{
		{
			ACTOR_ASSET.HAND_MINION,
			"Card_Hand_Ally_Diamond.prefab:5fdbef3fa7e0c05419050d01202a85d3"
		},
		{
			ACTOR_ASSET.HAND_HERO,
			"Card_Hand_Hero_Premium.prefab:aca669662daf766449cd351fe4691f8f"
		},
		{
			ACTOR_ASSET.HAND_MERCENARY,
			"Card_Hand_Mercenary_Tier_3.prefab:1d00ef78a06433d4eb7eb52b9cccfc3a"
		},
		{
			ACTOR_ASSET.PLAY_MINION,
			"Card_Play_Ally_Diamond.prefab:42fb12461ed7d0142a34f9b72399421c"
		},
		{
			ACTOR_ASSET.PLAY_HERO,
			"Card_Play_Hero_Diamond.prefab:0cfe344df80dea44c898096484082e7b"
		},
		{
			ACTOR_ASSET.PLAY_MERCENARY,
			"Card_Play_Mercenary_Tier_3.prefab:bf967a38c2a6edf4c9b64d49f0ce41df"
		}
	};

	private static Dictionary<string, int> m_signatureFrames;

	private static Dictionary<int, string> m_signatureHandMinions;

	private static Dictionary<int, string> m_signaturePlayMinions;

	private static Dictionary<int, string> m_signatureSpells;

	private static Dictionary<int, string> m_signatureHeroes;

	public static Dictionary<string, int> SignatureFrames
	{
		get
		{
			if (m_signatureFrames == null)
			{
				Initialize();
			}
			return m_signatureFrames;
		}
	}

	public static Dictionary<int, string> SignatureHandMinions
	{
		get
		{
			if (m_signatureHandMinions == null)
			{
				Initialize();
			}
			return m_signatureHandMinions;
		}
	}

	public static Dictionary<int, string> SignaturePlayMinions
	{
		get
		{
			if (m_signaturePlayMinions == null)
			{
				Initialize();
			}
			return m_signaturePlayMinions;
		}
	}

	public static Dictionary<int, string> SignatureSpells
	{
		get
		{
			if (m_signatureSpells == null)
			{
				Initialize();
			}
			return m_signatureSpells;
		}
	}

	public static Dictionary<int, string> SignatureHeroes
	{
		get
		{
			if (m_signatureHeroes == null)
			{
				Initialize();
			}
			return m_signatureHeroes;
		}
	}

	public static string GetZoneActor(EntityBase entityBase, TAG_ZONE zoneTag, Player controller, TAG_PREMIUM premium, bool forceRevealed = false)
	{
		TAG_CARDTYPE cardType = entityBase.GetCardType();
		TAG_CLASS classTag = entityBase.GetClass();
		bool isGhostly = entityBase.HasTag(GAME_TAG.GHOSTLY);
		switch (zoneTag)
		{
		case TAG_ZONE.DECK:
		case TAG_ZONE.REMOVEDFROMGAME:
		case TAG_ZONE.SETASIDE:
			return "Card_Invisible.prefab:579b3b9a80234754593f24582f9cb93b";
		case TAG_ZONE.SECRET:
		{
			bool isQuest = entityBase.HasTag(GAME_TAG.QUEST);
			bool isSideQuest = entityBase.HasTag(GAME_TAG.SIDE_QUEST);
			bool isQuestline = entityBase.HasTag(GAME_TAG.QUESTLINE);
			bool isSigil = entityBase.HasTag(GAME_TAG.SIGIL);
			bool isObjective = entityBase.HasTag(GAME_TAG.OBJECTIVE);
			bool isPuzzle = entityBase.HasTag(GAME_TAG.PUZZLE);
			TAG_PUZZLE_TYPE puzzleType = entityBase.GetPuzzleType();
			bool isRulebook = entityBase.HasTag(GAME_TAG.RULEBOOK);
			if (entityBase != null && entityBase.IsBobQuest())
			{
				return "Card_Play_Bacon_Bob_Quest.prefab:b179261da0ff34e4390103a43c4d46dc";
			}
			if (isQuest)
			{
				if (GameMgr.Get() != null && GameMgr.Get().IsBattlegrounds())
				{
					return "Card_Play_Bacon_Quest.prefab:30e69416dada17e43b7f2722ffb25f0e";
				}
				return "Card_Play_Quest.prefab:321b6d1ad558ebd46996c1f4eeaccb0c";
			}
			if (isQuestline)
			{
				return "Card_Play_Questline.prefab:47f7e5eb9be22de42813a3b32660a1d0";
			}
			if (isPuzzle)
			{
				switch (puzzleType)
				{
				case TAG_PUZZLE_TYPE.MIRROR:
					return "Card_Play_Puzzle_Mirror.prefab:4583d6e2b04fad74986ef47b4ff00c79";
				case TAG_PUZZLE_TYPE.LETHAL:
					return "Card_Play_Puzzle_Lethal.prefab:00d669c10a286e84cb91df1d40312d4b";
				case TAG_PUZZLE_TYPE.SURVIVAL:
					return "Card_Play_Puzzle_Survival.prefab:036a2c2eee552fc4db25051107a0b797";
				case TAG_PUZZLE_TYPE.CLEAR:
					return "Card_Play_Puzzle_BoardClear.prefab:fd9eec17f48c319468f103336095ad7b";
				}
			}
			if (isRulebook)
			{
				return "Card_Play_Rulebook.prefab:a8fbb8b315f4a3244be82718c1606858";
			}
			if (isSideQuest)
			{
				switch (classTag)
				{
				case TAG_CLASS.DRUID:
					return "Card_Play_SideQuest_Druid.prefab:d1430dc4bc9786640a02f4b178b59393";
				case TAG_CLASS.HUNTER:
					return "Card_Play_SideQuest_Hunter.prefab:c9ed37b5a056d4e4885dc882d9d37664";
				case TAG_CLASS.MAGE:
					return "Card_Play_SideQuest_Mage.prefab:39faefe5a4f9cf54ba9d85deb7627acb";
				case TAG_CLASS.PALADIN:
					return "Card_Play_SideQuest_Paladin.prefab:396bf10a7c7da404ea3624e009861780";
				case TAG_CLASS.ROGUE:
					return "Card_Play_SideQuest_Rogue.prefab:e805c70aa076e6743925e8d06a4be247";
				}
			}
			if (isSigil && classTag == TAG_CLASS.DEMONHUNTER)
			{
				return "Card_Play_Sigil_DemonHunter.prefab:b1ee048f6f0150e4ebd512208fb6a707";
			}
			if (isObjective)
			{
				if (entityBase.HasTag(GAME_TAG.OBJECTIVE_AURA) || entityBase.HasTag(GAME_TAG.PALADIN_AURA))
				{
					return "Card_Play_Hero_Trigger_Aura.prefab:cf92394f0897f4443a5593d3b30be4af";
				}
				return "Card_Play_Hero_Trigger.prefab:61b3b672a79aecf46a40b7d88e2e1637";
			}
			return classTag switch
			{
				TAG_CLASS.HUNTER => "Card_Play_Secret_Hunter.prefab:fdf71d0657e17a7428a43c1a8f319818", 
				TAG_CLASS.MAGE => "Card_Play_Secret_Mage.prefab:ffc78954f637f6f4d8b8bb7ec0b936ca", 
				TAG_CLASS.PALADIN => "Card_Play_Secret_Paladin.prefab:b0f3901ff0fad674bb7c72faa7966e73", 
				TAG_CLASS.ROGUE => "Card_Play_Secret_Rogue.prefab:1b224ad272f03724c9bc0aa802456c3e", 
				TAG_CLASS.WARRIOR => "Card_Play_Secret_Wanderer.prefab:9eaa9bf6015f05f4e9bbe9ba5e42b20f", 
				_ => "Card_Play_Secret_Mage.prefab:ffc78954f637f6f4d8b8bb7ec0b936ca", 
			};
		}
		case TAG_ZONE.PLAY:
		case TAG_ZONE.LETTUCE_ABILITY:
		{
			string playActor = GetPlayActorByTags(entityBase, premium);
			if (!"Card_Hidden.prefab:1a94649d257bc284ca6e2962f634a8b9".Equals(playActor))
			{
				return playActor;
			}
			break;
		}
		case TAG_ZONE.HAND:
			if ((controller?.IsRevealed() ?? false) || forceRevealed)
			{
				string handActor = GetHandActorByTags(entityBase, premium);
				if (!"Card_Hidden.prefab:1a94649d257bc284ca6e2962f634a8b9".Equals(handActor))
				{
					return handActor;
				}
				break;
			}
			return "Card_Hidden.prefab:1a94649d257bc284ca6e2962f634a8b9";
		case TAG_ZONE.GRAVEYARD:
			if (isGhostly && controller.GetSide() == Player.Side.OPPOSING)
			{
				return "Card_Hidden.prefab:1a94649d257bc284ca6e2962f634a8b9";
			}
			switch (cardType)
			{
			case TAG_CARDTYPE.MINION:
				return GetNameWithPremiumType(ACTOR_ASSET.HAND_MINION, premium, entityBase.GetCardId());
			case TAG_CARDTYPE.WEAPON:
				return GetNameWithPremiumType(ACTOR_ASSET.HAND_WEAPON, premium, entityBase.GetCardId());
			case TAG_CARDTYPE.SPELL:
				return GetNameWithPremiumType(ACTOR_ASSET.HAND_SPELL, premium, entityBase.GetCardId());
			case TAG_CARDTYPE.BATTLEGROUND_SPELL:
				return GetNameWithPremiumType(ACTOR_ASSET.HAND_SPELL, premium, entityBase.GetCardId());
			case TAG_CARDTYPE.HERO:
				return GetNameWithPremiumType(ACTOR_ASSET.HAND_HERO, premium, entityBase.GetCardId());
			case TAG_CARDTYPE.LOCATION:
				return GetNameWithPremiumType(ACTOR_ASSET.HAND_LOCATION, premium, entityBase.GetCardId());
			case TAG_CARDTYPE.BATTLEGROUND_QUEST_REWARD:
				return GetNameWithPremiumType(ACTOR_ASSET.HAND_SPELL, premium);
			case TAG_CARDTYPE.BATTLEGROUND_HERO_BUDDY:
				return GetNameWithPremiumType(ACTOR_ASSET.HAND_SPELL, premium);
			case TAG_CARDTYPE.BATTLEGROUND_CLICKABLE_BUTTON:
				return GetNameWithPremiumType(ACTOR_ASSET.HAND_SPELL, premium, entityBase.GetCardId());
			}
			break;
		}
		Debug.LogWarningFormat("ActorNames.GetZoneActor() - Can't determine actor for {0}. Returning {1} instead.", cardType, "Card_Hidden.prefab:1a94649d257bc284ca6e2962f634a8b9");
		return "Card_Hidden.prefab:1a94649d257bc284ca6e2962f634a8b9";
	}

	public static string GetZoneActor(Entity entity, TAG_ZONE zoneTag, bool forceRevealed = false)
	{
		if (entity.IsHero() && zoneTag == TAG_ZONE.GRAVEYARD)
		{
			return GetGraveyardActorForHero(entity);
		}
		if (entity.HasTag(GAME_TAG.PENDING_TRANSFORM_TO_CARD))
		{
			int cardDBID = entity.GetTag(GAME_TAG.PENDING_TRANSFORM_TO_CARD);
			using DefLoader.DisposableFullDef fullDef = DefLoader.Get().GetFullDef(cardDBID);
			if (fullDef.EntityDef != null)
			{
				return GetZoneActor(fullDef.EntityDef, zoneTag);
			}
		}
		return GetZoneActor(entity, zoneTag, entity.GetController(), entity.GetPremiumType(), forceRevealed);
	}

	public static string GetZoneActor(EntityDef entityDef, TAG_ZONE zoneTag)
	{
		return GetZoneActor(entityDef, zoneTag, null, TAG_PREMIUM.NORMAL);
	}

	public static string GetZoneActor(EntityDef entityDef, TAG_ZONE zoneTag, TAG_PREMIUM premium)
	{
		return GetZoneActor(entityDef, zoneTag, null, premium);
	}

	private static string GetGraveyardActorForHero(Entity entity)
	{
		Card card = entity.GetCard();
		if (entity.IsHero() && card != null && card.GetPrevZone() is ZoneHero)
		{
			return GetPlayActorByTags(entity, TAG_PREMIUM.NORMAL);
		}
		return GetZoneActor(entity, TAG_ZONE.GRAVEYARD, entity.GetController(), entity.GetPremiumType());
	}

	public static string GetHandActor(TAG_CARDTYPE cardType, TAG_PREMIUM premiumType, string cardId = null)
	{
		switch (cardType)
		{
		case TAG_CARDTYPE.MINION:
			return GetNameWithPremiumType(ACTOR_ASSET.HAND_MINION, premiumType, cardId);
		case TAG_CARDTYPE.WEAPON:
			return GetNameWithPremiumType(ACTOR_ASSET.HAND_WEAPON, premiumType, cardId);
		case TAG_CARDTYPE.SPELL:
		case TAG_CARDTYPE.BATTLEGROUND_QUEST_REWARD:
		case TAG_CARDTYPE.BATTLEGROUND_SPELL:
		case TAG_CARDTYPE.BATTLEGROUND_CLICKABLE_BUTTON:
			return GetNameWithPremiumType(ACTOR_ASSET.HAND_SPELL, premiumType, cardId);
		case TAG_CARDTYPE.HERO:
			return GetNameWithPremiumType(ACTOR_ASSET.HAND_HERO, premiumType, cardId);
		case TAG_CARDTYPE.HERO_POWER:
			return GetNameWithPremiumType(ACTOR_ASSET.HISTORY_HERO_POWER, premiumType, cardId);
		case TAG_CARDTYPE.BATTLEGROUND_HERO_BUDDY:
			return GetNameWithPremiumType(ACTOR_ASSET.HAND_MINION, premiumType, cardId);
		case TAG_CARDTYPE.LOCATION:
			return GetNameWithPremiumType(ACTOR_ASSET.HAND_LOCATION, premiumType, cardId);
		case TAG_CARDTYPE.BATTLEGROUND_ANOMALY:
			return GetNameWithPremiumType(ACTOR_ASSET.BIG_CARD_BG_ANOMALY, premiumType, cardId);
		case TAG_CARDTYPE.BATTLEGROUND_TRINKET:
			return GetNameWithPremiumType(ACTOR_ASSET.BIG_CARD_BG_TRINKET, premiumType, cardId);
		default:
			return "Card_Hidden.prefab:1a94649d257bc284ca6e2962f634a8b9";
		}
	}

	public static string GetHandActorByTags(EntityBase entityBase, TAG_PREMIUM premiumType)
	{
		if (entityBase.IsLettuceMercenary())
		{
			return GetNameWithPremiumType(ACTOR_ASSET.HAND_MERCENARY, premiumType);
		}
		if (entityBase.IsLettuceEquipment())
		{
			return GetNameWithPremiumType(ACTOR_ASSET.BIG_CARD_LETTUCE_EQUIPMENT, premiumType);
		}
		if (entityBase.IsLettuceAbility())
		{
			if (entityBase.IsLettuceAbilityMinionSummoning())
			{
				return GetNameWithPremiumType(ACTOR_ASSET.BIG_CARD_LETTUCE_ABILITY_MINION, premiumType);
			}
			return GetNameWithPremiumType(ACTOR_ASSET.BIG_CARD_LETTUCE_ABILITY_SPELL, premiumType);
		}
		return GetHandActor(entityBase.GetCardType(), premiumType, entityBase.GetCardId());
	}

	public static string GetHandActor(TAG_CARDTYPE cardType)
	{
		return GetHandActor(cardType, TAG_PREMIUM.NORMAL);
	}

	public static string GetHandActor(Entity entity)
	{
		return GetHandActorByTags(entity, entity.GetPremiumType());
	}

	public static string GetHandActor(EntityDef entityDef)
	{
		return GetHandActorByTags(entityDef, TAG_PREMIUM.NORMAL);
	}

	public static string GetHandActor(EntityDef entityDef, TAG_PREMIUM premiumType)
	{
		return GetHandActorByTags(entityDef, premiumType);
	}

	public static string GetHeroSkinOrHandActor(EntityDef entityDef, TAG_PREMIUM premium)
	{
		if (entityDef.GetCardType() == TAG_CARDTYPE.HERO)
		{
			if (entityDef.HasTag(GAME_TAG.HAS_DIAMOND_QUALITY))
			{
				return "Card_Hero_Skin_Diamond.prefab:1a326c865ba7db441afbdaa681e518e5";
			}
			return "Card_Hero_Skin.prefab:ed2af57fa6b571741ab047c2c3e0e663";
		}
		return GetHandActorByTags(entityDef, premium);
	}

	public static string GetHeroSkinOrHandActor(TAG_CARDTYPE type, TAG_PREMIUM premium)
	{
		if (type == TAG_CARDTYPE.HERO)
		{
			if (premium == TAG_PREMIUM.DIAMOND)
			{
				return "Card_Hero_Skin_Diamond.prefab:1a326c865ba7db441afbdaa681e518e5";
			}
			return "Card_Hero_Skin.prefab:ed2af57fa6b571741ab047c2c3e0e663";
		}
		return GetHandActor(type, premium);
	}

	public static string GetNerfGlowsActor(TAG_CARDTYPE cardType)
	{
		return cardType switch
		{
			TAG_CARDTYPE.MINION => "Card_Hand_Ally_NerfGlows.prefab:a693fa02720fcb644b3223d7d75d26eb", 
			TAG_CARDTYPE.SPELL => "Card_Hand_Ability_NerfGlows.prefab:adb8690f5caa2a84eb9431b8f09664db", 
			TAG_CARDTYPE.WEAPON => "Card_Hand_Weapon_NerfGlows.prefab:645b0cbf4d3be464a8e4fe447f6a0dee", 
			TAG_CARDTYPE.HERO => "Card_Hand_Hero_NerfGlows.prefab:6f101676067a4514f8641429c0592adc", 
			TAG_CARDTYPE.LOCATION => "Card_Hand_Location_NerfGlows.prefab:32299cf99a8ea8541b06329fb1961c71", 
			_ => "Card_Hand_Ability_NerfGlows.prefab:adb8690f5caa2a84eb9431b8f09664db", 
		};
	}

	public static string GetPlayActorByTags(EntityBase entityBase, TAG_PREMIUM premiumType)
	{
		Player player = null;
		if (GameState.Get() != null && entityBase != null)
		{
			player = GameState.Get().GetPlayer(entityBase.GetControllerId());
		}
		switch (entityBase.GetCardType())
		{
		case TAG_CARDTYPE.MINION:
			if (entityBase.IsLettuceMercenary())
			{
				return GetNameWithPremiumType(ACTOR_ASSET.PLAY_MERCENARY, premiumType);
			}
			return GetNameWithPremiumType(ACTOR_ASSET.PLAY_MINION, premiumType, entityBase.GetCardId());
		case TAG_CARDTYPE.WEAPON:
			return GetNameWithPremiumType(ACTOR_ASSET.PLAY_WEAPON, premiumType, entityBase.GetCardId(), player);
		case TAG_CARDTYPE.SPELL:
			return "Card_Invisible.prefab:579b3b9a80234754593f24582f9cb93b";
		case TAG_CARDTYPE.BATTLEGROUND_SPELL:
			return GetNameWithPremiumType(ACTOR_ASSET.PLAY_BATTLEGROUND_SPELL, premiumType, entityBase.GetCardId());
		case TAG_CARDTYPE.BATTLEGROUND_TRINKET:
			return GetNameWithPremiumType(ACTOR_ASSET.PLAY_BATTLEGROUND_TRINKET, premiumType, entityBase.GetCardId());
		case TAG_CARDTYPE.BATTLEGROUND_CLICKABLE_BUTTON:
			return GetNameWithPremiumType(ACTOR_ASSET.PLAY_BATTLEGROUND_CLICKABLE_BUTTON, premiumType, entityBase.GetCardId());
		case TAG_CARDTYPE.HERO:
			if (entityBase.HasTag(GAME_TAG.BACON_IS_KEL_THUZAD) || entityBase.HasTag(GAME_TAG.BACON_PLAYER_RESULTS_HERO_OVERRIDE))
			{
				return "Card_Play_Bacon_Hero.prefab:227eb40f91281fa429c48c8a730c982f";
			}
			return GameUtils.GetHeroType(entityBase.GetCardId()) switch
			{
				CardHero.HeroType.BATTLEGROUNDS_HERO => "Card_Play_Bacon_Hero.prefab:227eb40f91281fa429c48c8a730c982f", 
				CardHero.HeroType.BATTLEGROUNDS_GUIDE => "Card_Play_Bacon_Guide.prefab:6cf6c56b1ef6f4c4db7210533b95f4ac", 
				_ => GetNameWithPremiumType(ACTOR_ASSET.PLAY_HERO, premiumType, entityBase.GetCardId()), 
			};
		case TAG_CARDTYPE.HERO_POWER:
			return GetNameWithPremiumType(ACTOR_ASSET.PLAY_HERO_POWER, premiumType, null, player);
		case TAG_CARDTYPE.ENCHANTMENT:
			return "Card_Play_Enchantment.prefab:cc1eafed24951ee4c92ad007507b1b69";
		case TAG_CARDTYPE.GAME_MODE_BUTTON:
			return GetNameWithPremiumType(ACTOR_ASSET.PLAY_GAME_MODE_BUTTON, premiumType);
		case TAG_CARDTYPE.MOVE_MINION_HOVER_TARGET:
			return GetNameWithPremiumType(ACTOR_ASSET.PLAY_MOVE_MINION_HOVER_TARGET, premiumType);
		case TAG_CARDTYPE.BATTLEGROUND_HERO_BUDDY:
			return GetNameWithPremiumType(ACTOR_ASSET.PLAY_BATTLEGROUND_HERO_BUDDY, premiumType);
		case TAG_CARDTYPE.BATTLEGROUND_ANOMALY:
			return GetNameWithPremiumType(ACTOR_ASSET.PLAY_BATTLEGROUND_ANOMALY, premiumType);
		case TAG_CARDTYPE.BATTLEGROUND_QUEST_REWARD:
			return GetNameWithPremiumType(ACTOR_ASSET.PLAY_BATTLEGROUND_QUEST_REWARD, premiumType);
		case TAG_CARDTYPE.LETTUCE_ABILITY:
			if (entityBase.IsLettuceEquipment())
			{
				return GetNameWithPremiumType(ACTOR_ASSET.PLAY_LETTUCE_EQUIPMENT, premiumType);
			}
			if (entityBase.IsLettuceAbilityMinionSummoning())
			{
				return GetNameWithPremiumType(ACTOR_ASSET.PLAY_LETTUCE_ABILITY_MINION, premiumType);
			}
			return GetNameWithPremiumType(ACTOR_ASSET.PLAY_LETTUCE_ABILITY_SPELL, premiumType);
		case TAG_CARDTYPE.LOCATION:
			return GetNameWithPremiumType(ACTOR_ASSET.PLAY_LOCATION, premiumType, entityBase.GetCardId());
		default:
			return "Card_Hidden.prefab:1a94649d257bc284ca6e2962f634a8b9";
		}
	}

	public static string GetPlayActor(EntityDef entityDef, TAG_PREMIUM premiumType)
	{
		return GetPlayActorByTags(entityDef, premiumType);
	}

	public static string GetBigCardActor(Entity entity)
	{
		return GetHistoryActor(entity, HistoryInfoType.NONE);
	}

	public static bool ShouldDisplayTooltipInsteadOfBigCard(Entity entity)
	{
		if (entity.GetCardType() != TAG_CARDTYPE.GAME_MODE_BUTTON && !entity.IsBobQuest())
		{
			if (entity.IsBattlegroundTrinket())
			{
				return entity.HasTag(GAME_TAG.BACON_IS_POTENTIAL_TRINKET);
			}
			return false;
		}
		return true;
	}

	public static string GetHistoryActor(Entity entity, HistoryInfoType historyTileType)
	{
		if (entity.IsHiddenSecret())
		{
			return GetHistorySecretActor(entity);
		}
		if (entity.IsHiddenForge())
		{
			return GetHistoryForgeActor(entity);
		}
		if (string.IsNullOrEmpty(entity.GetCardId()))
		{
			return "Card_Hidden.prefab:1a94649d257bc284ca6e2962f634a8b9";
		}
		TAG_CARDTYPE cardType = entity.GetCardType();
		TAG_PREMIUM premium = entity.GetPremiumType();
		switch (cardType)
		{
		case TAG_CARDTYPE.HERO_POWER:
			if (entity.GetController().IsFriendlySide())
			{
				return GetNameWithPremiumType(ACTOR_ASSET.HISTORY_HERO_POWER, premium, null, entity.GetController());
			}
			return GetNameWithPremiumType(ACTOR_ASSET.HISTORY_HERO_POWER_OPPONENT, premium, null, entity.GetController());
		case TAG_CARDTYPE.HERO:
			if ((entity.GetZone() != TAG_ZONE.PLAY || historyTileType == HistoryInfoType.CARD_PLAYED) && entity.GetEntityDef().GetCardSet() != TAG_CARD_SET.HERO_SKINS)
			{
				return GetNameWithPremiumType(ACTOR_ASSET.HAND_HERO, premium, entity.GetCardId());
			}
			if (premium == TAG_PREMIUM.SIGNATURE)
			{
				return "History_Hero_Signature.prefab:ee3e9cdfbb2f84041803a7f3066f0fdb";
			}
			return "History_Hero.prefab:a040b63fa76fd4348b2a41b3bdc9789c";
		default:
			return GetHandActor(entity);
		}
	}

	public static string GetHistorySecretActor(Entity entity)
	{
		TAG_CLASS classTag = entity.GetClass();
		switch (classTag)
		{
		case TAG_CLASS.HUNTER:
			return "History_Secret_Hunter.prefab:5e8dcf274b20d714abaec2a80904d83e";
		case TAG_CLASS.MAGE:
			return "History_Secret_Mage.prefab:6efbdae2809ad704ab794654d8bf2156";
		case TAG_CLASS.PALADIN:
			return "History_Secret_Paladin.prefab:158dc4838feed994db5c6d8e6cb7792b";
		case TAG_CLASS.ROGUE:
			return "History_Secret_Rogue.prefab:c827cbea9c33b7c45967ec3281c012cf";
		default:
			if (entity.IsDarkWandererSecret())
			{
				return "History_Secret_Wanderer.prefab:7b140cf72c157604899f60f60bb37bd8";
			}
			Debug.LogWarning(string.Format("ActorNames.GetHistorySecretActor() - No actor for class {0}. Returning {1} instead.", classTag, "History_Secret_Mage.prefab:6efbdae2809ad704ab794654d8bf2156"));
			return "History_Secret_Mage.prefab:6efbdae2809ad704ab794654d8bf2156";
		}
	}

	public static string GetHistoryForgeActor(Entity entity)
	{
		return "History_Forged.prefab:7cb6975d37805d84a92af3aba4d0f12d";
	}

	public static string GetNameWithPremiumType(ACTOR_ASSET actorName, TAG_PREMIUM premiumType, string cardId = null, Entity player = null)
	{
		string actorAsset = null;
		if (player != null)
		{
			CornerReplacementSpellType type = player.GetTag<CornerReplacementSpellType>(GAME_TAG.CORNER_REPLACEMENT_TYPE);
			actorAsset = CornerReplacementConfig.Get().GetActor(type, actorName, premiumType);
			if (actorAsset != null)
			{
				return actorAsset;
			}
		}
		if (!string.IsNullOrEmpty(cardId))
		{
			EntityDef entityDef = DefLoader.Get().GetEntityDef(cardId);
			if (entityDef != null)
			{
				CornerReplacementSpellType type2 = entityDef.GetTag<CornerReplacementSpellType>(GAME_TAG.CORNER_REPLACEMENT_TYPE);
				actorAsset = CornerReplacementConfig.Get().GetActor(type2, actorName, premiumType);
				if (actorAsset != null)
				{
					return actorAsset;
				}
			}
		}
		switch (premiumType)
		{
		case TAG_PREMIUM.DIAMOND:
			if (s_diamondActorAssets.TryGetValue(actorName, out actorAsset))
			{
				return actorAsset;
			}
			goto case TAG_PREMIUM.SIGNATURE;
		case TAG_PREMIUM.SIGNATURE:
			if (cardId != null)
			{
				string signatureActor = GetSignatureActor(cardId, actorName);
				if (!string.IsNullOrEmpty(signatureActor))
				{
					return signatureActor;
				}
			}
			goto case TAG_PREMIUM.GOLDEN;
		case TAG_PREMIUM.GOLDEN:
			if (s_premiumActorAssets.TryGetValue(actorName, out actorAsset))
			{
				return actorAsset;
			}
			break;
		}
		if (s_actorAssets.TryGetValue(actorName, out actorAsset))
		{
			return actorAsset;
		}
		return null;
	}

	private static string GetSignatureActor(string cardId, ACTOR_ASSET actorName)
	{
		if (!SignatureFrames.TryGetValue(cardId, out var cardDbId))
		{
			Debug.LogError("Signature frame for " + cardId + " not found.");
			return null;
		}
		switch (actorName)
		{
		case ACTOR_ASSET.HAND_MINION:
		case ACTOR_ASSET.HAND_SPELL:
		case ACTOR_ASSET.HAND_WEAPON:
		case ACTOR_ASSET.HAND_HERO:
		{
			if (SignatureHandMinions.TryGetValue(cardDbId, out var signatureActor2))
			{
				return signatureActor2;
			}
			Debug.LogError("No Signature Hand frame registered for " + cardId + ".");
			return null;
		}
		case ACTOR_ASSET.PLAY_MINION:
		case ACTOR_ASSET.PLAY_HERO:
		{
			if (SignaturePlayMinions.TryGetValue(cardDbId, out var signatureActor3))
			{
				return signatureActor3;
			}
			Debug.LogError("No Signature Play frame registered for " + cardId + ".");
			return null;
		}
		case ACTOR_ASSET.PLAY_WEAPON:
		{
			if (!SignaturePlayMinions.TryGetValue(cardDbId, out var _))
			{
				Debug.LogError("No Signature Weapon frame registered for " + cardId + ".");
				return null;
			}
			break;
		}
		}
		Debug.LogError("Possible card type/Signature conflict for " + cardId + ".");
		return null;
	}

	public static int GetSignatureFrameId(string cardId)
	{
		if (!string.IsNullOrEmpty(cardId) && SignatureFrames.TryGetValue(cardId, out var frameId))
		{
			return frameId;
		}
		return 0;
	}

	public static bool SignatureFrameHasPowersText(string cardId)
	{
		if (GetSignatureFrameId(cardId) < 2)
		{
			return true;
		}
		return false;
	}

	public static List<int> GetSignatureFrameIds()
	{
		return new List<int>(SignatureFrames.Values);
	}

	private static void Initialize()
	{
		if (m_signatureFrames == null)
		{
			m_signatureFrames = new Dictionary<string, int>();
		}
		if (m_signatureHandMinions == null)
		{
			m_signatureHandMinions = new Dictionary<int, string>();
		}
		if (m_signaturePlayMinions == null)
		{
			m_signaturePlayMinions = new Dictionary<int, string>();
		}
		if (m_signatureSpells == null)
		{
			m_signatureSpells = new Dictionary<int, string>();
		}
		if (m_signatureHeroes == null)
		{
			m_signatureHeroes = new Dictionary<int, string>();
		}
		if (!GameDbf.IsLoaded)
		{
			GameDbf.LoadXml();
		}
		foreach (SignatureFrameDbfRecord frameRecord in GameDbf.SignatureFrame.GetRecords())
		{
			foreach (SignatureCardDbfRecord card in frameRecord.Cards)
			{
				string cardId = GameUtils.TranslateDbIdToCardId(card.CardId);
				if (!m_signatureFrames.TryAdd(cardId, frameRecord.ID))
				{
					Debug.LogError("Failed to add the card id('" + cardId + "') to signatureFrame.");
				}
			}
			if (!m_signatureHandMinions.TryAdd(frameRecord.ID, frameRecord.HandActorPrefab))
			{
				Debug.LogError($"Failed to add the frame record id('{frameRecord.ID}') to signatureHandMinions.");
			}
			if (!m_signaturePlayMinions.TryAdd(frameRecord.ID, frameRecord.PlayActorPrefab))
			{
				Debug.LogError($"Failed to add the frame record id('{frameRecord.ID}') to signaturePlayMinions.");
			}
			if (!m_signatureSpells.TryAdd(frameRecord.ID, frameRecord.HandActorPrefab))
			{
				Debug.LogError($"Failed to add the frame record id('{frameRecord.ID}') to signatureSpells.");
			}
			if (!m_signatureHeroes.TryAdd(frameRecord.ID, frameRecord.HandActorPrefab))
			{
				Debug.LogError($"Failed to add the frame record id('{frameRecord.ID}') to signatureHeroes.");
			}
		}
	}
}
