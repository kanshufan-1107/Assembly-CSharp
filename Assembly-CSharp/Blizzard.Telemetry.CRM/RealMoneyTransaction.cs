using System.IO;
using System.Text;

namespace Blizzard.Telemetry.CRM;

public class RealMoneyTransaction : IProtoBuf
{
	public bool HasApplicationId;

	private string _ApplicationId;

	public bool HasAppStore;

	private string _AppStore;

	public bool HasReceipt;

	private string _Receipt;

	public bool HasReceiptSignature;

	private string _ReceiptSignature;

	public bool HasProductId;

	private string _ProductId;

	public bool HasItemCost;

	private string _ItemCost;

	public bool HasItemQuantity;

	private string _ItemQuantity;

	public bool HasLocalCurrency;

	private string _LocalCurrency;

	public bool HasTransactionId;

	private string _TransactionId;

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

	public string AppStore
	{
		get
		{
			return _AppStore;
		}
		set
		{
			_AppStore = value;
			HasAppStore = value != null;
		}
	}

	public string Receipt
	{
		get
		{
			return _Receipt;
		}
		set
		{
			_Receipt = value;
			HasReceipt = value != null;
		}
	}

	public string ReceiptSignature
	{
		get
		{
			return _ReceiptSignature;
		}
		set
		{
			_ReceiptSignature = value;
			HasReceiptSignature = value != null;
		}
	}

