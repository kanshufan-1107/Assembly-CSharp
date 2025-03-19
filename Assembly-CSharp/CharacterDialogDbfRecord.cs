using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class CharacterDialogDbfRecord : DbfRecord
{
	[SerializeField]
	private int m_onCompleteBannerId;

	[SerializeField]
	private string m_noteDesc;

	[SerializeField]
	private bool m_ignorePopups = true;

	[SerializeField]
	private bool m_blockInput;

	[SerializeField]
	private bool m_deferOnComplete = true;

	[DbfField("ON_COMPLETE_BANNER_ID")]
	public int OnCompleteBannerId => m_onCompleteBannerId;

	[DbfField("IGNORE_POPUPS")]
	public bool IgnorePopups => m_ignorePopups;

	[DbfField("BLOCK_INPUT")]
	public bool BlockInput => m_blockInput;

	[DbfField("DEFER_ON_COMPLETE")]
	public bool DeferOnComplete => m_deferOnComplete;

	public override object GetVar(string name)
	{
		return name switch
		{
			"ID" => base.ID, 
			"ON_COMPLETE_BANNER_ID" => m_onCompleteBannerId, 
			"NOTE_DESC" => m_noteDesc, 
			"IGNORE_POPUPS" => m_ignorePopups, 
			"BLOCK_INPUT" => m_blockInput, 
			"DEFER_ON_COMPLETE" => m_deferOnComplete, 
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
		case "ON_COMPLETE_BANNER_ID":
			m_onCompleteBannerId = (int)val;
			break;
		case "NOTE_DESC":
			m_noteDesc = (string)val;
			break;
		case "IGNORE_POPUPS":
			m_ignorePopups = (bool)val;
			break;
		case "BLOCK_INPUT":
			m_blockInput = (bool)val;
			break;
		case "DEFER_ON_COMPLETE":
			m_deferOnComplete = (bool)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"ON_COMPLETE_BANNER_ID" => typeof(int), 
			"NOTE_DESC" => typeof(string), 
			"IGNORE_POPUPS" => typeof(bool), 
			"BLOCK_INPUT" => typeof(bool), 
			"DEFER_ON_COMPLETE" => typeof(bool), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadCharacterDialogDbfRecords loadRecords = new LoadCharacterDialogDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		CharacterDialogDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(CharacterDialogDbfAsset)) as CharacterDialogDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"CharacterDialogDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
