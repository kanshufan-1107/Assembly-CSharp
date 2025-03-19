using System;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class DkRuneListDbfRecord : DbfRecord
{
	[SerializeField]
	private int m_deckTemplateId;

	[SerializeField]
	private DkRuneList.DkruneTypes m_rune;

	[DbfField("DECK_TEMPLATE_ID")]
	public int DeckTemplateId => m_deckTemplateId;

	[DbfField("RUNE")]
	public DkRuneList.DkruneTypes Rune => m_rune;

	public override object GetVar(string name)
	{
		return name switch
		{
			"ID" => base.ID, 
			"DECK_TEMPLATE_ID" => m_deckTemplateId, 
			"RUNE" => m_rune, 
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
		case "DECK_TEMPLATE_ID":
			m_deckTemplateId = (int)val;
			break;
		case "RUNE":
			if (val == null)
			{
				m_rune = DkRuneList.DkruneTypes.NONERUNE;
			}
			else if (val is DkRuneList.DkruneTypes || val is int)
			{
				m_rune = (DkRuneList.DkruneTypes)val;
			}
			else if (val is string)
			{
				m_rune = DkRuneList.ParseDkruneTypesValue((string)val);
			}
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"DECK_TEMPLATE_ID" => typeof(int), 
			"RUNE" => typeof(DkRuneList.DkruneTypes), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadDkRuneListDbfRecords loadRecords = new LoadDkRuneListDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		DkRuneListDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(DkRuneListDbfAsset)) as DkRuneListDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"DkRuneListDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
