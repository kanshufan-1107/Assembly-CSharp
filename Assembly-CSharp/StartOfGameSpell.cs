using UnityEngine;

public class StartOfGameSpell : SuperSpell
{
	public GameObject m_InitialVO;

	public GameObject m_ResponseVO;

	public override bool AddPowerTargets()
	{
		Card sourceCard = GetSourceCard();
		EntityDef sourceEntityDef = sourceCard.GetEntity().GetEntityDef();
		Player controller = sourceCard.GetController();
		if (controller.HasSeenStartOfGameSpell(sourceEntityDef))
		{
			return false;
		}
		bool num = base.AddPowerTargets();
		if (num)
		{
			controller.MarkStartOfGameSpellAsSeen(sourceEntityDef);
		}
		return num;
	}

	protected override void OnAction(SpellStateType prevStateType)
	{
		Card sourceCard = GetSourceCard();
		EntityDef entityDef = sourceCard.GetEntity().GetEntityDef();
		TAG_PREMIUM premium = sourceCard.GetPremium();
		GameObject fakeCardObj = AssetLoader.Get().InstantiatePrefab(ActorNames.GetHandActor(entityDef, premium), AssetLoadingOptions.IgnorePrefabPosition);
		Actor component = fakeCardObj.GetComponent<Actor>();
		component.SetCardDefFromCard(sourceCard);
		component.SetEntityDef(entityDef);
		component.SetPremium(premium);
		component.UpdateAllComponents();
		fakeCardObj.SetActive(value: false);
		PlayMakerFSM fsm = GetComponent<PlayMakerFSM>();
		fsm.FsmVariables.GetFsmGameObject("CardGO").Value = fakeCardObj;
		bool hasOpponentSeenSpell = GameState.Get().GetFirstOpponentPlayer(sourceCard.GetController()).HasSeenStartOfGameSpell(entityDef);
		if (!hasOpponentSeenSpell && m_InitialVO != null)
		{
			fsm.FsmVariables.GetFsmGameObject("VOLineGO").Value = m_InitialVO;
		}
		else if (hasOpponentSeenSpell && m_ResponseVO != null)
		{
			fsm.FsmVariables.GetFsmGameObject("VOLineGO").Value = m_ResponseVO;
		}
		base.OnAction(prevStateType);
	}
}
