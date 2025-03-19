using Blizzard.T5.Core.Utils;
using UnityEngine;
using UnityEngine.Playables;

namespace HutongGames.PlayMaker.Actions;

[Tooltip("Sets the Parent of a Game Object.")]
[ActionCategory("Pegasus")]
public class SocketInReparentToSpell : FsmStateAction
{
	[Tooltip("Spell object")]
	[RequiredField]
	public FsmOwnerDefault m_spellObject;

	[Tooltip("Actor Object Output")]
	[RequiredField]
	public FsmGameObject m_actorObjectOutput;

	[Tooltip("Root Object Output")]
	[RequiredField]
	public FsmGameObject m_rootObjectOutput;

	[RequiredField]
	[Tooltip("The Game Object to parent.")]
	public GameObject m_parentObject;

	[Tooltip("The Game Object to parent.")]
	public bool m_playTimeline;

	[Tooltip("Apply default wait")]
	public bool m_wait;

	private float m_appliedWaitTime;

	private float m_timer;

	private const float c_friendlyWaitTime = 1.8f;

	private const float c_opponentWaitTime = 0.5f;

	public override void Reset()
	{
		m_spellObject = null;
		m_parentObject = null;
		m_playTimeline = false;
		m_wait = false;
	}

	private Actor GetActor(GameObject spellObject)
	{
		Actor actor = GameObjectUtils.FindComponentInThisOrParents<Actor>(spellObject);
		if (actor == null)
		{
			Card card = GameObjectUtils.FindComponentInThisOrParents<Card>(spellObject);
			if (card != null)
			{
				actor = card.GetActor();
			}
		}
		return actor;
	}

	public override void OnEnter()
	{
		base.OnEnter();
		m_appliedWaitTime = 0f;
		m_timer = 0f;
		GameObject spellObject = base.Fsm.GetOwnerDefaultTarget(m_spellObject);
		if (!spellObject)
		{
			Finish();
			return;
		}
		Actor actor = GetActor(spellObject);
		if (!actor)
		{
			Finish();
			return;
		}
		GameObject rootObject = actor.GetRootObject();
		if (!rootObject)
		{
			Finish();
			return;
		}
		if (!m_parentObject)
		{
			Finish();
			return;
		}
		if (!m_actorObjectOutput.IsNone)
		{
			m_actorObjectOutput.Value = actor.gameObject;
		}
		if (!m_rootObjectOutput.IsNone)
		{
			m_rootObjectOutput.Value = rootObject;
		}
		LegendaryHeroAnimEventHandler eventHandler = spellObject.GetComponentInChildren<LegendaryHeroAnimEventHandler>();
		if (eventHandler != null)
		{
			eventHandler.SetActor(actor);
		}
		Transform transform = spellObject.transform;
		Transform actorTransform = actor.transform;
		Transform rootTransform = rootObject.transform;
		Transform parentTransform = m_parentObject.transform;
		rootTransform.SetParent(null, worldPositionStays: true);
		Transform parentsParent = parentTransform.parent;
		parentTransform.SetParent(null);
		parentTransform.localPosition = rootTransform.localPosition;
		parentTransform.localRotation = rootTransform.localRotation;
		parentTransform.localScale = rootTransform.localScale;
		rootTransform.localPosition = Vector3.zero;
		rootTransform.localRotation = Quaternion.identity;
		rootTransform.localScale = Vector3.one;
		transform.localPosition = Vector3.zero;
		transform.localRotation = Quaternion.identity;
		transform.localScale = Vector3.one;
		actorTransform.localPosition = Vector3.zero;
		actorTransform.localRotation = Quaternion.identity;
		actorTransform.localScale = Vector3.one;
		parentTransform.SetParent(parentsParent, worldPositionStays: true);
		rootTransform.SetParent(parentTransform, worldPositionStays: false);
		if (m_playTimeline)
		{
			PlayableDirector timeline = spellObject.GetComponent<PlayableDirector>();
			if (timeline != null)
			{
				timeline.Play();
			}
		}
		if (m_wait)
		{
			Card card = actor.GetCard();
			switch ((card != null) ? card.GetControllerSide() : Player.Side.NEUTRAL)
			{
			case Player.Side.FRIENDLY:
				m_appliedWaitTime = 1.8f;
				break;
			case Player.Side.OPPOSING:
				m_appliedWaitTime = 0.5f;
				break;
			default:
				Finish();
				break;
			}
		}
		else
		{
			Finish();
		}
	}

	public override void OnUpdate()
	{
		m_timer += Time.deltaTime;
		if (m_timer >= m_appliedWaitTime)
		{
			Finish();
		}
	}
}
