using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Blizzard.T5.Core;
using Blizzard.T5.Core.Utils;
using Hearthstone.Core.Streaming;
using Hearthstone.Util;
using UnityEngine;

public class LocalOptions
{
	private enum LoadResult
	{
		INVALID,
		SUCCESS,
		FAIL
	}

	private static LocalOptions s_instance;

	private string m_path;

	private LoadResult m_loadResult;

	private int m_currentLineVersion;

	private Map<string, object> m_options = new Map<string, object>();

	private List<string> m_sortedKeys = new List<string>();

	private List<string> m_temporaryKeys = new List<string>();

	private bool m_dirty;

	private Dictionary<string, Option> m_cachedOptionDescriptions;

	public static string OptionsPath
	{
		get
		{
			string optionsPath = $"{PlatformFilePaths.ExternalDataPath}/{PlatformFilePaths.GetOptionsFileName()}";
			if (!File.Exists(optionsPath))
			{
				optionsPath = $"{PlatformFilePaths.PersistentDataPath}/{PlatformFilePaths.GetOptionsFileName()}";
			}
			return optionsPath;
		}
	}

	public static LocalOptions Get()
	{
		if (s_instance == null)
		{
			s_instance = new LocalOptions();
		}
		return s_instance;
	}

	public void Initialize()
	{
		m_path = OptionsPath;
		m_currentLineVersion = 2;
		if (Load())
		{
			OptionsMigration.UpgradeClientOptions();
		}
		LaunchArguments.AddEnabledLogInOptions(null);
	}

	public void Clear()
	{
		m_dirty = false;
		m_options.Clear();
		m_sortedKeys.Clear();
	}

	public bool Has(string key)
	{
		return m_options.ContainsKey(key);
	}

	public void Delete(string key)
	{
		if (m_options.Remove(key))
		{
			m_sortedKeys.Remove(key);
			m_dirty = true;
			Save(key);
		}
	}

	public T Get<T>(string key)
	{
		if (!m_options.TryGetValue(key, out var val))
		{
			return default(T);
		}
		return (T)val;
	}

	public bool GetBool(string key)
	{
		return Get<bool>(key);
	}

	public int GetInt(string key)
	{
		return Get<int>(key);
	}

	public long GetLong(string key)
	{
		return Get<long>(key);
	}

	public ulong GetULong(string key)
	{
		return Get<ulong>(key);
	}

	public float GetFloat(string key)
	{
		return Get<float>(key);
	}

	public string GetString(string key)
	{
		return Get<string>(key);
	}

	public void Set(string key, object val)
	{
		Set(key, val, permanent: true);
	}

	public void Set(string key, object val, bool permanent)
	{
		if (m_options.TryGetValue(key, out var currVal))
		{
			if (currVal == val || (currVal != null && currVal.Equals(val)))
			{
				return;
			}
		}
		else
		{
			m_sortedKeys.Add(key);
			SortKeys();
		}
		m_options[key] = val;
		if (permanent)
		{
			m_temporaryKeys.Remove(key);
			m_dirty = true;
			Save(key);
		}
		else
		{
			m_temporaryKeys.Add(key);
		}
	}

	public void SetByLine(string line, bool permanent)
	{
		if (!LoadLine(line, out var _, out var key, out var val, out var _))
		{
			Log.ConfigFile.PrintError("LoadLine failed with '{0}'", line);
		}
		else
		{
			Set(key, val, permanent);
		}
	}

	private bool Load()
	{
		Clear();
		Log.ConfigFile.Print("Loading Options File: {0}", m_path);
		if (!File.Exists(m_path))
		{
			m_loadResult = LoadResult.SUCCESS;
			return true;
		}
		if (!LoadFile(out var lines))
		{
			m_loadResult = LoadResult.FAIL;
			return false;
		}
		bool formatChanged = false;
		if (!LoadAllLines(lines, out formatChanged))
		{
			m_loadResult = LoadResult.FAIL;
			return false;
		}
		foreach (string key in m_options.Keys)
		{
			m_sortedKeys.Add(key);
		}
		SortKeys();
		m_loadResult = LoadResult.SUCCESS;
		if (formatChanged)
		{
			m_dirty = true;
			Save();
		}
		return true;
	}

	private bool LoadFile(out string[] lines)
	{
		try
		{
			lines = File.ReadAllLines(m_path);
			return true;
		}
		catch (Exception ex)
		{
			Debug.LogError($"LocalOptions.LoadFile() - Failed to read {m_path}. Exception={ex.Message}");
			lines = null;
			return false;
		}
	}

