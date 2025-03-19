using System;
using Blizzard.T5.Fonts;
using Blizzard.T5.MaterialService;
using Hearthstone.UI;

namespace Hearthstone.Util.Services;

public static class DynamicServiceSets
{
	public static Type[] UberText()
	{
		return new Type[3]
		{
			typeof(IGraphicsManager),
			typeof(IGameStringsService),
			typeof(IFontTable)
		};
	}

	public static Type[] UIFramework()
	{
		return new Type[5]
		{
			typeof(UniversalInputManager),
			typeof(SoundManager),
			typeof(IAssetLoader),
			typeof(FullScreenFXMgr),
			typeof(WidgetRunner)
		};
	}

	public static Type[] DiamondViewer()
	{
		return new Type[3]
		{
			typeof(DiamondRenderToTextureService),
			typeof(IMaterialService),
			typeof(LegendaryHeroRenderToTextureService)
		};
	}
}
