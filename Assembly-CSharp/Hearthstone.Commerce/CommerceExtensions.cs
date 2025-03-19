using System.Collections.Generic;
using Shared.Scripts.Game.Shop.Product;

namespace Hearthstone.Commerce;

public static class CommerceExtensions
{
	public static CatalogUtils.TagData GetTags(this AttributeSet attributes)
	{
		if (attributes != null && attributes.GetValue("tags").TryGetValue(out var tagSource))
		{
			return CatalogUtils.ParseTagsString(tagSource);
		}
		CatalogUtils.TagData result = default(CatalogUtils.TagData);
		result.Tags = new HashSet<string>();
		result.ExtraData = new Dictionary<string, string>();
		return result;
	}
}
