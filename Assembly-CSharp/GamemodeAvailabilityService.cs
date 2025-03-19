using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using Hearthstone.Core.Streaming;
using Hearthstone.Streaming;

public class GamemodeAvailabilityService : IGamemodeAvailabilityService, IService
{
	private class TutorialJobDependency : IJobDependency, IAsyncJobResult
	{
		private const int c_timeoutSeconds = 60;

		private readonly DateTime m_dependencyTimeout;

		private bool m_hasTimedOut;

		public TutorialJobDependency()
		{
			m_dependencyTimeout = DateTime.UtcNow + TimeSpan.FromSeconds(60.0);
		}

		public bool IsReady()
		{
			if (DateTime.UtcNow >= m_dependencyTimeout)
			{
				m_hasTimedOut = true;
				return true;
			}
			NetCache netCache = NetCache.Get();
			if (netCache != null && netCache.IsNetObjectAvailable<NetCache.NetCacheProfileProgress>())
			{
				return GameUtils.CanCheckTutorialCompletion();
			}
			return false;
		}

		public bool HasTimedOut()
		{
			return m_hasTimedOut;
		}
	}

	private readonly Dictionary<IGamemodeAvailabilityService.Gamemode, IGamemodeAvailabilityService.Status> m_gamemodeStatusLookup;

	private readonly List<IGamemodeAvailabilityService.Gamemode> m_gamemodeStatusInitializationQueue;

	private static Dictionary<IGamemodeAvailabilityService.Gamemode, string> s_modeToLocalizedStringLookup = new Dictionary<IGamemodeAvailabilityService.Gamemode, string>
	{
		{
			IGamemodeAvailabilityService.Gamemode.HEARTHSTONE,
			"GLUE_TRADITIONAL"
		},
		{
			IGamemodeAvailabilityService.Gamemode.BATTLEGROUNDS,
			"GLOBAL_BATTLEGROUNDS"
		},
		{
			IGamemodeAvailabilityService.Gamemode.MERCENARIES,
			"GLOBAL_MERCENARIES"
		},
		{
			IGamemodeAvailabilityService.Gamemode.TAVERN_BRAWL,
			"GLOBAL_TAVERN_BRAWL"
		},
		{
			IGamemodeAvailabilityService.Gamemode.SOLO_ADVENTURE,
			"GLUE_ADVENTURE"
		},
		{
			IGamemodeAvailabilityService.Gamemode.ARENA,
			"GLOBAL_ARENA"
		}
	};

	private static readonly Type[] s_serviceDependencies = new Type[2]
	{
		typeof(NetCache),
		typeof(GameDownloadManager)
	};

	public static event IGamemodeAvailabilityService.OnGamemodeStateChanged GamemodeStateChange;

	public static bool TryGetGamemodeLocalizedString(IGamemodeAvailabilityService.Gamemode mode, out string locString)
	{
		return s_modeToLocalizedStringLookup.TryGetValue(mode, out locString);
	}

	public GamemodeAvailabilityService()
	{
		m_gamemodeStatusLookup = new Dictionary<IGamemodeAvailabilityService.Gamemode, IGamemodeAvailabilityService.Status>();
		m_gamemodeStatusInitializationQueue = new List<IGamemodeAvailabilityService.Gamemode>();
		foreach (IGamemodeAvailabilityService.Gamemode mode in Enum.GetValues(typeof(IGamemodeAvailabilityService.Gamemode)))
		{
			m_gamemodeStatusInitializationQueue.Add(mode);
		}
		MassUpdateAvailabilityLookup(m_gamemodeStatusInitializationQueue, IGamemodeAvailabilityService.Status.UNINITIALIZED);
	}

	Type[] IService.GetDependencies()
	{
		return s_serviceDependencies;
	}

