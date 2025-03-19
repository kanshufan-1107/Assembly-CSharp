using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class MercenaryArtVariationDbfRecord : DbfRecord
{
	[SerializeField]
	private int m_lettuceMercenaryId;

	[SerializeField]
	private int m_cardId;

	[SerializeField]
	private bool m_defaultVariation;

	[DbfField("LETTUCE_MERCENARY_ID")]
	public int LettuceMercenaryId => m_lettuceMercenaryId;

	[DbfField("CARD_ID")]
	public int CardId => m_cardId;

	public CardDbfRecord CardRecord => GameDbf.Card.GetRecord(m_cardId);

	[DbfField("DEFAULT_VARIATION")]
	public bool DefaultVariation => m_defaultVariation;

	public List<MercenaryArtVariationPremiumDbfRecord> MercenaryArtVariationPremiums
	{
		get
		{
			int id = base.ID;
			List<MercenaryArtVariationPremiumDbfRecord> returnRecords = new List<MercenaryArtVariationPremiumDbfRecord>();
			List<MercenaryArtVariationPremiumDbfRecord> records = GameDbf.MercenaryArtVariationPremium.GetRecords();
			int i = 0;
			for (int iMax = records.Count; i < iMax; i++)
			{
				MercenaryArtVariationPremiumDbfRecord record = records[i];
				if (record.MercenaryArtVariationId == id)
				{
					returnRecords.Add(record);
				}
			}
			return returnRecords;
		}
	}

	public override object GetVar(string name)
	{
		return name switch
		{
			"ID" => base.ID, 
			"LETTUCE_MERCENARY_ID" => m_lettuceMercenaryId, 
			"CARD_ID" => m_cardId, 
			"DEFAULT_VARIATION" => m_defaultVariation, 
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
		case "LETTUCE_MERCENARY_ID":
			m_lettuceMercenaryId = (int)val;
			break;
		case "CARD_ID":
			m_cardId = (int)val;
			break;
		case "DEFAULT_VARIATION":
			m_defaultVariation = (bool)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"LETTUCE_MERCENARY_ID" => typeof(int), 
			"CARD_ID" => typeof(int), 
			"DEFAULT_VARIATION" => typeof(bool), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadMercenaryArtVariationDbfRecords loadRecords = new LoadMercenaryArtVariationDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		MercenaryArtVariationDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(MercenaryArtVariationDbfAsset)) as MercenaryArtVariationDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"MercenaryArtVariationDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
