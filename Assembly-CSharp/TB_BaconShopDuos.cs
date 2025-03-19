using UnityEngine;

public class TB_BaconShopDuos : TB_BaconShop
{
	public override bool ShouldUseAlternateNameForPlayer(Player.Side side)
	{
		if (side != Player.Side.OPPOSING && !IsBattlePhase())
		{
			if (TeammateBoardViewer.Get() != null)
			{
				return TeammateBoardViewer.Get().IsViewingTeammate();
			}
			return false;
		}
		return true;
	}

	protected override void EnemyEmoteHandlerDoneLoadingCallback(AssetReference assetRef, GameObject go, object callbackData)
	{
		base.EnemyEmoteHandlerDoneLoadingCallback(assetRef, go, callbackData);
		if (TeammateBoardViewer.Get() != null)
		{
			TeammateBoardViewer.Get().SetEnemyEmoteHandler(go.GetComponent<EnemyEmoteHandler>());
		}
	}

	public override InputManager.ZoneTooltipSettings GetZoneTooltipSettings()
	{
		InputManager.ZoneTooltipSettings zoneTooltipSettings = base.GetZoneTooltipSettings();
		zoneTooltipSettings.ShowFriendlyHandWhenHoveringOpponentDeck = true;
		return zoneTooltipSettings;
	}
}
