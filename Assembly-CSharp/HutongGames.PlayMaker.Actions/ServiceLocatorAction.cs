using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using Hearthstone.Core;

namespace HutongGames.PlayMaker.Actions;

[Tooltip("Waits for services to be dynamically loaded and ready")]
[ActionCategory("Pegasus Debug")]
public class ServiceLocatorAction : FsmStateAction
{
	public bool m_sound;

	public override void Reset()
	{
		m_sound = false;
	}

	public override void OnEnter()
	{
		List<Type> services = new List<Type>();
		if (m_sound)
		{
			services.Add(typeof(SoundManager));
		}
		if (services.Count == 0)
		{
			Finish();
			return;
		}
		IJobDependency[] serviceDependencies = null;
		ServiceManager.InitializeDynamicServicesIfEditor(out serviceDependencies, services.ToArray());
		Processor.QueueJob("Playmaker.ServiceLocator", Job_Initialize(), serviceDependencies);
	}

	private IEnumerator<IAsyncJobResult> Job_Initialize()
	{
		Finish();
		yield break;
	}
}
