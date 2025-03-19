using System;
using UnityEngine;

public class CustomVisualReward : MonoBehaviour
{
	private Action m_callback;

	private ScreenEffectsHandle m_screenEffectsHandle;

	private Achievement m_associatedAchievement;

	public Achievement AssociatedAchievement
	{
		get
		{
			return m_associatedAchievement;
		}
		set
		{
			m_associatedAchievement = value;
			OnNewAssociatedAchievement();
		}
	}

	public virtual void Start()
	{
		m_screenEffectsHandle = new ScreenEffectsHandle(this);
		m_screenEffectsHandle.StartEffect(ScreenEffectParameters.BlurVignetteDesaturatePerspective);
	}

	protected virtual void OnNewAssociatedAchievement()
	{
	}

	public void SetCompleteCallback(Action c)
	{
		m_callback = c;
	}

	public void Complete()
	{
		if (m_callback != null)
		{
			m_callback();
		}
		m_screenEffectsHandle?.StopEffect();
		UnityEngine.Object.Destroy(base.gameObject);
	}
}
