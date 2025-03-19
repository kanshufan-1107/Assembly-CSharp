using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Blizzard.T5.Core;

namespace Hearthstone.UI.Scripting;

public class MethodScriptSyntaxTreeRule : ScriptSyntaxTreeRule<MethodScriptSyntaxTreeRule>
{
	public abstract class Evaluator
	{
		private string m_methodSymbolCache;

		private Type[] m_expectedArgsCache;

		public string MethodSymbol => m_methodSymbolCache ?? (m_methodSymbolCache = MethodSymbolInternal);

		public Type[] ExpectedArgs => m_expectedArgsCache ?? (m_expectedArgsCache = ExpectedArgsInternal);

		public virtual bool HandlesNullArgs => false;

		public virtual bool EndsWithVariableArguments => false;

		public abstract Type ReturnType { get; }

		protected abstract string MethodSymbolInternal { get; }

		protected virtual Type[] ExpectedArgsInternal => new Type[0];

		public abstract void Evaluate(List<object> args, ScriptSyntaxTreeNode node, ScriptContext.EvaluationContext context, out object outValue);

		public void EmitSuggestions(ScriptSyntaxTreeNode node, ScriptContext.EvaluationContext context, int argIndex)
		{
			_ = context.SuggestionsEnabled;
		}

		[Conditional("UNITY_EDITOR")]
		protected virtual void EmitSuggestionsInternal(ScriptSyntaxTreeNode methodNode, ScriptContext.EvaluationContext context, int argIndex)
		{
		}
	}

	public abstract class Evaluator<T> : Evaluator where T : Evaluator, new()
	{
		private static T s_instance;

		public static T Get()
		{
			return s_instance ?? (s_instance = new T());
		}
	}

	private static readonly Map<string, Evaluator> s_methodEvaluators = new Map<string, Evaluator>
	{
		{
			Evaluator<HasDataMethodEvaluator>.Get().MethodSymbol,
			Evaluator<HasDataMethodEvaluator>.Get()
		},
		{
			Evaluator<HasNoDataMethodEvaluator>.Get().MethodSymbol,
			Evaluator<HasNoDataMethodEvaluator>.Get()
		},
		{
			Evaluator<IsInStateMethodEvaluator>.Get().MethodSymbol,
			Evaluator<IsInStateMethodEvaluator>.Get()
		},
		{
			Evaluator<RandomIntegerMethodEvaluator>.Get().MethodSymbol,
			Evaluator<RandomIntegerMethodEvaluator>.Get()
		},
		{
			Evaluator<StringFormatMethodEvaluator>.Get().MethodSymbol,
			Evaluator<StringFormatMethodEvaluator>.Get()
		}
	};

	public static Map<string, Evaluator> MethodEvaluators => s_methodEvaluators;

	protected override HashSet<ScriptToken.TokenType> TokensInternal => new HashSet<ScriptToken.TokenType> { ScriptToken.TokenType.Method };

	protected override IEnumerable<ScriptSyntaxTreeRule> ExpectedRulesInternal => new ScriptSyntaxTreeRule[1] { ScriptSyntaxTreeRule<ExpressionGroupScriptSyntaxTreeRule>.Get() };

	public static bool TryGetMethodEvaluator(string methodSymbol, out Evaluator evaluator)
	{
		evaluator = null;
		if (s_methodEvaluators.TryGetValue(methodSymbol, out var methodEvaluator))
		{
			evaluator = methodEvaluator;
			return true;
		}
		return false;
	}

	public override ParseResult Parse(ScriptToken[] tokens, ref int tokenIndex, out string parseErrorMessage, out ScriptSyntaxTreeNode node)
	{
		node = null;
		ScriptToken methodToken = tokens[tokenIndex];
		if (TryGetMethodEvaluator(methodToken.Value, out var _))
		{
			node = new ScriptSyntaxTreeNode(ScriptSyntaxTreeRule<MethodScriptSyntaxTreeRule>.Get());
			parseErrorMessage = null;
			return ParseResult.Success;
		}
		parseErrorMessage = "No such method \"" + methodToken.Value + "\" exists";
		return ParseResult.Failed;
	}

	public override void Evaluate(ScriptSyntaxTreeNode node, ScriptContext.EvaluationContext context, out object outValue)
	{
		outValue = null;
		if (!context.CheckFeatureIsSupported(ScriptFeatureFlags.Methods))
		{
			return;
		}
		string methodSymbol = node.Token.Value;
		if (!TryGetMethodEvaluator(methodSymbol, out var methodEvaluator))
		{
			context.EmitError(ScriptContext.ErrorCodes.EvaluationError, "No such method \"" + methodSymbol + "\" exists");
			context.Results.SetFailedNodeIfNoneExists(node, node);
			return;
		}
		if (node.Right == null || node.Right.Rule != ScriptSyntaxTreeRule<ExpressionGroupScriptSyntaxTreeRule>.Get())
		{
			context.EmitError(ScriptContext.ErrorCodes.EvaluationError, "Method signature expected '()' brackets");
			context.Results.SetFailedNodeIfNoneExists(node, node);
			return;
		}
		if (!node.Right.Evaluate(context, out var rValue))
		{
			context.EmitError(ScriptContext.ErrorCodes.EvaluationError, "Unexpected error evaluating arguments for method " + methodSymbol + "!");
			context.Results.SetFailedNodeIfNoneExists(node, node.Right);
			return;
		}
		ArrayList argTypePairs = rValue as ArrayList;
		if (argTypePairs == null)
		{
			argTypePairs = new ArrayList
			{
				new DynamicValue(rValue, node.Right.ValueType)
			};
		}
		List<object> validatedValues = new List<object>(methodEvaluator.ExpectedArgs.Length);
		if (ValidateAndPackageArguments(node, context, methodEvaluator, argTypePairs, methodEvaluator.ExpectedArgs, validatedValues))
		{
			node.ValueType = methodEvaluator.ReturnType;
			methodEvaluator.Evaluate(validatedValues, node, context, out outValue);
		}
	}

