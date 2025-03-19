using System;
using Hearthstone.UI.Internal;
using UnityEngine;

namespace Hearthstone.UI;

[Serializable]
public class AsyncReference
{
	[SerializeField]
	private NestedReference m_nestedReference;

	public UnityEngine.Object Object { get; private set; }

	public bool IsReady { get; private set; }

	private event Action<UnityEngine.Object> m_readyListeners;

	private event Action<AsyncReference> m_selfReadyListener;

	public void RegisterSelfReadyListener<T>(Action<AsyncReference> listener) where T : UnityEngine.Object
	{
		m_selfReadyListener += listener;
		Resolve<T>(null);
	}

	public void RegisterReadyListener<T>(Action<T> listener) where T : UnityEngine.Object
	{
		m_readyListeners += delegate(UnityEngine.Object a)
		{
			listener(a as T);
		};
		Resolve<T>(null);
	}

	private void HandleReady(UnityEngine.Object target)
	{
		IsReady = true;
		Object = target;
		if (this.m_readyListeners != null)
		{
			this.m_readyListeners(Object);
			this.m_readyListeners = null;
		}
		if (this.m_selfReadyListener != null)
		{
			this.m_selfReadyListener(this);
			this.m_selfReadyListener = null;
		}
	}

	private void Resolve<T>(object unused) where T : UnityEngine.Object
	{
		IAsyncInitializationBehavior behaviorToWaitFor = null;
		UnityEngine.Object resolvedTarget = null;
		Action<INestedReferenceResolver> resolveFunc = delegate(INestedReferenceResolver resolver)
		{
			if (!resolver.IsReady)
			{
				behaviorToWaitFor = resolver;
			}
		};
		Component rootObjectComponent = m_nestedReference.RootObject as Component;
		if (rootObjectComponent != null)
		{
			WidgetInstance widgetInstanceRoot = (m_nestedReference.RootObject as WidgetInstance) ?? rootObjectComponent.GetComponent<WidgetInstance>();
			bool isDirectReference = m_nestedReference.TargetObjectIds == null || m_nestedReference.TargetObjectIds.Length == 0;
			if (widgetInstanceRoot != null && isDirectReference)
			{
				resolvedTarget = widgetInstanceRoot;
				resolveFunc(widgetInstanceRoot);
			}
			else
			{
				NestedReference.Resolve(m_nestedReference.RootObject, m_nestedReference.TargetObjectIds, out resolvedTarget, resolveFunc);
			}
		}
		if (behaviorToWaitFor == null && resolvedTarget != null)
		{
			if (!(resolvedTarget is T))
			{
				Component resolvedComponent = resolvedTarget as Component;
				if (resolvedComponent != null)
				{
					resolvedTarget = resolvedComponent.GetComponent(typeof(T));
				}
				if (resolvedTarget == null)
				{
					WidgetInstance resolvedInstance = resolvedComponent as WidgetInstance;
					if (resolvedInstance != null && resolvedInstance.Widget != null)
					{
						resolvedTarget = resolvedInstance.Widget.GetComponent(typeof(T));
					}
				}
			}
			if (resolvedTarget is IAsyncInitializationBehavior { IsReady: false } asyncBehavior)
			{
				behaviorToWaitFor = asyncBehavior;
			}
		}
		if (behaviorToWaitFor != null)
		{
			behaviorToWaitFor.RegisterReadyListener(Resolve<T>);
		}
		else
		{
			HandleReady(resolvedTarget);
		}
	}
}
