using System.IO;

namespace Blizzard.Telemetry.WTCG.Client;

public class BlizzardCheckoutIsReady : IProtoBuf
{
	public bool HasPlayer;

	private Player _Player;

	public bool HasDeviceInfo;

	private DeviceInfo _DeviceInfo;

	public bool HasSecondsShown;

	private double _SecondsShown;

	public bool HasIsReady;

	private bool _IsReady;

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

	public double SecondsShown
	{
		get
		{
			return _SecondsShown;
		}
		set
		{
			_SecondsShown = value;
			HasSecondsShown = true;
		}
	}

	public bool IsReady
	{
		get
		{
			return _IsReady;
		}
		set
		{
			_IsReady = value;
			HasIsReady = true;
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
		if (HasSecondsShown)
		{
			hash ^= SecondsShown.GetHashCode();
		}
		if (HasIsReady)
		{
			hash ^= IsReady.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is BlizzardCheckoutIsReady other))
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
		if (HasSecondsShown != other.HasSecondsShown || (HasSecondsShown && !SecondsShown.Equals(other.SecondsShown)))
		{
			return false;
		}
		if (HasIsReady != other.HasIsReady || (HasIsReady && !IsReady.Equals(other.IsReady)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static BlizzardCheckoutIsReady Deserialize(Stream stream, BlizzardCheckoutIsReady instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static BlizzardCheckoutIsReady DeserializeLengthDelimited(Stream stream)
	{
		BlizzardCheckoutIsReady instance = new BlizzardCheckoutIsReady();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static BlizzardCheckoutIsReady DeserializeLengthDelimited(Stream stream, BlizzardCheckoutIsReady instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static BlizzardCheckoutIsReady Deserialize(Stream stream, BlizzardCheckoutIsReady instance, long limit)
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
			case 25:
				instance.SecondsShown = br.ReadDouble();
				continue;
			case 32:
				instance.IsReady = ProtocolParser.ReadBool(stream);
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

	public static void Serialize(Stream stream, BlizzardCheckoutIsReady instance)
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
		if (instance.HasSecondsShown)
		{
			stream.WriteByte(25);
			bw.Write(instance.SecondsShown);
		}
		if (instance.HasIsReady)
		{
			stream.WriteByte(32);
			ProtocolParser.WriteBool(stream, instance.IsReady);
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
		if (HasSecondsShown)
		{
			size++;
			size += 8;
		}
		if (HasIsReady)
		{
			size++;
			size++;
		}
		return size;
	}
}
