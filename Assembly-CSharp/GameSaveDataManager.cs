using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Blizzard.T5.Core;
using Hearthstone;
using Hearthstone.Core;
using PegasusShared;
using PegasusUtil;
using UnityEngine;

public class GameSaveDataManager
{
	public struct AdventureDungeonCrawlClassProgressSubkeys
	{
		public GameSaveKeySubkeyId bossWins;

		public GameSaveKeySubkeyId runWins;
	}

	public struct AdventureDungeonCrawlWingProgressSubkeys
	{
		public GameSaveKeySubkeyId heroCardWins;

		public GameSaveKeySubkeyId heroPowerWins;

		public GameSaveKeySubkeyId deckWins;

		public GameSaveKeySubkeyId treasureWins;
	}

	public struct GameSaveKeyTuple
	{
		public GameSaveKeyId Key;

		public GameSaveKeySubkeyId Subkey;

		public GameSaveKeyTuple(GameSaveKeyId key, GameSaveKeySubkeyId subkey)
		{
			Key = key;
			Subkey = subkey;
		}

		public override bool Equals(object obj)
		{
			if (obj is GameSaveKeyTuple)
			{
				return Equals((GameSaveKeyTuple)obj);
			}
			return false;
		}

		public bool Equals(GameSaveKeyTuple p)
		{
			if (Key == p.Key)
			{
				return Subkey == p.Subkey;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (int)Key ^ (int)Subkey;
		}
	}

	public class SubkeySaveRequest
	{
		public readonly GameSaveKeyId Key;

		public readonly GameSaveKeySubkeyId Subkey;

		public readonly long[] Long_Values;

		public readonly string[] String_Values;

		public SubkeySaveRequest(GameSaveKeyId key, GameSaveKeySubkeyId subkey, params long[] values)
		{
			Key = key;
			Subkey = subkey;
			Long_Values = values;
		}

		public SubkeySaveRequest(GameSaveKeyId key, GameSaveKeySubkeyId subkey, params string[] values)
		{
			Key = key;
			Subkey = subkey;
			String_Values = values;
		}
	}

	private class PendingRequestContext
	{
		public readonly List<GameSaveKeyId> AffectedKeys = new List<GameSaveKeyId>();

		public readonly OnRequestDataResponseDelegate RequestCallback;

		public readonly OnSaveDataResponseDelegate SaveCallback;

		public PendingRequestContext(List<GameSaveKeyId> requestedKeys, OnRequestDataResponseDelegate requestCallback)
		{
			AffectedKeys.AddRange(requestedKeys);
			RequestCallback = requestCallback;
		}

		public PendingRequestContext(List<SubkeySaveRequest> requests, OnSaveDataResponseDelegate saveCallback)
		{
			foreach (SubkeySaveRequest request in requests)
			{
				AffectedKeys.Add(request.Key);
			}
			SaveCallback = saveCallback;
		}
	}

	private class ServerOptionFlagMigrationData
	{
		public readonly GameSaveKeyId KeyId;

		public readonly GameSaveKeySubkeyId SubkeyId;

		public readonly int FlagTrueValue;

		public readonly int FlagFalseValue;

		public ServerOptionFlagMigrationData(GameSaveKeyId keyId, GameSaveKeySubkeyId subkeyId, int flagTrueValue = 1, int flagFalseValue = 0)
		{
			FlagTrueValue = flagTrueValue;
			FlagFalseValue = flagFalseValue;
			KeyId = keyId;
			SubkeyId = subkeyId;
		}
	}

	public delegate void OnRequestDataResponseDelegate(bool success);

	public delegate void OnSaveDataResponseDelegate(bool success);

	public delegate void OnGameSaveDataUpdateDelegate(GameSaveKeyId key);

	private static int s_clientToken = 0;

