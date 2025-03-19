using System;
using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.Game.Spells;
using UnityEngine;

public class SpellUtils
{
	public static SpellClassTag ConvertClassTagToSpellEnum(TAG_CLASS classTag)
	{
		return classTag switch
		{
			TAG_CLASS.DEATHKNIGHT => SpellClassTag.DEATHKNIGHT, 
			TAG_CLASS.DRUID => SpellClassTag.DRUID, 
			TAG_CLASS.HUNTER => SpellClassTag.HUNTER, 
			TAG_CLASS.MAGE => SpellClassTag.MAGE, 
			TAG_CLASS.PALADIN => SpellClassTag.PALADIN, 
			TAG_CLASS.PRIEST => SpellClassTag.PRIEST, 
			TAG_CLASS.ROGUE => SpellClassTag.ROGUE, 
			TAG_CLASS.SHAMAN => SpellClassTag.SHAMAN, 
			TAG_CLASS.WARLOCK => SpellClassTag.WARLOCK, 
			TAG_CLASS.WARRIOR => SpellClassTag.WARRIOR, 
			_ => SpellClassTag.NONE, 
		};
	}

	public static Player.Side ConvertSpellSideToPlayerSide(Spell spell, SpellPlayerSide spellSide)
	{
		Card sourceCard = spell.GetSourceCard();
		Entity sourceEntity = ((sourceCard != null) ? sourceCard.GetEntity() : null);
		switch (spellSide)
		{
		case SpellPlayerSide.FRIENDLY:
			return Player.Side.FRIENDLY;
		case SpellPlayerSide.OPPONENT:
			return Player.Side.OPPOSING;
		case SpellPlayerSide.SOURCE:
			if (sourceEntity == null)
			{
				Log.Gameplay.PrintError("sourceEntity null for spell: {0}", spell.name);
				return Player.Side.NEUTRAL;
			}
			if (sourceEntity.IsControlledByFriendlySidePlayer())
			{
				return Player.Side.FRIENDLY;
			}
			return Player.Side.OPPOSING;
		case SpellPlayerSide.TARGET:
			if (sourceEntity == null)
			{
				Log.Gameplay.PrintError("sourceEntity null for spell: {0}", spell.name);
				return Player.Side.NEUTRAL;
			}
			if (sourceEntity.IsControlledByFriendlySidePlayer())
			{
				return Player.Side.OPPOSING;
			}
			return Player.Side.FRIENDLY;
		default:
			return Player.Side.NEUTRAL;
		}
	}

	public static List<Zone> FindZonesFromTag(SpellZoneTag zoneTag)
	{
		ZoneMgr zm = ZoneMgr.Get();
		if (zm == null)
		{
			return null;
		}
		switch (zoneTag)
		{
		case SpellZoneTag.PLAY:
			return zm.FindZonesOfType<Zone, ZonePlay>();
		case SpellZoneTag.HERO:
			return zm.FindZonesOfType<Zone, ZoneHero>();
		case SpellZoneTag.HERO_POWER:
			return zm.FindZonesOfType<Zone, ZoneHeroPower>();
		case SpellZoneTag.WEAPON:
			return zm.FindZonesOfType<Zone, ZoneWeapon>();
		case SpellZoneTag.HERO_BUDDY:
			return zm.FindZonesOfType<Zone, ZoneBattlegroundHeroBuddy>();
		case SpellZoneTag.QUEST_REWARD:
			return zm.FindZonesOfType<Zone, ZoneBattlegroundQuestReward>();
		case SpellZoneTag.TRINKET:
			return zm.FindZonesOfType<Zone, ZoneBattlegroundTrinket>();
		case SpellZoneTag.DECK:
			return zm.FindZonesOfType<Zone, ZoneDeck>();
		case SpellZoneTag.HAND:
			return zm.FindZonesOfType<Zone, ZoneHand>();
		case SpellZoneTag.GRAVEYARD:
			return zm.FindZonesOfType<Zone, ZoneGraveyard>();
		case SpellZoneTag.SECRET:
			return zm.FindZonesOfType<Zone, ZoneSecret>();
		default:
			Debug.LogWarning($"SpellUtils.FindZonesFromTag() - unhandled zoneTag {zoneTag}");
			return null;
		}
	}

