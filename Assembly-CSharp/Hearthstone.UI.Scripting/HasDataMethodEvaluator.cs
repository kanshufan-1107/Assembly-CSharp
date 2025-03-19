using System;
using System.Collections;
using System.Collections.Generic;

namespace Hearthstone.UI.Scripting;

public class HasDataMethodEvaluator : MethodScriptSyntaxTreeRule.Evaluator<HasDataMethodEvaluator>
{
	public override Type ReturnType => typeof(bool);

	public override bool HandlesNullArgs => true;

	protected override string MethodSymbolInternal => "hasData";

	protected override Type[] ExpectedArgsInternal => new Type[1] { typeof(object) };

	public static bool HasData(object value)
	{
		if (value == null)
		{
			return false;
		}
		if (value.GetType().IsValueType)
		{
			return true;
		}
		if (value is string str)
		{
			return str.Length > 0;
		}
		if (value is ICollection collection)
		{
			return collection.Count > 0;
		}
		return true;
	}

	public override void Evaluate(List<object> args, ScriptSyntaxTreeNode node, ScriptContext.EvaluationContext context, out object outValue)
	{
		outValue = HasData(args[0]);
	}
}
