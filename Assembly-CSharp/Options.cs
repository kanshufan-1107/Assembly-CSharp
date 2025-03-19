using System;
using System.Collections.Generic;
using Blizzard.T5.Core;
using Blizzard.T5.Core.Utils;
using PegasusShared;
using UnityEngine;

public sealed class Options : IOptions
{
	public delegate void ChangedCallback(Option option, object prevValue, bool existed, object userData);

	private class ChangedListener : EventListener<ChangedCallback>
	{
		public void Fire(Option option, object prevValue, bool didExist)
		{
			m_callback(option, prevValue, didExist, m_userData);
		}
	}

	private static readonly ServerOption[] s_serverFlagContainers = new ServerOption[10]
	{
		ServerOption.FLAGS1,
		ServerOption.FLAGS2,
		ServerOption.FLAGS3,
		ServerOption.FLAGS4,
		ServerOption.FLAGS5,
		ServerOption.FLAGS6,
		ServerOption.FLAGS7,
		ServerOption.FLAGS8,
		ServerOption.FLAGS9,
		ServerOption.FLAGS10
	};

	private static Options s_instance;

	private Map<Option, string> m_clientOptionMap;

	private Map<Option, ServerOption> m_serverOptionMap;

	private Map<Option, ServerOptionFlag> m_serverOptionFlagMap;

	private Map<Option, List<ChangedListener>> m_changedListeners = new Map<Option, List<ChangedListener>>();

	private List<ChangedListener> m_globalChangedListeners = new List<ChangedListener>();

	public static Options Get()
	{
		if (s_instance == null)
		{
			s_instance = new Options();
			s_instance.Initialize();
		}
		return s_instance;
	}

	public Map<Option, string> GetClientOptions()
	{
		return m_clientOptionMap;
	}

	public Type GetOptionType(Option option)
	{
		if (OptionDataTables.s_typeMap.TryGetValue(option, out var type))
		{
			return type;
		}
		if (m_serverOptionFlagMap.ContainsKey(option))
		{
			return typeof(bool);
		}
		return null;
	}

	public Type GetServerOptionType(ServerOption serverOption)
	{
		if (Array.Exists(s_serverFlagContainers, (ServerOption flagContainer) => flagContainer == serverOption))
		{
			return typeof(ulong);
		}
		foreach (KeyValuePair<Option, ServerOption> pair in m_serverOptionMap)
		{
			if (pair.Value == serverOption)
			{
				Option option = pair.Key;
				if (OptionDataTables.s_typeMap.TryGetValue(option, out var type))
				{
					return type;
				}
				break;
			}
		}
		return null;
	}

	public static FormatType GetFormatType()
	{
		FormatType formatType = Get().GetEnum<FormatType>(Option.FORMAT_TYPE);
		switch (formatType)
		{
		case FormatType.FT_CLASSIC:
			SetFormatType(FormatType.FT_STANDARD);
			return FormatType.FT_STANDARD;
		case FormatType.FT_TWIST:
			if (SceneMgr.Get().GetMode() == SceneMgr.Mode.COLLECTIONMANAGER && RankMgr.IsCurrentTwistSeasonUsingHeroicDecks())
			{
				return FormatType.FT_STANDARD;
			}
			break;
		}
		return formatType;
	}

	public static void SetFormatType(FormatType formatType)
	{
		switch (formatType)
		{
		case FormatType.FT_UNKNOWN:
			RankMgr.LogMessage("Options.SetFormatType()  format type somehow got passed in as FT_UNKOWN", "SetFormatType", "D:\\p4Workspace\\32.0.0\\Pegasus\\Client\\Assets\\Shared\\Scripts\\Game\\Options.cs", 175);
			break;
		case FormatType.FT_CLASSIC:
			Get().SetEnum(Option.FORMAT_TYPE, FormatType.FT_STANDARD);
			break;
		default:
			Get().SetEnum(Option.FORMAT_TYPE, formatType);
			break;
		}
	}

	public static bool GetInRankedPlayMode()
	{
		return Get().GetBool(Option.IN_RANKED_PLAY_MODE);
	}

