public class ScrollBarThumb : PegUIElement
{
	private bool m_isBarDragging;

	private void Update()
	{
		if (IsDragging() && InputCollection.GetMouseButtonUp(0))
		{
			StopDragging();
		}
	}

	public bool IsDragging()
	{
		return m_isBarDragging;
	}

	public void StartDragging()
	{
		m_isBarDragging = true;
	}

	public void StopDragging()
	{
		m_isBarDragging = false;
	}

	protected override void OnDrag()
	{
		StartDragging();
	}
}
