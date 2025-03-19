using System;
using System.Collections.Generic;
using Blizzard.T5.Core;

namespace Hearthstone.UI.Scripting;

public class EventScriptSyntaxTreeRule : ScriptSyntaxTreeRule<EventScriptSyntaxTreeRule>
{
	protected override HashSet<ScriptToken.TokenType> TokensInternal => new HashSet<ScriptToken.TokenType> { ScriptToken.TokenType.Colon };

	protected override IEnumerable<ScriptSyntaxTreeRule> ExpectedRulesInternal => new ScriptSyntaxTreeRule[2]
	{
		ScriptSyntaxTreeRule<ConditionalScriptSyntaxTreeRule>.Get(),
		ScriptSyntaxTreeRule<EmptyScriptSyntaxTreeRule>.Get()
	};

	public override ParseResult Parse(ScriptToken[] tokens, ref int tokenIndex, out string parseErrorMessage, out ScriptSyntaxTreeNode node)
	{
		parseErrorMessage = null;
		node = null;
		if (tokenIndex >= tokens.Length - 1)
		{
			parseErrorMessage = "Event types expected: changed";
			return ParseResult.Failed;
		}
		ScriptToken eventType = tokens[++tokenIndex];
		if (eventType.Value != "changed")
		{
			parseErrorMessage = "Event types expected: changed";
			return ParseResult.Failed;
		}
		node = new ScriptSyntaxTreeNode(this);
		return ParseResult.Success;
	}

	public override void Evaluate(ScriptSyntaxTreeNode node, ScriptContext.EvaluationContext context, out object value)
	{
		value = null;
		if (!context.CheckFeatureIsSupported(ScriptFeatureFlags.Events))
		{
			return;
		}
		if (context.CachedNodeValues == null)
		{
			context.CachedNodeValues = new Map<ScriptSyntaxTreeNode, int>();
		}
		if (node.Left != null && node.Left.Evaluate(context, out var lValue) && node.Tokens[1].Value == "changed")
		{
			int lValueHash = 0;
			if (lValue != null)
			{
				lValueHash = ((lValue is IDataModelProperties lValueModel) ? lValueModel.GetPropertiesHashCode() : lValue.GetHashCode());
			}
			value = false;
			if (!context.CachedNodeValues.TryGetValue(node, out var cachedHash) && lValue != null)
			{
				Type lValueType = lValue.GetType();
				value = (lValueType.IsPrimitive ? (lValueHash != Activator.CreateInstance(lValueType).GetHashCode()) : (lValueHash != 0));
			}
			else
			{
				value = lValueHash != cachedHash;
			}
			ref bool eventRaised = ref context.Results.EventRaised;
			eventRaised |= (bool)value;
			context.CachedNodeValues[node] = lValueHash;
		}
	}
}
