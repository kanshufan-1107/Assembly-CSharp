using System.IO;

namespace Blizzard.Telemetry.WTCG.Client;

public class LoadProducts : IProtoBuf
{
	public bool HasDeviceInfo;

	private DeviceInfo _DeviceInfo;

	public bool HasPlayer;

	private Player _Player;

	public bool HasTimeToLoadProducts;

	private float _TimeToLoadProducts;

	public bool HasTimeToDeserialize;

	private float _TimeToDeserialize;

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

	public float TimeToLoadProducts
	{
		get
		{
			return _TimeToLoadProducts;
		}
		set
		{
			_TimeToLoadProducts = value;
			HasTimeToLoadProducts = true;
		}
	}

	public float TimeToDeserialize
	{
		get
		{
			return _TimeToDeserialize;
		}
		set
		{
			_TimeToDeserialize = value;
			HasTimeToDeserialize = true;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasDeviceInfo)
		{
			hash ^= DeviceInfo.GetHashCode();
		}
		if (HasPlayer)
		{
			hash ^= Player.GetHashCode();
		}
		if (HasTimeToLoadProducts)
		{
			hash ^= TimeToLoadProducts.GetHashCode();
		}
		if (HasTimeToDeserialize)
		{
			hash ^= TimeToDeserialize.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is LoadProducts other))
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
		if (HasTimeToLoadProducts != other.HasTimeToLoadProducts || (HasTimeToLoadProducts && !TimeToLoadProducts.Equals(other.TimeToLoadProducts)))
		{
			return false;
		}
		if (HasTimeToDeserialize != other.HasTimeToDeserialize || (HasTimeToDeserialize && !TimeToDeserialize.Equals(other.TimeToDeserialize)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static LoadProducts Deserialize(Stream stream, LoadProducts instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static LoadProducts DeserializeLengthDelimited(Stream stream)
	{
		LoadProducts instance = new LoadProducts();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static LoadProducts DeserializeLengthDelimited(Stream stream, LoadProducts instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static LoadProducts Deserialize(Stream stream, LoadProducts instance, long limit)
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
				if (instance.DeviceInfo == null)
				{
					instance.DeviceInfo = DeviceInfo.DeserializeLengthDelimited(stream);
				}
				else
				{
					DeviceInfo.DeserializeLengthDelimited(stream, instance.DeviceInfo);
				}
				continue;
			case 18:
				if (instance.Player == null)
				{
					instance.Player = Player.DeserializeLengthDelimited(stream);
				}
				else
				{
					Player.DeserializeLengthDelimited(stream, instance.Player);
				}
				continue;
			case 29:
				instance.TimeToLoadProducts = br.ReadSingle();
				continue;
			case 37:
				instance.TimeToDeserialize = br.ReadSingle();
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

	public static void Serialize(Stream stream, LoadProducts instance)
	{
		BinaryWriter bw = new BinaryWriter(stream);
		if (instance.HasDeviceInfo)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteUInt32(stream, instance.DeviceInfo.GetSerializedSize());
			DeviceInfo.Serialize(stream, instance.DeviceInfo);
		}
		if (instance.HasPlayer)
		{
			stream.WriteByte(18);
			ProtocolParser.WriteUInt32(stream, instance.Player.GetSerializedSize());
			Player.Serialize(stream, instance.Player);
		}
		if (instance.HasTimeToLoadProducts)
		{
			stream.WriteByte(29);
			bw.Write(instance.TimeToLoadProducts);
		}
		if (instance.HasTimeToDeserialize)
		{
			stream.WriteByte(37);
			bw.Write(instance.TimeToDeserialize);
		}
	}

	public uint GetSerializedSize()
	{
		uint size = 0u;
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
		if (HasTimeToLoadProducts)
		{
			size++;
			size += 4;
		}
		if (HasTimeToDeserialize)
		{
			size++;
			size += 4;
		}
		return size;
	}
}
