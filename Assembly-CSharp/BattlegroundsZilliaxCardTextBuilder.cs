using System.Collections.Generic;
using UnityEngine;

public class BattlegroundsZilliaxCardTextBuilder : CardTextBuilder
{
	private readonly Dictionary<int, GAME_TAG> m_zilliaxMinionDBIDToKeywords = new Dictionary<int, GAME_TAG>
	{
		{
			107909,
			GAME_TAG.TAUNT
		},
		{
			107910,
			GAME_TAG.REBORN
		},
		{
			107911,
			GAME_TAG.DIVINE_SHIELD
		},
		{
			108931,
			GAME_TAG.STEALTH
		},
		{
			109802,
			GAME_TAG.WINDFURY
		},
		{
			109811,
			GAME_TAG.MAGNETIC
		}
	};

	public BattlegroundsZilliaxCardTextBuilder()
	{
		m_useEntityForTextInPlay = true;
	}

	protected string BuildCardText(string rawText, int[] baseMinionIDs)
	{
		string baseMinionsKeywordsStr = "";
		HashSet<int> appearedID = new HashSet<int>();
		foreach (int baseMinionID in baseMinionIDs)
		{
			if (baseMinionID != 0 && !appearedID.Contains(baseMinionID))
			{
				appearedID.Add(baseMinionID);
				if (!m_zilliaxMinionDBIDToKeywords.ContainsKey(baseMinionID))
				{
					Debug.LogWarning($"BattlegroundsZilliaxCardTextBuilder - Unknown base zilliax module {baseMinionID}");
				}
				else
				{
					baseMinionsKeywordsStr = ((baseMinionsKeywordsStr.Length != 0) ? (baseMinionsKeywordsStr + "\n" + GameStrings.GetKeywordName(m_zilliaxMinionDBIDToKeywords[baseMinionID])) : GameStrings.GetKeywordName(m_zilliaxMinionDBIDToKeywords[baseMinionID]));
				}
			}
		}
		return TextUtils.TransformCardText(rawText.Replace("@", baseMinionsKeywordsStr));
	}

	private int GetTripleBaseMinionID(Entity entity, GAME_TAG tag)
	{
		int value = entity.GetTag(tag);
		if (value == 0)
		{
			value = entity.GetEntityDef().GetTag(tag);
		}
		return value;
	}

	protected string BuildCardTextInternal(Entity entity)
	{
		string rawText = CardTextBuilder.GetRawCardTextInHand(entity.GetCardId());
		return BuildCardText(rawText, new int[3]
		{
			GetTripleBaseMinionID(entity, GAME_TAG.BACON_TRIPLED_BASE_MINION_ID),
			GetTripleBaseMinionID(entity, GAME_TAG.BACON_TRIPLED_BASE_MINION_ID2),
			GetTripleBaseMinionID(entity, GAME_TAG.BACON_TRIPLED_BASE_MINION_ID3)
		});
	}

	protected string BuildCardTextInternal(EntityDef entityDef)
	{
		string rawText = CardTextBuilder.GetRawCardTextInHand(entityDef.GetCardId());
		return BuildCardText(rawText, new int[3]
		{
			entityDef.GetTag(GAME_TAG.BACON_TRIPLED_BASE_MINION_ID),
			entityDef.GetTag(GAME_TAG.BACON_TRIPLED_BASE_MINION_ID2),
			entityDef.GetTag(GAME_TAG.BACON_TRIPLED_BASE_MINION_ID3)
		});
	}

	public override string BuildCardTextInHand(Entity entity)
	{
		return BuildCardTextInternal(entity);
	}

	public override string BuildCardTextInHistory(Entity entity)
	{
		return BuildCardTextInternal(entity);
	}

	public override string BuildCardTextInHand(EntityDef entityDef)
	{
		return BuildCardTextInternal(entityDef);
	}

	public override void OnTagChange(Card card, TagDelta tagChange)
	{
		GAME_TAG tag = (GAME_TAG)tagChange.tag;
		if (tag == GAME_TAG.BACON_TRIPLED_BASE_MINION_ID || (uint)(tag - 3499) <= 1u)
		{
			if (card != null && card.GetActor() != null)
			{
				card.GetActor().UpdateTextComponents();
			}
		}
		else
		{
			base.OnTagChange(card, tagChange);
		}
	}
}
