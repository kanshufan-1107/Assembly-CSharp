using UnityEngine;

public class HeroHandSummonOutSpell : Spell
{
	private const string FRIENDLY_BONE_NAME = "FriendlyHeroSummonOut";

	private const string OPPONENT_BONE_NAME = "OpponentHeroSummonOut";

	public float m_MoveTime;

	protected override void OnBirth(SpellStateType prevStateType)
	{
		base.OnBirth(prevStateType);
		MoveToTarget();
	}

	private void MoveToTarget()
	{
		Card card = GetSourceCard();
		string boneName = ((card.GetControllerSide() == Player.Side.FRIENDLY) ? "FriendlyHeroSummonOut" : "OpponentHeroSummonOut");
		Transform transform = Board.Get().FindBone(boneName);
		if (transform == null)
		{
			Debug.LogErrorFormat("Failed to find a target bone: {0}, card: {1}", boneName, card);
		}
		else
		{
			card.SetDoNotSort(on: true);
			iTween.MoveTo(card.gameObject, transform.position, m_MoveTime);
			iTween.RotateTo(card.gameObject, transform.localEulerAngles, m_MoveTime);
			iTween.ScaleTo(card.gameObject, transform.localScale, m_MoveTime);
		}
	}

	public override void OnSpellFinished()
	{
		GetSourceCard().SetDoNotSort(on: false);
		base.OnSpellFinished();
	}
}
