using System.Collections.Generic;
using System.Linq;
using Assets;
using Hearthstone.DataModels;
using Hearthstone.UI;
using PegasusLettuce;
using PegasusShared;
using UnityEngine;

public class LettuceMap : MonoBehaviour
{
	public AsyncReference m_FinalBossChestReference;

	public Transform MapBoundsLeftBone;

	public Transform MapBoundsRightBone;

	public bool EnableRandomCoinPositionsInRow = true;

	private const float MinDistanceBetweenCoinsOnRow = 15f;

	private Dictionary<LettuceMapCoinDataModel, LettuceMapCoin> m_lettuceCoinsByDataModel = new Dictionary<LettuceMapCoinDataModel, LettuceMapCoin>();

	private Dictionary<int, LettuceMapCoin> m_lettuceCoinsByNodeId = new Dictionary<int, LettuceMapCoin>();

	private List<LettuceMapCoinDataModel> m_lettuceCoinDataModels = new List<LettuceMapCoinDataModel>();

	private int m_numDataModelsLeftToRegister;

	private bool m_allLinesDrawn;

	private bool m_finalBossChestFinishedLoading;

	private LettuceMapCoinDataModel m_selectedCoin;

	private GameObject m_finalBossChest;

	private bool m_isFinalBossDefeated;

	private int m_seed;

	private List<DefLoader.DisposableCardDef> m_loadedCoinCardDefs = new List<DefLoader.DisposableCardDef>();

	public int NumberOfRows { get; private set; }

	public List<LettuceMapNode> NodeData { get; private set; }

	private void Start()
	{
		m_FinalBossChestReference.RegisterReadyListener<VisualController>(OnFinalBossChestReady);
	}

	private void OnDestroy()
	{
		foreach (DefLoader.DisposableCardDef loadedCoinCardDef in m_loadedCoinCardDefs)
		{
			loadedCoinCardDef.Dispose();
		}
	}

	private LettuceMapDataModel GetLettuceMapDataModel()
	{
		VisualController visualController = GetComponent<VisualController>();
		if (visualController == null)
		{
			return null;
		}
		Widget owner = visualController.Owner;
		if (!owner.GetDataModel(198, out var dataModel))
		{
			dataModel = new LettuceMapDataModel();
			owner.BindDataModel(dataModel);
		}
		return dataModel as LettuceMapDataModel;
	}

	public void OnFinalBossChestReady(VisualController visualController)
	{
		if (visualController == null)
		{
			Error.AddDevWarning("UI Error!", "FinalBossChest could not be found!");
			return;
		}
		m_finalBossChest = visualController.gameObject;
		m_finalBossChestFinishedLoading = true;
	}

	public bool IsFinishedLoading()
	{
		if (m_allLinesDrawn)
		{
			return m_finalBossChestFinishedLoading;
		}
		return false;
	}

