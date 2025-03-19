public class EventListener<Delegate>
{
	protected Delegate m_callback;

	protected object m_userData;

	public override bool Equals(object obj)
	{
		if (!(obj is EventListener<Delegate> other))
		{
			return base.Equals(obj);
		}
		if (m_callback.Equals(other.m_callback))
		{
			return m_userData == other.m_userData;
		}
		return false;
	}

	public override int GetHashCode()
	{
		int hash = 23;
		if (m_callback != null)
		{
			hash = hash * 17 + m_callback.GetHashCode();
		}
		if (m_userData != null)
		{
			hash = hash * 17 + m_userData.GetHashCode();
		}
		return hash;
	}

	public Delegate GetCallback()
	{
		return m_callback;
	}

	public void SetCallback(Delegate callback)
	{
		m_callback = callback;
	}

	public void SetUserData(object userData)
	{
		m_userData = userData;
	}
}
