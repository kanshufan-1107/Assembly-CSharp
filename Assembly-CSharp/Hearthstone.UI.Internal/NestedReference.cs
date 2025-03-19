using System;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;

namespace Hearthstone.UI.Internal;

[Serializable]
public struct NestedReference
{
	public enum TargetTypes
	{
		Unknown,
		String,
		Color,
		Float,
		Bool,
		Int,
		Material,
		Double,
		Long,
		Component,
		Texture,
		Vector2,
		Vector3,
		Vector4,
		Enum,
		Mesh
	}

	public enum ResolveError
	{
		None,
		ReferenceNotProvided,
		ReferenceNotLoaded,
		ReferenceMissing,
		NestedResolverMissing,
		UnableToResolveProperty,
		UnknownResolveTarget
	}

	public struct ResolveResult
	{
		public bool IsSuccessful;

		public ResolveError ResolveError;
	}

	public enum Scope
	{
		Default,
		Children,
		Foreign
	}

	[SerializeField]
	private long[] m_targetObjectIds;

	[SerializeField]
	private UnityEngine.Object m_targetObject;

	[SerializeField]
	private string m_targetPath;

	[SerializeField]
	private TargetTypes m_targetType;

	[SerializeField]
	private string m_stringType;

	private Action<object> m_readyListener;

	private object m_payload;

	public UnityEngine.Object RootObject => m_targetObject;

	public long[] TargetObjectIds => m_targetObjectIds;

	public TargetTypes TargetType => m_targetType;

	public string TargetPath => m_targetPath;

	public ResolveResult Resolve(out NestedReferenceTargetInfo targetInfo)
	{
		return Resolve(m_targetObject, m_targetObjectIds, m_targetPath, out targetInfo);
	}

	public ResolveResult Resolve(out GameObject gameObject)
	{
		gameObject = null;
		NestedReferenceTargetInfo targetInfo;
		ResolveResult result = Resolve(out targetInfo);
		if (!result.IsSuccessful)
		{
			return result;
		}
		Component component = targetInfo.Target as Component;
		if (component != null)
		{
			gameObject = component.gameObject;
			result.IsSuccessful = true;
			return result;
		}
		gameObject = targetInfo.Target as GameObject;
		if (gameObject != null)
		{
			result.IsSuccessful = true;
			return result;
		}
		result.IsSuccessful = false;
		result.ResolveError = ResolveError.UnknownResolveTarget;
		return result;
	}

