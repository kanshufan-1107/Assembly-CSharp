using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class DetailsVideoCueDbfRecord : DbfRecord
{
	[SerializeField]
	private int m_battlegroundsFinisherId;

	[SerializeField]
	private DbfLocValue m_captionTitle;

	[SerializeField]
	private DbfLocValue m_captionSubtitle;

	[SerializeField]
	private double m_startSeconds;

	[DbfField("BATTLEGROUNDS_FINISHER_ID")]
	public int BattlegroundsFinisherId => m_battlegroundsFinisherId;

	[DbfField("CAPTION_TITLE")]
	public DbfLocValue CaptionTitle => m_captionTitle;

	[DbfField("CAPTION_SUBTITLE")]
	public DbfLocValue CaptionSubtitle => m_captionSubtitle;

	[DbfField("START_SECONDS")]
	public double StartSeconds => m_startSeconds;

	public override object GetVar(string name)
	{
		return name switch
		{
			"BATTLEGROUNDS_FINISHER_ID" => m_battlegroundsFinisherId, 
			"CAPTION_TITLE" => m_captionTitle, 
			"CAPTION_SUBTITLE" => m_captionSubtitle, 
			"START_SECONDS" => m_startSeconds, 
			_ => null, 
		};
	}

	public override void SetVar(string name, object val)
	{
		switch (name)
		{
		case "BATTLEGROUNDS_FINISHER_ID":
			m_battlegroundsFinisherId = (int)val;
			break;
		case "CAPTION_TITLE":
			m_captionTitle = (DbfLocValue)val;
			break;
		case "CAPTION_SUBTITLE":
			m_captionSubtitle = (DbfLocValue)val;
			break;
		case "START_SECONDS":
			m_startSeconds = (double)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"BATTLEGROUNDS_FINISHER_ID" => typeof(int), 
			"CAPTION_TITLE" => typeof(DbfLocValue), 
			"CAPTION_SUBTITLE" => typeof(DbfLocValue), 
			"START_SECONDS" => typeof(double), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadDetailsVideoCueDbfRecords loadRecords = new LoadDetailsVideoCueDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		DetailsVideoCueDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(DetailsVideoCueDbfAsset)) as DetailsVideoCueDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"DetailsVideoCueDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
		m_captionTitle.StripUnusedLocales();
		m_captionSubtitle.StripUnusedLocales();
	}
}
