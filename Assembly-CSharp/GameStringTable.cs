using System;
using System.Collections.Generic;
using System.IO;
using Assets;
using UnityEngine;

public class GameStringTable
{
	public class Entry
	{
		public string m_key;

		public string m_value;
	}

	public class Header
	{
		public int m_entryStartIndex = -1;

		public int m_keyIndex = -1;

		public int m_valueIndex = -1;
	}

	private delegate string FilePathFromCategoryCallback(Global.GameStringCategory cat, Locale locale, bool native);

	public const string KEY_FIELD_NAME = "TAG";

	public const string VALUE_FIELD_NAME = "TEXT";

	private Global.GameStringCategory m_category;

	private Dictionary<string, string> m_table = new Dictionary<string, string>();

	public bool Load(Global.GameStringCategory cat, bool native = false)
	{
		string path = GetFilePathWithLoadOrder(cat, native, GetFilePathFromCategory);
		string audioPath = GetFilePathWithLoadOrder(cat, native, GetAudioFilePathFromCategory);
		return Load(cat, path, audioPath);
	}

	public bool Load(Global.GameStringCategory cat, Locale locale, bool native = false)
	{
		string path = GetFilePathFromCategory(cat, locale, native);
		string audioPath = GetAudioFilePathFromCategory(cat, locale, native);
		return Load(cat, path, audioPath);
	}

	public bool Load(Global.GameStringCategory cat, string path, string audioPath)
	{
		m_category = Global.GameStringCategory.INVALID;
		m_table.Clear();
		if (File.Exists(path) && !LoadFile(path))
		{
			Error.AddDevWarningNonRepeating("GameStrings Error", "GameStringTable.Load() - Failed to load {0} for category {1}.", path, cat);
			return false;
		}
		if (File.Exists(audioPath) && !LoadFile(audioPath))
		{
			Error.AddDevWarningNonRepeating("GameStrings Error", "GameStringTable.Load() - Failed to load {0} for category {1}.", audioPath, cat);
			return false;
		}
		if (m_table.Count == 0)
		{
			Error.AddDevWarningNonRepeating("GameStrings Error", "GameStringTable.Load() - There are no entries for category {0} - path: {1}.", cat, path);
			return false;
		}
		m_category = cat;
		return true;
	}

	public string Get(string key)
	{
		m_table.TryGetValue(key, out var val);
		return val;
	}

	public Dictionary<string, string> GetAll()
	{
		return m_table;
	}

	public Global.GameStringCategory GetCategory()
	{
		return m_category;
	}

	public bool LoadFile(string path)
	{
		Header header = null;
		bool hasHeader = false;
		try
		{
			using ReadOnlySpanStreamReader sr = new ReadOnlySpanStreamReader(path);
			int lineNum = 0;
			ReadOnlySpan<char> line = null;
			while (sr.ReadLine(ref line))
			{
				lineNum++;
				if (!hasHeader)
				{
					header = LoadFileHeader(line);
					hasHeader = header != null;
				}
				else
				{
					LoadEntry(header, line, lineNum, path);
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning($"GameStringTable.LoadFile() - Failed to read \"{path}\".\n\nException: {ex.Message}");
			return false;
		}
		if (!hasHeader)
		{
			Debug.LogWarning($"GameStringTable.LoadFile() - \"{path}\" had a malformed header.");
			return false;
		}
		return true;
	}

	private static string GetFilePathWithLoadOrder(Global.GameStringCategory cat, bool native, FilePathFromCategoryCallback pathCallback)
	{
		Locale locale = Localization.GetActualLocale();
		string path = pathCallback(cat, locale, native);
		if (File.Exists(path))
		{
			return path;
		}
		Log.Downloader.PrintDebug("category {0}, native {1}, locale {2}, path {3}.", cat, native, Localization.GetLocaleName(), path);
		return null;
	}

	private static string GetFilePathFromCategory(Global.GameStringCategory cat, Locale locale, bool native)
	{
		string fileName = $"{cat}.txt";
		return GameStrings.GetAssetPath(locale, fileName, native);
	}

	private static string GetAudioFilePathFromCategory(Global.GameStringCategory cat, Locale locale, bool native)
	{
		string fileName = $"{cat}_AUDIO.txt";
		return GameStrings.GetAssetPath(locale, fileName, native);
	}

	private static Header LoadFileHeader(ReadOnlySpan<char> lineSpan)
	{
		if (lineSpan.Length == 0)
		{
			return null;
		}
		if (lineSpan[0] == '#')
		{
			return null;
		}
		Header header = new Header();
		ReadOnlySpan<char> span = lineSpan;
		Span<char> span2 = stackalloc char[1] { '\t' };
		ReadOnlySpanExtensions.SplitEnumerator fields = span.Split(span2);
		int index = 0;
		ReadOnlySpanExtensions.SplitEnumerator enumerator = fields.GetEnumerator();
		while (enumerator.MoveNext())
		{
			ReadOnlySpan<char> fieldEntry = enumerator.Current;
			if (MemoryExtensions.Equals(fieldEntry, "TAG".AsSpan(), StringComparison.InvariantCulture))
			{
				header.m_keyIndex = index;
				if (header.m_valueIndex >= 0)
				{
					break;
				}
			}
			else if (MemoryExtensions.Equals(fieldEntry, "TEXT".AsSpan(), StringComparison.InvariantCulture))
			{
				header.m_valueIndex = index;
				if (header.m_keyIndex >= 0)
				{
					break;
				}
			}
			index++;
		}
		if (header.m_keyIndex < 0 && header.m_valueIndex < 0)
		{
			return null;
		}
		return header;
	}

	private void LoadEntry(Header header, ReadOnlySpan<char> lineSpan, int lineNum, string path)
	{
		if (lineSpan.Length == 0 || lineSpan[0] == '#' || !lineSpan.HasNonSpaceCharacter())
		{
			return;
		}
		ReadOnlySpan<char> span = lineSpan;
		Span<char> span2 = stackalloc char[1] { '\t' };
		ReadOnlySpanExtensions.SplitEnumerator fields = span.Split(span2);
		string key = null;
		string value = null;
		int index = 0;
		bool keyFound = false;
		bool valueFound = false;
		ReadOnlySpanExtensions.SplitEnumerator enumerator = fields.GetEnumerator();
		while (enumerator.MoveNext())
		{
			ReadOnlySpan<char> field = enumerator.Current;
			if (index == header.m_keyIndex)
			{
				key = field.ToString();
				keyFound = true;
			}
			else if (index == header.m_valueIndex)
			{
				value = TextUtils.DecodeWhitespaces(field.Trim().ToString());
				valueFound = true;
			}
			if (keyFound && valueFound)
			{
				break;
			}
			index++;
		}
		if (string.IsNullOrEmpty(key))
		{
			if (!string.IsNullOrEmpty(value))
			{
				string output = lineSpan.ToString();
				Debug.LogErrorFormat("GameStringTable loading bad line #{0} of file {1} <{2}>", lineNum, path, output);
			}
		}
		else
		{
			value = value ?? string.Empty;
			if (keyFound)
			{
				m_table[key] = (valueFound ? value : string.Empty);
			}
		}
	}
}
