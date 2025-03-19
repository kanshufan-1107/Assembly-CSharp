using UnityEngine;

public class CardTileListDisplay : MonoBehaviour
{
	public SoundDucker m_SoundDucker;

	private ScreenEffectsHandle m_screenEffectsHandle;

	protected virtual void Awake()
	{
		m_screenEffectsHandle = new ScreenEffectsHandle(this);
	}

	protected virtual void Start()
	{
	}

	protected virtual void OnDestroy()
	{
	}

	protected void AnimateVignetteIn()
	{
		ScreenEffectParameters screenEffectParameters = ScreenEffectParameters.VignetteDesaturatePerspective;
		m_screenEffectsHandle.StartEffect(screenEffectParameters);
	}

	protected void AnimateVignetteOut()
	{
		m_screenEffectsHandle.StopEffect(0.1f, OnFullScreenEffectOutFinished);
	}

	protected void AnimateBlurVignetteIn()
	{
		ScreenEffectParameters screenEffectParameters = ScreenEffectParameters.BlurVignetteDesaturatePerspective;
		m_screenEffectsHandle.StartEffect(screenEffectParameters);
	}

	protected void AnimateBlurVignetteOut()
	{
		m_screenEffectsHandle.StopEffect(0.1f, OnFullScreenEffectOutFinished);
	}

	protected virtual void OnFullScreenEffectOutFinished()
	{
	}
}
