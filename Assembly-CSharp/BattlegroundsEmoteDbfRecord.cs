using System;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class BattlegroundsEmoteDbfRecord : DbfRecord
{
	[SerializeField]
	private bool m_enabled = true;

	[SerializeField]
	private int m_rarityId = 2;

	[SerializeField]
	private DbfLocValue m_collectionShortName;

	[SerializeField]
	private DbfLocValue m_description;

	[SerializeField]
	private string m_animationPath;

	[SerializeField]
	private double m_xOffset;

	[SerializeField]
	private double m_zOffset = -0.079;

	[SerializeField]
	private bool m_isDefault;

	[SerializeField]
	private bool m_isAnimating;

	[SerializeField]
	private BattlegroundsEmote.Bordertype m_borderType;

	[DbfField("RARITY")]
	public int Rarity => m_rarityId;

	[DbfField("COLLECTION_SHORT_NAME")]
	public DbfLocValue CollectionShortName => m_collectionShortName;

	[DbfField("DESCRIPTION")]
	public DbfLocValue Description => m_description;

	[DbfField("ANIMATION_PATH")]
	public string AnimationPath => m_animationPath;

	[DbfField("X_OFFSET")]
	public double XOffset => m_xOffset;

	[DbfField("Z_OFFSET")]
	public double ZOffset => m_zOffset;

	[DbfField("IS_DEFAULT")]
	public bool IsDefault => m_isDefault;

	[DbfField("IS_ANIMATING")]
	public bool IsAnimating => m_isAnimating;

	[DbfField("BORDER_TYPE")]
	public BattlegroundsEmote.Bordertype BorderType => m_borderType;

	public override object GetVar(string name)
	{
		return name switch
		{
			"ID" => base.ID, 
			"ENABLED" => m_enabled, 
			"RARITY" => m_rarityId, 
			"COLLECTION_SHORT_NAME" => m_collectionShortName, 
			"DESCRIPTION" => m_description, 
			"ANIMATION_PATH" => m_animationPath, 
			"X_OFFSET" => m_xOffset, 
			"Z_OFFSET" => m_zOffset, 
			"IS_DEFAULT" => m_isDefault, 
			"IS_ANIMATING" => m_isAnimating, 
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
		case "COLLECTION_SHORT_NAME":
			m_collectionShortName = (DbfLocValue)val;
			break;
		case "DESCRIPTION":
			m_description = (DbfLocValue)val;
			break;
		case "ANIMATION_PATH":
			m_animationPath = (string)val;
			break;
		case "X_OFFSET":
			m_xOffset = (double)val;
			break;
		case "Z_OFFSET":
			m_zOffset = (double)val;
			break;
		case "IS_DEFAULT":
			m_isDefault = (bool)val;
			break;
		case "IS_ANIMATING":
			m_isAnimating = (bool)val;
			break;
		case "BORDER_TYPE":
			if (val == null)
			{
				m_borderType = BattlegroundsEmote.Bordertype.NONE;
			}
			else if (val is BattlegroundsEmote.Bordertype || val is int)
			{
				m_borderType = (BattlegroundsEmote.Bordertype)val;
			}
			else if (val is string)
			{
				m_borderType = BattlegroundsEmote.ParseBordertypeValue((string)val);
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
			"COLLECTION_SHORT_NAME" => typeof(DbfLocValue), 
			"DESCRIPTION" => typeof(DbfLocValue), 
			"ANIMATION_PATH" => typeof(string), 
			"X_OFFSET" => typeof(double), 
			"Z_OFFSET" => typeof(double), 
			"IS_DEFAULT" => typeof(bool), 
			"IS_ANIMATING" => typeof(bool), 
			"BORDER_TYPE" => typeof(BattlegroundsEmote.Bordertype), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadBattlegroundsEmoteDbfRecords loadRecords = new LoadBattlegroundsEmoteDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		BattlegroundsEmoteDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(BattlegroundsEmoteDbfAsset)) as BattlegroundsEmoteDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"BattlegroundsEmoteDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
		m_collectionShortName.StripUnusedLocales();
		m_description.StripUnusedLocales();
	}
}
