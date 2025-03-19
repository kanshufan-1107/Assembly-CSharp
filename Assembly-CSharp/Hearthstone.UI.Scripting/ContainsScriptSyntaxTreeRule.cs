using System;
using System.Collections;
using System.Collections.Generic;

namespace Hearthstone.UI.Scripting;

public class ContainsScriptSyntaxTreeRule : ScriptSyntaxTreeRule<ContainsScriptSyntaxTreeRule>
{
	private HashSet<object> m_cachedHashSet = new HashSet<object>();

	private static Dictionary<Type, Type> s_collectionToGenericArgument = new Dictionary<Type, Type>();

	protected override HashSet<ScriptToken.TokenType> TokensInternal => new HashSet<ScriptToken.TokenType> { ScriptToken.TokenType.Has };

	protected override IEnumerable<ScriptSyntaxTreeRule> ExpectedRulesInternal => new ScriptSyntaxTreeRule[5]
	{
		ScriptSyntaxTreeRule<ExpressionGroupScriptSyntaxTreeRule>.Get(),
		ScriptSyntaxTreeRule<NumberScriptSyntaxTreeRule>.Get(),
		ScriptSyntaxTreeRule<StringScriptSyntaxTreeRule>.Get(),
		ScriptSyntaxTreeRule<IdentifierScriptSyntaxTreeRule>.Get(),
		ScriptSyntaxTreeRule<MethodScriptSyntaxTreeRule>.Get()
	};

	public override void Evaluate(ScriptSyntaxTreeNode node, ScriptContext.EvaluationContext context, out object value)
	{
		value = null;
		if (!context.CheckFeatureIsSupported(ScriptFeatureFlags.Relational))
		{
			return;
		}
		if (node.Left == null || !node.Left.Evaluate(context, out var lValue))
		{
			context.Results.SetFailedNodeIfNoneExists(node, node.Left);
		}
		else
		{
			if (lValue == null)
			{
				return;
			}
			if (!(lValue is IList lCollection))
			{
				context.EmitError(ScriptContext.ErrorCodes.EvaluationError, "Only collections can be queried");
				context.Results.SetFailedNodeIfNoneExists(node, node.Left);
				return;
			}
			if (node.Right == null || !node.Right.Evaluate(context, out var rValue) || rValue == null)
			{
				context.EmitError(ScriptContext.ErrorCodes.EvaluationError, "Expected one or more values on the right!");
				context.Results.SetFailedNodeIfNoneExists(node, node.Right);
				return;
			}
			Type lType = GetCollectionGenericArgument(lCollection);
			if (rValue is IList rCollection)
			{
				int rCount = 0;
				m_cachedHashSet.Clear();
				int rCollectionCount = rCollection.Count;
				for (int i = 0; i < rCollectionCount; i++)
				{
					DynamicValue objTypePair = ((rCollection[i] is DynamicValue) ? ((DynamicValue)rCollection[i]) : default(DynamicValue));
					object x = (objTypePair.HasValidValue ? objTypePair.Value : rCollection[i]);
					m_cachedHashSet.Add(((IConvertible)x).ToType(lType, null));
					rCount++;
				}
				int matchCount = 0;
				int lCollectionCount = lCollection.Count;
				for (int j = 0; j < lCollectionCount; j++)
				{
					object x2 = lCollection[j];
					if (m_cachedHashSet.Contains(x2))
					{
						matchCount++;
					}
					if (matchCount == rCount)
					{
						break;
					}
				}
				value = rCount == matchCount;
				return;
			}
			bool found = false;
			object rConverted = ((IConvertible)rValue).ToType(lType, null);
			int lCollectionCount2 = lCollection.Count;
			for (int k = 0; k < lCollectionCount2; k++)
			{
				if (object.Equals(lCollection[k], rConverted))
				{
					found = true;
					break;
				}
			}
			value = found;
		}
	}

	private static Type GetCollectionGenericArgument(IList collection)
	{
		Type collectionType = collection.GetType();
		Type genericArgumentType = null;
		if (!s_collectionToGenericArgument.TryGetValue(collectionType, out genericArgumentType))
		{
			genericArgumentType = collectionType.GetGenericArguments()[0];
			s_collectionToGenericArgument[collectionType] = genericArgumentType;
		}
		return genericArgumentType;
	}
}
