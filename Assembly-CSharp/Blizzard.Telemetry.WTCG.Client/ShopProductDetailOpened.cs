using System.IO;

namespace Blizzard.Telemetry.WTCG.Client;

public class ShopProductDetailOpened : IProtoBuf
{
	public bool HasPlayer;

	private Player _Player;

	public bool HasDeviceInfo;

	private DeviceInfo _DeviceInfo;

	public bool HasProductId;

	private long _ProductId;

	public bool HasLoadingTime;

	private float _LoadingTime;

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

	public float LoadingTime
	{
		get
		{
			return _LoadingTime;
		}
		set
		{
			_LoadingTime = value;
			HasLoadingTime = true;
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
		if (HasProductId)
		{
			hash ^= ProductId.GetHashCode();
		}
		if (HasLoadingTime)
		{
			hash ^= LoadingTime.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is ShopProductDetailOpened other))
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
		if (HasProductId != other.HasProductId || (HasProductId && !ProductId.Equals(other.ProductId)))
		{
			return false;
		}
		if (HasLoadingTime != other.HasLoadingTime || (HasLoadingTime && !LoadingTime.Equals(other.LoadingTime)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static ShopProductDetailOpened Deserialize(Stream stream, ShopProductDetailOpened instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static ShopProductDetailOpened DeserializeLengthDelimited(Stream stream)
	{
		ShopProductDetailOpened instance = new ShopProductDetailOpened();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static ShopProductDetailOpened DeserializeLengthDelimited(Stream stream, ShopProductDetailOpened instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static ShopProductDetailOpened Deserialize(Stream stream, ShopProductDetailOpened instance, long limit)
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
				if (instance.DeviceInfo == null)
				{
					instance.DeviceInfo = DeviceInfo.DeserializeLengthDelimited(stream);
				}
				else
				{
					DeviceInfo.DeserializeLengthDelimited(stream, instance.DeviceInfo);
				}
				continue;
			case 24:
				instance.ProductId = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			case 37:
				instance.LoadingTime = br.ReadSingle();
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

	public static void Serialize(Stream stream, ShopProductDetailOpened instance)
	{
		BinaryWriter bw = new BinaryWriter(stream);
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
		if (instance.HasProductId)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.ProductId);
		}
		if (instance.HasLoadingTime)
		{
			stream.WriteByte(37);
			bw.Write(instance.LoadingTime);
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
		if (HasProductId)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)ProductId);
		}
		if (HasLoadingTime)
		{
			size++;
			size += 4;
		}
		return size;
	}
}
