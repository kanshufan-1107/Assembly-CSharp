namespace HutongGames.PlayMaker.Actions;

[Tooltip("Get the banner game object when a choice is happening")]
[ActionCategory("Pegasus")]
public class GetChoiceBannerAction : FsmStateAction
{
	public FsmGameObject m_bannerObject;

	public override void Reset()
	{
		m_bannerObject = null;
	}

	public override void OnEnter()
	{
		base.OnEnter();
		Banner banner = ChoiceCardMgr.Get()?.GetChoiceBanner();
		if (banner == null)
		{
			global::Log.PlayMaker.PrintError("{0}.OnEnter(): banner object is null", this);
			Finish();
			return;
		}
		if (!m_bannerObject.IsNone)
		{
			m_bannerObject.Value = banner.gameObject;
		}
		Finish();
	}
}