	public string ProductId
	{
		get
		{
			return _ProductId;
		}
		set
		{
			_ProductId = value;
			HasProductId = value != null;
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

	public string LocalCurrency
	{
		get
		{
			return _LocalCurrency;
		}
		set
		{
			_LocalCurrency = value;
			HasLocalCurrency = value != null;
		}
	}

	public string TransactionId
	{
		get
		{
			return _TransactionId;
		}
		set
		{
			_TransactionId = value;
			HasTransactionId = value != null;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasApplicationId)
		{
			hash ^= ApplicationId.GetHashCode();
		}
		if (HasAppStore)
		{
			hash ^= AppStore.GetHashCode();
		}
		if (HasReceipt)
		{
			hash ^= Receipt.GetHashCode();
		}
		if (HasReceiptSignature)
		{
			hash ^= ReceiptSignature.GetHashCode();
		}
		if (HasProductId)
		{
			hash ^= ProductId.GetHashCode();
		}
		if (HasItemCost)
		{
			hash ^= ItemCost.GetHashCode();
		}
		if (HasItemQuantity)
		{
			hash ^= ItemQuantity.GetHashCode();
		}
		if (HasLocalCurrency)
		{
			hash ^= LocalCurrency.GetHashCode();
		}
		if (HasTransactionId)
		{
			hash ^= TransactionId.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is RealMoneyTransaction other))
		{
			return false;
		}
		if (HasApplicationId != other.HasApplicationId || (HasApplicationId && !ApplicationId.Equals(other.ApplicationId)))
		{
			return false;
		}
		if (HasAppStore != other.HasAppStore || (HasAppStore && !AppStore.Equals(other.AppStore)))
		{
			return false;
		}
		if (HasReceipt != other.HasReceipt || (HasReceipt && !Receipt.Equals(other.Receipt)))
		{
			return false;
		}
		if (HasReceiptSignature != other.HasReceiptSignature || (HasReceiptSignature && !ReceiptSignature.Equals(other.ReceiptSignature)))
		{
			return false;
		}
		if (HasProductId != other.HasProductId || (HasProductId && !ProductId.Equals(other.ProductId)))
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
		if (HasLocalCurrency != other.HasLocalCurrency || (HasLocalCurrency && !LocalCurrency.Equals(other.LocalCurrency)))
		{
			return false;
		}
		if (HasTransactionId != other.HasTransactionId || (HasTransactionId && !TransactionId.Equals(other.TransactionId)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static RealMoneyTransaction Deserialize(Stream stream, RealMoneyTransaction instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static RealMoneyTransaction Deserialize(Stream stream, RealMoneyTransaction instance, long limit)
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
						instance.AppStore = ProtocolParser.ReadString(stream);
					}
					break;
				case 30u:
					if (key.WireType == Wire.LengthDelimited)
					{
						instance.Receipt = ProtocolParser.ReadString(stream);
					}
					break;
				case 40u:
					if (key.WireType == Wire.LengthDelimited)
					{
						instance.ReceiptSignature = ProtocolParser.ReadString(stream);
					}
					break;
				case 50u:
					if (key.WireType == Wire.LengthDelimited)
					{
						instance.ProductId = ProtocolParser.ReadString(stream);
					}
					break;
				case 60u:
					if (key.WireType == Wire.LengthDelimited)
					{
						instance.ItemCost = ProtocolParser.ReadString(stream);
					}
					break;
				case 70u:
					if (key.WireType == Wire.LengthDelimited)
					{
						instance.ItemQuantity = ProtocolParser.ReadString(stream);
					}
					break;
				case 80u:
					if (key.WireType == Wire.LengthDelimited)
					{
						instance.LocalCurrency = ProtocolParser.ReadString(stream);
					}
					break;
				case 900u:
					if (key.WireType == Wire.LengthDelimited)
					{
						instance.TransactionId = ProtocolParser.ReadString(stream);
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

	public static void Serialize(Stream stream, RealMoneyTransaction instance)
	{
		if (instance.HasApplicationId)
		{
			stream.WriteByte(82);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.ApplicationId));
		}
		if (instance.HasAppStore)
		{
			stream.WriteByte(162);
			stream.WriteByte(1);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.AppStore));
		}
		if (instance.HasReceipt)
		{
			stream.WriteByte(242);
			stream.WriteByte(1);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.Receipt));
		}
		if (instance.HasReceiptSignature)
		{
			stream.WriteByte(194);
			stream.WriteByte(2);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.ReceiptSignature));
		}
		if (instance.HasProductId)
		{
			stream.WriteByte(146);
			stream.WriteByte(3);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.ProductId));
		}
		if (instance.HasItemCost)
		{
			stream.WriteByte(226);
			stream.WriteByte(3);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.ItemCost));
		}
		if (instance.HasItemQuantity)
		{
			stream.WriteByte(178);
			stream.WriteByte(4);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.ItemQuantity));
		}
		if (instance.HasLocalCurrency)
		{
			stream.WriteByte(130);
			stream.WriteByte(5);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.LocalCurrency));
		}
		if (instance.HasTransactionId)
		{
			stream.WriteByte(162);
			stream.WriteByte(56);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.TransactionId));
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
		if (HasAppStore)
		{
			size += 2;
			uint byteCount20 = (uint)Encoding.UTF8.GetByteCount(AppStore);
			size += ProtocolParser.SizeOfUInt32(byteCount20) + byteCount20;
		}
		if (HasReceipt)
		{
			size += 2;
			uint byteCount30 = (uint)Encoding.UTF8.GetByteCount(Receipt);
			size += ProtocolParser.SizeOfUInt32(byteCount30) + byteCount30;
		}
		if (HasReceiptSignature)
		{
			size += 2;
			uint byteCount40 = (uint)Encoding.UTF8.GetByteCount(ReceiptSignature);
			size += ProtocolParser.SizeOfUInt32(byteCount40) + byteCount40;
		}
		if (HasProductId)
		{
			size += 2;
			uint byteCount50 = (uint)Encoding.UTF8.GetByteCount(ProductId);
			size += ProtocolParser.SizeOfUInt32(byteCount50) + byteCount50;
		}
		if (HasItemCost)
		{
			size += 2;
			uint byteCount60 = (uint)Encoding.UTF8.GetByteCount(ItemCost);
			size += ProtocolParser.SizeOfUInt32(byteCount60) + byteCount60;
		}
		if (HasItemQuantity)
		{
			size += 2;
			uint byteCount70 = (uint)Encoding.UTF8.GetByteCount(ItemQuantity);
			size += ProtocolParser.SizeOfUInt32(byteCount70) + byteCount70;
		}
		if (HasLocalCurrency)
		{
			size += 2;
			uint byteCount80 = (uint)Encoding.UTF8.GetByteCount(LocalCurrency);
			size += ProtocolParser.SizeOfUInt32(byteCount80) + byteCount80;
		}
		if (HasTransactionId)
		{
			size += 2;
			uint byteCount900 = (uint)Encoding.UTF8.GetByteCount(TransactionId);
			size += ProtocolParser.SizeOfUInt32(byteCount900) + byteCount900;
		}
		return size;
	}
}
