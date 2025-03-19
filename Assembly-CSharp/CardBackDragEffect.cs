using Blizzard.T5.Services;
using UnityEngine;

public class CardBackDragEffect : MonoBehaviour
{
	private const float MIN_VELOCITY = 2f;

	private const float MAX_VELOCITY = 30f;

	public Actor m_Actor;

	public GameObject m_EffectsRoot;

	private CardBackManager m_CardBackManager;

	private Vector3 m_LastPosition;

	private float m_Speed;

	private bool m_Active;

	private float m_Min = 2f;

	private float m_Max = 30f;

	private void Start()
	{
		if (!ServiceManager.TryGet<SceneMgr>(out var sceneMgr) || sceneMgr.GetMode() != SceneMgr.Mode.GAMEPLAY)
		{
			base.enabled = false;
			return;
		}
		m_LastPosition = base.transform.position;
		if (m_CardBackManager == null)
		{
			m_CardBackManager = CardBackManager.Get();
			if (m_CardBackManager == null)
			{
				Debug.LogError("Failed to get CardBackManager!");
				base.enabled = false;
			}
		}
		if (m_CardBackManager != null)
		{
			m_CardBackManager.RegisterUpdateCardbacksListener(SetEffect);
		}
		SetEffect();
	}

	private void Update()
	{
		if (!(m_EffectsRoot != null))
		{
			return;
		}
		if (!GetComponent<Renderer>().enabled)
		{
			ShowParticles(show: false);
			if (m_EffectsRoot.activeSelf)
			{
				m_EffectsRoot.SetActive(value: false);
			}
		}
		else
		{
			m_Speed = (base.transform.position - m_LastPosition).magnitude / Time.deltaTime * 3.6f;
			UpdateDragEffect();
			m_LastPosition = base.transform.position;
		}
	}

	private void OnDisable()
	{
		if (m_EffectsRoot != null)
		{
			ShowParticles(show: false);
		}
	}

	private void OnDestroy()
	{
		if (CardBackManager.Get() != null)
		{
			CardBackManager.Get().UnregisterUpdateCardbacksListener(SetEffect);
		}
	}

	public void SetEffect()
	{
		if (m_CardBackManager == null)
		{
			m_CardBackManager = CardBackManager.Get();
			if (m_CardBackManager == null)
			{
				Debug.LogError("Failed to get CardBackManager!");
				base.enabled = false;
				return;
			}
		}
		CardBackManager.CardBackSlot slot = (m_CardBackManager.IsActorFriendly(m_Actor) ? CardBackManager.CardBackSlot.FRIENDLY : CardBackManager.CardBackSlot.OPPONENT);
		m_CardBackManager.UpdateDragEffect(base.gameObject, slot);
		CardBack cardback = m_CardBackManager.GetCardBackForActor(m_Actor);
		if (cardback != null)
		{
			m_Min = cardback.m_EffectMinVelocity;
			m_Max = cardback.m_EffectMaxVelocity;
		}
	}

	private void UpdateDragEffect()
	{
		if (m_Speed > m_Min && m_Speed < m_Max)
		{
			if (m_Active)
			{
				return;
			}
			m_Active = true;
		}
		else
		{
			if (!m_Active)
			{
				return;
			}
			m_Active = false;
		}
		ShowParticles(m_Active);
	}

	private void ShowParticles(bool show)
	{
		ParticleSystem[] componentsInChildren = GetComponentsInChildren<ParticleSystem>();
		foreach (ParticleSystem particle in componentsInChildren)
		{
			if (!(particle == null))
			{
				if (show)
				{
					particle.Play();
				}
				else
				{
					particle.Stop();
				}
			}
		}
	}
}
