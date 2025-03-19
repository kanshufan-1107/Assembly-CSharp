using System;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class LuckyDrawRewardsDbfRecord : DbfRecord
{
	[SerializeField]
	private int m_luckyDrawBoxId;

	[SerializeField]
	private int m_rewardListId;

	[SerializeField]
	private LuckyDrawRewards.LuckyDrawStyle m_style;

	[DbfField("REWARD_LIST_ID")]
	public int RewardListId => m_rewardListId;

	public RewardListDbfRecord RewardListRecord => GameDbf.RewardList.GetRecord(m_rewardListId);

	[DbfField("STYLE")]
	public LuckyDrawRewards.LuckyDrawStyle Style => m_style;

	public override object GetVar(string name)
	{
		return name switch
		{
			"ID" => base.ID, 
			"LUCKY_DRAW_BOX_ID" => m_luckyDrawBoxId, 
			"REWARD_LIST_ID" => m_rewardListId, 
			"STYLE" => m_style, 
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
		case "LUCKY_DRAW_BOX_ID":
			m_luckyDrawBoxId = (int)val;
			break;
		case "REWARD_LIST_ID":
			m_rewardListId = (int)val;
			break;
		case "STYLE":
			if (val == null)
			{
				m_style = LuckyDrawRewards.LuckyDrawStyle.COMMON;
			}
			else if (val is LuckyDrawRewards.LuckyDrawStyle || val is int)
			{
				m_style = (LuckyDrawRewards.LuckyDrawStyle)val;
			}
			else if (val is string)
			{
				m_style = LuckyDrawRewards.ParseLuckyDrawStyleValue((string)val);
			}
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"LUCKY_DRAW_BOX_ID" => typeof(int), 
			"REWARD_LIST_ID" => typeof(int), 
			"STYLE" => typeof(LuckyDrawRewards.LuckyDrawStyle), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadLuckyDrawRewardsDbfRecords loadRecords = new LoadLuckyDrawRewardsDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		LuckyDrawRewardsDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(LuckyDrawRewardsDbfAsset)) as LuckyDrawRewardsDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"LuckyDrawRewardsDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
