using System.Collections.Generic;

namespace Hearthstone.UI.Scripting;

public class ExpressionGroupScriptSyntaxTreeRule : GenericExpressionGroupScriptSyntaxTreeRule<ExpressionGroupScriptSyntaxTreeRule>
{
	private DataModelList<object> m_matchingElements = new DataModelList<object>();

	public override void Evaluate(ScriptSyntaxTreeNode node, ScriptContext.EvaluationContext context, out object outValue)
	{
		outValue = null;
		if (node.Left != null && node.Left.Rule == ScriptSyntaxTreeRule<AccessorScriptSyntaxTreeRule>.Get())
		{
			if (node.Left == null || !node.Left.Evaluate(context, out var lValue) || !(lValue is DataModelProperty.QueryDelegate) || !context.CheckFeatureIsSupported(ScriptFeatureFlags.Methods))
			{
				return;
			}
			if (!(node.Left.Target is IDataModelList queriedList))
			{
				context.EmitError(ScriptContext.ErrorCodes.EvaluationError, "Only collections are queryable!");
				context.Results.SetFailedNodeIfNoneExists(node, node.Left);
				return;
			}
			m_matchingElements.Clear();
			m_matchingElements.DontUpdateDataVersionOnChange();
			if (context.EncodingPolicy != 0 || context.EditMode)
			{
				queriedList.DontUpdateDataVersionOnChange();
				queriedList.Clear();
				queriedList.AddDefaultValue();
				m_matchingElements.Add(queriedList.GetElementAtIndex(0));
			}
			if (context.QueryObjects == null)
			{
				context.QueryObjects = new List<object>();
			}
			context.QueryObjects.Add(null);
			for (int i = 0; i < queriedList.Count; i++)
			{
				object queriedObject = queriedList.GetElementAtIndex(i);
				context.QueryObjects[context.QueryObjects.Count - 1] = queriedObject;
				if (node.Right == null)
				{
					EmitLambdaSuggestion(context);
					context.EmitError(ScriptContext.ErrorCodes.EvaluationError, "Missing query!");
					context.Results.SetFailedNodeIfNoneExists(node, node.Right);
					return;
				}
				if (!node.Right.Evaluate(context, out node.Value))
				{
					EmitLambdaSuggestion(context);
					context.Results.SetFailedNodeIfNoneExists(node, node.Right);
					return;
				}
				if (object.Equals(node.Value, true))
				{
					m_matchingElements.Add(queriedObject);
				}
			}
			context.QueryObjects.RemoveAt(context.QueryObjects.Count - 1);
			DataModelProperty.QueryDelegate queryMethod = (DataModelProperty.QueryDelegate)node.Left.Value;
			node.Value = (outValue = queryMethod(m_matchingElements));
			node.ValueType = node.Value.GetType();
		}
		else if (node.Right == null)
		{
			if (node.Left == null || node.Left.Rule != ScriptSyntaxTreeRule<MethodScriptSyntaxTreeRule>.Get())
			{
				context.EmitError(ScriptContext.ErrorCodes.EvaluationError, "Expression group cannot be empty!");
				context.Results.SetFailedNodeIfNoneExists(node, node.Right);
			}
		}
		else if (!node.Right.Evaluate(context, out node.Value))
		{
			context.Results.SetFailedNodeIfNoneExists(node, node.Right);
		}
		else
		{
			node.ValueType = node.Right.ValueType ?? node.Value?.GetType();
			outValue = node.Value;
		}
	}

	private void EmitLambdaSuggestion(ScriptContext.EvaluationContext context)
	{
		_ = context.SuggestionsEnabled;
	}
}
