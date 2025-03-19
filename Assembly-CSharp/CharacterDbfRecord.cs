using System;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class CharacterDbfRecord : DbfRecord
{
	[SerializeField]
	private string m_name;

	[SerializeField]
	private int m_characterTagId;

	[SerializeField]
	private Character.CharacterGender m_characterGender;

	[SerializeField]
	private Character.CharacterRace m_characterRace;

	[SerializeField]
	private Character.CharacterClass m_characterClass;

	public override object GetVar(string name)
	{
		return name switch
		{
			"ID" => base.ID, 
			"NAME" => m_name, 
			"CHARACTER_TAG_ID" => m_characterTagId, 
			"CHARACTER_GENDER" => m_characterGender, 
			"CHARACTER_RACE" => m_characterRace, 
			"CHARACTER_CLASS" => m_characterClass, 
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
		case "NAME":
			m_name = (string)val;
			break;
		case "CHARACTER_TAG_ID":
			m_characterTagId = (int)val;
			break;
		case "CHARACTER_GENDER":
			if (val == null)
			{
				m_characterGender = Character.CharacterGender.MALE;
			}
			else if (val is Character.CharacterGender || val is int)
			{
				m_characterGender = (Character.CharacterGender)val;
			}
			else if (val is string)
			{
				m_characterGender = Character.ParseCharacterGenderValue((string)val);
			}
			break;
		case "CHARACTER_RACE":
			if (val == null)
			{
				m_characterRace = Character.CharacterRace.INVALID;
			}
			else if (val is Character.CharacterRace || val is int)
			{
				m_characterRace = (Character.CharacterRace)val;
			}
			else if (val is string)
			{
				m_characterRace = Character.ParseCharacterRaceValue((string)val);
			}
			break;
		case "CHARACTER_CLASS":
			if (val == null)
			{
				m_characterClass = Character.CharacterClass.INVALID;
			}
			else if (val is Character.CharacterClass || val is int)
			{
				m_characterClass = (Character.CharacterClass)val;
			}
			else if (val is string)
			{
				m_characterClass = Character.ParseCharacterClassValue((string)val);
			}
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"NAME" => typeof(string), 
			"CHARACTER_TAG_ID" => typeof(int), 
			"CHARACTER_GENDER" => typeof(Character.CharacterGender), 
			"CHARACTER_RACE" => typeof(Character.CharacterRace), 
			"CHARACTER_CLASS" => typeof(Character.CharacterClass), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadCharacterDbfRecords loadRecords = new LoadCharacterDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		CharacterDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(CharacterDbfAsset)) as CharacterDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"CharacterDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
