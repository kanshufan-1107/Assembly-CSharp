using System.Collections.Generic;
using UnityEngine;

public class ZoneSecret : Zone
{
	private Vector3? m_originalPosition;

	private ZoneTransitionStyle m_transitionStyleOverride;

	private const float MAX_LAYOUT_PYRAMID_LEVEL = 2f;

	private const float LAYOUT_ANIM_SEC = 1f;

	private void Awake()
	{
		if (GameState.Get() != null)
		{
			GameState.Get().RegisterGameOverListener(OnGameOver);
		}
	}

	public void SetZoneTransitionStyleOverride(ZoneTransitionStyle zoneTransitionStyle)
	{
		m_transitionStyleOverride = zoneTransitionStyle;
	}

	public void SetOriginalPosition(Vector3 position)
	{
		m_originalPosition = position;
	}

	public override void UpdateLayout()
	{
		m_updatingLayout++;
		if (!m_originalPosition.HasValue)
		{
			m_originalPosition = base.transform.localPosition;
		}
		Card heroCard = ((m_controller != null) ? m_controller.GetHeroCard() : null);
		Actor heroActor = ((heroCard != null) ? heroCard.GetActor() : null);
		if ((bool)heroActor)
		{
			base.transform.localPosition = m_originalPosition.Value + new Vector3(0f, heroActor.ZoneHeroPositionOffset, 0f);
		}
		else
		{
			base.transform.localPosition = m_originalPosition.Value;
		}
		if (IsBlockingLayout())
		{
			UpdateLayoutFinished();
		}
		else if ((bool)UniversalInputManager.UsePhoneUI)
		{
			UpdateLayout_Phone();
		}
		else
		{
			UpdateLayout_Default();
		}
	}

	public List<Card> GetSecretCards()
	{
		List<Card> secrets = new List<Card>();
		foreach (Card card in m_cards)
		{
			if (card.GetEntity() != null && card.GetEntity().IsSecret())
			{
				secrets.Add(card);
			}
		}
		return secrets;
	}

	public List<Card> GetSideQuestCards()
	{
		List<Card> sideQuests = new List<Card>();
		foreach (Card card in m_cards)
		{
			if (card.GetEntity() != null && card.GetEntity().IsSideQuest())
			{
				sideQuests.Add(card);
			}
		}
		return sideQuests;
	}

	public List<Card> GetSigilCards()
	{
		List<Card> sigils = new List<Card>();
		foreach (Card card in m_cards)
		{
			if (card.GetEntity() != null && card.GetEntity().IsSigil())
			{
				sigils.Add(card);
			}
		}
		return sigils;
	}

	public List<Card> GetObjectiveCards()
	{
		List<Card> objective = new List<Card>();
		foreach (Card card in m_cards)
		{
			if (card.GetEntity() != null && card.GetEntity().IsObjective())
			{
				objective.Add(card);
			}
		}
		return objective;
	}

	public Entity GetPuzzleEntity()
	{
		foreach (Card card in m_cards)
		{
			if (card.GetEntity() != null && card.GetEntity().IsPuzzle())
			{
				return card.GetEntity();
			}
		}
		return null;
	}

	public int GetSecretCount()
	{
		int numSecrets = 0;
		foreach (Card card in m_cards)
		{
			if (card.GetEntity() != null && card.GetEntity().IsSecret())
			{
				numSecrets++;
			}
		}
		return numSecrets;
	}

	public int GetSideQuestCount()
	{
		int numSideQuests = 0;
		foreach (Card card in m_cards)
		{
			if (card.GetEntity() != null && card.GetEntity().IsSideQuest())
			{
				numSideQuests++;
			}
		}
		return numSideQuests;
	}

	public override void OnHealingDoesDamageEntityEnteredPlay()
	{
	}

	public override void OnHealingDoesDamageEntityMousedOut()
	{
	}

	public override void OnHealingDoesDamageEntityMousedOver()
	{
	}

	public override void OnLifestealDoesDamageEntityEnteredPlay()
	{
	}

	public override void OnLifestealDoesDamageEntityMousedOut()
	{
	}

	public override void OnLifestealDoesDamageEntityMousedOver()
	{
	}