	public void CreateMapFromProto(PegasusLettuce.LettuceMap lettuceMap)
	{
		LettuceMapDataModel dataModel = GetLettuceMapDataModel();
		if (dataModel == null)
		{
			Log.Lettuce.PrintError("CreateMapFromProto: No data model for lettuce map.");
			return;
		}
		if (lettuceMap == null)
		{
			Log.Lettuce.PrintError("CreateMapFromProto: No map provided.");
			return;
		}
		PegasusLettuce.LettuceMap.Type mapType = lettuceMap.MapType;
		if (mapType != 0 && mapType == PegasusLettuce.LettuceMap.Type.TYPE_BOSS_RUSH)
		{
			dataModel.MapType = LettuceMapType.BOSS_RUSH;
		}
		else
		{
			dataModel.MapType = LettuceMapType.STANDARD;
		}
		NodeData = lettuceMap.Nodes;
		int lastRowId = 0;
		Dictionary<uint, List<LettuceMapNode>> nodesByRowId = new Dictionary<uint, List<LettuceMapNode>>();
		Dictionary<uint, DataModelList<int>> parentIdByNodeId = new Dictionary<uint, DataModelList<int>>();
		foreach (LettuceMapNode node in lettuceMap.Nodes)
		{
			if (!nodesByRowId.ContainsKey(node.Row))
			{
				nodesByRowId.Add(node.Row, new List<LettuceMapNode>());
			}
			nodesByRowId[node.Row].Add(node);
			if (node.Row > lastRowId)
			{
				lastRowId = (int)node.Row;
			}
			if (GameUtils.IsFinalBossNodeType((int)node.NodeTypeId) && node.NodeState_ == LettuceMapNode.NodeState.COMPLETE)
			{
				m_isFinalBossDefeated = true;
			}
			foreach (uint childId in node.ChildNodeIds)
			{
				if (!parentIdByNodeId.ContainsKey(childId))
				{
					parentIdByNodeId.Add(childId, new DataModelList<int>());
				}
				parentIdByNodeId[childId].Add((int)node.NodeId);
			}
		}
		if (!nodesByRowId.ContainsKey(0u))
		{
			Debug.LogError("LettuceMap had no root node (no node with row == 0)!");
			return;
		}
		dataModel.Rows = new DataModelList<LettuceMapRowDataModel>();
		for (int rowId = lastRowId; rowId >= 0; rowId--)
		{
			DataModelList<LettuceMapCoinDataModel> coins = new DataModelList<LettuceMapCoinDataModel>();
			foreach (LettuceMapNode node2 in nodesByRowId[(uint)rowId])
			{
				DataModelList<int> neighborIds = new DataModelList<int>();
				foreach (uint neighborId in node2.ChildNodeIds)
				{
					neighborIds.Add((int)neighborId);
				}
				DataModelList<int> parentIdList = ((!parentIdByNodeId.ContainsKey(node2.NodeId)) ? new DataModelList<int>() : parentIdByNodeId[node2.NodeId]);
				LettuceMapCoinDataModel newCoin = new LettuceMapCoinDataModel
				{
					Id = (int)node2.NodeId,
					NeighborIds = neighborIds,
					NodeTypeId = (int)node2.NodeTypeId,
					MercenaryRole = node2.NodeRole,
					CoinState = node2.NodeState_,
					CoinData = GetCoinDataForNode(node2),
					ParentIds = parentIdList,
					NodeVisualId = ((node2.NodeTypeId != 0) ? GameDbf.LettuceMapNodeType.GetRecord((int)node2.NodeTypeId).NodeVisualId : string.Empty)
				};
				GetTooltipStringsForNodeType((int)node2.NodeTypeId, GameUtils.GetMercenaryTagRoleFromProtoRole(node2.NodeRole), out var headerString, out var bodyString);
				newCoin.HoverTooltipHeader = headerString;
				newCoin.HoverTooltipBody = bodyString;
				SetGrantedAnomalyCardFromMapData(lettuceMap, (int)node2.NodeId, newCoin);
				coins.Add(newCoin);
				m_lettuceCoinDataModels.Add(newCoin);
				m_numDataModelsLeftToRegister++;
			}
			LettuceMapRowDataModel nodeRow = new LettuceMapRowDataModel
			{
				Coins = coins
			};
			dataModel.Rows.Add(nodeRow);
		}
		NumberOfRows = dataModel.Rows.Count;
		m_seed = lettuceMap.Seed;
		m_lettuceCoinDataModels = m_lettuceCoinDataModels.OrderBy((LettuceMapCoinDataModel c) => c.Id).ToList();
	}

	public void RegisterCoin(LettuceMapCoin coin, LettuceMapCoinDataModel coinDataModel)
	{
		if (coinDataModel == null)
		{
			Debug.LogError("LettuceMap.RegisterCoin() - Coin had no data model!");
			return;
		}
		m_lettuceCoinsByDataModel.Add(coinDataModel, coin);
		m_lettuceCoinsByNodeId.Add(coinDataModel.Id, coin);
		m_numDataModelsLeftToRegister--;
		if (m_numDataModelsLeftToRegister == 0)
		{
			OnAllCoinsLoaded();
		}
	}

	public void SelectCoin(LettuceMapCoinDataModel selectedCoin)
	{
		if (!m_lettuceCoinsByDataModel.ContainsKey(selectedCoin))
		{
			Debug.LogError("SelectCoin() - No coin with id=" + selectedCoin.Id);
			return;
		}
		if (m_selectedCoin != null)
		{
			m_selectedCoin.CoinData.Selected = false;
			m_lettuceCoinsByDataModel[m_selectedCoin].GetComponent<Widget>().BindDataModel(m_selectedCoin);
		}
		selectedCoin.CoinData.Selected = true;
		m_lettuceCoinsByDataModel[selectedCoin].GetComponent<Widget>().BindDataModel(selectedCoin);
		m_selectedCoin = selectedCoin;
	}

