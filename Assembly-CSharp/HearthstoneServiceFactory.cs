using System;
using System.Collections.Generic;
using Blizzard.T5.Core.Time;
using Blizzard.T5.Fonts;
using Blizzard.T5.MaterialService;
using Blizzard.T5.Services;
using Hearthstone.UI;

public static class HearthstoneServiceFactory
{
	private class HearthstoneServiceFactoryPrivate : IServiceFactory
	{
		public bool TryCreateService(Type serviceType, out IService service)
		{
			if (ServiceConstructors.TryGetValue(serviceType, out var serviceConstructor))
			{
				service = serviceConstructor();
				return true;
			}
			service = null;
			return false;
		}
	}

	public static readonly Dictionary<Type, Func<IService>> ServiceConstructors = new Dictionary<Type, Func<IService>>
	{
		{
			typeof(UniversalInputManager),
			() => new UniversalInputManager()
		},
		{
			typeof(SoundManager),
			() => new SoundManager()
		},
		{
			typeof(FullScreenFXMgr),
			() => new FullScreenFXMgr()
		},
		{
			typeof(IAssetLoader),
			() => new AssetLoader()
		},
		{
			typeof(IAliasedAssetResolver),
			() => new AliasedAssetResolver()
		},
		{
			typeof(IFontTable),
			() => new FontTable(new FontLoader(Log.Font))
		},
		{
			typeof(IGraphicsManager),
			() => new GraphicsManager()
		},
		{
			typeof(ShaderTime),
			() => new ShaderTime()
		},
		{
			typeof(GameDbf),
			() => new GameDbf()
		},
		{
			typeof(WidgetRunner),
			() => new WidgetRunner()
		},
		{
			typeof(SpriteAtlasProvider),
			() => new SpriteAtlasProvider()
		},
		{
			typeof(SpellManager),
			() => new SpellManager()
		},
		{
			typeof(IMaterialService),
			() => new MaterialService(new UnityTimeProvider())
		},
		{
			typeof(DiamondRenderToTextureService),
			() => new DiamondRenderToTextureService()
		},
		{
			typeof(LegendaryHeroRenderToTextureService),
			() => new LegendaryHeroRenderToTextureService()
		},
		{
			typeof(IGameStringsService),
			() => new GameStringsService()
		},
		{
			typeof(ITouchScreenService),
			() => new W8Touch()
		}
	};

	public static IServiceFactory CreateServiceFactory()
	{
		return new HearthstoneServiceFactoryPrivate();
	}
}
