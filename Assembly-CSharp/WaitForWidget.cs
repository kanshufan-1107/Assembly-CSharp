using Blizzard.T5.Jobs;
using Hearthstone.UI;

public class WaitForWidget : IJobDependency, IAsyncJobResult
{
	private Widget m_widget;

	public WaitForWidget(Widget widget)
	{
		m_widget = widget;
	}

	public bool IsReady()
	{
		if (m_widget == null)
		{
			return true;
		}
		return m_widget.IsReady;
	}
}