	public void RefreshWithNewData(PegasusLettuce.LettuceMap updatedMapData)
	{
		foreach (LettuceMapCoinDataModel coinDataModel in m_lettuceCoinsByDataModel.Keys)
		{
			LettuceMapNode updatedNodeData = updatedMapData.Nodes.FirstOrDefault((LettuceMapNode n) => n.NodeId == coinDataModel.Id);
			if (updatedNodeData != null)
			{
				coinDataModel.CoinData = GetCoinDataForNode(updatedNodeData);
				coinDataModel.CoinState = updatedNodeData.NodeState_;
				coinDataModel.MercenaryRole = updatedNodeData.NodeRole;
				coinDataModel.NodeTypeId = (int)updatedNodeData.NodeTypeId;
				if (updatedNodeData.NodeTypeId != 0)
				{
					coinDataModel.NodeVisualId = GameDbf.LettuceMapNodeType.GetRecord((int)updatedNodeData.NodeTypeId).NodeVisualId;
				}
				GetTooltipStringsForNodeType((int)updatedNodeData.NodeTypeId, GameUtils.GetMercenaryTagRoleFromProtoRole(updatedNodeData.NodeRole), out var headerString, out var bodyString);
				coinDataModel.HoverTooltipHeader = headerString;
				coinDataModel.HoverTooltipBody = bodyString;
				SetGrantedAnomalyCardFromMapData(updatedMapData, coinDataModel.Id, coinDataModel);
			}
		}
		UpdateCoinGlowLines();
		NodeData = updatedMapData.Nodes;
	}

	public List<LettuceMapCoin> GetCompletedCoins()
	{
		List<LettuceMapCoin> completedCoins = new List<LettuceMapCoin>();
		foreach (LettuceMapCoinDataModel key in m_lettuceCoinsByDataModel.Keys)
		{
			if (key.CoinState == LettuceMapNode.NodeState.COMPLETE)
			{
				LettuceMapCoin coin = m_lettuceCoinsByDataModel[key];
				completedCoins.Add(coin);
			}
		}
		return completedCoins.OrderBy((LettuceMapCoin c) => c.NodeId).ToList();
	}

	public List<LettuceMapCoinDataModel> GetUnlockedCoinDataModels()
	{
		List<LettuceMapCoinDataModel> datamodels = new List<LettuceMapCoinDataModel>();
		foreach (LettuceMapCoinDataModel key in m_lettuceCoinsByDataModel.Keys)
		{
			if (key.CoinState == LettuceMapNode.NodeState.UNLOCKED)
			{
				datamodels.Add(key);
			}
		}
		return datamodels;
	}

	public LettuceMapCoinDataModel GetCoinDataModelById(int id)
	{
		foreach (LettuceMapCoinDataModel key in m_lettuceCoinsByDataModel.Keys)
		{
			if (key.Id == id)
			{
				return key;
			}
		}
		return null;
	}

	public LettuceMapCoinDataModel GetFinalBossCoinDataModel()
	{
		return m_lettuceCoinDataModels.Last();
	}

	public LettuceMapCoinDataModel GetDefeatCoinDataModel()
	{
		return m_lettuceCoinDataModels.FirstOrDefault((LettuceMapCoinDataModel dataModel) => dataModel.CoinState == LettuceMapNode.NodeState.DEFEAT);
	}

	public LettuceMapCoinDataModel GetLastCompletedCoinDataModel()
	{
		return m_lettuceCoinDataModels.OrderByDescending((LettuceMapCoinDataModel c) => c.Id).FirstOrDefault((LettuceMapCoinDataModel c) => c.CoinState == LettuceMapNode.NodeState.COMPLETE);
	}

	public bool IsFinalBossDefeated()
	{
		return m_isFinalBossDefeated;
	}

	public void FlipUnlockedCoins()
	{
		foreach (KeyValuePair<LettuceMapCoinDataModel, LettuceMapCoin> item in m_lettuceCoinsByDataModel)
		{
			LettuceMapCoinDataModel coinDataModel = item.Key;
			if (coinDataModel.CoinState == LettuceMapNode.NodeState.UNLOCKED)
			{
				coinDataModel.CoinData.MissionState = AdventureMissionState.UNLOCKED;
			}
		}
	}

	public Vector3 GetWorldSpacePositionOfCoin(int coinId)
	{
		LettuceMapCoinDataModel coinDataModel = GetLettuceMapCoinFromCoinId(coinId);
		if (coinDataModel == null)
		{
			return Vector3.zero;
		}
		return m_lettuceCoinsByDataModel[coinDataModel].transform.position;
	}

