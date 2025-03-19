using UnityEngine;

public class CornerSpellReplacementManager
{
	private Spell[] m_cornerReplacementSpells = new Spell[4];

	private CornerReplacementSpellType m_opposingPlayerCornerType;

	private CornerReplacementSpellType m_friendlyPlayerCornerType;

	private bool m_isInShop;

	public CornerSpellReplacementManager(bool isInShop = false)
	{
		m_isInShop = isInShop;
	}

	public void UpdateCornerReplacements(CornerReplacementSpellType friendlyNewType = CornerReplacementSpellType.NONE, CornerReplacementSpellType opposingNewType = CornerReplacementSpellType.NONE)
	{
		if (m_isInShop)
		{
			UpdateCornerSpellReplacements(friendlyNewType, opposingNewType);
		}
		else
		{
			if (GameState.Get() == null)
			{
				return;
			}
			Player player = GameState.Get().GetPlayerBySide(Player.Side.FRIENDLY);
			if (player != null)
			{
				Player opposingPlayer = GameState.Get().GetPlayerBySide(Player.Side.OPPOSING);
				if (opposingPlayer != null)
				{
					friendlyNewType = (CornerReplacementSpellType)player.GetTag(GAME_TAG.CORNER_REPLACEMENT_TYPE);
					opposingNewType = (CornerReplacementSpellType)opposingPlayer.GetTag(GAME_TAG.CORNER_REPLACEMENT_TYPE);
					UpdateCornerSpellReplacements(friendlyNewType, opposingNewType);
				}
			}
		}
	}

	public Spell[] GetCornerSpells()
	{
		return m_cornerReplacementSpells;
	}

	private Board GetBoard()
	{
		if (m_isInShop)
		{
			return CutsceneBoard.Get();
		}
		return Board.Get();
	}

	private void UpdateCornerSpellReplacements(CornerReplacementSpellType friendlyNewType, CornerReplacementSpellType opposingNewType)
	{
		if (m_friendlyPlayerCornerType != friendlyNewType)
		{
			m_friendlyPlayerCornerType = friendlyNewType;
			UpdateCornerReplacement(m_friendlyPlayerCornerType, CornerReplacementPosition.BOTTOM_LEFT);
			UpdateCornerReplacement(m_friendlyPlayerCornerType, CornerReplacementPosition.BOTTOM_RIGHT);
			UpdateTableTop(m_friendlyPlayerCornerType, Player.Side.FRIENDLY);
			UpdateFrame(m_friendlyPlayerCornerType, Player.Side.FRIENDLY);
			UpdatePlayArea(m_friendlyPlayerCornerType, Player.Side.FRIENDLY);
		}
		if (m_opposingPlayerCornerType != opposingNewType)
		{
			m_opposingPlayerCornerType = opposingNewType;
			UpdateCornerReplacement(m_opposingPlayerCornerType, CornerReplacementPosition.TOP_LEFT);
			UpdateCornerReplacement(m_opposingPlayerCornerType, CornerReplacementPosition.TOP_RIGHT);
			UpdateTableTop(m_opposingPlayerCornerType, Player.Side.OPPOSING);
			UpdateFrame(m_opposingPlayerCornerType, Player.Side.OPPOSING);
			UpdatePlayArea(m_opposingPlayerCornerType, Player.Side.OPPOSING);
		}
	}

	private void UpdateCornerReplacement(CornerReplacementSpellType spellType, CornerReplacementPosition corner)
	{
		Board board = GetBoard();
		if (!(board == null) && board.IsCornerReplacementCompatible() && (!(InputManager.Get() != null) || !InputManager.Get().HasPlayFromMiniHandEnabled() || !UniversalInputManager.UsePhoneUI || (corner != 0 && corner != CornerReplacementPosition.BOTTOM_RIGHT)))
		{
			board.DisableCorner(corner);
			Spell currentCornerSpell = m_cornerReplacementSpells[(int)corner];
			if (currentCornerSpell != null)
			{
				currentCornerSpell.ActivateState(SpellStateType.DEATH);
			}
			currentCornerSpell = CornerReplacementConfig.Get().GetSpell(spellType, corner);
			if (currentCornerSpell != null)
			{
				currentCornerSpell.ActivateState(SpellStateType.BIRTH);
			}
			m_cornerReplacementSpells[(int)corner] = currentCornerSpell;
		}
	}

	private void UpdateTableTop(CornerReplacementSpellType spellType, Player.Side side)
	{
		Board board = GetBoard();
		if (!(board == null) && board.IsCornerReplacementCompatible())
		{
			Texture tex = CornerReplacementConfig.Get().GetTableTopTexture(spellType);
			board.SetTableTopTexture(tex, side);
		}
	}

	private void UpdateFrame(CornerReplacementSpellType spellType, Player.Side side)
	{
		Board board = GetBoard();
		if (!(board == null) && Board.Get().IsCornerReplacementCompatible())
		{
			Texture tex = CornerReplacementConfig.Get().GetFrameTexture(spellType);
			board.SetFrameTexture(tex, side);
		}
	}

	private void UpdatePlayArea(CornerReplacementSpellType spellType, Player.Side side)
	{
		Board board = GetBoard();
		if (!(board == null) && board.IsCornerReplacementCompatible())
		{
			Texture tex = CornerReplacementConfig.Get().GetPlayAreaTexture(spellType);
			Texture maskTex = CornerReplacementConfig.Get().GetPlayAreaMaskTexture(spellType);
			board.SetPlayAreaTexture(tex, maskTex, side);
		}
	}
}
