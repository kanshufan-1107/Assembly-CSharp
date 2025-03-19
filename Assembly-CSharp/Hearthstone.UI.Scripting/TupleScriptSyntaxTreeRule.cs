using System.Collections;
using System.Collections.Generic;

namespace Hearthstone.UI.Scripting;

public class TupleScriptSyntaxTreeRule : ScriptSyntaxTreeRule<TupleScriptSyntaxTreeRule>
{
	protected override HashSet<ScriptToken.TokenType> TokensInternal => new HashSet<ScriptToken.TokenType> { ScriptToken.TokenType.Comma };

	protected override IEnumerable<ScriptSyntaxTreeRule> ExpectedRulesInternal => new ScriptSyntaxTreeRule[5]
	{
		ScriptSyntaxTreeRule<IdentifierScriptSyntaxTreeRule>.Get(),
		ScriptSyntaxTreeRule<NumberScriptSyntaxTreeRule>.Get(),
		ScriptSyntaxTreeRule<StringScriptSyntaxTreeRule>.Get(),
		ScriptSyntaxTreeRule<ExpressionGroupScriptSyntaxTreeRule>.Get(),
		ScriptSyntaxTreeRule<MethodScriptSyntaxTreeRule>.Get()
	};

	public override void Evaluate(ScriptSyntaxTreeNode node, ScriptContext.EvaluationContext context, out object outValue)
	{
		outValue = null;
		if (!context.CheckFeatureIsSupported(ScriptFeatureFlags.Tuples))
		{
			return;
		}
		if (node.Left == null || !node.Left.Evaluate(context, out var lValue) || (lValue == null && (node.Left.ValueType == null || node.Left.ValueType.IsValueType)))
		{
			context.EmitError(ScriptContext.ErrorCodes.EvaluationError, "Expected left value");
			context.Results.SetFailedNodeIfNoneExists(node, node.Left);
			return;
		}
		if (node.Right == null || !node.Right.Evaluate(context, out var rValue) || (rValue == null && (node.Right.ValueType == null || node.Left.ValueType.IsValueType)))
		{
			context.EmitError(ScriptContext.ErrorCodes.EvaluationError, "Expected right value");
			context.Results.SetFailedNodeIfNoneExists(node, node.Right);
			return;
		}
		ArrayList valueList = new ArrayList();
		ICollection lCollection = lValue as ICollection;
		ICollection rCollection = rValue as ICollection;
		if (lCollection != null)
		{
			foreach (object x in lCollection)
			{
				valueList.Add(x);
			}
		}
		else
		{
			valueList.Add(new DynamicValue(lValue, node.Left.ValueType));
		}
		if (rCollection != null)
		{
			foreach (object x2 in rCollection)
			{
				valueList.Add(x2);
			}
		}
		else
		{
			valueList.Add(new DynamicValue(rValue, node.Right.ValueType));
		}
		outValue = valueList;
	}
}
