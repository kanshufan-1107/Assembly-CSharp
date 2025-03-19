using Blizzard.T5.AssetManager;
using UnityEngine;

[CustomEditClass]
internal class HeroCardSwitcher : MonoBehaviour
{
	public enum ReplaceOn
	{
		GamePlaying,
		GameOverVictory,
		GameOverDefeat
	}

	[CustomEditField(T = EditType.GAME_OBJECT)]
	public string m_ReplacementCardDefAsset;

	public bool m_LoadCardDefTextures;

	public ReplaceOn m_ReplaceOn;

	public bool m_ReplaceHeroTray = true;

	public bool m_FinalSwitch;

	private DefLoader.DisposableCardDef m_ReplacementCardDef;

	private CardDef m_CardDefToSwitch;

	public void SetCardDefToSwitch(DefLoader.DisposableCardDef cardDef)
	{
		m_CardDefToSwitch = cardDef.CardDef;
	}

	public void OnGameVictory(Actor screenActor)
	{
		if (ShouldDoReplace(GameState.CreateGamePhase.CREATED, TAG_PLAYSTATE.WON))
		{
			screenActor.SetCardDef(GetReplacementCardDef());
			screenActor.UpdateAllComponents();
		}
	}

	public void OnGameDefeat(Actor screenActor)
	{
		if (ShouldDoReplace(GameState.CreateGamePhase.CREATED, TAG_PLAYSTATE.LOST))
		{
			screenActor.SetCardDef(GetReplacementCardDef());
			screenActor.UpdateAllComponents();
		}
	}

	public DefLoader.DisposableCardDef GetReplacementCardDef()
	{
		if (m_ReplacementCardDef == null)
		{
			AssetHandle<GameObject> go = AssetLoader.Get()?.LoadAsset<GameObject>(m_ReplacementCardDefAsset, AssetLoadingOptions.IgnorePrefabPosition);
			if (!go)
			{
				Debug.LogError("No asset at path " + m_ReplacementCardDefAsset + " was found, hero card switch disabled");
				return null;
			}
			m_ReplacementCardDef = new DefLoader.DisposableCardDef(go);
			if (!m_FinalSwitch && go.Asset.GetComponent<HeroCardSwitcher>() != null)
			{
				Object.Instantiate(go.Asset).GetComponent<HeroCardSwitcher>().SetCardDefToSwitch(m_ReplacementCardDef.Share());
			}
			if (m_LoadCardDefTextures)
			{
				CardTextureLoader.Load(m_ReplacementCardDef.CardDef, new CardPortraitQuality(3, TAG_PREMIUM.GOLDEN));
			}
		}
		return m_ReplacementCardDef;
	}

	private void Start()
	{
		Init();
	}

	private void Init()
	{
		if (m_CardDefToSwitch == null)
		{
			m_CardDefToSwitch = GetComponent<CardDef>();
		}
		GameState gs = GameState.Get();
		if (gs != null)
		{
			TAG_PLAYSTATE friendlyPlayState = gs.GetFriendlySidePlayer().GetTag<TAG_PLAYSTATE>(GAME_TAG.PLAYSTATE);
			if (ShouldDoReplace(gs.GetCreateGamePhase(), friendlyPlayState))
			{
				DoReplace();
			}
			else if (m_ReplaceOn == ReplaceOn.GamePlaying)
			{
				GameState.Get()?.RegisterCreateGameListener(OnGameStateCreated);
			}
		}
	}

	private void OnDestroy()
	{
		GameState.Get()?.UnregisterCreateGameListener(OnGameStateCreated);
		if (m_ReplacementCardDef != null)
		{
			m_ReplacementCardDef.Dispose();
		}
	}

	private bool ShouldDoReplace(GameState.CreateGamePhase createGamePhase, TAG_PLAYSTATE playState)
	{
		if (m_ReplaceOn == ReplaceOn.GamePlaying)
		{
			GameState gs = GameState.Get();
			if (createGamePhase != GameState.CreateGamePhase.CREATED || gs.IsMulliganPhasePending() || gs.IsGameOver())
			{
				return false;
			}
			return true;
		}
		if (m_ReplaceOn == ReplaceOn.GameOverVictory)
		{
			if (playState == TAG_PLAYSTATE.WON)
			{
				return true;
			}
		}
		else if (playState == TAG_PLAYSTATE.LOST)
		{
			return true;
		}
		return false;
	}

	private void OnGameStateCreated(GameState.CreateGamePhase phase, object userData)
	{
		if (ShouldDoReplace(phase, TAG_PLAYSTATE.INVALID))
		{
			DoReplace();
		}
	}

	private void DoReplace()
	{
		foreach (Player player in GameState.Get().GetPlayerMap().Values)
		{
			Card heroCard = player.GetHeroCard();
			DoReplacementForCard(heroCard, player.GetSide());
		}
	}

	public void DoReplacementForCard(Card card, Player.Side side = Player.Side.NEUTRAL)
	{
		if (card.HasSameCardDef(m_CardDefToSwitch))
		{
			card.SetCardDef(GetReplacementCardDef(), updateActor: false);
			if (m_ReplaceHeroTray)
			{
				Board.Get().UpdateCustomHeroTray(side);
			}
		}
	}
}
