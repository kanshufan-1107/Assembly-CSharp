using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class HiddenCardSetsDbfRecord : DbfRecord
{
	[SerializeField]
	private int m_rankedPlaySeasonId;

	[SerializeField]
	private int m_cardSetId;

	[DbfField("RANKED_PLAY_SEASON_ID")]
	public int RankedPlaySeasonId => m_rankedPlaySeasonId;

	[DbfField("CARD_SET_ID")]
	public int CardSetId => m_cardSetId;

	public override object GetVar(string name)
	{
		if (!(name == "RANKED_PLAY_SEASON_ID"))
		{
			if (name == "CARD_SET_ID")
			{
				return m_cardSetId;
			}
			return null;
		}
		return m_rankedPlaySeasonId;
	}

	public override void SetVar(string name, object val)
	{
		if (!(name == "RANKED_PLAY_SEASON_ID"))
		{
			if (name == "CARD_SET_ID")
			{
				m_cardSetId = (int)val;
			}
		}
		else
		{
			m_rankedPlaySeasonId = (int)val;
		}
	}

	public override Type GetVarType(string name)
	{
		if (!(name == "RANKED_PLAY_SEASON_ID"))
		{
			if (name == "CARD_SET_ID")
			{
				return typeof(int);
			}
			return null;
		}
		return typeof(int);
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadHiddenCardSetsDbfRecords loadRecords = new LoadHiddenCardSetsDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		HiddenCardSetsDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(HiddenCardSetsDbfAsset)) as HiddenCardSetsDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"HiddenCardSetsDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
