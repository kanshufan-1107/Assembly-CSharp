using System.Globalization;
using Blizzard.T5.Core;

public class ClipLengthEstimator
{
	public static readonly float MINIMUM_SAFE_DURATION = 0.5f;

	private static Map<Locale, float> perLocaleCharacterReadTime = new Map<Locale, float>
	{
		{
			Locale.enUS,
			0.075f
		},
		{
			Locale.koKR,
			0.375f
		},
		{
			Locale.zhTW,
			0.375f
		},
		{
			Locale.zhCN,
			0.375f
		}
	};

	public static float StringToReadTime(string input)
	{
		int lengthInTextElements = new StringInfo(input).LengthInTextElements;
		float characterReadTime = perLocaleCharacterReadTime[Locale.enUS];
		Locale currentLocale = Localization.GetLocale();
		if (perLocaleCharacterReadTime.ContainsKey(currentLocale))
		{
			characterReadTime = perLocaleCharacterReadTime[currentLocale];
		}
		float totalTime = (float)lengthInTextElements * characterReadTime;
		if (totalTime < MINIMUM_SAFE_DURATION)
		{
			return MINIMUM_SAFE_DURATION;
		}
		return totalTime;
	}
}
