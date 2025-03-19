using System.IO;
using System.Text;

namespace Blizzard.Telemetry.WTCG.Client;

public class ShopCardClick : IProtoBuf
{
	public bool HasPlayer;

	private Player _Player;

	public bool HasShopcard;

	private ShopCard _Shopcard;

	public bool HasStoreType;

	private string _StoreType;

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

	public ShopCard Shopcard
	{
		get
		{
			return _Shopcard;
		}
		set
		{
			_Shopcard = value;
			HasShopcard = value != null;
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
		if (HasShopcard)
		{
			hash ^= Shopcard.GetHashCode();
		}
		if (HasStoreType)
		{
			hash ^= StoreType.GetHashCode();
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
		if (!(obj is ShopCardClick other))
		{
			return false;
		}
		if (HasPlayer != other.HasPlayer || (HasPlayer && !Player.Equals(other.Player)))
		{
			return false;
		}
		if (HasShopcard != other.HasShopcard || (HasShopcard && !Shopcard.Equals(other.Shopcard)))
		{
			return false;
		}
		if (HasStoreType != other.HasStoreType || (HasStoreType && !StoreType.Equals(other.StoreType)))
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

	public static ShopCardClick Deserialize(Stream stream, ShopCardClick instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static ShopCardClick DeserializeLengthDelimited(Stream stream)
	{
		ShopCardClick instance = new ShopCardClick();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static ShopCardClick DeserializeLengthDelimited(Stream stream, ShopCardClick instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static ShopCardClick Deserialize(Stream stream, ShopCardClick instance, long limit)
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
				if (instance.Shopcard == null)
				{
					instance.Shopcard = ShopCard.DeserializeLengthDelimited(stream);
				}
				else
				{
					ShopCard.DeserializeLengthDelimited(stream, instance.Shopcard);
				}
				continue;
			case 26:
				instance.StoreType = ProtocolParser.ReadString(stream);
				continue;
			case 34:
				instance.ShopTab = ProtocolParser.ReadString(stream);
				continue;
			case 42:
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

	public static void Serialize(Stream stream, ShopCardClick instance)
	{
		if (instance.HasPlayer)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteUInt32(stream, instance.Player.GetSerializedSize());
			Player.Serialize(stream, instance.Player);
		}
		if (instance.HasShopcard)
		{
			stream.WriteByte(18);
			ProtocolParser.WriteUInt32(stream, instance.Shopcard.GetSerializedSize());
			ShopCard.Serialize(stream, instance.Shopcard);
		}
		if (instance.HasStoreType)
		{
			stream.WriteByte(26);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.StoreType));
		}
		if (instance.HasShopTab)
		{
			stream.WriteByte(34);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.ShopTab));
		}
		if (instance.HasShopSubTab)
		{
			stream.WriteByte(42);
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
		if (HasShopcard)
		{
			size++;
			uint size3 = Shopcard.GetSerializedSize();
			size += size3 + ProtocolParser.SizeOfUInt32(size3);
		}
		if (HasStoreType)
		{
			size++;
			uint byteCount3 = (uint)Encoding.UTF8.GetByteCount(StoreType);
			size += ProtocolParser.SizeOfUInt32(byteCount3) + byteCount3;
		}
		if (HasShopTab)
		{
			size++;
			uint byteCount4 = (uint)Encoding.UTF8.GetByteCount(ShopTab);
			size += ProtocolParser.SizeOfUInt32(byteCount4) + byteCount4;
		}
		if (HasShopSubTab)
		{
			size++;
			uint byteCount5 = (uint)Encoding.UTF8.GetByteCount(ShopSubTab);
			size += ProtocolParser.SizeOfUInt32(byteCount5) + byteCount5;
		}
		return size;
	}
}
