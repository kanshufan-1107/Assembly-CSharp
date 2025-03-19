using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class DebugUtils
{
	public static string GetHierarchyPath(Object obj, char separator = '.')
	{
		StringBuilder stringBuilder = new StringBuilder();
		GetHierarchyPath_Internal(stringBuilder, obj, separator);
		return stringBuilder.ToString();
	}

	public static string GetHierarchyPathAndType(Object obj, char separator = '.')
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("[Type]=").Append(obj.GetType().FullName).Append(" [Path]=");
		GetHierarchyPath_Internal(stringBuilder, obj, separator);
		return stringBuilder.ToString();
	}

	private static bool GetHierarchyPath_Internal(StringBuilder b, Object obj, char separator)
	{
		if (obj == null)
		{
			return false;
		}
		Transform transform = ((obj is GameObject) ? ((GameObject)obj).transform : ((obj is Component) ? ((Component)obj).transform : null));
		List<string> objNames = new List<string>();
		while (transform != null)
		{
			objNames.Insert(0, transform.gameObject.name);
			transform = transform.parent;
		}
		if (objNames.Count > 0 && separator == '/')
		{
			b.Append(separator);
		}
		for (int i = 0; i < objNames.Count; i++)
		{
			b.Append(objNames[i]);
			if (i < objNames.Count - 1)
			{
				b.Append(separator);
			}
		}
		return true;
	}
}
