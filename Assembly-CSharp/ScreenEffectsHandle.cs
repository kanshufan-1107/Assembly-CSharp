using System;

public class ScreenEffectsHandle
{
	public object Owner;

	private FullScreenFXMgr.ScreenEffectsInstance m_fxInstance;

	private bool m_isSet;

	public FullScreenFXMgr.ScreenEffectsInstance ScreenEffectsInstance => m_fxInstance;

	public ScreenEffectsHandle(object owner)
	{
		Owner = owner;
		m_fxInstance = new FullScreenFXMgr.ScreenEffectsInstance(owner);
		m_isSet = false;
	}

	~ScreenEffectsHandle()
	{
		StopEffect();
	}

	public void StartEffect(ScreenEffectParameters parameters, Action onFinishedCallback = null)
	{
		FullScreenFXMgr fxMgr = FullScreenFXMgr.Get();
		if (fxMgr == null)
		{
			Log.FullScreenFX.PrintError("FullscreenFXMgr is missing!");
			return;
		}
		fxMgr.AddEffect(this, parameters, onFinishedCallback);
		m_isSet = true;
	}

	public void StopEffect(Action callback = null)
	{
		if (!HasBeenResetOrReleased())
		{
			FullScreenFXMgr fxMgr = FullScreenFXMgr.Get();
			if (fxMgr == null)
			{
				Log.FullScreenFX.PrintError("FullscreenFXMgr is missing!");
				return;
			}
			m_fxInstance.OnFinishedCallback = callback;
			fxMgr.StopEffect(m_fxInstance, m_fxInstance == null);
			m_isSet = false;
		}
	}

	public void StopEffect(float time, iTween.EaseType easeType, Action callback = null)
	{
		if (!HasBeenResetOrReleased())
		{
			m_fxInstance.Parameters.Time = time;
			m_fxInstance.Parameters.EaseType = easeType;
			StopEffect(callback);
		}
	}

	public void StopEffect(float time, Action callback = null)
	{
		if (!HasBeenResetOrReleased())
		{
			StopEffect(time, m_fxInstance.Parameters.EaseType, callback);
		}
	}

	public void SetFinishedCallback(Action onFinishedCallback)
	{
		m_fxInstance.OnFinishedCallback = onFinishedCallback;
	}

	public void ClearCallbacks()
	{
		if (m_fxInstance != null)
		{
			m_fxInstance.OnFinishedCallback = null;
		}
	}

	private bool HasBeenResetOrReleased()
	{
		if (m_isSet)
		{
			return m_fxInstance.Released;
		}
		return true;
	}
}
