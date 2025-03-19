public class MercenaryUnlock
{
	public enum UnlockType
	{
		None,
		Packs,
		VisitorTask,
		Achievement,
		Bounty,
		RewardTrack,
		Custom
	}

	public UnlockType m_unlockType;

	public AchievementDbfRecord m_achievement;

	public LettuceBountyDbfRecord m_bounty;

	public VisitorTaskDbfRecord m_visitorTask;

	public int m_visitorTaskIndex;

	public string m_customAcquireText = "";

	public static readonly MercenaryUnlock FromNone = new MercenaryUnlock(UnlockType.None);

	public static readonly MercenaryUnlock FromPacks = new MercenaryUnlock(UnlockType.Packs);

	public static readonly MercenaryUnlock FromRewardTrack = new MercenaryUnlock(UnlockType.RewardTrack);

	public MercenaryUnlock(UnlockType unlockType, string unlockText = "")
	{
		m_unlockType = unlockType;
		m_customAcquireText = unlockText;
	}

	public static MercenaryUnlock Create(VisitorTaskDbfRecord visitorTask, int index)
	{
		return new MercenaryUnlock(UnlockType.VisitorTask)
		{
			m_visitorTask = visitorTask,
			m_visitorTaskIndex = index
		};
	}

	public static MercenaryUnlock Create(AchievementDbfRecord achievement)
	{
		return new MercenaryUnlock(UnlockType.Achievement)
		{
			m_achievement = achievement
		};
	}

	public static MercenaryUnlock Create(LettuceBountyDbfRecord bounty)
	{
		return new MercenaryUnlock(UnlockType.Bounty)
		{
			m_bounty = bounty
		};
	}

	public override string ToString()
	{
		return m_unlockType switch
		{
			UnlockType.Achievement => string.Format($"Unlock Type {m_unlockType} {m_achievement.Name.GetString()}"), 
			UnlockType.Bounty => string.Format($"Unlock Type {m_unlockType} {m_bounty.NoteDesc}"), 
			UnlockType.VisitorTask => string.Format($"Unlock Type {m_unlockType} {m_visitorTask.TaskTitle.GetString()} {m_visitorTaskIndex}"), 
			UnlockType.Custom => string.Format($"Unlock Type {m_unlockType} {m_customAcquireText}"), 
			_ => string.Format($"Unlock Type {m_unlockType}"), 
		};
	}
}
