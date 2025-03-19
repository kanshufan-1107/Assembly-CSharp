using System;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class LettuceBountyFinalRespresentiveRewardsDbfRecord : DbfRecord
{
	[SerializeField]
	private int m_lettuceBountyId;

	[SerializeField]
	private LettuceBountyFinalRespresentiveRewards.RewardType m_rewardType;

	[DbfField("LETTUCE_BOUNTY_ID")]
	public int LettuceBountyId => m_lettuceBountyId;

	[DbfField("REWARD_TYPE")]
	public LettuceBountyFinalRespresentiveRewards.RewardType RewardType => m_rewardType;

	public override object GetVar(string name)
	{
		return name switch
		{
			"ID" => base.ID, 
			"LETTUCE_BOUNTY_ID" => m_lettuceBountyId, 
			"REWARD_TYPE" => m_rewardType, 
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
		case "LETTUCE_BOUNTY_ID":
			m_lettuceBountyId = (int)val;
			break;
		case "REWARD_TYPE":
			if (val == null)
			{
				m_rewardType = LettuceBountyFinalRespresentiveRewards.RewardType.NONE;
			}
			else if (val is LettuceBountyFinalRespresentiveRewards.RewardType || val is int)
			{
				m_rewardType = (LettuceBountyFinalRespresentiveRewards.RewardType)val;
			}
			else if (val is string)
			{
				m_rewardType = LettuceBountyFinalRespresentiveRewards.ParseRewardTypeValue((string)val);
			}
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"LETTUCE_BOUNTY_ID" => typeof(int), 
			"REWARD_TYPE" => typeof(LettuceBountyFinalRespresentiveRewards.RewardType), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadLettuceBountyFinalRespresentiveRewardsDbfRecords loadRecords = new LoadLettuceBountyFinalRespresentiveRewardsDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		LettuceBountyFinalRespresentiveRewardsDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(LettuceBountyFinalRespresentiveRewardsDbfAsset)) as LettuceBountyFinalRespresentiveRewardsDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"LettuceBountyFinalRespresentiveRewardsDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
