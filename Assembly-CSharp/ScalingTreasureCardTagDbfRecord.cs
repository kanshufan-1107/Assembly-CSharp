using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class ScalingTreasureCardTagDbfRecord : DbfRecord
{
	[SerializeField]
	private int m_lettuceTreasureId;

	[SerializeField]
	private int m_tagId = 2;

	[SerializeField]
	private int m_tagScaleValue = 1;

	[SerializeField]
	private bool m_isReferenceTag;

	[SerializeField]
	private bool m_isPowerKeywordTag;

	[DbfField("LETTUCE_TREASURE_ID")]
	public int LettuceTreasureId => m_lettuceTreasureId;

	[DbfField("TAG_ID")]
	public int TagId => m_tagId;

	[DbfField("TAG_SCALE_VALUE")]
	public int TagScaleValue => m_tagScaleValue;

	[DbfField("IS_REFERENCE_TAG")]
	public bool IsReferenceTag => m_isReferenceTag;

	[DbfField("IS_POWER_KEYWORD_TAG")]
	public bool IsPowerKeywordTag => m_isPowerKeywordTag;

	public override object GetVar(string name)
	{
		return name switch
		{
			"LETTUCE_TREASURE_ID" => m_lettuceTreasureId, 
			"TAG_ID" => m_tagId, 
			"TAG_SCALE_VALUE" => m_tagScaleValue, 
			"IS_REFERENCE_TAG" => m_isReferenceTag, 
			"IS_POWER_KEYWORD_TAG" => m_isPowerKeywordTag, 
			_ => null, 
		};
	}

	public override void SetVar(string name, object val)
	{
		switch (name)
		{
		case "LETTUCE_TREASURE_ID":
			m_lettuceTreasureId = (int)val;
			break;
		case "TAG_ID":
			m_tagId = (int)val;
			break;
		case "TAG_SCALE_VALUE":
			m_tagScaleValue = (int)val;
			break;
		case "IS_REFERENCE_TAG":
			m_isReferenceTag = (bool)val;
			break;
		case "IS_POWER_KEYWORD_TAG":
			m_isPowerKeywordTag = (bool)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"LETTUCE_TREASURE_ID" => typeof(int), 
			"TAG_ID" => typeof(int), 
			"TAG_SCALE_VALUE" => typeof(int), 
			"IS_REFERENCE_TAG" => typeof(bool), 
			"IS_POWER_KEYWORD_TAG" => typeof(bool), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadScalingTreasureCardTagDbfRecords loadRecords = new LoadScalingTreasureCardTagDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		ScalingTreasureCardTagDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(ScalingTreasureCardTagDbfAsset)) as ScalingTreasureCardTagDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"ScalingTreasureCardTagDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
