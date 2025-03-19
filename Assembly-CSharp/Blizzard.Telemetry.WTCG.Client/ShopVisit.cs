using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Blizzard.Telemetry.WTCG.Client;

public class ShopVisit : IProtoBuf
{
	public bool HasPlayer;

	private Player _Player;

	private List<ShopCard> _Cards = new List<ShopCard>();

	public bool HasStoreType;

	private string _StoreType;

	public bool HasShopTab;

	private string _ShopTab;

	public bool HasShopSubTab;

	private string _ShopSubTab;

	public bool HasLoadTimeSeconds;

	private float _LoadTimeSeconds;

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

	public List<ShopCard> Cards
	{
		get
		{
			return _Cards;
		}
		set
		{
			_Cards = value;
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

	public float LoadTimeSeconds
	{
		get
		{
			return _LoadTimeSeconds;
		}
		set
		{
			_LoadTimeSeconds = value;
			HasLoadTimeSeconds = true;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasPlayer)
		{
			hash ^= Player.GetHashCode();
		}
		foreach (ShopCard i in Cards)
		{
			hash ^= i.GetHashCode();
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
		if (HasLoadTimeSeconds)
		{
			hash ^= LoadTimeSeconds.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is ShopVisit other))
		{
			return false;
		}
		if (HasPlayer != other.HasPlayer || (HasPlayer && !Player.Equals(other.Player)))
		{
			return false;
		}
		if (Cards.Count != other.Cards.Count)
		{
			return false;
		}
		for (int i = 0; i < Cards.Count; i++)
		{
			if (!Cards[i].Equals(other.Cards[i]))
			{
				return false;
			}
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
		if (HasLoadTimeSeconds != other.HasLoadTimeSeconds || (HasLoadTimeSeconds && !LoadTimeSeconds.Equals(other.LoadTimeSeconds)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static ShopVisit Deserialize(Stream stream, ShopVisit instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static ShopVisit DeserializeLengthDelimited(Stream stream)
	{
		ShopVisit instance = new ShopVisit();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static ShopVisit DeserializeLengthDelimited(Stream stream, ShopVisit instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static ShopVisit Deserialize(Stream stream, ShopVisit instance, long limit)
	{
		BinaryReader br = new BinaryReader(stream);
		if (instance.Cards == null)
		{
			instance.Cards = new List<ShopCard>();
		}
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
				instance.Cards.Add(ShopCard.DeserializeLengthDelimited(stream));
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
			case 53:
				instance.LoadTimeSeconds = br.ReadSingle();
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

	public static void Serialize(Stream stream, ShopVisit instance)
	{
		BinaryWriter bw = new BinaryWriter(stream);
		if (instance.HasPlayer)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteUInt32(stream, instance.Player.GetSerializedSize());
			Player.Serialize(stream, instance.Player);
		}
		if (instance.Cards.Count > 0)
		{
			foreach (ShopCard i2 in instance.Cards)
			{
				stream.WriteByte(18);
				ProtocolParser.WriteUInt32(stream, i2.GetSerializedSize());
				ShopCard.Serialize(stream, i2);
			}
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
		if (instance.HasLoadTimeSeconds)
		{
			stream.WriteByte(53);
			bw.Write(instance.LoadTimeSeconds);
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
		if (Cards.Count > 0)
		{
			foreach (ShopCard card in Cards)
			{
				size++;
				uint size3 = card.GetSerializedSize();
				size += size3 + ProtocolParser.SizeOfUInt32(size3);
			}
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
		if (HasLoadTimeSeconds)
		{
			size++;
			size += 4;
		}
		return size;
	}
}
