using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MiniJSON;
using UnityEngine;

public class MinSpecManager
{
	public enum MinSpecKind
	{
		RAM_SIZE,
		CPU_FREQ,
		OS_SPEC,
		OPENGL_SPEC,
		MAX_KIND_SIZE
	}

	private class RequirementSpec
	{
		public string m_keyName;

		public MinSpecKind m_kind;

		public float m_systemValue;

		public SortedDictionary<float, float> m_requirement;

		public RequirementSpec(string key, MinSpecKind kind, float defaultVal)
		{
			m_keyName = key;
			m_kind = kind;
			m_requirement = new SortedDictionary<float, float>();
			m_systemValue = ((defaultVal > 0f) ? defaultVal : GetSystemValue(kind));
		}
	}

	private RequirementSpec[] m_requirements;

	private static MinSpecManager s_instance;

	public static MinSpecManager Get()
	{
		if (s_instance == null)
		{
			s_instance = new MinSpecManager();
			s_instance.Initialize();
		}
		return s_instance;
	}

	public bool LoadJsonData(string json, string keyOS = "other")
	{
		if (string.IsNullOrEmpty(json))
		{
			return false;
		}
		bool success = false;
		try
		{
			if (Json.Deserialize(json) is JsonNode response && response.ContainsKey(keyOS))
			{
				JsonNode requires = response[keyOS] as JsonNode;
				RequirementSpec[] requirements = m_requirements;
				foreach (RequirementSpec req in requirements)
				{
					if (!requires.ContainsKey(req.m_keyName))
					{
						continue;
					}
					JsonNode j = requires[req.m_keyName] as JsonNode;
					foreach (string key in j.Keys)
					{
						if (float.TryParse(key, out var keyval))
						{
							req.m_requirement[keyval] = (float)Convert.ChangeType(j[key], typeof(float));
						}
					}
					success = true;
				}
			}
		}
		catch (Exception ex)
		{
			Log.MinSpecManager.PrintError("Failed to parse the minspec info: {0}\n'{1}'", ex.Message, json);
		}
		return success;
	}

	public List<MinSpecKind> GetNotEnoughSpecs(bool isChangedVersion, string LiveVersion)
	{
		float version = 32f;
		if (isChangedVersion)
		{
			if (UpdateUtils.GetSplitVersion(LiveVersion, out var versionInt))
			{
				version = (float)versionInt[0] + (float)versionInt[1] * 0.1f;
			}
			else
			{
				Log.MinSpecManager.PrintWarning("The live version string is wrong, using the binary version info instead: " + LiveVersion);
			}
		}
		List<MinSpecKind> warningList = new List<MinSpecKind>();
		RequirementSpec[] requirements = m_requirements;
		foreach (RequirementSpec req in requirements)
		{
			if (req.m_systemValue == 0f)
			{
				Log.MinSpecManager.PrintInfo("Skipped to check because there is no system value of {0}", req.m_kind.ToString());
			}
			else if (req.m_requirement.Count > 0)
			{
				float minSpecValue = GetRequrement(req.m_requirement, version);
				if (minSpecValue > req.m_systemValue)
				{
					Log.MinSpecManager.PrintInfo("Detected a Minspec warning - {0}: {1} > {2}", req.m_kind.ToString(), minSpecValue, req.m_systemValue);
					warningList.Add(req.m_kind);
				}
			}
		}
		return warningList;
	}

	protected void Initialize(float systemRam = 0f, float systemCPUFreq = 0f, float systemOSSpec = 0f, float OpenGLSpec = 0f)
	{
		m_requirements = new RequirementSpec[4]
		{
			new RequirementSpec("required_ram", MinSpecKind.RAM_SIZE, systemRam),
			new RequirementSpec("cpu_freq", MinSpecKind.CPU_FREQ, systemCPUFreq),
			new RequirementSpec("required_osspecs", MinSpecKind.OS_SPEC, systemOSSpec),
			new RequirementSpec("required_opengl", MinSpecKind.OPENGL_SPEC, OpenGLSpec)
		};
	}

	private float GetRequrement(SortedDictionary<float, float> data, float version)
	{
		float ret = data.First().Value;
		foreach (KeyValuePair<float, float> r in data)
		{
			if (r.Key <= version)
			{
				ret = r.Value;
			}
		}
		return ret;
	}

	private static float GetOpenGLVersion()
	{
		Match match = Regex.Match(SystemInfo.graphicsDeviceVersion, "OpenGL\\D*([\\d|\\.]*).*");
		if (match.Success && float.TryParse(match.Groups[1].Value, out var ret))
		{
			return ret;
		}
		return 0f;
	}

	private static float GetSystemValue(MinSpecKind kind)
	{
		float system_result = 0f;
		try
		{
			switch (kind)
			{
			case MinSpecKind.RAM_SIZE:
				system_result = MobileCallbackManager.GetSystemTotalMemoryMB();
				break;
			case MinSpecKind.CPU_FREQ:
				system_result = SystemInfo.processorFrequency;
				break;
			case MinSpecKind.OS_SPEC:
				system_result = MobileCallbackManager.GetSystemOSSpec();
				break;
			case MinSpecKind.OPENGL_SPEC:
				system_result = GetOpenGLVersion();
				break;
			}
		}
		catch (Exception ex)
		{
			Log.MinSpecManager.PrintError("Failed to set the system value of '{0}': {1}", kind.ToString(), ex.Message);
		}
		return system_result;
	}
}