	public static void SetInRankedPlayMode(bool inRankedPlayMode)
	{
		Get().SetBool(Option.IN_RANKED_PLAY_MODE, inRankedPlayMode);
	}

	public bool RegisterChangedListener(Option option, ChangedCallback callback)
	{
		return RegisterChangedListener(option, callback, null);
	}

	public bool RegisterChangedListener(Option option, ChangedCallback callback, object userData)
	{
		ChangedListener listener = new ChangedListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		if (!m_changedListeners.TryGetValue(option, out var listeners))
		{
			listeners = new List<ChangedListener>();
			m_changedListeners.Add(option, listeners);
		}
		else if (listeners.Contains(listener))
		{
			return false;
		}
		listeners.Add(listener);
		return true;
	}

	public bool UnregisterChangedListener(Option option, ChangedCallback callback)
	{
		return UnregisterChangedListener(option, callback, null);
	}

	public bool UnregisterChangedListener(Option option, ChangedCallback callback, object userData)
	{
		ChangedListener listener = new ChangedListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		if (!m_changedListeners.TryGetValue(option, out var listeners))
		{
			return false;
		}
		if (!listeners.Remove(listener))
		{
			return false;
		}
		if (listeners.Count == 0)
		{
			m_changedListeners.Remove(option);
		}
		return true;
	}

	public bool HasOption(Option option)
	{
		if (m_clientOptionMap.TryGetValue(option, out var optionName))
		{
			return LocalOptions.Get().Has(optionName);
		}
		if (m_serverOptionMap.TryGetValue(option, out var serverOption))
		{
			return NetCache.Get().ClientOptionExists(serverOption);
		}
		if (m_serverOptionFlagMap.TryGetValue(option, out var serverOptionFlag))
		{
			return HasServerOptionFlag(serverOptionFlag);
		}
		return false;
	}

	public void DeleteOption(Option option)
	{
		ServerOption serverOption;
		ServerOptionFlag serverOptionFlag;
		if (m_clientOptionMap.TryGetValue(option, out var optionName))
		{
			DeleteClientOption(option, optionName);
		}
		else if (m_serverOptionMap.TryGetValue(option, out serverOption))
		{
			DeleteServerOption(option, serverOption);
		}
		else if (m_serverOptionFlagMap.TryGetValue(option, out serverOptionFlag))
		{
			DeleteServerOptionFlag(option, serverOptionFlag);
		}
	}

	public void DeleteOption(string optionStr)
	{
		Option option = Option.INVALID;
		try
		{
			option = EnumUtils.GetEnum<Option>(optionStr, StringComparison.OrdinalIgnoreCase);
		}
		catch (ArgumentException)
		{
			Debug.LogErrorFormat("No matched option with '{0}'", optionStr);
			return;
		}
		DeleteOption(option);
	}

	public object GetOption(Option option)
	{
		if (GetOptionImpl(option, out var val))
		{
			return val;
		}
		if (OptionDataTables.s_defaultsMap.TryGetValue(option, out var defaultVal))
		{
			return defaultVal;
		}
		return null;
	}

	public object GetOption(Option option, object defaultVal)
	{
		if (GetOptionImpl(option, out var val))
		{
			return val;
		}
		return defaultVal;
	}

	public bool GetBool(Option option)
	{
		if (GetBoolImpl(option, out var val))
		{
			return val;
		}
		if (OptionDataTables.s_defaultsMap.TryGetValue(option, out var defaultVal))
		{
			return (bool)defaultVal;
		}
		return false;
	}

	public bool GetBool(Option option, bool defaultVal)
	{
		if (GetBoolImpl(option, out var val))
		{
			return val;
		}
		return defaultVal;
	}

	public int GetInt(Option option)
	{
		if (GetIntImpl(option, out var val))
		{
			return val;
		}
		if (OptionDataTables.s_defaultsMap.TryGetValue(option, out var defaultVal))
		{
			return (int)defaultVal;
		}
		return 0;
	}

	public int GetInt(Option option, int defaultVal)
	{
		if (GetIntImpl(option, out var val))
		{
			return val;
		}
		return defaultVal;
	}

