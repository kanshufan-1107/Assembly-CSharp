using System;

public static class OptionUtils
{
	public static Option GetOptionFromString(string optionName)
	{
		if (string.IsNullOrEmpty(optionName))
		{
			return Option.INVALID;
		}
		object parsedOption = Enum.Parse(typeof(Option), optionName, ignoreCase: true);
		if (parsedOption == null)
		{
			return Option.INVALID;
		}
		return (Option)parsedOption;
	}
}
