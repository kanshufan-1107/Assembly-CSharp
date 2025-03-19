using System;
using System.Collections.Generic;
using UnityEngine;

public class LettuceFakeHandController
{
	private struct OriginalSetting
	{
		public Actor m_Actor { get; set; }

		public Vector3 m_Position { get; set; }

		public Vector3 m_LocalEulerAngles { get; set; }

		public Vector3 m_LocalScale { get; set; }
	}

	private readonly List<OriginalSetting> m_originalSettings = new List<OriginalSetting>();

	private readonly List<Actor> m_fakeHandActors = new List<Actor>();

	private bool m_shown;

	private bool ShouldShowOpposingFakeHand()
	{
		foreach (Player opposingPlayer in GameState.Get().GetOpposingPlayers())
		{
			if (opposingPlayer.HasTag(GAME_TAG.LETTUCE_MERCENARIES_TO_NOMINATE))
			{
				return true;
			}
		}
		return false;
	}

	public void ShowOpposingFakeHand(Action onFinish)
	{
		if (m_shown)
		{
			onFinish();
			return;
		}
		if (!ShouldShowOpposingFakeHand())
		{
			m_shown = false;
			onFinish();
			return;
		}
		Player opposingSidePlayer = GameState.Get().GetOpposingSidePlayer();
		List<Card> cards = opposingSidePlayer.GetDeckZone().GetCards();
		int handSize = cards.Count;
		if (handSize == 0)
		{
			m_shown = false;
			onFinish();
			return;
		}
		m_shown = true;
		m_originalSettings.Clear();
		ZoneHand handZone = opposingSidePlayer.GetHandZone();
		int animationCount = handSize;
		for (int handIndex = 0; handIndex < handSize; handIndex++)
		{
			if (handIndex >= m_fakeHandActors.Count)
			{
				GameObject go = AssetLoader.Get().InstantiatePrefab("Card_Hidden.prefab:1a94649d257bc284ca6e2962f634a8b9", AssetLoadingOptions.IgnorePrefabPosition);
				m_fakeHandActors.Add(go.GetComponent<Actor>());
			}
			Actor fakeActor = m_fakeHandActors[handIndex];
			Card card = cards[handIndex];
			Entity entity = card.GetEntity();
			fakeActor.SetCard(card);
			fakeActor.SetCardDefFromCard(card);
			fakeActor.SetEntity(entity);
			fakeActor.SetEntityDef(entity.GetEntityDef());
			fakeActor.SetCardBackSideOverride(entity.GetControllerSide());
			fakeActor.UpdateAllComponents();
			m_originalSettings.Add(new OriginalSetting
			{
				m_Actor = card.GetActor(),
				m_Position = card.transform.position,
				m_LocalEulerAngles = card.transform.localEulerAngles,
				m_LocalScale = card.transform.localScale
			});
			card.transform.position = handZone.GetCardPosition(handIndex, handSize);
			card.transform.localEulerAngles = handZone.GetCardRotation(handIndex, handSize);
			card.transform.localScale = handZone.GetCardScale();
			fakeActor.Hide();
			card.SetActor(fakeActor);
			card.ActivateActorSpell(SpellType.SUMMON_IN, delegate
			{
				int num = animationCount - 1;
				animationCount = num;
				if (animationCount == 0)
				{
					onFinish();
				}
			});
		}
	}

	public void HideOpposingFakeHand(Action onFinish)
	{
		if (!m_shown)
		{
			onFinish();
			return;
		}
		m_shown = false;
		List<Card> cards = GameState.Get().GetOpposingSidePlayer().GetDeckZone()
			.GetCards();
		int animationCount = cards.Count;
		if (animationCount == 0)
		{
			onFinish();
			return;
		}
		for (int i = 0; i < cards.Count && i < m_fakeHandActors.Count && i < m_originalSettings.Count; i++)
		{
			Card card = cards[i];
			Actor fakeActor = m_fakeHandActors[i];
			if (!(fakeActor.transform.parent != null))
			{
				continue;
			}
			OriginalSetting setting = m_originalSettings[i];
			card.ActivateActorSpell(SpellType.SUMMON_OUT, delegate
			{
				fakeActor.ReleaseSpell(SpellType.SUMMON_OUT);
				fakeActor.SetCard(null);
				card.SetActor(setting.m_Actor);
				card.transform.position = setting.m_Position;
				card.transform.localEulerAngles = setting.m_LocalEulerAngles;
				card.transform.localScale = setting.m_LocalScale;
				int num = animationCount - 1;
				animationCount = num;
				if (animationCount == 0)
				{
					onFinish();
				}
			});
		}
	}
}
