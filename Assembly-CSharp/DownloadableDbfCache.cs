using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Blizzard.T5.Core;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using Hearthstone.Core;
using Hearthstone.Util;
using PegasusShared;
using PegasusUtil;
using UnityEngine;

public class DownloadableDbfCache : IService
{
	public delegate void LoadCachedAssetCallback(AssetKey requestedKey, ErrorCode code, byte[] assetBytes);

	private Map<int, KeyValuePair<AssetRecordInfo, LoadCachedAssetCallback>> m_assetRequests = new Map<int, KeyValuePair<AssetRecordInfo, LoadCachedAssetCallback>>();

	private HashSet<AssetKey> m_requiredClientStaticAssetsStillPending = new HashSet<AssetKey>();

	private int m_nextCallbackToken = -1;

	public bool IsRequiredClientStaticAssetsStillPending
	{
		get
		{
			if (NetCache.Get().GetNetObject<ClientStaticAssetsResponse>() == null)
			{
				return true;
			}
			return m_requiredClientStaticAssetsStillPending.Count > 0;
		}
	}

	private int NextCallbackToken => ++m_nextCallbackToken;

	public IEnumerator<IAsyncJobResult> Initialize(ServiceLocator serviceLocator)
	{
		serviceLocator.Get<Network>().RegisterNetHandler(GetAssetResponse.PacketID.ID, Network_OnGetAssetResponse);
		serviceLocator.Get<NetCache>().RegisterUpdatedListener(typeof(ClientStaticAssetsResponse), NetCache_OnClientStaticAssetsResponse);
		yield break;
	}

	public Type[] GetDependencies()
	{
		return new Type[2]
		{
			typeof(Network),
			typeof(NetCache)
		};
	}

	public void Shutdown()
	{
	}

	public static DownloadableDbfCache Get()
	{
		return ServiceManager.Get<DownloadableDbfCache>();
	}

	public bool IsAssetRequestInProgress(int assetId, AssetType assetType)
	{
		if (m_assetRequests.Any((KeyValuePair<int, KeyValuePair<AssetRecordInfo, LoadCachedAssetCallback>> kv) => kv.Value.Key.Asset.AssetId == assetId && kv.Value.Key.Asset.Type == assetType))
		{
			return true;
		}
		return false;
	}

