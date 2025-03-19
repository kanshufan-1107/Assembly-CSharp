using System.IO;
using System.Text;

namespace Blizzard.Telemetry.WTCG.Client;

public class ShopPurchaseEvent : IProtoBuf
{
	public bool HasPlayer;

	private Player _Player;

	public bool HasProduct;

	private Product _Product;

	public bool HasQuantity;

	private int _Quantity;

	public bool HasCurrency;

	private string _Currency;

	public bool HasAmount;

	private double _Amount;

	public bool HasIsGift;

	private bool _IsGift;

	public bool HasStorefront;

	private string _Storefront;

	public bool HasPurchaseComplete;

	private bool _PurchaseComplete;

	public bool HasStoreType;

	private string _StoreType;

	public bool HasRedirectedProductId;

	private string _RedirectedProductId;

	public bool HasShopTab;

	private string _ShopTab;

	public bool HasShopSubTab;

	private string _ShopSubTab;

	public Player Player
	{
		get
		{
			return _Player;
		}
		set
		{
			_Player = value;
			HasPlayer = value != null;
		}
	}

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

	public int Quantity
	{
		get
		{
			return _Quantity;
		}
		set
		{
			_Quantity = value;
			HasQuantity = true;
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

	public double Amount
	{
		get
		{
			return _Amount;
		}
		set
		{
			_Amount = value;
			HasAmount = true;
		}
	}

	public bool IsGift
	{
		get
		{
			return _IsGift;
		}
		set
		{
			_IsGift = value;
			HasIsGift = true;
		}
	}

	public string Storefront
	{
		get
		{
			return _Storefront;
		}
		set
		{
			_Storefront = value;
			HasStorefront = value != null;
		}
	}

	public bool PurchaseComplete
	{
		get
		{
			return _PurchaseComplete;
		}
		set
		{
			_PurchaseComplete = value;
			HasPurchaseComplete = true;
		}
	}

	public string StoreType
	{
		get
		{
			return _StoreType;
		}
		set
		{
			_StoreType = value;
			HasStoreType = value != null;
		}
	}

	public string RedirectedProductId
	{
		get
		{
			return _RedirectedProductId;
		}
		set
		{
			_RedirectedProductId = value;
			HasRedirectedProductId = value != null;
		}
	}

	public string ShopTab
	{
		get
		{
			return _ShopTab;
		}
		set
		{
			_ShopTab = value;
			HasShopTab = value != null;
		}
	}

	public string ShopSubTab
	{
		get
		{
			return _ShopSubTab;
		}
		set
		{
			_ShopSubTab = value;
			HasShopSubTab = value != null;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasPlayer)
		{
			hash ^= Player.GetHashCode();
		}
		if (HasProduct)
		{
			hash ^= Product.GetHashCode();
		}
		if (HasQuantity)
		{
			hash ^= Quantity.GetHashCode();
		}
		if (HasCurrency)
		{
			hash ^= Currency.GetHashCode();
		}
		if (HasAmount)
		{
			hash ^= Amount.GetHashCode();
		}
		if (HasIsGift)
		{
			hash ^= IsGift.GetHashCode();
		}
		if (HasStorefront)
		{
			hash ^= Storefront.GetHashCode();
		}
		if (HasPurchaseComplete)
		{
			hash ^= PurchaseComplete.GetHashCode();
		}
		if (HasStoreType)
		{
			hash ^= StoreType.GetHashCode();
		}
		if (HasRedirectedProductId)
		{
			hash ^= RedirectedProductId.GetHashCode();
		}
		if (HasShopTab)
		{
			hash ^= ShopTab.GetHashCode();
		}
		if (HasShopSubTab)
		{
			hash ^= ShopSubTab.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is ShopPurchaseEvent other))
		{
			return false;
		}
		if (HasPlayer != other.HasPlayer || (HasPlayer && !Player.Equals(other.Player)))
		{
			return false;
		}
		if (HasProduct != other.HasProduct || (HasProduct && !Product.Equals(other.Product)))
		{
			return false;
		}
		if (HasQuantity != other.HasQuantity || (HasQuantity && !Quantity.Equals(other.Quantity)))
		{
			return false;
		}
		if (HasCurrency != other.HasCurrency || (HasCurrency && !Currency.Equals(other.Currency)))
		{
			return false;
		}
		if (HasAmount != other.HasAmount || (HasAmount && !Amount.Equals(other.Amount)))
		{
			return false;
		}
		if (HasIsGift != other.HasIsGift || (HasIsGift && !IsGift.Equals(other.IsGift)))
		{
			return false;
		}
		if (HasStorefront != other.HasStorefront || (HasStorefront && !Storefront.Equals(other.Storefront)))
		{
			return false;
		}
		if (HasPurchaseComplete != other.HasPurchaseComplete || (HasPurchaseComplete && !PurchaseComplete.Equals(other.PurchaseComplete)))
		{
			return false;
		}
		if (HasStoreType != other.HasStoreType || (HasStoreType && !StoreType.Equals(other.StoreType)))
		{
			return false;
		}
		if (HasRedirectedProductId != other.HasRedirectedProductId || (HasRedirectedProductId && !RedirectedProductId.Equals(other.RedirectedProductId)))
		{
			return false;
		}
		if (HasShopTab != other.HasShopTab || (HasShopTab && !ShopTab.Equals(other.ShopTab)))
		{
			return false;
		}
		if (HasShopSubTab != other.HasShopSubTab || (HasShopSubTab && !ShopSubTab.Equals(other.ShopSubTab)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static ShopPurchaseEvent Deserialize(Stream stream, ShopPurchaseEvent instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static ShopPurchaseEvent DeserializeLengthDelimited(Stream stream)
	{
		ShopPurchaseEvent instance = new ShopPurchaseEvent();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static ShopPurchaseEvent DeserializeLengthDelimited(Stream stream, ShopPurchaseEvent instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static ShopPurchaseEvent Deserialize(Stream stream, ShopPurchaseEvent instance, long limit)
	{
		BinaryReader br = new BinaryReader(stream);
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
				if (instance.Player == null)
				{
					instance.Player = Player.DeserializeLengthDelimited(stream);
				}
				else
				{
					Player.DeserializeLengthDelimited(stream, instance.Player);
				}
				continue;
			case 18:
				if (instance.Product == null)
				{
					instance.Product = Product.DeserializeLengthDelimited(stream);
				}
				else
				{
					Product.DeserializeLengthDelimited(stream, instance.Product);
				}
				continue;
			case 24:
				instance.Quantity = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 34:
				instance.Currency = ProtocolParser.ReadString(stream);
				continue;
			case 41:
				instance.Amount = br.ReadDouble();
				continue;
			case 48:
				instance.IsGift = ProtocolParser.ReadBool(stream);
				continue;
			case 58:
				instance.Storefront = ProtocolParser.ReadString(stream);
				continue;
			case 64:
				instance.PurchaseComplete = ProtocolParser.ReadBool(stream);
				continue;
			case 74:
				instance.StoreType = ProtocolParser.ReadString(stream);
				continue;
			case 82:
				instance.RedirectedProductId = ProtocolParser.ReadString(stream);
				continue;
			case 90:
				instance.ShopTab = ProtocolParser.ReadString(stream);
				continue;
			case 98:
				instance.ShopSubTab = ProtocolParser.ReadString(stream);
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

	public static void Serialize(Stream stream, ShopPurchaseEvent instance)
	{
		BinaryWriter bw = new BinaryWriter(stream);
		if (instance.HasPlayer)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteUInt32(stream, instance.Player.GetSerializedSize());
			Player.Serialize(stream, instance.Player);
		}
		if (instance.HasProduct)
		{
			stream.WriteByte(18);
			ProtocolParser.WriteUInt32(stream, instance.Product.GetSerializedSize());
			Product.Serialize(stream, instance.Product);
		}
		if (instance.HasQuantity)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.Quantity);
		}
		if (instance.HasCurrency)
		{
			stream.WriteByte(34);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.Currency));
		}
		if (instance.HasAmount)
		{
			stream.WriteByte(41);
			bw.Write(instance.Amount);
		}
		if (instance.HasIsGift)
		{
			stream.WriteByte(48);
			ProtocolParser.WriteBool(stream, instance.IsGift);
		}
		if (instance.HasStorefront)
		{
			stream.WriteByte(58);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.Storefront));
		}
		if (instance.HasPurchaseComplete)
		{
			stream.WriteByte(64);
			ProtocolParser.WriteBool(stream, instance.PurchaseComplete);
		}
		if (instance.HasStoreType)
		{
			stream.WriteByte(74);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.StoreType));
		}
		if (instance.HasRedirectedProductId)
		{
			stream.WriteByte(82);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.RedirectedProductId));
		}
		if (instance.HasShopTab)
		{
			stream.WriteByte(90);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.ShopTab));
		}
		if (instance.HasShopSubTab)
		{
			stream.WriteByte(98);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.ShopSubTab));
		}
	}

	public uint GetSerializedSize()
	{
		uint size = 0u;
		if (HasPlayer)
		{
			size++;
			uint size2 = Player.GetSerializedSize();
			size += size2 + ProtocolParser.SizeOfUInt32(size2);
		}
		if (HasProduct)
		{
			size++;
			uint size3 = Product.GetSerializedSize();
			size += size3 + ProtocolParser.SizeOfUInt32(size3);
		}
		if (HasQuantity)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)Quantity);
		}
		if (HasCurrency)
		{
			size++;
			uint byteCount4 = (uint)Encoding.UTF8.GetByteCount(Currency);
			size += ProtocolParser.SizeOfUInt32(byteCount4) + byteCount4;
		}
		if (HasAmount)
		{
			size++;
			size += 8;
		}
		if (HasIsGift)
		{
			size++;
			size++;
		}
		if (HasStorefront)
		{
			size++;
			uint byteCount7 = (uint)Encoding.UTF8.GetByteCount(Storefront);
			size += ProtocolParser.SizeOfUInt32(byteCount7) + byteCount7;
		}
		if (HasPurchaseComplete)
		{
			size++;
			size++;
		}
		if (HasStoreType)
		{
			size++;
			uint byteCount9 = (uint)Encoding.UTF8.GetByteCount(StoreType);
			size += ProtocolParser.SizeOfUInt32(byteCount9) + byteCount9;
		}
		if (HasRedirectedProductId)
		{
			size++;
			uint byteCount10 = (uint)Encoding.UTF8.GetByteCount(RedirectedProductId);
			size += ProtocolParser.SizeOfUInt32(byteCount10) + byteCount10;
		}
		if (HasShopTab)
		{
			size++;
			uint byteCount11 = (uint)Encoding.UTF8.GetByteCount(ShopTab);
			size += ProtocolParser.SizeOfUInt32(byteCount11) + byteCount11;
		}
		if (HasShopSubTab)
		{
			size++;
			uint byteCount12 = (uint)Encoding.UTF8.GetByteCount(ShopSubTab);
			size += ProtocolParser.SizeOfUInt32(byteCount12) + byteCount12;
		}
		return size;
	}
}
