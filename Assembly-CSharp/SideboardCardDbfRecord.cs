using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class SideboardCardDbfRecord : DbfRecord
{
	[SerializeField]
	private int m_deckCardId;

	[SerializeField]
	private int m_sideboardCardId;

	[DbfField("DECK_CARD_ID")]
	public int DeckCardId => m_deckCardId;

	[DbfField("SIDEBOARD_CARD_ID")]
	public int SideboardCardId => m_sideboardCardId;

	public override object GetVar(string name)
	{
		return name switch
		{
			"ID" => base.ID, 
			"DECK_CARD_ID" => m_deckCardId, 
			"SIDEBOARD_CARD_ID" => m_sideboardCardId, 
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
		case "DECK_CARD_ID":
			m_deckCardId = (int)val;
			break;
		case "SIDEBOARD_CARD_ID":
			m_sideboardCardId = (int)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"DECK_CARD_ID" => typeof(int), 
			"SIDEBOARD_CARD_ID" => typeof(int), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadSideboardCardDbfRecords loadRecords = new LoadSideboardCardDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		SideboardCardDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(SideboardCardDbfAsset)) as SideboardCardDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"SideboardCardDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