	public long GetLong(Option option)
	{
		if (GetLongImpl(option, out var val))
		{
			return val;
		}
		if (OptionDataTables.s_defaultsMap.TryGetValue(option, out var defaultVal))
		{
			return (long)defaultVal;
		}
		return 0L;
	}

	public long GetLong(Option option, long defaultVal)
	{
		if (GetLongImpl(option, out var val))
		{
			return val;
		}
		return defaultVal;
	}

	public float GetFloat(Option option)
	{
		if (GetFloatImpl(option, out var val))
		{
			return val;
		}
		if (OptionDataTables.s_defaultsMap.TryGetValue(option, out var defaultVal))
		{
			return (float)defaultVal;
		}
		return 0f;
	}

	public float GetFloat(Option option, float defaultVal)
	{
		if (GetFloatImpl(option, out var val))
		{
			return val;
		}
		return defaultVal;
	}

	public ulong GetULong(Option option)
	{
		if (GetULongImpl(option, out var val))
		{
			return val;
		}
		if (OptionDataTables.s_defaultsMap.TryGetValue(option, out var defaultVal))
		{
			return (ulong)defaultVal;
		}
		return 0uL;
	}

	public ulong GetULong(Option option, ulong defaultVal)
	{
		if (GetULongImpl(option, out var val))
		{
			return val;
		}
		return defaultVal;
	}

	public string GetString(Option option)
	{
		if (GetStringImpl(option, out var val))
		{
			return val;
		}
		if (OptionDataTables.s_defaultsMap.TryGetValue(option, out var defaultVal))
		{
			return (string)defaultVal;
		}
		return "";
	}

	public string GetString(Option option, string defaultVal)
	{
		if (GetStringImpl(option, out var val))
		{
			return val;
		}
		return defaultVal;
	}

	public T GetEnum<T>(Option option)
	{
		if (GetEnumImpl<T>(option, out var val))
		{
			return val;
		}
		if (OptionDataTables.s_defaultsMap.TryGetValue(option, out var defaultVal) && TranslateEnumVal<T>(option, defaultVal, out val))
		{
			return val;
		}
		return default(T);
	}

	public T GetEnum<T>(Option option, T defaultVal)
	{
		if (GetEnumImpl<T>(option, out var val))
		{
			return val;
		}
		return defaultVal;
	}

	public void SetOption(Option option, object val)
	{
		Type type = GetOptionType(option);
		if (type == typeof(bool))
		{
			SetBool(option, (bool)val);
			return;
		}
		if (type == typeof(int))
		{
			SetInt(option, (int)val);
			return;
		}
		if (type == typeof(long))
		{
			SetLong(option, (long)val);
			return;
		}
		if (type == typeof(float))
		{
			SetFloat(option, (float)val);
			return;
		}
		if (type == typeof(string))
		{
			SetString(option, (string)val);
			return;
		}
		if (type == typeof(ulong))
		{
			SetULong(option, (ulong)val);
			return;
		}
		Error.AddDevFatal("Options.SetOption() - option {0} has unsupported underlying type {1}", option, type);
	}

	public void SetBool(Option option, bool val)
	{
		ServerOptionFlag serverOptionFlag;
		if (m_clientOptionMap.TryGetValue(option, out var optionName))
		{
			bool exists = LocalOptions.Get().Has(optionName);
			bool prevVal = LocalOptions.Get().GetBool(optionName);
			if (!exists || prevVal != val)
			{
				LocalOptions.Get().Set(optionName, val);
				FireChangedEvent(option, prevVal, exists);
			}
		}
		else if (m_serverOptionFlagMap.TryGetValue(option, out serverOptionFlag))
		{
			GetServerOptionFlagInfo(serverOptionFlag, out var serverOption, out var flagBit, out var existenceBit);
			ulong prevContainerVal = NetCache.Get().GetULongOption(serverOption);
			bool prevVal2 = (prevContainerVal & flagBit) != 0;
			bool exists2 = (prevContainerVal & existenceBit) != 0;
			if (!exists2 || prevVal2 != val)
			{
				ulong containerVal = (val ? (prevContainerVal | flagBit) : (prevContainerVal & ~flagBit));
				containerVal |= existenceBit;
				NetCache.Get().SetULongOption(serverOption, containerVal);
				FireChangedEvent(option, prevVal2, exists2);
			}
		}
	}

