using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class SignatureCardDbfRecord : DbfRecord
{
	[SerializeField]
	private int m_signatureFrameId;

	[SerializeField]
	private int m_cardId;

	[DbfField("SIGNATURE_FRAME_ID")]
	public int SignatureFrameId => m_signatureFrameId;

	[DbfField("CARD_ID")]
	public int CardId => m_cardId;

	public override object GetVar(string name)
	{
		return name switch
		{
			"ID" => base.ID, 
			"SIGNATURE_FRAME_ID" => m_signatureFrameId, 
			"CARD_ID" => m_cardId, 
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
		case "SIGNATURE_FRAME_ID":
			m_signatureFrameId = (int)val;
			break;
		case "CARD_ID":
			m_cardId = (int)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"SIGNATURE_FRAME_ID" => typeof(int), 
			"CARD_ID" => typeof(int), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadSignatureCardDbfRecords loadRecords = new LoadSignatureCardDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		SignatureCardDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(SignatureCardDbfAsset)) as SignatureCardDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"SignatureCardDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
