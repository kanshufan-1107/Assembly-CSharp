using Blizzard.T5.Services;
using Hearthstone.Core;

namespace Hearthstone.Util.Services;

public static class ServicesBootstrapper
{
	public static void SetupForStandaloneScenes()
	{
		Log.Initialize();
		PlatformSettings.RecomputeDeviceSettings();
		SetupServices();
	}

	private static void SetupServices()
	{
		ServiceManager.ServiceManagerCallbacks serviceManagerCallbacks = new ServiceManager.ServiceManagerCallbacks
		{
			RegisterUpdateServices = Processor.RegisterUpdateDelegate,
			UnregisterUpdateServices = Processor.UnregisterUpdateDelegate,
			RegisterLateUpdateServices = Processor.RegisterLateUpdateDelegate,
			UnregisterLateUpdateServices = Processor.UnregisterLateUpdateDelegate
		};
		ServiceManager.SetDependencies(Processor.JobQueue, serviceManagerCallbacks, Log.Services, HearthstoneServiceFactory.CreateServiceFactory());
	}
}