	public void SetInt(Option option, int val)
	{
		ServerOption serverOption;
		if (m_clientOptionMap.TryGetValue(option, out var optionName))
		{
			bool exists = LocalOptions.Get().Has(optionName);
			int prevVal = LocalOptions.Get().GetInt(optionName);
			if (!exists || prevVal != val)
			{
				LocalOptions.Get().Set(optionName, val);
				FireChangedEvent(option, prevVal, exists);
			}
		}
		else if (m_serverOptionMap.TryGetValue(option, out serverOption))
		{
			int prevVal2;
			bool exists2 = NetCache.Get().GetIntOption(serverOption, out prevVal2);
			if (!exists2 || prevVal2 != val)
			{
				NetCache.Get().SetIntOption(serverOption, val);
				FireChangedEvent(option, prevVal2, exists2);
			}
		}
	}

	public void SetLong(Option option, long val)
	{
		ServerOption serverOption;
		if (m_clientOptionMap.TryGetValue(option, out var optionName))
		{
			bool exists = LocalOptions.Get().Has(optionName);
			long prevVal = LocalOptions.Get().GetLong(optionName);
			if (!exists || prevVal != val)
			{
				LocalOptions.Get().Set(optionName, val);
				FireChangedEvent(option, prevVal, exists);
			}
		}
		else if (m_serverOptionMap.TryGetValue(option, out serverOption))
		{
			long prevVal2;
			bool exists2 = NetCache.Get().GetLongOption(serverOption, out prevVal2);
			if (!exists2 || prevVal2 != val)
			{
				NetCache.Get().SetLongOption(serverOption, val);
				FireChangedEvent(option, prevVal2, exists2);
			}
		}
	}

	public void SetFloat(Option option, float val)
	{
		ServerOption serverOption;
		if (m_clientOptionMap.TryGetValue(option, out var optionName))
		{
			bool exists = LocalOptions.Get().Has(optionName);
			float prevVal = LocalOptions.Get().GetFloat(optionName);
			if (!exists || prevVal != val)
			{
				LocalOptions.Get().Set(optionName, val);
				FireChangedEvent(option, prevVal, exists);
			}
		}
		else if (m_serverOptionMap.TryGetValue(option, out serverOption))
		{
			float prevVal2;
			bool exists2 = NetCache.Get().GetFloatOption(serverOption, out prevVal2);
			if (!exists2 || prevVal2 != val)
			{
				NetCache.Get().SetFloatOption(serverOption, val);
				FireChangedEvent(option, prevVal2, exists2);
			}
		}
	}

	public void SetULong(Option option, ulong val)
	{
		ServerOption serverOption;
		if (m_clientOptionMap.TryGetValue(option, out var optionName))
		{
			bool exists = LocalOptions.Get().Has(optionName);
			ulong prevVal = LocalOptions.Get().GetULong(optionName);
			if (!exists || prevVal != val)
			{
				LocalOptions.Get().Set(optionName, val);
				FireChangedEvent(option, prevVal, exists);
			}
		}
		else if (m_serverOptionMap.TryGetValue(option, out serverOption))
		{
			ulong prevVal2;
			bool exists2 = NetCache.Get().GetULongOption(serverOption, out prevVal2);
			if (!exists2 || prevVal2 != val)
			{
				NetCache.Get().SetULongOption(serverOption, val);
				FireChangedEvent(option, prevVal2, exists2);
			}
		}
	}

	public void SetString(Option option, string val)
	{
		if (m_clientOptionMap.TryGetValue(option, out var optionName))
		{
			bool exists = LocalOptions.Get().Has(optionName);
			string prevVal = LocalOptions.Get().GetString(optionName);
			if (!exists || prevVal != val)
			{
				LocalOptions.Get().Set(optionName, val);
				FireChangedEvent(option, prevVal, exists);
			}
		}
	}