	public static List<Zone> FindZonesFromTag(Spell spell, SpellZoneTag zoneTag, SpellPlayerSide spellSide)
	{
		if (ZoneMgr.Get() == null)
		{
			return null;
		}
		switch (spellSide)
		{
		case SpellPlayerSide.NEUTRAL:
			return null;
		case SpellPlayerSide.BOTH:
			return FindZonesFromTag(zoneTag);
		default:
		{
			Player.Side playerSide = ConvertSpellSideToPlayerSide(spell, spellSide);
			switch (zoneTag)
			{
			case SpellZoneTag.PLAY:
				return ZoneMgr.Get().FindZonesOfType<Zone, ZonePlay>(playerSide);
			case SpellZoneTag.HERO:
				return ZoneMgr.Get().FindZonesOfType<Zone, ZoneHero>(playerSide);
			case SpellZoneTag.HERO_POWER:
				return ZoneMgr.Get().FindZonesOfType<Zone, ZoneHeroPower>(playerSide);
			case SpellZoneTag.WEAPON:
				return ZoneMgr.Get().FindZonesOfType<Zone, ZoneWeapon>(playerSide);
			case SpellZoneTag.HERO_BUDDY:
				return ZoneMgr.Get().FindZonesOfType<Zone, ZoneBattlegroundHeroBuddy>(playerSide);
			case SpellZoneTag.QUEST_REWARD:
				return ZoneMgr.Get().FindZonesOfType<Zone, ZoneBattlegroundQuestReward>(playerSide);
			case SpellZoneTag.TRINKET:
				return ZoneMgr.Get().FindZonesOfType<Zone, ZoneBattlegroundTrinket>(playerSide);
			case SpellZoneTag.DECK:
				return ZoneMgr.Get().FindZonesOfType<Zone, ZoneDeck>(playerSide);
			case SpellZoneTag.HAND:
				return ZoneMgr.Get().FindZonesOfType<Zone, ZoneHand>(playerSide);
			case SpellZoneTag.GRAVEYARD:
				return ZoneMgr.Get().FindZonesOfType<Zone, ZoneGraveyard>(playerSide);
			case SpellZoneTag.SECRET:
				return ZoneMgr.Get().FindZonesOfType<Zone, ZoneSecret>(playerSide);
			default:
				Debug.LogWarning($"SpellUtils.FindZonesFromTag() - Unhandled zoneTag {zoneTag}. spellSide={spellSide} playerSide={playerSide}");
				return null;
			}
		}
		}
	}

	public static Transform GetLocationTransform(Spell spell)
	{
		GameObject locationObject = GetLocationObject(spell);
		if (!(locationObject == null))
		{
			return locationObject.transform;
		}
		return null;
	}

	public static GameObject GetLocationObject(Spell spell)
	{
		SpellLocation location = spell.Location;
		return GetSpellLocationObject(spell, location);
	}

