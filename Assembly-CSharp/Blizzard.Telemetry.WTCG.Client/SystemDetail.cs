using System.IO;

namespace Blizzard.Telemetry.WTCG.Client;

public class SystemDetail : IProtoBuf
{
	public bool HasInfo;

	private UnitySystemInfo _Info;

	public UnitySystemInfo Info
	{
		get
		{
			return _Info;
		}
		set
		{
			_Info = value;
			HasInfo = value != null;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasInfo)
		{
			hash ^= Info.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is SystemDetail other))
		{
			return false;
		}
		if (HasInfo != other.HasInfo || (HasInfo && !Info.Equals(other.Info)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static SystemDetail Deserialize(Stream stream, SystemDetail instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static SystemDetail DeserializeLengthDelimited(Stream stream)
	{
		SystemDetail instance = new SystemDetail();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static SystemDetail DeserializeLengthDelimited(Stream stream, SystemDetail instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static SystemDetail Deserialize(Stream stream, SystemDetail instance, long limit)
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
			case 10:
				if (instance.Info == null)
				{
					instance.Info = UnitySystemInfo.DeserializeLengthDelimited(stream);
				}
				else
				{
					UnitySystemInfo.DeserializeLengthDelimited(stream, instance.Info);
				}
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

	public static void Serialize(Stream stream, SystemDetail instance)
	{
		if (instance.HasInfo)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteUInt32(stream, instance.Info.GetSerializedSize());
			UnitySystemInfo.Serialize(stream, instance.Info);
		}
	}

	public uint GetSerializedSize()
	{
		uint size = 0u;
		if (HasInfo)
		{
			size++;
			uint size2 = Info.GetSerializedSize();
			size += size2 + ProtocolParser.SizeOfUInt32(size2);
		}
		return size;
	}
}
