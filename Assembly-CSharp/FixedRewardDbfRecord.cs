using System;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class FixedRewardDbfRecord : DbfRecord
{
	[SerializeField]
	private string m_noteDesc;

	[SerializeField]
	private Assets.FixedReward.Type m_type = Assets.FixedReward.ParseTypeValue("unknown");

	[SerializeField]
	private int m_battlegroundsGuideSkinId;

	[SerializeField]
	private int m_battlegroundsHeroSkinId;

	[SerializeField]
	private int m_battlegroundsBoardSkinId;

	[SerializeField]
	private int m_battlegroundsFinisherId;

	[SerializeField]
	private int m_battlegroundsEmoteId;

	[SerializeField]
	private int m_cardId;

	[SerializeField]
	private int m_cardPremium;

	[SerializeField]
	private int m_cardBackId;

	[SerializeField]
	private int m_metaActionId;

	[SerializeField]
	private ulong m_metaActionFlags;

	[SerializeField]
	private int m_luckyDrawBoxId;

	[DbfField("TYPE")]
	public Assets.FixedReward.Type Type => m_type;

	[DbfField("BATTLEGROUNDS_GUIDE_SKIN_ID")]
	public int BattlegroundsGuideSkinId => m_battlegroundsGuideSkinId;

	public BattlegroundsGuideSkinDbfRecord BattlegroundsGuideSkinRecord => GameDbf.BattlegroundsGuideSkin.GetRecord(m_battlegroundsGuideSkinId);

	[DbfField("BATTLEGROUNDS_HERO_SKIN_ID")]
	public int BattlegroundsHeroSkinId => m_battlegroundsHeroSkinId;

	public BattlegroundsHeroSkinDbfRecord BattlegroundsHeroSkinRecord => GameDbf.BattlegroundsHeroSkin.GetRecord(m_battlegroundsHeroSkinId);

	[DbfField("BATTLEGROUNDS_BOARD_SKIN_ID")]
	public int BattlegroundsBoardSkinId => m_battlegroundsBoardSkinId;

	[DbfField("BATTLEGROUNDS_FINISHER_ID")]
	public int BattlegroundsFinisherId => m_battlegroundsFinisherId;

	[DbfField("BATTLEGROUNDS_EMOTE_ID")]
	public int BattlegroundsEmoteId => m_battlegroundsEmoteId;

	[DbfField("CARD_ID")]
	public int CardId => m_cardId;

	public CardDbfRecord CardRecord => GameDbf.Card.GetRecord(m_cardId);

	[DbfField("CARD_PREMIUM")]
	public int CardPremium => m_cardPremium;

	[DbfField("CARD_BACK_ID")]
	public int CardBackId => m_cardBackId;

	[DbfField("META_ACTION_ID")]
	public int MetaActionId => m_metaActionId;

	[DbfField("META_ACTION_FLAGS")]
	public ulong MetaActionFlags => m_metaActionFlags;

	public override object GetVar(string name)
	{
		return name switch
		{
			"ID" => base.ID, 
			"NOTE_DESC" => m_noteDesc, 
			"TYPE" => m_type, 
			"BATTLEGROUNDS_GUIDE_SKIN_ID" => m_battlegroundsGuideSkinId, 
			"BATTLEGROUNDS_HERO_SKIN_ID" => m_battlegroundsHeroSkinId, 
			"BATTLEGROUNDS_BOARD_SKIN_ID" => m_battlegroundsBoardSkinId, 
			"BATTLEGROUNDS_FINISHER_ID" => m_battlegroundsFinisherId, 
			"BATTLEGROUNDS_EMOTE_ID" => m_battlegroundsEmoteId, 
			"CARD_ID" => m_cardId, 
			"CARD_PREMIUM" => m_cardPremium, 
			"CARD_BACK_ID" => m_cardBackId, 
			"META_ACTION_ID" => m_metaActionId, 
			"META_ACTION_FLAGS" => m_metaActionFlags, 
			"LUCKY_DRAW_BOX_ID" => m_luckyDrawBoxId, 
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
		case "TYPE":
			if (val == null)
			{
				m_type = Assets.FixedReward.Type.UNKNOWN;
			}
			else if (val is Assets.FixedReward.Type || val is int)
			{
				m_type = (Assets.FixedReward.Type)val;
			}
			else if (val is string)
			{
				m_type = Assets.FixedReward.ParseTypeValue((string)val);
			}
			break;
		case "BATTLEGROUNDS_GUIDE_SKIN_ID":
			m_battlegroundsGuideSkinId = (int)val;
			break;
		case "BATTLEGROUNDS_HERO_SKIN_ID":
			m_battlegroundsHeroSkinId = (int)val;
			break;
		case "BATTLEGROUNDS_BOARD_SKIN_ID":
			m_battlegroundsBoardSkinId = (int)val;
			break;
		case "BATTLEGROUNDS_FINISHER_ID":
			m_battlegroundsFinisherId = (int)val;
			break;
		case "BATTLEGROUNDS_EMOTE_ID":
			m_battlegroundsEmoteId = (int)val;
			break;
		case "CARD_ID":
			m_cardId = (int)val;
			break;
		case "CARD_PREMIUM":
			m_cardPremium = (int)val;
			break;
		case "CARD_BACK_ID":
			m_cardBackId = (int)val;
			break;
		case "META_ACTION_ID":
			m_metaActionId = (int)val;
			break;
		case "META_ACTION_FLAGS":
			m_metaActionFlags = (ulong)val;
			break;
		case "LUCKY_DRAW_BOX_ID":
			m_luckyDrawBoxId = (int)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"NOTE_DESC" => typeof(string), 
			"TYPE" => typeof(Assets.FixedReward.Type), 
			"BATTLEGROUNDS_GUIDE_SKIN_ID" => typeof(int), 
			"BATTLEGROUNDS_HERO_SKIN_ID" => typeof(int), 
			"BATTLEGROUNDS_BOARD_SKIN_ID" => typeof(int), 
			"BATTLEGROUNDS_FINISHER_ID" => typeof(int), 
			"BATTLEGROUNDS_EMOTE_ID" => typeof(int), 
			"CARD_ID" => typeof(int), 
			"CARD_PREMIUM" => typeof(int), 
			"CARD_BACK_ID" => typeof(int), 
			"META_ACTION_ID" => typeof(int), 
			"META_ACTION_FLAGS" => typeof(ulong), 
			"LUCKY_DRAW_BOX_ID" => typeof(int), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadFixedRewardDbfRecords loadRecords = new LoadFixedRewardDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		FixedRewardDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(FixedRewardDbfAsset)) as FixedRewardDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"FixedRewardDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
