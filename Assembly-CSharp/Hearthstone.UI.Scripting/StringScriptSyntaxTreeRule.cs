using System.Collections.Generic;
using System.Text;

namespace Hearthstone.UI.Scripting;

public class StringScriptSyntaxTreeRule : ScriptSyntaxTreeRule<StringScriptSyntaxTreeRule>
{
	protected override HashSet<ScriptToken.TokenType> TokensInternal => new HashSet<ScriptToken.TokenType> { ScriptToken.TokenType.DoubleQuote };

	protected override IEnumerable<ScriptSyntaxTreeRule> ExpectedRulesInternal => new ScriptSyntaxTreeRule[4]
	{
		ScriptSyntaxTreeRule<ConditionalScriptSyntaxTreeRule>.Get(),
		ScriptSyntaxTreeRule<RelationalScriptSyntaxTreeRule>.Get(),
		ScriptSyntaxTreeRule<TupleScriptSyntaxTreeRule>.Get(),
		ScriptSyntaxTreeRule<EmptyScriptSyntaxTreeRule>.Get()
	};

	public override ParseResult Parse(ScriptToken[] tokens, ref int tokenIndex, out string parseErrorMessage, out ScriptSyntaxTreeNode node)
	{
		node = null;
		parseErrorMessage = null;
		bool closed = false;
		StringBuilder rawString = new StringBuilder();
		int i;
		for (i = tokenIndex; i < tokens.Length; i++)
		{
			bool num = tokens[i].Type == ScriptToken.TokenType.DoubleQuote;
			if (!num)
			{
				rawString.Append(tokens[i].Value);
			}
			if (num && i > tokenIndex)
			{
				closed = true;
				break;
			}
		}
		if (!closed)
		{
			parseErrorMessage = "";
			return ParseResult.Failed;
		}
		tokenIndex = i;
		node = new ScriptSyntaxTreeNode(this)
		{
			Payload = rawString.ToString()
		};
		return ParseResult.Success;
	}

	public override void Evaluate(ScriptSyntaxTreeNode node, ScriptContext.EvaluationContext evaluationContext, out object value)
	{
		_ = evaluationContext.EncodingPolicy;
		value = (string)node.Payload;
	}
}
