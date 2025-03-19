using System;
using MiniJSON;

public static class DataUtilities
{
	public static T GetValueAs<T>(this JsonNode node, string key, T defaultValue = default(T))
	{
		if (node.TryGetValueAs<T>(key, out var outputValue))
		{
			return outputValue;
		}
		return defaultValue;
	}

	public static bool TryGetValueAs<T>(this JsonNode node, string key, out T outputValue)
	{
		outputValue = default(T);
		if (key == null || node == null)
		{
			return false;
		}
		try
		{
			if (node.TryGetValue(key, out var valObject))
			{
				if (valObject != null)
				{
					outputValue = (T)Convert.ChangeType(valObject, typeof(T));
					return true;
				}
				if (!typeof(T).IsValueType)
				{
					return true;
				}
			}
		}
		catch
		{
		}
		return false;
	}
}
