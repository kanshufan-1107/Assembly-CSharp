using System;
using System.Collections.Generic;
using Blizzard.T5.Fonts;
using Blizzard.T5.MaterialService;
using Blizzard.T5.Services;
using Hearthstone.UI;
using Scripts.Setup;

public class FXBuilderApplicationProvider : FXBuilderApplicationProviderBase
{
	private static readonly Type[] s_fxBuilderRequiredRuntimeServices = new Type[17]
	{
		typeof(UniversalInputManager),
		typeof(SoundManager),
		typeof(FullScreenFXMgr),
		typeof(IAssetLoader),
		typeof(IAliasedAssetResolver),
		typeof(IFontTable),
		typeof(IGraphicsManager),
		typeof(ShaderTime),
		typeof(GameDbf),
		typeof(WidgetRunner),
		typeof(SpriteAtlasProvider),
		typeof(SpellManager),
		typeof(IMaterialService),
		typeof(DiamondRenderToTextureService),
		typeof(LegendaryHeroRenderToTextureService),
		typeof(IGameStringsService),
		typeof(ITouchScreenService)
	};

	private IServiceFactory m_cachedServiceFactory;

	public override IServiceFactory GetServiceFactory()
	{
		if (m_cachedServiceFactory == null)
		{
			m_cachedServiceFactory = HearthstoneServiceFactory.CreateServiceFactory();
		}
		return m_cachedServiceFactory;
	}

	public override IEnumerable<Type> GetServiceTypes()
	{
		return s_fxBuilderRequiredRuntimeServices;
	}
}
