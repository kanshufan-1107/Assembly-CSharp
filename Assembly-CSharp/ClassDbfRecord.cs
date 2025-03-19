using System;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class ClassDbfRecord : DbfRecord
{
	[SerializeField]
	private Class.AssetFlags m_assetFlags;

	[SerializeField]
	private int m_defaultHeroCardId;

	[SerializeField]
	private DbfLocValue m_previewDesc;

	[SerializeField]
	private DbfLocValue m_strengthsDesc;

	[SerializeField]
	private DbfLocValue m_weaknessesDesc;

	[SerializeField]
	private DbfLocValue m_classFirstSelectionQuote;

	public CardDbfRecord DefaultHeroCardRecord => GameDbf.Card.GetRecord(m_defaultHeroCardId);

	[DbfField("PREVIEW_DESC")]
	public DbfLocValue PreviewDesc => m_previewDesc;

	[DbfField("STRENGTHS_DESC")]
	public DbfLocValue StrengthsDesc => m_strengthsDesc;

	[DbfField("WEAKNESSES_DESC")]
	public DbfLocValue WeaknessesDesc => m_weaknessesDesc;

	[DbfField("CLASS_FIRST_SELECTION_QUOTE")]
	public DbfLocValue ClassFirstSelectionQuote => m_classFirstSelectionQuote;

	public override object GetVar(string name)
	{
		return name switch
		{
			"ID" => base.ID, 
			"ASSET_FLAGS" => m_assetFlags, 
			"DEFAULT_HERO_CARD_ID" => m_defaultHeroCardId, 
			"PREVIEW_DESC" => m_previewDesc, 
			"STRENGTHS_DESC" => m_strengthsDesc, 
			"WEAKNESSES_DESC" => m_weaknessesDesc, 
			"CLASS_FIRST_SELECTION_QUOTE" => m_classFirstSelectionQuote, 
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
		case "ASSET_FLAGS":
			if (val == null)
			{
				m_assetFlags = Class.AssetFlags.NONE;
			}
			else if (val is Class.AssetFlags || val is int)
			{
				m_assetFlags = (Class.AssetFlags)val;
			}
			else if (val is string)
			{
				m_assetFlags = Class.ParseAssetFlagsValue((string)val);
			}
			break;
		case "DEFAULT_HERO_CARD_ID":
			m_defaultHeroCardId = (int)val;
			break;
		case "PREVIEW_DESC":
			m_previewDesc = (DbfLocValue)val;
			break;
		case "STRENGTHS_DESC":
			m_strengthsDesc = (DbfLocValue)val;
			break;
		case "WEAKNESSES_DESC":
			m_weaknessesDesc = (DbfLocValue)val;
			break;
		case "CLASS_FIRST_SELECTION_QUOTE":
			m_classFirstSelectionQuote = (DbfLocValue)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"ASSET_FLAGS" => typeof(Class.AssetFlags), 
			"DEFAULT_HERO_CARD_ID" => typeof(int), 
			"PREVIEW_DESC" => typeof(DbfLocValue), 
			"STRENGTHS_DESC" => typeof(DbfLocValue), 
			"WEAKNESSES_DESC" => typeof(DbfLocValue), 
			"CLASS_FIRST_SELECTION_QUOTE" => typeof(DbfLocValue), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadClassDbfRecords loadRecords = new LoadClassDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		ClassDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(ClassDbfAsset)) as ClassDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"ClassDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
		m_previewDesc.StripUnusedLocales();
		m_strengthsDesc.StripUnusedLocales();
		m_weaknessesDesc.StripUnusedLocales();
		m_classFirstSelectionQuote.StripUnusedLocales();
	}
}
