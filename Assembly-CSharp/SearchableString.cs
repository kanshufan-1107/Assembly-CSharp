using System;
using System.Globalization;
using System.Text;
using Blizzard.T5.Core;

public class SearchableString
{
	private string m_rawString;

	private bool m_isTextInitialized;

	private string m_textNonEuropean;

	private bool m_isNonEuropeanTextInitialized;

	private string m_textNoDiacritics;

	private bool m_isNoDiacriticsInitialized;

	private string m_text;

	private static Map<char, string> s_europeanConversionTable = new Map<char, string>
	{
		{ 'œ', "oe" },
		{ 'æ', "ae" },
		{ '’', "'" },
		{ '«', "\"" },
		{ '»', "\"" },
		{ 'ä', "ae" },
		{ 'ü', "ue" },
		{ 'ö', "oe" },
		{ 'ß', "ss" }
	};

	private string Text
	{
		get
		{
			if (!m_isTextInitialized)
			{
				ReadOnlySpan<char> trimmed = UberText.RemoveMarkupAndCollapseWhitespaces(m_rawString).AsSpan().Trim();
				Span<char> lowerSpan = stackalloc char[trimmed.Length];
				trimmed.ToLower(lowerSpan, CultureInfo.CurrentCulture);
				m_text = lowerSpan.ToString();
				m_isTextInitialized = true;
			}
			return m_text;
		}
	}

	private string TextNonEuropean
	{
		get
		{
			if (!m_isNonEuropeanTextInitialized)
			{
				m_textNonEuropean = (HasEuropeanCharacters(m_rawString) ? ConvertEuropeanCharacters(m_rawString) : null);
				m_isNonEuropeanTextInitialized = true;
			}
			return m_textNonEuropean;
		}
	}

	private string TextNoDiacritics
	{
		get
		{
			if (!m_isNoDiacriticsInitialized)
			{
				m_textNoDiacritics = RemoveDiacritics(m_rawString);
				if (m_textNoDiacritics.Equals(m_rawString))
				{
					m_textNoDiacritics = null;
				}
				m_isNoDiacriticsInitialized = true;
			}
			return m_textNoDiacritics;
		}
	}

	public SearchableString(string text)
	{
		m_rawString = text;
	}

	public bool Search(string text)
	{
		if (Text.IndexOf(text) != -1)
		{
			return true;
		}
		if (TextNonEuropean != null && TextNonEuropean.Contains(text, StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		if (TextNoDiacritics != null && TextNoDiacritics.Contains(text, StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		return false;
	}

	public static bool SearchInternationalText(string textToSearchIn, string textToSearchFor)
	{
		string textToSearch = UberText.RemoveMarkupAndCollapseWhitespaces(textToSearchIn);
		ReadOnlySpan<char> trimmedText = textToSearch.AsSpan().Trim();
		Span<char> lowerText = stackalloc char[trimmedText.Length];
		trimmedText.ToLower(lowerText, CultureInfo.CurrentCulture);
		if (MemoryExtensions.Contains(lowerText, textToSearchFor.AsSpan(), StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		if (HasEuropeanCharacters(textToSearch) && ConvertEuropeanCharacters(textToSearch).Contains(textToSearchFor, StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		if (NeedsDiacriticsRemoved(textToSearch) && RemoveDiacritics(textToSearch).Contains(textToSearchFor, StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		return false;
	}

	public static bool HasEuropeanCharacters(string input)
	{
		int length = input.Length;
		for (int i = 0; i < length; i++)
		{
			if (s_europeanConversionTable.ContainsKey(input[i]))
			{
				return true;
			}
		}
		return false;
	}

	public static string ConvertEuropeanCharacters(string input)
	{
		int rawLength = input.Length;
		Span<char> convertedBuffer = stackalloc char[CalculateEuropeanConvertedStringLength(input.AsSpan())];
		int currentBufferIndex = 0;
		for (int i = 0; i < rawLength; i++)
		{
			if (s_europeanConversionTable.TryGetValue(input[i], out var newStr))
			{
				for (int j = 0; j < newStr.Length; j++)
				{
					convertedBuffer[currentBufferIndex++] = newStr[j];
				}
			}
			else
			{
				convertedBuffer[currentBufferIndex++] = input[i];
			}
		}
		return convertedBuffer.ToString();
	}

	private static int CalculateEuropeanConvertedStringLength(ReadOnlySpan<char> input)
	{
		int length = 0;
		for (int i = 0; i < input.Length; i++)
		{
			length = ((!s_europeanConversionTable.TryGetValue(input[i], out var newStr)) ? (length + 1) : (length + newStr.Length));
		}
		return length;
	}

	public static string RemoveDiacritics(string input)
	{
		string formDNormalized = (input.IsNormalized(NormalizationForm.FormD) ? input : input.Normalize(NormalizationForm.FormD));
		int formDNormalizedLength = formDNormalized.Length;
		bool needsRebuilding = false;
		for (int j = 0; j < formDNormalizedLength; j++)
		{
			if (CharUnicodeInfo.GetUnicodeCategory(formDNormalized[j]) == UnicodeCategory.NonSpacingMark)
			{
				needsRebuilding = true;
				break;
			}
		}
		if (needsRebuilding)
		{
			StringBuilder stringBuilder = new StringBuilder(formDNormalizedLength);
			for (int i = 0; i < formDNormalizedLength; i++)
			{
				if (CharUnicodeInfo.GetUnicodeCategory(formDNormalized[i]) != UnicodeCategory.NonSpacingMark)
				{
					stringBuilder.Append(formDNormalized[i]);
				}
			}
			string builtString = stringBuilder.ToString();
			if (!builtString.IsNormalized(NormalizationForm.FormC))
			{
				return builtString.Normalize(NormalizationForm.FormC);
			}
			return builtString;
		}
		if (!formDNormalized.IsNormalized(NormalizationForm.FormC))
		{
			return formDNormalized.Normalize(NormalizationForm.FormC);
		}
		return formDNormalized;
	}

	public static bool NeedsDiacriticsRemoved(string input)
	{
		if (!input.IsNormalized(NormalizationForm.FormD))
		{
			return true;
		}
		for (int i = 0; i < input.Length; i++)
		{
			if (CharUnicodeInfo.GetUnicodeCategory(input[i]) == UnicodeCategory.NonSpacingMark)
			{
				return true;
			}
		}
		return !input.IsNormalized(NormalizationForm.FormC);
	}
}
