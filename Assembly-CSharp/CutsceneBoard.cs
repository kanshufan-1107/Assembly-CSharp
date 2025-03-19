using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

[CustomEditClass]
public class CutsceneBoard : Board
{
	private Actor m_friendlyHeroActor;

	private Actor m_opponentHeroActor;

	private static CutsceneBoard s_instance;

	private Vector3 m_friendlyHeroTrayPosition = new Vector3(0f, -0.08f, 0f);

	public void SetFriendlyHeroActor(Actor hero)
	{
		m_friendlyHeroActor = hero;
	}

	public void SetOpponentHeroActor(Actor hero)
	{
		m_opponentHeroActor = hero;
	}

	private void Awake()
	{
		s_instance = this;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		s_instance = null;
	}

	public new static CutsceneBoard Get()
	{
		return s_instance;
	}

	public void UpdateCustomHeroTray(Player.Side side, Actor actor)
	{
		if (!(actor == null))
		{
			UpdateHeroFrame(actor);
			StartCoroutine(ApplyHeroTrayFromCard(actor.GetCard(), side, isFromOfflineRequest: true));
		}
	}

	public override void UpdateCustomHeroTray(Player.Side side)
	{
		Actor actor;
		switch (side)
		{
		case Player.Side.NEUTRAL:
			return;
		default:
			actor = m_opponentHeroActor;
			break;
		case Player.Side.FRIENDLY:
			actor = m_friendlyHeroActor;
			break;
		}
		Actor actor2 = actor;
		if (!(actor2 == null))
		{
			UpdateCustomHeroTray(side, actor2);
		}
	}

	public void UpdateHeroFrame(Actor actor)
	{
		Card friendlyHeroCard = actor.GetCard();
		if (friendlyHeroCard.HeroFrameFriendlyPath != null && friendlyHeroCard.HeroFrameFriendlyPath.Length > 0)
		{
			AssetLoader.Get().InstantiatePrefab(friendlyHeroCard.HeroFrameFriendlyPath, ShowFriendlyHeroTray);
		}
	}

	protected override void ShowFriendlyHeroTray(AssetReference assetRef, GameObject go, object callbackData)
	{
		go.transform.transform.SetParent(m_friendlyHeroActor.transform);
		go.transform.localPosition = m_friendlyHeroTrayPosition;
		go.SetActive(value: true);
		Renderer[] componentsInChildren = go.GetComponentsInChildren<Renderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].GetMaterial().color = m_GoldenHeroTrayColor;
		}
		Object.Destroy(m_FriendlyHeroTray);
		m_FriendlyHeroTray = go;
	}
}