	public bool LoadCachedAssets(bool canRequestFromServer, LoadCachedAssetCallback cb, params AssetRecordInfo[] assets)
	{
		if (assets.Length == 0)
		{
			return false;
		}
		List<AssetKey> requestKeys = new List<AssetKey>();
		byte[] firstLoadedAsset = null;
		foreach (AssetRecordInfo info in assets)
		{
			if (info == null)
			{
				continue;
			}
			if (info.RecordHash == null)
			{
				if (info.RecordByteSize == 0)
				{
					requestKeys.Add(info.Asset);
				}
				continue;
			}
			bool requestFromServer = false;
			string filePath = GetCachedAssetFilePath(info.Asset.Type, info.Asset.AssetId, info.RecordHash);
			if (!File.Exists(filePath))
			{
				requestFromServer = info.RecordByteSize != 0;
				if (!requestFromServer)
				{
					m_requiredClientStaticAssetsStillPending.Remove(info.Asset);
				}
				try
				{
					Directory.CreateDirectory(GetCachedAssetFolder(info.Asset.Type));
				}
				catch (Exception ex)
				{
					Error.AddDevFatal("Error creating cached asset folder {0}:\n{1}", filePath, ex.ToString());
					return false;
				}
			}
			else
			{
				try
				{
					if (new FileInfo(filePath).Length != info.RecordByteSize)
					{
						requestFromServer = true;
					}
					else if (info.RecordByteSize != 0)
					{
						byte[] assetBytes = File.ReadAllBytes(filePath);
						if (GeneralUtils.AreArraysEqual(SHA1.Create().ComputeHash(assetBytes, 0, assetBytes.Length), info.RecordHash))
						{
							Log.Downloader.Print("LoadCachedAsset: locally available=true {0} id={1} hash={2}", info.Asset.Type, info.Asset.AssetId, (info.RecordHash == null) ? "<null>" : info.RecordHash.ToHexString());
							if (firstLoadedAsset == null)
							{
								firstLoadedAsset = assetBytes;
							}
							SetCachedAssetIntoDbfSystem(info.Asset.Type, assetBytes);
							m_requiredClientStaticAssetsStillPending.Remove(info.Asset);
						}
						else
						{
							requestFromServer = true;
						}
					}
				}
				catch (Exception ex2)
				{
					Error.AddDevFatal("Error reading cached asset folder {0}:\n{1}", filePath, ex2.ToString());
					requestKeys.Add(info.Asset);
				}
			}
			if (requestFromServer)
			{
				requestKeys.Add(info.Asset);
				if (canRequestFromServer)
				{
					Log.Downloader.Print("LoadCachedAsset: locally available=false, requesting from server {0} id={1} hash={2}", info.Asset.Type, info.Asset.AssetId, (info.RecordHash == null) ? "<null>" : info.RecordHash.ToHexString());
				}
				else
				{
					Log.Downloader.Print("LoadCachedAsset: locally available=false, not requesting from server yet - {0} id={1} hash={2}", info.Asset.Type, info.Asset.AssetId, (info.RecordHash == null) ? "<null>" : info.RecordHash.ToHexString());
				}
			}
		}
		AssetRecordInfo firstInfo = assets[0];
		if (requestKeys.Count > 0)
		{
			if (canRequestFromServer)
			{
				int clientToken = NextCallbackToken;
				if (cb != null)
				{
					m_assetRequests[clientToken] = new KeyValuePair<AssetRecordInfo, LoadCachedAssetCallback>(firstInfo, cb);
				}
				Network.Get().SendAssetRequest(clientToken, requestKeys);
			}
		}
		else if (firstInfo != null && cb != null)
		{
			if (firstLoadedAsset == null)
			{
				firstLoadedAsset = new byte[0];
			}
			cb(firstInfo.Asset, ErrorCode.ERROR_OK, firstLoadedAsset);
		}
		return requestKeys.Count == 0;
	}

	private static string GetCachedAssetFolder(AssetType assetType)
	{
		string subFolder = assetType switch
		{
			AssetType.ASSET_TYPE_SCENARIO => "Scenario", 
			AssetType.ASSET_TYPE_SUBSET_CARD => "Subset", 
			AssetType.ASSET_TYPE_DECK_RULESET => "DeckRuleset", 
			_ => "Other", 
		};
		string baseFolder = PlatformFilePaths.CachePath;
		return $"{baseFolder}/{subFolder}";
	}

	private static string GetCachedAssetFileExtension(AssetType assetType)
	{
		return assetType switch
		{
			AssetType.ASSET_TYPE_SCENARIO => "scen", 
			AssetType.ASSET_TYPE_SUBSET_CARD => "subset_card", 
			AssetType.ASSET_TYPE_DECK_RULESET => "deck_ruleset", 
			_ => assetType.ToString().Replace("ASSET_TYPE_", "").ToLower(), 
		};
	}

	private static string GetCachedAssetFilePath(AssetType assetType, int assetId, byte[] assetHash)
	{
		string folder = GetCachedAssetFolder(assetType);
		string extension = GetCachedAssetFileExtension(assetType);
		return $"{folder}/{assetId}_{assetHash.ToHexString()}.{extension}";
	}

	private static void StoreReceivedAssetIntoLocalCache(AssetType assetType, int assetId, byte[] assetBytes, int assetBytesLength)
	{
		byte[] hash = SHA1.Create().ComputeHash(assetBytes, 0, assetBytesLength);
		string filePath = GetCachedAssetFilePath(assetType, assetId, hash);
		try
		{
			if (!File.Exists(filePath))
			{
				File.Create(filePath).Dispose();
			}
			using FileStream stream = new FileStream(filePath, FileMode.Truncate);
			stream.Write(assetBytes, 0, assetBytesLength);
		}
		catch (Exception ex)
		{
			Error.AddDevFatal("Error saving cached asset {0}:\n{1}", filePath, ex.ToString());
		}
	}