	public static ResolveResult Resolve(UnityEngine.Object root, long[] objectIds, string propertyPath, out NestedReferenceTargetInfo property, Action<INestedReferenceResolver> visitor = null)
	{
		property = default(NestedReferenceTargetInfo);
		ResolveResult result = Resolve(root, objectIds, out property.Target, visitor);
		if (!result.IsSuccessful)
		{
			if (result.ResolveError == ResolveError.ReferenceNotProvided && !string.IsNullOrEmpty(propertyPath))
			{
				result.ResolveError = ResolveError.ReferenceMissing;
			}
			return result;
		}
		if (string.IsNullOrEmpty(propertyPath))
		{
			result.IsSuccessful = true;
			return result;
		}
		IDynamicPropertyResolver propertyResolver = DynamicPropertyResolvers.TryGetResolver(property.Target);
		if (propertyResolver != null)
		{
			if (propertyResolver.GetDynamicPropertyValue(propertyPath, out var _))
			{
				property.Path = propertyPath;
				result.IsSuccessful = true;
				return result;
			}
			result.IsSuccessful = false;
			result.ResolveError = ResolveError.UnableToResolveProperty;
			return result;
		}
		string[] parts = propertyPath.Split('.');
		Type currentType = property.Target.GetType();
		for (int i = 0; i < parts.Length; i++)
		{
			PropertyInfo prop = currentType.GetProperty(parts[i], BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (prop != null)
			{
				if (i == parts.Length - 1)
				{
					property.Property = prop;
					result.IsSuccessful = true;
					return result;
				}
				currentType = prop.PropertyType;
			}
		}
		result.IsSuccessful = false;
		result.ResolveError = ResolveError.UnableToResolveProperty;
		return result;
	}

	public bool TryGetValueFromTarget<T>(out T value)
	{
		value = default(T);
		if (!Resolve(m_targetObject, m_targetObjectIds, out var target).IsSuccessful)
		{
			return false;
		}
		IDynamicPropertyResolver propertyResolver = DynamicPropertyResolvers.TryGetResolver(target);
		if (propertyResolver != null && propertyResolver.GetDynamicPropertyValue(m_targetPath, out var propertyValue))
		{
			if (propertyValue is T)
			{
				value = (T)propertyValue;
				return true;
			}
			Log.All.PrintError("TryGetPropertyValue on NestedReference invalid type!");
			return false;
		}
		string[] parts = m_targetPath.Split('.');
		Type currentType = target.GetType();
		object currentPropValue = target;
		for (int i = 0; i < parts.Length; i++)
		{
			PropertyInfo prop = currentType.GetProperty(parts[i], BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (!(prop != null))
			{
				continue;
			}
			currentPropValue = prop.GetValue(currentPropValue);
			if (i == parts.Length - 1)
			{
				if (currentPropValue is T)
				{
					value = (T)currentPropValue;
					return true;
				}
				Log.UIFramework.PrintError("TryGetPropertyValue on NestedReference invalid type!");
			}
			currentType = prop.PropertyType;
		}
		return false;
	}

	public static ResolveResult Resolve(UnityEngine.Object root, long[] objectIds, out UnityEngine.Object target, Action<INestedReferenceResolver> visitor = null)
	{
		ResolveResult result = default(ResolveResult);
		target = null;
		if (root == null)
		{
			result.IsSuccessful = false;
			result.ResolveError = ResolveError.ReferenceNotProvided;
			return result;
		}
		if (objectIds == null || objectIds.Length == 0)
		{
			target = root;
			result.IsSuccessful = true;
			return result;
		}
		INestedReferenceResolver nestedResolver = root as INestedReferenceResolver;
		for (int i = 0; i < objectIds.Length; i++)
		{
			if (nestedResolver == null)
			{
				break;
			}
			visitor?.Invoke(nestedResolver);
			NestedReferenceComponentInfo info = nestedResolver.GetComponentInfoById(objectIds[i]);
			target = info.FoundComponent;
			if (target == null)
			{
				result.IsSuccessful = false;
				result.ResolveError = (info.CheckedAllComponents ? ResolveError.ReferenceMissing : ResolveError.ReferenceNotLoaded);
				return result;
			}
			nestedResolver = target as INestedReferenceResolver;
		}
		if (target != null)
		{
			result.IsSuccessful = true;
			return result;
		}
		result.IsSuccessful = false;
		result.ResolveError = ResolveError.NestedResolverMissing;
		return result;
	}

	public void RegisterReadyListener(Action<object> action, object payload = null)
	{
		m_readyListener = action;
		m_payload = payload;
		RegisterReadyHandler(null);
	}

	public void RemoveReadyOrInactiveListener()
	{
		m_readyListener = null;
		m_payload = null;
	}

	private void RegisterReadyHandler(object unused)
	{
		IAsyncInitializationBehavior behaviorToWaitFor = null;
		if (RootObject == null || TargetObjectIds == null || TargetObjectIds.Length == 0)
		{
			if (m_readyListener != null)
			{
				m_readyListener(m_payload);
			}
			return;
		}
		Resolve(RootObject, TargetObjectIds, out var resolvedTarget, delegate(INestedReferenceResolver resolver)
		{
			if (!resolver.IsReady)
			{
				behaviorToWaitFor = resolver;
			}
		});
		if (behaviorToWaitFor == null && resolvedTarget != null && resolvedTarget is IAsyncInitializationBehavior { IsReady: false } asyncBehavior)
		{
			behaviorToWaitFor = asyncBehavior;
		}
		if (behaviorToWaitFor != null)
		{
			behaviorToWaitFor.RegisterReadyListener(RegisterReadyHandler);
		}
		else if (m_readyListener != null)
		{
			m_readyListener(m_payload);
		}
	}

	[Conditional("UNITY_EDITOR")]
	private static void SetErrorFriendlyPathOnResult(ResolveResult result, string path)
	{
	}
}
