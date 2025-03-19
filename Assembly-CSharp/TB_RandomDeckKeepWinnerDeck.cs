public class TB_RandomDeckKeepWinnerDeck : MissionEntity
{
	public override void PreloadAssets()
	{
		PreloadSound("tutorial_mission_hero_coin_mouse_away.prefab:6266be3ca0b50a645915b9ea0a59d774");
	}

	public override string GetNameBannerOverride(Player.Side playerSide)
	{
		Player playerBySide = GameState.Get().GetPlayerBySide(playerSide);
		int scriptDataNum1 = playerBySide.GetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_1);
		int scriptDataNum2 = playerBySide.GetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_2);
		int tbRandomDeckTimeID = playerBySide.GetTag(GAME_TAG.TAG_TB_RANDOM_DECK_TIME_ID);
		EntityDef groupTitleDef = DefLoader.Get().GetEntityDef(scriptDataNum1);
		if (groupTitleDef != null)
		{
			string name = groupTitleDef.GetName();
			string groupTitle2 = string.Empty;
			if (scriptDataNum2 != 0)
			{
				groupTitleDef = DefLoader.Get().GetEntityDef(scriptDataNum2);
				if (groupTitleDef != null)
				{
					groupTitle2 = " + " + groupTitleDef.GetName();
				}
			}
			string deckDigits = ((tbRandomDeckTimeID <= 0) ? string.Empty : (" " + tbRandomDeckTimeID));
			return name + groupTitle2 + deckDigits;
		}
		return null;
	}
}