	private static void SetCachedAssetIntoDbfSystem(AssetType assetType, byte[] assetBytes)
	{
		switch (assetType)
		{
		case AssetType.ASSET_TYPE_SCENARIO:
			SetCachedAssetIntoDbfSystem_Scenario(ProtobufUtil.ParseFrom<ScenarioDbRecord>(assetBytes, 0, assetBytes.Length));
			break;
		case AssetType.ASSET_TYPE_DECK_RULESET:
			SetCachedAssetIntoDbfSystem_DeckRuleset(ProtobufUtil.ParseFrom<DeckRulesetDbRecord>(assetBytes, 0, assetBytes.Length));
			break;
		case AssetType.ASSET_TYPE_SUBSET_CARD:
			SetCachedAssetIntoDbfSystem_SubsetCard(ProtobufUtil.ParseFrom<SubsetCardListDbRecord>(assetBytes, 0, assetBytes.Length));
			break;
		case AssetType.ASSET_TYPE_REWARD_CHEST:
			SetCachedAssetIntoDbfSystem_RewardChest(ProtobufUtil.ParseFrom<RewardChestDbRecord>(assetBytes, 0, assetBytes.Length));
			break;
		case AssetType.ASSET_TYPE_GUEST_HEROES:
			SetCachedAssetIntoDbfSystem_GuestHero(ProtobufUtil.ParseFrom<GuestHeroDbRecord>(assetBytes, 0, assetBytes.Length));
			break;
		default:
			Debug.LogError("DownloadableDbfCache:SetCachedAssetIntoDbfSystem received an unsupported asset type: " + assetType);
			break;
		}
	}

	private static void SetCachedAssetIntoDbfSystem_Scenario(ScenarioDbRecord protoScenario)
	{
		List<ScenarioGuestHeroesDbfRecord> outScenarioGuestHeroRecords;
		List<ClassExclusionsDbfRecord> outClassExclusionsRecords;
		ScenarioDbfRecord dbf = DbfUtils.ConvertFromProtobuf(protoScenario, out outScenarioGuestHeroRecords, out outClassExclusionsRecords);
		if (dbf == null)
		{
			Log.Downloader.Print("DbfUtils.ConvertFromProtobuf(protoScenario) returned null:\n{0}", (protoScenario == null) ? "(null)" : protoScenario.ToString());
			return;
		}
		GameDbf.Scenario.ReplaceRecordByRecordId(dbf);
		int dbfId = dbf.ID;
		GameDbf.ScenarioGuestHeroes.RemoveRecordsWhere((ScenarioGuestHeroesDbfRecord r) => r.ScenarioId == dbfId);
		foreach (ScenarioGuestHeroesDbfRecord heroRecord in outScenarioGuestHeroRecords)
		{
			GameDbf.ScenarioGuestHeroes.AddRecord(heroRecord);
		}
		GameDbf.ClassExclusions.RemoveRecordsWhere((ClassExclusionsDbfRecord r) => r.ScenarioId == dbfId);
		foreach (ClassExclusionsDbfRecord classExclusionRecord in outClassExclusionsRecords)
		{
			GameDbf.ClassExclusions.AddRecord(classExclusionRecord);
		}
	}

