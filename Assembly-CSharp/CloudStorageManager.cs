public static class CloudStorageManager
{
	public static bool SetString(string key, string value)
	{
		if (!IsCloudStorageSupported())
		{
			return false;
		}
		CloudSetString(key, value);
		return true;
	}

	public static string GetString(string key)
	{
		if (!IsCloudStorageSupported())
		{
			return null;
		}
		return CloudGetString(key);
	}

	public static void RemoveObject(string key)
	{
		if (IsCloudStorageSupported())
		{
			CloudRemoveObject(key);
		}
	}

	private static void CloudSetString(string key, string value)
	{
	}

	private static string CloudGetString(string key)
	{
		return null;
	}

	private static void CloudRemoveObject(string key)
	{
	}

	private static bool IsCloudStorageSupported()
	{
		return false;
	}
}
