using System.Collections;
using UnityEngine;

namespace Blizzard.T5.Game.Cutscene;

public class AttackTimelineAction : SpellTimelineAction
{
	private readonly CutsceneAttackSpellController m_attackSpellController;

	public AttackTimelineAction(bool isFriendly, CutsceneAttackSpellController attSpellController, Actor source, Actor target)
		: base(isFriendly ? SpellType.FRIENDLY_ATTACK : SpellType.OPPONENT_ATTACK, source, target)
	{
		m_attackSpellController = attSpellController;
	}

	public override void Init()
	{
		base.Init();
		m_isReady &= m_attackSpellController != null && m_actionTarget != null;
		if (m_spell != null)
		{
			m_spell.gameObject.SetActive(value: false);
		}
	}

	public override IEnumerator Play()
	{
		if (!m_isReady)
		{
			Log.CosmeticPreview.PrintWarning("AttackTimelineAction aborted playing as it wasn't ready...");
			yield break;
		}
		m_hasFinishedPlaying = false;
		m_spell.gameObject.SetActive(value: true);
		m_attackSpellController.AddFinishedCallback(delegate
		{
			m_hasFinishedPlaying = true;
		});
		m_attackSpellController.TriggerAttackSpell(m_spell, m_actionSource, m_actionTarget);
		yield return new WaitUntil(() => m_hasFinishedPlaying);
	}
}
