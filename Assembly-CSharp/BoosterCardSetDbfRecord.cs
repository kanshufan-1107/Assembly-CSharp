using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class BoosterCardSetDbfRecord : DbfRecord
{
	[SerializeField]
	private int m_subsetId;

	[SerializeField]
	private int m_cardSetId;

	[SerializeField]
	private string m_watermarkTextureOverride;

	[SerializeField]
	private bool m_useLatestExpansionSetWatermark;

	[DbfField("SUBSET_ID")]
	public int SubsetId => m_subsetId;

	public SubsetDbfRecord SubsetRecord => GameDbf.Subset.GetRecord(m_subsetId);

	[DbfField("CARD_SET_ID")]
	public int CardSetId => m_cardSetId;

	public CardSetDbfRecord CardSetRecord => GameDbf.CardSet.GetRecord(m_cardSetId);

	[DbfField("WATERMARK_TEXTURE_OVERRIDE")]
	public string WatermarkTextureOverride => m_watermarkTextureOverride;

	[DbfField("USE_LATEST_EXPANSION_SET_WATERMARK")]
	public bool UseLatestExpansionSetWatermark => m_useLatestExpansionSetWatermark;

	public override object GetVar(string name)
	{
		return name switch
		{
			"ID" => base.ID, 
			"SUBSET_ID" => m_subsetId, 
			"CARD_SET_ID" => m_cardSetId, 
			"WATERMARK_TEXTURE_OVERRIDE" => m_watermarkTextureOverride, 
			"USE_LATEST_EXPANSION_SET_WATERMARK" => m_useLatestExpansionSetWatermark, 
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
		case "SUBSET_ID":
			m_subsetId = (int)val;
			break;
		case "CARD_SET_ID":
			m_cardSetId = (int)val;
			break;
		case "WATERMARK_TEXTURE_OVERRIDE":
			m_watermarkTextureOverride = (string)val;
			break;
		case "USE_LATEST_EXPANSION_SET_WATERMARK":
			m_useLatestExpansionSetWatermark = (bool)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"SUBSET_ID" => typeof(int), 
			"CARD_SET_ID" => typeof(int), 
			"WATERMARK_TEXTURE_OVERRIDE" => typeof(string), 
			"USE_LATEST_EXPANSION_SET_WATERMARK" => typeof(bool), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadBoosterCardSetDbfRecords loadRecords = new LoadBoosterCardSetDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		BoosterCardSetDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(BoosterCardSetDbfAsset)) as BoosterCardSetDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"BoosterCardSetDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
