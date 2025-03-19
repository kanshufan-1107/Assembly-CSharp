using System.Collections.Generic;

public class MultiAttributeParser
{
	private Dictionary<string, string> rawDict;

	public bool load(string[] args, out string errMsg)
	{
		errMsg = null;
		rawDict = new Dictionary<string, string>();
		if (args.Length == 0)
		{
			errMsg = "There are too few number of arguments.";
			return false;
		}
		for (int i = 0; i < args.Length; i++)
		{
			string[] attributeSplit = args[i].Split('=');
			if (attributeSplit.Length <= 1)
			{
				errMsg = "Failed to parse into raw dictionary: no value provided.";
				return false;
			}
			rawDict.Add(attributeSplit[0], attributeSplit[1]);
		}
		return true;
	}

	public bool getIntAttribute(string key, out int? value, out string errMsg)
	{
		errMsg = null;
		value = null;
		if (rawDict.ContainsKey(key))
		{
			if (!int.TryParse(rawDict[key], out var attrValue))
			{
				errMsg = $"Failed to parse {key} int attribute value: The value must be a valid number.";
				return false;
			}
			value = attrValue;
		}
		return true;
	}

	public bool getBoolAttribute(string key, out bool? value, out string errMsg)
	{
		errMsg = null;
		value = null;
		if (rawDict.ContainsKey(key))
		{
			if (!bool.TryParse(rawDict[key], out var attrValue))
			{
				errMsg = $"Failed to parse {key} boolean attribute value: The value must be a valid boolean(true/false).";
				return false;
			}
			value = attrValue;
		}
		return true;
	}

	public bool getStringAttribute(string key, out string value)
	{
		value = null;
		if (rawDict.ContainsKey(key))
		{
			value = rawDict[key];
		}
		return true;
	}
}
