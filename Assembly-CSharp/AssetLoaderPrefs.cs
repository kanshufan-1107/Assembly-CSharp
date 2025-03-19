public static class AssetLoaderPrefs
{
	public enum ASSET_LOADING_METHOD
	{
		EDITOR_FILES,
		ASSET_BUNDLES
	}

	public static ASSET_LOADING_METHOD AssetLoadingMethod => ASSET_LOADING_METHOD.ASSET_BUNDLES;
}