	private static readonly Map<TAG_CLASS, AdventureDungeonCrawlClassProgressSubkeys> AdventureDungeonCrawlClassToSubkeyMapping = new Map<TAG_CLASS, AdventureDungeonCrawlClassProgressSubkeys>
	{
		{
			TAG_CLASS.PALADIN,
			new AdventureDungeonCrawlClassProgressSubkeys
			{
				bossWins = GameSaveKeySubkeyId.DUNGEON_CRAWL_PALADIN_BOSS_WINS,
				runWins = GameSaveKeySubkeyId.DUNGEON_CRAWL_PALADIN_RUN_WINS
			}
		},
		{
			TAG_CLASS.HUNTER,
			new AdventureDungeonCrawlClassProgressSubkeys
			{
				bossWins = GameSaveKeySubkeyId.DUNGEON_CRAWL_HUNTER_BOSS_WINS,
				runWins = GameSaveKeySubkeyId.DUNGEON_CRAWL_HUNTER_RUN_WINS
			}
		},
		{
			TAG_CLASS.MAGE,
			new AdventureDungeonCrawlClassProgressSubkeys
			{
				bossWins = GameSaveKeySubkeyId.DUNGEON_CRAWL_MAGE_BOSS_WINS,
				runWins = GameSaveKeySubkeyId.DUNGEON_CRAWL_MAGE_RUN_WINS
			}
		},
		{
			TAG_CLASS.SHAMAN,
			new AdventureDungeonCrawlClassProgressSubkeys
			{
				bossWins = GameSaveKeySubkeyId.DUNGEON_CRAWL_SHAMAN_BOSS_WINS,
				runWins = GameSaveKeySubkeyId.DUNGEON_CRAWL_SHAMAN_RUN_WINS
			}
		},
		{
			TAG_CLASS.WARRIOR,
			new AdventureDungeonCrawlClassProgressSubkeys
			{
				bossWins = GameSaveKeySubkeyId.DUNGEON_CRAWL_WARRIOR_BOSS_WINS,
				runWins = GameSaveKeySubkeyId.DUNGEON_CRAWL_WARRIOR_RUN_WINS
			}
		},
		{
			TAG_CLASS.ROGUE,
			new AdventureDungeonCrawlClassProgressSubkeys
			{
				bossWins = GameSaveKeySubkeyId.DUNGEON_CRAWL_ROGUE_BOSS_WINS,
				runWins = GameSaveKeySubkeyId.DUNGEON_CRAWL_ROGUE_RUN_WINS
			}
		},
		{
			TAG_CLASS.WARLOCK,
			new AdventureDungeonCrawlClassProgressSubkeys
			{
				bossWins = GameSaveKeySubkeyId.DUNGEON_CRAWL_WARLOCK_BOSS_WINS,
				runWins = GameSaveKeySubkeyId.DUNGEON_CRAWL_WARLOCK_RUN_WINS
			}
		},
		{
			TAG_CLASS.PRIEST,
			new AdventureDungeonCrawlClassProgressSubkeys
			{
				bossWins = GameSaveKeySubkeyId.DUNGEON_CRAWL_PRIEST_BOSS_WINS,
				runWins = GameSaveKeySubkeyId.DUNGEON_CRAWL_PRIEST_RUN_WINS
			}
		},
		{
			TAG_CLASS.DRUID,
			new AdventureDungeonCrawlClassProgressSubkeys
			{
				bossWins = GameSaveKeySubkeyId.DUNGEON_CRAWL_DRUID_BOSS_WINS,
				runWins = GameSaveKeySubkeyId.DUNGEON_CRAWL_DRUID_RUN_WINS
			}
		},
		{
			TAG_CLASS.DEMONHUNTER,
			new AdventureDungeonCrawlClassProgressSubkeys
			{
				bossWins = GameSaveKeySubkeyId.DUNGEON_CRAWL_DEMON_HUNTER_BOSS_WINS,
				runWins = GameSaveKeySubkeyId.DUNGEON_CRAWL_DEMON_HUNTER_RUN_WINS
			}
		}
	};

	public static readonly Map<long, GameSaveKeySubkeyId> AdventureDungeonCrawlHeroToSubkeyMapping = new Map<long, GameSaveKeySubkeyId> { 
	{
		79338L,
		GameSaveKeySubkeyId.BOH_FAELIN_TOTAL_WINS
	} };

	private static readonly Map<int, GameSaveKeySubkeyId> AdventureDungeonCrawlGuestHeroToBossWinSubkeyMapping = new Map<int, GameSaveKeySubkeyId>
	{
		{
			162,
			GameSaveKeySubkeyId.PVPDR_DIABLO_BOSS_WINS
		},
		{
			329,
			GameSaveKeySubkeyId.PVPDR_VANNDAR_BOSS_WINS
		},
		{
			333,
			GameSaveKeySubkeyId.PVPDR_DREKTHAR_BOSS_WINS
		},
		{
			345,
			GameSaveKeySubkeyId.PVPDR_FINLEY_BOSS_WINS
		},
		{
			346,
			GameSaveKeySubkeyId.PVPDR_ELISE_BOSS_WINS
		},
		{
			347,
			GameSaveKeySubkeyId.PVPDR_BRANN_BOSS_WINS
		},
		{
			348,
			GameSaveKeySubkeyId.PVPDR_RENO_BOSS_WINS
		},
		{
			362,
			GameSaveKeySubkeyId.PVPDR_DARIOUS_BOSS_WINS
		},
		{
			363,
			GameSaveKeySubkeyId.PVPDR_SHAW_BOSS_WINS
		},
		{
			364,
			GameSaveKeySubkeyId.PVPDR_TESS_BOSS_WINS
		},
		{
			369,
			GameSaveKeySubkeyId.PVPDR_SAI_BOSS_WINS
		},
		{
			370,
			GameSaveKeySubkeyId.PVPDR_SCARLET_BOSS_WINS
		}
	};

	private static readonly List<AdventureDungeonCrawlWingProgressSubkeys> ProgressSubkeysForDungeonCrawlWings = new List<AdventureDungeonCrawlWingProgressSubkeys>
	{
		new AdventureDungeonCrawlWingProgressSubkeys
		{
			heroCardWins = GameSaveKeySubkeyId.DUNGEON_CRAWL_HERO_CARD_WING_1_WINS,
			deckWins = GameSaveKeySubkeyId.DUNGEON_CRAWL_DECK_WING_1_WINS,
			heroPowerWins = GameSaveKeySubkeyId.DUNGEON_CRAWL_HERO_POWER_WING_1_WINS,
			treasureWins = GameSaveKeySubkeyId.DUNGEON_CRAWL_TREASURE_WING_1_WINS
		},
		new AdventureDungeonCrawlWingProgressSubkeys
		{
			heroCardWins = GameSaveKeySubkeyId.DUNGEON_CRAWL_HERO_CARD_WING_2_WINS,
			deckWins = GameSaveKeySubkeyId.DUNGEON_CRAWL_DECK_WING_2_WINS,
			heroPowerWins = GameSaveKeySubkeyId.DUNGEON_CRAWL_HERO_POWER_WING_2_WINS,
			treasureWins = GameSaveKeySubkeyId.DUNGEON_CRAWL_TREASURE_WING_2_WINS
		},
		new AdventureDungeonCrawlWingProgressSubkeys
		{
			heroCardWins = GameSaveKeySubkeyId.DUNGEON_CRAWL_HERO_CARD_WING_3_WINS,
			deckWins = GameSaveKeySubkeyId.DUNGEON_CRAWL_DECK_WING_3_WINS,
			heroPowerWins = GameSaveKeySubkeyId.DUNGEON_CRAWL_HERO_POWER_WING_3_WINS,
			treasureWins = GameSaveKeySubkeyId.DUNGEON_CRAWL_TREASURE_WING_3_WINS
		},
		new AdventureDungeonCrawlWingProgressSubkeys
		{
			heroCardWins = GameSaveKeySubkeyId.DUNGEON_CRAWL_HERO_CARD_WING_4_WINS,
			deckWins = GameSaveKeySubkeyId.DUNGEON_CRAWL_DECK_WING_4_WINS,
			heroPowerWins = GameSaveKeySubkeyId.DUNGEON_CRAWL_HERO_POWER_WING_4_WINS,
			treasureWins = GameSaveKeySubkeyId.DUNGEON_CRAWL_TREASURE_WING_4_WINS
		},
		new AdventureDungeonCrawlWingProgressSubkeys
		{
			heroCardWins = GameSaveKeySubkeyId.DUNGEON_CRAWL_HERO_CARD_WING_5_WINS,
			deckWins = GameSaveKeySubkeyId.DUNGEON_CRAWL_DECK_WING_5_WINS,
			heroPowerWins = GameSaveKeySubkeyId.DUNGEON_CRAWL_HERO_POWER_WING_5_WINS,
			treasureWins = GameSaveKeySubkeyId.DUNGEON_CRAWL_TREASURE_WING_5_WINS
		}
	};

