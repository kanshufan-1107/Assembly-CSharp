using System;
using System.Collections.Generic;
using UnityEngine;

namespace Hearthstone.UI.Scripting;

public class RandomIntegerMethodEvaluator : MethodScriptSyntaxTreeRule.Evaluator<RandomIntegerMethodEvaluator>
{
	private const string MethodSymbolName = "randInt";

	private static readonly Type s_returnType = typeof(int);

	public override Type ReturnType => s_returnType;

	protected override string MethodSymbolInternal => "randInt";

	protected override Type[] ExpectedArgsInternal => new Type[2]
	{
		typeof(int),
		typeof(int)
	};

	public override void Evaluate(List<object> args, ScriptSyntaxTreeNode node, ScriptContext.EvaluationContext context, out object outValue)
	{
		outValue = 0;
		if (!(args[0] is int) || !(args[1] is int))
		{
			context.EmitError(ScriptContext.ErrorCodes.EvaluationError, "Expected int arguments, but got something else!");
			context.Results.SetFailedNodeIfNoneExists(node, node);
		}
		else
		{
			int min = (int)args[0];
			int max = (int)args[1];
			outValue = UnityEngine.Random.Range(min, max + 1);
		}
	}
}