	public static GameObject GetSpellLocationObject(Spell spell, SpellLocation location, string overrideTransformName = null)
	{
		if (location == SpellLocation.NONE)
		{
			return null;
		}
		GameObject locationObject = null;
		if (location == SpellLocation.SOURCE)
		{
			locationObject = spell.GetSource();
		}
		else if (location == SpellLocation.SOURCE_AUTO)
		{
			locationObject = FindSourceAutoObjectForSpell(spell);
		}
		else if (location == SpellLocation.SOURCE_HERO)
		{
			Card sourceCard = spell.GetSourceCard();
			Card lettuceAbilityOwnerCard = sourceCard?.GetEntity().GetLettuceAbilityOwner()?.GetCard();
			if (lettuceAbilityOwnerCard != null)
			{
				locationObject = lettuceAbilityOwnerCard.gameObject;
			}
			Card sourceHeroCard = FindHeroCard(sourceCard);
			if (sourceHeroCard != null)
			{
				locationObject = sourceHeroCard.gameObject;
			}
		}
		else if (location == SpellLocation.SOURCE_HERO_POWER)
		{
			Card sourceHeroPowerCard = FindHeroPowerCard(spell.GetSourceCard());
			if (sourceHeroPowerCard == null)
			{
				return null;
			}
			locationObject = sourceHeroPowerCard.gameObject;
		}
		else if (location == SpellLocation.SOURCE_PLAY_ZONE)
		{
			Card sourceCard2 = spell.GetSourceCard();
			if (sourceCard2 == null)
			{
				return null;
			}
			Player sourcePlayer = sourceCard2.GetEntity().GetController();
			ZonePlay zonePlay = ZoneMgr.Get().FindZoneOfType<ZonePlay>(sourcePlayer.GetSide());
			if (zonePlay == null)
			{
				return null;
			}
			locationObject = zonePlay.gameObject;
		}
		else if (location == SpellLocation.SOURCE_HAND_ZONE)
		{
			Card sourceCard3 = spell.GetSourceCard();
			if (sourceCard3 == null)
			{
				return null;
			}
			Player sourcePlayer2 = sourceCard3.GetEntity().GetController();
			ZoneHand zoneHand = ZoneMgr.Get().FindZoneOfType<ZoneHand>(sourcePlayer2.GetSide());
			if (zoneHand == null)
			{
				return null;
			}
			locationObject = zoneHand.gameObject;
		}
		else if (location == SpellLocation.SOURCE_DECK_ZONE)
		{
			Card sourceCard4 = spell.GetSourceCard();
			if (sourceCard4 == null)
			{
				return null;
			}
			Player sourcePlayer3 = sourceCard4.GetEntity().GetController();
			ZoneDeck zoneDeck = ZoneMgr.Get().FindZoneOfType<ZoneDeck>(sourcePlayer3.GetSide());
			if (zoneDeck == null)
			{
				return null;
			}
			locationObject = zoneDeck.gameObject;
		}
		else if (location == SpellLocation.TARGET)
		{
			locationObject = spell.GetVisualTarget();
		}
		else if (location == SpellLocation.TARGET_AUTO)
		{
			locationObject = FindTargetAutoObjectForSpell(spell);
		}
		else if (location == SpellLocation.TARGET_HERO)
		{
			Card targetHeroCard = FindHeroCard(spell.GetVisualTargetCard());
			if (targetHeroCard == null)
			{
				return null;
			}
			locationObject = targetHeroCard.gameObject;
		}
		else if (location == SpellLocation.TARGET_HERO_POWER)
		{
			Card targetHeroPowerCard = FindHeroPowerCard(spell.GetVisualTargetCard());
			if (targetHeroPowerCard == null)
			{
				return null;
			}
			locationObject = targetHeroPowerCard.gameObject;
		}
		else if (location == SpellLocation.TARGET_PLAY_ZONE)
		{
			Card targetCard = spell.GetVisualTargetCard();
			if (targetCard == null)
			{
				return null;
			}
			Player targetPlayer = targetCard.GetEntity().GetController();
			ZonePlay zonePlay2 = ZoneMgr.Get().FindZoneOfType<ZonePlay>(targetPlayer.GetSide());
			if (zonePlay2 == null)
			{
				return null;
			}
			locationObject = zonePlay2.gameObject;
		}
		else if (location == SpellLocation.TARGET_HAND_ZONE)
		{
			Card targetCard2 = spell.GetVisualTargetCard();
			if (targetCard2 == null)
			{
				return null;
			}
			Player targetPlayer2 = targetCard2.GetEntity().GetController();
			ZoneHand zoneHand2 = ZoneMgr.Get().FindZoneOfType<ZoneHand>(targetPlayer2.GetSide());
			if (zoneHand2 == null)
			{
				return null;
			}
			locationObject = zoneHand2.gameObject;
		}
		else if (location == SpellLocation.TARGET_DECK_ZONE)
		{
			Card targetCard3 = spell.GetVisualTargetCard();
			if (targetCard3 == null)
			{
				return null;
			}
			Player targetPlayer3 = targetCard3.GetEntity().GetController();
			ZoneDeck zoneDeck2 = ZoneMgr.Get().FindZoneOfType<ZoneDeck>(targetPlayer3.GetSide());
			if (zoneDeck2 == null)
			{
				return null;
			}
			locationObject = zoneDeck2.gameObject;
		}
		else if (location == SpellLocation.BOARD)
		{
			if (Board.Get() == null)
			{
				return null;
			}
			locationObject = Board.Get().gameObject;
		}
		else if (location == SpellLocation.FRIENDLY_HERO)
		{
			Player player = FindFriendlyPlayer(spell);
			if (player == null)
			{
				return null;
			}
			Card heroCard = player.GetHeroCard();
			if (!heroCard)
			{
				return null;
			}
			locationObject = heroCard.gameObject;
		}
		else if (location == SpellLocation.FRIENDLY_HERO_POWER)
		{
			Player player2 = FindFriendlyPlayer(spell);
			if (player2 == null)
			{
				return null;
			}
			Card heroPowerCard = player2.GetHeroPowerCard();
			if (!heroPowerCard)
			{
				return null;
			}
			locationObject = heroPowerCard.gameObject;
		}
		else if (location == SpellLocation.FRIENDLY_PLAY_ZONE)
		{
			ZonePlay zonePlay3 = FindFriendlyPlayZone(spell);
			if (!zonePlay3)
			{
				return null;
			}
			locationObject = zonePlay3.gameObject;
		}
		else if (location == SpellLocation.OPPONENT_HERO)
		{
			Player player3 = FindOpponentPlayer(spell);
			if (player3 == null)
			{
				return null;
			}
			Card heroCard2 = player3.GetHeroCard();
			if (!heroCard2)
			{
				return null;
			}
			locationObject = heroCard2.gameObject;
		}
		else if (location == SpellLocation.OPPONENT_HERO_POWER)
		{
			Player player4 = FindOpponentPlayer(spell);
			if (player4 == null)
			{
				return null;
			}
			Card heroPowerCard2 = player4.GetHeroPowerCard();
			if (!heroPowerCard2)
			{
				return null;
			}
			locationObject = heroPowerCard2.gameObject;
		}
		else if (location == SpellLocation.OPPONENT_PLAY_ZONE)
		{
			ZonePlay zonePlay4 = FindOpponentPlayZone(spell);
			if (!zonePlay4)
			{
				return null;
			}
			locationObject = zonePlay4.gameObject;
		}
		else if (location == SpellLocation.CHOSEN_TARGET)
		{
			Card targetCard4 = spell.GetPowerTargetCard();
			if (targetCard4 == null)
			{
				return null;
			}
			locationObject = targetCard4.gameObject;
		}
		else if (location == SpellLocation.FRIENDLY_HAND_ZONE)
		{
			Player friendlyPlayer = FindFriendlyPlayer(spell);
			if (friendlyPlayer == null)
			{
				return null;
			}
			ZoneHand zoneHand3 = ZoneMgr.Get().FindZoneOfType<ZoneHand>(friendlyPlayer.GetSide());
			if (!zoneHand3)
			{
				return null;
			}
			locationObject = zoneHand3.gameObject;
		}
		else if (location == SpellLocation.OPPONENT_HAND_ZONE)
		{
			Player opponentPlayer = FindOpponentPlayer(spell);
			if (opponentPlayer == null)
			{
				return null;
			}
			ZoneHand zoneHand4 = ZoneMgr.Get().FindZoneOfType<ZoneHand>(opponentPlayer.GetSide());
			if (!zoneHand4)
			{
				return null;
			}
			locationObject = zoneHand4.gameObject;
		}
		else if (location == SpellLocation.FRIENDLY_DECK_ZONE)
		{
			Player friendlyPlayer2 = FindFriendlyPlayer(spell);
			if (friendlyPlayer2 == null)
			{
				return null;
			}
			ZoneDeck zoneDeck3 = ZoneMgr.Get().FindZoneOfType<ZoneDeck>(friendlyPlayer2.GetSide());
			if (!zoneDeck3)
			{
				return null;
			}
			locationObject = zoneDeck3.gameObject;
		}
		else if (location == SpellLocation.OPPONENT_DECK_ZONE)
		{
			Player opponentPlayer2 = FindOpponentPlayer(spell);
			if (opponentPlayer2 == null)
			{
				return null;
			}
			ZoneDeck zoneDeck4 = ZoneMgr.Get().FindZoneOfType<ZoneDeck>(opponentPlayer2.GetSide());
			if (!zoneDeck4)
			{
				return null;
			}
			locationObject = zoneDeck4.gameObject;
		}
		else if (location == SpellLocation.FRIENDLY_WEAPON)
		{
			Player player5 = FindFriendlyPlayer(spell);
			if (player5 == null)
			{
				return null;
			}
			Card weaponCard = player5.GetWeaponCard();
			if (!weaponCard)
			{
				ZoneWeapon zoneWeapon = ZoneMgr.Get().FindZoneOfType<ZoneWeapon>(player5.GetSide());
				if (!zoneWeapon)
				{
					return null;
				}
				locationObject = zoneWeapon.gameObject;
			}
			else
			{
				locationObject = weaponCard.gameObject;
			}
		}
		else if (location == SpellLocation.OPPONENT_WEAPON)
		{
			Player player6 = FindOpponentPlayer(spell);
			if (player6 == null)
			{
				return null;
			}
			Card weaponCard2 = player6.GetWeaponCard();
			if (!weaponCard2)
			{
				ZoneWeapon zoneWeapon2 = ZoneMgr.Get().FindZoneOfType<ZoneWeapon>(player6.GetSide());
				if (!zoneWeapon2)
				{
					return null;
				}
				locationObject = zoneWeapon2.gameObject;
			}
			else
			{
				locationObject = weaponCard2.gameObject;
			}
		}
		else if (location == SpellLocation.FRIENDLY_HERO_BUDDY)
		{
			Player player7 = FindFriendlyPlayer(spell);
			if (player7 == null)
			{
				return null;
			}
			Card heroBuddyCard = player7.GetHeroBuddyCard();
			if (!heroBuddyCard)
			{
				ZoneBattlegroundHeroBuddy zoneHeroBuddy = ZoneMgr.Get().FindZoneOfType<ZoneBattlegroundHeroBuddy>(player7.GetSide());
				if (!zoneHeroBuddy)
				{
					return null;
				}
				locationObject = zoneHeroBuddy.gameObject;
			}
			else
			{
				locationObject = heroBuddyCard.gameObject;
			}
		}
		else if (location == SpellLocation.OPPONENT_HERO_BUDDY)
		{
			Player player8 = FindOpponentPlayer(spell);
			if (player8 == null)
			{
				return null;
			}
			Card heroBuddyCard2 = player8.GetHeroBuddyCard();
			if (!heroBuddyCard2)
			{
				ZoneBattlegroundHeroBuddy zoneHeroBuddy2 = ZoneMgr.Get().FindZoneOfType<ZoneBattlegroundHeroBuddy>(player8.GetSide());
				if (!zoneHeroBuddy2)
				{
					return null;
				}
				locationObject = zoneHeroBuddy2.gameObject;
			}
			else
			{
				locationObject = heroBuddyCard2.gameObject;
			}
		}
		else if (location == SpellLocation.FRIENDLY_QUEST_REWARD || location == SpellLocation.FRIENDLY_QUEST_REWARD_HERO_POWER || location == SpellLocation.OPPONENT_QUEST_REWARD || location == SpellLocation.OPPONENT_QUEST_REWARD_HERO_POWER)
		{
			Player player9 = ((location == SpellLocation.FRIENDLY_QUEST_REWARD || location == SpellLocation.FRIENDLY_QUEST_REWARD_HERO_POWER) ? FindFriendlyPlayer(spell) : FindOpponentPlayer(spell));
			if (player9 == null)
			{
				return null;
			}
			Card questRewardCard = ((location == SpellLocation.FRIENDLY_QUEST_REWARD || location == SpellLocation.OPPONENT_QUEST_REWARD) ? player9.GetQuestRewardCard() : player9.GetQuestRewardFromHeroPowerCard());
			if (!questRewardCard)
			{
				List<ZoneBattlegroundQuestReward> questRewardZones = ZoneMgr.Get().FindZonesOfType<ZoneBattlegroundQuestReward>(player9.GetSide());
				if (questRewardZones.Count == 0)
				{
					return null;
				}
				foreach (ZoneBattlegroundQuestReward zone in questRewardZones)
				{
					if (zone.m_isHeroPower == (location == SpellLocation.FRIENDLY_QUEST_REWARD_HERO_POWER))
					{
						locationObject = zone.gameObject;
						break;
					}
				}
			}
			else
			{
				locationObject = questRewardCard.gameObject;
			}
		}
		else if (location >= SpellLocation.FRIENDLY_TRINKET_1 && location <= SpellLocation.OPPONENT_TRINKET_HERO_POWER)
		{
			Player player10 = ((location <= SpellLocation.FRIENDLY_TRINKET_HERO_POWER) ? FindFriendlyPlayer(spell) : FindOpponentPlayer(spell));
			if (player10 == null)
			{
				return null;
			}
			List<ZoneBattlegroundTrinket> trinketZones = ZoneMgr.Get().FindZonesOfType<ZoneBattlegroundTrinket>(player10.GetSide());
			if (trinketZones.Count == 0)
			{
				return null;
			}
			SpellLocation zeroIndex = ((location <= SpellLocation.FRIENDLY_TRINKET_HERO_POWER) ? SpellLocation.OPPONENT_QUEST_REWARD_HERO_POWER : SpellLocation.FRIENDLY_TRINKET_HERO_POWER);
			foreach (ZoneBattlegroundTrinket zone2 in trinketZones)
			{
				if (zone2.slot == location - zeroIndex)
				{
					Card card = zone2.GetFirstCard();
					locationObject = ((!(card != null)) ? zone2.gameObject : card.gameObject);
					break;
				}
			}
		}
		if (locationObject == null)
		{
			return null;
		}
		if (string.IsNullOrEmpty(overrideTransformName))
		{
			overrideTransformName = spell.GetLocationTransformName();
		}
		if (!string.IsNullOrEmpty(overrideTransformName))
		{
			GameObject transformObject = GameObjectUtils.FindChildBySubstring(locationObject, overrideTransformName);
			if (transformObject != null)
			{
				return transformObject;
			}
		}
		Card locationObjCard = locationObject.GetComponent<Card>();
		if (locationObjCard != null && locationObjCard.GetEntity() != null)
		{
			Entity locationObjectEnt = locationObjCard.GetEntity();
			if (locationObjectEnt.GetZone() == TAG_ZONE.SETASIDE)
			{
				Zone fallbackZone = ZoneMgr.Get().FindZoneOfType<ZoneHero>(locationObjectEnt.GetControllerSide());
				if (fallbackZone != null)
				{
					locationObject = fallbackZone.gameObject;
				}
			}
		}
		return locationObject;
	}

