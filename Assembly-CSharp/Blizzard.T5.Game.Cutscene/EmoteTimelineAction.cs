using System.Collections;
using UnityEngine;

namespace Blizzard.T5.Game.Cutscene;

public class EmoteTimelineAction : TimelineAction
{
	private EmoteType m_emoteType;

	private Actor m_actor;

	private Spell m_activeEmoteSpell;

	public EmoteTimelineAction(EmoteType emote, Actor actor)
	{
		InternalInitialize(emote, actor);
	}

	private void InternalInitialize(EmoteType emote, Actor actor)
	{
		if (emote == EmoteType.INVALID)
		{
			Log.CosmeticPreview.PrintError($"Cutscene timeline error! Requested emote: {emote}, can not be played!");
		}
		else if (!(actor == null))
		{
			m_actor = actor;
			m_emoteType = emote;
		}
	}

	public override void Dispose()
	{
		if (m_activeEmoteSpell != null)
		{
			SpellManager.Get()?.ReleaseSpell(m_activeEmoteSpell);
			m_activeEmoteSpell = null;
		}
		if (m_actor != null)
		{
			m_actor = null;
		}
		m_isReady = false;
	}

	public override void Init()
	{
		if (!m_isReady)
		{
			m_isReady = m_actor != null;
		}
	}

	public override IEnumerator Play()
	{
		if (!m_isReady)
		{
			Log.CosmeticPreview.PrintError($"Cutscene timeline logic error! Play requested on {m_emoteType} when action is not ready!");
			yield break;
		}
		Card card = m_actor.GetCard();
		if (card == null)
		{
			Log.CosmeticPreview.PrintWarning(string.Format("{0} failed to play emote {1} as Actor had no Card!", "EmoteTimelineAction", m_emoteType));
			yield break;
		}
		m_activeEmoteSpell = card.PlayEmote(m_emoteType, Notification.SpeechBubbleDirection.BottomLeft);
		if (m_activeEmoteSpell != null)
		{
			yield return new WaitUntil(() => m_activeEmoteSpell == null || m_activeEmoteSpell.IsFinished());
		}
	}

	public override void Stop()
	{
		if (m_activeEmoteSpell != null)
		{
			m_activeEmoteSpell.Deactivate();
		}
	}

	public override void Reset()
	{
	}
}
