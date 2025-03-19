using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace Hearthstone.Tendril;

public class TendrilController : MonoBehaviour
{
	public class TendrilType
	{
		public Type type;

		public List<TendrilBase> tendrils = new List<TendrilBase>();
	}

	public TendrilBase.UpdateMode updateMode;

	private List<TendrilBase> m_tendrils = new List<TendrilBase>();

	private List<TendrilType> m_tendrilTypes = new List<TendrilType>();

	private void OnEnable()
	{
		RenderPipelineManager.endFrameRendering += EndFrameRendering;
	}

	private void OnDisable()
	{
		RenderPipelineManager.endFrameRendering -= EndFrameRendering;
	}

	private void Awake()
	{
		m_tendrils = (from x in GetComponentsInChildren<TendrilBase>()
			orderby x.priority
			select x).ToList();
		List<TendrilBase> tendrilsToRemove = new List<TendrilBase>();
		foreach (TendrilBase tendril in m_tendrils)
		{
			if (tendril.enabled && !tendril.autonomous)
			{
				for (int i = 0; i < m_tendrilTypes.Count; i++)
				{
					if (m_tendrilTypes[i].type == tendril.GetType())
					{
						m_tendrilTypes[i].tendrils.Add(tendril);
					}
				}
				TendrilType tendrilType = new TendrilType
				{
					type = tendril.GetType()
				};
				tendrilType.tendrils.Add(tendril);
				m_tendrilTypes.Add(tendrilType);
			}
			else
			{
				tendrilsToRemove.Add(tendril);
			}
		}
		foreach (TendrilBase tendril2 in tendrilsToRemove)
		{
			m_tendrils.Remove(tendril2);
		}
	}

	public List<TendrilBase> GetTendrils()
	{
		return m_tendrils;
	}

	public bool GetTendrilsByType(Type type, out List<TendrilBase> tendrils)
	{
		tendrils = new List<TendrilBase>();
		foreach (TendrilType tendrilType in m_tendrilTypes)
		{
			if (tendrilType.type == type)
			{
				tendrils = tendrilType.tendrils;
				return true;
			}
		}
		return false;
	}

	private void CacheTendrils(List<TendrilBase> tendrils)
	{
		for (int i = 0; i < tendrils.Count; i++)
		{
			tendrils[i].Cache();
		}
	}

	private void ResolveTendrils(List<TendrilBase> tendrils)
	{
		for (int i = 0; i < tendrils.Count; i++)
		{
			tendrils[i].Resolve();
		}
	}

	private void PostRenderTendrils(List<TendrilBase> tendrils)
	{
		for (int i = 0; i < tendrils.Count; i++)
		{
			tendrils[i].PostRender();
		}
	}

	private void Update()
	{
		if (updateMode == TendrilBase.UpdateMode.Update)
		{
			PostRenderTendrils(m_tendrils);
			CacheTendrils(m_tendrils);
			ResolveTendrils(m_tendrils);
		}
	}

	private void LateUpdate()
	{
		if (updateMode == TendrilBase.UpdateMode.LateUpdate)
		{
			CacheTendrils(m_tendrils);
			ResolveTendrils(m_tendrils);
		}
	}

	private void EndFrameRendering(ScriptableRenderContext context, Camera[] cameras)
	{
		if (updateMode == TendrilBase.UpdateMode.LateUpdate)
		{
			PostRenderTendrils(m_tendrils);
		}
	}
}
