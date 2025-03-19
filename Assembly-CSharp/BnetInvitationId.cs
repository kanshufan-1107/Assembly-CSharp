public class BnetInvitationId
{
	private ulong m_val;

	public BnetInvitationId(ulong val)
	{
		m_val = val;
	}

	public ulong GetVal()
	{
		return m_val;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (!(obj is BnetInvitationId other))
		{
			return false;
		}
		return m_val == other.m_val;
	}

	public bool Equals(BnetInvitationId other)
	{
		if ((object)other == null)
		{
			return false;
		}
		return m_val == other.m_val;
	}

	public override int GetHashCode()
	{
		return m_val.GetHashCode();
	}

	public static bool operator ==(BnetInvitationId a, BnetInvitationId b)
	{
		if ((object)a == b)
		{
			return true;
		}
		if ((object)a == null || (object)b == null)
		{
			return false;
		}
		return a.m_val == b.m_val;
	}

	public override string ToString()
	{
		return m_val.ToString();
	}
}
