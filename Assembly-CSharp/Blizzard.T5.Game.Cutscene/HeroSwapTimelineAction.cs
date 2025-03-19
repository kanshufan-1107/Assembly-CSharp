using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Blizzard.T5.Game.Cutscene;

public class HeroSwapTimelineAction : SocketInTimelineAction
{
	private string m_spellFxPath;

	private string m_customSoundSpellPath;

	private Spell m_customSoundSpell;

	public HeroSwapTimelineAction(Player.Side side, Actor source, Actor target, CutsceneSceneDef.CutsceneActionRequest request, List<Actor> hideableActors = null)
		: base(side, source, request, hideableActors)
	{
		m_actionSource = source;
		m_actionTarget = target;
		m_spellFxPath = request.SourceCard.CustomSocketInSpell;
		m_customSoundSpellPath = request.SourceCard.CustomSoundSpell;
	}

	public override void Init()
	{
		if (!(m_actionTarget == null) && !string.IsNullOrEmpty(m_spellFxPath))
		{
			m_isReady = TryLoadMyHeroSkinSocketInEffect(m_actionTarget, m_spellFxPath, out m_spell);
			TryLoadCustomSoundSpell(m_actionTarget, m_customSoundSpellPath, out m_customSoundSpell);
		}
	}

	public override IEnumerator Play()
	{
		if (!m_isReady)
		{
			Log.CosmeticPreview.PrintWarning("HeroSwapTimelineAction aborted playing as it wasn't ready...");
			yield break;
		}
		m_hasFinishedPlaying = false;
		m_spell.GameObject.SetActive(value: true);
		UpdateCutsceneBoard(m_actionTarget);
		UpdateCornerSpells();
		HideActors();
		m_actionSource.SetVisibility(isVisible: false, isInternal: false);
		m_actionTarget.SetVisibility(isVisible: true, isInternal: false);
		m_spell.Activate();
		TriggerSound();
		m_spell.AddStateFinishedCallback(delegate
		{
			m_hasFinishedPlaying = true;
		});
		yield return new WaitUntil(() => m_hasFinishedPlaying);
	}

	public override void Stop()
	{
		base.Stop();
		if (m_customSoundSpell != null)
		{
			m_customSoundSpell.Deactivate();
		}
	}

	public override void Dispose()
	{
		if (m_customSoundSpell != null)
		{
			SpellManager.Get()?.ReleaseSpell(m_customSoundSpell);
			m_customSoundSpell = null;
		}
		base.Dispose();
	}

	private void TriggerSound()
	{
		if (m_customSoundSpell != null)
		{
			m_customSoundSpell.Activate();
		}
	}

	private static bool TryLoadMyHeroSkinSocketInEffect(Actor parentActor, string spellFxPath, out Spell spell)
	{
		GameObject socketInEffectGo = AssetLoader.Get().InstantiatePrefab(spellFxPath);
		socketInEffectGo.transform.parent = parentActor.transform;
		socketInEffectGo.transform.position = parentActor.transform.position;
		spell = socketInEffectGo.GetComponent<Spell>();
		if (spell == null)
		{
			Log.CosmeticPreview.PrintError("Failed to locate Spell on custom socket in effect!");
			return false;
		}
		if (spell.HasUsableState(SpellStateType.IDLE))
		{
			spell.ActivateState(SpellStateType.IDLE);
		}
		else
		{
			spell.GameObject.SetActive(value: false);
		}
		return true;
	}

	private static void TryLoadCustomSoundSpell(Actor parentActor, string customSpellSound, out Spell spell)
	{
		spell = null;
		if (!string.IsNullOrEmpty(customSpellSound))
		{
			spell = SpellUtils.LoadAndSetupSpell(customSpellSound, parentActor);
		}
	}
}
