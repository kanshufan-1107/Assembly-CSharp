using System;
using System.Collections.Generic;

namespace Hearthstone.UI.Scripting;

public struct ScriptToken
{
	public enum TokenType
	{
		Invalid,
		WhiteSpace,
		Literal,
		Numerical,
		Period,
		Has,
		Equal,
		NotEqual,
		Less,
		LessEqual,
		Greater,
		GreaterEqual,
		Plus,
		Minus,
		Star,
		ForwardSlash,
		Percent,
		Caret,
		Or,
		And,
		Colon,
		OpenRoundBrackets,
		ClosedRoundBrackets,
		OpenSquareBrackets,
		ClosedSquareBrackets,
		DoubleQuote,
		Comma,
		Method
	}

	public TokenType Type;

	public int StartIndex;

	public int EndIndex;

	public int Index;

	public string Value { get; private set; }

	public static List<ScriptToken> Tokenize(string str)
	{
		List<ScriptToken> tokens = new List<ScriptToken>();
		bool inString = false;
		for (int index = 0; index < str.Length; index++)
		{
			char c = str[index];
			if (IsWhiteSpace(str, index))
			{
				ScriptToken whiteSpace = CreateToken(TokenType.WhiteSpace, str, ref index, IsWhiteSpace);
				if (inString)
				{
					tokens.Add(whiteSpace);
				}
				continue;
			}
			if (inString && PeekNext(str, index, "\\") && index + 1 < str.Length)
			{
				tokens.Add(CreateToken(str, TokenType.Literal, ref index, 2));
				continue;
			}
			if (PeekNext(str, index, "has"))
			{
				tokens.Add(CreateToken(str, TokenType.Has, ref index, 3));
				continue;
			}
			if (PeekNext(str, index, "and"))
			{
				tokens.Add(CreateToken(str, TokenType.And, ref index, 3));
				continue;
			}
			if (PeekNext(str, index, "or"))
			{
				tokens.Add(CreateToken(str, TokenType.Or, ref index, 2));
				continue;
			}
			if (PeekNext(str, index, "not"))
			{
				tokens.Add(CreateToken(str, TokenType.NotEqual, ref index, 3));
				continue;
			}
			if (PeekNext(str, index, "<="))
			{
				tokens.Add(CreateToken(str, TokenType.LessEqual, ref index, 2));
				continue;
			}
			if (PeekNext(str, index, ">="))
			{
				tokens.Add(CreateToken(str, TokenType.GreaterEqual, ref index, 2));
				continue;
			}
			if (PeekNext(str, index, "is"))
			{
				tokens.Add(CreateToken(str, TokenType.Equal, ref index, 2));
				continue;
			}
			switch (c)
			{
			case '>':
				tokens.Add(CreateToken(str, TokenType.Greater, ref index, 1));
				continue;
			case '<':
				tokens.Add(CreateToken(str, TokenType.Less, ref index, 1));
				continue;
			case '+':
				tokens.Add(CreateToken(str, TokenType.Plus, ref index, 1));
				continue;
			case '*':
				tokens.Add(CreateToken(str, TokenType.Star, ref index, 1));
				continue;
			case '/':
				tokens.Add(CreateToken(str, TokenType.ForwardSlash, ref index, 1));
				continue;
			case '%':
				tokens.Add(CreateToken(str, TokenType.Percent, ref index, 1));
				continue;
			case '^':
				tokens.Add(CreateToken(str, TokenType.Caret, ref index, 1));
				continue;
			}
			if (IsMethodSignature(str, index, out var methodSymbol) && TryCreateMethodToken(methodSymbol, ref index, out var token))
			{
				tokens.Add(token);
				continue;
			}
			if (c == '$' || char.IsLetter(c) || c == '_')
			{
				tokens.Add(CreateToken(TokenType.Literal, str, ref index, IsLiteral));
				continue;
			}
			if (char.IsDigit(c) || (c == '-' && char.IsDigit(PeekNext(str, index))))
			{
				tokens.Add(CreateToken(TokenType.Numerical, str, ref index, IsDigit));
				continue;
			}
			switch (c)
			{
			case '-':
				tokens.Add(CreateToken(str, TokenType.Minus, ref index, 1));
				break;
			case '.':
				tokens.Add(CreateToken(str, TokenType.Period, ref index, 1));
				break;
			case ':':
				tokens.Add(CreateToken(str, TokenType.Colon, ref index, 1));
				break;
			case ',':
				tokens.Add(CreateToken(str, TokenType.Comma, ref index, 1));
				break;
			case '(':
				tokens.Add(CreateToken(str, TokenType.OpenRoundBrackets, ref index, 1));
				break;
			case ')':
				tokens.Add(CreateToken(str, TokenType.ClosedRoundBrackets, ref index, 1));
				break;
			case '[':
				tokens.Add(CreateToken(str, TokenType.OpenSquareBrackets, ref index, 1));
				break;
			case ']':
				tokens.Add(CreateToken(str, TokenType.ClosedSquareBrackets, ref index, 1));
				break;
			case '"':
				inString = !inString;
				tokens.Add(CreateToken(str, TokenType.DoubleQuote, ref index, 1));
				break;
			default:
				tokens.Add(CreateToken(str, TokenType.Invalid, ref index, 1));
				break;
			}
		}
		for (int i = 0; i < tokens.Count; i++)
		{
			ScriptToken t = tokens[i];
			t.Index = i;
			tokens[i] = t;
		}
		return tokens;
	}

