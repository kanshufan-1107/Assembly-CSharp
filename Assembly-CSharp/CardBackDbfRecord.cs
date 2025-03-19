using System;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class CardBackDbfRecord : DbfRecord
{
	public const int TotalCollectedToAutoFavorite = 3;

	public const int ClassicCardBackId = 0;

	public const int SamsungCardBackId = 29;

	public const int RandomCardBackId = 158;

	[SerializeField]
	private long m_data1;

	[SerializeField]
	private Assets.CardBack.Source m_source = Assets.CardBack.ParseSourceValue("unknown");

	[SerializeField]
	private bool m_enabled;

	[SerializeField]
	private Assets.CardBack.SortCategory m_sortCategory;

	[SerializeField]
	private int m_sortOrder;

	[SerializeField]
	private string m_prefabName;

	[SerializeField]
	private DbfLocValue m_name;

	[SerializeField]
	private DbfLocValue m_description;

	[SerializeField]
	private bool m_isRandomCardBack;

	[SerializeField]
	private int m_collectionManagerPurchaseProductId;

	[DbfField("DATA1")]
	public long Data1 => m_data1;

	[DbfField("SOURCE")]
	public Assets.CardBack.Source Source => m_source;

	[DbfField("ENABLED")]
	public bool Enabled => m_enabled;

	[DbfField("SORT_CATEGORY")]
	public Assets.CardBack.SortCategory SortCategory => m_sortCategory;

	[DbfField("SORT_ORDER")]
	public int SortOrder => m_sortOrder;

	[DbfField("PREFAB_NAME")]
	public string PrefabName => m_prefabName;

	[DbfField("NAME")]
	public DbfLocValue Name => m_name;

	[DbfField("DESCRIPTION")]
	public DbfLocValue Description => m_description;

	[DbfField("IS_RANDOM_CARD_BACK")]
	public bool IsRandomCardBack => m_isRandomCardBack;

	[DbfField("COLLECTION_MANAGER_PURCHASE_PRODUCT_ID")]
	public int CollectionManagerPurchaseProductId => m_collectionManagerPurchaseProductId;

	public override object GetVar(string name)
	{
		return name switch
		{
			"ID" => base.ID, 
			"DATA1" => m_data1, 
			"SOURCE" => m_source, 
			"ENABLED" => m_enabled, 
			"SORT_CATEGORY" => m_sortCategory, 
			"SORT_ORDER" => m_sortOrder, 
			"PREFAB_NAME" => m_prefabName, 
			"NAME" => m_name, 
			"DESCRIPTION" => m_description, 
			"IS_RANDOM_CARD_BACK" => m_isRandomCardBack, 
			"COLLECTION_MANAGER_PURCHASE_PRODUCT_ID" => m_collectionManagerPurchaseProductId, 
			_ => null, 
		};
	}

	public override void SetVar(string name, object val)
	{
		switch (name)
		{
		case "ID":
			SetID((int)val);
			break;
		case "DATA1":
			m_data1 = (long)val;
			break;
		case "SOURCE":
			if (val == null)
			{
				m_source = Assets.CardBack.Source.STARTUP;
			}
			else if (val is Assets.CardBack.Source || val is int)
			{
				m_source = (Assets.CardBack.Source)val;
			}
			else if (val is string)
			{
				m_source = Assets.CardBack.ParseSourceValue((string)val);
			}
			break;
		case "ENABLED":
			m_enabled = (bool)val;
			break;
		case "SORT_CATEGORY":
			if (val == null)
			{
				m_sortCategory = Assets.CardBack.SortCategory.NONE;
			}
			else if (val is Assets.CardBack.SortCategory || val is int)
			{
				m_sortCategory = (Assets.CardBack.SortCategory)val;
			}
			else if (val is string)
			{
				m_sortCategory = Assets.CardBack.ParseSortCategoryValue((string)val);
			}
			break;
		case "SORT_ORDER":
			m_sortOrder = (int)val;
			break;
		case "PREFAB_NAME":
			m_prefabName = (string)val;
			break;
		case "NAME":
			m_name = (DbfLocValue)val;
			break;
		case "DESCRIPTION":
			m_description = (DbfLocValue)val;
			break;
		case "IS_RANDOM_CARD_BACK":
			m_isRandomCardBack = (bool)val;
			break;
		case "COLLECTION_MANAGER_PURCHASE_PRODUCT_ID":
			m_collectionManagerPurchaseProductId = (int)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"DATA1" => typeof(long), 
			"SOURCE" => typeof(Assets.CardBack.Source), 
			"ENABLED" => typeof(bool), 
			"SORT_CATEGORY" => typeof(Assets.CardBack.SortCategory), 
			"SORT_ORDER" => typeof(int), 
			"PREFAB_NAME" => typeof(string), 
			"NAME" => typeof(DbfLocValue), 
			"DESCRIPTION" => typeof(DbfLocValue), 
			"IS_RANDOM_CARD_BACK" => typeof(bool), 
			"COLLECTION_MANAGER_PURCHASE_PRODUCT_ID" => typeof(int), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadCardBackDbfRecords loadRecords = new LoadCardBackDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		CardBackDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(CardBackDbfAsset)) as CardBackDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"CardBackDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
			return false;
		}
		for (int i = 0; i < dbfAsset.Records.Count; i++)
		{
			dbfAsset.Records[i].StripUnusedLocales();
		}
		records = dbfAsset.Records as List<T>;
		return true;
	}

	public override bool SaveRecordsToAsset<T>(string assetPath, List<T> records)
	{
		return false;
	}

	public override void StripUnusedLocales()
	{
		m_name.StripUnusedLocales();
		m_description.StripUnusedLocales();
	}
}
