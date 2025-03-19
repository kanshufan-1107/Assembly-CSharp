using System;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class OwnershipReqListDbfRecord : DbfRecord
{
	[SerializeField]
	private int m_deckTemplateId;

	[SerializeField]
	private OwnershipReqList.ReqTypes m_reqType;

	[SerializeField]
	private int m_reqCardId;

	[DbfField("DECK_TEMPLATE_ID")]
	public int DeckTemplateId => m_deckTemplateId;

	[DbfField("REQ_TYPE")]
	public OwnershipReqList.ReqTypes ReqType => m_reqType;

	[DbfField("REQ_CARD_ID")]
	public int ReqCardId => m_reqCardId;

	public override object GetVar(string name)
	{
		return name switch
		{
			"ID" => base.ID, 
			"DECK_TEMPLATE_ID" => m_deckTemplateId, 
			"REQ_TYPE" => m_reqType, 
			"REQ_CARD_ID" => m_reqCardId, 
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
		case "REQ_TYPE":
			if (val == null)
			{
				m_reqType = OwnershipReqList.ReqTypes.NONE;
			}
			else if (val is OwnershipReqList.ReqTypes || val is int)
			{
				m_reqType = (OwnershipReqList.ReqTypes)val;
			}
			else if (val is string)
			{
				m_reqType = OwnershipReqList.ParseReqTypesValue((string)val);
			}
			break;
		case "REQ_CARD_ID":
			m_reqCardId = (int)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"DECK_TEMPLATE_ID" => typeof(int), 
			"REQ_TYPE" => typeof(OwnershipReqList.ReqTypes), 
			"REQ_CARD_ID" => typeof(int), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadOwnershipReqListDbfRecords loadRecords = new LoadOwnershipReqListDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		OwnershipReqListDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(OwnershipReqListDbfAsset)) as OwnershipReqListDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"OwnershipReqListDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
