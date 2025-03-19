using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Hearthstone.UI.Scripting;

public abstract class ScriptSyntaxTreeRule
{
	public enum ParseResult
	{
		Failed,
		Success
	}

	protected static class Utilities
	{
		private static Type s_objectType = typeof(object);

		private static readonly Dictionary<string, int?> s_stringToNumericalIdentifier = new Dictionary<string, int?>();

		private static IDataModel[] s_globalDataModels;

		public static bool IsDynamicType(Type type)
		{
			return type == s_objectType;
		}

		public static bool UsesNumericalEncoding(ScriptSyntaxTreeNode node)
		{
			return node.Token.Value[0] == '$';
		}

		public static bool TryParseNumericalIdentifier(ScriptSyntaxTreeNode node, out int id)
		{
			if (!s_stringToNumericalIdentifier.TryGetValue(node.Token.Value, out var nullableId))
			{
				nullableId = (int.TryParse(node.Token.Value.Substring(1, node.Token.Value.Length - 1), out id) ? new int?(id) : ((int?)null));
				s_stringToNumericalIdentifier[node.Token.Value] = nullableId;
			}
			id = nullableId.GetValueOrDefault();
			return nullableId.HasValue;
		}

		public static bool GetPropertyByDisplayName(IDataModelProperties dataModel, string displayName, out DataModelProperty outProperty)
		{
			DataModelProperty[] properties = dataModel.Properties;
			for (int i = 0; i < properties.Length; i++)
			{
				DataModelProperty property = properties[i];
				if (property.PropertyDisplayName == displayName)
				{
					outProperty = property;
					return true;
				}
			}
			outProperty = default(DataModelProperty);
			return false;
		}

		public static void CollectSuggestionsInGlobalNamespace(ScriptContext.EvaluationContext evaluationContext, List<ScriptContext.SuggestionInfo> candidates, Func<string, bool> tokenPredicate = null)
		{
			if (s_globalDataModels == null)
			{
				Type dataModelInterface = typeof(IDataModel);
				s_globalDataModels = (from a in dataModelInterface.Assembly.GetTypes()
					where a != dataModelInterface && dataModelInterface.IsAssignableFrom(a)
					select Activator.CreateInstance(a) as IDataModel).ToArray().ToArray();
			}
			IDataModel[] array = s_globalDataModels;
			foreach (IDataModel model in array)
			{
				if (tokenPredicate == null || tokenPredicate(model.DataModelDisplayName))
				{
					candidates.Add(new ScriptContext.SuggestionInfo
					{
						Identifier = model.DataModelDisplayName,
						CandidateType = ScriptContext.SuggestionInfo.Types.Model,
						ValueType = model.GetType(),
						Weight = 1
					});
				}
			}
			foreach (KeyValuePair<string, MethodScriptSyntaxTreeRule.Evaluator> symbolEvaluatorPair in MethodScriptSyntaxTreeRule.MethodEvaluators)
			{
				string symbol = symbolEvaluatorPair.Key;
				if (tokenPredicate == null || tokenPredicate(symbol))
				{
					candidates.Add(new ScriptContext.SuggestionInfo
					{
						Identifier = symbol + "()",
						CandidateType = ScriptContext.SuggestionInfo.Types.Method,
						ValueType = symbolEvaluatorPair.Value.ReturnType,
						Weight = 1
					});
				}
			}
			if ((evaluationContext.SupportedFeatures & ScriptFeatureFlags.Keywords) == 0)
			{
				return;
			}
			foreach (KeyValuePair<string, object> keyword in ScriptKeywords.Keywords)
			{
				if (tokenPredicate == null || tokenPredicate(keyword.Key))
				{
					candidates.Add(new ScriptContext.SuggestionInfo
					{
						Identifier = keyword.Key,
						CandidateType = ScriptContext.SuggestionInfo.Types.Keyword,
						ValueType = keyword.Value.GetType()
					});
				}
			}
		}

		[Conditional("UNITY_EDITOR")]
		public static void EmitGlobalNamespaceSuggestions(ScriptContext.EvaluationContext evaluationContext)
		{
			if (!evaluationContext.SuggestionsEnabled)
			{
				return;
			}
			List<ScriptContext.SuggestionInfo> suggestions = new List<ScriptContext.SuggestionInfo>();
			CollectSuggestionsInGlobalNamespace(evaluationContext, suggestions);
			foreach (ScriptContext.SuggestionInfo item in suggestions)
			{
				_ = item;
			}
		}
	}

	private IEnumerable<ScriptSyntaxTreeRule> m_cachedExpectedRules;

	private IEnumerable<ScriptSyntaxTreeRule> m_cachedNestedRules;

	private HashSet<ScriptToken.TokenType> m_cachedTokens;

	protected abstract IEnumerable<ScriptSyntaxTreeRule> ExpectedRulesInternal { get; }

	protected virtual IEnumerable<ScriptSyntaxTreeRule> NestedRulesInternal => null;

	protected abstract HashSet<ScriptToken.TokenType> TokensInternal { get; }

	public HashSet<ScriptToken.TokenType> Tokens => m_cachedTokens ?? (m_cachedTokens = TokensInternal);

	public IEnumerable<ScriptSyntaxTreeRule> ExpectedRules => m_cachedExpectedRules ?? (m_cachedExpectedRules = ExpectedRulesInternal);

	public IEnumerable<ScriptSyntaxTreeRule> NestedRules => m_cachedNestedRules ?? (m_cachedNestedRules = NestedRulesInternal);

	public virtual ParseResult Parse(ScriptToken[] tokens, ref int tokenIndex, out string parseErrorMessage, out ScriptSyntaxTreeNode node)
	{
		parseErrorMessage = null;
		node = new ScriptSyntaxTreeNode(this);
		return ParseResult.Success;
	}

	public virtual bool ParseStepInto(ScriptToken[] tokens, ref int startToken, ref int endToken)
	{
		return false;
	}

	public abstract void Evaluate(ScriptSyntaxTreeNode node, ScriptContext.EvaluationContext evaluationContext, out object value);
}
public abstract class ScriptSyntaxTreeRule<T> : ScriptSyntaxTreeRule where T : ScriptSyntaxTreeRule, new()
{
	private static ScriptSyntaxTreeRule s_instance;

	public static ScriptSyntaxTreeRule Get()
	{
		return s_instance ?? (s_instance = new T());
	}
}
