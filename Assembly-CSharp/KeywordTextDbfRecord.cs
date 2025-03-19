using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class KeywordTextDbfRecord : DbfRecord
{
	[SerializeField]
	private string m_noteDesc;

	[SerializeField]
	private int m_tagId;

	[SerializeField]
	private string m_name;

	[SerializeField]
	private string m_text;

	[SerializeField]
	private string m_refText;

	[SerializeField]
	private string m_collectionText;

	[SerializeField]
	private bool m_isCollectionOnly;

	[SerializeField]
	private bool m_autoAddSearchableToken;

	[SerializeField]
	private int m_tooltipInitOrder;

	[DbfField("TAG")]
	public int Tag => m_tagId;

	[DbfField("NAME")]
	public string Name => m_name;

	[DbfField("TEXT")]
	public string Text => m_text;

	[DbfField("REF_TEXT")]
	public string RefText => m_refText;

	[DbfField("COLLECTION_TEXT")]
	public string CollectionText => m_collectionText;

	[DbfField("IS_COLLECTION_ONLY")]
	public bool IsCollectionOnly => m_isCollectionOnly;

	[DbfField("AUTO_ADD_SEARCHABLE_TOKEN")]
	public bool AutoAddSearchableToken => m_autoAddSearchableToken;

	[DbfField("TOOLTIP_INIT_ORDER")]
	public int TooltipInitOrder => m_tooltipInitOrder;

	public override object GetVar(string name)
	{
		return name switch
		{
			"ID" => base.ID, 
			"NOTE_DESC" => m_noteDesc, 
			"TAG" => m_tagId, 
			"NAME" => m_name, 
			"TEXT" => m_text, 
			"REF_TEXT" => m_refText, 
			"COLLECTION_TEXT" => m_collectionText, 
			"IS_COLLECTION_ONLY" => m_isCollectionOnly, 
			"AUTO_ADD_SEARCHABLE_TOKEN" => m_autoAddSearchableToken, 
			"TOOLTIP_INIT_ORDER" => m_tooltipInitOrder, 
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
		case "TAG":
			m_tagId = (int)val;
			break;
		case "NAME":
			m_name = (string)val;
			break;
		case "TEXT":
			m_text = (string)val;
			break;
		case "REF_TEXT":
			m_refText = (string)val;
			break;
		case "COLLECTION_TEXT":
			m_collectionText = (string)val;
			break;
		case "IS_COLLECTION_ONLY":
			m_isCollectionOnly = (bool)val;
			break;
		case "AUTO_ADD_SEARCHABLE_TOKEN":
			m_autoAddSearchableToken = (bool)val;
			break;
		case "TOOLTIP_INIT_ORDER":
			m_tooltipInitOrder = (int)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"NOTE_DESC" => typeof(string), 
			"TAG" => typeof(int), 
			"NAME" => typeof(string), 
			"TEXT" => typeof(string), 
			"REF_TEXT" => typeof(string), 
			"COLLECTION_TEXT" => typeof(string), 
			"IS_COLLECTION_ONLY" => typeof(bool), 
			"AUTO_ADD_SEARCHABLE_TOKEN" => typeof(bool), 
			"TOOLTIP_INIT_ORDER" => typeof(int), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadKeywordTextDbfRecords loadRecords = new LoadKeywordTextDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		KeywordTextDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(KeywordTextDbfAsset)) as KeywordTextDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"KeywordTextDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
	}
}
