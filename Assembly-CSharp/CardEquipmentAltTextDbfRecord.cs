using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class CardEquipmentAltTextDbfRecord : DbfRecord
{
	[SerializeField]
	private int m_cardId;

	[SerializeField]
	private int m_equipmentCardId;

	[SerializeField]
	private int m_altTextIndex;

	[DbfField("CARD_ID")]
	public int CardId => m_cardId;

	public CardDbfRecord EquipmentCardRecord => GameDbf.Card.GetRecord(m_equipmentCardId);

	[DbfField("ALT_TEXT_INDEX")]
	public int AltTextIndex => m_altTextIndex;

	public override object GetVar(string name)
	{
		return name switch
		{
			"ID" => base.ID, 
			"CARD_ID" => m_cardId, 
			"EQUIPMENT_CARD_ID" => m_equipmentCardId, 
			"ALT_TEXT_INDEX" => m_altTextIndex, 
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
		case "EQUIPMENT_CARD_ID":
			m_equipmentCardId = (int)val;
			break;
		case "ALT_TEXT_INDEX":
			m_altTextIndex = (int)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"CARD_ID" => typeof(int), 
			"EQUIPMENT_CARD_ID" => typeof(int), 
			"ALT_TEXT_INDEX" => typeof(int), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadCardEquipmentAltTextDbfRecords loadRecords = new LoadCardEquipmentAltTextDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		CardEquipmentAltTextDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(CardEquipmentAltTextDbfAsset)) as CardEquipmentAltTextDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"CardEquipmentAltTextDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
