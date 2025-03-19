using System;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class LettuceMercenaryDbfRecord : DbfRecord
{
	[SerializeField]
	private string m_noteDesc;

	[SerializeField]
	private int m_rarityId;

	[SerializeField]
	private bool m_collectible;

	[SerializeField]
	private bool m_craftable = true;

	[SerializeField]
	private Assets.LettuceMercenary.Acquiretype m_acquireType;

	[SerializeField]
	private DbfLocValue m_howToAcquireText;

	[SerializeField]
	private int m_coinCraftCost = 50;

	[DbfField("NOTE_DESC")]
	public string NoteDesc => m_noteDesc;

	[DbfField("RARITY")]
	public int Rarity => m_rarityId;

	[DbfField("COLLECTIBLE")]
	public bool Collectible => m_collectible;

	[DbfField("CRAFTABLE")]
	public bool Craftable => m_craftable;

	[DbfField("ACQUIRE_TYPE")]
	public Assets.LettuceMercenary.Acquiretype AcquireType => m_acquireType;

	[DbfField("HOW_TO_ACQUIRE_TEXT")]
	public DbfLocValue HowToAcquireText => m_howToAcquireText;

	[DbfField("COIN_CRAFT_COST")]
	public int CoinCraftCost => m_coinCraftCost;

	public List<LettuceMercenaryEquipmentDbfRecord> LettuceMercenaryEquipment
	{
		get
		{
			int id = base.ID;
			List<LettuceMercenaryEquipmentDbfRecord> returnRecords = new List<LettuceMercenaryEquipmentDbfRecord>();
			List<LettuceMercenaryEquipmentDbfRecord> records = GameDbf.LettuceMercenaryEquipment.GetRecords();
			int i = 0;
			for (int iMax = records.Count; i < iMax; i++)
			{
				LettuceMercenaryEquipmentDbfRecord record = records[i];
				if (record.LettuceMercenaryId == id)
				{
					returnRecords.Add(record);
				}
			}
			return returnRecords;
		}
	}

	public List<LettuceMercenarySpecializationDbfRecord> LettuceMercenarySpecializations
	{
		get
		{
			int id = base.ID;
			List<LettuceMercenarySpecializationDbfRecord> returnRecords = new List<LettuceMercenarySpecializationDbfRecord>();
			List<LettuceMercenarySpecializationDbfRecord> records = GameDbf.LettuceMercenarySpecialization.GetRecords();
			int i = 0;
			for (int iMax = records.Count; i < iMax; i++)
			{
				LettuceMercenarySpecializationDbfRecord record = records[i];
				if (record.LettuceMercenaryId == id)
				{
					returnRecords.Add(record);
				}
			}
			return returnRecords;
		}
	}

	public List<MercenaryAllowedTreasureDbfRecord> MercenaryTreasure
	{
		get
		{
			int id = base.ID;
			List<MercenaryAllowedTreasureDbfRecord> returnRecords = new List<MercenaryAllowedTreasureDbfRecord>();
			List<MercenaryAllowedTreasureDbfRecord> records = GameDbf.MercenaryAllowedTreasure.GetRecords();
			int i = 0;
			for (int iMax = records.Count; i < iMax; i++)
			{
				MercenaryAllowedTreasureDbfRecord record = records[i];
				if (record.LettuceMercenaryId == id)
				{
					returnRecords.Add(record);
				}
			}
			return returnRecords;
		}
	}

	public List<MercenaryArtVariationDbfRecord> MercenaryArtVariations
	{
		get
		{
			int id = base.ID;
			List<MercenaryArtVariationDbfRecord> returnRecords = new List<MercenaryArtVariationDbfRecord>();
			List<MercenaryArtVariationDbfRecord> records = GameDbf.MercenaryArtVariation.GetRecords();
			int i = 0;
			for (int iMax = records.Count; i < iMax; i++)
			{
				MercenaryArtVariationDbfRecord record = records[i];
				if (record.LettuceMercenaryId == id)
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
			"NOTE_DESC" => m_noteDesc, 
			"RARITY" => m_rarityId, 
			"COLLECTIBLE" => m_collectible, 
			"CRAFTABLE" => m_craftable, 
			"ACQUIRE_TYPE" => m_acquireType, 
			"HOW_TO_ACQUIRE_TEXT" => m_howToAcquireText, 
			"COIN_CRAFT_COST" => m_coinCraftCost, 
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
		case "RARITY":
			m_rarityId = (int)val;
			break;
		case "COLLECTIBLE":
			m_collectible = (bool)val;
			break;
		case "CRAFTABLE":
			m_craftable = (bool)val;
			break;
		case "ACQUIRE_TYPE":
			if (val == null)
			{
				m_acquireType = Assets.LettuceMercenary.Acquiretype.NONE;
			}
			else if (val is Assets.LettuceMercenary.Acquiretype || val is int)
			{
				m_acquireType = (Assets.LettuceMercenary.Acquiretype)val;
			}
			else if (val is string)
			{
				m_acquireType = Assets.LettuceMercenary.ParseAcquiretypeValue((string)val);
			}
			break;
		case "HOW_TO_ACQUIRE_TEXT":
			m_howToAcquireText = (DbfLocValue)val;
			break;
		case "COIN_CRAFT_COST":
			m_coinCraftCost = (int)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"NOTE_DESC" => typeof(string), 
			"RARITY" => typeof(int), 
			"COLLECTIBLE" => typeof(bool), 
			"CRAFTABLE" => typeof(bool), 
			"ACQUIRE_TYPE" => typeof(Assets.LettuceMercenary.Acquiretype), 
			"HOW_TO_ACQUIRE_TEXT" => typeof(DbfLocValue), 
			"COIN_CRAFT_COST" => typeof(int), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadLettuceMercenaryDbfRecords loadRecords = new LoadLettuceMercenaryDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		LettuceMercenaryDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(LettuceMercenaryDbfAsset)) as LettuceMercenaryDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"LettuceMercenaryDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
		m_howToAcquireText.StripUnusedLocales();
	}
}
