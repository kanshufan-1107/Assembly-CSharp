using System.Collections.Generic;
using System.Linq;
using Shared.Scripts.Util;
using Shared.Scripts.Util.ValueTypes;

namespace Shared.Scripts.Game.Shop.Product;

public class AttributeSet : Record
{
	private readonly Dictionary<string, string> m_attributes;

	public AttributeSet()
	{
		m_attributes = new Dictionary<string, string>();
	}

	public AttributeSet(IEnumerable<Attribute> attributes)
	{
		m_attributes = attributes.ToDictionary((Attribute attribute) => attribute.Name, (Attribute attribute) => attribute.Value);
	}

	public void AddAttributeIfValid(Maybe<Attribute> maybeAttribute)
	{
		if (maybeAttribute.TryGetValue(out var attribute) && Attribute.IsValid(attribute.Name, attribute.Value))
		{
			m_attributes.Add(attribute.Name, attribute.Value);
		}
	}

	public Maybe<string> GetValue(string name)
	{
		if (m_attributes.TryGetValue(name, out var value))
		{
			return value;
		}
		return Maybe.None;
	}

	protected override IEnumerable<object> GetEqualityComponents()
	{
		foreach (KeyValuePair<string, string> attribute in m_attributes)
		{
			yield return attribute.Key;
			yield return attribute.Value;
		}
	}
}
