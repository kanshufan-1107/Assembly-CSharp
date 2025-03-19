using System;
using System.Collections.Generic;
using System.Linq;
using Assets;
using Blizzard.T5.Core;
using PegasusShared;

public class TavernBrawlMission
{
	private CharacterDialogSequence m_firstTimeSeenCharacterDialogSequence;

	private int m_selectedBrawlIndex = -1;

	private Map<int, DeckRuleset> m_cachedSelectedDeckRuleset = new Map<int, DeckRuleset>();

	public TavernBrawlSeasonSpec tavernBrawlSpec { get; private set; }

	public int seasonId => tavernBrawlSpec.GameContentSeason.SeasonId;

	public GameContentScenario SelectedBrawl
	{
		get
		{
			if (tavernBrawlSpec == null || m_selectedBrawlIndex < 0 || m_selectedBrawlIndex >= tavernBrawlSpec.GameContentSeason.Scenarios.Count)
			{
				if (tavernBrawlSpec != null && tavernBrawlSpec.GameContentSeason.Scenarios.Count == 1)
				{
					return tavernBrawlSpec.GameContentSeason.Scenarios[0];
				}
				return null;
			}
			return tavernBrawlSpec.GameContentSeason.Scenarios[m_selectedBrawlIndex];
		}
	}

	public IList<GameContentScenario> BrawlList
	{
		get
		{
			if (tavernBrawlSpec != null)
			{
				return tavernBrawlSpec.GameContentSeason.Scenarios;
			}
			return new List<GameContentScenario>();
		}
	}

	public int missionId => SelectedBrawl?.ScenarioId ?? 0;

	public int SelectedBrawlLibraryItemId => SelectedBrawl?.LibraryItemId ?? 0;

	public TavernBrawlMode brawlMode => SelectedBrawl?.BrawlMode ?? TavernBrawlMode.TB_MODE_NORMAL;

	public FormatType formatType => SelectedBrawl?.FormatType ?? FormatType.FT_UNKNOWN;

	public DateTime? endDateLocal
	{
		get
		{
			if (!tavernBrawlSpec.GameContentSeason.HasEndSecondsFromNow)
			{
				return null;
			}
			return DateTime.Now + new TimeSpan(0, 0, (int)tavernBrawlSpec.GameContentSeason.EndSecondsFromNow);
		}
	}

	public DateTime? closedToNewSessionsDateLocal
	{
		get
		{
			GameContentScenario scenario = SelectedBrawl;
			if (scenario == null || !scenario.HasClosedToNewSessionsSecondsFromNow)
			{
				return null;
			}
			return DateTime.Now + new TimeSpan(0, 0, (int)scenario.ClosedToNewSessionsSecondsFromNow);
		}
	}

	public bool canCreateDeck => CanCreateDeck(SelectedBrawlLibraryItemId);

	public bool canEditDeck
	{
		get
		{
			ScenarioDbfRecord scen = GameDbf.Scenario.GetRecord(missionId);
			if (scen != null && scen.RuleType == Scenario.RuleType.CHOOSE_DECK)
			{
				return true;
			}
			return false;
		}
	}

	public bool canSelectHeroForDeck
	{
		get
		{
			ScenarioDbfRecord scen = GameDbf.Scenario.GetRecord(missionId);
			if (scen != null)
			{
				RuleType ruleType = (RuleType)scen.RuleType;
				if ((uint)(ruleType - 1) <= 1u)
				{
					return true;
				}
			}
			return false;
		}
	}

	public int ticketType => SelectedBrawl?.TicketType ?? 0;

	public int ticketAmount => SelectedBrawl?.TicketAmount ?? 0;

	public bool showOnlySetsFromDeckRuleset => SelectedBrawl?.ShowOnlySetsFromDeckRuleset ?? false;

	public RewardType rewardType => SelectedBrawl?.RewardType ?? RewardType.REWARD_UNKNOWN;

	public RewardTrigger rewardTrigger => SelectedBrawl?.RewardTrigger ?? RewardTrigger.REWARD_TRIGGER_UNKNOWN;

	public long RewardData1 => SelectedBrawl?.RewardData1 ?? 0;

	public long RewardData2 => SelectedBrawl?.RewardData2 ?? 0;

	public int RewardTriggerQuota => SelectedBrawl?.RewardTriggerQuota ?? 0;

	public int maxWins => SelectedBrawl?.MaxWins ?? 0;

	public int maxLosses => SelectedBrawl?.MaxLosses ?? 0;

	public int maxSessions => SelectedBrawl?.MaxSessions ?? 0;

	public int SeasonEndSecondsSpreadCount => tavernBrawlSpec.GameContentSeason.SeasonEndSecondSpreadCount;

	public bool friendlyChallengeDisabled => SelectedBrawl?.FriendlyChallengeDisabled ?? false;

