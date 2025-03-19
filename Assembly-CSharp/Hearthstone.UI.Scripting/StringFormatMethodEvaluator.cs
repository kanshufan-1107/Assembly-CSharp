using System;
using System.Collections.Generic;

namespace Hearthstone.UI.Scripting;

public class StringFormatMethodEvaluator : MethodScriptSyntaxTreeRule.Evaluator<StringFormatMethodEvaluator>
{
	public override Type ReturnType => typeof(string);

	public override bool EndsWithVariableArguments => true;

	protected override string MethodSymbolInternal => "strformat";

	protected override Type[] ExpectedArgsInternal => new Type[2]
	{
		typeof(string),
		typeof(object)
	};

	public override void Evaluate(List<object> args, ScriptSyntaxTreeNode node, ScriptContext.EvaluationContext context, out object outValue)
	{
		outValue = string.Empty;
		if (args[0] == null)
		{
			if (!context.EditMode)
			{
				context.EmitError(ScriptContext.ErrorCodes.EvaluationError, "Format string cannot be null!");
				context.Results.SetFailedNodeIfNoneExists(node, node);
			}
			return;
		}
		try
		{
			string format = args[0] as string;
			object[] formatBits = args.GetRange(1, args.Count - 1).ToArray();
			string locString = GameStrings.Format(format, formatBits);
			if (locString != format)
			{
				outValue = locString;
			}
			else
			{
				outValue = GameStrings.FormatLocalizedString(format, formatBits);
			}
		}
		catch (FormatException)
		{
			context.EmitError(ScriptContext.ErrorCodes.EvaluationError, "Check that the format string matches the number of arguments passed in.");
			context.Results.SetFailedNodeIfNoneExists(node, node);
		}
		catch (Exception ex2)
		{
			context.EmitError(ScriptContext.ErrorCodes.EvaluationError, "Unexpected error performing string format!\n" + ex2.Message);
			context.Results.SetFailedNodeIfNoneExists(node, node);
		}
	}
}