	private const float BATCHED_SAVE_SUBKEY_REQUEST_RATE = 1f;

	private static GameSaveDataManager s_instance = null;

	private Map<GameSaveKeyId, Map<GameSaveKeySubkeyId, GameSaveDataValue>> m_gameSaveDataMapByKey = new Map<GameSaveKeyId, Map<GameSaveKeySubkeyId, GameSaveDataValue>>();

	private Dictionary<GameSaveKeyId, bool> m_isRequestPendingForKey;

	private Dictionary<int, PendingRequestContext> m_pendingRequestsByClientToken = new Dictionary<int, PendingRequestContext>();

	private List<GameSaveDataUpdate> m_batchedSaveUpdates = new List<GameSaveDataUpdate>();

	private List<SubkeySaveRequest> m_batchedSubkeySaveRequests = new List<SubkeySaveRequest>();

	private List<OnSaveDataResponseDelegate> m_batchedSaveUpdateCallbacks = new List<OnSaveDataResponseDelegate>();

	private DateTime m_timeOfLastSetGameSaveDataRequest;

	public event OnGameSaveDataUpdateDelegate OnGameSaveDataUpdate;

	public static bool IsGameSaveKeyValid(GameSaveKeyId key)
	{
		if (GameSaveKeyId.INVALID != key)
		{
			return key != (GameSaveKeyId)0;
		}
		return false;
	}

	public bool IsDataReady(GameSaveKeyId key)
	{
		if (!IsGameSaveKeyValid(key))
		{
			Debug.LogWarning("GameSaveDataManager.IsDataReady() called with an invalid key ID!");
			return false;
		}
		return m_gameSaveDataMapByKey.ContainsKey(key);
	}

	public GameSaveDataManager()
	{
		Network.Get().RegisterNetHandler(GameSaveDataResponse.PacketID.ID, OnRequestGameSaveDataResponse);
		Network.Get().RegisterNetHandler(SetGameSaveDataResponse.PacketID.ID, OnSetGameSaveDataResponse);
		Network.Get().RegisterNetHandler(GameSaveDataStateUpdate.PacketID.ID, OnGameSaveDataStateUpdate);
		FatalErrorMgr.Get().AddErrorListener(OnFatalError);
		HandleGameSaveDataMigration();
		HearthstoneApplication.Get().WillReset += OnWillReset;
		m_timeOfLastSetGameSaveDataRequest = DateTime.Now;
	}

	private void OnRequestGameSaveDataResponse()
	{
		bool hasEncounteredErrors = false;
		GameSaveDataResponse gameSaveData = Network.Get().GetGameSaveDataResponse();
		if (gameSaveData.ErrorCode != 0)
		{
			Log.All.PrintError("GameSaveDataManager.OnRequestGameSaveDataResponse() - GameSaveDataResponse has error code {0} (error #{1})", gameSaveData.ErrorCode, (int)gameSaveData.ErrorCode);
			hasEncounteredErrors = true;
		}
		if (!hasEncounteredErrors)
		{
			ReadGameSaveDataUpdates(gameSaveData.Data);
		}
		if (m_pendingRequestsByClientToken.TryGetValue(gameSaveData.ClientToken, out var context))
		{
			m_pendingRequestsByClientToken.Remove(gameSaveData.ClientToken);
			if (context.RequestCallback != null)
			{
				context.RequestCallback(!hasEncounteredErrors);
			}
		}
	}

	private void OnSetGameSaveDataResponse()
	{
		bool hasEncounteredErrors = false;
		SetGameSaveDataResponse setDataResponse = Network.Get().GetSetGameSaveDataResponse();
		if (setDataResponse.ErrorCode != 0)
		{
			Log.All.PrintError("GameSaveDataManager.OnSetGameSaveDataResponse() - SetGameSaveDataResponse has error code {0}", setDataResponse.ErrorCode);
			hasEncounteredErrors = true;
		}
		if (!hasEncounteredErrors)
		{
			ReadGameSaveDataUpdates(setDataResponse.Data);
		}
		if (m_pendingRequestsByClientToken.TryGetValue(setDataResponse.ClientToken, out var context))
		{
			m_pendingRequestsByClientToken.Remove(setDataResponse.ClientToken);
			if (context.SaveCallback != null)
			{
				context.SaveCallback(!hasEncounteredErrors);
			}
		}
	}

