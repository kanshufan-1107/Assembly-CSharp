using System.Collections;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.Services;
using UnityEngine;

public class GhostCardEffect : Spell
{
	public GameObject m_Glow;

	public GameObject m_GlowUnique;

	protected override void OnBirth(SpellStateType prevStateType)
	{
		if (m_Glow != null)
		{
			m_Glow.GetComponent<Renderer>().enabled = false;
		}
		if (m_GlowUnique != null)
		{
			m_GlowUnique.GetComponent<Renderer>().enabled = false;
		}
		StartCoroutine(GhostEffect(prevStateType));
	}

	protected override void OnDeath(SpellStateType prevStateType)
	{
		if (m_Glow != null)
		{
			m_Glow.GetComponent<Renderer>().enabled = false;
		}
		if (m_GlowUnique != null)
		{
			m_GlowUnique.GetComponent<Renderer>().enabled = false;
		}
		base.OnDeath(prevStateType);
		OnSpellFinished();
	}

	private IEnumerator GhostEffect(SpellStateType prevStateType)
	{
		Actor actor = GameObjectUtils.FindComponentInParents<Actor>(base.gameObject);
		if (actor == null)
		{
			Debug.LogWarning("GhostCardEffect actor is null");
			yield break;
		}
		GhostCard ghostCard = base.gameObject.GetComponentInChildren<GhostCard>();
		if (ghostCard == null)
		{
			Debug.LogWarning("GhostCardEffect GhostCard is null");
			yield break;
		}
		if (m_Glow != null)
		{
			GameObject glow = m_Glow;
			if (actor.IsElite() && m_GlowUnique != null)
			{
				glow = m_GlowUnique;
			}
			glow.GetComponent<Renderer>().enabled = true;
		}
		TooltipPanelManager.Get().HideKeywordHelp();
		ghostCard.RenderGhostCard();
		yield return new WaitForEndOfFrame();
		RenderToTexture r2t = base.gameObject.GetComponentInChildren<RenderToTexture>();
		if ((bool)r2t)
		{
			if (ServiceManager.Get<IGraphicsManager>().RenderQualityLevel == GraphicsQuality.High && actor.GetPremium() == TAG_PREMIUM.GOLDEN)
			{
				r2t.m_RealtimeRender = true;
			}
			else
			{
				r2t.m_RealtimeRender = false;
			}
			r2t.m_LateUpdate = true;
		}
		ghostCard.RenderGhostCard(forceRender: true);
		actor.Show();
		TooltipPanelManager.Get().HideKeywordHelp();
		r2t.Render();
		base.OnBirth(prevStateType);
		OnSpellFinished();
	}
}
