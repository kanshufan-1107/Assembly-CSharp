using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomChoiceRevealSpell : CustomChoiceSpell
{
	public List<Transform> m_bones;

	private List<Actor> m_fakeActors;

	protected override void OnAction(SpellStateType prevStateType)
	{
		base.OnAction(prevStateType);
		DoEffect();
	}

	public override void OnSpellEvent(string eventName, object eventData)
	{
		base.OnSpellEvent(eventName, eventData);
		if (eventName == "showCards")
		{
			StartCoroutine(ShowCards());
		}
	}

	private void DoEffect()
	{
		foreach (Card card in m_choiceState.m_cards)
		{
			card.SetInputEnabled(enabled: false);
		}
		LoadFakeActors();
		PlayMakerFSM component = GetComponent<PlayMakerFSM>();
		component.FsmVariables.GetFsmGameObject("CardA").Value = m_fakeActors[0].gameObject;
		component.FsmVariables.GetFsmGameObject("CardB").Value = m_fakeActors[1].gameObject;
		component.FsmVariables.GetFsmGameObject("CardC").Value = m_fakeActors[2].gameObject;
	}

	private void LoadFakeActors()
	{
		Player.Side cardBackSide = ((!m_choiceState.m_isFriendly) ? Player.Side.FRIENDLY : Player.Side.OPPOSING);
		m_fakeActors = new List<Actor>();
		for (int i = 0; i < m_choiceState.m_cards.Count; i++)
		{
			Actor fakeActor = AssetLoader.Get().InstantiatePrefab("Card_Hidden.prefab:1a94649d257bc284ca6e2962f634a8b9", AssetLoadingOptions.IgnorePrefabPosition).GetComponent<Actor>();
			fakeActor.SetCardBackSideOverride(cardBackSide);
			fakeActor.UpdateAllComponents();
			TransformUtil.CopyWorld(fakeActor, m_choiceState.m_cards[i].GetActor());
			m_fakeActors.Add(fakeActor);
			if (i < m_bones.Count)
			{
				fakeActor.transform.parent = m_bones[i];
				fakeActor.transform.position = m_bones[i].position;
			}
		}
	}

	private IEnumerator ShowCards()
	{
		int i = 0;
		while (i < m_fakeActors.Count)
		{
			Card card = m_choiceState.m_cards[i];
			Actor fakeActor = m_fakeActors[i];
			if (!card.GetEntity().IsHidden())
			{
				if (i == m_fakeActors.Count - 1)
				{
					yield return StartCoroutine(SpellUtils.FlipActorAndReplaceWithCard(fakeActor, card, 0.5f));
				}
				else
				{
					StartCoroutine(SpellUtils.FlipActorAndReplaceWithCard(fakeActor, card, 0.5f));
				}
			}
			else
			{
				card.ShowCard();
				Player.Side cardBackSide = ((!m_choiceState.m_isFriendly) ? Player.Side.FRIENDLY : Player.Side.OPPOSING);
				card.GetActor().SetCardBackSideOverride(cardBackSide);
				card.GetActor().UpdateAllComponents();
				fakeActor.Hide();
			}
			int num = i + 1;
			i = num;
		}
		OnSpellFinished();
		OnStateFinished();
		foreach (Card card2 in m_choiceState.m_cards)
		{
			card2.SetInputEnabled(enabled: true);
		}
		foreach (Actor fakeActor2 in m_fakeActors)
		{
			fakeActor2.Destroy();
		}
	}
}