	public void SetEnum<T>(Option option, T val)
	{
		if (!Enum.IsDefined(typeof(T), val))
		{
			Error.AddDevFatal("Options.SetEnum() - {0} is not convertible to enum type {1} for option {2}", val, typeof(T), option);
			return;
		}
		Type underlyingType = GetOptionType(option);
		if (underlyingType == typeof(int))
		{
			SetInt(option, Convert.ToInt32(val));
			return;
		}
		if (underlyingType == typeof(long))
		{
			SetLong(option, Convert.ToInt64(val));
			return;
		}
		Error.AddDevFatal("Options.SetEnum() - option {0} has unsupported underlying type {1}", option, underlyingType);
	}

	public void RevertFloatToDefault(Option option)
	{
		if (OptionDataTables.s_defaultsMap.TryGetValue(option, out var defaultFloatValue))
		{
			SetFloat(option, (float)defaultFloatValue);
		}
	}

	public void RevertBoolToDefault(Option option)
	{
		if (OptionDataTables.s_defaultsMap.TryGetValue(option, out var defaultBoolValue))
		{
			SetBool(option, (bool)defaultBoolValue);
		}
	}

	private void Initialize()
	{
		Array values = Enum.GetValues(typeof(Option));
		Map<string, Option> options = new Map<string, Option>();
		foreach (Option option in values)
		{
			if (option != 0)
			{
				string optionString = option.ToString();
				options.Add(optionString, option);
			}
		}
		BuildClientOptionMap(options);
		BuildServerOptionMap(options);
		BuildServerOptionFlagMap(options);
	}

	private void BuildClientOptionMap(Map<string, Option> options)
	{
		m_clientOptionMap = new Map<Option, string>();
		foreach (ClientOption clientOption in Enum.GetValues(typeof(ClientOption)))
		{
			if (clientOption != 0)
			{
				string optionString = clientOption.ToString();
				if (!options.TryGetValue(optionString, out var option))
				{
					Debug.LogError($"Options.BuildClientOptionMap() - ClientOption {clientOption} is not mirrored in the Option enum");
					continue;
				}
				if (!OptionDataTables.s_typeMap.TryGetValue(option, out var _))
				{
					Debug.LogError($"Options.BuildClientOptionMap() - ClientOption {clientOption} has no type. Please add its type to the type map.");
					continue;
				}
				string optionName = EnumUtils.GetString(option);
				m_clientOptionMap.Add(option, optionName);
			}
		}
	}

	private void BuildServerOptionMap(Map<string, Option> options)
	{
		m_serverOptionMap = new Map<Option, ServerOption>();
		foreach (ServerOption serverOption in Enum.GetValues(typeof(ServerOption)))
		{
			if (serverOption == ServerOption.INVALID || serverOption == ServerOption.LIMIT)
			{
				continue;
			}
			string optionString = serverOption.ToString();
			if (!optionString.StartsWith("FLAGS") && !optionString.StartsWith("DEPRECATED"))
			{
				Type optionType;
				if (!options.TryGetValue(optionString, out var option))
				{
					Debug.LogError($"Options.BuildServerOptionMap() - ServerOption {serverOption} is not mirrored in the Option enum");
				}
				else if (!OptionDataTables.s_typeMap.TryGetValue(option, out optionType))
				{
					Debug.LogError($"Options.BuildServerOptionMap() - ServerOption {serverOption} has no type. Please add its type to the type map.");
				}
				else if (optionType == typeof(bool))
				{
					Debug.LogError($"Options.BuildServerOptionMap() - ServerOption {serverOption} is a bool. You should convert it to a ServerOptionFlag.");
				}
				else
				{
					m_serverOptionMap.Add(option, serverOption);
				}
			}
		}
	}