	public static bool ValidateAndPackageArguments(ScriptSyntaxTreeNode node, ScriptContext.EvaluationContext context, Evaluator methodEvaluator, ArrayList inputArguments, IList<Type> expectedArgTypes, IList<object> outArgValues)
	{
		int requiredArgCount = (methodEvaluator.EndsWithVariableArguments ? (expectedArgTypes.Count - 1) : expectedArgTypes.Count);
		int argIndex;
		for (argIndex = 0; argIndex < requiredArgCount; argIndex++)
		{
			if (argIndex >= inputArguments.Count)
			{
				context.EmitError(ScriptContext.ErrorCodes.EvaluationError, $"{methodEvaluator.MethodSymbol} expects {expectedArgTypes.Count} arguments");
				context.Results.SetFailedNodeIfNoneExists(node, node.Right);
				return false;
			}
			Type expectedType = expectedArgTypes[argIndex];
			DynamicValue argument = ((inputArguments[argIndex] is DynamicValue) ? ((DynamicValue)inputArguments[argIndex]) : default(DynamicValue));
			if (!ValidateAndConvertIfNeeded(node, context, methodEvaluator, expectedType, argument, argIndex, out var validatedValue))
			{
				return false;
			}
			outArgValues.Add(validatedValue);
		}
		if (methodEvaluator.EndsWithVariableArguments && argIndex == expectedArgTypes.Count - 1)
		{
			Type expectedType2 = expectedArgTypes[argIndex];
			for (; argIndex < inputArguments.Count; argIndex++)
			{
				DynamicValue argument2 = ((inputArguments[argIndex] is DynamicValue) ? ((DynamicValue)inputArguments[argIndex]) : default(DynamicValue));
				if (!ValidateAndConvertIfNeeded(node, context, methodEvaluator, expectedType2, argument2, argIndex, out var validatedValue2))
				{
					return false;
				}
				outArgValues.Add(validatedValue2);
			}
		}
		return true;
	}

	private static bool ValidateAndConvertIfNeeded(ScriptSyntaxTreeNode node, ScriptContext.EvaluationContext context, Evaluator methodEvaluator, Type expectedType, DynamicValue inputArgument, int argumentIndex, out object validatedValue)
	{
		validatedValue = null;
		if (inputArgument.Value == null && methodEvaluator.HandlesNullArgs)
		{
			validatedValue = inputArgument.Value;
			return true;
		}
		if (!inputArgument.HasValidValue)
		{
			context.EmitError(ScriptContext.ErrorCodes.EvaluationError, $"{methodEvaluator.MethodSymbol} argument {argumentIndex + 1} was an invalid value");
			context.Results.SetFailedNodeIfNoneExists(node, node.Right);
			return false;
		}
		if (inputArgument.ValueType == expectedType || inputArgument.ValueType.IsSubclassOf(expectedType) || (expectedType.IsInterface && inputArgument.ValueType.GetInterface(expectedType.FullName) != null))
		{
			validatedValue = inputArgument.Value;
			return true;
		}
		bool num = inputArgument.ValueType == typeof(int) || inputArgument.ValueType == typeof(long);
		bool inputIsFloatNum = inputArgument.ValueType == typeof(float) || inputArgument.ValueType == typeof(double);
		bool expectedWholeNum = expectedType == typeof(int) || expectedType == typeof(long);
		bool expectedFloatNum = expectedType == typeof(float) || expectedType == typeof(double);
		if ((num || inputIsFloatNum) && (expectedWholeNum || expectedFloatNum))
		{
			if (!TryConvertToType((IConvertible)inputArgument.Value, expectedType, out var convertedValue))
			{
				context.EmitError(ScriptContext.ErrorCodes.InternalError, methodEvaluator.MethodSymbol + " failed to convert to the expected type! Reach out to a UI Framework engineer for help.");
				context.Results.SetFailedNodeIfNoneExists(node, node.Right);
				return false;
			}
			validatedValue = convertedValue;
			return true;
		}
		context.EmitError(ScriptContext.ErrorCodes.EvaluationError, $"{methodEvaluator.MethodSymbol} argument {argumentIndex + 1} must be type {expectedType}");
		context.Results.SetFailedNodeIfNoneExists(node, node.Right);
		return false;
	}

	private static bool TryConvertToType(IConvertible convertibleValue, Type targetType, out object value)
	{
		value = null;
		try
		{
			if (targetType == typeof(int))
			{
				value = convertibleValue.ToInt32(null);
			}
			else if (targetType == typeof(float))
			{
				value = convertibleValue.ToSingle(null);
			}
			else if (targetType == typeof(long))
			{
				value = convertibleValue.ToInt64(null);
			}
			else
			{
				if (!(targetType == typeof(double)))
				{
					return false;
				}
				value = convertibleValue.ToDouble(null);
			}
		}
		catch
		{
			return false;
		}
		return true;
	}
}
