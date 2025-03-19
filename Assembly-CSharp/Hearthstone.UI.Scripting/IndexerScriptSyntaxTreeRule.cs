using System;

namespace Hearthstone.UI.Scripting;

public class IndexerScriptSyntaxTreeRule : GenericExpressionGroupScriptSyntaxTreeRule<IndexerScriptSyntaxTreeRule>
{
	public IndexerScriptSyntaxTreeRule()
		: base(ScriptToken.TokenType.OpenSquareBrackets, ScriptToken.TokenType.ClosedSquareBrackets)
	{
	}

	public override void Evaluate(ScriptSyntaxTreeNode node, ScriptContext.EvaluationContext context, out object outValue)
	{
		outValue = null;
		if (!context.CheckFeatureIsSupported(ScriptFeatureFlags.Identifiers))
		{
			return;
		}
		if (node.Left == null || !node.Left.Evaluate(context, out var lValue))
		{
			context.EmitError(ScriptContext.ErrorCodes.EvaluationError, "Element indexed is null.");
			context.Results.SetFailedNodeIfNoneExists(node, node.Left);
		}
		else
		{
			if (lValue == null)
			{
				return;
			}
			if (!(lValue is IDataModelList collection))
			{
				context.EmitError(ScriptContext.ErrorCodes.EvaluationError, "Only collections can be indexed.");
				context.Results.SetFailedNodeIfNoneExists(node, node.Left);
				return;
			}
			if (context.EditMode)
			{
				collection.DontUpdateDataVersionOnChange();
			}
			if (node.Right == null || !node.Right.Evaluate(context, out var rValue))
			{
				context.EmitError(ScriptContext.ErrorCodes.EvaluationError, "Index contains errors.");
				context.Results.SetFailedNodeIfNoneExists(node, node.Right);
			}
			else if (rValue is IConvertible rConvertible)
			{
				int index;
				try
				{
					index = rConvertible.ToInt32(null);
				}
				catch (Exception ex)
				{
					context.EmitError(ScriptContext.ErrorCodes.EvaluationError, context.EditMode ? ex.ToString() : null);
					context.Results.SetFailedNodeIfNoneExists(node, node.Right);
					return;
				}
				if (index < 0)
				{
					context.EmitError(ScriptContext.ErrorCodes.EvaluationError, "Index must be a positive integer.");
					context.Results.SetFailedNodeIfNoneExists(node, node.Right);
				}
				else if (index < collection.Count)
				{
					outValue = collection.GetElementAtIndex(index);
					node.ValueType = ((outValue != null) ? outValue.GetType() : null);
				}
			}
		}
	}
}