	public uint FreeSessions
	{
		get
		{
			GameContentScenario scen = SelectedBrawl;
			if (scen == null || !scen.HasFreeSessions)
			{
				return 0u;
			}
			return scen.FreeSessions;
		}
	}

	public bool IsSessionBased
	{
		get
		{
			if (maxWins <= 0)
			{
				return maxLosses > 0;
			}
			return true;
		}
	}

	public BrawlType BrawlType { get; private set; }

	public int FirstTimeSeenCharacterDialogID => SelectedBrawl?.FirstTimeSeenDialogId ?? 0;

	public bool IsDungeonRun
	{
		get
		{
			ScenarioDbfRecord scen = GameDbf.Scenario.GetRecord(missionId);
			if (scen != null)
			{
				AdventureModeDbId mode = (AdventureModeDbId)scen.ModeId;
				if (mode != AdventureModeDbId.DUNGEON_CRAWL)
				{
					return mode == AdventureModeDbId.DUNGEON_CRAWL_HEROIC;
				}
				return true;
			}
			return false;
		}
	}

	public CharacterDialogSequence FirstTimeSeenCharacterDialogSequence
	{
		get
		{
			if (FirstTimeSeenCharacterDialogID < 1)
			{
				return null;
			}
			if (m_firstTimeSeenCharacterDialogSequence == null)
			{
				m_firstTimeSeenCharacterDialogSequence = new CharacterDialogSequence(FirstTimeSeenCharacterDialogID);
			}
			return m_firstTimeSeenCharacterDialogSequence;
		}
	}

	public GameType GameType => GetGameType(BrawlType, missionId);

	public bool CanCreateDeck(int brawlLibraryItemId)
	{
		if (brawlLibraryItemId == 0)
		{
			brawlLibraryItemId = SelectedBrawlLibraryItemId;
		}
		int scenarioId = GetScenarioId(brawlLibraryItemId);
		ScenarioDbfRecord scen = GameDbf.Scenario.GetRecord(scenarioId);
		if (scen != null && scen.RuleType == Scenario.RuleType.CHOOSE_DECK)
		{
			return true;
		}
		return false;
	}

	public DeckRuleset GetDeckRuleset(int brawlLibraryItemId)
	{
		DeckRuleset ruleset = null;
		if (!m_cachedSelectedDeckRuleset.TryGetValue(brawlLibraryItemId, out ruleset))
		{
			int scenarioId = GetScenarioId(brawlLibraryItemId);
			ScenarioDbfRecord scen = GameDbf.Scenario.GetRecord(scenarioId);
			if (scen != null)
			{
				ruleset = DeckRuleset.GetDeckRuleset(scen.DeckRulesetId);
				m_cachedSelectedDeckRuleset[brawlLibraryItemId] = ruleset;
			}
		}
		return ruleset;
	}

	public void SetSeasonSpec(TavernBrawlSeasonSpec spec, BrawlType brawlType)
	{
		if (spec == null)
		{
			throw new ArgumentNullException("TavernBrawlMissions must have a spec provided");
		}
		tavernBrawlSpec = spec;
		BrawlType = brawlType;
		m_selectedBrawlIndex = -1;
		m_firstTimeSeenCharacterDialogSequence = null;
		m_cachedSelectedDeckRuleset.Clear();
		spec.GameContentSeason.Scenarios.Sort(delegate(GameContentScenario a, GameContentScenario b)
		{
			if (a.IsRequired != b.IsRequired)
			{
				if (!a.IsRequired)
				{
					return 1;
				}
				return -1;
			}
			return (a.IsFallback != b.IsFallback) ? ((!a.IsFallback) ? 1 : (-1)) : (b.ScenarioId - a.ScenarioId);
		});
	}

	private static GameType GetGameType(BrawlType brawlType, int scenarioId, bool isFriendlyChallenge = false)
	{
		if (!isFriendlyChallenge)
		{
			return GameType.GT_TAVERNBRAWL;
		}
		return GameType.GT_VS_FRIEND;
	}

	public void SetSelectedBrawlLibraryItemId(int brawlLibraryItemId)
	{
		m_selectedBrawlIndex = -1;
		IList<GameContentScenario> brawls = BrawlList;
		for (int i = 0; i < brawls.Count; i++)
		{
			if (brawls[i].LibraryItemId == brawlLibraryItemId)
			{
				m_selectedBrawlIndex = i;
				break;
			}
		}
	}

	public int GetScenarioId(int brawlLibraryItemId)
	{
		if (brawlLibraryItemId == 0)
		{
			brawlLibraryItemId = SelectedBrawlLibraryItemId;
		}
		return BrawlList.FirstOrDefault((GameContentScenario s) => s.LibraryItemId == brawlLibraryItemId)?.ScenarioId ?? 0;
	}
}
