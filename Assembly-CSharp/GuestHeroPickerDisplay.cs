using System.Collections;
using Hearthstone.UI;
using UnityEngine;

[CustomEditClass]
public class GuestHeroPickerDisplay : MonoBehaviour
{
	public AsyncReference m_trayControllerReference;

	public AsyncReference m_trayControllerReference_phone;

	private static GuestHeroPickerDisplay s_instance;

	private GuestHeroPickerTrayDisplay m_heroPickerTray;

	private Vector3 startOffset = new Vector3(-120f, 0f, 0f);

	private Vector3 startPosition;

	private void Awake()
	{
		if (s_instance != null)
		{
			Debug.LogWarning("GuestHeroPickerDisplay is supposed to be a singleton, but a second instance of it is being created!");
		}
		s_instance = this;
		startPosition = base.transform.localPosition;
		base.transform.localPosition = startPosition + startOffset;
		m_trayControllerReference.RegisterReadyListener<VisualController>(OnTrayControllerReady);
		m_trayControllerReference_phone.RegisterReadyListener<VisualController>(OnTrayControllerReady);
		SoundManager.Get().Load(SoundUtils.SquarePanelSlideOnSFX);
		SoundManager.Get().Load(SoundUtils.SquarePanelSlideOffSFX);
	}

	private void OnTrayControllerReady(VisualController trayController)
	{
		m_heroPickerTray = trayController.GetComponentInChildren<GuestHeroPickerTrayDisplay>();
		if (m_heroPickerTray == null)
		{
			Debug.LogError("GuestHeroPickerTrayDisplay component not found in GuestHeroPickerTray object.");
		}
		else if (trayController == null)
		{
			Debug.LogError("trayController was null in OnTrayControllerReady!");
		}
		else
		{
			m_heroPickerTray.InitAssets();
		}
	}

	public void ShowTray()
	{
		Hashtable posArgs = iTweenManager.Get().GetTweenHashTable();
		posArgs.Add("position", startPosition);
		posArgs.Add("time", 1f);
		posArgs.Add("islocal", true);
		posArgs.Add("oncomplete", "OnTrayShown");
		posArgs.Add("oncompletetarget", base.gameObject);
		posArgs.Add("easetype", iTween.EaseType.easeOutBounce);
		SoundManager.Get().LoadAndPlay(SoundUtils.SquarePanelSlideOnSFX);
		iTween.MoveTo(base.gameObject, posArgs);
	}

	public void OnTrayShown()
	{
		m_heroPickerTray.EnableBackButton(enabled: true);
	}

	public void HideTray(float delay = 0f)
	{
		Hashtable posArgs = iTweenManager.Get().GetTweenHashTable();
		posArgs.Add("position", startPosition + startOffset);
		posArgs.Add("time", 1f);
		posArgs.Add("islocal", true);
		posArgs.Add("oncomplete", "OnTrayHidden");
		posArgs.Add("oncompletetarget", base.gameObject);
		posArgs.Add("easetype", iTween.EaseType.easeOutBounce);
		posArgs.Add("delay", delay);
		SoundManager.Get().LoadAndPlay(SoundUtils.SquarePanelSlideOffSFX);
		iTween.MoveTo(base.gameObject, posArgs);
	}

	private void OnTrayHidden()
	{
		m_heroPickerTray.Unload();
		Object.DestroyImmediate(base.gameObject);
		if (TavernBrawlDisplay.Get() != null)
		{
			TavernBrawlDisplay.Get().OnHeroPickerClosed();
		}
	}

	public static GuestHeroPickerDisplay Get()
	{
		return s_instance;
	}
}
