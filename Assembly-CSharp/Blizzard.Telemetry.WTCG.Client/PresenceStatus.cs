using System.IO;

namespace Blizzard.Telemetry.WTCG.Client;

public class PresenceStatus : IProtoBuf
{
	public bool HasPresenceId;

	private long _PresenceId;

	public bool HasPresenceSubId;

	private long _PresenceSubId;

	public long PresenceId
	{
		get
		{
			return _PresenceId;
		}
		set
		{
			_PresenceId = value;
			HasPresenceId = true;
		}
	}

	public long PresenceSubId
	{
		get
		{
			return _PresenceSubId;
		}
		set
		{
			_PresenceSubId = value;
			HasPresenceSubId = true;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasPresenceId)
		{
			hash ^= PresenceId.GetHashCode();
		}
		if (HasPresenceSubId)
		{
			hash ^= PresenceSubId.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is PresenceStatus other))
		{
			return false;
		}
		if (HasPresenceId != other.HasPresenceId || (HasPresenceId && !PresenceId.Equals(other.PresenceId)))
		{
			return false;
		}
		if (HasPresenceSubId != other.HasPresenceSubId || (HasPresenceSubId && !PresenceSubId.Equals(other.PresenceSubId)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static PresenceStatus Deserialize(Stream stream, PresenceStatus instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static PresenceStatus DeserializeLengthDelimited(Stream stream)
	{
		PresenceStatus instance = new PresenceStatus();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static PresenceStatus DeserializeLengthDelimited(Stream stream, PresenceStatus instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static PresenceStatus Deserialize(Stream stream, PresenceStatus instance, long limit)
	{
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
			case 8:
				instance.PresenceId = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			case 16:
				instance.PresenceSubId = (long)ProtocolParser.ReadUInt64(stream);
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

	public static void Serialize(Stream stream, PresenceStatus instance)
	{
		if (instance.HasPresenceId)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.PresenceId);
		}
		if (instance.HasPresenceSubId)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.PresenceSubId);
		}
	}

	public uint GetSerializedSize()
	{
		uint size = 0u;
		if (HasPresenceId)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)PresenceId);
		}
		if (HasPresenceSubId)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)PresenceSubId);
		}
		return size;
	}
}