	private LettuceMapCoinDataModel GetLettuceMapCoinFromCoinId(int coinId)
	{
		foreach (LettuceMapCoinDataModel coinDataModel in m_lettuceCoinsByDataModel.Keys)
		{
			if (coinDataModel.Id == coinId)
			{
				return coinDataModel;
			}
		}
		Debug.LogError("LettuceMap.GetLettuceMapCoinFromCoinId() - No coin found with id=" + coinId);
		return null;
	}

	private void OnAllCoinsLoaded()
	{
		PositionCoinsInRow();
		DrawCoinConnectionLines();
		UpdateCoinGlowLines();
	}

	private void PositionCoinsInRow()
	{
		LettuceMapDataModel dataModel = GetLettuceMapDataModel();
		float leftSideX = MapBoundsLeftBone.position.x;
		float pageWidth = MapBoundsRightBone.position.x - leftSideX;
		Random.InitState(m_seed);
		for (int rowIndex = 0; rowIndex < dataModel.Rows.Count; rowIndex++)
		{
			LettuceMapRowDataModel row = dataModel.Rows[rowIndex];
			if (rowIndex == 0 || rowIndex >= dataModel.Rows.Count - 2)
			{
				float centerPositionX = leftSideX + pageWidth / 2f;
				LettuceMapCoinDataModel coinDataModel = row.Coins.First();
				LettuceMapCoin coin = m_lettuceCoinsByDataModel[coinDataModel];
				coin.transform.position = new Vector3(centerPositionX, coin.transform.position.y, coin.transform.position.z);
				continue;
			}
			int numCoinsInRow = row.Coins.Count;
			for (int i = 0; i < numCoinsInRow; i++)
			{
				LettuceMapCoinDataModel coinDataModel2 = row.Coins[i];
				LettuceMapCoin coin2 = m_lettuceCoinsByDataModel[coinDataModel2];
				float spacePerCoin = pageWidth / (float)numCoinsInRow;
				float minX = leftSideX + spacePerCoin * (float)i + 7.5f;
				float maxX = leftSideX + spacePerCoin * (float)(i + 1) - 7.5f;
				float positionX = ((!EnableRandomCoinPositionsInRow) ? ((minX + maxX) / 2f) : Random.Range(minX, maxX));
				coin2.transform.position = new Vector3(positionX, coin2.transform.position.y, coin2.transform.position.z);
			}
		}
	}

	private void DrawCoinConnectionLines()
	{
		foreach (LettuceMapCoinDataModel coinDataModel in m_lettuceCoinsByDataModel.Keys)
		{
			LettuceMapCoin coin = m_lettuceCoinsByDataModel[coinDataModel];
			for (int neighborIndex = 0; neighborIndex < coinDataModel.NeighborIds.Count; neighborIndex++)
			{
				int neighborId = coinDataModel.NeighborIds[neighborIndex];
				LettuceMapCoinDataModel neighborDataModel = GetLettuceMapCoinFromCoinId(neighborId);
				if (neighborDataModel == null)
				{
					continue;
				}
				LettuceMapCoin neighborCoin = m_lettuceCoinsByDataModel[neighborDataModel];
				int numParentsOnLeft = 0;
				int numParentsOnRight = 0;
				int currentCoinIndexInParentList = 0;
				for (int parentIndex = 0; parentIndex < neighborDataModel.ParentIds.Count; parentIndex++)
				{
					int parentId = neighborDataModel.ParentIds[parentIndex];
					LettuceMapCoin lettuceMapCoin = m_lettuceCoinsByNodeId[parentId];
					if (parentId == coinDataModel.Id)
					{
						currentCoinIndexInParentList = parentIndex;
					}
					if (lettuceMapCoin.transform.position.x < neighborCoin.transform.position.x)
					{
						numParentsOnLeft++;
					}
					else
					{
						numParentsOnRight++;
					}
				}
				coin.DrawLineToObjectOnNextRow(neighborCoin.gameObject, currentCoinIndexInParentList, numParentsOnLeft, numParentsOnRight);
			}
			if (GameUtils.IsFinalBossNodeType(coinDataModel.NodeTypeId))
			{
				coin.DrawLineToObjectOnSameRow(m_finalBossChest);
			}
		}
		m_allLinesDrawn = true;
	}

