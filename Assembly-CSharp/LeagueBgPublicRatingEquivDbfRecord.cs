using System;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class LeagueBgPublicRatingEquivDbfRecord : DbfRecord
{
	[SerializeField]
	private int m_leagueId;

	[SerializeField]
	private LeagueBgPublicRatingEquiv.FormatType m_formatType;

	[SerializeField]
	private LeagueBgPublicRatingEquiv.Region m_region;

	[SerializeField]
	private int m_starLevel;

	[SerializeField]
	private int m_legendRank;

	[SerializeField]
	private int m_bgPublicRatingEquiv;

	[DbfField("LEAGUE_ID")]
	public int LeagueId => m_leagueId;

	[DbfField("FORMAT_TYPE")]
	public LeagueBgPublicRatingEquiv.FormatType FormatType => m_formatType;

	[DbfField("REGION")]
	public LeagueBgPublicRatingEquiv.Region Region => m_region;

	[DbfField("STAR_LEVEL")]
	public int StarLevel => m_starLevel;

	[DbfField("LEGEND_RANK")]
	public int LegendRank => m_legendRank;

	[DbfField("BG_PUBLIC_RATING_EQUIV")]
	public int BgPublicRatingEquiv => m_bgPublicRatingEquiv;

	public override object GetVar(string name)
	{
		return name switch
		{
			"ID" => base.ID, 
			"LEAGUE_ID" => m_leagueId, 
			"FORMAT_TYPE" => m_formatType, 
			"REGION" => m_region, 
			"STAR_LEVEL" => m_starLevel, 
			"LEGEND_RANK" => m_legendRank, 
			"BG_PUBLIC_RATING_EQUIV" => m_bgPublicRatingEquiv, 
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
		case "LEAGUE_ID":
			m_leagueId = (int)val;
			break;
		case "FORMAT_TYPE":
			if (val == null)
			{
				m_formatType = LeagueBgPublicRatingEquiv.FormatType.FT_UNKNOWN;
			}
			else if (val is LeagueBgPublicRatingEquiv.FormatType || val is int)
			{
				m_formatType = (LeagueBgPublicRatingEquiv.FormatType)val;
			}
			else if (val is string)
			{
				m_formatType = LeagueBgPublicRatingEquiv.ParseFormatTypeValue((string)val);
			}
			break;
		case "REGION":
			if (val == null)
			{
				m_region = LeagueBgPublicRatingEquiv.Region.REGION_UNKNOWN;
			}
			else if (val is LeagueBgPublicRatingEquiv.Region || val is int)
			{
				m_region = (LeagueBgPublicRatingEquiv.Region)val;
			}
			else if (val is string)
			{
				m_region = LeagueBgPublicRatingEquiv.ParseRegionValue((string)val);
			}
			break;
		case "STAR_LEVEL":
			m_starLevel = (int)val;
			break;
		case "LEGEND_RANK":
			m_legendRank = (int)val;
			break;
		case "BG_PUBLIC_RATING_EQUIV":
			m_bgPublicRatingEquiv = (int)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"LEAGUE_ID" => typeof(int), 
			"FORMAT_TYPE" => typeof(LeagueBgPublicRatingEquiv.FormatType), 
			"REGION" => typeof(LeagueBgPublicRatingEquiv.Region), 
			"STAR_LEVEL" => typeof(int), 
			"LEGEND_RANK" => typeof(int), 
			"BG_PUBLIC_RATING_EQUIV" => typeof(int), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadLeagueBgPublicRatingEquivDbfRecords loadRecords = new LoadLeagueBgPublicRatingEquivDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		LeagueBgPublicRatingEquivDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(LeagueBgPublicRatingEquivDbfAsset)) as LeagueBgPublicRatingEquivDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"LeagueBgPublicRatingEquivDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
