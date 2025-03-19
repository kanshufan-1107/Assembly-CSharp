using Hearthstone;
using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

public class BaconFinisherCollectionDetails : BaconVideoCollectionDetails
{
	[SerializeField]
	private VisualController m_favoriteButtonController;

	public FinisherVideoCaptionDriver Captions;

	private BattlegroundsFinisherDataModel m_dataModel;

	private BattlegroundsFinisherCollectionPageDataModel m_pageDataModel;

	protected override string DebugTextValue => $"Finisher ID: {m_dataModel?.FinisherDbiId}";

	public override void AssignDataModels(IDataModel dataModel, IDataModel pageDataModel)
	{
		m_dataModel = dataModel as BattlegroundsFinisherDataModel;
		m_pageDataModel = pageDataModel as BattlegroundsFinisherCollectionPageDataModel;
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
		EventFunctions.TriggerEvent(m_favoriteButtonController.transform, "DEFAULT_VISIBILITY", TriggerEventParameters.Standard);
		if (Captions != null)
		{
			Captions.OnClose();
		}
	}

	private void ToggleFavoriteButton()
	{
		if (CanToggleFavoriteBattlegroundsStrike(m_dataModel))
		{
			EventFunctions.TriggerEvent(m_favoriteButtonController.transform, "ENABLE_FAVORITE_BUTTON", TriggerEventParameters.Standard);
		}
		else
		{
			EventFunctions.TriggerEvent(m_favoriteButtonController.transform, "DISABLE_FAVORITE_BUTTON", TriggerEventParameters.Standard);
		}
	}

	private bool CanToggleFavoriteBattlegroundsStrike(BattlegroundsFinisherDataModel dataModel)
	{
		if (dataModel == null)
		{
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
		NetCache.NetCacheBattlegroundsFinishers netCacheBGFinishers = NetCache.Get().GetNetObject<NetCache.NetCacheBattlegroundsFinishers>();
		if (netCacheBGFinishers == null)
		{
			return false;
		}
		if (netCacheBGFinishers.BattlegroundsFavoriteFinishers == null)
		{
			return false;
		}
		return netCacheBGFinishers.BattlegroundsFavoriteFinishers.Count > 1;
	}

	private void ToggleFavorite()
	{
		if (CanToggleFavoriteBattlegroundsStrike(m_dataModel))
		{
			BattlegroundsFinisherId battlegroundsFinisherId = BattlegroundsFinisherId.FromTrustedValue(m_dataModel.FinisherDbiId);
			if (m_dataModel.IsFavorite)
			{
				ClearFavorite(battlegroundsFinisherId);
			}
			else
			{
				MakeFavorite(battlegroundsFinisherId);
			}
		}
	}

	private void MakeFavorite(BattlegroundsFinisherId battlegroundsFinisherId)
	{
		Network.Get().SetBattlegroundsFavoriteFinisher(battlegroundsFinisherId);
		foreach (BattlegroundsFinisherDataModel iteratedDataModel in m_pageDataModel.FinisherList)
		{
			if (iteratedDataModel == m_dataModel)
			{
				iteratedDataModel.IsFavorite = true;
				break;
			}
		}
	}

	private void ClearFavorite(BattlegroundsFinisherId battlegroundsFinisherId)
	{
		Network.Get().ClearBattlegroundsFavoriteFinisher(battlegroundsFinisherId);
		foreach (BattlegroundsFinisherDataModel iteratedDataModel in m_pageDataModel.FinisherList)
		{
			if (iteratedDataModel == m_dataModel)
			{
				iteratedDataModel.IsFavorite = false;
				break;
			}
		}
	}

	protected override bool ValidateDataModels(IDataModel dataModel, IDataModel pageDataModel)
	{
		if (dataModel is BattlegroundsFinisherDataModel)
		{
			return pageDataModel is BattlegroundsFinisherCollectionPageDataModel;
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
