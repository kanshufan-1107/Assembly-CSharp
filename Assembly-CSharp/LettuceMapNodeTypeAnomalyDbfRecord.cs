using System;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class LettuceMapNodeTypeAnomalyDbfRecord : DbfRecord
{
	[SerializeField]
	private int m_lettuceMapNodeTypeId;

	[SerializeField]
	private int m_anomalyCardId;

	[SerializeField]
	private LettuceMapNodeTypeAnomaly.MercenariesBonusRewardType m_bonusRewardType;

	[DbfField("ANOMALY_CARD")]
	public int AnomalyCard => m_anomalyCardId;

	[DbfField("BONUS_REWARD_TYPE")]
	public LettuceMapNodeTypeAnomaly.MercenariesBonusRewardType BonusRewardType => m_bonusRewardType;

	public override object GetVar(string name)
	{
		return name switch
		{
			"ID" => base.ID, 
			"LETTUCE_MAP_NODE_TYPE_ID" => m_lettuceMapNodeTypeId, 
			"ANOMALY_CARD" => m_anomalyCardId, 
			"BONUS_REWARD_TYPE" => m_bonusRewardType, 
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
		case "LETTUCE_MAP_NODE_TYPE_ID":
			m_lettuceMapNodeTypeId = (int)val;
			break;
		case "ANOMALY_CARD":
			m_anomalyCardId = (int)val;
			break;
		case "BONUS_REWARD_TYPE":
			if (val == null)
			{
				m_bonusRewardType = LettuceMapNodeTypeAnomaly.MercenariesBonusRewardType.NONE;
			}
			else if (val is LettuceMapNodeTypeAnomaly.MercenariesBonusRewardType || val is int)
			{
				m_bonusRewardType = (LettuceMapNodeTypeAnomaly.MercenariesBonusRewardType)val;
			}
			else if (val is string)
			{
				m_bonusRewardType = LettuceMapNodeTypeAnomaly.ParseMercenariesBonusRewardTypeValue((string)val);
			}
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"LETTUCE_MAP_NODE_TYPE_ID" => typeof(int), 
			"ANOMALY_CARD" => typeof(int), 
			"BONUS_REWARD_TYPE" => typeof(LettuceMapNodeTypeAnomaly.MercenariesBonusRewardType), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadLettuceMapNodeTypeAnomalyDbfRecords loadRecords = new LoadLettuceMapNodeTypeAnomalyDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		LettuceMapNodeTypeAnomalyDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(LettuceMapNodeTypeAnomalyDbfAsset)) as LettuceMapNodeTypeAnomalyDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"LettuceMapNodeTypeAnomalyDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
