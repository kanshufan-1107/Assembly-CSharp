using Assets;
using UnityEngine;

public class LettuceLobbyChooserSubButton : ChooserSubButton
{
	[CustomEditField(Sections = "New Unlocks UI")]
	public GameObject m_unlocksCounter;

	[CustomEditField(Sections = "New Unlocks UI")]
	public UberText m_unlocksCounterText;

	private string m_customLockedText;

	private MercenariesDataUtil.MercenariesBountyLockedReason m_lockedReason;

	private SceneMgr.Mode m_mode;

	private int m_bountySetRecord;

	private LettuceBounty.MercenariesBountyDifficulty m_difficulty = LettuceBounty.MercenariesBountyDifficulty.NORMAL;

	public void LockButton(MercenariesDataUtil.MercenariesBountyLockedReason lockReason)
	{
		m_lockedReason = lockReason;
		SetDesaturate(desaturate: true);
	}

	public void SetUnlocks(int amount)
	{
		if (amount > 0)
		{
			m_unlocksCounter.SetActive(value: true);
			m_unlocksCounterText.Text = amount.ToString() ?? "";
		}
		else
		{
			m_unlocksCounter.SetActive(value: false);
		}
	}

	public void SetBountySetRecord(int record)
	{
		m_bountySetRecord = record;
	}

	public LettuceBountySetDbfRecord GetBountySetRecord()
	{
		return GameDbf.LettuceBountySet.GetRecord(m_bountySetRecord);
	}

	public void SetCustomLockedText(string newText)
	{
		m_customLockedText = newText;
	}

	public string GetCustomLockedText()
	{
		return m_customLockedText;
	}

	public MercenariesDataUtil.MercenariesBountyLockedReason GetLockedReason()
	{
		return m_lockedReason;
	}

	public void SetMode(SceneMgr.Mode newMode)
	{
		m_mode = newMode;
	}

	public SceneMgr.Mode GetMode()
	{
		return m_mode;
	}

	public void SetDifficulty(LettuceBounty.MercenariesBountyDifficulty difficulty)
	{
		m_difficulty = difficulty;
	}

	public LettuceBounty.MercenariesBountyDifficulty GetDifficulty()
	{
		return m_difficulty;
	}
}
