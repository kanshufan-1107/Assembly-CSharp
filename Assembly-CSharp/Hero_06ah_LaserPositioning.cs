using UnityEngine;

public class Hero_06ah_LaserPositioning : MonoBehaviour
{
	public Spell m_spell;

	public GameObject m_laser;

	public GameObject m_target;

	public float m_laserDistanceMod = 0.1f;

	public Vector3 m_laserSourceOffset;

	public Vector3 m_laserDestOffset;

	public float m_laserAttackSourceTweak;

	public string m_eyeObjectName;

	public string m_attackSourceObjectName;

	private GameObject m_eye;

	private GameObject m_attackSource;

	private Actor m_actor;

	private void OnEnable()
	{
		if (m_spell == null || m_target == null)
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		Card sourceHero = m_spell.GetSourceCard().GetHeroCard();
		if (sourceHero == null)
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		m_actor = sourceHero.GetActor();
		if (m_actor == null)
		{
			base.gameObject.SetActive(value: false);
		}
		else
		{
			if (!(m_eye == null) && !(m_attackSource == null))
			{
				return;
			}
			ILegendaryHeroPortrait heroPortrait = m_actor.LegendaryHeroPortrait;
			m_eye = heroPortrait?.FindGameObjectInLegendaryPortraitPrefab(m_eyeObjectName);
			if (m_eye == null)
			{
				Debug.LogError($"Invalid object name specified for eye object {m_eye}");
				base.gameObject.SetActive(value: false);
				return;
			}
			m_attackSource = heroPortrait.FindGameObjectInLegendaryPortraitPrefab(m_attackSourceObjectName);
			if (m_attackSource == null)
			{
				Debug.LogError($"Invalid object name specified for attack source object {m_eye}");
				base.gameObject.SetActive(value: false);
			}
		}
	}

	private void Update()
	{
		CalcuateSourceAndDestOffsets();
	}

	private void CalcuateSourceAndDestOffsets()
	{
		Vector3 toSource = m_attackSource.transform.position + m_attackSource.transform.right * m_laserAttackSourceTweak - m_eye.transform.position;
		toSource.y = 0f;
		Vector3 laserStartPos = m_actor.transform.position + toSource * m_laserDistanceMod + m_laserSourceOffset;
		m_laser.gameObject.transform.position = laserStartPos;
		GameObject target = m_spell.GetTarget();
		if (target != null)
		{
			m_target.transform.position = target.transform.position + m_laserDestOffset;
		}
	}
}
