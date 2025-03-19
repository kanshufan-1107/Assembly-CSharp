using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using UnityEngine;

public class ScreenEffectsMgr : IService, IHasUpdate
{
	private Camera m_MainCamera;

	private ScreenEffectsRender m_ScreenEffectsRender;

	private bool m_enabled;

	private static List<ScreenEffect> m_ActiveScreenEffects;

	public bool IsActive => m_enabled;

	public IEnumerator<IAsyncJobResult> Initialize(ServiceLocator serviceLocator)
	{
		if (m_ActiveScreenEffects == null)
		{
			m_ActiveScreenEffects = new List<ScreenEffect>();
		}
		yield return new WaitForMainCamera();
		OnEnable();
	}

	public Type[] GetDependencies()
	{
		return new Type[1] { typeof(UniversalInputManager) };
	}

	public void Shutdown()
	{
		OnDisable();
		if (m_ActiveScreenEffects != null)
		{
			m_ActiveScreenEffects.Clear();
			m_ActiveScreenEffects = null;
		}
	}

	public void Update()
	{
		if (m_MainCamera == null)
		{
			if (Camera.main == null)
			{
				return;
			}
			Init();
		}
		if (m_ScreenEffectsRender == null)
		{
			return;
		}
		if (m_ActiveScreenEffects != null && m_ActiveScreenEffects.Count > 0)
		{
			if (!m_ScreenEffectsRender.enabled)
			{
				m_ScreenEffectsRender.enabled = true;
			}
		}
		else if (m_ScreenEffectsRender.enabled)
		{
			m_ScreenEffectsRender.enabled = false;
		}
	}

	public void SetActive(bool enabled)
	{
		if (m_enabled != enabled)
		{
			m_enabled = enabled;
			if (m_enabled)
			{
				OnEnable();
			}
			else
			{
				OnDisable();
			}
		}
	}

	private void OnDisable()
	{
		if (m_ScreenEffectsRender != null)
		{
			m_ScreenEffectsRender.enabled = false;
		}
	}

	private void OnEnable()
	{
		if (!(Camera.main == null))
		{
			Init();
		}
	}

	public static ScreenEffectsMgr Get()
	{
		return ServiceManager.Get<ScreenEffectsMgr>();
	}

	public static void RegisterScreenEffect(ScreenEffect effect)
	{
		if (m_ActiveScreenEffects == null)
		{
			m_ActiveScreenEffects = new List<ScreenEffect>();
		}
		if (!m_ActiveScreenEffects.Contains(effect))
		{
			m_ActiveScreenEffects.Add(effect);
		}
	}

	public static void UnRegisterScreenEffect(ScreenEffect effect)
	{
		if (m_ActiveScreenEffects != null)
		{
			m_ActiveScreenEffects.Remove(effect);
		}
	}

	public int GetActiveScreenEffectsCount()
	{
		if (m_ActiveScreenEffects == null)
		{
			return 0;
		}
		return m_ActiveScreenEffects.Count;
	}

	private void Init()
	{
		m_MainCamera = Camera.main;
		if (!(m_MainCamera == null))
		{
			m_ScreenEffectsRender = m_MainCamera.GetComponent<ScreenEffectsRender>();
			if (m_ScreenEffectsRender == null)
			{
				m_ScreenEffectsRender = m_MainCamera.gameObject.AddComponent<ScreenEffectsRender>();
				m_MainCamera.allowHDR = false;
			}
			else
			{
				m_ScreenEffectsRender.enabled = true;
			}
		}
	}
}
