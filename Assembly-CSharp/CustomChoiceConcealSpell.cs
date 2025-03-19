using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Game.Spells;
using PegasusGame;
using UnityEngine;

public class CustomChoiceConcealSpell : CustomChoiceSpell
{
	public Spell m_WrongChoiceSpell;

	public Spell m_CorrectChoiceSpell;

	public Spell m_SuperCorrectChoiceSpell;

	public Spell m_HiddenWrongChoiceSpell;

	public Spell m_HiddenCorrectChoiceSpell;

	public Spell m_HiddenSuperCorrectChoiceSpell;

	public Spell m_CorrectChoiceFadeAwaySpell;

	public float m_SendCardBackToOpponentsDeckDelay = 0.25f;

	private bool m_choseCorrectly;

	private Card m_correctCard;

	private Actor m_fakeCorrectCardActor;

	private Actor m_fakeCorrectCardBackActor;

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (m_fakeCorrectCardActor != null)
		{
			m_fakeCorrectCardActor.Destroy();
		}
		if (m_fakeCorrectCardBackActor != null)
		{
			m_fakeCorrectCardBackActor.Destroy();
		}
	}

	protected override void OnAction(SpellStateType prevStateType)
	{
		base.OnAction(prevStateType);
		StartCoroutine(DoEffects());
	}

	private IEnumerator DoEffects()
	{
		while (!FindResultOfChoice())
		{
			yield return null;
		}
		if (!m_choseCorrectly && !LoadFakeActors())
		{
			OnSpellFinished();
			OnStateFinished();
			yield break;
		}
		yield return StartCoroutine(PlayChoiceEffects());
		ResetCorrectCardCardback();
		foreach (Card card in m_choiceState.m_cards)
		{
			if (!(card == m_correctCard) || !m_choseCorrectly)
			{
				card.HideCard();
			}
		}
		if (!m_choseCorrectly)
		{
			m_fakeCorrectCardActor.Show();
			yield return new WaitForSeconds(m_SendCardBackToOpponentsDeckDelay);
			if (!m_fakeCorrectCardActor.GetEntity().IsHidden())
			{
				yield return StartCoroutine(SpellUtils.FlipActorAndReplaceWithOtherActor(m_fakeCorrectCardActor, m_fakeCorrectCardBackActor, 0.5f));
			}
			else
			{
				m_fakeCorrectCardBackActor.Show();
				m_fakeCorrectCardActor.Hide();
			}
			PlayFadeAwaySpellThenFinish();
		}
		else
		{
			FinishSpell();
		}
	}

	private void FinishSpell()
	{
		OnSpellFinished();
		OnStateFinished();
	}

	private bool LoadFakeActors()
	{
		GameObject fakeCorrectCardActorGO = AssetLoader.Get().InstantiatePrefab(m_correctCard.GetActorAssetPath(), AssetLoadingOptions.IgnorePrefabPosition);
		if (fakeCorrectCardActorGO == null)
		{
			Log.Spells.PrintError("CustomChoiceConcealSpell.LoadFakeActors(): Failed to load fake actor for card " + m_correctCard);
			return false;
		}
		Player.Side side = Player.GetOppositePlayerSide(m_correctCard.GetControllerSide());
		m_fakeCorrectCardActor = fakeCorrectCardActorGO.GetComponent<Actor>();
		m_fakeCorrectCardActor.SetCardDefFromCard(m_correctCard);
		m_fakeCorrectCardActor.SetEntity(m_correctCard.GetEntity());
		m_fakeCorrectCardActor.SetEntityDef(m_correctCard.GetEntity().GetEntityDef());
		m_fakeCorrectCardActor.SetCardBackSideOverride(side);
		m_fakeCorrectCardActor.UpdateAllComponents();
		TransformUtil.CopyWorld(m_fakeCorrectCardActor, m_correctCard.GetActor());
		m_fakeCorrectCardActor.Hide();
		GameObject fakeActorBackGO = AssetLoader.Get().InstantiatePrefab("Card_Hidden.prefab:1a94649d257bc284ca6e2962f634a8b9", AssetLoadingOptions.IgnorePrefabPosition);
		if (fakeActorBackGO == null)
		{
			Log.Spells.PrintError("CustomChoiceConcealSpell.LoadFakeActors(): Failed to load fake card back actor.");
			return false;
		}
		m_fakeCorrectCardBackActor = fakeActorBackGO.GetComponent<Actor>();
		m_fakeCorrectCardBackActor.SetCardBackSideOverride(side);
		m_fakeCorrectCardBackActor.UpdateAllComponents();
		TransformUtil.CopyWorld(m_fakeCorrectCardBackActor, m_correctCard.GetActor());
		m_fakeCorrectCardBackActor.Hide();
		return true;
	}

	private void OnEffectStateFinished(Spell spell, SpellStateType prevStateType, object userData)
	{
		if (spell != null && spell.GetActiveState() == SpellStateType.NONE)
		{
			SpellManager.Get().ReleaseSpell(spell);
		}
	}

	private void OnChoiceEffectSpellEvent(string eventName, object eventData, object userData)
	{
		if (eventName == "ResetCorrectCardCardBack")
		{
			ResetCorrectCardCardback();
		}
	}

	private IEnumerator PlayChoiceEffects()
	{
		int effectsToWaitFor = 0;
		ISpellCallbackHandler<Spell>.FinishedCallback OnEffectFinished = delegate
		{
			int num = effectsToWaitFor - 1;
			effectsToWaitFor = num;
		};
		foreach (Card card in m_choiceState.m_cards)
		{
			bool num2 = card.GetEntity().IsHidden();
			Spell correctChoiceSpell = (num2 ? m_HiddenCorrectChoiceSpell : m_CorrectChoiceSpell);
			Spell superCorrectChoiceSpell = (num2 ? m_HiddenSuperCorrectChoiceSpell : m_SuperCorrectChoiceSpell);
			Spell wrongChoiceSpell = (num2 ? m_HiddenWrongChoiceSpell : m_WrongChoiceSpell);
			Actor actor = card.GetActor();
			SpellManager spellManager = SpellManager.Get();
			if (card == m_correctCard)
			{
				int num3 = effectsToWaitFor + 1;
				effectsToWaitFor = num3;
				Spell obj = (m_choseCorrectly ? spellManager.GetSpell(superCorrectChoiceSpell) : spellManager.GetSpell(correctChoiceSpell));
				obj.transform.parent = actor.transform;
				TransformUtil.Identity(obj);
				obj.AddFinishedCallback(OnEffectFinished);
				obj.AddStateFinishedCallback(OnEffectStateFinished);
				obj.AddSpellEventCallback(OnChoiceEffectSpellEvent);
				obj.Activate();
			}
			else
			{
				int num3 = effectsToWaitFor + 1;
				effectsToWaitFor = num3;
				Spell spell2 = spellManager.GetSpell(wrongChoiceSpell);
				spell2.transform.parent = actor.transform;
				TransformUtil.Identity(spell2);
				spell2.AddFinishedCallback(OnEffectFinished);
				spell2.AddStateFinishedCallback(OnEffectStateFinished);
				spell2.Activate();
			}
		}
		while (effectsToWaitFor > 0)
		{
			yield return null;
		}
	}

	private void PlayFadeAwaySpellThenFinish()
	{
		ISpellCallbackHandler<Spell>.FinishedCallback OnFadeAwayFinished = delegate
		{
			m_fakeCorrectCardBackActor.Hide();
			FinishSpell();
		};
		Spell spell2 = SpellManager.Get().GetSpell(m_CorrectChoiceFadeAwaySpell);
		spell2.transform.parent = m_correctCard.GetActor().transform;
		TransformUtil.Identity(spell2);
		spell2.AddFinishedCallback(OnFadeAwayFinished);
		spell2.AddStateFinishedCallback(OnEffectStateFinished);
		spell2.Activate();
	}

	private void ResetCorrectCardCardback()
	{
		m_correctCard.GetActor().SetCardBackSideOverride(null);
		m_correctCard.GetActor().UpdateCardBack();
		if (m_correctCard.GetControllerSide() == Player.Side.FRIENDLY)
		{
			m_correctCard.SetTransitionStyle(ZoneTransitionStyle.SLOW);
		}
	}

	private bool FindResultOfChoice()
	{
		List<PowerTaskList> powerQueue = new List<PowerTaskList>();
		powerQueue.Add(GameState.Get().GetPowerProcessor().GetCurrentTaskList());
		foreach (PowerTaskList powerTaskList in GameState.Get().GetPowerProcessor().GetPowerQueue()
			.GetList())
		{
			powerQueue.Add(powerTaskList);
		}
		foreach (PowerTaskList powerTaskList2 in powerQueue)
		{
			if (powerTaskList2 == null)
			{
				continue;
			}
			foreach (PowerTask task in powerTaskList2.GetTaskList())
			{
				if (!(task.GetPower() is Network.HistMetaData { MetaType: HistoryMeta.Type.TARGET } metaData))
				{
					continue;
				}
				Entity targetEntity = GameState.Get().GetEntity(metaData.Info[0]);
				if (targetEntity == null)
				{
					continue;
				}
				foreach (Card card in m_choiceState.m_cards)
				{
					if (card.GetEntity() == targetEntity)
					{
						m_correctCard = card;
						m_choseCorrectly = m_choiceState.m_chosenEntities.Contains(card.GetEntity());
						return true;
					}
				}
			}
		}
		return false;
	}
}
