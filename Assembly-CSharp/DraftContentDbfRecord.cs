using System;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class DraftContentDbfRecord : DbfRecord
{
	[SerializeField]
	private string m_noteDesc;

	[SerializeField]
	private int m_slot;

	[SerializeField]
	private int m_deckId;

	[SerializeField]
	private DraftContent.SlotType m_slotType;

	public override object GetVar(string name)
	{
		return name switch
		{
			"NOTE_DESC" => m_noteDesc, 
			"SLOT" => m_slot, 
			"DECK_ID" => m_deckId, 
			"SLOT_TYPE" => m_slotType, 
			_ => null, 
		};
	}

	public override void SetVar(string name, object val)
	{
		switch (name)
		{
		case "NOTE_DESC":
			m_noteDesc = (string)val;
			break;
		case "SLOT":
			m_slot = (int)val;
			break;
		case "DECK_ID":
			m_deckId = (int)val;
			break;
		case "SLOT_TYPE":
			if (val == null)
			{
				m_slotType = DraftContent.SlotType.NONE;
			}
			else if (val is DraftContent.SlotType || val is int)
			{
				m_slotType = (DraftContent.SlotType)val;
			}
			else if (val is string)
			{
				m_slotType = DraftContent.ParseSlotTypeValue((string)val);
			}
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"NOTE_DESC" => typeof(string), 
			"SLOT" => typeof(int), 
			"DECK_ID" => typeof(int), 
			"SLOT_TYPE" => typeof(DraftContent.SlotType), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadDraftContentDbfRecords loadRecords = new LoadDraftContentDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		DraftContentDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(DraftContentDbfAsset)) as DraftContentDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"DraftContentDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
