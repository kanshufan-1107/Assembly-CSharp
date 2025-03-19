using System;
using UnityEngine;

[CustomEditClass]
public class AdventureSubSceneDisplay : MonoBehaviour
{
	[CustomEditField(Sections = "UI")]
	public float m_BigCardScale = 1f;

	[CustomEditField(Sections = "Bones")]
	public GameObject m_BossPowerBone;

	[CustomEditField(Sections = "Bones")]
	public GameObject m_HeroPowerBigCardBone;

	protected Actor m_BossActor;

	protected Actor m_HeroPowerActor;

	protected Actor m_BossPowerBigCard;

	protected Actor m_HeroPowerBigCard;

	protected DefLoader.DisposableFullDef m_CurrentBossHeroPowerFullDef;

	protected Vector3 m_BossPowerTweenOrigin;

	private AssetLoadingHelper m_assetLoadingHelper;

	protected AssetLoadingHelper AssetLoadingHelper
	{
		get
		{
			if (m_assetLoadingHelper == null)
			{
				m_assetLoadingHelper = new AssetLoadingHelper();
				m_assetLoadingHelper.AssetLoadingComplete += OnAssetsLoaded;
			}
			return m_assetLoadingHelper;
		}
	}

	protected virtual void OnDestroy()
	{
		m_CurrentBossHeroPowerFullDef?.Dispose();
		m_CurrentBossHeroPowerFullDef = null;
	}

	private void OnAssetsLoaded(object sender, EventArgs args)
	{
		OnSubSceneLoaded();
	}

	public static Actor OnActorLoaded(string actorName, GameObject actorObject, GameObject container, bool withRotation = false)
	{
		Actor actor = actorObject.GetComponent<Actor>();
		if (actor != null)
		{
			if (container != null)
			{
				GameUtils.SetParent(actor, container, withRotation);
				LayerUtils.SetLayer(actor, container.layer);
			}
			actor.SetUnlit();
			actor.Hide();
		}
		else
		{
			Debug.LogWarning($"ERROR actor \"{actorName}\" has no Actor component");
		}
		return actor;
	}

	protected bool AddAssetToLoad(int assetCount = 1)
	{
		if (IsSubsceneLoaded())
		{
			return false;
		}
		AssetLoadingHelper.AddAssetToLoad(assetCount);
		return true;
	}

	protected void AssetLoadCompleted()
	{
		if (!IsSubsceneLoaded())
		{
			AssetLoadingHelper.AssetLoadCompleted();
		}
	}

	protected virtual void OnSubSceneLoaded()
	{
		AdventureSubScene subscene = GetComponent<AdventureSubScene>();
		if (subscene != null)
		{
			subscene.AddSubSceneTransitionFinishedListener(OnSubSceneTransitionComplete);
			subscene.SetIsLoaded(loaded: true);
		}
	}

	private bool IsSubsceneLoaded()
	{
		AdventureSubScene subscene = GetComponent<AdventureSubScene>();
		if (subscene != null)
		{
			return subscene.IsLoaded();
		}
		return false;
	}

	protected virtual void OnSubSceneTransitionComplete()
	{
		AdventureSubScene subscene = GetComponent<AdventureSubScene>();
		if (subscene != null)
		{
			subscene.RemoveSubSceneTransitionFinishedListener(OnSubSceneTransitionComplete);
		}
	}

	protected void ShowBossPowerBigCard()
	{
		Vector3? origin = null;
		if (m_HeroPowerActor != null)
		{
			origin = m_HeroPowerActor.gameObject.transform.position;
		}
		BigCardHelper.ShowBigCard(m_BossPowerBigCard, m_CurrentBossHeroPowerFullDef, m_HeroPowerBigCardBone, m_BigCardScale, origin);
	}

	protected void HideBossPowerBigCard()
	{
		BigCardHelper.HideBigCard(m_BossPowerBigCard);
	}
}
