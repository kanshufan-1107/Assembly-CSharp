using Blizzard.T5.Core.Utils;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[Tooltip("Animate a card object out of the deck like joust")]
[ActionCategory("Pegasus")]
public class JoustRevealAction : FsmStateAction
{
	[Tooltip("Time it takes to reveal animation to finish")]
	[RequiredField]
	public float m_AnimateTime = 1.2f;

	[Tooltip("Dummy card object to move")]
	[RequiredField]
	public FsmGameObject m_DummyCardObject;

	[RequiredField]
	[Tooltip("Destination location")]
	public FsmVector3 m_DestinationLocation;

	[RequiredField]
	[Tooltip("In-deck position offset")]
	public FsmVector3 m_DeckLocationOffset = new Vector3(0.15f, 0f, 0f);

	[RequiredField]
	[Tooltip("In-deck rotation offset")]
	public FsmVector3 m_DeckRotationOffset = new Vector3(0f, 0f, 180f);

	[Tooltip("Initial delay")]
	public float m_Delay;

	[Tooltip("If this is true, draw from the opponent's deck instead")]
	public bool m_OppositePlayer;

	public override void OnEnter()
	{
		GameObject dummyCard = m_DummyCardObject.Value;
		if (dummyCard == null)
		{
			Finish();
			return;
		}
		Vector3 deckLocalOffset = new Vector3(4.488504f, -6.613904f, 0.9635792f);
		Player spellController = GameObjectUtils.FindComponentInThisOrParents<Spell>(base.Owner).GetSourceCard().GetController();
		if (spellController == null)
		{
			Finish();
			return;
		}
		if (m_OppositePlayer)
		{
			spellController = ((!spellController.IsFriendlySide()) ? GameState.Get().GetFriendlySidePlayer() : GameState.Get().GetOpposingSidePlayer());
		}
		Actor deckActor = spellController.GetDeckZone().GetThicknessForLayout();
		if (deckActor == null)
		{
			Finish();
			return;
		}
		Renderer deckRenderer = deckActor.GetMeshRenderer();
		if (deckRenderer == null)
		{
			Finish();
			return;
		}
		deckLocalOffset = deckRenderer.bounds.center + Card.IN_DECK_OFFSET + m_DeckLocationOffset.Value - dummyCard.transform.position;
		float halfShowSec = 0.5f * m_AnimateTime;
		Vector3 initialPos = dummyCard.transform.position + deckLocalOffset;
		Vector3 intermedPos = initialPos + Card.ABOVE_DECK_OFFSET;
		Vector3 finalPos = m_DestinationLocation.Value;
		Vector3 finalAngles = dummyCard.transform.rotation.eulerAngles;
		Vector3 finalScale = dummyCard.transform.localScale;
		Vector3[] movePath = new Vector3[3] { initialPos, intermedPos, finalPos };
		dummyCard.transform.position = initialPos;
		dummyCard.transform.rotation = Card.IN_DECK_HIDDEN_ROTATION * Quaternion.Euler(m_DeckRotationOffset.Value);
		dummyCard.transform.localScale = Card.IN_DECK_SCALE;
		iTween.MoveTo(dummyCard, iTween.Hash("path", movePath, "delay", m_Delay, "time", m_AnimateTime, "easetype", iTween.EaseType.easeInOutQuart));
		iTween.RotateTo(dummyCard, iTween.Hash("rotation", finalAngles, "delay", m_Delay + halfShowSec, "time", halfShowSec, "easetype", iTween.EaseType.easeInOutCubic));
		iTween.ScaleTo(dummyCard, iTween.Hash("scale", finalScale, "delay", m_Delay + halfShowSec, "time", halfShowSec, "easetype", iTween.EaseType.easeInOutQuint));
		Finish();
	}
}
