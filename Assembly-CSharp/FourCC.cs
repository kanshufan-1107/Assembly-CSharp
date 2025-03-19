using System;
using System.Text;

[Serializable]
public class FourCC
{
	protected uint m_value;

	public FourCC()
	{
	}

	public FourCC(string stringVal)
	{
		SetString(stringVal);
	}

	public uint GetValue()
	{
		return m_value;
	}

	public string GetString()
	{
		StringBuilder builder = new StringBuilder(4);
		for (int i = 24; i >= 0; i -= 8)
		{
			char c = (char)((m_value >> i) & 0xFF);
			if (c != 0)
			{
				builder.Append(c);
			}
		}
		return builder.ToString();
	}

	public void SetString(string str)
	{
		m_value = 0u;
		for (int i = 0; i < str.Length && i < 4; i++)
		{
			m_value = (m_value << 8) | (byte)str[i];
		}
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (!(obj is FourCC other))
		{
			return false;
		}
		return m_value == other.m_value;
	}

	public override int GetHashCode()
	{
		return m_value.GetHashCode();
	}

	public override string ToString()
	{
		return GetString();
	}
}
