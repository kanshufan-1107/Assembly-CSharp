using System.IO;

namespace Blizzard.Telemetry.WTCG.Client;

public class RepairPrestep : IProtoBuf
{
	public bool HasDoubletapFingers;

	private int _DoubletapFingers;

	public bool HasLocales;

	private int _Locales;

	public int DoubletapFingers
	{
		get
		{
			return _DoubletapFingers;
		}
		set
		{
			_DoubletapFingers = value;
			HasDoubletapFingers = true;
		}
	}

	public int Locales
	{
		get
		{
			return _Locales;
		}
		set
		{
			_Locales = value;
			HasLocales = true;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasDoubletapFingers)
		{
			hash ^= DoubletapFingers.GetHashCode();
		}
		if (HasLocales)
		{
			hash ^= Locales.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is RepairPrestep other))
		{
			return false;
		}
		if (HasDoubletapFingers != other.HasDoubletapFingers || (HasDoubletapFingers && !DoubletapFingers.Equals(other.DoubletapFingers)))
		{
			return false;
		}
		if (HasLocales != other.HasLocales || (HasLocales && !Locales.Equals(other.Locales)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static RepairPrestep Deserialize(Stream stream, RepairPrestep instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static RepairPrestep DeserializeLengthDelimited(Stream stream)
	{
		RepairPrestep instance = new RepairPrestep();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static RepairPrestep DeserializeLengthDelimited(Stream stream, RepairPrestep instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static RepairPrestep Deserialize(Stream stream, RepairPrestep instance, long limit)
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
				instance.DoubletapFingers = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 16:
				instance.Locales = (int)ProtocolParser.ReadUInt64(stream);
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

	public static void Serialize(Stream stream, RepairPrestep instance)
	{
		if (instance.HasDoubletapFingers)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.DoubletapFingers);
		}
		if (instance.HasLocales)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.Locales);
		}
	}

	public uint GetSerializedSize()
	{
		uint size = 0u;
		if (HasDoubletapFingers)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)DoubletapFingers);
		}
		if (HasLocales)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)Locales);
		}
		return size;
	}
}