	private void UpdateLayout_Default()
	{
		SortQuestsToTop();
		Vector2 heroOffsets = new Vector2(1f, 2f);
		if (m_controller != null)
		{
			Card heroCard = m_controller.GetHeroCard();
			if (heroCard != null && heroCard.GetActor() != null)
			{
				Bounds heroBounds = heroCard.GetActor().GetMeshRenderer().bounds;
				heroOffsets.x = heroBounds.extents.x;
				heroOffsets.y = heroBounds.extents.z * 0.9f;
			}
		}
		float verticalPyramidOffset = 0.6f * heroOffsets.y;
		int cardsAnimated = 0;
		for (int i = 0; i < m_cards.Count; i++)
		{
			Card card = m_cards[i];
			if (CanAnimateCard(card))
			{
				bool num = !card.IsShown();
				card.ShowCard();
				if (num)
				{
					card.ShowSecretQuestBirth();
				}
				Vector3 secretPosition = base.transform.position;
				float pyramidLevel = i + 1 >> 1;
				int num2 = i & 1;
				float pyramidXScalar = ((pyramidLevel > 2f) ? 1f : ((!Mathf.Approximately(pyramidLevel, 1f)) ? (pyramidLevel / 2f) : 0.6f));
				if (num2 == 0)
				{
					secretPosition.x += heroOffsets.x * pyramidXScalar;
				}
				else
				{
					secretPosition.x -= heroOffsets.x * pyramidXScalar;
				}
				secretPosition.z -= heroOffsets.y * (pyramidXScalar * pyramidXScalar);
				if (pyramidLevel > 2f)
				{
					secretPosition.z -= verticalPyramidOffset * (pyramidLevel - 2f);
				}
				if (card.GetHeroCard() != null && card.GetHeroCard().GetActor() != null)
				{
					Actor heroActor = card.GetHeroCard().GetActor();
					secretPosition += heroActor.GetCustomFrameSecretZoneOffset(i);
				}
				Vector3 secretScale = base.transform.localScale;
				if (card.GetHeroCard() != null && card.GetHeroCard().GetActor() != null)
				{
					Actor heroActor2 = card.GetHeroCard().GetActor();
					secretScale *= heroActor2.GetCustomFrameSecretZoneScale();
				}
				iTween.Stop(card.gameObject);
				ZoneTransitionStyle transitionStyle = card.GetTransitionStyle();
				card.SetTransitionStyle(ZoneTransitionStyle.NORMAL);
				if (transitionStyle == ZoneTransitionStyle.INSTANT || m_transitionStyleOverride == ZoneTransitionStyle.INSTANT)
				{
					card.EnableTransitioningZones(enable: false);
					card.transform.position = secretPosition;
					card.transform.rotation = base.transform.rotation;
					card.transform.localScale = secretScale;
				}
				else
				{
					card.EnableTransitioningZones(enable: true);
					cardsAnimated++;
					iTween.MoveTo(card.gameObject, secretPosition, 1f);
					iTween.RotateTo(card.gameObject, base.transform.localEulerAngles, 1f);
					iTween.ScaleTo(card.gameObject, secretScale, 1f);
				}
			}
		}
		if (cardsAnimated > 0)
		{
			StartFinishLayoutTimer(1f);
		}
		else
		{
			UpdateLayoutFinished();
		}
	}