	private void OnGameSaveDataStateUpdate()
	{
		GameSaveDataStateUpdate gameSaveDataStateUpdate = Network.Get().GetGameSaveDataStateUpdate();
		if (gameSaveDataStateUpdate == null)
		{
			Debug.LogError("OnGameSaveDataStateUpdate(): No response received.");
		}
		else
		{
			Get().ApplyGameSaveDataUpdate(gameSaveDataStateUpdate.GameSaveData);
		}
	}

	private void HandleGameSaveDataMigration()
	{
		List<SubkeySaveRequest> requests = new List<SubkeySaveRequest>();
		foreach (KeyValuePair<Option, ServerOptionFlagMigrationData> kvp in new Dictionary<Option, ServerOptionFlagMigrationData>
		{
			{
				Option.HAS_SEEN_LOOT_BOSS_HERO_POWER,
				new ServerOptionFlagMigrationData(GameSaveKeyId.ADVENTURE_DATA_CLIENT_LOOT, GameSaveKeySubkeyId.DUNGEON_CRAWL_BOSS_HERO_POWER_TUTORIAL_PROGRESS, 2)
			},
			{
				Option.HAS_SEEN_LOOT_COMPLETE_ALL_CLASSES_VO,
				new ServerOptionFlagMigrationData(GameSaveKeyId.ADVENTURE_DATA_CLIENT_LOOT, GameSaveKeySubkeyId.DUNGEON_CRAWL_HAS_SEEN_COMPLETE_ALL_CLASSES_VO)
			},
			{
				Option.HAS_SEEN_LATEST_DUNGEON_RUN_COMPLETE,
				new ServerOptionFlagMigrationData(GameSaveKeyId.ADVENTURE_DATA_SERVER_LOOT, GameSaveKeySubkeyId.DUNGEON_CRAWL_HAS_SEEN_LATEST_DUNGEON_RUN_COMPLETE)
			},
			{
				Option.HAS_SEEN_LOOT_CHARACTER_SELECT_VO,
				new ServerOptionFlagMigrationData(GameSaveKeyId.ADVENTURE_DATA_CLIENT_LOOT, GameSaveKeySubkeyId.DUNGEON_CRAWL_HAS_SEEN_CHARACTER_SELECT_VO)
			},
			{
				Option.HAS_SEEN_LOOT_WELCOME_BANNER_VO,
				new ServerOptionFlagMigrationData(GameSaveKeyId.ADVENTURE_DATA_CLIENT_LOOT, GameSaveKeySubkeyId.DUNGEON_CRAWL_HAS_SEEN_WELCOME_BANNER_VO)
			},
			{
				Option.HAS_SEEN_LOOT_BOSS_FLIP_1_VO,
				new ServerOptionFlagMigrationData(GameSaveKeyId.ADVENTURE_DATA_CLIENT_LOOT, GameSaveKeySubkeyId.DUNGEON_CRAWL_HAS_SEEN_BOSS_FLIP_1_VO)
			},
			{
				Option.HAS_SEEN_LOOT_BOSS_FLIP_2_VO,
				new ServerOptionFlagMigrationData(GameSaveKeyId.ADVENTURE_DATA_CLIENT_LOOT, GameSaveKeySubkeyId.DUNGEON_CRAWL_HAS_SEEN_BOSS_FLIP_2_VO)
			},
			{
				Option.HAS_SEEN_LOOT_BOSS_FLIP_3_VO,
				new ServerOptionFlagMigrationData(GameSaveKeyId.ADVENTURE_DATA_CLIENT_LOOT, GameSaveKeySubkeyId.DUNGEON_CRAWL_HAS_SEEN_BOSS_FLIP_3_VO)
			},
			{
				Option.HAS_SEEN_LOOT_OFFER_TREASURE_1_VO,
				new ServerOptionFlagMigrationData(GameSaveKeyId.ADVENTURE_DATA_CLIENT_LOOT, GameSaveKeySubkeyId.DUNGEON_CRAWL_HAS_SEEN_OFFER_TREASURE_1_VO)
			},
			{
				Option.HAS_SEEN_LOOT_OFFER_LOOT_PACKS_1_VO,
				new ServerOptionFlagMigrationData(GameSaveKeyId.ADVENTURE_DATA_CLIENT_LOOT, GameSaveKeySubkeyId.DUNGEON_CRAWL_HAS_SEEN_OFFER_LOOT_PACKS_1_VO)
			},
			{
				Option.HAS_SEEN_LOOT_OFFER_LOOT_PACKS_2_VO,
				new ServerOptionFlagMigrationData(GameSaveKeyId.ADVENTURE_DATA_CLIENT_LOOT, GameSaveKeySubkeyId.DUNGEON_CRAWL_HAS_SEEN_OFFER_LOOT_PACKS_2_VO)
			},
			{
				Option.HAS_SEEN_LOOT_IN_GAME_WIN_VO,
				new ServerOptionFlagMigrationData(GameSaveKeyId.ADVENTURE_DATA_CLIENT_LOOT, GameSaveKeySubkeyId.DUNGEON_CRAWL_HAS_SEEN_IN_GAME_WIN_VO)
			},
			{
				Option.HAS_SEEN_LOOT_IN_GAME_LOSE_VO,
				new ServerOptionFlagMigrationData(GameSaveKeyId.ADVENTURE_DATA_CLIENT_LOOT, GameSaveKeySubkeyId.DUNGEON_CRAWL_HAS_SEEN_IN_GAME_LOSE_VO)
			},
			{
				Option.HAS_SEEN_LOOT_IN_GAME_MULLIGAN_1_VO,
				new ServerOptionFlagMigrationData(GameSaveKeyId.ADVENTURE_DATA_CLIENT_LOOT, GameSaveKeySubkeyId.DUNGEON_CRAWL_HAS_SEEN_IN_GAME_MULLIGAN_1_VO)
			},
			{
				Option.HAS_SEEN_LOOT_IN_GAME_MULLIGAN_2_VO,
				new ServerOptionFlagMigrationData(GameSaveKeyId.ADVENTURE_DATA_CLIENT_LOOT, GameSaveKeySubkeyId.DUNGEON_CRAWL_HAS_SEEN_IN_GAME_MULLIGAN_2_VO)
			},
			{
				Option.HAS_SEEN_LOOT_IN_GAME_LOSE_2_VO,
				new ServerOptionFlagMigrationData(GameSaveKeyId.ADVENTURE_DATA_CLIENT_LOOT, GameSaveKeySubkeyId.DUNGEON_CRAWL_HAS_SEEN_IN_GAME_LOSE_2_VO)
			},
			{
				Option.HAS_SEEN_NAXX,
				new ServerOptionFlagMigrationData(GameSaveKeyId.ADVENTURE_DATA_CLIENT_NAXX, GameSaveKeySubkeyId.ADVENTURE_HAS_SEEN_ADVENTURE)
			},
			{
				Option.HAS_SEEN_BRM,
				new ServerOptionFlagMigrationData(GameSaveKeyId.ADVENTURE_DATA_CLIENT_BRM, GameSaveKeySubkeyId.ADVENTURE_HAS_SEEN_ADVENTURE)
			},
			{
				Option.HAS_SEEN_LOE,
				new ServerOptionFlagMigrationData(GameSaveKeyId.ADVENTURE_DATA_CLIENT_LOE, GameSaveKeySubkeyId.ADVENTURE_HAS_SEEN_ADVENTURE)
			},
			{
				Option.HAS_SEEN_KARA,
				new ServerOptionFlagMigrationData(GameSaveKeyId.ADVENTURE_DATA_CLIENT_KARA, GameSaveKeySubkeyId.ADVENTURE_HAS_SEEN_ADVENTURE)
			},
			{
				Option.HAS_SEEN_ICC,
				new ServerOptionFlagMigrationData(GameSaveKeyId.ADVENTURE_DATA_CLIENT_ICC, GameSaveKeySubkeyId.ADVENTURE_HAS_SEEN_ADVENTURE)
			},
			{
				Option.HAS_SEEN_LOOT,
				new ServerOptionFlagMigrationData(GameSaveKeyId.ADVENTURE_DATA_CLIENT_LOOT, GameSaveKeySubkeyId.ADVENTURE_HAS_SEEN_ADVENTURE)
			}
		})
		{
			Option option = kvp.Key;
			GameSaveKeyId gameSaveKey = kvp.Value.KeyId;
			GameSaveKeySubkeyId gameSaveSubkey = kvp.Value.SubkeyId;
			int trueValue = kvp.Value.FlagTrueValue;
			int falseValue = kvp.Value.FlagFalseValue;
			if (Options.Get().HasOption(option))
			{
				int value = (Options.Get().GetBool(option) ? trueValue : falseValue);
				requests.Add(new SubkeySaveRequest(gameSaveKey, gameSaveSubkey, value));
				Options.Get().DeleteOption(option);
			}
		}
		if (requests.Count > 0)
		{
			SaveSubkeys(requests);
		}
	}

