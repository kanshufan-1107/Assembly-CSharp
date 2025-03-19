using System.IO;

namespace Blizzard.Telemetry.WTCG.NGDP;

public class VersionStarted : IProtoBuf
{
	public bool HasDummy;

	private int _Dummy;

	public int Dummy
	{
		get
		{
			return _Dummy;
		}
		set
		{
			_Dummy = value;
			HasDummy = true;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasDummy)
		{
			hash ^= Dummy.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is VersionStarted other))
		{
			return false;
		}
		if (HasDummy != other.HasDummy || (HasDummy && !Dummy.Equals(other.Dummy)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static VersionStarted Deserialize(Stream stream, VersionStarted instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static VersionStarted DeserializeLengthDelimited(Stream stream)
	{
		VersionStarted instance = new VersionStarted();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static VersionStarted DeserializeLengthDelimited(Stream stream, VersionStarted instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static VersionStarted Deserialize(Stream stream, VersionStarted instance, long limit)
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
				instance.Dummy = (int)ProtocolParser.ReadUInt64(stream);
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

	public static void Serialize(Stream stream, VersionStarted instance)
	{
		if (instance.HasDummy)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.Dummy);
		}
	}

	public uint GetSerializedSize()
	{
		uint size = 0u;
		if (HasDummy)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)Dummy);
		}
		return size;
	}
}