	private void UpdateLayout_Phone()
	{
		int cardsAnimated = 0;
		SortQuestsToTop();
		bool hasMainQuest = HaveMainQuest();
		int secretPos = 0;
		int sigilPos = 0;
		int sideQuestPos = 0;
		int objectivePos = 0;
		int numCardTypes = 0;
		GetZoneInfo(ref numCardTypes, ref secretPos, ref sigilPos, ref sideQuestPos, ref objectivePos);
		for (int i = 0; i < m_cards.Count; i++)
		{
			Card card = m_cards[i];
			Entity entity = card.GetEntity();
			if (!CanAnimateCard(card))
			{
				continue;
			}
			iTween.Stop(card.gameObject);
			if (entity.IsSecret() && GetSecretCards().IndexOf(card) == 0)
			{
				if (!card.IsShown())
				{
					card.ShowExhaustedChange(entity.IsExhausted());
					card.ShowCard();
				}
				Actor actor = card.GetActor();
				if (actor != null)
				{
					actor.UpdateAllComponents();
				}
			}
			if (entity.IsSideQuest() && GetSideQuestCards().IndexOf(card) == 0)
			{
				if (!card.IsShown())
				{
					card.ShowExhaustedChange(entity.IsExhausted());
					card.ShowCard();
				}
				Actor actor2 = card.GetActor();
				if (actor2 != null)
				{
					actor2.UpdateAllComponents();
				}
			}
			if (entity.IsSigil() && GetSigilCards().IndexOf(card) == 0)
			{
				if (!card.IsShown())
				{
					card.ShowExhaustedChange(entity.IsExhausted());
					card.ShowCard();
				}
				Actor actor3 = card.GetActor();
				if (actor3 != null)
				{
					actor3.UpdateAllComponents();
				}
			}
			if (entity.IsObjective() && GetObjectiveCards().IndexOf(card) == 0)
			{
				if (!card.IsShown())
				{
					card.ShowExhaustedChange(entity.IsExhausted());
					card.ShowCard();
				}
				Actor actor4 = card.GetActor();
				if (actor4 != null)
				{
					actor4.UpdateAllComponents();
				}
			}
			Vector3 secretPosition = base.transform.position;
			if (numCardTypes == 2 && !hasMainQuest)
			{
				Vector3[] posOffsetsTwo = new Vector3[2]
				{
					new Vector3(-0.5f, 0f, -0.1f),
					new Vector3(0.5f, 0f, -0.1f)
				};
				if (entity.IsSecret())
				{
					if (secretPos >= posOffsetsTwo.Length)
					{
						Log.Gameplay.PrintError("UpdateLayout_Phone() - Secret Position overflow, use position 0 instead");
						secretPos = 0;
					}
					secretPosition += posOffsetsTwo[secretPos];
				}
				else if (entity.IsSigil())
				{
					if (sigilPos >= posOffsetsTwo.Length)
					{
						Log.Gameplay.PrintError("UpdateLayout_Phone() - Sigil Position overflow, use position 0 instead");
						sigilPos = 0;
					}
					secretPosition += posOffsetsTwo[sigilPos];
				}
				else if (entity.IsSideQuest())
				{
					if (sideQuestPos >= posOffsetsTwo.Length)
					{
						Log.Gameplay.PrintError("UpdateLayout_Phone() - Sidequest Position overflow, use position 0 instead");
						sideQuestPos = 0;
					}
					secretPosition += posOffsetsTwo[sideQuestPos];
				}
				else if (entity.IsObjective())
				{
					if (objectivePos >= posOffsetsTwo.Length)
					{
						Log.Gameplay.PrintError("UpdateLayout_Phone() - Objective Position overflow, use position 0 instead");
						objectivePos = 0;
					}
					secretPosition += posOffsetsTwo[objectivePos];
				}
			}
			else if (numCardTypes > 1)
			{
				Vector3[] posOffsets = new Vector3[5]
				{
					new Vector3(0f, 0f, 0f),
					new Vector3(-0.7f, 0f, -0.2f),
					new Vector3(0.7f, 0f, -0.2f),
					new Vector3(-0.9f, 0f, -0.85f),
					new Vector3(0.9f, 0f, -0.85f)
				};
				if (entity.IsQuestOrQuestline() || entity.IsBobQuest())
				{
					int questPos = i;
					if (questPos >= posOffsets.Length)
					{
						Log.Gameplay.PrintError("UpdateLayout_Phone() - quest Position overflow, use position 0 instead");
						questPos = 0;
					}
					secretPosition += posOffsets[questPos];
				}
				else if (entity.IsSecret())
				{
					if (secretPos >= posOffsets.Length)
					{
						Log.Gameplay.PrintError("UpdateLayout_Phone() - Secret Position overflow, use position 0 instead");
						secretPos = 0;
					}
					secretPosition += posOffsets[secretPos];
				}
				else if (entity.IsSigil())
				{
					if (sigilPos >= posOffsets.Length)
					{
						Log.Gameplay.PrintError("UpdateLayout_Phone() - Sigil Position overflow, use position 0 instead");
						sigilPos = 0;
					}
					secretPosition += posOffsets[sigilPos];
				}
				else if (entity.IsSideQuest())
				{
					if (sideQuestPos >= posOffsets.Length)
					{
						Log.Gameplay.PrintError("UpdateLayout_Phone() - Sidequest Position overflow, use position 0 instead");
						sideQuestPos = 0;
					}
					secretPosition += posOffsets[sideQuestPos];
				}
				else if (entity.IsObjective())
				{
					if (objectivePos >= posOffsets.Length)
					{
						Log.Gameplay.PrintError("UpdateLayout_Phone() - Objective Position overflow, use position 0 instead");
						objectivePos = 0;
					}
					secretPosition += posOffsets[objectivePos];
				}
			}
			if (card.GetHeroCard() != null && card.GetHeroCard().GetActor() != null)
			{
				Actor heroActor = card.GetHeroCard().GetActor();
				secretPosition += heroActor.GetCustomFrameSecretZoneOffset(i);
			}
			Vector3 secretScale = base.transform.localScale;
			if (card.GetHeroCard() != null && card.GetHeroCard().GetActor() != null)
			{
				Actor heroActor2 = card.GetHeroCard().GetActor();
				secretScale *= heroActor2.GetCustomFrameSecretZoneScale();
			}
			ZoneTransitionStyle transitionStyle = card.GetTransitionStyle();
			card.SetTransitionStyle(ZoneTransitionStyle.NORMAL);
			if (transitionStyle == ZoneTransitionStyle.INSTANT)
			{
				card.EnableTransitioningZones(enable: false);
				card.transform.position = secretPosition;
			}
			else
			{
				card.EnableTransitioningZones(enable: true);
				cardsAnimated++;
				iTween.MoveTo(card.gameObject, secretPosition, 1f);
			}
			card.transform.rotation = base.transform.rotation;
			card.transform.localScale = secretScale;
		}
		if (cardsAnimated > 0)
		{
			StartFinishLayoutTimer(1f);
		}
		else
		{
			UpdateLayoutFinished();
		}
	}

