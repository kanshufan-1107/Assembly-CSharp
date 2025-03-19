using System;
using UnityEngine;

[CustomEditClass]
public class Hero_06ah_TentacleSelector : MonoBehaviour
{
	[Serializable]
	[CustomEditClass]
	public class TentacleCollection
	{
		[CustomEditField(T = EditType.GAME_OBJECT)]
		public string m_FriendlyInPlay;

		[CustomEditField(T = EditType.GAME_OBJECT)]
		public string m_OpponentInPlay;

		[CustomEditField(T = EditType.GAME_OBJECT)]
		public string m_VictoryScreen;

		[CustomEditField(T = EditType.GAME_OBJECT)]
		public string m_DefeatScreen;

		public GameObject m_StoreTentacles;

		public Vector3 m_StoreTentaclesLocalPosition;
	}

	public TentacleCollection m_TentacleCollection;

	public TentacleCollection m_MobileTentacleCollection;

	private bool m_ShowingStoreTentacles;

	private void Awake()
	{
		if (StoreManager.Get().IsShown())
		{
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				UnityEngine.Object.Instantiate(m_MobileTentacleCollection.m_StoreTentacles, base.transform, worldPositionStays: false).transform.localPosition = m_MobileTentacleCollection.m_StoreTentaclesLocalPosition;
			}
			else
			{
				UnityEngine.Object.Instantiate(m_TentacleCollection.m_StoreTentacles, base.transform, worldPositionStays: false).transform.localPosition = m_TentacleCollection.m_StoreTentaclesLocalPosition;
			}
			m_ShowingStoreTentacles = true;
		}
	}

	private void Start()
	{
		if (m_ShowingStoreTentacles)
		{
			return;
		}
		if (GameState.Get() != null && GameState.Get().IsGameOver())
		{
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				LoadVictoryDefeatTentacles(m_MobileTentacleCollection);
			}
			else
			{
				LoadVictoryDefeatTentacles(m_TentacleCollection);
			}
		}
		else if ((bool)UniversalInputManager.UsePhoneUI)
		{
			LoadInPlayTentacles(m_MobileTentacleCollection);
		}
		else
		{
			LoadInPlayTentacles(m_TentacleCollection);
		}
	}

	private void OnTentaclesPrefabLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		if (go != null)
		{
			go.transform.SetParent(base.transform.parent, worldPositionStays: false);
			LayerUtils.SetLayer(go, base.transform.parent.gameObject.layer, null);
			UnityEngine.Object.Destroy(base.gameObject);
		}
		else
		{
			Debug.LogError($"Couldn't instantiate tentacles prefab for asset {assetRef}");
		}
	}

	private void LoadInPlayTentacles(TentacleCollection collection)
	{
		Actor actor = LegendaryUtil.FindLegendaryHeroActor(base.gameObject);
		if (!(actor == null) && actor.GetActorStateType() != ActorStateType.CARD_HISTORY)
		{
			Card card = actor.GetCard();
			if (((!(card != null)) ? Player.Side.FRIENDLY : card.GetControllerSide()) == Player.Side.FRIENDLY)
			{
				LoadTentaclesPrefab(collection.m_FriendlyInPlay);
			}
			else
			{
				LoadTentaclesPrefab(collection.m_OpponentInPlay);
			}
		}
	}

	private void LoadVictoryDefeatTentacles(TentacleCollection collection)
	{
		if (GameMgr.Get() == null || GameMgr.Get().LastGameData.GameResult == TAG_PLAYSTATE.WON)
		{
			LoadTentaclesPrefab(collection.m_VictoryScreen);
		}
		else
		{
			LoadTentaclesPrefab(collection.m_DefeatScreen);
		}
	}

	private void LoadTentaclesPrefab(string prefab)
	{
		if (!string.IsNullOrEmpty(prefab))
		{
			AssetLoader.Get().InstantiatePrefab(prefab, OnTentaclesPrefabLoaded);
		}
	}
}
