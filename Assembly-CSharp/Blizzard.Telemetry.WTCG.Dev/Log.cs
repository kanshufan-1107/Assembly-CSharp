using System.IO;
using System.Text;

namespace Blizzard.Telemetry.WTCG.Dev;

public class Log : IProtoBuf
{
	public bool HasCategory;

	private string _Category;

	public bool HasDetails;

	private string _Details;

	public string Category
	{
		get
		{
			return _Category;
		}
		set
		{
			_Category = value;
			HasCategory = value != null;
		}
	}

	public string Details
	{
		get
		{
			return _Details;
		}
		set
		{
			_Details = value;
			HasDetails = value != null;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasCategory)
		{
			hash ^= Category.GetHashCode();
		}
		if (HasDetails)
		{
			hash ^= Details.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is Log other))
		{
			return false;
		}
		if (HasCategory != other.HasCategory || (HasCategory && !Category.Equals(other.Category)))
		{
			return false;
		}
		if (HasDetails != other.HasDetails || (HasDetails && !Details.Equals(other.Details)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static Log Deserialize(Stream stream, Log instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static Log DeserializeLengthDelimited(Stream stream)
	{
		Log instance = new Log();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static Log DeserializeLengthDelimited(Stream stream, Log instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static Log Deserialize(Stream stream, Log instance, long limit)
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
				instance.Category = ProtocolParser.ReadString(stream);
				continue;
			case 18:
				instance.Details = ProtocolParser.ReadString(stream);
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

	public static void Serialize(Stream stream, Log instance)
	{
		if (instance.HasCategory)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.Category));
		}
		if (instance.HasDetails)
		{
			stream.WriteByte(18);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.Details));
		}
	}

	public uint GetSerializedSize()
	{
		uint size = 0u;
		if (HasCategory)
		{
			size++;
			uint byteCount1 = (uint)Encoding.UTF8.GetByteCount(Category);
			size += ProtocolParser.SizeOfUInt32(byteCount1) + byteCount1;
		}
		if (HasDetails)
		{
			size++;
			uint byteCount2 = (uint)Encoding.UTF8.GetByteCount(Details);
			size += ProtocolParser.SizeOfUInt32(byteCount2) + byteCount2;
		}
		return size;
	}
}
