using System;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class AchievementDbfRecord : DbfRecord
{
	[SerializeField]
	private int m_achievementSectionId;

	[SerializeField]
	private int m_sortOrder;

	[SerializeField]
	private bool m_enabled = true;

	[SerializeField]
	private DbfLocValue m_name;

	[SerializeField]
	private DbfLocValue m_description;

	[SerializeField]
	private Assets.Achievement.AchievementVisibility m_achievementVisibility;

	[SerializeField]
	private int m_quota = 1;

	[SerializeField]
	private bool m_allowExceedQuota;

	[SerializeField]
	private int m_triggerId;

	[SerializeField]
	private int m_points;

	[SerializeField]
	private int m_rewardTrackXp;

	[SerializeField]
	private Assets.Achievement.RewardTrackType m_rewardTrackType;

	[SerializeField]
	private int m_rewardListId;

	[SerializeField]
	private int m_nextTierId;

	[SerializeField]
	private bool m_socialToast;

	[DbfField("ACHIEVEMENT_SECTION")]
	public int AchievementSection => m_achievementSectionId;

	public AchievementSectionDbfRecord AchievementSectionRecord => GameDbf.AchievementSection.GetRecord(m_achievementSectionId);

	[DbfField("SORT_ORDER")]
	public int SortOrder => m_sortOrder;

	[DbfField("ENABLED")]
	public bool Enabled => m_enabled;

	[DbfField("NAME")]
	public DbfLocValue Name => m_name;

	[DbfField("DESCRIPTION")]
	public DbfLocValue Description => m_description;

	[DbfField("ACHIEVEMENT_VISIBILITY")]
	public Assets.Achievement.AchievementVisibility AchievementVisibility => m_achievementVisibility;

	[DbfField("QUOTA")]
	public int Quota => m_quota;

	[DbfField("ALLOW_EXCEED_QUOTA")]
	public bool AllowExceedQuota => m_allowExceedQuota;

	[DbfField("POINTS")]
	public int Points => m_points;

	[DbfField("REWARD_TRACK_XP")]
	public int RewardTrackXp => m_rewardTrackXp;

	[DbfField("REWARD_TRACK_TYPE")]
	public Assets.Achievement.RewardTrackType RewardTrackType => m_rewardTrackType;

	[DbfField("REWARD_LIST")]
	public int RewardList => m_rewardListId;

	public RewardListDbfRecord RewardListRecord => GameDbf.RewardList.GetRecord(m_rewardListId);

	[DbfField("NEXT_TIER")]
	public int NextTier => m_nextTierId;

	[DbfField("SOCIAL_TOAST")]
	public bool SocialToast => m_socialToast;

	public override object GetVar(string name)
	{
		return name switch
		{
			"ID" => base.ID, 
			"ACHIEVEMENT_SECTION" => m_achievementSectionId, 
			"SORT_ORDER" => m_sortOrder, 
			"ENABLED" => m_enabled, 
			"NAME" => m_name, 
			"DESCRIPTION" => m_description, 
			"ACHIEVEMENT_VISIBILITY" => m_achievementVisibility, 
			"QUOTA" => m_quota, 
			"ALLOW_EXCEED_QUOTA" => m_allowExceedQuota, 
			"TRIGGER" => m_triggerId, 
			"POINTS" => m_points, 
			"REWARD_TRACK_XP" => m_rewardTrackXp, 
			"REWARD_TRACK_TYPE" => m_rewardTrackType, 
			"REWARD_LIST" => m_rewardListId, 
			"NEXT_TIER" => m_nextTierId, 
			"SOCIAL_TOAST" => m_socialToast, 
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
		case "ACHIEVEMENT_SECTION":
			m_achievementSectionId = (int)val;
			break;
		case "SORT_ORDER":
			m_sortOrder = (int)val;
			break;
		case "ENABLED":
			m_enabled = (bool)val;
			break;
		case "NAME":
			m_name = (DbfLocValue)val;
			break;
		case "DESCRIPTION":
			m_description = (DbfLocValue)val;
			break;
		case "ACHIEVEMENT_VISIBILITY":
			if (val == null)
			{
				m_achievementVisibility = Assets.Achievement.AchievementVisibility.VISIBLE;
			}
			else if (val is Assets.Achievement.AchievementVisibility || val is int)
			{
				m_achievementVisibility = (Assets.Achievement.AchievementVisibility)val;
			}
			else if (val is string)
			{
				m_achievementVisibility = Assets.Achievement.ParseAchievementVisibilityValue((string)val);
			}
			break;
		case "QUOTA":
			m_quota = (int)val;
			break;
		case "ALLOW_EXCEED_QUOTA":
			m_allowExceedQuota = (bool)val;
			break;
		case "TRIGGER":
			m_triggerId = (int)val;
			break;
		case "POINTS":
			m_points = (int)val;
			break;
		case "REWARD_TRACK_XP":
			m_rewardTrackXp = (int)val;
			break;
		case "REWARD_TRACK_TYPE":
			if (val == null)
			{
				m_rewardTrackType = Assets.Achievement.RewardTrackType.NONE;
			}
			else if (val is Assets.Achievement.RewardTrackType || val is int)
			{
				m_rewardTrackType = (Assets.Achievement.RewardTrackType)val;
			}
			else if (val is string)
			{
				m_rewardTrackType = Assets.Achievement.ParseRewardTrackTypeValue((string)val);
			}
			break;
		case "REWARD_LIST":
			m_rewardListId = (int)val;
			break;
		case "NEXT_TIER":
			m_nextTierId = (int)val;
			break;
		case "SOCIAL_TOAST":
			m_socialToast = (bool)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"ACHIEVEMENT_SECTION" => typeof(int), 
			"SORT_ORDER" => typeof(int), 
			"ENABLED" => typeof(bool), 
			"NAME" => typeof(DbfLocValue), 
			"DESCRIPTION" => typeof(DbfLocValue), 
			"ACHIEVEMENT_VISIBILITY" => typeof(Assets.Achievement.AchievementVisibility), 
			"QUOTA" => typeof(int), 
			"ALLOW_EXCEED_QUOTA" => typeof(bool), 
			"TRIGGER" => typeof(int), 
			"POINTS" => typeof(int), 
			"REWARD_TRACK_XP" => typeof(int), 
			"REWARD_TRACK_TYPE" => typeof(Assets.Achievement.RewardTrackType), 
			"REWARD_LIST" => typeof(int), 
			"NEXT_TIER" => typeof(int), 
			"SOCIAL_TOAST" => typeof(bool), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadAchievementDbfRecords loadRecords = new LoadAchievementDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		AchievementDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(AchievementDbfAsset)) as AchievementDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"AchievementDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
		m_name.StripUnusedLocales();
		m_description.StripUnusedLocales();
	}
}
