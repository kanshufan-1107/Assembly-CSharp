using System.Collections.Generic;

namespace Hearthstone.Core;

public interface IAssetManifest
{
	string[] GetTagGroups();

	void GetTagsInTagGroup(string tagGroup, ref List<string> tags);

	string GetTagGroupForTag(string tag);

	void ReadLocaleCatalogs();

	string[] GetAllAssetBundleNames(Locale locale = Locale.UNKNOWN);

	List<string> GetAssetBundleNamesForTags(string[] tags);

	bool TryGetDirectBundleFromGuid(string guid, out string assetBundleName);

	void GetTagsFromAssetBundle(string assetBundleName, List<string> tagList);

	List<string> GetAllTags(string tagGroup, bool excludeOverridenTag);

	string ConvertToOverrideTag(string tag, string tagGroup);

	uint GetBundleSize(string bundleName);

	bool TryResolveAsset(string guid, out string resolvedGuid, out string resolvedBundle, AssetVariantTags.Locale locale = AssetVariantTags.Locale.enUS, AssetVariantTags.Quality quality = AssetVariantTags.Quality.Normal, AssetVariantTags.Platform platform = AssetVariantTags.Platform.Any, AssetVariantTags.Region region = AssetVariantTags.Region.US);
}
