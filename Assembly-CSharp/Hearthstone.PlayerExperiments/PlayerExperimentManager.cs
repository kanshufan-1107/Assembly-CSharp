using System;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using PegasusUtil;

namespace Hearthstone.PlayerExperiments;

public class PlayerExperimentManager : IService
{
	public static bool HasReceivedExperiments;

	private Dictionary<PlayerExperiment.TestFeature, TestVariation.TestGroup> m_playerExperimentGroups;

	public IEnumerator<IAsyncJobResult> Initialize(ServiceLocator serviceLocator)
	{
		m_playerExperimentGroups = new Dictionary<PlayerExperiment.TestFeature, TestVariation.TestGroup>();
		HearthstoneApplication.Get().WillReset += WillReset;
		serviceLocator.Get<Network>().RegisterNetHandler(InitialClientState.PacketID.ID, OnInitialClientState);
		yield break;
	}

	public Type[] GetDependencies()
	{
		return new Type[1] { typeof(Network) };
	}

	public void Shutdown()
	{
		if (ServiceManager.TryGet<Network>(out var net))
		{
			net.RemoveNetHandler(InitialClientState.PacketID.ID, OnInitialClientState);
		}
	}

	private void WillReset()
	{
		m_playerExperimentGroups = new Dictionary<PlayerExperiment.TestFeature, TestVariation.TestGroup>();
	}

	private void OnInitialClientState()
	{
		HasReceivedExperiments = true;
		foreach (PlayerExperimentState playerExperiment in Network.Get().GetInitialClientState().PlayerExperiments)
		{
			PlayerExperiment.TestFeature feature = (PlayerExperiment.TestFeature)playerExperiment.PlayerExperimentFeature;
			TestVariation.TestGroup testGroup = (TestVariation.TestGroup)playerExperiment.PlayerExperimentVariationGroup;
			m_playerExperimentGroups[feature] = testGroup;
		}
	}
}
