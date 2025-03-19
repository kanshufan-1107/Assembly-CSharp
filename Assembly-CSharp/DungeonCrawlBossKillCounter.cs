using System;
using System.Collections.Generic;
using Blizzard.T5.MaterialService.Extensions;
using Hearthstone.DungeonCrawl;
using UnityEngine;

public class DungeonCrawlBossKillCounter : MonoBehaviour
{
	[Serializable]
	public class BossKillCounterStyleOverride
	{
		public DungeonRunVisualStyle VisualStyle;

		public string BossWinsRunNotCompletedString;

		public string BossWinsRunCompletedString;

		public string RunWinsString;

		public Material NumberHolderShadowMaterial;

		public Color DescriptionTextColor;
	}

	public UberText[] m_bossWinsText;

	public UberText m_runWinsText;

	public UberText m_runNotCompleteBossWinsHeader;

	public UberText m_runCompleteBossWinsHeader;

	public UberText m_runCompleteRunWinsHeader;

	public UberText m_fullPanelText;

	public GameObject m_runNotCompletedPanel;

	public GameObject m_runCompletedPanel;

	public GameObject[] m_numberHolderShadow;

	public BossKillCounterStyleOverride[] m_bossKillCounterStyle;

	private long m_bossWins;

	private long m_runWins;

	private TAG_CLASS m_heroClass;

	private string m_bossWinsHeaderRunNotCompletedString;

	private string m_bossWinsHeaderRunCompletedString;

	private IDungeonCrawlData m_dungeonCrawlData;

	private void Awake()
	{
		m_runNotCompletedPanel.SetActive(value: false);
		m_runCompletedPanel.SetActive(value: false);
	}

	public void SetDungeonRunData(IDungeonCrawlData data)
	{
		m_dungeonCrawlData = data;
		SetBossKillCounterVisualStyle();
	}

	public void SetHeroClass(TAG_CLASS heroClass)
	{
		m_heroClass = heroClass;
	}

	public void SetBossWins(long bossWins)
	{
		m_bossWins = bossWins;
		UberText[] bossWinsText = m_bossWinsText;
		for (int i = 0; i < bossWinsText.Length; i++)
		{
			bossWinsText[i].Text = m_bossWins.ToString();
		}
	}

	public void SetRunWins(long runWins)
	{
		m_runWins = runWins;
		m_runWinsText.Text = m_runWins.ToString();
	}

	public void UpdateLayout()
	{
		AdventureDataDbfRecord adventureDataRecord = m_dungeonCrawlData.GetSelectedAdventureDataRecord();
		if (adventureDataRecord != null && adventureDataRecord.DungeonCrawlShowBossKillCount)
		{
			m_fullPanelText.gameObject.SetActive(value: false);
			bool atLeastOneRunCompleted = m_runWins > 0;
			m_runNotCompletedPanel.SetActive(!atLeastOneRunCompleted);
			m_runCompletedPanel.SetActive(atLeastOneRunCompleted);
			if (!atLeastOneRunCompleted && m_runNotCompleteBossWinsHeader != null)
			{
				AdventureUtils.GetGuestHeroIdFromHeroCardDbId(m_dungeonCrawlData, (int)m_dungeonCrawlData.SelectedHeroCardDbId);
				string displayName = GetDisplayableClassName(preferClassNameOverHeroName: true);
				if (displayName == "UNKNOWN")
				{
					m_runNotCompleteBossWinsHeader.Text = GameStrings.Format(m_bossWinsHeaderRunCompletedString);
					return;
				}
				m_runNotCompleteBossWinsHeader.Text = GameStrings.Format(m_bossWinsHeaderRunNotCompletedString, displayName);
			}
			else if (atLeastOneRunCompleted && m_runCompleteBossWinsHeader != null)
			{
				m_runCompleteBossWinsHeader.Text = GameStrings.Format(m_bossWinsHeaderRunCompletedString);
			}
		}
		else
		{
			m_runNotCompletedPanel.SetActive(value: false);
			m_runCompletedPanel.SetActive(value: false);
			m_fullPanelText.gameObject.SetActive(value: true);
			ScenarioDbId scenarioId = m_dungeonCrawlData.GetMissionToPlay();
			ScenarioDbfRecord scenarioDbf = GameDbf.Scenario.GetRecord((int)scenarioId);
			if (scenarioDbf != null)
			{
				m_fullPanelText.Text = scenarioDbf.Description;
			}
		}
	}