	public void ApplyGameSaveDataUpdate(GameSaveDataUpdate gameSaveDataUpdate)
	{
		if (gameSaveDataUpdate != null && gameSaveDataUpdate.Tuple.Count > 0)
		{
			ReadGameSaveDataUpdates(new List<GameSaveDataUpdate> { gameSaveDataUpdate }, overrideExisting: false);
		}
	}

	public void ApplyGameSaveDataFromInitialClientState()
	{
		InitialClientState packet = Network.Get().GetInitialClientState();
		if (packet?.GameSaveData != null)
		{
			ReadGameSaveDataUpdates(packet.GameSaveData);
		}
	}

	private void ReadGameSaveDataUpdates(List<GameSaveDataUpdate> gameSaveDataUpdate, bool overrideExisting = true)
	{
		foreach (GameSaveDataUpdate keyUpdate in gameSaveDataUpdate)
		{
			if (keyUpdate.Tuple.Count < 1)
			{
				Log.All.PrintWarning("GameSaveDataManager.ReadGameSaveDataUpdates() - Received update that contains no key");
				continue;
			}
			GameSaveKeyId key = (GameSaveKeyId)keyUpdate.Tuple[0].Id;
			if (overrideExisting || !m_gameSaveDataMapByKey.ContainsKey(key) || m_gameSaveDataMapByKey[key] == null)
			{
				m_gameSaveDataMapByKey[key] = new Map<GameSaveKeySubkeyId, GameSaveDataValue>();
			}
			if (!keyUpdate.HasValue)
			{
				Log.All.Print("GameSaveDataManager.ReadGameSaveDataUpdates() - Received update that contains no data for the requested key {0}", key);
				continue;
			}
			if (keyUpdate.Value.MapKeys.Count == 0 && keyUpdate.Value.MapValues.Count == 0)
			{
				GameSaveKeySubkeyId subkey = (GameSaveKeySubkeyId)keyUpdate.Tuple[1].Id;
				m_gameSaveDataMapByKey[key][subkey] = keyUpdate.Value;
			}
			else
			{
				for (int i = 0; i < keyUpdate.Value.MapKeys.Count && i < keyUpdate.Value.MapValues.Count; i++)
				{
					GameSaveKeySubkeyId subkey2 = (GameSaveKeySubkeyId)keyUpdate.Value.MapKeys[i];
					m_gameSaveDataMapByKey[key][subkey2] = keyUpdate.Value.MapValues[i];
				}
			}
			this.OnGameSaveDataUpdate?.Invoke(key);
		}
	}

