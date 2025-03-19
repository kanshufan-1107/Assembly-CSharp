using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Blizzard.T5.Core;
using UnityEngine;

namespace Hearthstone.UI;

public class ParticleSystemDynamicPropertyResolverProxy : IDynamicPropertyResolverProxy, IDynamicPropertyResolver
{
	private struct PropertyTargetInfo
	{
		public object m_target;

		public PropertyInfo m_property;
	}

	private const char PathDelimiter = '.';

	private static readonly string[] s_propertyPathStrs = new string[2] { "Main.StartColor.Color", "Shape.Mesh" };

	private static Type s_obsoleteAttributeType = typeof(ObsoleteAttribute);

	private Map<string, DynamicPropertyInfo> m_properties = new Map<string, DynamicPropertyInfo>();

	private ParticleSystem m_particleSystem;

	public ICollection<DynamicPropertyInfo> DynamicProperties => m_properties.Values;

	public void SetTarget(object target)
	{
		m_particleSystem = (ParticleSystem)target;
		m_properties.Clear();
		GeneratePropertyData(target);
	}

	private void GeneratePropertyData(object target)
	{
		if (target == null)
		{
			return;
		}
		string[] array = s_propertyPathStrs;
		foreach (string propPathStr in array)
		{
			string pathId = GetFormattedPathId(propPathStr);
			PropertyTargetInfo[] propertyTargets = GetTargetsFromPathId(pathId);
			PropertyInfo prop = null;
			object newTarget = target;
			if (propertyTargets.Length != 0)
			{
				prop = propertyTargets.Last().m_property;
				newTarget = propertyTargets.Last().m_target;
			}
			if (!(prop == null) && NestedReferenceUtils.IsSupportedType(prop.PropertyType))
			{
				DynamicPropertyInfo dynPropInfo = new DynamicPropertyInfo
				{
					Id = pathId,
					Name = propPathStr,
					Type = prop.PropertyType,
					Value = (prop.PropertyType.IsEnum ? ((object)Convert.ToInt32(prop.GetValue(newTarget))) : prop.GetValue(newTarget))
				};
				m_properties.Add(pathId, dynPropInfo);
			}
		}
	}

	private PropertyTargetInfo[] GetTargetsFromPathId(string path)
	{
		PropertyInfo prop = null;
		object newTarget = m_particleSystem;
		string[] propNames = path.Split('.');
		PropertyTargetInfo[] propertyTargets = new PropertyTargetInfo[propNames.Length];
		for (int i = 0; i < propNames.Length; i++)
		{
			string propName = propNames[i];
			if (prop != null)
			{
				bool isStruct = prop.PropertyType.IsValueType && !prop.PropertyType.IsEnum;
				bool isClass = prop.PropertyType.IsClass;
				if (newTarget == null || (!isStruct && !isClass))
				{
					return new PropertyTargetInfo[0];
				}
				newTarget = prop.GetValue(newTarget);
			}
			prop = newTarget?.GetType().GetProperty(propName);
			if (prop == null || prop.IsDefined(s_obsoleteAttributeType))
			{
				return new PropertyTargetInfo[0];
			}
			propertyTargets[i].m_target = newTarget;
			propertyTargets[i].m_property = prop;
		}
		return propertyTargets;
	}

	private string GetFormattedPathId(string pathStr)
	{
		string[] propNames = pathStr.Split('.');
		string[] formattedPropNames = new string[propNames.Length];
		for (int i = 0; i < propNames.Length; i++)
		{
			formattedPropNames[i] = char.ToLowerInvariant(propNames[i][0]) + propNames[i].Substring(1);
		}
		return string.Join('.'.ToString(), formattedPropNames);
	}

	public bool GetDynamicPropertyValue(string id, out object value)
	{
		value = null;
		DynamicPropertyInfo dynPropInfo = null;
		if (m_properties.TryGetValue(id, out dynPropInfo))
		{
			value = dynPropInfo.Value;
			return true;
		}
		return false;
	}

	public bool SetDynamicPropertyValue(string id, object value)
	{
		if (m_properties.TryGetValue(id, out var dynPropInfo))
		{
			dynPropInfo.Value = (dynPropInfo.Type.IsEnum ? ((object)Convert.ToInt32(value)) : value);
			PropertyTargetInfo[] propertyTargets = GetTargetsFromPathId(id);
			object newValue = dynPropInfo.Value;
			object target = null;
			for (int i = propertyTargets.Length - 1; i >= 0; i--)
			{
				target = propertyTargets[i].m_target;
				propertyTargets[i].m_property.SetValue(target, newValue);
				newValue = target;
			}
			return true;
		}
		return false;
	}
}
