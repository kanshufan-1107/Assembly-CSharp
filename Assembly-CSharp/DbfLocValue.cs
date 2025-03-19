using System;
using UnityEngine;

[Serializable]
public class DbfLocValue
{
	[SerializeField]
	private string[] m_locValues = new string[Localization.SupportedLocales.Count];

	[SerializeField]
	private int m_locId;

	private int m_recordId;

	private string m_recordColumn;

	private string m_currentLocaleValue = string.Empty;

	private bool m_stripped;

	private bool m_hideDebugInfo = true;

	public DbfLocValue()
	{
	}

	public DbfLocValue(bool hideDebugInfo)
	{
		m_hideDebugInfo = hideDebugInfo;
	}

	public string GetString()
	{
		return GetString(Localization.GetLocale());
	}

	public string GetString(Locale loc)
	{
		if (m_stripped)
		{
			return m_currentLocaleValue;
		}
		Locale mappedLocale = Localization.GetActualLocale(loc);
		int locIndex = Localization.SupportedLocales.IndexOf(mappedLocale);
		string ret = GetStringFromIndex(locIndex);
		if (!string.IsNullOrEmpty(ret))
		{
			return ret;
		}
		if (!m_hideDebugInfo)
		{
			return $"ID={m_recordId} COLUMN={m_recordColumn}";
		}
		return string.Empty;
	}

	private string GetStringFromIndex(int index)
	{
		if (0 <= index)
		{
			return m_locValues[index];
		}
		return string.Empty;
	}

	public void SetString(Locale loc, string value)
	{
		if (m_stripped)
		{
			Debug.LogError("DbfLocValue cannot set string as localization values have already been stripped.");
			return;
		}
		Locale mappedLocale = Localization.GetActualLocale(loc);
		int locIndex = Localization.SupportedLocales.IndexOf(mappedLocale);
		if (0 <= locIndex)
		{
			m_locValues[locIndex] = value;
		}
		else
		{
			Debug.LogWarning($"Locale {loc} is unsupported. Unable to set localization string {value}");
		}
	}

	public void SetString(string value)
	{
		SetString(Localization.GetLocale(), value);
	}

	public void SetLocId(int locId)
	{
		m_locId = locId;
	}

	public void SetDebugInfo(int recordId, string recordColumn)
	{
		m_recordId = recordId;
		m_recordColumn = recordColumn;
	}

	public static implicit operator string(DbfLocValue v)
	{
		if (v == null)
		{
			return string.Empty;
		}
		return v.GetString();
	}

	public void StripUnusedLocales()
	{
		if (!m_stripped)
		{
			Locale locale = Localization.GetActualLocale();
			int index = Localization.SupportedLocales.IndexOf(locale);
			if (0 <= index)
			{
				m_stripped = true;
				m_currentLocaleValue = m_locValues[index];
				m_locValues = null;
			}
		}
	}
}
