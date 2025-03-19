using System.Collections.Generic;

namespace Hearthstone.UI.Scripting;

public class ConditionalScriptSyntaxTreeRule : ScriptSyntaxTreeRule<ConditionalScriptSyntaxTreeRule>
{
	protected override HashSet<ScriptToken.TokenType> TokensInternal => new HashSet<ScriptToken.TokenType>
	{
		ScriptToken.TokenType.Or,
		ScriptToken.TokenType.And
	};

	protected override IEnumerable<ScriptSyntaxTreeRule> ExpectedRulesInternal => new ScriptSyntaxTreeRule[5]
	{
		ScriptSyntaxTreeRule<IdentifierScriptSyntaxTreeRule>.Get(),
		ScriptSyntaxTreeRule<NumberScriptSyntaxTreeRule>.Get(),
		ScriptSyntaxTreeRule<StringScriptSyntaxTreeRule>.Get(),
		ScriptSyntaxTreeRule<ExpressionGroupScriptSyntaxTreeRule>.Get(),
		ScriptSyntaxTreeRule<MethodScriptSyntaxTreeRule>.Get()
	};

	public override void Evaluate(ScriptSyntaxTreeNode node, ScriptContext.EvaluationContext evaluationContext, out object value)
	{
		value = null;
		if (!evaluationContext.CheckFeatureIsSupported(ScriptFeatureFlags.Conditionals))
		{
			return;
		}
		if (node.Left == null || !node.Left.Evaluate(evaluationContext, out var lValue))
		{
			evaluationContext.Results.SetFailedNodeIfNoneExists(node, node.Left);
			return;
		}
		_ = evaluationContext.EncodingPolicy;
		bool lBool = ((lValue is bool) ? ((bool)lValue) : (lValue != null));
		bool isShortCircuit = (node.Token.Type == ScriptToken.TokenType.And && !lBool) || (node.Token.Type == ScriptToken.TokenType.Or && lBool);
		object rValue;
		if (evaluationContext.ShouldGenerateFullSyntax || !isShortCircuit)
		{
			if (node.Right == null)
			{
				evaluationContext.EmitError(ScriptContext.ErrorCodes.EvaluationError, "");
				return;
			}
			if (!node.Right.Evaluate(evaluationContext, out rValue))
			{
				evaluationContext.Results.SetFailedNodeIfNoneExists(node, node.Right);
				rValue = false;
				if (evaluationContext.ShouldGenerateFullSyntax)
				{
					return;
				}
			}
		}
		else
		{
			rValue = false;
		}
		bool rBool = ((rValue is bool) ? ((bool)rValue) : (rValue != null));
		node.ValueType = typeof(bool);
		switch (node.Token.Type)
		{
		case ScriptToken.TokenType.And:
			value = (node.Value = rBool && lBool);
			break;
		case ScriptToken.TokenType.Or:
			value = (node.Value = rBool || lBool);
			break;
		default:
			value = (node.Value = false);
			break;
		}
	}
}