	private bool LoadAllLines(string[] lines, out bool formatChanged)
	{
		formatChanged = false;
		int failedLines = 0;
		for (int i = 0; i < lines.Length; i++)
		{
			string line = lines[i];
			line = line.Trim();
			if (line.Length == 0 || line.StartsWith("#"))
			{
				continue;
			}
			if (!LoadLine(line, out var version, out var key, out var val, out var lineFormatChanged))
			{
				Debug.LogError($"LocalOptions.LoadAllLines() - Failed to load line {i + 1}\n\"{line}\".");
				failedLines++;
				if (failedLines > 4)
				{
					m_loadResult = LoadResult.FAIL;
					return false;
				}
			}
			else
			{
				m_options[key] = val;
				formatChanged = formatChanged || version != m_currentLineVersion || lineFormatChanged;
			}
		}
		return true;
	}

	private bool LoadLine(string line, out int version, out string key, out object val, out bool formatChanged)
	{
		version = 0;
		key = null;
		val = null;
		formatChanged = false;
		int tempVersion = 0;
		string tempKey = null;
		string valStr = null;
		bool parseFailed = false;
		string separator = "=";
		line = line.Trim();
		string[] lineTokens = line.Split(separator[0]);
		if (lineTokens.Length >= 2)
		{
			tempKey = lineTokens[0].Trim();
			valStr = ((lineTokens.Length != 2) ? string.Join(separator, lineTokens.Slice(1)).Trim() : lineTokens[1].Trim());
			if (string.IsNullOrEmpty(tempKey) || string.IsNullOrEmpty(valStr))
			{
				parseFailed = true;
			}
			tempVersion = 2;
		}
		else
		{
			parseFailed = true;
		}
		if (parseFailed)
		{
			return false;
		}
		if (m_cachedOptionDescriptions == null)
		{
			Array enumValues = Enum.GetValues(typeof(Option));
			m_cachedOptionDescriptions = new Dictionary<string, Option>(enumValues.Length);
			foreach (Option enumVal in enumValues)
			{
				string enumDesc = EnumUtils.GetString(enumVal);
				m_cachedOptionDescriptions.Add(enumDesc, enumVal);
			}
		}
		Option option = Option.INVALID;
		if (!m_cachedOptionDescriptions.TryGetValue(tempKey, out option))
		{
			version = tempVersion;
			key = tempKey;
			val = valStr;
			return true;
		}
		bool tempFormatChanged = false;
		if (option == Option.LOCALE && GeneralUtils.TryParseInt(valStr, out var valInt) && EnumUtils.TryCast<Locale>(valInt, out var locale))
		{
			valStr = locale.ToString();
			tempFormatChanged = true;
		}
		Type type = OptionDataTables.s_typeMap[option];
		if (type == typeof(bool))
		{
			val = GeneralUtils.ForceBool(valStr);
		}
		else if (type == typeof(int))
		{
			val = GeneralUtils.ForceInt(valStr);
		}
		else if (type == typeof(long))
		{
			val = GeneralUtils.ForceLong(valStr);
		}
		else if (type == typeof(ulong))
		{
			val = GeneralUtils.ForceULong(valStr);
		}
		else if (type == typeof(float))
		{
			val = GeneralUtils.ForceFloat(valStr);
		}
		else
		{
			if (!(type == typeof(string)))
			{
				return false;
			}
			val = valStr;
		}
		version = tempVersion;
		key = tempKey;
		formatChanged = tempFormatChanged;
		return true;
	}

	private bool Save(string triggerKey)
	{
		LoadResult loadResult = m_loadResult;
		if (loadResult == LoadResult.INVALID || loadResult == LoadResult.FAIL)
		{
			return false;
		}
		return Save();
	}

	private bool Save()
	{
		if (!m_dirty)
		{
			return true;
		}
		List<string> lines = new List<string>();
		for (int i = 0; i < m_sortedKeys.Count; i++)
		{
			string key = m_sortedKeys[i];
			if (!m_temporaryKeys.Contains(key))
			{
				object val = m_options[key];
				string line = $"{key}={val}";
				lines.Add(line);
			}
		}
		bool ret = WriteOptionsFile($"{PlatformFilePaths.ExternalDataPath}/{PlatformFilePaths.GetOptionsFileName()}", lines);
		if (!ret)
		{
			ret = WriteOptionsFile($"{PlatformFilePaths.PersistentDataPath}/{PlatformFilePaths.GetOptionsFileName()}", lines);
		}
		return ret;
	}

	private bool WriteOptionsFile(string optionsFilePath, List<string> lines)
	{
		try
		{
			File.WriteAllLines(optionsFilePath, lines.ToArray(), new UTF8Encoding());
		}
		catch (Exception ex)
		{
			Debug.LogError($"LocalOptions.Save() - Failed to save {optionsFilePath}. Exception={ex.Message}");
			return false;
		}
		m_dirty = false;
		return true;
	}

	private void SortKeys()
	{
		m_sortedKeys.Sort(KeyComparison);
	}

	private int KeyComparison(string key1, string key2)
	{
		return string.Compare(key1, key2, ignoreCase: true);
	}
}
