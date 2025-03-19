using Blizzard.T5.Core.Utils;
using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

public class Crush : Spell
{
	public MinionPieces m_minionPieces;

	public Material m_premiumTauntMaterial;

	public Material m_premiumEliteMaterial;

	public UberText m_attack;

	public UberText m_health;

	protected override void OnAction(SpellStateType prevStateType)
	{
		base.OnAction(prevStateType);
		Entity origCardEntity = GetSourceCard().GetEntity();
		Actor actor = GameObjectUtils.FindComponentInParents<Actor>(this);
		GameObject minionObj = m_minionPieces.m_main;
		bool isPremium = origCardEntity.HasTag(GAME_TAG.PREMIUM);
		if (isPremium)
		{
			minionObj = m_minionPieces.m_premium;
			RenderUtils.EnableRenderers(m_minionPieces.m_main, enable: false);
		}
		GameObject portraitMesh = actor.GetPortraitMesh();
		minionObj.GetComponent<Renderer>().SetMaterial(portraitMesh.GetComponent<Renderer>().GetSharedMaterial());
		minionObj.SetActive(value: true);
		RenderUtils.EnableRenderers(minionObj, enable: true);
		if (origCardEntity.HasTaunt())
		{
			if (isPremium)
			{
				m_minionPieces.m_taunt.GetComponent<Renderer>().SetMaterial(m_premiumTauntMaterial);
			}
			m_minionPieces.m_taunt.SetActive(value: true);
			RenderUtils.EnableRenderers(m_minionPieces.m_taunt, enable: true);
		}
		if (origCardEntity.IsElite())
		{
			if (isPremium)
			{
				m_minionPieces.m_legendary.GetComponent<Renderer>().SetMaterial(m_premiumEliteMaterial);
			}
			m_minionPieces.m_legendary.SetActive(value: true);
			RenderUtils.EnableRenderers(m_minionPieces.m_legendary, enable: true);
		}
		m_attack.SetText(GameStrings.Get(origCardEntity.GetATK().ToString()));
		m_health.SetText(GameStrings.Get(origCardEntity.GetHealth().ToString()));
	}
}