	private bool ValidateThereAreNoPendingRequestsForKey(GameSaveKeyId key, string loggingContext)
	{
		if (IsRequestPending(key))
		{
			Log.All.PrintError("GameSaveDataManager.{0}() - Detected pending operation for key {1}", loggingContext, key);
			return false;
		}
		return true;
	}

	public void Request(GameSaveKeyId key, OnRequestDataResponseDelegate callback = null)
	{
		Request(new List<GameSaveKeyId> { key }, callback);
	}

	public void Request(List<GameSaveKeyId> keys, OnRequestDataResponseDelegate callback = null)
	{
		List<long> keysToRequest = new List<long>();
		int clientToken = ++s_clientToken;
		foreach (GameSaveKeyId key in keys)
		{
			if (ValidateThereAreNoPendingRequestsForKey(key, "Request"))
			{
				keysToRequest.Add((long)key);
			}
		}
		if (keysToRequest.Count > 0)
		{
			m_pendingRequestsByClientToken.Add(clientToken, new PendingRequestContext(keys, callback));
			Network.Get().RequestGameSaveData(keysToRequest, clientToken);
		}
		else
		{
			callback?.Invoke(success: false);
		}
	}

	public bool SaveSubkey(SubkeySaveRequest request, OnSaveDataResponseDelegate callback = null)
	{
		List<SubkeySaveRequest> requests = new List<SubkeySaveRequest>();
		requests.Add(request);
		return SaveSubkeys(requests, callback);
	}

	public bool SaveSubkeys(List<SubkeySaveRequest> requests, OnSaveDataResponseDelegate callback = null)
	{
		if (requests == null || requests.Count == 0)
		{
			Log.All.PrintError("GameSaveDataManager.SaveSubkeys() - No save requests specified");
			return false;
		}
		HashSet<GameSaveKeyTuple> tuplesEncountered = new HashSet<GameSaveKeyTuple>();
		foreach (SubkeySaveRequest request in requests)
		{
			GameSaveKeyTuple tuple = new GameSaveKeyTuple(request.Key, request.Subkey);
			if (tuplesEncountered.Contains(tuple))
			{
				Log.All.PrintError("GameSaveDataManager.SaveSubkeys() - Found multiple save requests for key {0} subkey {1}", request.Key, request.Subkey);
				return false;
			}
			tuplesEncountered.Add(tuple);
		}
		List<GameSaveDataUpdate> updates = new List<GameSaveDataUpdate>();
		foreach (SubkeySaveRequest request2 in requests)
		{
			if (ValidateThereAreNoPendingRequestsForKey(request2.Key, "SaveSubkeys"))
			{
				GameSaveDataUpdate update = new GameSaveDataUpdate();
				GameSaveDataValue updateValue = new GameSaveDataValue();
				update.Tuple.Add(new GameSaveKey
				{
					Id = (long)request2.Key
				});
				update.Tuple.Add(new GameSaveKey
				{
					Id = (long)request2.Subkey
				});
				SetGameSaveDataValueFromRequest(request2, ref updateValue);
				update.Value = updateValue;
				updates.Add(update);
				SaveSubkeyToLocalCache(request2);
				m_batchedSubkeySaveRequests.Add(request2);
			}
		}
		if (callback != null && updates.Count > 0)
		{
			m_batchedSaveUpdateCallbacks.Add(callback);
		}
		BatchGameSaveUpdates(updates);
		return updates.Count > 0;
	}

	private void SetGameSaveDataValueFromRequest(SubkeySaveRequest request, ref GameSaveDataValue value)
	{
		if (request.Long_Values != null && request.String_Values != null)
		{
			Log.All.PrintError("Error writing game save data: Attempting to write Long and String into the same key!");
		}
		else if (request.Long_Values != null)
		{
			value.IntValue = request.Long_Values.ToList();
		}
		else if (request.String_Values != null)
		{
			value.StringValue = request.String_Values.ToList();
		}
	}

	private void BatchGameSaveUpdates(List<GameSaveDataUpdate> saveUpdates)
	{
		if (m_batchedSaveUpdates.Count == 0)
		{
			if ((DateTime.Now - m_timeOfLastSetGameSaveDataRequest).TotalSeconds > 1.0)
			{
				Processor.RunCoroutine(SendAllBatchedGameSaveUpdatesNextFrame());
			}
			else
			{
				Processor.ScheduleCallback(1f, realTime: false, SendAllBatchedGameSaveDataUpdates);
			}
		}
		foreach (GameSaveDataUpdate update in saveUpdates)
		{
			GameSaveDataUpdate batchedUpdateWithSameKeyAndSubkey = m_batchedSaveUpdates.FirstOrDefault((GameSaveDataUpdate u) => u.Tuple[0].Id == update.Tuple[0].Id && u.Tuple[1].Id == update.Tuple[1].Id);
			if (batchedUpdateWithSameKeyAndSubkey != null)
			{
				m_batchedSaveUpdates.Remove(batchedUpdateWithSameKeyAndSubkey);
			}
			m_batchedSaveUpdates.Add(update);
		}
	}

	public SubkeySaveRequest GenerateSaveRequestToAddValuesToSubkeyIfTheyDoNotExist(GameSaveKeyId key, GameSaveKeySubkeyId subkeyId, List<long> valuesToAdd)
	{
		if (valuesToAdd == null)
		{
			return null;
		}
		GetSubkeyValue(key, subkeyId, out List<long> currentValues);
		if (currentValues == null)
		{
			currentValues = new List<long>();
		}
		bool valueAdded = false;
		foreach (long value in valuesToAdd)
		{
			if (!currentValues.Contains(value))
			{
				currentValues.Add(value);
				valueAdded = true;
			}
		}
		if (!valueAdded)
		{
			return null;
		}
		return new SubkeySaveRequest(key, subkeyId, currentValues.ToArray());
	}

