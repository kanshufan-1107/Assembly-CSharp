using Hearthstone.UI;
using UnityEngine;

public class CardTooltipElement : PegUIElement
{
	[SerializeField]
	private TooltipPanelManager.Orientation m_orientation;

	private Hearthstone.UI.Card m_cardRef;

	[ContextMenu("Assign Component References From Children")]
	public void AssignComponentReferencesFromChildren()
	{
		if (m_cardRef == null)
		{
			m_cardRef = GetComponentInChildren<Hearthstone.UI.Card>(includeInactive: true);
		}
	}

	protected override void OnOver(InteractionState oldState)
	{
		base.OnOver(oldState);
		AssignComponentReferencesFromChildren();
		if (m_cardRef == null)
		{
			return;
		}
		Actor actor = m_cardRef.CardActor;
		if (!(actor == null))
		{
			EntityDef entityDef = actor.GetEntityDef();
			if (entityDef != null)
			{
				TooltipPanelManager.Get().UpdateKeywordHelpForCollectionManager(entityDef, actor, m_orientation);
			}
		}
	}

	protected override void OnOut(InteractionState oldState)
	{
		base.OnOut(oldState);
		TooltipPanelManager.Get().HideKeywordHelp();
	}

	public override void SetEnabled(bool enabled, bool isInternal = false)
	{
		base.SetEnabled(enabled, isInternal);
		if (!enabled)
		{
			TooltipPanelManager.Get().HideKeywordHelp();
		}
	}
}
