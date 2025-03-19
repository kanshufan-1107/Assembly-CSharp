using System;
using System.Collections.Generic;

namespace Hearthstone.UI.Scripting;

public class IsInStateMethodEvaluator : MethodScriptSyntaxTreeRule.Evaluator<IsInStateMethodEvaluator>
{
	private const string MethodSymbolName = "isInState";

	private static readonly Type s_returnType = typeof(string);

	public override Type ReturnType => s_returnType;

	protected override string MethodSymbolInternal => "isInState";

	protected override Type[] ExpectedArgsInternal => new Type[1] { typeof(string) };

	public override void Evaluate(List<object> args, ScriptSyntaxTreeNode node, ScriptContext.EvaluationContext context, out object outValue)
	{
		outValue = false;
		if (!(args[0] is string s) || string.IsNullOrEmpty(s))
		{
			if (!context.EditMode)
			{
				context.EmitError(ScriptContext.ErrorCodes.EvaluationError, "Missing state name!");
				context.Results.SetFailedNodeIfNoneExists(node, node);
			}
		}
		else if (context.StateCollection != null && context.StateCollection.ActiveState != null && context.StateCollection.ActiveState.Name == s)
		{
			outValue = true;
		}
	}

	protected override void EmitSuggestionsInternal(ScriptSyntaxTreeNode methodNode, ScriptContext.EvaluationContext context, int argIndex)
	{
		if (argIndex != 0 || context.StateCollection?.GetStateList() == null)
		{
			return;
		}
		foreach (IWidgetState state in context.StateCollection.GetStateList())
		{
			_ = state;
		}
	}
}
