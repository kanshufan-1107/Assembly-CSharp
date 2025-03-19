using System;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class BattlegroundsFinisherDbfRecord : DbfRecord
{
	[SerializeField]
	private bool m_enabled = true;

	[SerializeField]
	private int m_rarityId = 2;

	[SerializeField]
	private DbfLocValue m_collectionName;

	[SerializeField]
	private DbfLocValue m_collectionShortName;

	[SerializeField]
	private DbfLocValue m_description;

	[SerializeField]
	private string m_gameplaySettings;

	[SerializeField]
	private string m_destroyOpponentPrefab;

	[SerializeField]
	private string m_destroyOpponentVictoryPrefab;

	[SerializeField]
	private string m_detailsTexture;

	[SerializeField]
	private string m_detailsMovie;

	[SerializeField]
	private string m_detailsRenderConfig;

	[SerializeField]
	private string m_miniBodyMaterial;

	[SerializeField]
	private string m_miniArtMaterial;

	[SerializeField]
	private bool m_isDefault;

	[SerializeField]
	private BattlegroundsFinisher.CapsuleType m_capsuleType;

	[DbfField("ENABLED")]
	public bool Enabled => m_enabled;

	[DbfField("RARITY")]
	public int Rarity => m_rarityId;

	[DbfField("COLLECTION_NAME")]
	public DbfLocValue CollectionName => m_collectionName;

	[DbfField("COLLECTION_SHORT_NAME")]
	public DbfLocValue CollectionShortName => m_collectionShortName;

	[DbfField("DESCRIPTION")]
	public DbfLocValue Description => m_description;

	[DbfField("GAMEPLAY_SETTINGS")]
	public string GameplaySettings => m_gameplaySettings;

	[DbfField("DETAILS_TEXTURE")]
	public string DetailsTexture => m_detailsTexture;

	[DbfField("DETAILS_MOVIE")]
	public string DetailsMovie => m_detailsMovie;

	[DbfField("DETAILS_RENDER_CONFIG")]
	public string DetailsRenderConfig => m_detailsRenderConfig;

	[DbfField("MINI_BODY_MATERIAL")]
	public string MiniBodyMaterial => m_miniBodyMaterial;

	[DbfField("MINI_ART_MATERIAL")]
	public string MiniArtMaterial => m_miniArtMaterial;

	[DbfField("CAPSULE_TYPE")]
	public BattlegroundsFinisher.CapsuleType CapsuleType => m_capsuleType;

	public List<DetailsVideoCueDbfRecord> VideoCues
	{
		get
		{
			int id = base.ID;
			List<DetailsVideoCueDbfRecord> returnRecords = new List<DetailsVideoCueDbfRecord>();
			List<DetailsVideoCueDbfRecord> records = GameDbf.DetailsVideoCue.GetRecords();
			int i = 0;
			for (int iMax = records.Count; i < iMax; i++)
			{
				DetailsVideoCueDbfRecord record = records[i];
				if (record.BattlegroundsFinisherId == id)
				{
					returnRecords.Add(record);
				}
			}
			return returnRecords;
		}
	}

	public override object GetVar(string name)
	{
		return name switch
		{
			"ID" => base.ID, 
			"ENABLED" => m_enabled, 
			"RARITY" => m_rarityId, 
			"COLLECTION_NAME" => m_collectionName, 
			"COLLECTION_SHORT_NAME" => m_collectionShortName, 
			"DESCRIPTION" => m_description, 
			"GAMEPLAY_SETTINGS" => m_gameplaySettings, 
			"DESTROY_OPPONENT_PREFAB" => m_destroyOpponentPrefab, 
			"DESTROY_OPPONENT_VICTORY_PREFAB" => m_destroyOpponentVictoryPrefab, 
			"DETAILS_TEXTURE" => m_detailsTexture, 
			"DETAILS_MOVIE" => m_detailsMovie, 
			"DETAILS_RENDER_CONFIG" => m_detailsRenderConfig, 
			"MINI_BODY_MATERIAL" => m_miniBodyMaterial, 
			"MINI_ART_MATERIAL" => m_miniArtMaterial, 
			"IS_DEFAULT" => m_isDefault, 
			"CAPSULE_TYPE" => m_capsuleType, 
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
		case "ENABLED":
			m_enabled = (bool)val;
			break;
		case "RARITY":
			m_rarityId = (int)val;
			break;
		case "COLLECTION_NAME":
			m_collectionName = (DbfLocValue)val;
			break;
		case "COLLECTION_SHORT_NAME":
			m_collectionShortName = (DbfLocValue)val;
			break;
		case "DESCRIPTION":
			m_description = (DbfLocValue)val;
			break;
		case "GAMEPLAY_SETTINGS":
			m_gameplaySettings = (string)val;
			break;
		case "DESTROY_OPPONENT_PREFAB":
			m_destroyOpponentPrefab = (string)val;
			break;
		case "DESTROY_OPPONENT_VICTORY_PREFAB":
			m_destroyOpponentVictoryPrefab = (string)val;
			break;
		case "DETAILS_TEXTURE":
			m_detailsTexture = (string)val;
			break;
		case "DETAILS_MOVIE":
			m_detailsMovie = (string)val;
			break;
		case "DETAILS_RENDER_CONFIG":
			m_detailsRenderConfig = (string)val;
			break;
		case "MINI_BODY_MATERIAL":
			m_miniBodyMaterial = (string)val;
			break;
		case "MINI_ART_MATERIAL":
			m_miniArtMaterial = (string)val;
			break;
		case "IS_DEFAULT":
			m_isDefault = (bool)val;
			break;
		case "CAPSULE_TYPE":
			if (val == null)
			{
				m_capsuleType = BattlegroundsFinisher.CapsuleType.DEFAULT;
			}
			else if (val is BattlegroundsFinisher.CapsuleType || val is int)
			{
				m_capsuleType = (BattlegroundsFinisher.CapsuleType)val;
			}
			else if (val is string)
			{
				m_capsuleType = BattlegroundsFinisher.ParseCapsuleTypeValue((string)val);
			}
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"ENABLED" => typeof(bool), 
			"RARITY" => typeof(int), 
			"COLLECTION_NAME" => typeof(DbfLocValue), 
			"COLLECTION_SHORT_NAME" => typeof(DbfLocValue), 
			"DESCRIPTION" => typeof(DbfLocValue), 
			"GAMEPLAY_SETTINGS" => typeof(string), 
			"DESTROY_OPPONENT_PREFAB" => typeof(string), 
			"DESTROY_OPPONENT_VICTORY_PREFAB" => typeof(string), 
			"DETAILS_TEXTURE" => typeof(string), 
			"DETAILS_MOVIE" => typeof(string), 
			"DETAILS_RENDER_CONFIG" => typeof(string), 
			"MINI_BODY_MATERIAL" => typeof(string), 
			"MINI_ART_MATERIAL" => typeof(string), 
			"IS_DEFAULT" => typeof(bool), 
			"CAPSULE_TYPE" => typeof(BattlegroundsFinisher.CapsuleType), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadBattlegroundsFinisherDbfRecords loadRecords = new LoadBattlegroundsFinisherDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		BattlegroundsFinisherDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(BattlegroundsFinisherDbfAsset)) as BattlegroundsFinisherDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"BattlegroundsFinisherDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
		m_collectionName.StripUnusedLocales();
		m_collectionShortName.StripUnusedLocales();
		m_description.StripUnusedLocales();
	}
}
