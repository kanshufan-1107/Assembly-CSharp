using UnityEngine;

public class BnetBattleTag
{
	private string m_name;

	private string m_number;

	public static BnetBattleTag CreateFromString(string src)
	{
		BnetBattleTag dst = new BnetBattleTag();
		if (!dst.SetString(src))
		{
			return null;
		}
		return dst;
	}

	public BnetBattleTag Clone()
	{
		return (BnetBattleTag)MemberwiseClone();
	}

	public string GetName()
	{
		return m_name;
	}

	public void SetName(string name)
	{
		m_name = name;
	}

	public string GetNumber()
	{
		return m_number;
	}

	public void SetNumber(string number)
	{
		m_number = number;
	}

	public string GetString()
	{
		return $"{m_name}#{m_number}";
	}

	public bool SetString(string composite)
	{
		if (composite == null)
		{
			Error.AddDevFatal("BnetBattleTag.SetString() - Given null string.");
			return false;
		}
		string[] parts = composite.Split('#');
		if (parts.Length < 2)
		{
			Debug.LogWarningFormat("BnetBattleTag.SetString() - Failed to split BattleTag \"{0}\" into 2 parts - this will prevent this player from showing up in Friends list and other places.", composite);
			return false;
		}
		m_name = parts[0];
		m_number = parts[1];
		return true;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (!(obj is BnetBattleTag other))
		{
			return false;
		}
		if (m_name == other.m_name)
		{
			return m_number == other.m_number;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (17 * 11 + m_name.GetHashCode()) * 11 + m_number.GetHashCode();
	}

	public static bool operator ==(BnetBattleTag a, BnetBattleTag b)
	{
		if ((object)a == b)
		{
			return true;
		}
		if ((object)a == null || (object)b == null)
		{
			return false;
		}
		if (a.m_name == b.m_name)
		{
			return a.m_number == b.m_number;
		}
		return false;
	}

	public static bool operator !=(BnetBattleTag a, BnetBattleTag b)
	{
		return !(a == b);
	}

	public override string ToString()
	{
		return $"{m_name}#{m_number}";
	}
}
