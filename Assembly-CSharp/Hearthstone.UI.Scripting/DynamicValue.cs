using System;

namespace Hearthstone.UI.Scripting;

public struct DynamicValue
{
	public readonly object Value;

	public readonly Type ValueType;

	public bool HasValidValue
	{
		get
		{
			if (ValueType != null)
			{
				if (ValueType.IsValueType)
				{
					return Value != null;
				}
				return true;
			}
			return false;
		}
	}

	public DynamicValue(object value, Type valueType)
	{
		Value = value;
		ValueType = ((valueType == null && value != null) ? value.GetType() : valueType);
	}
}