	public SubkeySaveRequest GenerateSaveRequestToRemoveValueFromSubkeyIfItExists(GameSaveKeyId key, GameSaveKeySubkeyId subkeyId, long valueToRemove)
	{
		if (!GetSubkeyValue(key, subkeyId, out List<long> currentValues))
		{
			return null;
		}
		if (currentValues == null)
		{
			return null;
		}
		if (!currentValues.Remove(valueToRemove))
		{
			return null;
		}
		return new SubkeySaveRequest(key, subkeyId, currentValues.ToArray());
	}

	private IEnumerator SendAllBatchedGameSaveUpdatesNextFrame()
	{
		yield return new WaitForEndOfFrame();
		SendAllBatchedGameSaveDataUpdates(null);
	}

	private void SendAllBatchedGameSaveDataUpdates(object userdata)
	{
		int clientToken = ++s_clientToken;
		foreach (OnSaveDataResponseDelegate callback in m_batchedSaveUpdateCallbacks)
		{
			m_pendingRequestsByClientToken.Add(clientToken, new PendingRequestContext(m_batchedSubkeySaveRequests, callback));
		}
		Network.Get().SetGameSaveData(m_batchedSaveUpdates, clientToken);
		m_timeOfLastSetGameSaveDataRequest = DateTime.Now;
		m_batchedSaveUpdates.Clear();
		m_batchedSubkeySaveRequests.Clear();
		m_batchedSaveUpdateCallbacks.Clear();
	}

	private void SaveSubkeyToLocalCache(SubkeySaveRequest request)
	{
		if (!m_gameSaveDataMapByKey.TryGetValue(request.Key, out var data))
		{
			data = new Map<GameSaveKeySubkeyId, GameSaveDataValue>();
			m_gameSaveDataMapByKey.Add(request.Key, data);
		}
		if (!data.TryGetValue(request.Subkey, out var value))
		{
			value = new GameSaveDataValue();
			data.Add(request.Subkey, value);
		}
		SetGameSaveDataValueFromRequest(request, ref value);
	}

	public bool GetSubkeyValue(GameSaveKeyId key, GameSaveKeySubkeyId subkeyId, out long value)
	{
		value = 0L;
		if (GetSubkeyValue(key, subkeyId, out List<long> tempValues))
		{
			value = tempValues[0];
			return true;
		}
		return false;
	}

	public bool GetSubkeyValue(GameSaveKeyId key, GameSaveKeySubkeyId subkeyId, out string value)
	{
		value = "";
		if (GetSubkeyValue(key, subkeyId, out List<string> tempValues))
		{
			value = tempValues[0];
			return true;
		}
		return false;
	}

	public bool GetSubkeyValue(GameSaveKeyId key, GameSaveKeySubkeyId subkeyId, out List<long> values)
	{
		values = null;
		GameSaveDataValue value = GetSubkeyValue(key, subkeyId);
		if (value != null && value.IntValue.Count > 0)
		{
			values = new List<long>(value.IntValue);
			return true;
		}
		return false;
	}

	public bool GetSubkeyValue(GameSaveKeyId key, GameSaveKeySubkeyId subkeyId, List<long> values)
	{
		if (values == null)
		{
			values = new List<long>();
		}
		values.Clear();
		GameSaveDataValue value = GetSubkeyValue(key, subkeyId);
		if (value != null && value.IntValue.Count > 0)
		{
			values.AddRange(value.IntValue);
			return true;
		}
		return false;
	}

	public bool GetSubkeyValue(GameSaveKeyId key, GameSaveKeySubkeyId subkeyId, out List<string> values)
	{
		values = null;
		GameSaveDataValue value = GetSubkeyValue(key, subkeyId);
		if (value != null && value.StringValue.Count > 0)
		{
			values = new List<string>(value.StringValue);
			return true;
		}
		return false;
	}

	public bool GetSubkeyValue(GameSaveKeyId key, GameSaveKeySubkeyId subkeyId, out List<double> values)
	{
		values = null;
		GameSaveDataValue value = GetSubkeyValue(key, subkeyId);
		if (value != null && value.FloatValue.Count > 0)
		{
			values = new List<double>(value.FloatValue);
			return true;
		}
		return false;
	}

	public List<GameSaveKeySubkeyId> GetAllSubkeysForKey(GameSaveKeyId key)
	{
		List<GameSaveKeySubkeyId> subkeys = new List<GameSaveKeySubkeyId>();
		Map<GameSaveKeySubkeyId, GameSaveDataValue> map = null;
		if (m_gameSaveDataMapByKey.TryGetValue(key, out map))
		{
			subkeys = new List<GameSaveKeySubkeyId>(map.Keys);
		}
		return subkeys;
	}

	public void ClearLocalData(GameSaveKeyId key)
	{
		if (ValidateThereAreNoPendingRequestsForKey(key, "ClearLocalData"))
		{
			m_gameSaveDataMapByKey.Remove(key);
		}
	}