	private static void SetCachedAssetIntoDbfSystem_DeckRuleset(DeckRulesetDbRecord proto)
	{
		DeckRulesetDbfRecord dbf = DbfUtils.ConvertFromProtobuf(proto);
		if (dbf == null)
		{
			Log.Downloader.Print("DbfUtils.ConvertFromProtobuf(proto) returned null:\n{0}", (proto == null) ? "(null)" : proto.ToString());
		}
		else
		{
			GameDbf.DeckRuleset.ReplaceRecordByRecordId(dbf);
		}
		foreach (DeckRulesetRuleDbRecord rule in proto.Rules)
		{
			List<int> targetSubsetIds;
			DeckRulesetRuleDbfRecord dbfRule = DbfUtils.ConvertFromProtobuf(rule, out targetSubsetIds);
			GameDbf.DeckRulesetRule.ReplaceRecordByRecordId(dbfRule);
			int dbfRuleID = dbfRule.ID;
			GameDbf.DeckRulesetRuleSubset.RemoveRecordsWhere((DeckRulesetRuleSubsetDbfRecord r) => r.DeckRulesetRuleId == dbfRuleID);
			if (targetSubsetIds != null)
			{
				for (int i = 0; i < targetSubsetIds.Count; i++)
				{
					DeckRulesetRuleSubsetDbfRecord dbfTargetSubset = new DeckRulesetRuleSubsetDbfRecord();
					dbfTargetSubset.SetDeckRulesetRuleId(dbfRuleID);
					dbfTargetSubset.SetSubsetId(targetSubsetIds[i]);
					GameDbf.DeckRulesetRuleSubset.AddRecord(dbfTargetSubset);
				}
			}
		}
	}

	private static void SetCachedAssetIntoDbfSystem_SubsetCard(SubsetCardListDbRecord proto)
	{
		SubsetDbfRecord dbf = GameDbf.Subset.GetRecord(proto.SubsetId);
		if (dbf == null)
		{
			dbf = new SubsetDbfRecord();
			dbf.SetID(proto.SubsetId);
			GameDbf.Subset.AddRecord(dbf);
		}
		int dbfID = dbf.ID;
		GameDbf.SubsetCard.RemoveRecordsWhere((SubsetCardDbfRecord r) => r.SubsetId == dbfID);
		foreach (int cardAssetId in proto.CardIds)
		{
			SubsetCardDbfRecord dbfSubsetCard = new SubsetCardDbfRecord();
			dbfSubsetCard.SetSubsetId(dbfID);
			dbfSubsetCard.SetCardId(cardAssetId);
			GameDbf.SubsetCard.AddRecord(dbfSubsetCard);
		}
	}

	private static void SetCachedAssetIntoDbfSystem_RewardChest(RewardChestDbRecord proto)
	{
		RewardChestDbfRecord dbf = DbfUtils.ConvertFromProtobuf(proto);
		if (dbf == null)
		{
			Log.Downloader.Print("DbfUtils.ConvertFromProtobuf(RewardChestDbRecord) returned null:\n{0}", (proto == null) ? "(null)" : proto.ToString());
		}
		else
		{
			GameDbf.RewardChest.ReplaceRecordByRecordId(dbf);
		}
	}

	private static void SetCachedAssetIntoDbfSystem_GuestHero(GuestHeroDbRecord proto)
	{
		GuestHeroDbfRecord dbf = DbfUtils.ConvertFromProtobuf(proto);
		if (dbf == null)
		{
			Log.Downloader.Print("DbfUtils.ConvertFromProtobuf(GuestHeroDbfRecord) returned null:\n{0}", (proto == null) ? "(null)" : proto.ToString());
		}
		else
		{
			GameDbf.GuestHero.ReplaceRecordByRecordId(dbf);
		}
	}

	private void NetCache_OnClientStaticAssetsResponse()
	{
		ClientStaticAssetsResponse packet = NetCache.Get().GetNetObject<ClientStaticAssetsResponse>();
		if (packet == null)
		{
			return;
		}
		foreach (AssetRecordInfo info in packet.AssetsToGet)
		{
			m_requiredClientStaticAssetsStillPending.Add(info.Asset);
		}
		LoadCachedAssets(canRequestFromServer: true, null, packet.AssetsToGet.ToArray());
	}

