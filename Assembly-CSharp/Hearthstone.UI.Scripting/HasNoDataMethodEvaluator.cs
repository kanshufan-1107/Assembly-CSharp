using System;
using System.Collections.Generic;

namespace Hearthstone.UI.Scripting;

public class HasNoDataMethodEvaluator : MethodScriptSyntaxTreeRule.Evaluator<HasNoDataMethodEvaluator>
{
	public override Type ReturnType => typeof(bool);

	public override bool HandlesNullArgs => true;

	protected override string MethodSymbolInternal => "hasNoData";

	protected override Type[] ExpectedArgsInternal => new Type[1] { typeof(object) };

	public override void Evaluate(List<object> args, ScriptSyntaxTreeNode node, ScriptContext.EvaluationContext context, out object outValue)
	{
		outValue = !HasDataMethodEvaluator.HasData(args[0]);
	}
}
