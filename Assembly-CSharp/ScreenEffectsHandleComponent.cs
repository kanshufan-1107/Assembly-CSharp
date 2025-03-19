using UnityEngine;

public class ScreenEffectsHandleComponent : MonoBehaviour
{
	public ScreenEffectsHandle Handle;

	private void Awake()
	{
		Handle = new ScreenEffectsHandle(this);
	}

	public void StartEffect(ScreenEffectParameters parameters)
	{
		Handle.StartEffect(parameters);
	}

	public void StopEffect(ScreenEffectParameters? parameters)
	{
		if (parameters.HasValue)
		{
			Handle.StopEffect(parameters.Value.Time, parameters.Value.EaseType);
		}
		else
		{
			Handle.StopEffect();
		}
	}
}
