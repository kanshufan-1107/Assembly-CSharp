namespace HutongGames.PlayMaker.Actions;

[Tooltip("Store a player's cardback materials based on Player side.")]
[ActionCategory("Pegasus")]
public class PlayerGetCardbackAction : FsmStateAction
{
	public Player.Side m_PlayerSide;

	public FsmMaterial m_CardbackMaterial;

	public FsmTexture m_CardbackTextureFlat;

	public override void Reset()
	{
		m_PlayerSide = Player.Side.FRIENDLY;
		m_CardbackMaterial = null;
		m_CardbackTextureFlat = null;
	}

	public override void OnEnter()
	{
		base.OnEnter();
		CardBack playerCB = ((m_PlayerSide != Player.Side.OPPOSING) ? CardBackManager.Get().GetFriendlyCardBack() : CardBackManager.Get().GetOpponentCardBack());
		if (playerCB != null)
		{
			m_CardbackMaterial.Value = playerCB.m_CardBackMaterial;
			m_CardbackTextureFlat.Value = playerCB.m_CardBackTexture;
		}
		Finish();
	}
}
