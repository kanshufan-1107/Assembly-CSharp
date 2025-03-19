using System.IO;
using System.Text;

namespace Blizzard.Telemetry.WTCG.Client;

public class BoxProductBannerDisplayed : IProtoBuf
{
	public bool HasPlayer;

	private Player _Player;

	public bool HasDeviceInfo;

	private DeviceInfo _DeviceInfo;

	public bool HasBannerCampaignName;

	private string _BannerCampaignName;

	public bool HasBannerImageName;

	private string _BannerImageName;

	public bool HasProductId;

	private long _ProductId;

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

	public string BannerCampaignName
	{
		get
		{
			return _BannerCampaignName;
		}
		set
		{
			_BannerCampaignName = value;
			HasBannerCampaignName = value != null;
		}
	}

	public string BannerImageName
	{
		get
		{
			return _BannerImageName;
		}
		set
		{
			_BannerImageName = value;
			HasBannerImageName = value != null;
		}
	}

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

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasPlayer)
		{
			hash ^= Player.GetHashCode();
		}
		if (HasDeviceInfo)
		{
			hash ^= DeviceInfo.GetHashCode();
		}
		if (HasBannerCampaignName)
		{
			hash ^= BannerCampaignName.GetHashCode();
		}
		if (HasBannerImageName)
		{
			hash ^= BannerImageName.GetHashCode();
		}
		if (HasProductId)
		{
			hash ^= ProductId.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is BoxProductBannerDisplayed other))
		{
			return false;
		}
		if (HasPlayer != other.HasPlayer || (HasPlayer && !Player.Equals(other.Player)))
		{
			return false;
		}
		if (HasDeviceInfo != other.HasDeviceInfo || (HasDeviceInfo && !DeviceInfo.Equals(other.DeviceInfo)))
		{
			return false;
		}
		if (HasBannerCampaignName != other.HasBannerCampaignName || (HasBannerCampaignName && !BannerCampaignName.Equals(other.BannerCampaignName)))
		{
			return false;
		}
		if (HasBannerImageName != other.HasBannerImageName || (HasBannerImageName && !BannerImageName.Equals(other.BannerImageName)))
		{
			return false;
		}
		if (HasProductId != other.HasProductId || (HasProductId && !ProductId.Equals(other.ProductId)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static BoxProductBannerDisplayed Deserialize(Stream stream, BoxProductBannerDisplayed instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static BoxProductBannerDisplayed DeserializeLengthDelimited(Stream stream)
	{
		BoxProductBannerDisplayed instance = new BoxProductBannerDisplayed();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static BoxProductBannerDisplayed DeserializeLengthDelimited(Stream stream, BoxProductBannerDisplayed instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static BoxProductBannerDisplayed Deserialize(Stream stream, BoxProductBannerDisplayed instance, long limit)
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
				instance.BannerCampaignName = ProtocolParser.ReadString(stream);
				continue;
			case 34:
				instance.BannerImageName = ProtocolParser.ReadString(stream);
				continue;
			case 40:
				instance.ProductId = (long)ProtocolParser.ReadUInt64(stream);
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

	public static void Serialize(Stream stream, BoxProductBannerDisplayed instance)
	{
		if (instance.HasPlayer)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteUInt32(stream, instance.Player.GetSerializedSize());
			Player.Serialize(stream, instance.Player);
		}
		if (instance.HasDeviceInfo)
		{
			stream.WriteByte(18);
			ProtocolParser.WriteUInt32(stream, instance.DeviceInfo.GetSerializedSize());
			DeviceInfo.Serialize(stream, instance.DeviceInfo);
		}
		if (instance.HasBannerCampaignName)
		{
			stream.WriteByte(26);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.BannerCampaignName));
		}
		if (instance.HasBannerImageName)
		{
			stream.WriteByte(34);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.BannerImageName));
		}
		if (instance.HasProductId)
		{
			stream.WriteByte(40);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.ProductId);
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
		if (HasDeviceInfo)
		{
			size++;
			uint size3 = DeviceInfo.GetSerializedSize();
			size += size3 + ProtocolParser.SizeOfUInt32(size3);
		}
		if (HasBannerCampaignName)
		{
			size++;
			uint byteCount3 = (uint)Encoding.UTF8.GetByteCount(BannerCampaignName);
			size += ProtocolParser.SizeOfUInt32(byteCount3) + byteCount3;
		}
		if (HasBannerImageName)
		{
			size++;
			uint byteCount4 = (uint)Encoding.UTF8.GetByteCount(BannerImageName);
			size += ProtocolParser.SizeOfUInt32(byteCount4) + byteCount4;
		}
		if (HasProductId)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)ProductId);
		}
		return size;
	}
}
