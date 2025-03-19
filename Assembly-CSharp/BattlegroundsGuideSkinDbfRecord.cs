using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class BattlegroundsGuideSkinDbfRecord : DbfRecord
{
	[SerializeField]
	private bool m_enabled = true;

	[SerializeField]
	private int m_rarityId = 2;

	[SerializeField]
	private int m_skinCardId;

	[DbfField("ENABLED")]
	public bool Enabled => m_enabled;

	[DbfField("RARITY")]
	public int Rarity => m_rarityId;

	[DbfField("SKIN_CARD_ID")]
	public int SkinCardId => m_skinCardId;

	public CardDbfRecord SkinCardRecord => GameDbf.Card.GetRecord(m_skinCardId);

	public void SetEnabled(bool v)
	{
		m_enabled = v;
	}

	public void SetRarity(int v)
	{
		m_rarityId = v;
	}

	public void SetSkinCardId(int v)
	{
		m_skinCardId = v;
	}

	public override object GetVar(string name)
	{
		return name switch
		{
			"ID" => base.ID, 
			"ENABLED" => m_enabled, 
			"RARITY" => m_rarityId, 
			"SKIN_CARD_ID" => m_skinCardId, 
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
		case "ENABLED":
			m_enabled = (bool)val;
			break;
		case "RARITY":
			m_rarityId = (int)val;
			break;
		case "SKIN_CARD_ID":
			m_skinCardId = (int)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"ENABLED" => typeof(bool), 
			"RARITY" => typeof(int), 
			"SKIN_CARD_ID" => typeof(int), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadBattlegroundsGuideSkinDbfRecords loadRecords = new LoadBattlegroundsGuideSkinDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		BattlegroundsGuideSkinDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(BattlegroundsGuideSkinDbfAsset)) as BattlegroundsGuideSkinDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"BattlegroundsGuideSkinDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