	public static bool SetPositionFromLocation(Spell spell, bool setParent)
	{
		Transform locationTransform = GetLocationTransform(spell);
		if (locationTransform == null)
		{
			return false;
		}
		if (setParent)
		{
			spell.transform.parent = locationTransform;
		}
		spell.transform.position = locationTransform.position;
		return true;
	}

	public static bool SetOrientationFromFacing(Spell spell)
	{
		SpellFacing facing = spell.GetFacing();
		if (facing == SpellFacing.NONE)
		{
			return false;
		}
		SpellFacingOptions options = spell.GetFacingOptions();
		if (options == null)
		{
			options = new SpellFacingOptions();
		}
		switch (facing)
		{
		case SpellFacing.SAME_AS_SOURCE:
		{
			GameObject source6 = spell.GetSource();
			if (source6 == null)
			{
				return false;
			}
			FaceSameAs(spell, source6, options);
			break;
		}
		case SpellFacing.SAME_AS_SOURCE_AUTO:
		{
			GameObject source2 = FindSourceAutoObjectForSpell(spell);
			if (source2 == null)
			{
				return false;
			}
			FaceSameAs(spell, source2, options);
			break;
		}
		case SpellFacing.SAME_AS_SOURCE_HERO:
		{
			Card sourceHeroCard3 = FindHeroCard(spell.GetSourceCard());
			if (sourceHeroCard3 == null)
			{
				return false;
			}
			FaceSameAs(spell, sourceHeroCard3, options);
			break;
		}
		case SpellFacing.TOWARDS_SOURCE:
		{
			GameObject source = spell.GetSource();
			if (source == null)
			{
				return false;
			}
			FaceTowards(spell, source, options);
			break;
		}
		case SpellFacing.TOWARDS_SOURCE_AUTO:
		{
			GameObject source4 = FindSourceAutoObjectForSpell(spell);
			if (source4 == null)
			{
				return false;
			}
			FaceTowards(spell, source4, options);
			break;
		}
		case SpellFacing.TOWARDS_SOURCE_HERO:
		{
			Card sourceHeroCard = FindHeroCard(spell.GetSourceCard());
			if (sourceHeroCard == null)
			{
				return false;
			}
			FaceTowards(spell, sourceHeroCard, options);
			break;
		}
		case SpellFacing.TOWARDS_TARGET:
		{
			GameObject target = spell.GetVisualTarget();
			if (target == null)
			{
				return false;
			}
			FaceTowards(spell, target, options);
			break;
		}
		case SpellFacing.TOWARDS_TARGET_HERO:
		{
			Card targetHeroCard = FindHeroCard(FindBestTargetCard(spell));
			if (targetHeroCard == null)
			{
				return false;
			}
			FaceTowards(spell, targetHeroCard, options);
			break;
		}
		case SpellFacing.TOWARDS_CHOSEN_TARGET:
		{
			Card targetCard = spell.GetPowerTargetCard();
			if (targetCard == null)
			{
				return false;
			}
			FaceTowards(spell, targetCard, options);
			break;
		}
		case SpellFacing.OPPOSITE_OF_SOURCE:
		{
			GameObject source5 = spell.GetSource();
			if (source5 == null)
			{
				return false;
			}
			FaceOppositeOf(spell, source5, options);
			break;
		}
		case SpellFacing.OPPOSITE_OF_SOURCE_AUTO:
		{
			GameObject source3 = FindSourceAutoObjectForSpell(spell);
			if (source3 == null)
			{
				return false;
			}
			FaceOppositeOf(spell, source3, options);
			break;
		}
		case SpellFacing.OPPOSITE_OF_SOURCE_HERO:
		{
			Card sourceHeroCard2 = FindHeroCard(spell.GetSourceCard());
			if (sourceHeroCard2 == null)
			{
				return false;
			}
			FaceOppositeOf(spell, sourceHeroCard2, options);
			break;
		}
		case SpellFacing.TOWARDS_OPPONENT_HERO:
		{
			Card opponentHeroCard = FindOpponentHeroCard(spell);
			if (opponentHeroCard == null)
			{
				return false;
			}
			FaceTowards(spell, opponentHeroCard, options);
			break;
		}
		default:
			return false;
		}
		return true;
	}

