using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class LettuceBountySetDbfRecord : DbfRecord
{
	[SerializeField]
	private string m_noteDesc;

	[SerializeField]
	private EventTimingType m_event = EventTimingType.UNKNOWN;

	[SerializeField]
	private EventTimingType m_availableAfterEvent = EventTimingType.UNKNOWN;

	[SerializeField]
	private DbfLocValue m_eventComingSoonText;

	[SerializeField]
	private DbfLocValue m_name;

	[SerializeField]
	private DbfLocValue m_descriptionNormal;

	[SerializeField]
	private DbfLocValue m_descriptionHeroic;

	[SerializeField]
	private DbfLocValue m_descriptionMythic;

	[SerializeField]
	private DbfLocValue m_unlockPopupText;

	[SerializeField]
	private int m_sortOrder;

	[SerializeField]
	private string m_shortGuid;

	[SerializeField]
	private bool m_isTutorial;

	[SerializeField]
	private string m_watermarkTexture;

	[SerializeField]
	private string m_zoneArtTexture;

	[SerializeField]
	private string m_tileArtTexture;

	[SerializeField]
	private bool m_isComingSoon;

	[SerializeField]
	private int m_requiredCompletedBountyId;

	[DbfField("EVENT")]
	public EventTimingType Event => m_event;

	[DbfField("AVAILABLE_AFTER_EVENT")]
	public EventTimingType AvailableAfterEvent => m_availableAfterEvent;

	[DbfField("EVENT_COMING_SOON_TEXT")]
	public DbfLocValue EventComingSoonText => m_eventComingSoonText;

	[DbfField("NAME")]
	public DbfLocValue Name => m_name;

	[DbfField("DESCRIPTION_NORMAL")]
	public DbfLocValue DescriptionNormal => m_descriptionNormal;

	[DbfField("DESCRIPTION_HEROIC")]
	public DbfLocValue DescriptionHeroic => m_descriptionHeroic;

	[DbfField("DESCRIPTION_MYTHIC")]
	public DbfLocValue DescriptionMythic => m_descriptionMythic;

	[DbfField("UNLOCK_POPUP_TEXT")]
	public DbfLocValue UnlockPopupText => m_unlockPopupText;

	[DbfField("SORT_ORDER")]
	public int SortOrder => m_sortOrder;

	[DbfField("SHORT_GUID")]
	public string ShortGuid => m_shortGuid;

	[DbfField("IS_TUTORIAL")]
	public bool IsTutorial => m_isTutorial;

	[DbfField("WATERMARK_TEXTURE")]
	public string WatermarkTexture => m_watermarkTexture;

	[DbfField("ZONE_ART_TEXTURE")]
	public string ZoneArtTexture => m_zoneArtTexture;

	[DbfField("TILE_ART_TEXTURE")]
	public string TileArtTexture => m_tileArtTexture;

	[DbfField("IS_COMING_SOON")]
	public bool IsComingSoon => m_isComingSoon;

	[DbfField("REQUIRED_COMPLETED_BOUNTY")]
	public int RequiredCompletedBounty => m_requiredCompletedBountyId;

	public override object GetVar(string name)
	{
		return name switch
		{
			"ID" => base.ID, 
			"NOTE_DESC" => m_noteDesc, 
			"EVENT" => m_event, 
			"AVAILABLE_AFTER_EVENT" => m_availableAfterEvent, 
			"EVENT_COMING_SOON_TEXT" => m_eventComingSoonText, 
			"NAME" => m_name, 
			"DESCRIPTION_NORMAL" => m_descriptionNormal, 
			"DESCRIPTION_HEROIC" => m_descriptionHeroic, 
			"DESCRIPTION_MYTHIC" => m_descriptionMythic, 
			"UNLOCK_POPUP_TEXT" => m_unlockPopupText, 
			"SORT_ORDER" => m_sortOrder, 
			"SHORT_GUID" => m_shortGuid, 
			"IS_TUTORIAL" => m_isTutorial, 
			"WATERMARK_TEXTURE" => m_watermarkTexture, 
			"ZONE_ART_TEXTURE" => m_zoneArtTexture, 
			"TILE_ART_TEXTURE" => m_tileArtTexture, 
			"IS_COMING_SOON" => m_isComingSoon, 
			"REQUIRED_COMPLETED_BOUNTY" => m_requiredCompletedBountyId, 
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
		case "NOTE_DESC":
			m_noteDesc = (string)val;
			break;
		case "EVENT":
			m_event = DbfShared.GetEventMap().ConvertStringToSpecialEvent((string)val);
			break;
		case "AVAILABLE_AFTER_EVENT":
			m_availableAfterEvent = DbfShared.GetEventMap().ConvertStringToSpecialEvent((string)val);
			break;
		case "EVENT_COMING_SOON_TEXT":
			m_eventComingSoonText = (DbfLocValue)val;
			break;
		case "NAME":
			m_name = (DbfLocValue)val;
			break;
		case "DESCRIPTION_NORMAL":
			m_descriptionNormal = (DbfLocValue)val;
			break;
		case "DESCRIPTION_HEROIC":
			m_descriptionHeroic = (DbfLocValue)val;
			break;
		case "DESCRIPTION_MYTHIC":
			m_descriptionMythic = (DbfLocValue)val;
			break;
		case "UNLOCK_POPUP_TEXT":
			m_unlockPopupText = (DbfLocValue)val;
			break;
		case "SORT_ORDER":
			m_sortOrder = (int)val;
			break;
		case "SHORT_GUID":
			m_shortGuid = (string)val;
			break;
		case "IS_TUTORIAL":
			m_isTutorial = (bool)val;
			break;
		case "WATERMARK_TEXTURE":
			m_watermarkTexture = (string)val;
			break;
		case "ZONE_ART_TEXTURE":
			m_zoneArtTexture = (string)val;
			break;
		case "TILE_ART_TEXTURE":
			m_tileArtTexture = (string)val;
			break;
		case "IS_COMING_SOON":
			m_isComingSoon = (bool)val;
			break;
		case "REQUIRED_COMPLETED_BOUNTY":
			m_requiredCompletedBountyId = (int)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"NOTE_DESC" => typeof(string), 
			"EVENT" => typeof(string), 
			"AVAILABLE_AFTER_EVENT" => typeof(string), 
			"EVENT_COMING_SOON_TEXT" => typeof(DbfLocValue), 
			"NAME" => typeof(DbfLocValue), 
			"DESCRIPTION_NORMAL" => typeof(DbfLocValue), 
			"DESCRIPTION_HEROIC" => typeof(DbfLocValue), 
			"DESCRIPTION_MYTHIC" => typeof(DbfLocValue), 
			"UNLOCK_POPUP_TEXT" => typeof(DbfLocValue), 
			"SORT_ORDER" => typeof(int), 
			"SHORT_GUID" => typeof(string), 
			"IS_TUTORIAL" => typeof(bool), 
			"WATERMARK_TEXTURE" => typeof(string), 
			"ZONE_ART_TEXTURE" => typeof(string), 
			"TILE_ART_TEXTURE" => typeof(string), 
			"IS_COMING_SOON" => typeof(bool), 
			"REQUIRED_COMPLETED_BOUNTY" => typeof(int), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadLettuceBountySetDbfRecords loadRecords = new LoadLettuceBountySetDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		LettuceBountySetDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(LettuceBountySetDbfAsset)) as LettuceBountySetDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"LettuceBountySetDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
		m_eventComingSoonText.StripUnusedLocales();
		m_name.StripUnusedLocales();
		m_descriptionNormal.StripUnusedLocales();
		m_descriptionHeroic.StripUnusedLocales();
		m_descriptionMythic.StripUnusedLocales();
		m_unlockPopupText.StripUnusedLocales();
	}
}
