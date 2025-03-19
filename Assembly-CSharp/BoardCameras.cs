using System.Collections;
using UnityEngine;

public class BoardCameras : MonoBehaviour
{
	public AudioListener m_AudioListener;

	public float m_FieldOfViewDefault = 34.87045f;

	public float m_FieldOfViewZoomed = 25f;

	public AnimationCurve m_ZoomCurve;

	private static BoardCameras s_instance;

	private void Awake()
	{
		s_instance = this;
		if (LoadingScreen.Get() != null)
		{
			LoadingScreen.Get().NotifyMainSceneObjectAwoke(base.gameObject);
		}
	}

	private void OnDestroy()
	{
		s_instance = null;
	}

	public static BoardCameras Get()
	{
		return s_instance;
	}

	public AudioListener GetAudioListener()
	{
		return m_AudioListener;
	}

	public Camera GetCamera()
	{
		return base.transform.GetComponentInChildren<Camera>();
	}

	public IEnumerator TweenCameraFieldOfView(float finalFieldOfView, float tweenTime)
	{
		float initialFieldOfView = Camera.main.fieldOfView;
		if (finalFieldOfView == initialFieldOfView)
		{
			yield break;
		}
		Camera[] boardCameras = GetCameras();
		float timer = 0f;
		Camera[] array;
		while (timer < tweenTime)
		{
			timer += Time.deltaTime;
			array = boardCameras;
			foreach (Camera obj in array)
			{
				float curveValue = m_ZoomCurve.Evaluate(timer / tweenTime);
				obj.fieldOfView = Mathf.Lerp(initialFieldOfView, finalFieldOfView, curveValue);
			}
			yield return null;
		}
		array = boardCameras;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].fieldOfView = finalFieldOfView;
		}
	}

	private Camera[] GetCameras()
	{
		return base.transform.GetComponentsInChildren<Camera>();
	}
}
