using Hearthstone;
using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

public class BaconBoardCollectionDetails : BaconVideoCollectionDetails
{
	[SerializeField]
	private VisualController m_favoriteButtonController;

	private BattlegroundsBoardSkinDataModel m_dataModel;

	private BattlegroundsBoardSkinCollectionPageDataModel m_pageDataModel;

	protected override string DebugTextValue => $"Board ID: {m_dataModel?.BoardDbiId}";

	public override void AssignDataModels(IDataModel dataModel, IDataModel pageDataModel)
	{
		m_dataModel = dataModel as BattlegroundsBoardSkinDataModel;
		m_pageDataModel = pageDataModel as BattlegroundsBoardSkinCollectionPageDataModel;
		m_widget.BindDataModel(dataModel);
	}

	public override void Show()
	{
		base.Show();
		ToggleFavoriteButton();
	}

	public override void Hide()
	{
		base.Hide();
	}

	private void ToggleFavoriteButton()
	{
		if (CanToggleFavoriteBattlegroundsBoardSkin(m_dataModel))
		{
			EventFunctions.TriggerEvent(m_favoriteButtonController.transform, "ENABLE_FAVORITE_BUTTON", TriggerEventParameters.Standard);
		}
		else
		{
			EventFunctions.TriggerEvent(m_favoriteButtonController.transform, "DISABLE_FAVORITE_BUTTON", TriggerEventParameters.Standard);
		}
	}

	private bool CanToggleFavoriteBattlegroundsBoardSkin(BattlegroundsBoardSkinDataModel dataModel)
	{
		if (dataModel == null)
		{
			Log.CollectionManager.PrintWarning("BaconBoardCollectionDetails.CanToggleFavoriteBattlegroundsBoardSkin - BattlegroundsBoardSkinDataModel is null");
			return false;
		}
		if (!dataModel.IsOwned)
		{
			return false;
		}
		if (!dataModel.IsFavorite)
		{
			return true;
		}
		NetCache.NetCacheBattlegroundsBoardSkins netCacheBGBoardSkins = NetCache.Get().GetNetObject<NetCache.NetCacheBattlegroundsBoardSkins>();
		if (netCacheBGBoardSkins == null)
		{
			return false;
		}
		if (netCacheBGBoardSkins.BattlegroundsFavoriteBoardSkins == null)
		{
			return false;
		}
		return netCacheBGBoardSkins.BattlegroundsFavoriteBoardSkins.Count > 1;
	}

	protected void ToggleFavorite()
	{
		if (m_dataModel == null)
		{
			Log.CollectionManager.PrintWarning("BaconBoardCollectionDetails.ToggleFavorite - BattlegroundsBoardSkinDataModel is null");
		}
		else if (!m_dataModel.IsOwned)
		{
			Log.CollectionManager.PrintWarning(string.Format("{0}.{1} - BattlegroundsBoardSkinDataModel is not owned for DBID {2}", "BaconBoardCollectionDetails", "ToggleFavorite", m_dataModel.BoardDbiId));
		}
		else if (m_dataModel.IsFavorite)
		{
			Network.Get().ClearBattlegroundsFavoriteBoardSkin(BattlegroundsBoardSkinId.FromTrustedValue(m_dataModel.BoardDbiId));
		}
		else
		{
			Network.Get().SetBattlegroundsFavoriteBoardSkin(BattlegroundsBoardSkinId.FromTrustedValue(m_dataModel.BoardDbiId));
		}
	}

	protected override bool ValidateDataModels(IDataModel dataModel, IDataModel pageDataModel)
	{
		if (dataModel is BattlegroundsBoardSkinDataModel)
		{
			return pageDataModel is BattlegroundsBoardSkinCollectionPageDataModel;
		}
		return false;
	}

	protected override void ClearDataModels()
	{
		m_dataModel = null;
		m_pageDataModel = null;
	}

	protected override void DetailsEventListener(string eventName)
	{
		if (!(eventName == "OffDialogClick_code"))
		{
			if (eventName == "ToggleFavorite_code")
			{
				ToggleFavorite();
			}
			else
			{
				Debug.LogWarning("Unrecognized event handled in " + GetType().Name + ": " + eventName);
			}
		}
		else if (CanHide())
		{
			Hide();
		}
	}
}
