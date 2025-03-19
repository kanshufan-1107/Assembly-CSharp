using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class SellableDeckDbfRecord : DbfRecord
{
	[SerializeField]
	private int m_deckTemplateId;

	[SerializeField]
	private int m_boosterId;

	[SerializeField]
	private DbfLocValue m_goldenName;

	public DeckTemplateDbfRecord DeckTemplateRecord => GameDbf.DeckTemplate.GetRecord(m_deckTemplateId);

	[DbfField("BOOSTER_ID")]
	public int BoosterId => m_boosterId;

	public BoosterDbfRecord BoosterRecord => GameDbf.Booster.GetRecord(m_boosterId);

	[DbfField("GOLDEN_NAME")]
	public DbfLocValue GoldenName => m_goldenName;

	public override object GetVar(string name)
	{
		return name switch
		{
			"ID" => base.ID, 
			"DECK_TEMPLATE_ID" => m_deckTemplateId, 
			"BOOSTER_ID" => m_boosterId, 
			"GOLDEN_NAME" => m_goldenName, 
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
		case "DECK_TEMPLATE_ID":
			m_deckTemplateId = (int)val;
			break;
		case "BOOSTER_ID":
			m_boosterId = (int)val;
			break;
		case "GOLDEN_NAME":
			m_goldenName = (DbfLocValue)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"DECK_TEMPLATE_ID" => typeof(int), 
			"BOOSTER_ID" => typeof(int), 
			"GOLDEN_NAME" => typeof(DbfLocValue), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadSellableDeckDbfRecords loadRecords = new LoadSellableDeckDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		SellableDeckDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(SellableDeckDbfAsset)) as SellableDeckDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"SellableDeckDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
		m_goldenName.StripUnusedLocales();
	}
}
