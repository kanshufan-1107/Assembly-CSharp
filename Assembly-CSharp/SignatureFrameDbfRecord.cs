using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class SignatureFrameDbfRecord : DbfRecord
{
	[SerializeField]
	private string m_handActorPrefab;

	[SerializeField]
	private string m_playActorPrefab;

	[DbfField("HAND_ACTOR_PREFAB")]
	public string HandActorPrefab => m_handActorPrefab;

	[DbfField("PLAY_ACTOR_PREFAB")]
	public string PlayActorPrefab => m_playActorPrefab;

	public List<SignatureCardDbfRecord> Cards
	{
		get
		{
			int id = base.ID;
			List<SignatureCardDbfRecord> returnRecords = new List<SignatureCardDbfRecord>();
			List<SignatureCardDbfRecord> records = GameDbf.SignatureCard.GetRecords();
			int i = 0;
			for (int iMax = records.Count; i < iMax; i++)
			{
				SignatureCardDbfRecord record = records[i];
				if (record.SignatureFrameId == id)
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
			"HAND_ACTOR_PREFAB" => m_handActorPrefab, 
			"PLAY_ACTOR_PREFAB" => m_playActorPrefab, 
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
		case "HAND_ACTOR_PREFAB":
			m_handActorPrefab = (string)val;
			break;
		case "PLAY_ACTOR_PREFAB":
			m_playActorPrefab = (string)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"HAND_ACTOR_PREFAB" => typeof(string), 
			"PLAY_ACTOR_PREFAB" => typeof(string), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadSignatureFrameDbfRecords loadRecords = new LoadSignatureFrameDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		SignatureFrameDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(SignatureFrameDbfAsset)) as SignatureFrameDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"SignatureFrameDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
