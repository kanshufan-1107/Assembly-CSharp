using System.IO;
using System.Text;

namespace Blizzard.Telemetry.WTCG.Client;

public class FlowPerformanceShop : IProtoBuf
{
	public enum ShopType
	{
		GENERAL_STORE = 1,
		ARENA_STORE,
		ADVENTURE_STORE,
		TAVERN_BRAWL_STORE,
		ADVENTURE_STORE_WING_PURCHASE_WIDGET,
		ADVENTURE_STORE_FULL_PURCHASE_WIDGET,
		DUELS_STORE
	}

	public bool HasFlowId;

	private string _FlowId;

	public bool HasDeviceInfo;

	private DeviceInfo _DeviceInfo;

	public bool HasPlayer;

	private Player _Player;

	public bool HasShopType_;

	private ShopType _ShopType_;

	public string FlowId
	{
		get
		{
			return _FlowId;
		}
		set
		{
			_FlowId = value;
			HasFlowId = value != null;
		}
	}

	public DeviceInfo DeviceInfo
	{
		get
		{
			return _DeviceInfo;
		}
		set
		{
			_DeviceInfo = value;
			HasDeviceInfo = value != null;
		}
	}

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

	public ShopType ShopType_
	{
		get
		{
			return _ShopType_;
		}
		set
		{
			_ShopType_ = value;
			HasShopType_ = true;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasFlowId)
		{
			hash ^= FlowId.GetHashCode();
		}
		if (HasDeviceInfo)
		{
			hash ^= DeviceInfo.GetHashCode();
		}
		if (HasPlayer)
		{
			hash ^= Player.GetHashCode();
		}
		if (HasShopType_)
		{
			hash ^= ShopType_.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is FlowPerformanceShop other))
		{
			return false;
		}
		if (HasFlowId != other.HasFlowId || (HasFlowId && !FlowId.Equals(other.FlowId)))
		{
			return false;
		}
		if (HasDeviceInfo != other.HasDeviceInfo || (HasDeviceInfo && !DeviceInfo.Equals(other.DeviceInfo)))
		{
			return false;
		}
		if (HasPlayer != other.HasPlayer || (HasPlayer && !Player.Equals(other.Player)))
		{
			return false;
		}
		if (HasShopType_ != other.HasShopType_ || (HasShopType_ && !ShopType_.Equals(other.ShopType_)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static FlowPerformanceShop Deserialize(Stream stream, FlowPerformanceShop instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static FlowPerformanceShop DeserializeLengthDelimited(Stream stream)
	{
		FlowPerformanceShop instance = new FlowPerformanceShop();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static FlowPerformanceShop DeserializeLengthDelimited(Stream stream, FlowPerformanceShop instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static FlowPerformanceShop Deserialize(Stream stream, FlowPerformanceShop instance, long limit)
	{
		instance.ShopType_ = ShopType.GENERAL_STORE;
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
				instance.FlowId = ProtocolParser.ReadString(stream);
				continue;
			case 18:
				if (instance.DeviceInfo == null)
				{
					instance.DeviceInfo = DeviceInfo.DeserializeLengthDelimited(stream);
				}
				else
				{
					DeviceInfo.DeserializeLengthDelimited(stream, instance.DeviceInfo);
				}
				continue;
			case 26:
				if (instance.Player == null)
				{
					instance.Player = Player.DeserializeLengthDelimited(stream);
				}
				else
				{
					Player.DeserializeLengthDelimited(stream, instance.Player);
				}
				continue;
			case 32:
				instance.ShopType_ = (ShopType)ProtocolParser.ReadUInt64(stream);
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

	public static void Serialize(Stream stream, FlowPerformanceShop instance)
	{
		if (instance.HasFlowId)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.FlowId));
		}
		if (instance.HasDeviceInfo)
		{
			stream.WriteByte(18);
			ProtocolParser.WriteUInt32(stream, instance.DeviceInfo.GetSerializedSize());
			DeviceInfo.Serialize(stream, instance.DeviceInfo);
		}
		if (instance.HasPlayer)
		{
			stream.WriteByte(26);
			ProtocolParser.WriteUInt32(stream, instance.Player.GetSerializedSize());
			Player.Serialize(stream, instance.Player);
		}
		if (instance.HasShopType_)
		{
			stream.WriteByte(32);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.ShopType_);
		}
	}

	public uint GetSerializedSize()
	{
		uint size = 0u;
		if (HasFlowId)
		{
			size++;
			uint byteCount1 = (uint)Encoding.UTF8.GetByteCount(FlowId);
			size += ProtocolParser.SizeOfUInt32(byteCount1) + byteCount1;
		}
		if (HasDeviceInfo)
		{
			size++;
			uint size2 = DeviceInfo.GetSerializedSize();
			size += size2 + ProtocolParser.SizeOfUInt32(size2);
		}
		if (HasPlayer)
		{
			size++;
			uint size3 = Player.GetSerializedSize();
			size += size3 + ProtocolParser.SizeOfUInt32(size3);
		}
		if (HasShopType_)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)ShopType_);
		}
		return size;
	}
}
