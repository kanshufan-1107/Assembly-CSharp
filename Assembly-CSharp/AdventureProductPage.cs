using System.Linq;
using Blizzard.T5.AssetManager;
using Hearthstone.DataModels;
using UnityEngine;

public class AdventureProductPage : ProductPage
{
	public override void Open()
	{
		if (m_container != null)
		{
			m_container.OverrideMusic(MusicPlaylistType.Invalid);
		}
		base.Open();
	}

	protected override void OnProductSet()
	{
		base.OnProductSet();
		RewardItemDataModel adventureItem = base.Product.Items.FirstOrDefault((RewardItemDataModel item) => item.ItemType == RewardItemType.ADVENTURE);
		if (adventureItem == null)
		{
			Log.Store.PrintError("No Adventures in Product \"{0}\"", base.Product.Name);
			return;
		}
		using AssetHandle<GameObject> adventureDefPrefab = ShopUtils.LoadStoreAdventurePrefab((AdventureDbId)adventureItem.ItemId);
		StoreAdventureDef adventureDef = (adventureDefPrefab ? adventureDefPrefab.Asset.GetComponent<StoreAdventureDef>() : null);
		if (m_container != null)
		{
			m_container.OverrideMusic(adventureDef ? adventureDef.GetPlaylist() : MusicPlaylistType.Invalid);
		}
	}
}
