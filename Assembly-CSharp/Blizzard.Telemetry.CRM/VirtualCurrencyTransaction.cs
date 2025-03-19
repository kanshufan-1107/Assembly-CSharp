using System.IO;
using System.Text;

namespace Blizzard.Telemetry.CRM;

public class VirtualCurrencyTransaction : IProtoBuf
{
	public bool HasApplicationId;

	private string _ApplicationId;

	public bool HasItemId;

	private string _ItemId;

	public bool HasItemCost;

	private string _ItemCost;

	public bool HasItemQuantity;

	private string _ItemQuantity;

	public bool HasCurrency;

	private string _Currency;

	public bool HasPayload;

	private string _Payload;

	public string ApplicationId
	{
		get
		{
			return _ApplicationId;
		}
		set
		{
			_ApplicationId = value;
			HasApplicationId = value != null;
		}
	}

	public string ItemId
	{
		get
		{
			return _ItemId;
		}
		set
		{
			_ItemId = value;
			HasItemId = value != null;
		}
	}

	public string ItemCost
	{
		get
		{
			return _ItemCost;
		}
		set
		{
			_ItemCost = value;
			HasItemCost = value != null;
		}
	}

	public string ItemQuantity
	{
		get
		{
			return _ItemQuantity;
		}
		set
		{
			_ItemQuantity = value;
			HasItemQuantity = value != null;
		}
	}

	public string Currency
	{
		get
		{
			return _Currency;
		}
		set
		{
			_Currency = value;
			HasCurrency = value != null;
		}
	}

	public string Payload
	{
		get
		{
			return _Payload;
		}
		set
		{
			_Payload = value;
			HasPayload = value != null;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasApplicationId)
		{
			hash ^= ApplicationId.GetHashCode();
		}
		if (HasItemId)
		{
			hash ^= ItemId.GetHashCode();
		}
		if (HasItemCost)
		{
			hash ^= ItemCost.GetHashCode();
		}
		if (HasItemQuantity)
		{
			hash ^= ItemQuantity.GetHashCode();
		}
		if (HasCurrency)
		{
			hash ^= Currency.GetHashCode();
		}
		if (HasPayload)
		{
			hash ^= Payload.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is VirtualCurrencyTransaction other))
		{
			return false;
		}
		if (HasApplicationId != other.HasApplicationId || (HasApplicationId && !ApplicationId.Equals(other.ApplicationId)))
		{
			return false;
		}
		if (HasItemId != other.HasItemId || (HasItemId && !ItemId.Equals(other.ItemId)))
		{
			return false;
		}
		if (HasItemCost != other.HasItemCost || (HasItemCost && !ItemCost.Equals(other.ItemCost)))
		{
			return false;
		}
		if (HasItemQuantity != other.HasItemQuantity || (HasItemQuantity && !ItemQuantity.Equals(other.ItemQuantity)))
		{
			return false;
		}
		if (HasCurrency != other.HasCurrency || (HasCurrency && !Currency.Equals(other.Currency)))
		{
			return false;
		}
		if (HasPayload != other.HasPayload || (HasPayload && !Payload.Equals(other.Payload)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static VirtualCurrencyTransaction Deserialize(Stream stream, VirtualCurrencyTransaction instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static VirtualCurrencyTransaction Deserialize(Stream stream, VirtualCurrencyTransaction instance, long limit)
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
			case 82:
				instance.ApplicationId = ProtocolParser.ReadString(stream);
				continue;
			default:
			{
				Key key = ProtocolParser.ReadKey((byte)keyByte, stream);
				switch (key.Field)
				{
				case 0u:
					throw new ProtocolBufferException("Invalid field id: 0, something went wrong in the stream");
				case 20u:
					if (key.WireType == Wire.LengthDelimited)
					{
						instance.ItemId = ProtocolParser.ReadString(stream);
					}
					break;
				case 30u:
					if (key.WireType == Wire.LengthDelimited)
					{
						instance.ItemCost = ProtocolParser.ReadString(stream);
					}
					break;
				case 40u:
					if (key.WireType == Wire.LengthDelimited)
					{
						instance.ItemQuantity = ProtocolParser.ReadString(stream);
					}
					break;
				case 50u:
					if (key.WireType == Wire.LengthDelimited)
					{
						instance.Currency = ProtocolParser.ReadString(stream);
					}
					break;
				case 60u:
					if (key.WireType == Wire.LengthDelimited)
					{
						instance.Payload = ProtocolParser.ReadString(stream);
					}
					break;
				default:
					ProtocolParser.SkipKey(stream, key);
					break;
				}
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

	public static void Serialize(Stream stream, VirtualCurrencyTransaction instance)
	{
		if (instance.HasApplicationId)
		{
			stream.WriteByte(82);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.ApplicationId));
		}
		if (instance.HasItemId)
		{
			stream.WriteByte(162);
			stream.WriteByte(1);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.ItemId));
		}
		if (instance.HasItemCost)
		{
			stream.WriteByte(242);
			stream.WriteByte(1);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.ItemCost));
		}
		if (instance.HasItemQuantity)
		{
			stream.WriteByte(194);
			stream.WriteByte(2);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.ItemQuantity));
		}
		if (instance.HasCurrency)
		{
			stream.WriteByte(146);
			stream.WriteByte(3);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.Currency));
		}
		if (instance.HasPayload)
		{
			stream.WriteByte(226);
			stream.WriteByte(3);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.Payload));
		}
	}

	public uint GetSerializedSize()
	{
		uint size = 0u;
		if (HasApplicationId)
		{
			size++;
			uint byteCount10 = (uint)Encoding.UTF8.GetByteCount(ApplicationId);
			size += ProtocolParser.SizeOfUInt32(byteCount10) + byteCount10;
		}
		if (HasItemId)
		{
			size += 2;
			uint byteCount20 = (uint)Encoding.UTF8.GetByteCount(ItemId);
			size += ProtocolParser.SizeOfUInt32(byteCount20) + byteCount20;
		}
		if (HasItemCost)
		{
			size += 2;
			uint byteCount30 = (uint)Encoding.UTF8.GetByteCount(ItemCost);
			size += ProtocolParser.SizeOfUInt32(byteCount30) + byteCount30;
		}
		if (HasItemQuantity)
		{
			size += 2;
			uint byteCount40 = (uint)Encoding.UTF8.GetByteCount(ItemQuantity);
			size += ProtocolParser.SizeOfUInt32(byteCount40) + byteCount40;
		}
		if (HasCurrency)
		{
			size += 2;
			uint byteCount50 = (uint)Encoding.UTF8.GetByteCount(Currency);
			size += ProtocolParser.SizeOfUInt32(byteCount50) + byteCount50;
		}
		if (HasPayload)
		{
			size += 2;
			uint byteCount60 = (uint)Encoding.UTF8.GetByteCount(Payload);
			size += ProtocolParser.SizeOfUInt32(byteCount60) + byteCount60;
		}
		return size;
	}
}
