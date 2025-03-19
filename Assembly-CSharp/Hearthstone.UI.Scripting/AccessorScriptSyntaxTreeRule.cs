using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Hearthstone.UI.Scripting;

public class AccessorScriptSyntaxTreeRule : ScriptSyntaxTreeRule<AccessorScriptSyntaxTreeRule>
{
	private static Dictionary<Type, Dictionary<Type, bool>> s_isAssignableFromCache = new Dictionary<Type, Dictionary<Type, bool>>();

	protected override HashSet<ScriptToken.TokenType> TokensInternal => new HashSet<ScriptToken.TokenType> { ScriptToken.TokenType.Period };

	protected override IEnumerable<ScriptSyntaxTreeRule> ExpectedRulesInternal => new ScriptSyntaxTreeRule[1] { ScriptSyntaxTreeRule<IdentifierScriptSyntaxTreeRule>.Get() };

	public override void Evaluate(ScriptSyntaxTreeNode node, ScriptContext.EvaluationContext context, out object value)
	{
		value = null;
		if (!context.CheckFeatureIsSupported(ScriptFeatureFlags.Identifiers))
		{
			return;
		}
		if (node.Left == null || !node.Left.Evaluate(context, out var lValue))
		{
			context.Results.SetFailedNodeIfNoneExists(node, node.Left);
		}
		else if (Utilities.IsDynamicType(node.Left.ValueType))
		{
			_ = context.EncodingPolicy;
			node.ValueType = node.Left.ValueType;
		}
		else
		{
			if (node.Left.Value == null)
			{
				return;
			}
			if (!IsTypeAssignableFrom(typeof(IDataModelProperties), node.Left.ValueType))
			{
				context.EmitError(ScriptContext.ErrorCodes.EvaluationError, "'{0}' cannot be accessed because it's not a data model.", node.Left.Token.Value);
				context.Results.SetFailedNodeIfNoneExists(node, node.Left);
				return;
			}
			if (!(lValue is IDataModelProperties leftModel))
			{
				context.EmitError(ScriptContext.ErrorCodes.EvaluationError, "Model '{0}' cannot be accessed because it's null.", node.Left.Token.Value);
				context.Results.SetFailedNodeIfNoneExists(node, node.Left);
				return;
			}
			if (node.Right == null || node.Right.Rule != ScriptSyntaxTreeRule<IdentifierScriptSyntaxTreeRule>.Get())
			{
				context.EmitError(ScriptContext.ErrorCodes.EvaluationError, "Missing property.");
				context.Results.SetFailedNodeIfNoneExists(node, node.Right);
				return;
			}
			bool propertyFound;
			DataModelProperty propInfo;
			if (Utilities.UsesNumericalEncoding(node.Right))
			{
				if (!Utilities.TryParseNumericalIdentifier(node.Right, out var id))
				{
					context.EmitError(ScriptContext.ErrorCodes.EvaluationError, "Property identifier '{0}' is not valid because not numerical.", node.Right.Token.Value);
					context.Results.SetFailedNodeIfNoneExists(node, node.Right);
					return;
				}
				propertyFound = leftModel.GetPropertyInfo(id, out propInfo);
			}
			else
			{
				propertyFound = Utilities.GetPropertyByDisplayName(leftModel, node.Right.Token.Value, out propInfo);
			}
			if (!propertyFound)
			{
				context.EmitError(ScriptContext.ErrorCodes.EvaluationError, "Property '{0}' does not exist on model '{1}'.", node.Right.Token.Value, node.Left.Token.Value);
				context.Results.SetFailedNodeIfNoneExists(node, node.Right);
				return;
			}
			ScriptContext.EncodingPolicy encodingPolicy = context.EncodingPolicy;
			if (encodingPolicy != ScriptContext.EncodingPolicy.Numerical)
			{
				_ = 2;
			}
			if (propInfo.QueryMethod != null)
			{
				node.ValueType = typeof(DataModelProperty.QueryDelegate);
				node.Target = leftModel;
				value = propInfo.QueryMethod;
				return;
			}
			leftModel.GetPropertyValue(propInfo.PropertyId, out value);
			if (context.EditMode && value == null && context.DataModelDefaultConstructor != null && IsTypeAssignableFrom(typeof(IDataModel), propInfo.Type))
			{
				value = context.DataModelDefaultConstructor(propInfo.Type);
			}
			if (!context.EditMode && value != null && Utilities.IsDynamicType(propInfo.Type))
			{
				node.ValueType = value.GetType();
			}
			else
			{
				node.ValueType = propInfo.Type;
			}
		}
	}

	private static bool IsTypeAssignableFrom(Type toType, Type fromType)
	{
		if (!s_isAssignableFromCache.TryGetValue(toType, out var isAssignableMap))
		{
			isAssignableMap = new Dictionary<Type, bool>();
			s_isAssignableFromCache[toType] = isAssignableMap;
		}
		if (!isAssignableMap.TryGetValue(fromType, out var isAssignable))
		{
			isAssignable = (isAssignableMap[fromType] = toType.IsAssignableFrom(fromType));
		}
		return isAssignable;
	}

	[Conditional("UNITY_EDITOR")]
	private void EmitSuggestions(ScriptSyntaxTreeNode node, ScriptContext.EvaluationContext context)
	{
		if (!context.SuggestionsEnabled)
		{
			return;
		}
		IDataModelProperties leftModel = ((node.Left != null) ? (node.Left.Value as IDataModelProperties) : null);
		if (leftModel != null)
		{
			DataModelProperty[] properties = leftModel.Properties;
			for (int i = 0; i < properties.Length; i++)
			{
				_ = ref properties[i];
			}
		}
	}
}
