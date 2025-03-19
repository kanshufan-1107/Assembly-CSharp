using UnityEngine;

public class MercenariesDeckCover : DeckCover
{
	public GameObject m_ProtectorGem;

	public UberText m_ProtectorCountText;

	public GameObject m_FighterGem;

	public UberText m_FighterCountText;

	public GameObject m_CasterGem;

	public UberText m_CasterCountText;

	public override void UpdateVisual(Player.Side side)
	{
		ZoneDeck deck = ZoneMgr.Get().FindZoneOfType<ZoneDeck>(side);
		GetRoleCountInZone(deck, out var numProtectorInDeck, out var numCasterInDeck, out var numFighterInDeck);
		UpdateRoleComponent(numProtectorInDeck, m_ProtectorGem, m_ProtectorCountText);
		UpdateRoleComponent(numCasterInDeck, m_CasterGem, m_CasterCountText);
		UpdateRoleComponent(numFighterInDeck, m_FighterGem, m_FighterCountText);
	}

	private void UpdateRoleComponent(int count, GameObject rootGemObject, UberText gemText)
	{
		if (count == 0)
		{
			rootGemObject.SetActive(value: false);
			return;
		}
		rootGemObject.SetActive(value: true);
		gemText.Text = count.ToString();
	}

	private void GetRoleCountInZone(Zone zone, out int numProtector, out int numCaster, out int numFighter)
	{
		numProtector = 0;
		numCaster = 0;
		numFighter = 0;
		if (zone == null)
		{
			return;
		}
		foreach (Card card in zone.GetCards())
		{
			Entity entity = card.GetEntity();
			if (entity != null)
			{
				switch (entity.GetTag<TAG_ROLE>(GAME_TAG.LETTUCE_ROLE))
				{
				case TAG_ROLE.TANK:
					numProtector++;
					break;
				case TAG_ROLE.FIGHTER:
					numFighter++;
					break;
				case TAG_ROLE.CASTER:
					numCaster++;
					break;
				}
			}
		}
	}
}