	public static Player FindFriendlyPlayer(Spell spell)
	{
		if (spell == null)
		{
			return null;
		}
		Card sourceCard = spell.GetSourceCard();
		if (sourceCard == null)
		{
			return null;
		}
		return sourceCard.GetEntity().GetController();
	}

	public static Player FindOpponentPlayer(Spell spell)
	{
		Player friendlyPlayer = FindFriendlyPlayer(spell);
		if (friendlyPlayer == null)
		{
			return null;
		}
		return GameState.Get().GetFirstOpponentPlayer(friendlyPlayer);
	}

	public static ZonePlay FindFriendlyPlayZone(Spell spell)
	{
		Player friendlyPlayer = FindFriendlyPlayer(spell);
		if (friendlyPlayer == null)
		{
			return null;
		}
		return ZoneMgr.Get().FindZoneOfType<ZonePlay>(friendlyPlayer.GetSide());
	}

	public static ZonePlay FindOpponentPlayZone(Spell spell)
	{
		Player opponentPlayer = FindOpponentPlayer(spell);
		if (opponentPlayer == null)
		{
			return null;
		}
		return ZoneMgr.Get().FindZoneOfType<ZonePlay>(opponentPlayer.GetSide());
	}

	public static Card FindOpponentHeroCard(Spell spell)
	{
		return FindOpponentPlayer(spell)?.GetHeroCard();
	}