	IEnumerator<IAsyncJobResult> IService.Initialize(ServiceLocator serviceLocator)
	{
		for (int i = m_gamemodeStatusInitializationQueue.Count - 1; i >= 0; i--)
		{
			IGamemodeAvailabilityService.Gamemode mode = m_gamemodeStatusInitializationQueue[i];
			if (!IsDownloadModuleAvailableForMode(mode))
			{
				UpdateGamemodeStatus(mode, IGamemodeAvailabilityService.Status.NOT_DOWNLOADED);
				m_gamemodeStatusInitializationQueue.RemoveAt(i);
			}
		}
		for (int i2 = m_gamemodeStatusInitializationQueue.Count - 1; i2 >= 0; i2--)
		{
			IGamemodeAvailabilityService.Gamemode mode2 = m_gamemodeStatusInitializationQueue[i2];
			if ((uint)(mode2 - 1) > 6u)
			{
				m_gamemodeStatusInitializationQueue.RemoveAt(i2);
				m_gamemodeStatusLookup[mode2] = IGamemodeAvailabilityService.Status.READY;
			}
		}
		MassUpdateAvailabilityLookup(m_gamemodeStatusInitializationQueue, IGamemodeAvailabilityService.Status.WAITING_FOR_TUTORIAL);
		UpdateAndRemoveTutorialNotReadyModesFromCollection(m_gamemodeStatusInitializationQueue);
		MassUpdateAvailabilityLookup(m_gamemodeStatusInitializationQueue, IGamemodeAvailabilityService.Status.READY);
		m_gamemodeStatusInitializationQueue.Clear();
		RegisterForStateChangeListeners();
		yield break;
	}

	void IService.Shutdown()
	{
		UnRegisterForStateChangeListeners();
	}

	public IGamemodeAvailabilityService.Status GetGamemodeStatus(IGamemodeAvailabilityService.Gamemode mode)
	{
		RefreshAvailableStates();
		if (!m_gamemodeStatusLookup.TryGetValue(mode, out var status))
		{
			return IGamemodeAvailabilityService.Status.NONE;
		}
		return status;
	}

	private void RefreshAvailableStates()
	{
		foreach (IGamemodeAvailabilityService.Gamemode mode in Enum.GetValues(typeof(IGamemodeAvailabilityService.Gamemode)))
		{
			InternalCheckAndUpdateTutorialStateForMode(mode);
		}
	}

	private void InternalCheckAndUpdateTutorialStateForMode(IGamemodeAvailabilityService.Gamemode modeToCheck)
	{
		if (m_gamemodeStatusLookup.TryGetValue(modeToCheck, out var status) && status < IGamemodeAvailabilityService.Status.READY && status > IGamemodeAvailabilityService.Status.NOT_DOWNLOADED && IsTutorialCompleteForMode(modeToCheck))
		{
			UpdateGamemodeStatus(modeToCheck, IGamemodeAvailabilityService.Status.READY);
		}
	}

	private void RegisterForStateChangeListeners()
	{
		if (ServiceManager.TryGet<GameDownloadManager>(out var gdm))
		{
			gdm.RegisterModuleInstallationStateChangeListener(OnDownloadModuleStateChanged, invokeImmediately: false);
		}
		else
		{
			Log.Services.PrintError("GamemodeAvailabilityService failed to listen for GameDownloadManager module changes!");
		}
		if (ServiceManager.TryGet<NetCache>(out var netCache))
		{
			netCache.RegisterUpdatedListener(typeof(NetCache.NetCacheProfileProgress), OnNetCacheProfileProgressChanged);
		}
	}

	private void UnRegisterForStateChangeListeners()
	{
		if (ServiceManager.TryGet<GameDownloadManager>(out var gdm))
		{
			gdm.UnregisterModuleInstallationStateChangeListener(OnDownloadModuleStateChanged);
		}
		if (ServiceManager.TryGet<NetCache>(out var netCache))
		{
			netCache.RemoveUpdatedListener(typeof(NetCache.NetCacheProfileProgress), OnNetCacheProfileProgressChanged);
		}
	}

	private void OnNetCacheProfileProgressChanged()
	{
		if (IsTutorialCompleteForMode(IGamemodeAvailabilityService.Gamemode.HEARTHSTONE))
		{
			UpdateGamemodeStatus(IGamemodeAvailabilityService.Gamemode.HEARTHSTONE, IGamemodeAvailabilityService.Status.READY);
			NetCache.Get().RemoveUpdatedListener(typeof(NetCache.NetCacheProfileProgress), OnNetCacheProfileProgressChanged);
		}
	}

	private void OnDownloadModuleStateChanged(DownloadTags.Content moduleTag, ModuleState state)
	{
		IGamemodeAvailabilityService.Gamemode gamemode = GetGamemodeTypeFromDownloadModule(moduleTag);
		if (gamemode != 0)
		{
			if (state < ModuleState.ReadyToPlay)
			{
				UpdateGamemodeStatus(gamemode, IGamemodeAvailabilityService.Status.NOT_DOWNLOADED);
			}
			else
			{
				UpdateGamemodeStatus(gamemode, IsTutorialCompleteForMode(gamemode) ? IGamemodeAvailabilityService.Status.READY : IGamemodeAvailabilityService.Status.TUTORIAL_INCOMPLETE);
			}
		}
	}

