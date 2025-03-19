using Blizzard.T5.Jobs;
using Blizzard.T5.MaterialService;
using Blizzard.T5.Services;
using Hearthstone.Core;
using UnityEngine;

public class CardSelectorDiamondBootstrap : MonoBehaviour
{
	private void Start()
	{
		IJobDependency[] dependencies = null;
		ServiceManager.ServiceManagerCallbacks serviceManagerCallbacks = new ServiceManager.ServiceManagerCallbacks
		{
			RegisterUpdateServices = Processor.RegisterUpdateDelegate,
			UnregisterUpdateServices = Processor.UnregisterUpdateDelegate,
			RegisterLateUpdateServices = Processor.RegisterLateUpdateDelegate,
			UnregisterLateUpdateServices = Processor.UnregisterLateUpdateDelegate
		};
		ServiceManager.SetDependencies(Processor.JobQueue, serviceManagerCallbacks, Log.Services, HearthstoneServiceFactory.CreateServiceFactory());
		ServiceManager.InitializeDynamicServicesIfNeeded(out dependencies, typeof(IAssetLoader), typeof(DiamondRenderToTextureService), typeof(IMaterialService), typeof(LegendaryHeroRenderToTextureService));
	}

	private void OnDestroy()
	{
		ServiceManager.Shutdown();
	}
}
