using System.Collections.Generic;
using Assets;

public class RankedPlaySeason
{
	private RankedPlaySeasonDbfRecord m_rankedPlaySeasonRecord;

	private ScenarioOverrideDbfRecord m_overrideDbfRecord;

	private ScenarioDbfRecord m_scenarioRecord;

	private List<CollectionDeck> m_decks;

	public int ID => m_rankedPlaySeasonRecord?.ID ?? 0;

	public int Season => m_rankedPlaySeasonRecord?.Season ?? 0;

	public string SeasonTitle
	{
		get
		{
			if (m_rankedPlaySeasonRecord == null)
			{
				return null;
			}
			NetCache.NetCacheFeatures features = NetCache.Get()?.GetNetObject<NetCache.NetCacheFeatures>();
			if (features != null && !features.RankedPlayEnableScenarioOverrides)
			{
				return m_rankedPlaySeasonRecord.SeasonTitle;
			}
			ScenarioOverrideDbfRecord scenarioOverride = GetScenarioOverride(features);
			if (scenarioOverride == null)
			{
				return m_rankedPlaySeasonRecord.SeasonTitle;
			}
			return scenarioOverride.SeasonTitle;
		}
	}

	public string SeasonDesc
	{
		get
		{
			if (m_rankedPlaySeasonRecord == null)
			{
				return null;
			}
			NetCache.NetCacheFeatures features = NetCache.Get()?.GetNetObject<NetCache.NetCacheFeatures>();
			if (features != null && !features.RankedPlayEnableScenarioOverrides)
			{
				return m_rankedPlaySeasonRecord.SeasonDesc;
			}
			ScenarioOverrideDbfRecord scenarioOverride = GetScenarioOverride(features);
			if (scenarioOverride == null)
			{
				return m_rankedPlaySeasonRecord.SeasonDesc;
			}
			return scenarioOverride.SeasonDesc;
		}
	}

	public List<HiddenCardSetsDbfRecord> HiddenCardSets => m_rankedPlaySeasonRecord?.HiddenCardSets;

	public bool UsesPrebuiltDecks => (GetScenario()?.DeckTemplateChoices?.Count).GetValueOrDefault() > 0;

	public RankedPlaySeason(RankedPlaySeasonDbfRecord DbfRecord)
	{
		m_rankedPlaySeasonRecord = DbfRecord;
	}

	public ScenarioDbfRecord GetScenario()
	{
		if (m_rankedPlaySeasonRecord == null)
		{
			return null;
		}
		NetCache.NetCacheFeatures features = NetCache.Get()?.GetNetObject<NetCache.NetCacheFeatures>();
		if (features != null && !features.RankedPlayEnableScenarioOverrides)
		{
			return UpdateScenarioRecord(m_rankedPlaySeasonRecord.ScenarioRecord);
		}
		if (m_overrideDbfRecord != null && !EventTimingManager.Get().IsEventActive(m_overrideDbfRecord.EventTimingEvent))
		{
			UpdateScenarioRecord(null);
		}
		if (m_scenarioRecord == null)
		{
			UpdateScenarioRecord(m_rankedPlaySeasonRecord.ScenarioRecord);
			foreach (ScenarioOverrideDbfRecord scenarioOverride in m_rankedPlaySeasonRecord.ScenarioOverrides)
			{
				if (scenarioOverride != null && (features.TwistScenarioOverride == scenarioOverride.ScenarioId || EventTimingManager.Get().IsEventActive(scenarioOverride.EventTimingEvent)))
				{
					UpdateScenarioRecord(scenarioOverride.ScenarioRecord);
					m_overrideDbfRecord = scenarioOverride;
					break;
				}
			}
		}
		return m_scenarioRecord;
	}

	public List<CollectionDeck> CreateCopyOfDeckList()
	{
		List<CollectionDeck> decks = GetDecks();
		if (decks == null)
		{
			return null;
		}
		return new List<CollectionDeck>(decks);
	}

	public CollectionDeck GetDeck(int deckTemplateId)
	{
		List<CollectionDeck> decks = GetDecks();
		if (decks == null)
		{
			return null;
		}
		foreach (CollectionDeck deck in decks)
		{
			if (deck.DeckTemplateId == deckTemplateId)
			{
				return deck;
			}
		}
		return null;
	}

	public int GetDeckCount()
	{
		return GetDecks()?.Count ?? 0;
	}

	public List<CollectionDeck> GetDecks()
	{
		if (m_decks != null)
		{
			return m_decks;
		}
		ScenarioDbfRecord scenarioDbf = GetScenario();
		if (scenarioDbf == null)
		{
			return null;
		}
		if (scenarioDbf.DeckTemplateChoices == null || scenarioDbf.DeckTemplateChoices.Count == 0)
		{
			return null;
		}
		m_decks = new List<CollectionDeck>();
		foreach (DeckTemplateChoicesDbfRecord deckTemplateChoice in scenarioDbf.DeckTemplateChoices)
		{
			DeckTemplateDbfRecord deckTemplate = deckTemplateChoice.DeckTemplateRecord;
			if (deckTemplate != null && !GameUtils.IsBannedByTwistDenylist(deckTemplate))
			{
				CollectionDeck deck = CollectionDeck.Create(deckTemplate, DeckTemplate.SourceType.SCENARIO);
				if (deck != null)
				{
					m_decks.Add(deck);
				}
			}
		}
		return m_decks;
	}

	private ScenarioDbfRecord UpdateScenarioRecord(ScenarioDbfRecord scenario)
	{
		if (m_scenarioRecord != scenario)
		{
			m_scenarioRecord = scenario;
			m_overrideDbfRecord = null;
			m_decks = null;
		}
		return m_scenarioRecord;
	}

	private ScenarioOverrideDbfRecord GetScenarioOverride(NetCache.NetCacheFeatures features)
	{
		if (m_rankedPlaySeasonRecord == null)
		{
			return null;
		}
		ScenarioOverrideDbfRecord scenarioOverride = null;
		foreach (ScenarioOverrideDbfRecord so in m_rankedPlaySeasonRecord.ScenarioOverrides)
		{
			if (so != null && ((features != null && features.TwistScenarioOverride == so.ScenarioId) || EventTimingManager.Get().IsEventActive(so.EventTimingEvent)))
			{
				scenarioOverride = so;
				break;
			}
		}
		return scenarioOverride;
	}
}
