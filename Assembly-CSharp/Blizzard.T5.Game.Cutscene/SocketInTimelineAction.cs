using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Blizzard.T5.Game.Cutscene;

public class SocketInTimelineAction : SpellTimelineAction
{
	protected readonly Player.Side m_side;

	private readonly List<(Actor Actor, bool WasVisible)> m_hideableActors = new List<(Actor, bool)>();

	private Spell m_activeEmoteSpell;

	protected CornerReplacementSpellType m_friendlyCornerSpell;

	public SocketInTimelineAction(Player.Side side, Actor source, CutsceneSceneDef.CutsceneActionRequest request, List<Actor> hideableActors = null)
		: base(SpellType.NONE, source, null)
	{
		m_side = side;
		SetCornerSpells(request);
		if (hideableActors == null)
		{
			return;
		}
		foreach (Actor actor in hideableActors)
		{
			if (!(actor == null))
			{
				m_hideableActors.Add((actor, false));
			}
		}
	}

	private void SetCornerSpells(CutsceneSceneDef.CutsceneActionRequest request)
	{
		if (request.SourceCard != null)
		{
			m_friendlyCornerSpell = request.FriendlyCornerSpell;
		}
	}

	public override void Init()
	{
		if (!(m_actionSource == null))
		{
			m_isReady = TryLoadMyHeroSkinSocketInEffect(m_actionSource, m_side, ref m_spell);
		}
	}

	public override IEnumerator Play()
	{
		if (!m_isReady)
		{
			Log.CosmeticPreview.PrintWarning("SocketInTimelineAction aborted playing as it wasn't ready...");
			yield break;
		}
		m_hasFinishedPlaying = false;
		UpdateCutsceneBoard(m_actionSource);
		UpdateCornerSpells();
		HideActors();
		TriggerSocketInEffect();
		yield return new WaitUntil(() => m_hasFinishedPlaying);
		m_activeEmoteSpell = TriggerStartEmote();
		if (m_activeEmoteSpell != null)
		{
			yield return new WaitUntil(() => m_activeEmoteSpell == null || m_activeEmoteSpell.IsFinished());
		}
	}

	public override void Stop()
	{
		base.Stop();
		if (m_activeEmoteSpell != null)
		{
			m_activeEmoteSpell.Deactivate();
		}
		if (m_hideableActors == null)
		{
			return;
		}
		for (int i = 0; i < m_hideableActors.Count; i++)
		{
			(Actor, bool) hideable = m_hideableActors[i];
			if (!(hideable.Item1 == null))
			{
				if (hideable.Item2)
				{
					hideable.Item1.SetVisibility(isVisible: true, isInternal: false);
				}
				hideable.Item2 = false;
				m_hideableActors[i] = hideable;
			}
		}
	}

	public override void Dispose()
	{
		if (m_activeEmoteSpell != null)
		{
			SpellManager.Get()?.ReleaseSpell(m_activeEmoteSpell);
			m_activeEmoteSpell = null;
		}
		base.Dispose();
	}

	private void TriggerSocketInEffect()
	{
		if (!(m_spell == null))
		{
			m_spell.Location = SpellLocation.NONE;
			m_spell.GameObject.SetActive(value: true);
			if (m_actionSource.SocketInParentEffectToHero)
			{
				Vector3 myActorScale = m_actionSource.transform.localScale;
				m_actionSource.transform.localScale = Vector3.one;
				m_spell.GameObject.transform.parent = m_actionSource.transform;
				m_spell.GameObject.transform.localPosition = Vector3.zero;
				m_actionSource.transform.localScale = myActorScale;
			}
			m_spell.SetSource(m_actionSource.GetCard().gameObject);
			m_spell.RemoveAllTargets();
			GameObject myHeroSocketBone = m_actionSource.gameObject;
			m_spell.AddTarget(myHeroSocketBone);
			m_spell.ActivateState(SpellStateType.BIRTH);
			m_spell.AddStateFinishedCallback(delegate
			{
				m_actionSource.transform.position = myHeroSocketBone.transform.position;
				m_actionSource.transform.localScale = Vector3.one;
				m_hasFinishedPlaying = true;
			});
			if (!m_actionSource.SocketInOverrideHeroAnimation)
			{
				m_actionSource.gameObject.GetComponent<Animation>().Play("hisHeroAnimateToPosition");
			}
		}
	}

	private CardSoundSpell TriggerStartEmote()
	{
		EmoteType emoteType = ((m_side == Player.Side.FRIENDLY) ? EmoteType.START : EmoteType.MIRROR_START);
		Card card = m_actionSource.GetCard();
		if (card == null)
		{
			Log.CosmeticPreview.PrintWarning("SocketInTimelineAction failed to trigger start emote as Actor had no Card!");
			return null;
		}
		return card.PlayEmote(emoteType, Notification.SpeechBubbleDirection.BottomRight);
	}

	protected void HideActors()
	{
		for (int i = 0; i < m_hideableActors.Count; i++)
		{
			(Actor, bool) hideable = m_hideableActors[i];
			if (!(hideable.Item1 == null))
			{
				hideable.Item2 = hideable.Item1.IsShown();
				hideable.Item1.SetVisibility(isVisible: false, isInternal: false);
				m_hideableActors[i] = hideable;
			}
		}
	}

	private bool TryLoadMyHeroSkinSocketInEffect(Actor heroActor, Player.Side side, ref Spell spell)
	{
		if (m_spell != null)
		{
			SpellManager.Get()?.ReleaseSpell(m_spell);
			Object.Destroy(m_spell.GameObject);
			m_spell = null;
		}
		Entity actorEntity = heroActor.GetEntity();
		if (actorEntity == null || !actorEntity.IsHero())
		{
			Log.CosmeticPreview.PrintWarning("Failed to initialize SocketInTimelineAction as source actor isn't a hero card...");
			return false;
		}
		string socketEffectPath;
		switch (side)
		{
		case Player.Side.FRIENDLY:
			socketEffectPath = ((!UniversalInputManager.UsePhoneUI) ? heroActor.SocketInEffectFriendly : heroActor.SocketInEffectFriendlyPhone);
			break;
		case Player.Side.OPPOSING:
			socketEffectPath = ((!UniversalInputManager.UsePhoneUI) ? heroActor.SocketInEffectOpponent : heroActor.SocketInEffectOpponentPhone);
			break;
		default:
			return false;
		}
		GameObject socketInEffectGo = AssetLoader.Get().InstantiatePrefab(socketEffectPath);
		if (socketInEffectGo == null)
		{
			Log.CosmeticPreview.PrintError("Failed to load My custom hero socket in effect!");
			return false;
		}
		socketInEffectGo.transform.position = heroActor.transform.position;
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

	protected void UpdateCornerSpells()
	{
		CutsceneManager manager = CutsceneManager.Get();
		if (manager != null)
		{
			manager.UpdateCornerReplacement(m_friendlyCornerSpell, CornerReplacementSpellType.NONE);
		}
	}

	protected void UpdateCutsceneBoard(Actor actor)
	{
		CutsceneBoard board = CutsceneBoard.Get();
		if (board != null)
		{
			board.UpdateCustomHeroTray(Player.Side.FRIENDLY, actor);
		}
	}
}