	private void BuildServerOptionFlagMap(Map<string, Option> options)
	{
		m_serverOptionFlagMap = new Map<Option, ServerOptionFlag>();
		foreach (ServerOptionFlag serverOptionFlag in Enum.GetValues(typeof(ServerOptionFlag)))
		{
			if (serverOptionFlag == ServerOptionFlag.LIMIT)
			{
				continue;
			}
			string optionString = serverOptionFlag.ToString();
			if (!optionString.StartsWith("DEPRECATED"))
			{
				if (!options.TryGetValue(optionString, out var option))
				{
					Debug.LogError($"Options.BuildServerOptionFlagMap() - ServerOptionFlag {serverOptionFlag} is not mirrored in the Option enum");
				}
				else
				{
					m_serverOptionFlagMap.Add(option, serverOptionFlag);
				}
			}
		}
	}

	private void GetServerOptionFlagInfo(ServerOptionFlag flag, out ServerOption container, out ulong flagBit, out ulong existenceBit)
	{
		int num = 2 * (int)flag;
		int containerIndex = Mathf.FloorToInt((float)num * (1f / 64f));
		int flagIndex = num % 64;
		int existenceIndex = 1 + flagIndex;
		container = s_serverFlagContainers[containerIndex];
		flagBit = (ulong)(1L << flagIndex);
		existenceBit = (ulong)(1L << existenceIndex);
	}

	private bool HasServerOptionFlag(ServerOptionFlag serverOptionFlag)
	{
		GetServerOptionFlagInfo(serverOptionFlag, out var serverOption, out var _, out var existenceBit);
		return (NetCache.Get().GetULongOption(serverOption) & existenceBit) != 0;
	}

	private void DeleteClientOption(Option option, string optionName)
	{
		if (LocalOptions.Get().Has(optionName))
		{
			object prevVal = GetClientOption(option, optionName);
			LocalOptions.Get().Delete(optionName);
			RemoveListeners(option, prevVal);
		}
	}

	private void DeleteServerOption(Option option, ServerOption serverOption)
	{
		if (NetCache.Get().ClientOptionExists(serverOption))
		{
			object prevVal = GetServerOption(option, serverOption);
			NetCache.Get().DeleteClientOption(serverOption);
			RemoveListeners(option, prevVal);
		}
	}

	private void DeleteServerOptionFlag(Option option, ServerOptionFlag serverOptionFlag)
	{
		GetServerOptionFlagInfo(serverOptionFlag, out var serverOption, out var flagBit, out var existenceBit);
		ulong containerVal = NetCache.Get().GetULongOption(serverOption);
		if ((containerVal & existenceBit) != 0)
		{
			bool prevVal = (containerVal & flagBit) != 0;
			containerVal &= ~existenceBit;
			NetCache.Get().SetULongOption(serverOption, containerVal);
			RemoveListeners(option, prevVal);
		}
	}

	private object GetClientOption(Option option, string optionName)
	{
		Type type = GetOptionType(option);
		if (type == typeof(bool))
		{
			return LocalOptions.Get().GetBool(optionName);
		}
		if (type == typeof(int))
		{
			return LocalOptions.Get().GetInt(optionName);
		}
		if (type == typeof(long))
		{
			return LocalOptions.Get().GetLong(optionName);
		}
		if (type == typeof(ulong))
		{
			return LocalOptions.Get().GetULong(optionName);
		}
		if (type == typeof(float))
		{
			return LocalOptions.Get().GetFloat(optionName);
		}
		if (type == typeof(string))
		{
			return LocalOptions.Get().GetString(optionName);
		}
		Error.AddDevFatal("Options.GetClientOption() - option {0} has unsupported underlying type {1}", option, type);
		return null;
	}

	private object GetServerOption(Option option, ServerOption serverOption)
	{
		Type type = GetOptionType(option);
		if (type == typeof(int))
		{
			return NetCache.Get().GetIntOption(serverOption);
		}
		if (type == typeof(long))
		{
			return NetCache.Get().GetLongOption(serverOption);
		}
		if (type == typeof(float))
		{
			return NetCache.Get().GetFloatOption(serverOption);
		}
		if (type == typeof(ulong))
		{
			return NetCache.Get().GetULongOption(serverOption);
		}
		Error.AddDevFatal("Options.GetServerOption() - option {0} has unsupported underlying type {1}", option, type);
		return null;
	}

