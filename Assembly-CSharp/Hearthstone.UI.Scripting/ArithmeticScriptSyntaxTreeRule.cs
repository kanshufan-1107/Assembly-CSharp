using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Hearthstone.UI.Scripting;

public class ArithmeticScriptSyntaxTreeRule : ScriptSyntaxTreeRule<ArithmeticScriptSyntaxTreeRule>
{
	private struct EvalDoubleResult
	{
		public bool Success;

		public bool ValidResult;

		public EvalDoubleResult(bool success, bool validResult)
		{
			Success = success;
			ValidResult = validResult;
		}
	}

	protected override HashSet<ScriptToken.TokenType> TokensInternal => new HashSet<ScriptToken.TokenType>
	{
		ScriptToken.TokenType.Plus,
		ScriptToken.TokenType.Minus,
		ScriptToken.TokenType.Star,
		ScriptToken.TokenType.ForwardSlash,
		ScriptToken.TokenType.Percent,
		ScriptToken.TokenType.Caret
	};

	protected override IEnumerable<ScriptSyntaxTreeRule> ExpectedRulesInternal => new ScriptSyntaxTreeRule[4]
	{
		ScriptSyntaxTreeRule<IdentifierScriptSyntaxTreeRule>.Get(),
		ScriptSyntaxTreeRule<NumberScriptSyntaxTreeRule>.Get(),
		ScriptSyntaxTreeRule<ExpressionGroupScriptSyntaxTreeRule>.Get(),
		ScriptSyntaxTreeRule<MethodScriptSyntaxTreeRule>.Get()
	};

	public override void Evaluate(ScriptSyntaxTreeNode node, ScriptContext.EvaluationContext context, out object value)
	{
		value = null;
		if (!context.CheckFeatureIsSupported(ScriptFeatureFlags.Relational))
		{
			return;
		}
		double lValue;
		EvalDoubleResult lResult = EvaluateAsDouble(context, node.Left, out lValue);
		if (!lResult.Success)
		{
			context.Results.SetFailedNodeIfNoneExists(node, node.Left);
			return;
		}
		_ = context.EncodingPolicy;
		double rValue;
		EvalDoubleResult rResult = EvaluateAsDouble(context, node.Right, out rValue);
		if (!rResult.Success)
		{
			context.Results.SetFailedNodeIfNoneExists(node, node.Right);
			return;
		}
		node.ValueType = typeof(double);
		if (lResult.ValidResult && rResult.ValidResult)
		{
			switch (node.Token.Type)
			{
			case ScriptToken.TokenType.Plus:
				value = (node.Value = lValue + rValue);
				break;
			case ScriptToken.TokenType.Minus:
				value = (node.Value = lValue - rValue);
				break;
			case ScriptToken.TokenType.Star:
				value = (node.Value = lValue * rValue);
				break;
			case ScriptToken.TokenType.ForwardSlash:
				value = (node.Value = lValue / rValue);
				break;
			case ScriptToken.TokenType.Percent:
				value = (node.Value = lValue % rValue);
				break;
			case ScriptToken.TokenType.Caret:
				value = (node.Value = Math.Pow(lValue, rValue));
				break;
			default:
				value = (node.Value = 0);
				break;
			}
		}
	}

	private EvalDoubleResult EvaluateAsDouble(ScriptContext.EvaluationContext context, ScriptSyntaxTreeNode node, out double outValue)
	{
		outValue = 0.0;
		if (node == null)
		{
			context.EmitError(ScriptContext.ErrorCodes.EvaluationError, "Missing operand!");
			return new EvalDoubleResult(success: false, validResult: false);
		}
		if (!node.Evaluate(context, out var tempValue))
		{
			return new EvalDoubleResult(success: false, validResult: false);
		}
		if (tempValue == null)
		{
			return new EvalDoubleResult(success: true, validResult: false);
		}
		try
		{
			outValue = Convert.ToDouble(tempValue);
		}
		catch (InvalidCastException)
		{
			context.EmitError(ScriptContext.ErrorCodes.EvaluationError, "'{0}' is not a number", tempValue);
			return new EvalDoubleResult(success: false, validResult: false);
		}
		catch (Exception ex2)
		{
			context.EmitError(ScriptContext.ErrorCodes.EvaluationError, context.EditMode ? ex2.ToString() : null);
			return new EvalDoubleResult(success: false, validResult: false);
		}
		return new EvalDoubleResult(success: true, validResult: true);
	}

	[Conditional("UNITY_EDITOR")]
	private void EmitSuggestions(ScriptSyntaxTreeNode node, ScriptContext.EvaluationContext context, bool leftNodeFailed)
	{
		if (!context.SuggestionsEnabled)
		{
			return;
		}
		List<ScriptContext.SuggestionInfo> suggestions = new List<ScriptContext.SuggestionInfo>();
		if (leftNodeFailed)
		{
			_ = node.Left;
		}
		else if (node.Left != null && node.Left.ValueType != null && node.Left.ValueType.IsEnum)
		{
			string[] names = Enum.GetNames(node.Left.ValueType);
			foreach (string enumName in names)
			{
				suggestions.Add(new ScriptContext.SuggestionInfo
				{
					Identifier = enumName.ToLower(),
					CandidateType = ScriptContext.SuggestionInfo.Types.Property,
					ValueType = node.Left.ValueType
				});
			}
			suggestions.ForEach(delegate
			{
			});
		}
		else
		{
			_ = node.Right;
		}
	}
}