	private static ScriptToken CreateToken(TokenType tokenType, string str, ref int index, Func<string, int, bool> predicate)
	{
		ScriptToken scriptToken = default(ScriptToken);
		scriptToken.Type = tokenType;
		scriptToken.StartIndex = index;
		int i;
		for (i = index + 1; i < str.Length && predicate(str, i); i++)
		{
		}
		scriptToken.EndIndex = i;
		scriptToken.Value = str.Substring(scriptToken.StartIndex, scriptToken.EndIndex - scriptToken.StartIndex);
		index = i - 1;
		return scriptToken;
	}

	private static ScriptToken CreateToken(string str, TokenType tokenType, ref int index, int size)
	{
		ScriptToken scriptToken = default(ScriptToken);
		scriptToken.Type = tokenType;
		scriptToken.StartIndex = index;
		scriptToken.EndIndex = index + size;
		scriptToken.Value = str.Substring(index, size);
		ScriptToken result = scriptToken;
		index += size - 1;
		return result;
	}

	private static bool TryCreateMethodToken(string methodSymbol, ref int index, out ScriptToken token)
	{
		token = default(ScriptToken);
		if (MethodScriptSyntaxTreeRule.TryGetMethodEvaluator(methodSymbol, out var _))
		{
			token.Type = TokenType.Method;
			token.StartIndex = index;
			token.EndIndex = index + methodSymbol.Length;
			token.Value = methodSymbol;
			index += methodSymbol.Length - 1;
			return true;
		}
		return false;
	}

	private static bool IsLiteral(string str, int index)
	{
		char c = str[index];
		if (!char.IsLetterOrDigit(c))
		{
			return c == '_';
		}
		return true;
	}

	private static bool IsDigit(string str, int index)
	{
		char c = str[index];
		if (!char.IsDigit(c))
		{
			if (c == '.')
			{
				return char.IsDigit(PeekNext(str, index));
			}
			return false;
		}
		return true;
	}

	private static bool IsWhiteSpace(string str, int index)
	{
		return char.IsWhiteSpace(str[index]);
	}

	private static bool IsMethodSignature(string str, int index, out string methodSymbol)
	{
		methodSymbol = null;
		if (!char.IsLetter(str[index]))
		{
			return false;
		}
		bool openingBraceFound = false;
		int i = index + 1;
		int length = 1;
		while (i < str.Length)
		{
			if (!char.IsLetterOrDigit(str[i]))
			{
				if (str[i] == '(')
				{
					openingBraceFound = true;
				}
				break;
			}
			i++;
			length++;
		}
		if (!openingBraceFound)
		{
			return false;
		}
		methodSymbol = str.Substring(index, length);
		return true;
	}

	private static bool PeekNext(string str, int index, string search)
	{
		int j = 0;
		int i = index;
		while (i < str.Length && j < search.Length)
		{
			if (str[i] != search[j])
			{
				return false;
			}
			i++;
			j++;
		}
		bool isMatch = j == search.Length;
		if (i == str.Length)
		{
			return isMatch;
		}
		if (char.IsLetterOrDigit(search[search.Length - 1]))
		{
			if (isMatch)
			{
				return char.IsWhiteSpace(str[i]);
			}
			return false;
		}
		return isMatch;
	}

	private static char PeekNext(string str, int index)
	{
		if (index >= str.Length - 1)
		{
			return '\0';
		}
		return str[index + 1];
	}

	public static string TokenTypeToHumanReadableString(TokenType tokenType)
	{
		return tokenType switch
		{
			TokenType.WhiteSpace => "<whitespace>", 
			TokenType.Literal => "<name>", 
			TokenType.Numerical => "<number>", 
			TokenType.Equal => "is", 
			TokenType.NotEqual => "not", 
			TokenType.Less => "<", 
			TokenType.LessEqual => "<=", 
			TokenType.Greater => ">", 
			TokenType.GreaterEqual => ">=", 
			TokenType.Or => "or", 
			TokenType.And => "and", 
			TokenType.Period => ".", 
			TokenType.Colon => ":", 
			TokenType.OpenRoundBrackets => "(", 
			TokenType.ClosedRoundBrackets => ")", 
			TokenType.OpenSquareBrackets => "[", 
			TokenType.ClosedSquareBrackets => "]", 
			TokenType.DoubleQuote => "\"", 
			TokenType.Comma => ",", 
			TokenType.Has => "has", 
			TokenType.Method => "<method>", 
			_ => "unknown", 
		};
	}
}
