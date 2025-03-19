using System;
using System.Collections.Generic;
using HutongGames.PlayMaker;
using UnityEngine;

namespace Hearthstone.UI;

public class PlayMakerDynamicPropertyResolverProxy : IDynamicPropertyResolverProxy, IDynamicPropertyResolver
{
	private PlayMakerFSM m_fsm;

	private List<DynamicPropertyInfo> m_properties = new List<DynamicPropertyInfo>();

	public ICollection<DynamicPropertyInfo> DynamicProperties => m_properties;

	public void SetTarget(object target)
	{
		m_fsm = (PlayMakerFSM)target;
		m_properties.Clear();
		if (!(m_fsm != null) || m_fsm.FsmVariables == null)
		{
			return;
		}
		NamedVariable[] allNamedVariables = m_fsm.FsmVariables.GetAllNamedVariables();
		foreach (NamedVariable variable in allNamedVariables)
		{
			string id = variable.Name;
			switch (variable.VariableType)
			{
			case VariableType.Int:
				m_properties.Add(new DynamicPropertyInfo
				{
					Id = id,
					Name = id,
					Type = typeof(int),
					Value = variable.RawValue
				});
				break;
			case VariableType.Float:
				m_properties.Add(new DynamicPropertyInfo
				{
					Id = id,
					Name = id,
					Type = typeof(float),
					Value = variable.RawValue
				});
				break;
			case VariableType.Color:
				m_properties.Add(new DynamicPropertyInfo
				{
					Id = id,
					Name = id,
					Type = typeof(Color),
					Value = variable.RawValue
				});
				break;
			case VariableType.String:
				m_properties.Add(new DynamicPropertyInfo
				{
					Id = id,
					Name = id,
					Type = typeof(string),
					Value = variable.RawValue
				});
				break;
			case VariableType.Bool:
				m_properties.Add(new DynamicPropertyInfo
				{
					Id = id,
					Name = id,
					Type = typeof(bool),
					Value = variable.RawValue
				});
				break;
			case VariableType.Vector3:
			{
				Vector3 value2 = (Vector3)variable.RawValue;
				m_properties.Add(new DynamicPropertyInfo
				{
					Id = id + ".x",
					Name = id + ".x",
					Type = typeof(float),
					Value = value2.x
				});
				m_properties.Add(new DynamicPropertyInfo
				{
					Id = id + ".y",
					Name = id + ".y",
					Type = typeof(float),
					Value = value2.y
				});
				m_properties.Add(new DynamicPropertyInfo
				{
					Id = id + ".z",
					Name = id + ".z",
					Type = typeof(float),
					Value = value2.z
				});
				break;
			}
			case VariableType.Vector2:
			{
				Vector2 value = (Vector2)variable.RawValue;
				m_properties.Add(new DynamicPropertyInfo
				{
					Id = id + ".x",
					Name = id + ".x",
					Type = typeof(float),
					Value = value.x
				});
				m_properties.Add(new DynamicPropertyInfo
				{
					Id = id + ".y",
					Name = id + ".y",
					Type = typeof(float),
					Value = value.y
				});
				break;
			}
			}
		}
	}

	public bool GetDynamicPropertyValue(string id, out object value)
	{
		value = null;
		if (m_fsm != null && m_fsm.FsmVariables != null && id != null)
		{
			GetIdAndDimension(ref id, out var dimension);
			NamedVariable variable = m_fsm.FsmVariables.GetVariable(id);
			if (variable != null)
			{
				value = variable.RawValue;
				switch (variable.VariableType)
				{
				case VariableType.Vector2:
					switch (dimension)
					{
					case 'x':
						value = ((Vector2)variable.RawValue).x;
						break;
					case 'y':
						value = ((Vector2)variable.RawValue).y;
						break;
					}
					break;
				case VariableType.Vector3:
					switch (dimension)
					{
					case 'x':
						value = ((Vector3)variable.RawValue).x;
						break;
					case 'y':
						value = ((Vector3)variable.RawValue).y;
						break;
					case 'z':
						value = ((Vector3)variable.RawValue).z;
						break;
					}
					break;
				}
				return true;
			}
		}
		return false;
	}

	public bool SetDynamicPropertyValue(string id, object value)
	{
		if (m_fsm != null && Application.IsPlaying(m_fsm) && m_fsm.FsmVariables != null && id != null)
		{
			GetIdAndDimension(ref id, out var dimension);
			NamedVariable variable = m_fsm.FsmVariables.GetVariable(id);
			if (variable != null)
			{
				switch (variable.VariableType)
				{
				case VariableType.Int:
					((FsmInt)variable).RawValue = value;
					break;
				case VariableType.Float:
					((FsmFloat)variable).RawValue = value;
					break;
				case VariableType.String:
					((FsmString)variable).RawValue = value;
					break;
				case VariableType.Bool:
					((FsmBool)variable).RawValue = value;
					break;
				case VariableType.Color:
					((FsmColor)variable).RawValue = value;
					break;
				case VariableType.Vector2:
				{
					Vector2 vec2 = (Vector2)variable.RawValue;
					switch (dimension)
					{
					case 'x':
						vec2.x = (float)value;
						break;
					case 'y':
						vec2.y = (float)value;
						break;
					}
					((FsmVector2)variable).Value = vec2;
					break;
				}
				case VariableType.Vector3:
				{
					Vector3 vec = (Vector3)variable.RawValue;
					switch (dimension)
					{
					case 'x':
						vec.x = (float)value;
						break;
					case 'y':
						vec.y = (float)value;
						break;
					case 'z':
						vec.z = (float)value;
						break;
					}
					((FsmVector3)variable).Value = vec;
					break;
				}
				default:
					return false;
				}
				return true;
			}
		}
		return false;
	}

	private static void GetIdAndDimension(ref string id, out char dimension)
	{
		dimension = '\0';
		int dotIndex = id.IndexOf(".", StringComparison.Ordinal);
		if (dotIndex > 0)
		{
			dimension = id[id.Length - 1];
			id = id.Remove(dotIndex);
		}
	}
}