	public static Zone FindTargetZone(Spell spell)
	{
		Card targetCard = spell.GetTargetCard();
		if (targetCard == null)
		{
			return null;
		}
		Entity targetEntity = targetCard.GetEntity();
		return ZoneMgr.Get().FindZoneForEntity(targetEntity);
	}

	public static Actor GetParentActor(Spell spell)
	{
		return GameObjectUtils.FindComponentInThisOrParents<Actor>(spell.gameObject);
	}

	public static GameObject GetParentRootObject(Spell spell)
	{
		Actor actor = GetParentActor(spell);
		if (actor == null)
		{
			return null;
		}
		return actor.GetRootObject();
	}

	public static MeshRenderer GetParentRootObjectMesh(Spell spell)
	{
		Actor actor = GetParentActor(spell);
		if (actor == null)
		{
			return null;
		}
		return actor.GetMeshRenderer();
	}

	public static bool IsNonMetaTaskListInMetaBlock(PowerTaskList taskList)
	{
		if (!taskList.DoesBlockHaveEffectTimingMetaData())
		{
			return false;
		}
		if (taskList.HasEffectTimingMetaData())
		{
			return false;
		}
		return true;
	}

	public static bool CanAddPowerTargets(PowerTaskList taskList)
	{
		if (IsNonMetaTaskListInMetaBlock(taskList))
		{
			return false;
		}
		if (!taskList.HasTasks() && !taskList.IsEndOfBlock())
		{
			return false;
		}
		return true;
	}

	public static void SetCustomSpellParent(ISpell spell, Component c)
	{
		if (spell != null && !(c == null) && !(spell.GameObject == null))
		{
			spell.GameObject.transform.parent = c.transform;
			spell.GameObject.transform.localPosition = Vector3.zero;
		}
	}

	public static Spell LoadAndSetupSpell(string spellPath, Component owner)
	{
		Spell spellComponent = SpellManager.Get().GetSpell(spellPath);
		if (spellComponent == null)
		{
			Error.AddDevFatalUnlessWorkarounds("LoadAndSetupSpell() - \"{0}\" does not have a Spell component.", spellPath);
			return null;
		}
		if (owner != null)
		{
			SetupSpell(spellComponent, owner);
		}
		return spellComponent;
	}

	public static void SetupSpell(ISpell spell, Component c)
	{
		if (spell != null && !(c == null))
		{
			spell.SetSource(c.gameObject);
		}
	}

	public static void SetupSoundSpell(CardSoundSpell spell, Component c)
	{
		if (!(spell == null) && !(c == null))
		{
			spell.SetSource(c.gameObject);
			spell.transform.parent = c.transform;
			TransformUtil.Identity(spell.transform);
		}
	}

	public static bool ActivateStateIfNecessary(Spell spell, SpellStateType state)
	{
		switch (state)
		{
		case SpellStateType.BIRTH:
			return ActivateBirthIfNecessary(spell);
		case SpellStateType.DEATH:
			return ActivateDeathIfNecessary(spell);
		case SpellStateType.CANCEL:
			return ActivateCancelIfNecessary(spell);
		default:
			if (spell != null && spell.GetActiveState() != state)
			{
				spell.ActivateState(state);
				return true;
			}
			return false;
		}
	}

	public static bool ActivateBirthIfNecessary(Spell spell)
	{
		if (spell == null)
		{
			return false;
		}
		switch (spell.GetActiveState())
		{
		case SpellStateType.BIRTH:
			return false;
		case SpellStateType.IDLE:
			return false;
		default:
			spell.ActivateState(SpellStateType.BIRTH);
			return true;
		}
	}