	private string GetDisplayableHeroNameFromGuestHeroId(int guestHeroId)
	{
		AdventureGuestHeroesDbfRecord guestHero = GameDbf.AdventureGuestHeroes.GetRecord((AdventureGuestHeroesDbfRecord r) => r.GuestHeroId == guestHeroId);
		if (guestHero == null)
		{
			Debug.LogError($"GetDisplayableHeroNameFromGuestHeroId: No guest hero found for {guestHeroId}.");
			return string.Empty;
		}
		if (guestHero.GuestHeroRecord == null)
		{
			Debug.LogError($"GetDisplayableHeroNameFromGuestHeroId: No guest hero record found for {guestHeroId}.");
			return string.Empty;
		}
		return (!string.IsNullOrEmpty(guestHero.GuestHeroRecord.ShortName)) ? guestHero.GuestHeroRecord.ShortName : guestHero.GuestHeroRecord.Name;
	}

	private string GetDisplayableClassName(bool preferClassNameOverHeroName)
	{
		string className = GameStrings.GetClassName(m_heroClass);
		if (preferClassNameOverHeroName)
		{
			return className;
		}
		AdventureDbId currentAdventure = m_dungeonCrawlData.GetSelectedAdventure();
		List<AdventureGuestHeroesDbfRecord> records = GameDbf.AdventureGuestHeroes.GetRecords((AdventureGuestHeroesDbfRecord r) => r.AdventureId == (int)currentAdventure);
		List<CardDbfRecord> cardRecords = new List<CardDbfRecord>();
		foreach (AdventureGuestHeroesDbfRecord adventureGuestHero in records)
		{
			cardRecords.Add(GameDbf.Card.GetRecord(GameUtils.GetCardIdFromGuestHeroDbId(adventureGuestHero.GuestHeroId)));
		}
		foreach (CardDbfRecord cardRecord in cardRecords)
		{
			if (GameUtils.GetTagClassFromCardDbId(cardRecord.ID) == m_heroClass)
			{
				GuestHeroDbfRecord cardAdventurerRecord = GameDbf.GuestHero.GetRecord((GuestHeroDbfRecord r) => r.CardId == cardRecord.ID);
				if (cardAdventurerRecord != null)
				{
					className = cardAdventurerRecord.Name;
					break;
				}
			}
		}
		return className;
	}

	private void SetBossKillCounterVisualStyle()
	{
		DungeonRunVisualStyle style = m_dungeonCrawlData.VisualStyle;
		BossKillCounterStyleOverride[] bossKillCounterStyle = m_bossKillCounterStyle;
		foreach (BossKillCounterStyleOverride counterStyle in bossKillCounterStyle)
		{
			if (style != counterStyle.VisualStyle)
			{
				continue;
			}
			m_bossWinsHeaderRunNotCompletedString = counterStyle.BossWinsRunNotCompletedString;
			m_bossWinsHeaderRunCompletedString = counterStyle.BossWinsRunCompletedString;
			m_runCompleteRunWinsHeader.Text = counterStyle.RunWinsString;
			GameObject[] numberHolderShadow = m_numberHolderShadow;
			for (int j = 0; j < numberHolderShadow.Length; j++)
			{
				MeshRenderer renderer = numberHolderShadow[j].GetComponent<MeshRenderer>();
				if (renderer != null && counterStyle.NumberHolderShadowMaterial != null)
				{
					renderer.SetMaterial(counterStyle.NumberHolderShadowMaterial);
				}
			}
			m_runNotCompleteBossWinsHeader.TextColor = counterStyle.DescriptionTextColor;
			m_runCompleteBossWinsHeader.TextColor = counterStyle.DescriptionTextColor;
			m_runCompleteRunWinsHeader.TextColor = counterStyle.DescriptionTextColor;
			m_fullPanelText.TextColor = counterStyle.DescriptionTextColor;
			break;
		}
	}
}
