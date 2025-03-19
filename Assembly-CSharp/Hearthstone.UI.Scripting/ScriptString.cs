using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Hearthstone.UI.Scripting;

[Serializable]
public struct ScriptString
{
	private static StringBuilder s_currentString = new StringBuilder(32);

	private static int[] s_ParsedInts = new int[10];

	private static char[] s_searchChars = new char[4] { '(', ' ', '\n', '\r' };

	private static Regex s_propertyRegex = new Regex("(?:\\(|[\\s]|^)\\$([\\d]+)(?:\\.)\\$([\\d]+)");

	public string Script;

	public string HumanReadableScript;

	public HashSet<int> GetDataModelIDs(HashSet<int> dataModelIds)
	{
		s_currentString.Clear();
		int currentIndex = 0;
		while (currentIndex < Script.Length)
		{
			int firstIndex = Script.IndexOfAny(s_searchChars, currentIndex);
			if (firstIndex == -1)
			{
				firstIndex = Script.Length;
			}
			s_currentString.Clear();
			s_currentString.Append(Script, currentIndex, firstIndex - currentIndex);
			currentIndex = firstIndex + 1;
			if (s_currentString.Length == 0 || s_currentString[0] != '$')
			{
				continue;
			}
			int length = -1;
			for (int i = 1; i < s_currentString.Length; i++)
			{
				char currentChar = s_currentString[i];
				if (!char.IsDigit(currentChar))
				{
					break;
				}
				length++;
				s_ParsedInts[i - 1] = Convert.ToInt32(currentChar) - 48;
			}
			int result = 0;
			int multiplier = 1;
			for (int i2 = length; i2 > -1; i2--)
			{
				result += s_ParsedInts[i2] * multiplier;
				multiplier *= 10;
			}
			dataModelIds.Add(result);
		}
		return dataModelIds;
	}

	public HashSet<int> GetDataModelIDs()
	{
		HashSet<int> dataModelIds = new HashSet<int>();
		GetDataModelIDs(dataModelIds);
		return dataModelIds;
	}

	public Dictionary<int, HashSet<int>> GetPropertyIds()
	{
		MatchCollection results = s_propertyRegex.Matches(Script);
		Dictionary<int, HashSet<int>> dataModelIds = new Dictionary<int, HashSet<int>>();
		int i = 0;
		for (int iMax = results.Count; i < iMax; i++)
		{
			int dataModelId = int.Parse(results[i].Groups[1].Value);
			if (!dataModelIds.ContainsKey(dataModelId))
			{
				HashSet<int> propertyTable = new HashSet<int>();
				for (int j = 2; j < results[i].Groups.Count; j++)
				{
					propertyTable.Add(int.Parse(results[i].Groups[j].Value));
				}
				dataModelIds.Add(dataModelId, propertyTable);
			}
			else
			{
				for (int k = 2; k < results[i].Groups.Count; k++)
				{
					dataModelIds[dataModelId].Add(int.Parse(results[i].Groups[k].Value));
				}
			}
		}
		return dataModelIds;
	}
}
