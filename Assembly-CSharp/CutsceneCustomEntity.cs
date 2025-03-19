public class CutsceneCustomEntity : Entity
{
	private CutsceneCustomPlayer m_player;

	public void Init(Card sourceCard, Card heroCard, Player.Side side)
	{
		if (sourceCard == null)
		{
			Log.CosmeticPreview.PrintError("Failed to initialize CutsceneCustomEntity as source card was null!");
			return;
		}
		m_card = sourceCard;
		if (heroCard == null)
		{
			Log.CosmeticPreview.PrintWarning("Failed to set player entity for card: " + sourceCard.name + " as hero card was null!");
			return;
		}
		CutsceneCustomPlayer playerEntity = heroCard.GetController() as CutsceneCustomPlayer;
		if (playerEntity == null)
		{
			playerEntity = new CutsceneCustomPlayer();
			playerEntity.SetHero((heroCard == sourceCard) ? this : heroCard.GetEntity());
			playerEntity.SetCard(heroCard);
			playerEntity.SetSide(side);
		}
		m_player = playerEntity;
	}

	public override Entity GetHero()
	{
		return m_player.GetHero();
	}

	public override Card GetHeroCard()
	{
		return m_player.GetHeroCard();
	}

	public override Player GetController()
	{
		return m_player;
	}
}
