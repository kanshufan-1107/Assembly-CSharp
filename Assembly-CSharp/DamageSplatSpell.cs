using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

public class DamageSplatSpell : Spell
{
	[FormerlySerializedAs("m_BloodSplat")]
	public GameObject m_StandardSplat;

	public GameObject m_HealthSplat;

	public GameObject m_ArmorSplat;

	public GameObject m_CorpseSplat;

	public GameObject m_PoisonSplat;

	public GameObject m_HealSplat;

	public GameObject m_BloodCritSplat;

	public UberText m_DamageTextMesh;

	private GameObject m_activeSplat;

	private int m_damage;

	private bool m_poison;

	private bool m_damageIsCrit;

	private const float SCALE_IN_TIME = 1f;

	private const float DELAY_ASYNC_ANIM = 1.1f;

	private const float FADE_IN_TIME = 1f;

	private CancellationTokenSource m_animTokenSource;

	private TAG_CARD_ALTERNATE_COST m_alternateCostSplat;

	private GameObject[] m_splats;

	protected override void Awake()
	{
		base.Awake();
		m_splats = new GameObject[7] { m_StandardSplat, m_HealthSplat, m_ArmorSplat, m_CorpseSplat, m_PoisonSplat, m_HealSplat, m_BloodCritSplat };
		EnableAllRenderers(enabled: false);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (m_animTokenSource != null)
		{
			m_animTokenSource.Cancel();
			m_animTokenSource.Dispose();
		}
	}

	public float GetDamage()
	{
		return m_damage;
	}

	public void SetDamage(int damage)
	{
		m_damage = damage;
	}

	public void SetPoisonous(bool isPoisonous)
	{
		m_poison = isPoisonous;
		m_DamageTextMesh.gameObject.SetActive(!m_poison);
	}

	public bool IsPoisonous()
	{
		return m_poison;
	}

	public void SetDamageIsCrit(bool isCrit)
	{
		m_damageIsCrit = isCrit;
	}

	public bool IsDamageCritical()
	{
		return m_damageIsCrit;
	}

	public void SetAlternateCostSplat(TAG_CARD_ALTERNATE_COST splatType)
	{
		m_alternateCostSplat = splatType;
		if (m_fsm != null && m_fsm.FsmVariables.GetFsmInt("splat") != null)
		{
			m_fsm.FsmVariables.GetFsmInt("splat").Value = (int)splatType;
		}
	}

	public void DoSplatAnims()
	{
		StopAllAsyncs();
		iTween.Stop(base.gameObject);
		if (m_animTokenSource == null)
		{
			m_animTokenSource = new CancellationTokenSource();
		}
		else if (m_animTokenSource != null && m_animTokenSource.IsCancellationRequested)
		{
			m_animTokenSource.Dispose();
			m_animTokenSource = new CancellationTokenSource();
		}
		SplatAnimAsync(m_animTokenSource.Token).Forget();
	}

	private async UniTaskVoid SplatAnimAsync(CancellationToken token)
	{
		UpdateElements();
		base.transform.localScale = Vector3.zero;
		await UniTask.Yield(PlayerLoopTiming.Update, token);
		OnSpellFinished();
		SetDamageIsCrit(isCrit: false);
		iTween.ScaleTo(base.gameObject, iTween.Hash("scale", Vector3.one, "time", 1f, "easetype", iTween.EaseType.easeOutElastic));
		float hangTimeBeforeFade = 2f;
		if (IsPoisonous())
		{
			hangTimeBeforeFade = 0.8f;
		}
		await UniTask.Delay(TimeSpan.FromSeconds(hangTimeBeforeFade), ignoreTimeScale: false, PlayerLoopTiming.Update, token);
		iTween.FadeTo(base.gameObject, 0f, 1f);
		await UniTask.Delay(TimeSpan.FromSeconds(1.100000023841858), ignoreTimeScale: false, PlayerLoopTiming.Update, token);
		EnableAllRenderers(enabled: false);
		if (m_activeStateType != 0)
		{
			OnStateFinished();
		}
	}

	protected override void OnIdle(SpellStateType prevStateType)
	{
		StopAllAsyncs();
		UpdateElements();
		base.OnIdle(prevStateType);
	}

	protected override void OnAction(SpellStateType prevStateType)
	{
		UpdateElements();
		base.OnAction(prevStateType);
		DoSplatAnims();
	}

	protected override void OnNone(SpellStateType prevStateType)
	{
		base.OnAction(prevStateType);
		m_activeSplat = null;
	}

	protected override void ShowImpl()
	{
		base.ShowImpl();
		if (!(m_activeSplat == null))
		{
			RenderUtils.EnableRenderers(m_activeSplat.gameObject, enable: true);
			m_DamageTextMesh.gameObject.SetActive(!m_poison);
		}
	}

	protected override void HideImpl()
	{
		base.HideImpl();
		StopAllAsyncs();
		iTween.Stop(base.gameObject);
		EnableAllRenderers(enabled: false);
	}

	protected override void StopAllAsyncs()
	{
		base.StopAllAsyncs();
		if (m_animTokenSource != null && !m_animTokenSource.IsCancellationRequested)
		{
			m_animTokenSource.Cancel();
		}
	}

	private void UpdateElements()
	{
		iTween.Stop(base.gameObject);
		iTween.FadeTo(base.gameObject, 1f, 0f);
		for (int i = 0; i < m_splats.Length; i++)
		{
			GameObject splat = m_splats[i];
			if (splat != null)
			{
				RenderUtils.EnableRenderers(splat.gameObject, enable: false);
			}
		}
		string textFormat = "-{0}";
		int damageValue = m_damage;
		bool showText = true;
		if (m_damage < 0 && m_HealSplat != null)
		{
			m_activeSplat = m_HealSplat;
			textFormat = "+{0}";
			damageValue = Mathf.Abs(m_damage);
		}
		else if (m_poison && m_PoisonSplat != null)
		{
			m_activeSplat = m_PoisonSplat;
			showText = false;
		}
		else if (m_damageIsCrit && m_BloodCritSplat != null)
		{
			m_activeSplat = m_BloodCritSplat;
			textFormat = "-{0}!";
		}
		else if (m_HealthSplat != null && m_alternateCostSplat == TAG_CARD_ALTERNATE_COST.HEALTH)
		{
			m_activeSplat = m_HealthSplat;
		}
		else if (m_ArmorSplat != null && m_alternateCostSplat == TAG_CARD_ALTERNATE_COST.ARMOR)
		{
			m_activeSplat = m_ArmorSplat;
		}
		else if (m_CorpseSplat != null && m_alternateCostSplat == TAG_CARD_ALTERNATE_COST.CORPSES)
		{
			m_activeSplat = m_CorpseSplat;
		}
		else if (m_StandardSplat != null)
		{
			m_activeSplat = m_StandardSplat;
		}
		if (m_activeSplat != null)
		{
			RenderUtils.EnableRenderers(m_activeSplat.gameObject, enable: true);
		}
		if (m_DamageTextMesh != null)
		{
			m_DamageTextMesh.Text = string.Format(textFormat, damageValue);
			m_DamageTextMesh.gameObject.SetActive(showText);
		}
	}

	private void EnableAllRenderers(bool enabled)
	{
		for (int i = 0; i < m_splats.Length; i++)
		{
			GameObject splat = m_splats[i];
			if (splat != null)
			{
				RenderUtils.EnableRenderers(splat.gameObject, enabled);
			}
		}
		if (m_DamageTextMesh != null)
		{
			m_DamageTextMesh.gameObject.SetActive(enabled);
		}
	}
}