	private bool GetOptionImpl(Option option, out object val)
	{
		val = null;
		if (m_clientOptionMap.TryGetValue(option, out var optionName))
		{
			if (LocalOptions.Get().Has(optionName))
			{
				val = GetClientOption(option, optionName);
			}
		}
		else if (NetCache.Get() != null)
		{
			ServerOptionFlag serverOptionFlag;
			if (m_serverOptionMap.TryGetValue(option, out var serverOption))
			{
				if (NetCache.Get().ClientOptionExists(serverOption))
				{
					val = GetServerOption(option, serverOption);
				}
			}
			else if (m_serverOptionFlagMap.TryGetValue(option, out serverOptionFlag))
			{
				GetServerOptionFlagInfo(serverOptionFlag, out serverOption, out var flagBit, out var existenceBit);
				ulong containerVal = NetCache.Get().GetULongOption(serverOption);
				if ((containerVal & existenceBit) != 0)
				{
					val = (containerVal & flagBit) != 0;
				}
			}
		}
		return val != null;
	}

	private bool GetBoolImpl(Option option, out bool val)
	{
		val = false;
		if (GetOptionImpl(option, out var genericVal))
		{
			val = (bool)genericVal;
			return true;
		}
		return false;
	}

	private bool GetIntImpl(Option option, out int val)
	{
		val = 0;
		if (GetOptionImpl(option, out var genericVal))
		{
			val = (int)genericVal;
			return true;
		}
		return false;
	}

	private bool GetLongImpl(Option option, out long val)
	{
		val = 0L;
		if (GetOptionImpl(option, out var genericVal))
		{
			val = (long)genericVal;
			return true;
		}
		return false;
	}

	private bool GetFloatImpl(Option option, out float val)
	{
		val = 0f;
		if (GetOptionImpl(option, out var genericVal))
		{
			val = (float)genericVal;
			return true;
		}
		return false;
	}

	private bool GetULongImpl(Option option, out ulong val)
	{
		val = 0uL;
		if (GetOptionImpl(option, out var genericVal))
		{
			val = (ulong)genericVal;
			return true;
		}
		return false;
	}

	private bool GetStringImpl(Option option, out string val)
	{
		val = "";
		if (GetOptionImpl(option, out var genericVal))
		{
			val = (string)genericVal;
			return true;
		}
		return false;
	}

	private bool GetEnumImpl<T>(Option option, out T val)
	{
		val = default(T);
		if (GetOptionImpl(option, out var genericVal))
		{
			return TranslateEnumVal<T>(option, genericVal, out val);
		}
		return false;
	}

	private bool TranslateEnumVal<T>(Option option, object genericVal, out T val)
	{
		val = default(T);
		if (genericVal == null)
		{
			return true;
		}
		Type sourceType = genericVal.GetType();
		Type targetType = typeof(T);
		try
		{
			if (sourceType == targetType)
			{
				val = (T)genericVal;
				return true;
			}
			object underlyingTargetTypeVal = Convert.ChangeType(genericVal, Enum.GetUnderlyingType(targetType));
			val = (T)underlyingTargetTypeVal;
			return true;
		}
		catch (Exception ex)
		{
			Debug.LogErrorFormat("Options.TranslateEnumVal() - option {0} has value {1} ({2}), which cannot be converted to type {3}: {4}", option, genericVal, sourceType, targetType, ex.ToString());
			return false;
		}
	}

	private void RemoveListeners(Option option, object prevVal)
	{
		FireChangedEvent(option, prevVal, existed: true);
		m_changedListeners.Remove(option);
	}

	private void FireChangedEvent(Option option, object prevVal, bool existed)
	{
		if (m_changedListeners.TryGetValue(option, out var singleListeners))
		{
			ChangedListener[] singleListenersCopy = singleListeners.ToArray();
			for (int i = 0; i < singleListenersCopy.Length; i++)
			{
				singleListenersCopy[i].Fire(option, prevVal, existed);
			}
		}
		ChangedListener[] globalListenersCopy = m_globalChangedListeners.ToArray();
		for (int j = 0; j < globalListenersCopy.Length; j++)
		{
			globalListenersCopy[j].Fire(option, prevVal, existed);
		}
	}
}