	private void Network_OnGetAssetResponse()
	{
		GetAssetResponse packet = Network.Get().GetAssetResponse();
		if (packet == null)
		{
			return;
		}
		ErrorCode overallResult = ErrorCode.ERROR_OK;
		Map<AssetKey, byte[]> retrievedAssets = new Map<AssetKey, byte[]>();
		for (int i = 0; i < packet.Responses.Count; i++)
		{
			AssetResponse response = packet.Responses[i];
			if (response.ErrorCode == ErrorCode.ERROR_OK)
			{
				m_requiredClientStaticAssetsStillPending.Remove(response.RequestedKey);
			}
			else
			{
				Log.Downloader.Print("Network_OnGetAssetResponse: error={0}:{1} type={2}:{3} id={4}", (int)response.ErrorCode, response.ErrorCode.ToString(), (int)response.RequestedKey.Type, response.RequestedKey.Type.ToString(), response.RequestedKey.AssetId);
				if (overallResult == ErrorCode.ERROR_OK)
				{
					overallResult = response.ErrorCode;
				}
				if (m_requiredClientStaticAssetsStillPending.Contains(response.RequestedKey))
				{
					Error.AddDevFatal(GameStrings.Get("GLUE_REQUIRED_CLIENT_STATIC_ASSETS_ERROR_MESSAGE"));
					return;
				}
			}
			AssetKey key = response.RequestedKey;
			byte[] assetBytes = null;
			if (response.HasScenarioAsset)
			{
				assetBytes = ProtobufUtil.ToByteArray(response.ScenarioAsset);
			}
			if (response.HasSubsetCardListAsset)
			{
				assetBytes = ProtobufUtil.ToByteArray(response.SubsetCardListAsset);
			}
			if (response.HasDeckRulesetAsset)
			{
				assetBytes = ProtobufUtil.ToByteArray(response.DeckRulesetAsset);
			}
			if (response.HasRewardChestAsset)
			{
				assetBytes = ProtobufUtil.ToByteArray(response.RewardChestAsset);
			}
			if (response.HasGuestHeroAsset)
			{
				assetBytes = ProtobufUtil.ToByteArray(response.GuestHeroAsset);
			}
			if (assetBytes != null)
			{
				retrievedAssets[key] = assetBytes;
				StoreReceivedAssetIntoLocalCache(key.Type, key.AssetId, assetBytes, assetBytes.Length);
				SetCachedAssetIntoDbfSystem(key.Type, assetBytes);
			}
		}
		Processor.CancelScheduledCallback(PruneCachedAssetFiles);
		Processor.ScheduleCallback(5f, realTime: true, PruneCachedAssetFiles);
		if (!m_assetRequests.TryGetValue(packet.ClientToken, out var assetCb))
		{
			return;
		}
		AssetRecordInfo info = assetCb.Key;
		LoadCachedAssetCallback cb = assetCb.Value;
		m_assetRequests.Remove(packet.ClientToken);
		if (!retrievedAssets.TryGetValue(info.Asset, out var assetBytes2))
		{
			if (LoadCachedAssets(false, cb, info))
			{
				return;
			}
			assetBytes2 = new byte[0];
		}
		cb(info.Asset, overallResult, assetBytes2);
	}

	private static void PruneCachedAssetFiles(object userData)
	{
		string baseFolder = PlatformFilePaths.CachePath;
		string curFolder = null;
		string curFile = null;
		try
		{
			DirectoryInfo dir = new DirectoryInfo(baseFolder);
			if (!dir.Exists)
			{
				return;
			}
			DirectoryInfo[] directories = dir.GetDirectories();
			foreach (DirectoryInfo obj in directories)
			{
				curFolder = obj.FullName;
				FileInfo[] files = obj.GetFiles();
				foreach (FileInfo file in files)
				{
					curFile = file.Name;
					TimeSpan age = DateTime.Now - file.LastWriteTime;
					if (file.LastWriteTime < DateTime.Now && age.TotalDays > 124.0)
					{
						file.Delete();
					}
				}
			}
		}
		catch (Exception ex)
		{
			Error.AddDevWarning("Error pruning dir={0} file={1}:\n{2}", curFolder, curFile, ex.ToString());
		}
	}
}
