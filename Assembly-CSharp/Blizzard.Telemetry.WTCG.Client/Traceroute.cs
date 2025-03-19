using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Blizzard.Telemetry.WTCG.Client;

public class Traceroute : IProtoBuf
{
	public bool HasHost;

	private string _Host;

	private List<string> _Hops = new List<string>();

	public bool HasTotalHops;

	private int _TotalHops;

	public bool HasFailHops;

	private int _FailHops;

	public bool HasSuccessHops;

	private int _SuccessHops;

	public string Host
	{
		get
		{
			return _Host;
		}
		set
		{
			_Host = value;
			HasHost = value != null;
		}
	}

	public List<string> Hops
	{
		get
		{
			return _Hops;
		}
		set
		{
			_Hops = value;
		}
	}

	public int TotalHops
	{
		get
		{
			return _TotalHops;
		}
		set
		{
			_TotalHops = value;
			HasTotalHops = true;
		}
	}

	public int FailHops
	{
		get
		{
			return _FailHops;
		}
		set
		{
			_FailHops = value;
			HasFailHops = true;
		}
	}

	public int SuccessHops
	{
		get
		{
			return _SuccessHops;
		}
		set
		{
			_SuccessHops = value;
			HasSuccessHops = true;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasHost)
		{
			hash ^= Host.GetHashCode();
		}
		foreach (string i in Hops)
		{
			hash ^= i.GetHashCode();
		}
		if (HasTotalHops)
		{
			hash ^= TotalHops.GetHashCode();
		}
		if (HasFailHops)
		{
			hash ^= FailHops.GetHashCode();
		}
		if (HasSuccessHops)
		{
			hash ^= SuccessHops.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is Traceroute other))
		{
			return false;
		}
		if (HasHost != other.HasHost || (HasHost && !Host.Equals(other.Host)))
		{
			return false;
		}
		if (Hops.Count != other.Hops.Count)
		{
			return false;
		}
		for (int i = 0; i < Hops.Count; i++)
		{
			if (!Hops[i].Equals(other.Hops[i]))
			{
				return false;
			}
		}
		if (HasTotalHops != other.HasTotalHops || (HasTotalHops && !TotalHops.Equals(other.TotalHops)))
		{
			return false;
		}
		if (HasFailHops != other.HasFailHops || (HasFailHops && !FailHops.Equals(other.FailHops)))
		{
			return false;
		}
		if (HasSuccessHops != other.HasSuccessHops || (HasSuccessHops && !SuccessHops.Equals(other.SuccessHops)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static Traceroute Deserialize(Stream stream, Traceroute instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static Traceroute DeserializeLengthDelimited(Stream stream)
	{
		Traceroute instance = new Traceroute();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static Traceroute DeserializeLengthDelimited(Stream stream, Traceroute instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static Traceroute Deserialize(Stream stream, Traceroute instance, long limit)
	{
		if (instance.Hops == null)
		{
			instance.Hops = new List<string>();
		}
		while (true)
		{
			if (limit >= 0 && stream.Position >= limit)
			{
				if (stream.Position == limit)
				{
					break;
				}
				throw new ProtocolBufferException("Read past max limit");
			}
			int keyByte = stream.ReadByte();
			switch (keyByte)
			{
			case -1:
				break;
			case 10:
				instance.Host = ProtocolParser.ReadString(stream);
				continue;
			case 26:
				instance.Hops.Add(ProtocolParser.ReadString(stream));
				continue;
			case 40:
				instance.TotalHops = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 56:
				instance.FailHops = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 72:
				instance.SuccessHops = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			default:
			{
				Key key = ProtocolParser.ReadKey((byte)keyByte, stream);
				if (key.Field == 0)
				{
					throw new ProtocolBufferException("Invalid field id: 0, something went wrong in the stream");
				}
				ProtocolParser.SkipKey(stream, key);
				continue;
			}
			}
			if (limit < 0)
			{
				break;
			}
			throw new EndOfStreamException();
		}
		return instance;
	}

	public void Serialize(Stream stream)
	{
		Serialize(stream, this);
	}

	public static void Serialize(Stream stream, Traceroute instance)
	{
		if (instance.HasHost)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.Host));
		}
		if (instance.Hops.Count > 0)
		{
			foreach (string i3 in instance.Hops)
			{
				stream.WriteByte(26);
				ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(i3));
			}
		}
		if (instance.HasTotalHops)
		{
			stream.WriteByte(40);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.TotalHops);
		}
		if (instance.HasFailHops)
		{
			stream.WriteByte(56);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.FailHops);
		}
		if (instance.HasSuccessHops)
		{
			stream.WriteByte(72);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.SuccessHops);
		}
	}

	public uint GetSerializedSize()
	{
		uint size = 0u;
		if (HasHost)
		{
			size++;
			uint byteCount1 = (uint)Encoding.UTF8.GetByteCount(Host);
			size += ProtocolParser.SizeOfUInt32(byteCount1) + byteCount1;
		}
		if (Hops.Count > 0)
		{
			foreach (string i3 in Hops)
			{
				size++;
				uint byteCount3 = (uint)Encoding.UTF8.GetByteCount(i3);
				size += ProtocolParser.SizeOfUInt32(byteCount3) + byteCount3;
			}
		}
		if (HasTotalHops)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)TotalHops);
		}
		if (HasFailHops)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)FailHops);
		}
		if (HasSuccessHops)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)SuccessHops);
		}
		return size;
	}
}