	private void SortQuestsToTop()
	{
		int lastQuestIndex = 0;
		for (int cardIndex = 0; cardIndex < m_cards.Count; cardIndex++)
		{
			Card card = m_cards[cardIndex];
			Entity entity = card.GetEntity();
			if (entity.IsQuest() || entity.IsQuestline() || entity.IsBobQuest())
			{
				if (cardIndex > lastQuestIndex)
				{
					m_cards.RemoveAt(cardIndex);
					m_cards.Insert((!entity.IsBobQuest()) ? lastQuestIndex : 0, card);
				}
				lastQuestIndex++;
			}
		}
	}

	private void GetZoneInfo(ref int numCardTypes, ref int secretPos, ref int sigilPos, ref int sideQuestPos, ref int objectivePos)
	{
		bool hasQuest = false;
		bool hasSecret = false;
		bool hasSigil = false;
		bool hasSideQuest = false;
		bool hasObjective = false;
		numCardTypes = (secretPos = (sigilPos = (sideQuestPos = (objectivePos = 0))));
		foreach (Card card in m_cards)
		{
			if (card.GetEntity().IsQuest() || card.GetEntity().IsQuestline())
			{
				if (!hasQuest)
				{
					hasQuest = true;
				}
				numCardTypes++;
				secretPos++;
				sigilPos++;
				sideQuestPos++;
				objectivePos++;
			}
			else if (card.GetEntity().IsSecret() && !hasSecret)
			{
				hasSecret = true;
				numCardTypes++;
				sigilPos++;
				sideQuestPos++;
				objectivePos++;
			}
			else if (card.GetEntity().IsSigil() && !hasSigil)
			{
				hasSigil = true;
				numCardTypes++;
				sideQuestPos++;
				objectivePos++;
			}
			else if (card.GetEntity().IsSideQuest() && !hasSideQuest)
			{
				hasSideQuest = true;
				objectivePos++;
				numCardTypes++;
			}
			else if (card.GetEntity().IsObjective() && !hasObjective)
			{
				hasObjective = true;
				numCardTypes++;
			}
			else
			{
				Debug.LogWarningFormat("GetZoneInfo() - Unknown secret zone card type");
			}
		}
	}

	private bool HaveMainQuest()
	{
		foreach (Card card in m_cards)
		{
			Entity ent = card.GetEntity();
			if (ent != null && (ent.IsQuestOrQuestline() || card.GetEntity().IsBobQuest()))
			{
				return true;
			}
		}
		return false;
	}

	public override bool AddCard(Card card)
	{
		bool result = base.AddCard(card);
		card.ActivateCharacterPlayEffects();
		return result;
	}

	private bool CanAnimateCard(Card card)
	{
		if (card.IsDoNotSort())
		{
			return false;
		}
		return true;
	}

	private void OnGameOver(TAG_PLAYSTATE playState, object userData)
	{
		Player controller = GetController();
		if (controller == null || controller.GetTag<TAG_PLAYSTATE>(GAME_TAG.PLAYSTATE) == TAG_PLAYSTATE.WON)
		{
			return;
		}
		for (int i = 0; i < m_cards.Count; i++)
		{
			Card card = m_cards[i];
			if (!(card == null) && CanAnimateCard(card))
			{
				card.HideCard();
			}
		}
	}
}
