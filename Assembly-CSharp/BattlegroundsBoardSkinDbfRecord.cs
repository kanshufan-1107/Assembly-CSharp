using System;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class BattlegroundsBoardSkinDbfRecord : DbfRecord
{
	[SerializeField]
	private bool m_enabled = true;

	[SerializeField]
	private int m_rarityId = 2;

	[SerializeField]
	private string m_fullBoardPrefab;

	[SerializeField]
	private string m_fullBoardPrefabPhone;

	[SerializeField]
	private string m_fullTavernBoardPrefab;

	[SerializeField]
	private string m_fullTavernBoardPrefabPhone;

	[SerializeField]
	private DbfLocValue m_collectionName;

	[SerializeField]
	private DbfLocValue m_collectionShortName;

	[SerializeField]
	private DbfLocValue m_description;

	[SerializeField]
	private string m_detailsTexture;

	[SerializeField]
	private string m_detailsTexturePhone;

	[SerializeField]
	private string m_detailsMovie;

	[SerializeField]
	private string m_detailsMoviePhone;

	[SerializeField]
	private string m_detailsRenderConfig;

	[SerializeField]
	private BattlegroundsBoardSkin.Bordertype m_borderType;

	[DbfField("RARITY")]
	public int Rarity => m_rarityId;

	[DbfField("FULL_BOARD_PREFAB")]
	public string FullBoardPrefab => m_fullBoardPrefab;

	[DbfField("FULL_BOARD_PREFAB_PHONE")]
	public string FullBoardPrefabPhone => m_fullBoardPrefabPhone;

	[DbfField("FULL_TAVERN_BOARD_PREFAB")]
	public string FullTavernBoardPrefab => m_fullTavernBoardPrefab;

	[DbfField("FULL_TAVERN_BOARD_PREFAB_PHONE")]
	public string FullTavernBoardPrefabPhone => m_fullTavernBoardPrefabPhone;

	[DbfField("COLLECTION_NAME")]
	public DbfLocValue CollectionName => m_collectionName;

	[DbfField("COLLECTION_SHORT_NAME")]
	public DbfLocValue CollectionShortName => m_collectionShortName;

	[DbfField("DESCRIPTION")]
	public DbfLocValue Description => m_description;

	[DbfField("DETAILS_TEXTURE")]
	public string DetailsTexture => m_detailsTexture;

	[DbfField("DETAILS_TEXTURE_PHONE")]
	public string DetailsTexturePhone => m_detailsTexturePhone;

	[DbfField("DETAILS_MOVIE")]
	public string DetailsMovie => m_detailsMovie;

	[DbfField("DETAILS_MOVIE_PHONE")]
	public string DetailsMoviePhone => m_detailsMoviePhone;

	[DbfField("DETAILS_RENDER_CONFIG")]
	public string DetailsRenderConfig => m_detailsRenderConfig;

	[DbfField("BORDER_TYPE")]
	public BattlegroundsBoardSkin.Bordertype BorderType => m_borderType;

	public override object GetVar(string name)
	{
		return name switch
		{
			"ID" => base.ID, 
			"ENABLED" => m_enabled, 
			"RARITY" => m_rarityId, 
			"FULL_BOARD_PREFAB" => m_fullBoardPrefab, 
			"FULL_BOARD_PREFAB_PHONE" => m_fullBoardPrefabPhone, 
			"FULL_TAVERN_BOARD_PREFAB" => m_fullTavernBoardPrefab, 
			"FULL_TAVERN_BOARD_PREFAB_PHONE" => m_fullTavernBoardPrefabPhone, 
			"COLLECTION_NAME" => m_collectionName, 
			"COLLECTION_SHORT_NAME" => m_collectionShortName, 
			"DESCRIPTION" => m_description, 
			"DETAILS_TEXTURE" => m_detailsTexture, 
			"DETAILS_TEXTURE_PHONE" => m_detailsTexturePhone, 
			"DETAILS_MOVIE" => m_detailsMovie, 
			"DETAILS_MOVIE_PHONE" => m_detailsMoviePhone, 
			"DETAILS_RENDER_CONFIG" => m_detailsRenderConfig, 
			"BORDER_TYPE" => m_borderType, 
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
		case "FULL_BOARD_PREFAB":
			m_fullBoardPrefab = (string)val;
			break;
		case "FULL_BOARD_PREFAB_PHONE":
			m_fullBoardPrefabPhone = (string)val;
			break;
		case "FULL_TAVERN_BOARD_PREFAB":
			m_fullTavernBoardPrefab = (string)val;
			break;
		case "FULL_TAVERN_BOARD_PREFAB_PHONE":
			m_fullTavernBoardPrefabPhone = (string)val;
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
		case "DETAILS_TEXTURE":
			m_detailsTexture = (string)val;
			break;
		case "DETAILS_TEXTURE_PHONE":
			m_detailsTexturePhone = (string)val;
			break;
		case "DETAILS_MOVIE":
			m_detailsMovie = (string)val;
			break;
		case "DETAILS_MOVIE_PHONE":
			m_detailsMoviePhone = (string)val;
			break;
		case "DETAILS_RENDER_CONFIG":
			m_detailsRenderConfig = (string)val;
			break;
		case "BORDER_TYPE":
			if (val == null)
			{
				m_borderType = BattlegroundsBoardSkin.Bordertype.DEFAULT;
			}
			else if (val is BattlegroundsBoardSkin.Bordertype || val is int)
			{
				m_borderType = (BattlegroundsBoardSkin.Bordertype)val;
			}
			else if (val is string)
			{
				m_borderType = BattlegroundsBoardSkin.ParseBordertypeValue((string)val);
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
			"FULL_BOARD_PREFAB" => typeof(string), 
			"FULL_BOARD_PREFAB_PHONE" => typeof(string), 
			"FULL_TAVERN_BOARD_PREFAB" => typeof(string), 
			"FULL_TAVERN_BOARD_PREFAB_PHONE" => typeof(string), 
			"COLLECTION_NAME" => typeof(DbfLocValue), 
			"COLLECTION_SHORT_NAME" => typeof(DbfLocValue), 
			"DESCRIPTION" => typeof(DbfLocValue), 
			"DETAILS_TEXTURE" => typeof(string), 
			"DETAILS_TEXTURE_PHONE" => typeof(string), 
			"DETAILS_MOVIE" => typeof(string), 
			"DETAILS_MOVIE_PHONE" => typeof(string), 
			"DETAILS_RENDER_CONFIG" => typeof(string), 
			"BORDER_TYPE" => typeof(BattlegroundsBoardSkin.Bordertype), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadBattlegroundsBoardSkinDbfRecords loadRecords = new LoadBattlegroundsBoardSkinDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		BattlegroundsBoardSkinDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(BattlegroundsBoardSkinDbfAsset)) as BattlegroundsBoardSkinDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"BattlegroundsBoardSkinDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
