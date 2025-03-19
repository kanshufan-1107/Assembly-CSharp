using System.IO;

namespace Blizzard.Telemetry.WTCG.Client;

public class ChangePackQuantity : IProtoBuf
{
	public bool HasBoosterId;

	private int _BoosterId;

	public int BoosterId
	{
		get
		{
			return _BoosterId;
		}
		set
		{
			_BoosterId = value;
			HasBoosterId = true;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasBoosterId)
		{
			hash ^= BoosterId.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is ChangePackQuantity other))
		{
			return false;
		}
		if (HasBoosterId != other.HasBoosterId || (HasBoosterId && !BoosterId.Equals(other.BoosterId)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static ChangePackQuantity Deserialize(Stream stream, ChangePackQuantity instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static ChangePackQuantity DeserializeLengthDelimited(Stream stream)
	{
		ChangePackQuantity instance = new ChangePackQuantity();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static ChangePackQuantity DeserializeLengthDelimited(Stream stream, ChangePackQuantity instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static ChangePackQuantity Deserialize(Stream stream, ChangePackQuantity instance, long limit)
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
				instance.BoosterId = (int)ProtocolParser.ReadUInt64(stream);
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

	public static void Serialize(Stream stream, ChangePackQuantity instance)
	{
		if (instance.HasBoosterId)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.BoosterId);
		}
	}

	public uint GetSerializedSize()
	{
		uint size = 0u;
		if (HasBoosterId)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)BoosterId);
		}
		return size;
	}
}
