using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Hearthstone.UI.Scripting;

public class ScriptSyntaxTree
{
	public struct ParseResults
	{
		public enum ErrorCodes
		{
			Success,
			Error,
			UnexpectedToken,
			MissingToken
		}

		public ScriptToken[] Tokens;

		public ErrorCodes ErrorCode;

		public string ErrorMessage;

		public ScriptToken LastParsedToken;

		public ScriptToken FailedToken;

		public ScriptSyntaxTreeRule LastSuccessfulRule;

		public ScriptSyntaxTreeRule FailedRule;
	}

	private static Dictionary<string, ScriptSyntaxTree> s_syntaxTreeCache = new Dictionary<string, ScriptSyntaxTree>();

	private ParseResults m_results;

	public ScriptSyntaxTreeNode Root { get; private set; }

	public ParseResults Results => m_results;

	public static ScriptSyntaxTree Get(string script)
	{
		if (!s_syntaxTreeCache.TryGetValue(script, out var syntaxTree))
		{
			syntaxTree = new ScriptSyntaxTree();
			syntaxTree.Parse(script);
			s_syntaxTreeCache[script] = syntaxTree;
		}
		return syntaxTree;
	}

	public static void ClearSyntaxTreeCache()
	{
		s_syntaxTreeCache.Clear();
	}

	private ScriptSyntaxTree()
	{
	}

	private void Parse(string script)
	{
		m_results = default(ParseResults);
		m_results.ErrorCode = ParseResults.ErrorCodes.Success;
		m_results.Tokens = ScriptToken.Tokenize(script).ToArray();
		Root = new ScriptSyntaxTreeNode(ScriptSyntaxTreeRule<RootScriptSyntaxTreeRule>.Get())
		{
			Tokens = m_results.Tokens
		};
		ParseRecursive(m_results.Tokens, ScriptSyntaxTreeRule<RootScriptSyntaxTreeRule>.Get().ExpectedRules, Root);
	}

	private bool ParseRecursive(ScriptToken[] tokens, IEnumerable<ScriptSyntaxTreeRule> expectedRules, ScriptSyntaxTreeNode rootNode)
	{
		ScriptSyntaxTreeNode left = rootNode;
		int currentTokenIndex = 0;
		ScriptSyntaxTreeRule rule = null;
		while (currentTokenIndex < tokens.Length && expectedRules != null)
		{
			ScriptToken thisToken = tokens[currentTokenIndex];
			ScriptSyntaxTreeRule nextRule = null;
			foreach (ScriptSyntaxTreeRule expectedRule in expectedRules)
			{
				if (expectedRule.Tokens.Contains(thisToken.Type))
				{
					nextRule = expectedRule;
					break;
				}
			}
			if (nextRule == null)
			{
				m_results.ErrorCode = ParseResults.ErrorCodes.UnexpectedToken;
				m_results.FailedToken = thisToken;
				m_results.FailedRule = rule;
				m_results.ErrorMessage = FormatGenericErrorMessage(expectedRules);
				return false;
			}
			rule = nextRule;
			int firstTokenIndex = currentTokenIndex;
			int lastTokenIndex = firstTokenIndex;
			string ruleErrorMessage;
			ScriptSyntaxTreeNode newNode;
			ScriptSyntaxTreeRule.ParseResult num = rule.Parse(tokens, ref lastTokenIndex, out ruleErrorMessage, out newNode);
			currentTokenIndex = lastTokenIndex + 1;
			if (num == ScriptSyntaxTreeRule.ParseResult.Failed)
			{
				m_results.ErrorCode = ParseResults.ErrorCodes.Error;
				m_results.FailedToken = thisToken;
				m_results.FailedRule = rule;
				m_results.ErrorMessage = ruleErrorMessage;
				return false;
			}
			int num2 = currentTokenIndex - firstTokenIndex;
			bool isGroup = false;
			if (num2 > 0)
			{
				int nestedTokenEnd = currentTokenIndex;
				isGroup = rule.ParseStepInto(tokens, ref firstTokenIndex, ref nestedTokenEnd);
				newNode.Tokens = new ScriptToken[nestedTokenEnd - firstTokenIndex];
				for (int i = firstTokenIndex; i < nestedTokenEnd; i++)
				{
					newNode.Tokens[i - firstTokenIndex] = tokens[i];
				}
			}
			PushNodeToSyntaxTree(ref left, newNode, rootNode);
			m_results.LastParsedToken = thisToken;
			m_results.LastSuccessfulRule = rule;
			if (isGroup && !ParseRecursive(newNode.Tokens, nextRule.NestedRules, newNode))
			{
				return false;
			}
			expectedRules = nextRule.ExpectedRules;
		}
		return true;
	}

	private static string FormatGenericErrorMessage(IEnumerable<ScriptSyntaxTreeRule> expectedRules)
	{
		if (expectedRules == null)
		{
			return "";
		}
		StringBuilder strBuilder = new StringBuilder();
		strBuilder.Append("Expected tokens: ");
		foreach (ScriptSyntaxTreeRule expectedRule in expectedRules)
		{
			foreach (ScriptToken.TokenType token in expectedRule.Tokens)
			{
				strBuilder.Append(" '");
				strBuilder.Append(ScriptToken.TokenTypeToHumanReadableString(token));
				strBuilder.Append('\'');
			}
		}
		return strBuilder.ToString();
	}

	private void PrintTree(ScriptSyntaxTreeNode scriptSyntaxTreeNode, string depth)
	{
		if (scriptSyntaxTreeNode != null)
		{
			Debug.Log(depth + scriptSyntaxTreeNode.Token.Value);
			PrintTree(scriptSyntaxTreeNode.Left, depth + "   ");
			PrintTree(scriptSyntaxTreeNode.Right, depth + "   ");
		}
	}

	private void PushNodeToSyntaxTree(ref ScriptSyntaxTreeNode left, ScriptSyntaxTreeNode newNode, ScriptSyntaxTreeNode rootNode)
	{
		while (left != rootNode && newNode.Priority >= left.Priority)
		{
			left = left.Parent;
		}
		newNode.Parent = left;
		newNode.Left = ((left != null) ? left.Right : null);
		if (left != null)
		{
			if (left.Right != null)
			{
				left.Right.Parent = newNode;
			}
			left.Right = newNode;
		}
		left = newNode;
	}
}
