[CustomEditClass]
public class AdventureBossDefVariableIntroLines : AdventureBossDef
{
	[CustomEditField(Sections = "Intro Line")]
	public string m_IntroLineAchievementProgress;

	[CustomEditField(Sections = "Intro Line")]
	public string m_IntroLineAchievementComplete;

	[CustomEditField(Sections = "Intro Line")]
	public AchievementDbId m_IntroLineReferenceAchievement;

	public override string GetIntroLine()
	{
		Achievement mainAchievement = AchieveManager.Get().GetAchievement((int)m_IntroLineReferenceAchievement);
		if (mainAchievement == null)
		{
			return m_IntroLine;
		}
		if (!string.IsNullOrEmpty(m_IntroLineAchievementComplete) && mainAchievement.IsCompleted())
		{
			return m_IntroLineAchievementComplete;
		}
		if (!string.IsNullOrEmpty(m_IntroLineAchievementProgress) && mainAchievement.Progress > 0 && mainAchievement.Progress < mainAchievement.MaxProgress)
		{
			return m_IntroLineAchievementProgress;
		}
		return m_IntroLine;
	}
}
