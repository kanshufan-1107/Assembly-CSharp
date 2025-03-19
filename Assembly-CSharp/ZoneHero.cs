using UnityEngine;

public class ZoneHero : Zone
{
	private Vector3? m_originalPosition;

	public Vector3 OriginalPosition
	{
		get
		{
			if (!m_originalPosition.HasValue)
			{
				m_originalPosition = base.transform.localPosition;
			}
			if (base.transform.parent != null)
			{
				return base.transform.parent.TransformPoint(m_originalPosition.Value);
			}
			return m_originalPosition.Value;
		}
	}

	public override string ToString()
	{
		return $"{base.ToString()} (Hero)";
	}

	public override bool CanAcceptTags(int controllerId, TAG_ZONE zoneTag, TAG_CARDTYPE cardType, Entity entity)
	{
		if (!base.CanAcceptTags(controllerId, zoneTag, cardType, entity))
		{
			return false;
		}
		if (cardType != TAG_CARDTYPE.HERO)
		{
			return false;
		}
		return true;
	}

	public override void OnHealingDoesDamageEntityEnteredPlay()
	{
	}

	public override void OnHealingDoesDamageEntityMousedOut()
	{
	}

	public override void OnHealingDoesDamageEntityMousedOver()
	{
	}

	public override void OnLifestealDoesDamageEntityEnteredPlay()
	{
	}

	public override void OnLifestealDoesDamageEntityMousedOut()
	{
	}

	public override void OnLifestealDoesDamageEntityMousedOver()
	{
	}

	public override void UpdateLayout()
	{
		if (!m_originalPosition.HasValue)
		{
			m_originalPosition = base.transform.localPosition;
		}
		Actor actor = GetFirstCard()?.GetActor();
		if ((bool)actor)
		{
			base.transform.localPosition = m_originalPosition.Value + new Vector3(0f, actor.ZoneHeroPositionOffset, 0f);
		}
		else
		{
			base.transform.localPosition = m_originalPosition.Value;
		}
		base.UpdateLayout();
	}
}
