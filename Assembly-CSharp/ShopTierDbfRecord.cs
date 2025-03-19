using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class ShopTierDbfRecord : DbfRecord
{
	[SerializeField]
	private string m_noteDesc;

	[SerializeField]
	private string m_style;

	[SerializeField]
	private string m_tags;

	[SerializeField]
	private DbfLocValue m_header;

	[SerializeField]
	private int m_sortOrder;

	[SerializeField]
	private bool m_disabled;

	public override object GetVar(string name)
	{
		return name switch
		{
			"ID" => base.ID, 
			"NOTE_DESC" => m_noteDesc, 
			"STYLE" => m_style, 
			"TAGS" => m_tags, 
			"HEADER" => m_header, 
			"SORT_ORDER" => m_sortOrder, 
			"DISABLED" => m_disabled, 
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
		case "NOTE_DESC":
			m_noteDesc = (string)val;
			break;
		case "STYLE":
			m_style = (string)val;
			break;
		case "TAGS":
			m_tags = (string)val;
			break;
		case "HEADER":
			m_header = (DbfLocValue)val;
			break;
		case "SORT_ORDER":
			m_sortOrder = (int)val;
			break;
		case "DISABLED":
			m_disabled = (bool)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"NOTE_DESC" => typeof(string), 
			"STYLE" => typeof(string), 
			"TAGS" => typeof(string), 
			"HEADER" => typeof(DbfLocValue), 
			"SORT_ORDER" => typeof(int), 
			"DISABLED" => typeof(bool), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadShopTierDbfRecords loadRecords = new LoadShopTierDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		ShopTierDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(ShopTierDbfAsset)) as ShopTierDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"ShopTierDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
		m_header.StripUnusedLocales();
	}
}
