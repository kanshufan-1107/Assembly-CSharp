using System;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class LettuceMapNodeTypeDbfRecord : DbfRecord
{
	[SerializeField]
	private string m_noteDesc;

	[SerializeField]
	private bool m_usesGameplayScene;

	[SerializeField]
	private LettuceMapNodeType.LettuceMapBossType m_bossType;

	[SerializeField]
	private bool m_alwaysShowBossPreview;

	[SerializeField]
	private DbfLocValue m_hoverTooltipHeader;

	[SerializeField]
	private DbfLocValue m_hoverTooltipBody;

	[SerializeField]
	private DbfLocValue m_playButtonText;

	[SerializeField]
	private int m_scenarioOverrideId;

	[SerializeField]
	private int m_grantMercenaryId;

	[SerializeField]
	private string m_nodeVisualId = "BOSS";

	[SerializeField]
	private LettuceMapNodeType.Visitlogictype m_visitLogic;

	[SerializeField]
	private bool m_repeatable;

	[SerializeField]
	private bool m_autoPlay;

	[DbfField("USES_GAMEPLAY_SCENE")]
	public bool UsesGameplayScene => m_usesGameplayScene;

	[DbfField("BOSS_TYPE")]
	public LettuceMapNodeType.LettuceMapBossType BossType => m_bossType;

	[DbfField("ALWAYS_SHOW_BOSS_PREVIEW")]
	public bool AlwaysShowBossPreview => m_alwaysShowBossPreview;

	[DbfField("HOVER_TOOLTIP_HEADER")]
	public DbfLocValue HoverTooltipHeader => m_hoverTooltipHeader;

	[DbfField("HOVER_TOOLTIP_BODY")]
	public DbfLocValue HoverTooltipBody => m_hoverTooltipBody;

	[DbfField("PLAY_BUTTON_TEXT")]
	public DbfLocValue PlayButtonText => m_playButtonText;

	[DbfField("SCENARIO_OVERRIDE")]
	public int ScenarioOverride => m_scenarioOverrideId;

	[DbfField("GRANT_MERCENARY")]
	public int GrantMercenary => m_grantMercenaryId;

	[DbfField("NODE_VISUAL_ID")]
	public string NodeVisualId => m_nodeVisualId;

	[DbfField("VISIT_LOGIC")]
	public LettuceMapNodeType.Visitlogictype VisitLogic => m_visitLogic;

	[DbfField("REPEATABLE")]
	public bool Repeatable => m_repeatable;

	[DbfField("AUTO_PLAY")]
	public bool AutoPlay => m_autoPlay;

	public override object GetVar(string name)
	{
		return name switch
		{
			"ID" => base.ID, 
			"NOTE_DESC" => m_noteDesc, 
			"USES_GAMEPLAY_SCENE" => m_usesGameplayScene, 
			"BOSS_TYPE" => m_bossType, 
			"ALWAYS_SHOW_BOSS_PREVIEW" => m_alwaysShowBossPreview, 
			"HOVER_TOOLTIP_HEADER" => m_hoverTooltipHeader, 
			"HOVER_TOOLTIP_BODY" => m_hoverTooltipBody, 
			"PLAY_BUTTON_TEXT" => m_playButtonText, 
			"SCENARIO_OVERRIDE" => m_scenarioOverrideId, 
			"GRANT_MERCENARY" => m_grantMercenaryId, 
			"NODE_VISUAL_ID" => m_nodeVisualId, 
			"VISIT_LOGIC" => m_visitLogic, 
			"REPEATABLE" => m_repeatable, 
			"AUTO_PLAY" => m_autoPlay, 
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
		case "USES_GAMEPLAY_SCENE":
			m_usesGameplayScene = (bool)val;
			break;
		case "BOSS_TYPE":
			if (val == null)
			{
				m_bossType = LettuceMapNodeType.LettuceMapBossType.NONE;
			}
			else if (val is LettuceMapNodeType.LettuceMapBossType || val is int)
			{
				m_bossType = (LettuceMapNodeType.LettuceMapBossType)val;
			}
			else if (val is string)
			{
				m_bossType = LettuceMapNodeType.ParseLettuceMapBossTypeValue((string)val);
			}
			break;
		case "ALWAYS_SHOW_BOSS_PREVIEW":
			m_alwaysShowBossPreview = (bool)val;
			break;
		case "HOVER_TOOLTIP_HEADER":
			m_hoverTooltipHeader = (DbfLocValue)val;
			break;
		case "HOVER_TOOLTIP_BODY":
			m_hoverTooltipBody = (DbfLocValue)val;
			break;
		case "PLAY_BUTTON_TEXT":
			m_playButtonText = (DbfLocValue)val;
			break;
		case "SCENARIO_OVERRIDE":
			m_scenarioOverrideId = (int)val;
			break;
		case "GRANT_MERCENARY":
			m_grantMercenaryId = (int)val;
			break;
		case "NODE_VISUAL_ID":
			m_nodeVisualId = (string)val;
			break;
		case "VISIT_LOGIC":
			if (val == null)
			{
				m_visitLogic = LettuceMapNodeType.Visitlogictype.NONE;
			}
			else if (val is LettuceMapNodeType.Visitlogictype || val is int)
			{
				m_visitLogic = (LettuceMapNodeType.Visitlogictype)val;
			}
			else if (val is string)
			{
				m_visitLogic = LettuceMapNodeType.ParseVisitlogictypeValue((string)val);
			}
			break;
		case "REPEATABLE":
			m_repeatable = (bool)val;
			break;
		case "AUTO_PLAY":
			m_autoPlay = (bool)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"NOTE_DESC" => typeof(string), 
			"USES_GAMEPLAY_SCENE" => typeof(bool), 
			"BOSS_TYPE" => typeof(LettuceMapNodeType.LettuceMapBossType), 
			"ALWAYS_SHOW_BOSS_PREVIEW" => typeof(bool), 
			"HOVER_TOOLTIP_HEADER" => typeof(DbfLocValue), 
			"HOVER_TOOLTIP_BODY" => typeof(DbfLocValue), 
			"PLAY_BUTTON_TEXT" => typeof(DbfLocValue), 
			"SCENARIO_OVERRIDE" => typeof(int), 
			"GRANT_MERCENARY" => typeof(int), 
			"NODE_VISUAL_ID" => typeof(string), 
			"VISIT_LOGIC" => typeof(LettuceMapNodeType.Visitlogictype), 
			"REPEATABLE" => typeof(bool), 
			"AUTO_PLAY" => typeof(bool), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadLettuceMapNodeTypeDbfRecords loadRecords = new LoadLettuceMapNodeTypeDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		LettuceMapNodeTypeDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(LettuceMapNodeTypeDbfAsset)) as LettuceMapNodeTypeDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"LettuceMapNodeTypeDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
		m_hoverTooltipHeader.StripUnusedLocales();
		m_hoverTooltipBody.StripUnusedLocales();
		m_playButtonText.StripUnusedLocales();
	}
}