	public bool ValidateIfKeyCanBeAccessed(GameSaveKeyId key, string loggingContext)
	{
		if (!IsGameSaveKeyValid(key))
		{
			Log.All.PrintWarning("GameSaveDataManager.ValidateKeyCanBeAccessed() called with invalid key ID {0}!  Context: {1}\nStack Trace:\n{2}", key, loggingContext, StackTraceUtility.ExtractStackTrace());
			return false;
		}
		if (IsRequestPending(key))
		{
			Log.All.PrintWarning("GameSaveDataManager.ValidateKeyCanBeAccessed() - Request for key {0} is pending!  Context: {1}\nStack Trace:\n{2}", key, loggingContext, StackTraceUtility.ExtractStackTrace());
			return false;
		}
		if (!IsDataReady(key))
		{
			Log.All.Print("GameSaveDataManager.ValidateKeyCanBeAccessed() - Key {0} has no data - it has either not been created yet, or has not been requested.  Context: {1}\nStack Trace:\n{2}", key, loggingContext, StackTraceUtility.ExtractStackTrace());
			return false;
		}
		return true;
	}

	public bool IsRequestPending(GameSaveKeyId key)
	{
		foreach (PendingRequestContext value in m_pendingRequestsByClientToken.Values)
		{
			if (value.AffectedKeys.IndexOf(key) >= 0)
			{
				return true;
			}
		}
		return false;
	}

	public static GameSaveDataManager Get()
	{
		if (s_instance == null)
		{
			s_instance = new GameSaveDataManager();
		}
		return s_instance;
	}

	public static bool GetProgressSubkeysForDungeonCrawlWing(WingDbfRecord wingRecord, out AdventureDungeonCrawlWingProgressSubkeys progressSubkeys)
	{
		if (wingRecord == null)
		{
			Log.Adventures.PrintWarning("GetProgressSubkeysForDungeonCrawlWing: wingRecord is null!");
			progressSubkeys = default(AdventureDungeonCrawlWingProgressSubkeys);
			return false;
		}
		int wingIndex = GameUtils.GetSortedWingUnlockIndex(wingRecord);
		if (wingIndex < 0 || wingIndex >= ProgressSubkeysForDungeonCrawlWings.Count)
		{
			Log.Adventures.PrintWarning("GetProgressSubkeysForDungeonCrawlWing: could not find a valid Sorted Wing Unlock Index for WingDbfRecord {0} - WingIndex: {1}!", wingRecord.ID, wingIndex);
			progressSubkeys = default(AdventureDungeonCrawlWingProgressSubkeys);
			return false;
		}
		progressSubkeys = ProgressSubkeysForDungeonCrawlWings[wingIndex];
		return true;
	}

	public static bool GetProgressSubkeyForDungeonCrawlClass(TAG_CLASS tagClass, out AdventureDungeonCrawlClassProgressSubkeys progressSubkeys)
	{
		if (AdventureDungeonCrawlClassToSubkeyMapping.ContainsKey(tagClass))
		{
			progressSubkeys = AdventureDungeonCrawlClassToSubkeyMapping[tagClass];
			return true;
		}
		progressSubkeys = default(AdventureDungeonCrawlClassProgressSubkeys);
		return false;
	}

	public static bool GetBossWinsSubkeyForDungeonCrawlGuestHero(int guestHeroId, out GameSaveKeySubkeyId bossWinsSubkey)
	{
		if (AdventureDungeonCrawlGuestHeroToBossWinSubkeyMapping.ContainsKey(guestHeroId))
		{
			bossWinsSubkey = AdventureDungeonCrawlGuestHeroToBossWinSubkeyMapping[guestHeroId];
			return true;
		}
		bossWinsSubkey = GameSaveKeySubkeyId.INVALID;
		return false;
	}

	public static List<TAG_CLASS> GetClassesFromDungeonCrawlProgressMap()
	{
		return AdventureDungeonCrawlClassToSubkeyMapping.Keys.ToList();
	}

	public static List<long> GetHeroCardIdFromDungeonCrawlProgressMap()
	{
		return AdventureDungeonCrawlHeroToSubkeyMapping.Keys.ToList();
	}

	public void Cheat_SaveSubkeyToLocalCache(GameSaveKeyId key, GameSaveKeySubkeyId subkey, params long[] values)
	{
		if (HearthstoneApplication.IsInternal())
		{
			SubkeySaveRequest request = new SubkeySaveRequest(key, subkey, values);
			SaveSubkeyToLocalCache(request);
		}
	}

	public bool MigrateSubkeyIntValue(GameSaveKeyId sourceKey, GameSaveKeyId destinationKey, GameSaveKeySubkeyId subkeyId, long emptyValueForSource = 0L)
	{
		GameSaveDataValue value = GetSubkeyValue(sourceKey, subkeyId);
		if (value == null || value.IntValue == null || value.IntValue.Count < 1 || value.IntValue[0] == emptyValueForSource)
		{
			return false;
		}
		long valueToMigrate = value.IntValue[0];
		List<SubkeySaveRequest> subkeySaveRequests = new List<SubkeySaveRequest>();
		subkeySaveRequests.Add(new SubkeySaveRequest(destinationKey, subkeyId, valueToMigrate));
		subkeySaveRequests.Add(new SubkeySaveRequest(sourceKey, subkeyId, emptyValueForSource));
		return SaveSubkeys(subkeySaveRequests);
	}

	private GameSaveDataValue GetSubkeyValue(GameSaveKeyId key, GameSaveKeySubkeyId subkeyId)
	{
		if (!IsDataReady(key))
		{
			Debug.LogWarningFormat("Attempting to get subkey {0} from key {1} failed, key not received by client yet", subkeyId, key);
			return null;
		}
		if (m_gameSaveDataMapByKey.TryGetValue(key, out var data) && data != null && data.TryGetValue(subkeyId, out var value))
		{
			return value;
		}
		return null;
	}

	private static void OnWillReset()
	{
		HearthstoneApplication.Get().WillReset -= OnWillReset;
		s_instance = new GameSaveDataManager();
	}

	private void OnFatalError(FatalErrorMessage message, object userData)
	{
		m_pendingRequestsByClientToken.Clear();
	}
}
