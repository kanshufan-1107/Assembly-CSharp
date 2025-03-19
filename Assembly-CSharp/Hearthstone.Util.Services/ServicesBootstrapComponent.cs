using System;
using Blizzard.T5.Services;
using UnityEngine;

namespace Hearthstone.Util.Services;

public class ServicesBootstrapComponent : MonoBehaviour
{
	[Header("Features Required")]
	public bool UberText = true;

	public bool UIFramework = true;

	public bool DiamondViewer;

	public void Awake()
	{
		ServicesBootstrapper.SetupForStandaloneScenes();
		if (UberText)
		{
			InitializeServices(DynamicServiceSets.UberText());
		}
		if (UIFramework)
		{
			InitializeServices(DynamicServiceSets.UIFramework());
		}
		if (DiamondViewer)
		{
			InitializeServices(DynamicServiceSets.DiamondViewer());
		}
	}

	private static void InitializeServices(Type[] serviceTypes)
	{
		ServiceManager.InitializeDynamicServicesIfNeeded(out var _, serviceTypes);
	}
}
