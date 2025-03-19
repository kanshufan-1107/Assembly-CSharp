using System;
using System.Text;
using System.Text.RegularExpressions;

public static class TextUtils
{
	public class TransformCardTextParams
	{
		public int DamageBonus { get; set; }

		public int DamageBonusDouble { get; set; }

		public int HealingBonus { get; set; }

		public int HealingDouble { get; set; }

		public int AttackBonus { get; set; }

		public int ArmorBonus { get; set; }
	}

	private static StringBuilder s_textBuffer = new StringBuilder(128);

	public static string TryFormat(string format, params object[] args)
	{
		try
		{
			return string.Format(format, args);
		}
		catch (Exception exception)
		{
			int argCount = 0;
			if (args != null && args.Length != 0)
			{
				argCount = args.Length;
			}
			Log.TextUtils.PrintException(exception, $"String.Format Exception format:\"{format}\", argCount:{argCount}");
			return format;
		}
	}

	public static string DecodeWhitespaces(string text)
	{
		text = text.Replace("\\n", "\n");
		text = text.Replace("\\t", "\t");
		return text;
	}

	public static string TransformCardText(Entity entity, string text)
	{
		TransformCardTextParams parameters = new TransformCardTextParams
		{
			DamageBonus = entity.GetDamageBonus(),
			DamageBonusDouble = entity.GetDamageBonusDouble(),
			HealingBonus = entity.GetHealingBonus(),
			HealingDouble = entity.GetHealingDouble(),
			AttackBonus = entity.GetAttackBonus(),
			ArmorBonus = entity.GetArmorBonus()
		};
		if (entity.IsSpell() && entity.HasTag(GAME_TAG.SUPPRESS_SPELL_POWER_IN_TEXT))
		{
			parameters.DamageBonus = 0;
			parameters.DamageBonusDouble = 0;
		}
		return TransformCardText(text, parameters);
	}

	public static string TransformCardText(Entity entity, CardTextHistoryData historyData, string text)
	{
		TransformCardTextParams parameters = new TransformCardTextParams
		{
			DamageBonus = historyData.m_damageBonus,
			DamageBonusDouble = historyData.m_damageBonusDouble,
			HealingBonus = historyData.m_healingBonus,
			HealingDouble = historyData.m_healingDouble,
			AttackBonus = historyData.m_attackBonus,
			ArmorBonus = historyData.m_armorBonus
		};
		if (entity.IsSpell() && entity.HasTag(GAME_TAG.SUPPRESS_SPELL_POWER_IN_TEXT))
		{
			parameters.DamageBonus = 0;
			parameters.DamageBonusDouble = 0;
		}
		return TransformCardText(text, parameters);
	}

	public static string TransformCardText(string text, TransformCardTextParams parameters = null)
	{
		return GameStrings.ParseLanguageRules(TransformCardTextImpl(text, parameters));
	}

	public static string ToHexString(this byte[] bytes)
	{
		char[] c = new char[bytes.Length * 2];
		for (int i = 0; i < bytes.Length; i++)
		{
			int b = bytes[i] >> 4;
			c[i * 2] = (char)(55 + b + ((b - 10 >> 31) & -7));
			b = bytes[i] & 0xF;
			c[i * 2 + 1] = (char)(55 + b + ((b - 10 >> 31) & -7));
		}
		return new string(c);
	}

	public static string ToHexString(string str)
	{
		return Encoding.UTF8.GetBytes(str).ToHexString();
	}

	public static string FromHexString(string str)
	{
		if (str.Length % 2 == 1)
		{
			throw new Exception("Hex string must have an even number of digits");
		}
		byte[] bytearray = new byte[str.Length >> 1];
		for (int i = 0; i < str.Length >> 1; i++)
		{
			bytearray[i] = (byte)((GetHexValue(str[i << 1]) << 4) + GetHexValue(str[(i << 1) + 1]));
		}
		return Encoding.UTF8.GetString(bytearray);
	}

	private static int GetHexValue(char hex)
	{
		return hex - ((hex < ':') ? 48 : 55);
	}