	public static bool ActivateDeathIfNecessary(Spell spell)
	{
		if (spell == null)
		{
			return false;
		}
		switch (spell.GetActiveState())
		{
		case SpellStateType.DEATH:
			return false;
		case SpellStateType.NONE:
			return false;
		default:
			spell.ActivateState(SpellStateType.DEATH);
			return true;
		}
	}

	public static bool ActivateCancelIfNecessary(ISpell spell)
	{
		if (spell == null)
		{
			return false;
		}
		switch (spell.GetActiveState())
		{
		case SpellStateType.CANCEL:
			return false;
		case SpellStateType.NONE:
			return false;
		default:
			spell.ActivateState(SpellStateType.CANCEL);
			return true;
		}
	}

	public static void PurgeSpell(Spell spell)
	{
		if (!(spell == null) && spell.CanPurge())
		{
			SpellManager.Get()?.ReleaseSpell(spell);
		}
	}

	public static void PurgeSpells<T>(List<T> spells) where T : Spell
	{
		if (spells != null && spells.Count != 0)
		{
			for (int i = 0; i < spells.Count; i++)
			{
				PurgeSpell(spells[i]);
			}
		}
	}

	private static GameObject FindSourceAutoObjectForSpell(Spell spell)
	{
		GameObject obj = spell.GetSource();
		Card card = spell.GetSourceCard();
		if (card == null)
		{
			return obj;
		}
		Entity entity = card.GetEntity();
		TAG_CARDTYPE cardType = entity.GetCardType();
		PowerTaskList taskList = spell.GetPowerTaskList();
		if (taskList != null)
		{
			EntityDef entityDef = taskList.GetEffectEntityDef();
			if (entityDef != null)
			{
				cardType = entityDef.GetCardType();
			}
		}
		return FindAutoObjectForSpell(entity, card, cardType);
	}

	private static GameObject FindTargetAutoObjectForSpell(Spell spell)
	{
		GameObject obj = spell.GetVisualTarget();
		if (obj == null)
		{
			return null;
		}
		Card card = obj.GetComponent<Card>();
		if (card == null)
		{
			return obj;
		}
		Entity entity = card.GetEntity();
		TAG_CARDTYPE cardType = entity.GetCardType();
		return FindAutoObjectForSpell(entity, card, cardType);
	}

	private static GameObject FindAutoObjectForSpell(Entity entity, Card card, TAG_CARDTYPE cardType)
	{
		switch (cardType)
		{
		case TAG_CARDTYPE.SPELL:
		{
			Card heroCard = entity.GetController().GetHeroCard();
			if (heroCard != null)
			{
				return heroCard.gameObject;
			}
			Card lettuceAbilityOwnerCard = entity.GetLettuceAbilityOwner()?.GetCard();
			if (lettuceAbilityOwnerCard != null)
			{
				return lettuceAbilityOwnerCard.gameObject;
			}
			return card.gameObject;
		}
		case TAG_CARDTYPE.HERO_POWER:
		{
			Card heroPowerCard = entity.GetController().GetHeroPowerCard();
			if (heroPowerCard == null)
			{
				return card.gameObject;
			}
			return heroPowerCard.gameObject;
		}
		case TAG_CARDTYPE.ENCHANTMENT:
		{
			Entity attachedEntity = GameState.Get().GetEntity(entity.GetAttached());
			if (attachedEntity != null)
			{
				Card attachedCard = attachedEntity.GetCard();
				if (attachedCard != null)
				{
					return attachedCard.gameObject;
				}
			}
			break;
		}
		}
		return card.gameObject;
	}

	private static Card FindBestTargetCard(Spell spell)
	{
		Card sourceCard = spell.GetSourceCard();
		if (sourceCard == null)
		{
			return spell.GetVisualTargetCard();
		}
		Player sourcePlayer = sourceCard.GetEntity().GetController();
		if (sourcePlayer == null)
		{
			return spell.GetVisualTargetCard();
		}
		Player.Side sourcePlayerSide = sourcePlayer.GetSide();
		List<GameObject> targets = spell.GetVisualTargets();
		for (int i = 0; i < targets.Count; i++)
		{
			Card targetCard = targets[i].GetComponent<Card>();
			if (!(targetCard == null) && targetCard.GetEntity().GetController().GetSide() != sourcePlayerSide)
			{
				return targetCard;
			}
		}
		return spell.GetVisualTargetCard();
	}

	private static Card FindHeroCard(Card card)
	{
		if (card == null)
		{
			return null;
		}
		return card.GetEntity().GetController()?.GetHeroCard();
	}

	private static Card FindHeroPowerCard(Card card)
	{
		if (card == null)
		{
			return null;
		}
		return card.GetEntity().GetController()?.GetHeroPowerCard();
	}

	private static void FaceSameAs(Component source, GameObject target, SpellFacingOptions options)
	{
		FaceSameAs(source.transform, target.transform, options);
	}

	private static void FaceSameAs(Component source, Component target, SpellFacingOptions options)
	{
		FaceSameAs(source.transform, target.transform, options);
	}

	private static void FaceSameAs(Transform source, Transform target, SpellFacingOptions options)
	{
		SetOrientation(source, target.position, target.position + target.forward, options);
	}

	private static void FaceOppositeOf(Component source, GameObject target, SpellFacingOptions options)
	{
		FaceOppositeOf(source.transform, target.transform, options);
	}

	private static void FaceOppositeOf(Component source, Component target, SpellFacingOptions options)
	{
		FaceOppositeOf(source.transform, target.transform, options);
	}

