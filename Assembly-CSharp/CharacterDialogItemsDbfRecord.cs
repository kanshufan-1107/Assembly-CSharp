using System;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class CharacterDialogItemsDbfRecord : DbfRecord
{
	[SerializeField]
	private int m_characterDialogId;

	[SerializeField]
	private int m_playOrder;

	[SerializeField]
	private bool m_useInnkeeperQuote;

	[SerializeField]
	private string m_audioName;

	[SerializeField]
	private DbfLocValue m_bubbleText;

	[SerializeField]
	private string m_prefabName;

	[SerializeField]
	private bool m_altBubblePosition;

	[SerializeField]
	private bool m_persistPrefab;

	[SerializeField]
	private bool m_altPosition;

	[SerializeField]
	private string m_bannerPrefabName;

	[SerializeField]
	private bool m_useBannerStyle;

	[SerializeField]
	private CharacterDialogItems.CanvasAnchorType m_bannerAnchorPosition = CharacterDialogItems.CanvasAnchorType.BOTTOM;

	[SerializeField]
	private double m_waitBefore;

	[SerializeField]
	private double m_waitAfter;

	[SerializeField]
	private string m_achieveEventType;

	[SerializeField]
	private double m_minimumDurationSeconds;

	[SerializeField]
	private double m_localeExtraSeconds;

	[DbfField("CHARACTER_DIALOG_ID")]
	public int CharacterDialogId => m_characterDialogId;

	[DbfField("PLAY_ORDER")]
	public int PlayOrder => m_playOrder;

	[DbfField("USE_INNKEEPER_QUOTE")]
	public bool UseInnkeeperQuote => m_useInnkeeperQuote;

	[DbfField("AUDIO_NAME")]
	public string AudioName => m_audioName;

	[DbfField("BUBBLE_TEXT")]
	public DbfLocValue BubbleText => m_bubbleText;

	[DbfField("PREFAB_NAME")]
	public string PrefabName => m_prefabName;

	[DbfField("ALT_BUBBLE_POSITION")]
	public bool AltBubblePosition => m_altBubblePosition;

	[DbfField("PERSIST_PREFAB")]
	public bool PersistPrefab => m_persistPrefab;

	[DbfField("ALT_POSITION")]
	public bool AltPosition => m_altPosition;

	[DbfField("BANNER_PREFAB_NAME")]
	public string BannerPrefabName => m_bannerPrefabName;

	[DbfField("USE_BANNER_STYLE")]
	public bool UseBannerStyle => m_useBannerStyle;

	[DbfField("BANNER_ANCHOR_POSITION")]
	public CharacterDialogItems.CanvasAnchorType BannerAnchorPosition => m_bannerAnchorPosition;

	[DbfField("WAIT_BEFORE")]
	public double WaitBefore => m_waitBefore;

	[DbfField("WAIT_AFTER")]
	public double WaitAfter => m_waitAfter;

	[DbfField("ACHIEVE_EVENT_TYPE")]
	public string AchieveEventType => m_achieveEventType;

	[DbfField("MINIMUM_DURATION_SECONDS")]
	public double MinimumDurationSeconds => m_minimumDurationSeconds;

	[DbfField("LOCALE_EXTRA_SECONDS")]
	public double LocaleExtraSeconds => m_localeExtraSeconds;

	public override object GetVar(string name)
	{
		return name switch
		{
			"ID" => base.ID, 
			"CHARACTER_DIALOG_ID" => m_characterDialogId, 
			"PLAY_ORDER" => m_playOrder, 
			"USE_INNKEEPER_QUOTE" => m_useInnkeeperQuote, 
			"AUDIO_NAME" => m_audioName, 
			"BUBBLE_TEXT" => m_bubbleText, 
			"PREFAB_NAME" => m_prefabName, 
			"ALT_BUBBLE_POSITION" => m_altBubblePosition, 
			"PERSIST_PREFAB" => m_persistPrefab, 
			"ALT_POSITION" => m_altPosition, 
			"BANNER_PREFAB_NAME" => m_bannerPrefabName, 
			"USE_BANNER_STYLE" => m_useBannerStyle, 
			"BANNER_ANCHOR_POSITION" => m_bannerAnchorPosition, 
			"WAIT_BEFORE" => m_waitBefore, 
			"WAIT_AFTER" => m_waitAfter, 
			"ACHIEVE_EVENT_TYPE" => m_achieveEventType, 
			"MINIMUM_DURATION_SECONDS" => m_minimumDurationSeconds, 
			"LOCALE_EXTRA_SECONDS" => m_localeExtraSeconds, 
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
		case "CHARACTER_DIALOG_ID":
			m_characterDialogId = (int)val;
			break;
		case "PLAY_ORDER":
			m_playOrder = (int)val;
			break;
		case "USE_INNKEEPER_QUOTE":
			m_useInnkeeperQuote = (bool)val;
			break;
		case "AUDIO_NAME":
			m_audioName = (string)val;
			break;
		case "BUBBLE_TEXT":
			m_bubbleText = (DbfLocValue)val;
			break;
		case "PREFAB_NAME":
			m_prefabName = (string)val;
			break;
		case "ALT_BUBBLE_POSITION":
			m_altBubblePosition = (bool)val;
			break;
		case "PERSIST_PREFAB":
			m_persistPrefab = (bool)val;
			break;
		case "ALT_POSITION":
			m_altPosition = (bool)val;
			break;
		case "BANNER_PREFAB_NAME":
			m_bannerPrefabName = (string)val;
			break;
		case "USE_BANNER_STYLE":
			m_useBannerStyle = (bool)val;
			break;
		case "BANNER_ANCHOR_POSITION":
			if (val == null)
			{
				m_bannerAnchorPosition = CharacterDialogItems.CanvasAnchorType.CENTER;
			}
			else if (val is CharacterDialogItems.CanvasAnchorType || val is int)
			{
				m_bannerAnchorPosition = (CharacterDialogItems.CanvasAnchorType)val;
			}
			else if (val is string)
			{
				m_bannerAnchorPosition = CharacterDialogItems.ParseCanvasAnchorTypeValue((string)val);
			}
			break;
		case "WAIT_BEFORE":
			m_waitBefore = (double)val;
			break;
		case "WAIT_AFTER":
			m_waitAfter = (double)val;
			break;
		case "ACHIEVE_EVENT_TYPE":
			m_achieveEventType = (string)val;
			break;
		case "MINIMUM_DURATION_SECONDS":
			m_minimumDurationSeconds = (double)val;
			break;
		case "LOCALE_EXTRA_SECONDS":
			m_localeExtraSeconds = (double)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"CHARACTER_DIALOG_ID" => typeof(int), 
			"PLAY_ORDER" => typeof(int), 
			"USE_INNKEEPER_QUOTE" => typeof(bool), 
			"AUDIO_NAME" => typeof(string), 
			"BUBBLE_TEXT" => typeof(DbfLocValue), 
			"PREFAB_NAME" => typeof(string), 
			"ALT_BUBBLE_POSITION" => typeof(bool), 
			"PERSIST_PREFAB" => typeof(bool), 
			"ALT_POSITION" => typeof(bool), 
			"BANNER_PREFAB_NAME" => typeof(string), 
			"USE_BANNER_STYLE" => typeof(bool), 
			"BANNER_ANCHOR_POSITION" => typeof(CharacterDialogItems.CanvasAnchorType), 
			"WAIT_BEFORE" => typeof(double), 
			"WAIT_AFTER" => typeof(double), 
			"ACHIEVE_EVENT_TYPE" => typeof(string), 
			"MINIMUM_DURATION_SECONDS" => typeof(double), 
			"LOCALE_EXTRA_SECONDS" => typeof(double), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadCharacterDialogItemsDbfRecords loadRecords = new LoadCharacterDialogItemsDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		CharacterDialogItemsDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(CharacterDialogItemsDbfAsset)) as CharacterDialogItemsDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"CharacterDialogItemsDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
		m_bubbleText.StripUnusedLocales();
	}
}