	private void UpdateGamemodeStatus(IGamemodeAvailabilityService.Gamemode mode, IGamemodeAvailabilityService.Status status)
	{
		IGamemodeAvailabilityService.Status value;
		IGamemodeAvailabilityService.Status previousStatus = (m_gamemodeStatusLookup.TryGetValue(mode, out value) ? value : IGamemodeAvailabilityService.Status.NONE);
		if (status != previousStatus)
		{
			m_gamemodeStatusLookup[mode] = status;
			GamemodeAvailabilityService.GamemodeStateChange?.Invoke(mode, status, previousStatus);
		}
	}

	private void UpdateAndRemoveTutorialNotReadyModesFromCollection(List<IGamemodeAvailabilityService.Gamemode> modes)
	{
		for (int i = modes.Count - 1; i >= 0; i--)
		{
			IGamemodeAvailabilityService.Gamemode mode = modes[i];
			if (!IsTutorialCompleteForMode(mode))
			{
				UpdateGamemodeStatus(mode, IGamemodeAvailabilityService.Status.TUTORIAL_INCOMPLETE);
				modes.RemoveAt(i);
			}
		}
	}

	private bool IsTutorialCompleteForMode(IGamemodeAvailabilityService.Gamemode mode)
	{
		switch (mode)
		{
		case IGamemodeAvailabilityService.Gamemode.BATTLEGROUNDS:
		{
			GameSaveDataManager gameSaveDataManager2 = GameSaveDataManager.Get();
			if (gameSaveDataManager2 != null && gameSaveDataManager2.IsDataReady(GameSaveKeyId.BACON))
			{
				return GameUtils.IsBattleGroundsTutorialComplete();
			}
			return false;
		}
		case IGamemodeAvailabilityService.Gamemode.MERCENARIES:
		{
			GameSaveDataManager gameSaveDataManager = GameSaveDataManager.Get();
			if (gameSaveDataManager != null && gameSaveDataManager.IsDataReady(GameSaveKeyId.MERCENARIES))
			{
				return GameUtils.IsMercenariesVillageTutorialComplete();
			}
			return false;
		}
		default:
			return GameUtils.IsTraditionalTutorialComplete();
		}
	}

	private bool IsDownloadModuleAvailableForMode(IGamemodeAvailabilityService.Gamemode mode)
	{
		if (mode == IGamemodeAvailabilityService.Gamemode.NONE)
		{
			return true;
		}
		bool isAvailable = true;
		DownloadTags.Content modularDownloadTag = GetDownloadModuleTagFromGamemodeType(mode);
		if (modularDownloadTag != 0 && ServiceManager.TryGet<GameDownloadManager>(out var gdm))
		{
			isAvailable &= gdm.IsModuleReadyToPlay(modularDownloadTag);
		}
		return isAvailable;
	}

	private void MassUpdateAvailabilityLookup(List<IGamemodeAvailabilityService.Gamemode> keysToUpdate, IGamemodeAvailabilityService.Status status)
	{
		foreach (IGamemodeAvailabilityService.Gamemode value in keysToUpdate)
		{
			UpdateGamemodeStatus(value, status);
		}
	}

	private static IGamemodeAvailabilityService.Gamemode GetGamemodeTypeFromDownloadModule(DownloadTags.Content tag)
	{
		return tag switch
		{
			DownloadTags.Content.Adventure => IGamemodeAvailabilityService.Gamemode.SOLO_ADVENTURE, 
			DownloadTags.Content.Merc => IGamemodeAvailabilityService.Gamemode.MERCENARIES, 
			DownloadTags.Content.Bgs => IGamemodeAvailabilityService.Gamemode.BATTLEGROUNDS, 
			_ => IGamemodeAvailabilityService.Gamemode.NONE, 
		};
	}

	private static DownloadTags.Content GetDownloadModuleTagFromGamemodeType(IGamemodeAvailabilityService.Gamemode mode)
	{
		return mode switch
		{
			IGamemodeAvailabilityService.Gamemode.SOLO_ADVENTURE => DownloadTags.Content.Adventure, 
			IGamemodeAvailabilityService.Gamemode.MERCENARIES => DownloadTags.Content.Merc, 
			IGamemodeAvailabilityService.Gamemode.BATTLEGROUNDS => DownloadTags.Content.Bgs, 
			_ => DownloadTags.Content.Unknown, 
		};
	}
}
