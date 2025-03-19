using System;
using System.Collections.Generic;
using System.Globalization;

namespace Hearthstone.UI.Scripting;

public class NumberScriptSyntaxTreeRule : ScriptSyntaxTreeRule<NumberScriptSyntaxTreeRule>
{
	private static Dictionary<Type, Dictionary<int, bool>> s_isIntDefinedInEnumCache = new Dictionary<Type, Dictionary<int, bool>>();

	private static Dictionary<Type, Dictionary<int, object>> s_intToEnumObjectCache = new Dictionary<Type, Dictionary<int, object>>();

	private static Dictionary<string, double?> s_stringsToDoubles = new Dictionary<string, double?>();

	protected override HashSet<ScriptToken.TokenType> TokensInternal => new HashSet<ScriptToken.TokenType> { ScriptToken.TokenType.Numerical };

	protected override IEnumerable<ScriptSyntaxTreeRule> ExpectedRulesInternal => new ScriptSyntaxTreeRule[5]
	{
		ScriptSyntaxTreeRule<ConditionalScriptSyntaxTreeRule>.Get(),
		ScriptSyntaxTreeRule<RelationalScriptSyntaxTreeRule>.Get(),
		ScriptSyntaxTreeRule<ArithmeticScriptSyntaxTreeRule>.Get(),
		ScriptSyntaxTreeRule<TupleScriptSyntaxTreeRule>.Get(),
		ScriptSyntaxTreeRule<EmptyScriptSyntaxTreeRule>.Get()
	};

	public override void Evaluate(ScriptSyntaxTreeNode node, ScriptContext.EvaluationContext context, out object outValue)
	{
		outValue = null;
		if (!context.CheckFeatureIsSupported(ScriptFeatureFlags.Constants))
		{
			return;
		}
		string strNumber = node.Token.Value;
		if (!TryParseAsDouble(strNumber, out var number))
		{
			return;
		}
		if (node.Parent.Left != null && node.Parent.Left.ValueType != null && node.Parent.Left.ValueType.IsEnum)
		{
			Type enumType = node.Parent.Left.ValueType;
			double num = Math.Ceiling(number) - number;
			int enumValue = (int)number;
			object enumValueObject = IntToEnumObject(enumType, enumValue);
			if (num > 0.0 || enumValueObject == null)
			{
				context.EmitError(ScriptContext.ErrorCodes.EvaluationError, "Number {0} cannot be converted to enum {1}.", number, enumType);
				return;
			}
			node.Value = enumValueObject;
			node.ValueType = node.Parent.Left.ValueType;
			outValue = node.Value;
			ScriptContext.EncodingPolicy encodingPolicy = context.EncodingPolicy;
			if (encodingPolicy != ScriptContext.EncodingPolicy.Numerical)
			{
				_ = 2;
			}
		}
		else
		{
			node.Value = number;
			node.ValueType = typeof(double);
			outValue = node.Value;
		}
	}

	private static bool IsIntDefinedInEnum(Type enumType, int intValue)
	{
		if (!s_isIntDefinedInEnumCache.TryGetValue(enumType, out var isIntDefinedMap))
		{
			isIntDefinedMap = new Dictionary<int, bool>();
			s_isIntDefinedInEnumCache[enumType] = isIntDefinedMap;
		}
		if (!isIntDefinedMap.TryGetValue(intValue, out var isDefined))
		{
			isDefined = (isIntDefinedMap[intValue] = Enum.IsDefined(enumType, intValue));
		}
		return isDefined;
	}

	private static object IntToEnumObject(Type enumType, int intValue)
	{
		if (!IsIntDefinedInEnum(enumType, intValue))
		{
			return null;
		}
		if (!s_intToEnumObjectCache.TryGetValue(enumType, out var intToEnumValue))
		{
			intToEnumValue = new Dictionary<int, object>();
			s_intToEnumObjectCache[enumType] = intToEnumValue;
		}
		if (!intToEnumValue.TryGetValue(intValue, out var enumValue))
		{
			enumValue = (intToEnumValue[intValue] = Enum.ToObject(enumType, intValue));
		}
		return enumValue;
	}

	private bool TryParseAsDouble(string str, out double value)
	{
		if (!s_stringsToDoubles.TryGetValue(str, out var nullableValue))
		{
			if (double.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
			{
				nullableValue = value;
				s_stringsToDoubles[str] = nullableValue;
			}
			else
			{
				s_stringsToDoubles[str] = null;
			}
		}
		value = nullableValue.GetValueOrDefault();
		return nullableValue.HasValue;
	}
}