	public static bool HasBonusDamage(string powersText)
	{
		return HasBonusToken(powersText, '$');
	}

	public static bool HasBonusHealing(string powersText)
	{
		return HasBonusToken(powersText, '#');
	}

	private static bool HasBonusToken(string powersText, char token)
	{
		if (powersText == null)
		{
			return false;
		}
		for (int i = 0; i < powersText.Length; i++)
		{
			if (powersText[i] != token)
			{
				continue;
			}
			int j;
			for (j = ++i; j < powersText.Length; j++)
			{
				char num = powersText[j];
				bool nextIsDigit = char.IsDigit(num);
				bool nextIsAt = num == '@';
				bool nextIsScriptDataNumIndicator = false;
				if (num == '{' && j + 1 < powersText.Length)
				{
					char nextNextChar = powersText[j + 1];
					if ((nextNextChar == '0' || nextNextChar == '1') && j + 2 < powersText.Length)
					{
						nextIsScriptDataNumIndicator = powersText[j + 2] == '}';
						if (nextIsScriptDataNumIndicator)
						{
							j += 2;
						}
					}
				}
				if (!nextIsDigit && !nextIsAt && !nextIsScriptDataNumIndicator)
				{
					break;
				}
			}
			if (j != i)
			{
				return true;
			}
		}
		return false;
	}

	public static string StripHTMLTags(string input)
	{
		if (string.IsNullOrEmpty(input))
		{
			return input;
		}
		return Regex.Replace(input, "<.*?>", string.Empty);
	}

	private static string TransformCardTextImpl(string powersText, TransformCardTextParams parameters = null)
	{
		int damageBonus = parameters?.DamageBonus ?? 0;
		int damageBonusDouble = parameters?.DamageBonusDouble ?? 0;
		int healingBonus = parameters?.HealingBonus ?? 0;
		int healingDouble = parameters?.HealingDouble ?? 0;
		int attackBonus = parameters?.AttackBonus ?? 0;
		int armorBonus = parameters?.ArmorBonus ?? 0;
		if (powersText == null)
		{
			return string.Empty;
		}
		if (powersText == string.Empty)
		{
			return string.Empty;
		}
		s_textBuffer.Clear();
		bool haveDamageBonus = damageBonus != 0 || damageBonusDouble > 0;
		bool haveHealingBonus = healingBonus != 0 || healingDouble > 0;
		for (int i = 0; i < powersText.Length; i++)
		{
			char curr = powersText[i];
			if (curr != '$' && curr != '#')
			{
				s_textBuffer.Append(curr);
				continue;
			}
			i++;
			if (i < powersText.Length)
			{
				char digitChar = powersText[i];
				if (digitChar == 'a' || digitChar == 'd')
				{
					curr = digitChar;
					i++;
				}
			}
			int j;
			for (j = i; j < powersText.Length; j++)
			{
				char digitChar2 = powersText[j];
				if (digitChar2 < '0' || digitChar2 > '9')
				{
					break;
				}
			}
			if (j == i)
			{
				continue;
			}
			int power = int.Parse(powersText.AsSpan(i, j - i));
			switch (curr)
			{
			case '$':
			{
				power += damageBonus;
				for (int l = 0; l < damageBonusDouble; l++)
				{
					power *= 2;
				}
				if (power < 0)
				{
					power = 0;
				}
				break;
			}
			case '#':
			{
				power += healingBonus;
				for (int k = 0; k < healingDouble; k++)
				{
					power *= 2;
				}
				break;
			}
			case 'a':
				power += attackBonus;
				if (power < 0)
				{
					power = 0;
				}
				break;
			case 'd':
				power += armorBonus;
				if (power < 0)
				{
					power = 0;
				}
				break;
			}
			if ((haveDamageBonus && curr == '$') || (haveHealingBonus && curr == '#'))
			{
				s_textBuffer.Append('*');
				s_textBuffer.Append(power);
				s_textBuffer.Append('*');
			}
			else
			{
				s_textBuffer.Append(power);
			}
			i = j - 1;
		}
		return s_textBuffer.ToString();
	}
}
