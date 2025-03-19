namespace HutongGames.PlayMaker.Actions;

[Tooltip("Used to control TGT Grand Stands")]
[ActionCategory("Pegasus")]
public class TGTGrandStandAction : FsmStateAction
{
	public enum EMOTE
	{
		Cheer,
		OhNo
	}

	[RequiredField]
	public EMOTE m_emote;

	protected Actor m_actor;

	public void PlayEmote(EMOTE emote)
	{
		TGTGrandStand grandStand = TGTGrandStand.Get();
		if (!(grandStand == null))
		{
			switch (emote)
			{
			case EMOTE.Cheer:
				grandStand.PlayCheerAnimation();
				break;
			case EMOTE.OhNo:
				grandStand.PlayOhNoAnimation();
				break;
			}
		}
	}

	public override void Reset()
	{
		m_emote = EMOTE.Cheer;
	}

	public override void OnEnter()
	{
		PlayEmote(m_emote);
	}
}
