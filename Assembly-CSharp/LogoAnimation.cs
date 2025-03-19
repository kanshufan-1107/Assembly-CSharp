using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using Hearthstone;
using UnityEngine;

public class LogoAnimation : MonoBehaviour
{
	public GameObject m_logoContainer;

	private static LogoAnimation s_instance;

	private GameObject m_logo;

	public UberText m_logoCopyright;

	private AssetReference m_LogoAssetRef = "LogoImage.prefab:c7bbbc47f4498224491bb952df4c6bcb";

	private void Awake()
	{
		s_instance = this;
		m_logo = AssetLoader.Get().InstantiatePrefab(m_LogoAssetRef);
		m_logo.SetActive(value: true);
		GameUtils.SetParent(m_logo, m_logoContainer, withRotation: true);
		m_logoContainer.SetActive(value: false);
		if (Localization.GetLocale() == Locale.zhCN)
		{
			m_logoCopyright.gameObject.SetActive(value: true);
			RenderUtils.SetAlpha(m_logoCopyright.gameObject, 1f);
		}
		HearthstoneApplication.Get().WillReset += OnWillReset;
	}

	public static LogoAnimation Get()
	{
		return s_instance;
	}

	private void OnDestroy()
	{
		if (HearthstoneApplication.Get() != null)
		{
			HearthstoneApplication.Get().WillReset -= OnWillReset;
		}
		s_instance = null;
	}

	public void HideLogo()
	{
		if (m_logoContainer != null)
		{
			m_logoContainer.SetActive(value: false);
		}
	}

	public IEnumerator<IAsyncJobResult> Job_FadeLogoIn()
	{
		float fadeInTime = 0.5f;
		m_logoContainer.SetActive(value: true);
		Hashtable fadeInArgs = iTweenManager.Get().GetTweenHashTable();
		fadeInArgs.Add("amount", 1f);
		fadeInArgs.Add("time", fadeInTime);
		fadeInArgs.Add("includechildren", true);
		fadeInArgs.Add("easetype", iTween.EaseType.easeInCubic);
		iTween.FadeTo(m_logo, fadeInArgs);
		yield return new WaitForDuration(fadeInTime);
	}

	public IEnumerator<IAsyncJobResult> Job_FadeLogoOut()
	{
		float fadeOutTime = 0.5f;
		Hashtable args = iTweenManager.Get().GetTweenHashTable();
		args.Add("amount", 0f);
		args.Add("delay", 0f);
		args.Add("time", fadeOutTime);
		args.Add("easetype", iTween.EaseType.linear);
		iTween.FadeTo(m_logo, args);
		yield return new WaitForDuration(fadeOutTime);
		DestroyLogoAnimation();
	}

	private void DestroyLogoAnimation()
	{
		Object.Destroy(base.gameObject);
	}

	public void ShowLogo()
	{
		if (m_logoContainer != null)
		{
			m_logoContainer.SetActive(value: true);
		}
	}

	private void OnWillReset()
	{
		Object.Destroy(base.gameObject);
	}
}
