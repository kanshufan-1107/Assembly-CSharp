using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Hearthstone.UI.Scripting;

public class RelationalScriptSyntaxTreeRule : ScriptSyntaxTreeRule<RelationalScriptSyntaxTreeRule>
{
	protected override HashSet<ScriptToken.TokenType> TokensInternal => new HashSet<ScriptToken.TokenType>
	{
		ScriptToken.TokenType.Equal,
		ScriptToken.TokenType.NotEqual,
		ScriptToken.TokenType.Greater,
		ScriptToken.TokenType.GreaterEqual,
		ScriptToken.TokenType.Less,
		ScriptToken.TokenType.LessEqual
	};

	protected override IEnumerable<ScriptSyntaxTreeRule> ExpectedRulesInternal => new ScriptSyntaxTreeRule[5]
	{
		ScriptSyntaxTreeRule<IdentifierScriptSyntaxTreeRule>.Get(),
		ScriptSyntaxTreeRule<NumberScriptSyntaxTreeRule>.Get(),
		ScriptSyntaxTreeRule<ExpressionGroupScriptSyntaxTreeRule>.Get(),
		ScriptSyntaxTreeRule<StringScriptSyntaxTreeRule>.Get(),
		ScriptSyntaxTreeRule<MethodScriptSyntaxTreeRule>.Get()
	};

	public override void Evaluate(ScriptSyntaxTreeNode node, ScriptContext.EvaluationContext context, out object value)
	{
		value = null;
		if (!context.CheckFeatureIsSupported(ScriptFeatureFlags.Relational))
		{
			return;
		}
		if (node.Left == null || !node.Left.Evaluate(context, out var lValue))
		{
			context.Results.SetFailedNodeIfNoneExists(node, node.Left);
			return;
		}
		_ = context.EncodingPolicy;
		if (node.Right == null)
		{
			context.EmitError(ScriptContext.ErrorCodes.EvaluationError, "Expected right-hand operand");
			context.Results.SetFailedNodeIfNoneExists(node, node.Right);
			return;
		}
		if (!EvaluateRightHandValue(context, node, out var rValue))
		{
			context.Results.SetFailedNodeIfNoneExists(node, node.Right);
			return;
		}
		bool num = rValue is string;
		bool isLeftValueString = lValue is string;
		object castedRightValue = rValue;
		int comparisonResult = 0;
		if (num == isLeftValueString)
		{
			if (rValue is IConvertible rConvertible)
			{
				castedRightValue = ((lValue is int) ? ((object)rConvertible.ToInt32(null)) : ((lValue is long) ? ((object)rConvertible.ToInt64(null)) : ((lValue is double) ? ((object)rConvertible.ToDouble(null)) : ((lValue is float) ? ((object)rConvertible.ToSingle(null)) : rValue))));
			}
			Type valueType = node.Left.ValueType;
			Type rType = castedRightValue?.GetType() ?? node.Right.ValueType;
			bool num2 = valueType == rType;
			bool isLeftValueTypeDynamic = Utilities.IsDynamicType(node.Left.ValueType);
			bool isRightValueTypeDynamic = Utilities.IsDynamicType(node.Right.ValueType);
			bool skipDynamicValueTypeChecking = context.EditMode && (isLeftValueTypeDynamic || isRightValueTypeDynamic);
			if (!num2 && !skipDynamicValueTypeChecking && context.EditMode)
			{
				context.EmitError(ScriptContext.ErrorCodes.EvaluationError, "Comparison will fail because there is a type mismatch");
			}
			IComparable lComparable = lValue as IComparable;
			comparisonResult = ((num2 && lComparable != null && castedRightValue != null) ? lComparable.CompareTo(castedRightValue) : 0);
		}
		node.ValueType = typeof(bool);
		if (lValue == null || rValue == null)
		{
			node.Value = false;
			return;
		}
		switch (node.Token.Type)
		{
		case ScriptToken.TokenType.Equal:
			value = (node.Value = object.Equals(lValue, castedRightValue));
			break;
		case ScriptToken.TokenType.NotEqual:
			value = (node.Value = !object.Equals(lValue, castedRightValue));
			break;
		case ScriptToken.TokenType.Greater:
			value = (node.Value = comparisonResult > 0);
			break;
		case ScriptToken.TokenType.GreaterEqual:
			value = (node.Value = comparisonResult >= 0);
			break;
		case ScriptToken.TokenType.Less:
			value = (node.Value = comparisonResult < 0);
			break;
		case ScriptToken.TokenType.LessEqual:
			value = (node.Value = comparisonResult <= 0);
			break;
		default:
			value = (node.Value = false);
			break;
		}
	}

	private bool EvaluateRightHandValue(ScriptContext.EvaluationContext context, ScriptSyntaxTreeNode node, out object outValue)
	{
		outValue = null;
		if (node.Left.ValueType != null && node.Left.ValueType.IsEnum && node.Right.Rule == ScriptSyntaxTreeRule<IdentifierScriptSyntaxTreeRule>.Get())
		{
			string identifier = node.Right.Token.Value;
			Type enumType = node.Left.ValueType;
			try
			{
				outValue = Enum.Parse(enumType, identifier, ignoreCase: true);
				node.Right.Value = outValue;
				node.Right.ValueType = node.Left.ValueType;
				ScriptContext.EncodingPolicy encodingPolicy = context.EncodingPolicy;
				if (encodingPolicy != ScriptContext.EncodingPolicy.Numerical)
				{
					_ = 2;
				}
				return true;
			}
			catch (Exception)
			{
			}
		}
		if (Utilities.IsDynamicType(node.Left.ValueType))
		{
			bool isValidScipt = false;
			if (node.Right.Rule != ScriptSyntaxTreeRule<IdentifierScriptSyntaxTreeRule>.Get())
			{
				isValidScipt = node.Right.Evaluate(context, out outValue);
			}
			if (!isValidScipt && context.EncodingPolicy != 0)
			{
				context.Results.ErrorCode = ScriptContext.ErrorCodes.Success;
			}
			return true;
		}
		return node.Right.Evaluate(context, out outValue);
	}

	[Conditional("UNITY_EDITOR")]
	private void EmitSuggestions(ScriptSyntaxTreeNode node, ScriptContext.EvaluationContext context)
	{
		if (!context.SuggestionsEnabled)
		{
			return;
		}
		List<ScriptContext.SuggestionInfo> suggestions = new List<ScriptContext.SuggestionInfo>();
		if (node.Left != null && node.Left.ValueType != null && node.Left.ValueType.IsEnum)
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
		}
		else if (node.Right == null)
		{
			Utilities.CollectSuggestionsInGlobalNamespace(context, suggestions);
		}
		foreach (ScriptContext.SuggestionInfo item in suggestions)
		{
			_ = item;
		}
	}
}
