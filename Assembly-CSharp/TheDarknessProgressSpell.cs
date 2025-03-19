public class TheDarknessProgressSpell : Spell
{
	public UberText m_ProgressText;

	public override bool AddPowerTargets()
	{
		if (!base.AddPowerTargets())
		{
			return false;
		}
		Card source = GetSourceCard();
		int current = 0;
		if (m_taskList == null || !m_taskList.GetTagUpdatedValue(source?.GetEntity(), GAME_TAG.SCORE_VALUE_2, ref current))
		{
			return false;
		}
		int total = (source?.GetEntity()?.GetTag(GAME_TAG.SCORE_VALUE_1)).GetValueOrDefault();
		m_ProgressText.Text = GameStrings.Format("GAMEPLAY_PROGRESS_X_OF_Y", current, total);
		return true;
	}
}
