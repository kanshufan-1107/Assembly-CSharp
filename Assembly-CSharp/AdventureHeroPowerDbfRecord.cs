using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class AdventureHeroPowerDbfRecord : DbfRecord
{
	[SerializeField]
	private int m_adventureId;

	[SerializeField]
	private int m_classId;

	[SerializeField]
	private int m_guestHeroId;

	[SerializeField]
	private int m_cardId;

	[SerializeField]
	private int m_sortOrder;

	[SerializeField]
	private bool m_isDefault;

	[SerializeField]
	private DbfLocValue m_unlockCriteriaText;

	[SerializeField]
	private DbfLocValue m_unlockedDescriptionText;

	[SerializeField]
	private int m_unlockGameSaveSubkeyId;

	[SerializeField]
	private int m_unlockValue;

	[SerializeField]
	private int m_unlockAchievementId;

	[DbfField("ADVENTURE_ID")]
	public int AdventureId => m_adventureId;

	[DbfField("CLASS_ID")]
	public int ClassId => m_classId;

	[DbfField("GUEST_HERO_ID")]
	public int GuestHeroId => m_guestHeroId;

	[DbfField("CARD_ID")]
	public int CardId => m_cardId;

	[DbfField("SORT_ORDER")]
	public int SortOrder => m_sortOrder;

	[DbfField("UNLOCK_CRITERIA_TEXT")]
	public DbfLocValue UnlockCriteriaText => m_unlockCriteriaText;

	[DbfField("UNLOCKED_DESCRIPTION_TEXT")]
	public DbfLocValue UnlockedDescriptionText => m_unlockedDescriptionText;

	[DbfField("UNLOCK_GAME_SAVE_SUBKEY")]
	public int UnlockGameSaveSubkey => m_unlockGameSaveSubkeyId;

	[DbfField("UNLOCK_VALUE")]
	public int UnlockValue => m_unlockValue;

	[DbfField("UNLOCK_ACHIEVEMENT")]
	public int UnlockAchievement => m_unlockAchievementId;

	public override object GetVar(string name)
	{
		return name switch
		{
			"ID" => base.ID, 
			"ADVENTURE_ID" => m_adventureId, 
			"CLASS_ID" => m_classId, 
			"GUEST_HERO_ID" => m_guestHeroId, 
			"CARD_ID" => m_cardId, 
			"SORT_ORDER" => m_sortOrder, 
			"IS_DEFAULT" => m_isDefault, 
			"UNLOCK_CRITERIA_TEXT" => m_unlockCriteriaText, 
			"UNLOCKED_DESCRIPTION_TEXT" => m_unlockedDescriptionText, 
			"UNLOCK_GAME_SAVE_SUBKEY" => m_unlockGameSaveSubkeyId, 
			"UNLOCK_VALUE" => m_unlockValue, 
			"UNLOCK_ACHIEVEMENT" => m_unlockAchievementId, 
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
		case "ADVENTURE_ID":
			m_adventureId = (int)val;
			break;
		case "CLASS_ID":
			m_classId = (int)val;
			break;
		case "GUEST_HERO_ID":
			m_guestHeroId = (int)val;
			break;
		case "CARD_ID":
			m_cardId = (int)val;
			break;
		case "SORT_ORDER":
			m_sortOrder = (int)val;
			break;
		case "IS_DEFAULT":
			m_isDefault = (bool)val;
			break;
		case "UNLOCK_CRITERIA_TEXT":
			m_unlockCriteriaText = (DbfLocValue)val;
			break;
		case "UNLOCKED_DESCRIPTION_TEXT":
			m_unlockedDescriptionText = (DbfLocValue)val;
			break;
		case "UNLOCK_GAME_SAVE_SUBKEY":
			m_unlockGameSaveSubkeyId = (int)val;
			break;
		case "UNLOCK_VALUE":
			m_unlockValue = (int)val;
			break;
		case "UNLOCK_ACHIEVEMENT":
			m_unlockAchievementId = (int)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"ADVENTURE_ID" => typeof(int), 
			"CLASS_ID" => typeof(int), 
			"GUEST_HERO_ID" => typeof(int), 
			"CARD_ID" => typeof(int), 
			"SORT_ORDER" => typeof(int), 
			"IS_DEFAULT" => typeof(bool), 
			"UNLOCK_CRITERIA_TEXT" => typeof(DbfLocValue), 
			"UNLOCKED_DESCRIPTION_TEXT" => typeof(DbfLocValue), 
			"UNLOCK_GAME_SAVE_SUBKEY" => typeof(int), 
			"UNLOCK_VALUE" => typeof(int), 
			"UNLOCK_ACHIEVEMENT" => typeof(int), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadAdventureHeroPowerDbfRecords loadRecords = new LoadAdventureHeroPowerDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		AdventureHeroPowerDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(AdventureHeroPowerDbfAsset)) as AdventureHeroPowerDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"AdventureHeroPowerDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
		m_unlockCriteriaText.StripUnusedLocales();
		m_unlockedDescriptionText.StripUnusedLocales();
	}
}