	private AdventureMissionState GetAdventureMissionStateFromNode(LettuceMapNode node)
	{
		switch (node.NodeState_)
		{
		case LettuceMapNode.NodeState.UNLOCKED:
			return AdventureMissionState.LOCKED;
		case LettuceMapNode.NodeState.COMPLETE:
			return AdventureMissionState.COMPLETED;
		case LettuceMapNode.NodeState.BLOCKED:
			return AdventureMissionState.LOCKED;
		case LettuceMapNode.NodeState.DEFEAT:
			return AdventureMissionState.UNLOCKED;
		case LettuceMapNode.NodeState.LOCKED:
		{
			LettuceMapNodeTypeDbfRecord record = GameDbf.LettuceMapNodeType.GetRecord((int)node.NodeTypeId);
			if (record == null)
			{
				return AdventureMissionState.LOCKED;
			}
			if (record.BossType != 0 && record.AlwaysShowBossPreview)
			{
				return AdventureMissionState.UNLOCKED;
			}
			return AdventureMissionState.LOCKED;
		}
		default:
			Log.Lettuce.PrintError("Unable to get AdventureMissionState for node state: {0}", node.NodeState_);
			return AdventureMissionState.LOCKED;
		}
	}

	private AdventureMissionDataModel GetCoinDataForNode(LettuceMapNode node)
	{
		AdventureMissionDataModel obj = new AdventureMissionDataModel
		{
			ScenarioId = ScenarioDbId.LETTUCE_MAP,
			MissionState = GetAdventureMissionStateFromNode(node)
		};
		List<int> bossCards = node.BossCard.Select((PegasusShared.CardDef o) => o.Asset).ToList();
		bossCards.Sort();
		LettuceVillageDataUtil.ApplyMercenaryBossCoinMaterials(obj, bossCards, m_loadedCoinCardDefs);
		return obj;
	}

	private void UpdateCoinGlowLines()
	{
		foreach (LettuceMapCoinDataModel key in m_lettuceCoinsByDataModel.Keys)
		{
			key.LineGlowVisible = false;
		}
		List<LettuceMapCoin> completedCoins = GetCompletedCoins();
		if (completedCoins != null && completedCoins.Count != 0)
		{
			LettuceMapCoinDataModel coinDataModel = completedCoins.Last()?.GetMapCoinDataModel();
			if (coinDataModel != null)
			{
				coinDataModel.LineGlowVisible = true;
			}
		}
	}

	private void GetTooltipStringsForNodeType(int nodeTypeId, TAG_ROLE nodeRole, out string headerString, out string bodyString)
	{
		LettuceMapNodeTypeDbfRecord record = GameDbf.LettuceMapNodeType.GetRecord(nodeTypeId);
		if (nodeTypeId <= 0 || record == null)
		{
			headerString = GameStrings.Get("GLUE_LETTUCE_MAP_MYSTERY_TOOLTIP_HEADER");
			bodyString = GameStrings.Get("GLUE_LETTUCE_MAP_MYSTERY_TOOLTIP_BODY");
			return;
		}
		bodyString = record.HoverTooltipBody;
		if (nodeRole != 0)
		{
			if (record.BossType == LettuceMapNodeType.LettuceMapBossType.NORMAL_BOSS || record.BossType == LettuceMapNodeType.LettuceMapBossType.SIMPLE_BOSS)
			{
				headerString = GameStrings.Format("GLUE_LETTUCE_MAP_BOSS_FIGHT_TOOLTIP_HEADER", GameStrings.GetRoleName(nodeRole));
				return;
			}
			if (record.BossType == LettuceMapNodeType.LettuceMapBossType.ELITE_BOSS)
			{
				headerString = GameStrings.Format("GLUE_LETTUCE_MAP_ELITE_BOSS_FIGHT_TOOLTIP_HEADER", GameStrings.GetRoleName(nodeRole));
				return;
			}
		}
		headerString = record.HoverTooltipHeader;
	}

	private LettuceMapNodeType.LettuceMapBossType GetBossTypeForNodeType(int nodeTypeId)
	{
		return GameDbf.LettuceMapNodeType.GetRecord(nodeTypeId)?.BossType ?? LettuceMapNodeType.LettuceMapBossType.NONE;
	}

	private void SetGrantedAnomalyCardFromMapData(PegasusLettuce.LettuceMap mapData, int nodeId, LettuceMapCoinDataModel coinDataModel)
	{
		foreach (LettuceMapAnomalyAssignment anomalyCard in mapData.AnomalyCards)
		{
			if (anomalyCard.SourceNodeId == nodeId)
			{
				coinDataModel.GrantedAnomalyCard = new CardDataModel
				{
					CardId = GameUtils.TranslateDbIdToCardId(anomalyCard.AnomalyCard),
					Premium = TAG_PREMIUM.NORMAL
				};
			}
		}
	}
}
