using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class ModifiedLettuceAbilityCardTagDbfRecord : DbfRecord
{
	[SerializeField]
	private int m_modifiedLettuceAbilityValueId;

	[SerializeField]
	private int m_tagId;

	[SerializeField]
	private int m_tagValue;

	[SerializeField]
	private bool m_isReferenceTag;

	[SerializeField]
	private bool m_isPowerKeywordTag;

	[DbfField("MODIFIED_LETTUCE_ABILITY_VALUE_ID")]
	public int ModifiedLettuceAbilityValueId => m_modifiedLettuceAbilityValueId;

	[DbfField("TAG_ID")]
	public int TagId => m_tagId;

	[DbfField("TAG_VALUE")]
	public int TagValue => m_tagValue;

	[DbfField("IS_REFERENCE_TAG")]
	public bool IsReferenceTag => m_isReferenceTag;

	[DbfField("IS_POWER_KEYWORD_TAG")]
	public bool IsPowerKeywordTag => m_isPowerKeywordTag;

	public override object GetVar(string name)
	{
		return name switch
		{
			"MODIFIED_LETTUCE_ABILITY_VALUE_ID" => m_modifiedLettuceAbilityValueId, 
			"TAG_ID" => m_tagId, 
			"TAG_VALUE" => m_tagValue, 
			"IS_REFERENCE_TAG" => m_isReferenceTag, 
			"IS_POWER_KEYWORD_TAG" => m_isPowerKeywordTag, 
			_ => null, 
		};
	}

	public override void SetVar(string name, object val)
	{
		switch (name)
		{
		case "MODIFIED_LETTUCE_ABILITY_VALUE_ID":
			m_modifiedLettuceAbilityValueId = (int)val;
			break;
		case "TAG_ID":
			m_tagId = (int)val;
			break;
		case "TAG_VALUE":
			m_tagValue = (int)val;
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
			"MODIFIED_LETTUCE_ABILITY_VALUE_ID" => typeof(int), 
			"TAG_ID" => typeof(int), 
			"TAG_VALUE" => typeof(int), 
			"IS_REFERENCE_TAG" => typeof(bool), 
			"IS_POWER_KEYWORD_TAG" => typeof(bool), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadModifiedLettuceAbilityCardTagDbfRecords loadRecords = new LoadModifiedLettuceAbilityCardTagDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		ModifiedLettuceAbilityCardTagDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(ModifiedLettuceAbilityCardTagDbfAsset)) as ModifiedLettuceAbilityCardTagDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"ModifiedLettuceAbilityCardTagDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
