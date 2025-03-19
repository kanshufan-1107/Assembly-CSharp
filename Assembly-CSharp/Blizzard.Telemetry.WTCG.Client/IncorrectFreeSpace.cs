using System.IO;

namespace Blizzard.Telemetry.WTCG.Client;

public class IncorrectFreeSpace : IProtoBuf
{
	public bool HasNewMethod;

	private long _NewMethod;

	public bool HasOldMethod;

	private long _OldMethod;

	public long NewMethod
	{
		get
		{
			return _NewMethod;
		}
		set
		{
			_NewMethod = value;
			HasNewMethod = true;
		}
	}

	public long OldMethod
	{
		get
		{
			return _OldMethod;
		}
		set
		{
			_OldMethod = value;
			HasOldMethod = true;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasNewMethod)
		{
			hash ^= NewMethod.GetHashCode();
		}
		if (HasOldMethod)
		{
			hash ^= OldMethod.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is IncorrectFreeSpace other))
		{
			return false;
		}
		if (HasNewMethod != other.HasNewMethod || (HasNewMethod && !NewMethod.Equals(other.NewMethod)))
		{
			return false;
		}
		if (HasOldMethod != other.HasOldMethod || (HasOldMethod && !OldMethod.Equals(other.OldMethod)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static IncorrectFreeSpace Deserialize(Stream stream, IncorrectFreeSpace instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static IncorrectFreeSpace DeserializeLengthDelimited(Stream stream)
	{
		IncorrectFreeSpace instance = new IncorrectFreeSpace();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static IncorrectFreeSpace DeserializeLengthDelimited(Stream stream, IncorrectFreeSpace instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static IncorrectFreeSpace Deserialize(Stream stream, IncorrectFreeSpace instance, long limit)
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
				instance.NewMethod = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			case 24:
				instance.OldMethod = (long)ProtocolParser.ReadUInt64(stream);
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

	public static void Serialize(Stream stream, IncorrectFreeSpace instance)
	{
		if (instance.HasNewMethod)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.NewMethod);
		}
		if (instance.HasOldMethod)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.OldMethod);
		}
	}

	public uint GetSerializedSize()
	{
		uint size = 0u;
		if (HasNewMethod)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)NewMethod);
		}
		if (HasOldMethod)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)OldMethod);
		}
		return size;
	}
}
