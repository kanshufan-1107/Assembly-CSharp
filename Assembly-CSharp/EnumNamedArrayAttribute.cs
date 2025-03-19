using System;
using UnityEngine;

public class EnumNamedArrayAttribute : PropertyAttribute
{
	public string[] names;

	public EnumNamedArrayAttribute(Type names_enum_type)
	{
		names = Enum.GetNames(names_enum_type);
	}
}
