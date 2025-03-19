using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[ActionCategory("Pegasus")]
[Tooltip("INTERNAL USE ONLY. Do not put this on your FSMs.")]
public abstract class SpellCardIdAudioAction : SpellAction
{
	protected AudioSource GetAudioSource(FsmOwnerDefault ownerDefault)
	{
		GameObject go = base.Fsm.GetOwnerDefaultTarget(ownerDefault);
		if (go == null)
		{
			return null;
		}
		return go.GetComponent<AudioSource>();
	}

	protected SoundDef GetClipMatchingCardId(Which whichCard, string[] cardIds, SoundDef[] clips, SoundDef defaultClip)
	{
		Card card = GetCard(whichCard);
		if (card == null)
		{
			Debug.LogWarningFormat("SpellCardIdAudioAction.GetClipMatchingCardId() - could not find {0} card", whichCard);
			return null;
		}
		string cardId = card.GetEntity().GetCardId();
		int index = GetIndexMatchingCardId(cardId, cardIds);
		if (index < 0)
		{
			return defaultClip;
		}
		return clips[index];
	}

	protected AudioSource GetSourceMatchingCardId(Which whichCard, string[] cardIds, FsmGameObject[] sources, FsmGameObject defaultSource)
	{
		Card card = GetCard(whichCard);
		if (card == null)
		{
			Debug.LogWarningFormat("SpellCardIdAudioAction.GetSourceMatchingCardId() - could not find {0} card", whichCard);
			return null;
		}
		string cardId = card.GetEntity().GetCardId();
		int index = GetIndexMatchingCardId(cardId, cardIds);
		if (index < 0)
		{
			return defaultSource.Value.GetComponent<AudioSource>();
		}
		return sources[index].Value.GetComponent<AudioSource>();
	}
}
