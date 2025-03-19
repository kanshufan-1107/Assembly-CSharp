public class TutorialNotification : Notification
{
	public UIBButton m_ButtonStart;

	public UberText m_WantedText;

	private void Awake()
	{
		GameMgr gameMgr = GameMgr.Get();
		BnetBar bnetBar = BnetBar.Get();
		if (gameMgr != null && gameMgr.IsTraditionalTutorial() && bnetBar != null)
		{
			bnetBar.ShowSkipTutorialButton();
			bnetBar.Undim();
		}
	}

	public void SetWantedText(string txt)
	{
		if (m_WantedText != null)
		{
			m_WantedText.Text = txt;
			m_WantedText.gameObject.SetActive(value: true);
		}
	}
}
