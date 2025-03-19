using System;
using System.Text.RegularExpressions;

public static class StringUtils
{
	public static readonly char[] SPLIT_LINES_CHARS_ARRAY = "\n\r".ToCharArray();

	public static readonly char[] REGEX_RESERVED_CHARS_ARRAY = "\\*.+?^$()[]{}".ToCharArray();

	public static string StripNonNumbers(string str)
	{
		return Regex.Replace(str, "[^0-9]", string.Empty);
	}

	public static string StripNewlines(string str)
	{
		return Regex.Replace(str, "[\\r\\n]", string.Empty);
	}

	public static bool CompareIgnoreCase(string a, string b)
	{
		return string.Compare(a, b, StringComparison.OrdinalIgnoreCase) == 0;
	}
}
