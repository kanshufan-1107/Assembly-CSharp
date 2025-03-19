using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class LettuceBountyFinalRewardsDbfRecord : DbfRecord
{
	[SerializeField]
	private int m_lettuceBountyId;

	[SerializeField]
	private int m_rewardMercenaryId;

	[DbfField("LETTUCE_BOUNTY_ID")]
	public int LettuceBountyId => m_lettuceBountyId;

	[DbfField("REWARD_MERCENARY_ID")]
	public int RewardMercenaryId => m_rewardMercenaryId;

	public override object GetVar(string name)
	{
		return name switch
		{
			"ID" => base.ID, 
			"LETTUCE_BOUNTY_ID" => m_lettuceBountyId, 
			"REWARD_MERCENARY_ID" => m_rewardMercenaryId, 
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
		case "REWARD_MERCENARY_ID":
			m_rewardMercenaryId = (int)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"LETTUCE_BOUNTY_ID" => typeof(int), 
			"REWARD_MERCENARY_ID" => typeof(int), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadLettuceBountyFinalRewardsDbfRecords loadRecords = new LoadLettuceBountyFinalRewardsDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		LettuceBountyFinalRewardsDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(LettuceBountyFinalRewardsDbfAsset)) as LettuceBountyFinalRewardsDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"LettuceBountyFinalRewardsDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
