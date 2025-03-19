public class JadeGolemCardTextBuilder : CardTextBuilder
{
	protected bool m_showJadeGolemStatsInPlay;

	public JadeGolemCardTextBuilder()
	{
		m_useEntityForTextInPlay = true;
		m_showJadeGolemStatsInPlay = false;
	}

	public override string BuildCardTextInHand(Entity entity)
	{
		string builtText = base.BuildCardTextInHand(entity);
		int delimiterIndex = builtText.IndexOf('@');
		if (delimiterIndex >= 0)
		{
			builtText = ((entity.GetZone() != TAG_ZONE.PLAY || m_showJadeGolemStatsInPlay) ? builtText.Substring(0, delimiterIndex) : builtText.Substring(delimiterIndex + 1));
		}
		return FormatJadeGolemText(builtText, entity.GetJadeGolem());
	}

	private string FormatJadeGolemText(string builtText, int jadeGolemValue)
	{
		string englishArticleConsonant = "";
		if (jadeGolemValue == 8 || jadeGolemValue == 11 || jadeGolemValue == 18)
		{
			englishArticleConsonant = "n";
		}
		return TextUtils.TryFormat(builtText, jadeGolemValue + "/" + jadeGolemValue, englishArticleConsonant);
	}

	public override string BuildCardTextInHand(EntityDef entityDef)
	{
		string builtText = base.BuildCardTextInHand(entityDef);
		int delimiterIndex = builtText.IndexOf('@');
		if (delimiterIndex >= 0)
		{
			builtText = builtText.Substring(delimiterIndex + 1);
		}
		return builtText;
	}

	public override string BuildCardTextInHistory(Entity entity)
	{
		if (!(entity.GetCardTextHistoryData() is JadeGolemCardTextHistoryData historyData))
		{
			Log.All.Print("JadeGolemCardTextBuilder.BuildCardTextInHistory: entity {0} does not have a JadeGolemCardTextHistoryData object.", entity.GetEntityId());
			return string.Empty;
		}
		string builtText = base.BuildCardTextInHistory(entity);
		int delimiterIndex = builtText.IndexOf('@');
		if (delimiterIndex >= 0)
		{
			builtText = ((!historyData.m_wasInPlay || m_showJadeGolemStatsInPlay) ? builtText.Substring(0, delimiterIndex) : builtText.Substring(delimiterIndex + 1));
		}
		return FormatJadeGolemText(builtText, entity.GetJadeGolem());
	}

	public override CardTextHistoryData CreateCardTextHistoryData()
	{
		return new JadeGolemCardTextHistoryData();
	}
}
