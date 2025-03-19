using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class RelatedCardsDbfRecord : DbfRecord
{
	[SerializeField]
	private int m_cardId;

	[SerializeField]
	private int m_relatedCardDatabaseId;

	[DbfField("CARD_ID")]
	public int CardId => m_cardId;

	[DbfField("RELATED_CARD_DATABASE_ID")]
	public int RelatedCardDatabaseId => m_relatedCardDatabaseId;

	public override object GetVar(string name)
	{
		return name switch
		{
			"ID" => base.ID, 
			"CARD_ID" => m_cardId, 
			"RELATED_CARD_DATABASE_ID" => m_relatedCardDatabaseId, 
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
		case "CARD_ID":
			m_cardId = (int)val;
			break;
		case "RELATED_CARD_DATABASE_ID":
			m_relatedCardDatabaseId = (int)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"CARD_ID" => typeof(int), 
			"RELATED_CARD_DATABASE_ID" => typeof(int), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadRelatedCardsDbfRecords loadRecords = new LoadRelatedCardsDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		RelatedCardsDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(RelatedCardsDbfAsset)) as RelatedCardsDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"RelatedCardsDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
