using System;

public class Tags
{
	public static string DebugTag(int tag, int val)
	{
		string tagName = tag.ToString();
		try
		{
			GAME_TAG gAME_TAG = (GAME_TAG)tag;
			tagName = gAME_TAG.ToString();
		}
		catch (Exception)
		{
		}
		string valName = val.ToString();
		switch ((GAME_TAG)tag)
		{
		case GAME_TAG.STATE:
			try
			{
				TAG_STATE tAG_STATE = (TAG_STATE)val;
				valName = tAG_STATE.ToString();
			}
			catch (Exception)
			{
			}
			break;
		case GAME_TAG.ZONE:
			try
			{
				TAG_ZONE tAG_ZONE = (TAG_ZONE)val;
				valName = tAG_ZONE.ToString();
			}
			catch (Exception)
			{
			}
			break;
		case GAME_TAG.STEP:
		case GAME_TAG.NEXT_STEP:
			try
			{
				TAG_STEP tAG_STEP = (TAG_STEP)val;
				valName = tAG_STEP.ToString();
			}
			catch (Exception)
			{
			}
			break;
		case GAME_TAG.PLAYSTATE:
			try
			{
				TAG_PLAYSTATE tAG_PLAYSTATE = (TAG_PLAYSTATE)val;
				valName = tAG_PLAYSTATE.ToString();
			}
			catch (Exception)
			{
			}
			break;
		case GAME_TAG.CARDTYPE:
			try
			{
				TAG_CARDTYPE tAG_CARDTYPE = (TAG_CARDTYPE)val;
				valName = tAG_CARDTYPE.ToString();
			}
			catch (Exception)
			{
			}
			break;
		case GAME_TAG.MULLIGAN_STATE:
			try
			{
				TAG_MULLIGAN tAG_MULLIGAN = (TAG_MULLIGAN)val;
				valName = tAG_MULLIGAN.ToString();
			}
			catch (Exception)
			{
			}
			break;
		case GAME_TAG.CLASS:
			try
			{
				TAG_CLASS tAG_CLASS = (TAG_CLASS)val;
				valName = tAG_CLASS.ToString();
			}
			catch (Exception)
			{
			}
			break;
		case GAME_TAG.FACTION:
			try
			{
				TAG_FACTION tAG_FACTION = (TAG_FACTION)val;
				valName = tAG_FACTION.ToString();
			}
			catch (Exception)
			{
			}
			break;
		case GAME_TAG.CARDRACE:
			try
			{
				TAG_RACE tAG_RACE = (TAG_RACE)val;
				valName = tAG_RACE.ToString();
			}
			catch (Exception)
			{
			}
			break;
		case GAME_TAG.RARITY:
			try
			{
				TAG_RARITY tAG_RARITY = (TAG_RARITY)val;
				valName = tAG_RARITY.ToString();
			}
			catch (Exception)
			{
			}
			break;
		case GAME_TAG.ENCHANTMENT_BIRTH_VISUAL:
		case GAME_TAG.ENCHANTMENT_IDLE_VISUAL:
			try
			{
				TAG_ENCHANTMENT_VISUAL tAG_ENCHANTMENT_VISUAL = (TAG_ENCHANTMENT_VISUAL)val;
				valName = tAG_ENCHANTMENT_VISUAL.ToString();
			}
			catch (Exception)
			{
			}
			break;
		case GAME_TAG.CARD_SET:
			try
			{
				TAG_CARD_SET tAG_CARD_SET = (TAG_CARD_SET)val;
				valName = tAG_CARD_SET.ToString();
			}
			catch (Exception)
			{
			}
			break;
		}
		return $"tag={tagName} value={valName}";
	}
}
