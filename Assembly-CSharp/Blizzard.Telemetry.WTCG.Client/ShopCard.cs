using System.IO;
using System.Text;

namespace Blizzard.Telemetry.WTCG.Client;

public class ShopCard : IProtoBuf
{
	public bool HasProduct;

	private Product _Product;

	public bool HasSectionName;

	private string _SectionName;

	public bool HasSectionIndex;

	private int _SectionIndex;

	public bool HasSlotIndex;

	private int _SlotIndex;

	public bool HasSecondsRemaining;

	private int _SecondsRemaining;

	public Product Product
	{
		get
		{
			return _Product;
		}
		set
		{
			_Product = value;
			HasProduct = value != null;
		}
	}

	public string SectionName
	{
		get
		{
			return _SectionName;
		}
		set
		{
			_SectionName = value;
			HasSectionName = value != null;
		}
	}

	public int SectionIndex
	{
		get
		{
			return _SectionIndex;
		}
		set
		{
			_SectionIndex = value;
			HasSectionIndex = true;
		}
	}

	public int SlotIndex
	{
		get
		{
			return _SlotIndex;
		}
		set
		{
			_SlotIndex = value;
			HasSlotIndex = true;
		}
	}

	public int SecondsRemaining
	{
		get
		{
			return _SecondsRemaining;
		}
		set
		{
			_SecondsRemaining = value;
			HasSecondsRemaining = true;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasProduct)
		{
			hash ^= Product.GetHashCode();
		}
		if (HasSectionName)
		{
			hash ^= SectionName.GetHashCode();
		}
		if (HasSectionIndex)
		{
			hash ^= SectionIndex.GetHashCode();
		}
		if (HasSlotIndex)
		{
			hash ^= SlotIndex.GetHashCode();
		}
		if (HasSecondsRemaining)
		{
			hash ^= SecondsRemaining.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is ShopCard other))
		{
			return false;
		}
		if (HasProduct != other.HasProduct || (HasProduct && !Product.Equals(other.Product)))
		{
			return false;
		}
		if (HasSectionName != other.HasSectionName || (HasSectionName && !SectionName.Equals(other.SectionName)))
		{
			return false;
		}
		if (HasSectionIndex != other.HasSectionIndex || (HasSectionIndex && !SectionIndex.Equals(other.SectionIndex)))
		{
			return false;
		}
		if (HasSlotIndex != other.HasSlotIndex || (HasSlotIndex && !SlotIndex.Equals(other.SlotIndex)))
		{
			return false;
		}
		if (HasSecondsRemaining != other.HasSecondsRemaining || (HasSecondsRemaining && !SecondsRemaining.Equals(other.SecondsRemaining)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static ShopCard Deserialize(Stream stream, ShopCard instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static ShopCard DeserializeLengthDelimited(Stream stream)
	{
		ShopCard instance = new ShopCard();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static ShopCard DeserializeLengthDelimited(Stream stream, ShopCard instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static ShopCard Deserialize(Stream stream, ShopCard instance, long limit)
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
				if (instance.Product == null)
				{
					instance.Product = Product.DeserializeLengthDelimited(stream);
				}
				else
				{
					Product.DeserializeLengthDelimited(stream, instance.Product);
				}
				continue;
			case 18:
				instance.SectionName = ProtocolParser.ReadString(stream);
				continue;
			case 24:
				instance.SectionIndex = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 32:
				instance.SlotIndex = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 40:
				instance.SecondsRemaining = (int)ProtocolParser.ReadUInt64(stream);
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

	public static void Serialize(Stream stream, ShopCard instance)
	{
		if (instance.HasProduct)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteUInt32(stream, instance.Product.GetSerializedSize());
			Product.Serialize(stream, instance.Product);
		}
		if (instance.HasSectionName)
		{
			stream.WriteByte(18);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.SectionName));
		}
		if (instance.HasSectionIndex)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.SectionIndex);
		}
		if (instance.HasSlotIndex)
		{
			stream.WriteByte(32);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.SlotIndex);
		}
		if (instance.HasSecondsRemaining)
		{
			stream.WriteByte(40);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.SecondsRemaining);
		}
	}

	public uint GetSerializedSize()
	{
		uint size = 0u;
		if (HasProduct)
		{
			size++;
			uint size2 = Product.GetSerializedSize();
			size += size2 + ProtocolParser.SizeOfUInt32(size2);
		}
		if (HasSectionName)
		{
			size++;
			uint byteCount2 = (uint)Encoding.UTF8.GetByteCount(SectionName);
			size += ProtocolParser.SizeOfUInt32(byteCount2) + byteCount2;
		}
		if (HasSectionIndex)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)SectionIndex);
		}
		if (HasSlotIndex)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)SlotIndex);
		}
		if (HasSecondsRemaining)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)SecondsRemaining);
		}
		return size;
	}
}
