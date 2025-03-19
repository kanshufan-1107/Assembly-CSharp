using Blizzard.GameService.SDK.Client.Integration;

public class BnetInvitation
{
	private BnetInvitationId m_id;

	private BnetEntityId m_inviterId;

	private string m_inviterName;

	private BnetEntityId m_inviteeId;

	private string m_inviteeName;

	private string m_message;

	private ulong m_creationTimeMicrosec;

	private ulong m_expirationTimeMicrosec;

	public static BnetInvitation CreateFromFriendsUpdate(FriendsUpdate src)
	{
		BnetInvitation dst = new BnetInvitation();
		dst.m_id = new BnetInvitationId(src.long1);
		if (src.entity1 != null)
		{
			dst.m_inviterId = src.entity1.Clone();
		}
		if (src.entity2 != null)
		{
			dst.m_inviteeId = src.entity2.Clone();
		}
		dst.m_inviterName = src.string1;
		dst.m_inviteeName = src.string2;
		dst.m_message = src.string3;
		dst.m_creationTimeMicrosec = src.long2;
		dst.m_expirationTimeMicrosec = src.long3;
		return dst;
	}

	public BnetInvitationId GetId()
	{
		return m_id;
	}

	public BnetEntityId GetInviterId()
	{
		return m_inviterId;
	}

	public string GetInviterName()
	{
		return m_inviterName;
	}

	public ulong GetCreationTimeMicrosec()
	{
		return m_creationTimeMicrosec;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (!(obj is BnetInvitation other))
		{
			return false;
		}
		return m_id.Equals(other.m_id);
	}

	public override int GetHashCode()
	{
		return m_id.GetHashCode();
	}

	public static bool operator ==(BnetInvitation a, BnetInvitation b)
	{
		if ((object)a == b)
		{
			return true;
		}
		if ((object)a == null || (object)b == null)
		{
			return false;
		}
		return a.m_id == b.m_id;
	}

	public override string ToString()
	{
		if (m_id == null)
		{
			return "UNKNOWN INVITATION";
		}
		return $"[id={m_id} inviterId={m_inviterId} inviterName={m_inviterName} inviteeId={m_inviteeId} inviteeName={m_inviteeName} message={m_message}]";
	}
}
