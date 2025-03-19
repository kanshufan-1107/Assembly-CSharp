using System.IO;
using System.Text;

namespace Blizzard.Telemetry.WTCG.Client;

public class Product : IProtoBuf
{
	public bool HasProductId;

	private long _ProductId;

	public bool HasHsProductType;

	private string _HsProductType;

	public bool HasHsProductId;

	private int _HsProductId;

	public long ProductId
	{
		get
		{
			return _ProductId;
		}
		set
		{
			_ProductId = value;
			HasProductId = true;
		}
	}

	public string HsProductType
	{
		get
		{
			return _HsProductType;
		}
		set
		{
			_HsProductType = value;
			HasHsProductType = value != null;
		}
	}

	public int HsProductId
	{
		get
		{
			return _HsProductId;
		}
		set
		{
			_HsProductId = value;
			HasHsProductId = true;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasProductId)
		{
			hash ^= ProductId.GetHashCode();
		}
		if (HasHsProductType)
		{
			hash ^= HsProductType.GetHashCode();
		}
		if (HasHsProductId)
		{
			hash ^= HsProductId.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is Product other))
		{
			return false;
		}
		if (HasProductId != other.HasProductId || (HasProductId && !ProductId.Equals(other.ProductId)))
		{
			return false;
		}
		if (HasHsProductType != other.HasHsProductType || (HasHsProductType && !HsProductType.Equals(other.HsProductType)))
		{
			return false;
		}
		if (HasHsProductId != other.HasHsProductId || (HasHsProductId && !HsProductId.Equals(other.HsProductId)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static Product Deserialize(Stream stream, Product instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static Product DeserializeLengthDelimited(Stream stream)
	{
		Product instance = new Product();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static Product DeserializeLengthDelimited(Stream stream, Product instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static Product Deserialize(Stream stream, Product instance, long limit)
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
				instance.ProductId = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			case 18:
				instance.HsProductType = ProtocolParser.ReadString(stream);
				continue;
			case 24:
				instance.HsProductId = (int)ProtocolParser.ReadUInt64(stream);
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

	public static void Serialize(Stream stream, Product instance)
	{
		if (instance.HasProductId)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.ProductId);
		}
		if (instance.HasHsProductType)
		{
			stream.WriteByte(18);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.HsProductType));
		}
		if (instance.HasHsProductId)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.HsProductId);
		}
	}

	public uint GetSerializedSize()
	{
		uint size = 0u;
		if (HasProductId)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)ProductId);
		}
		if (HasHsProductType)
		{
			size++;
			uint byteCount2 = (uint)Encoding.UTF8.GetByteCount(HsProductType);
			size += ProtocolParser.SizeOfUInt32(byteCount2) + byteCount2;
		}
		if (HasHsProductId)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)HsProductId);
		}
		return size;
	}
}
