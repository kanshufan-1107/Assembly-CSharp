using System;
using UnityEngine;

namespace Hearthstone.Timeline;

[Serializable]
public class EventEmitterEntry
{
	public enum ParameterType
	{
		None,
		String,
		Float,
		Int,
		Bool,
		Object
	}

	private readonly SendMessageOptions m_sendMessageOptions = SendMessageOptions.DontRequireReceiver;

	[SerializeField]
	private string m_methodName = "";

	[SerializeField]
	private ParameterType m_parameterType;

	[SerializeField]
	private string m_stringParameter = "";

	[SerializeField]
	private float m_floatParameter;

	[SerializeField]
	private int m_intParameter;

	[SerializeField]
	private bool m_boolParameter;

	[SerializeField]
	private UnityEngine.Object m_objectParameter;

	public bool CanExecute => true;

	public void Invoke(GameObject target)
	{
		if (!(target == null))
		{
			switch (m_parameterType)
			{
			case ParameterType.String:
				target.SendMessage(m_methodName, m_stringParameter, m_sendMessageOptions);
				break;
			case ParameterType.Float:
				target.SendMessage(m_methodName, m_floatParameter, m_sendMessageOptions);
				break;
			case ParameterType.Int:
				target.SendMessage(m_methodName, m_intParameter, m_sendMessageOptions);
				break;
			case ParameterType.Bool:
				target.SendMessage(m_methodName, m_boolParameter, m_sendMessageOptions);
				break;
			case ParameterType.Object:
				target.SendMessage(m_methodName, m_objectParameter, m_sendMessageOptions);
				break;
			default:
				target.SendMessage(m_methodName, m_sendMessageOptions);
				break;
			}
		}
	}
}
