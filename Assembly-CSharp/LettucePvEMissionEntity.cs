using System.Collections.Generic;
using Blizzard.T5.Core;

public class LettucePvEMissionEntity : LettuceMissionEntity
{
	private bool m_skipTutorial;

	private static Map<GameEntityOption, bool> s_booleanOptions = InitBooleanOptions();

	private static Map<GameEntityOption, string> s_stringOptions = InitStringOptions();

	private static Map<GameEntityOption, bool> InitBooleanOptions()
	{
		return new Map<GameEntityOption, bool> { 
		{
			GameEntityOption.WAIT_FOR_RATING_INFO,
			false
		} };
	}

	private static Map<GameEntityOption, string> InitStringOptions()
	{
		return new Map<GameEntityOption, string>();
	}

	public LettucePvEMissionEntity(bool skipTutorial = false, VoPlaybackHandler voHandler = null)
		: base(voHandler)
	{
		m_gameOptions.AddOptions(s_booleanOptions, s_stringOptions);
		m_skipTutorial = skipTutorial;
	}

	public override void UpdateAllMercenaryAbilityOrderBubbleText(bool hideUnselectedAbilityBubbles = false)
	{
		if (m_gamePhase == 3 || !m_abilityOrderSpeechBubblesEnabled)
		{
			return;
		}
		List<Card> minionsInPlay = GetAllMinionsInPlay();
		minionsInPlay = SortDeterministicActionOrder(minionsInPlay);
		int actionOrder = 1;
		for (int i = 0; i < minionsInPlay.Count; i++)
		{
			Card minionInPlay = minionsInPlay[i];
			if (!(minionInPlay == null) && (m_enemyAbilityOrderSpeechBubblesEnabled || !minionInPlay.GetEntity().IsControlledByOpposingSidePlayer()))
			{
				minionInPlay.SetLettuceAbilityActionOrder(actionOrder++, isTied: false);
				minionInPlay.UpdateLettuceSpeechBubbleText(hideUnselectedAbilityBubbles);
			}
		}
	}

	private List<Card> SortDeterministicActionOrder(List<Card> cardsInPlay)
	{
		if (cardsInPlay == null || cardsInPlay.Count == 0)
		{
			return new List<Card>();
		}
		SortedDictionary<int, List<Card>> cardsBySpeed = new SortedDictionary<int, List<Card>>(new CardSpeedCamparer(ShouldSortAbilitiesLowToHigh()));
		foreach (Card card in cardsInPlay)
		{
			if (!(card == null))
			{
				int speed = card.GetPreparedLettuceAbilitySpeedValue();
				if (cardsBySpeed.ContainsKey(speed))
				{
					cardsBySpeed[speed].Add(card);
					continue;
				}
				List<Card> newCardBucket = new List<Card>();
				newCardBucket.Add(card);
				cardsBySpeed.Add(speed, newCardBucket);
			}
		}
		List<Card> tempFriendlyCharactersBucket = new List<Card>(12);
		List<Card> tempEnemyCharactersBucket = new List<Card>(12);
		List<Card> mercsinOrder = new List<Card>(12);
		foreach (KeyValuePair<int, List<Card>> item in cardsBySpeed)
		{
			List<Card> bucket = item.Value;
			if (bucket.Count == 1)
			{
				mercsinOrder.Add(bucket[0]);
				continue;
			}
			tempFriendlyCharactersBucket.Clear();
			tempEnemyCharactersBucket.Clear();
			foreach (Card c3 in bucket)
			{
				if (c3.GetEntity().IsControlledByFriendlySidePlayer())
				{
					tempFriendlyCharactersBucket.Add(c3);
				}
				else
				{
					tempEnemyCharactersBucket.Add(c3);
				}
			}
			tempFriendlyCharactersBucket.Sort(delegate(Card c1, Card c2)
			{
				int tag = c1.GetEntity().GetTag(GAME_TAG.LETTUCE_SELECTED_ABILITY_QUEUE_ORDER);
				int tag2 = c2.GetEntity().GetTag(GAME_TAG.LETTUCE_SELECTED_ABILITY_QUEUE_ORDER);
				return tag.CompareTo(tag2);
			});
			tempEnemyCharactersBucket.Sort(delegate(Card c1, Card c2)
			{
				int tag3 = c1.GetEntity().GetTag(GAME_TAG.ZONE_POSITION);
				int tag4 = c2.GetEntity().GetTag(GAME_TAG.ZONE_POSITION);
				return tag3.CompareTo(tag4);
			});
			foreach (Card c4 in tempFriendlyCharactersBucket)
			{
				mercsinOrder.Add(c4);
			}
			foreach (Card c5 in tempEnemyCharactersBucket)
			{
				mercsinOrder.Add(c5);
			}
		}
		return mercsinOrder;
	}
}