	private static void FaceOppositeOf(Transform source, Transform target, SpellFacingOptions options)
	{
		SetOrientation(source, target.position, target.position - target.forward, options);
	}

	private static void FaceTowards(Component source, GameObject target, SpellFacingOptions options)
	{
		FaceTowards(source.transform, target.transform, options);
	}

	private static void FaceTowards(Component source, Component target, SpellFacingOptions options)
	{
		FaceTowards(source.transform, target.transform, options);
	}

	private static void FaceTowards(Transform source, Transform target, SpellFacingOptions options)
	{
		SetOrientation(source, source.position, target.position, options);
	}

	private static void SetOrientation(Transform source, Vector3 sourcePosition, Vector3 targetPosition, SpellFacingOptions options)
	{
		if (!options.m_RotateX || !options.m_RotateY)
		{
			if (options.m_RotateX)
			{
				targetPosition.x = sourcePosition.x;
			}
			else
			{
				if (!options.m_RotateY)
				{
					return;
				}
				targetPosition.y = sourcePosition.y;
			}
		}
		Vector3 forward = targetPosition - sourcePosition;
		if (forward.sqrMagnitude > Mathf.Epsilon)
		{
			source.rotation = Quaternion.LookRotation(forward);
		}
	}

	public static T GetAppropriateElementAccordingToRanges<T>(T[] elements, Func<T, ValueRange> rangeAccessor, int desiredValue)
	{
		if (elements.Length == 0)
		{
			return default(T);
		}
		int arrayMaxIndex = -1;
		int maxHandledValue = int.MinValue;
		int arrayMinIndex = -1;
		int minHandledValue = int.MaxValue;
		int defaultIndex = -1;
		int i = 0;
		for (int iMax = elements.Length; i < iMax; i++)
		{
			T element = elements[i];
			int maxVal = rangeAccessor(element).m_maxValue;
			if (maxVal > maxHandledValue)
			{
				maxHandledValue = maxVal;
				arrayMaxIndex = i;
			}
			int minVal = rangeAccessor(element).m_minValue;
			if (minVal < minHandledValue)
			{
				minHandledValue = minVal;
				arrayMinIndex = i;
			}
			if (defaultIndex == -1 && desiredValue >= minVal && desiredValue <= maxVal)
			{
				defaultIndex = i;
			}
		}
		if (desiredValue > maxHandledValue && arrayMaxIndex != -1)
		{
			return elements[arrayMaxIndex];
		}
		if (desiredValue < minHandledValue && arrayMinIndex != -1)
		{
			return elements[arrayMinIndex];
		}
		if (defaultIndex != -1)
		{
			return elements[defaultIndex];
		}
		return default(T);
	}

	public static IEnumerator FlipActorAndReplaceWithCard(Actor actor, Card card, float time)
	{
		float halfTime = time * 0.5f;
		card.HideCard();
		Hashtable args = iTweenManager.Get().GetTweenHashTable();
		args.Add("z", 90f);
		args.Add("time", halfTime);
		args.Add("easetype", iTween.EaseType.linear);
		args.Add("name", "SpellUtils.FlipActorAndReplaceWithCard");
		iTween.RotateAdd(actor.gameObject, args);
		while (iTween.HasName(actor.gameObject, "SpellUtils.FlipActorAndReplaceWithCard"))
		{
			yield return null;
		}
		TransformUtil.CopyWorld(card, actor);
		card.transform.rotation *= Quaternion.Euler(0f, 0f, 180f);
		actor.Hide();
		card.ShowCard();
		args = iTweenManager.Get().GetTweenHashTable();
		args.Add("z", 90f);
		args.Add("time", halfTime);
		args.Add("easetype", iTween.EaseType.linear);
		args.Add("name", "SpellUtils.FlipActorAndReplaceWithCard");
		iTween.RotateAdd(card.gameObject, args);
		while (iTween.HasName(card.gameObject, "SpellUtils.FlipActorAndReplaceWithCard"))
		{
			yield return null;
		}
	}

	public static IEnumerator FlipActorAndReplaceWithOtherActor(Actor actor, Actor otherActor, float time)
	{
		float halfTime = time * 0.5f;
		otherActor.Hide();
		Hashtable args = iTweenManager.Get().GetTweenHashTable();
		args.Add("z", 90f);
		args.Add("time", halfTime);
		args.Add("easetype", iTween.EaseType.linear);
		args.Add("name", "SpellUtils.FlipActorAndReplaceWithOtherActor");
		iTween.RotateAdd(actor.gameObject, args);
		while (iTween.HasName(actor.gameObject, "SpellUtils.FlipActorAndReplaceWithOtherActor"))
		{
			yield return null;
		}
		TransformUtil.CopyWorld(otherActor, actor);
		otherActor.transform.rotation *= Quaternion.Euler(0f, 0f, 180f);
		actor.Hide();
		otherActor.Show();
		args = iTweenManager.Get().GetTweenHashTable();
		args.Add("z", 90f);
		args.Add("time", halfTime);
		args.Add("easetype", iTween.EaseType.linear);
		args.Add("name", "SpellUtils.FlipActorAndReplaceWithOtherActor");
		iTween.RotateAdd(otherActor.gameObject, args);
		while (iTween.HasName(otherActor.gameObject, "SpellUtils.FlipActorAndReplaceWithOtherActor"))
		{
			yield return null;
		}
	}
}
