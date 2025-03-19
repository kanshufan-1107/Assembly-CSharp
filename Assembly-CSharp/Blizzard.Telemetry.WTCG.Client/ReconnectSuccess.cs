using System.IO;
using System.Text;

namespace Blizzard.Telemetry.WTCG.Client;

public class ReconnectSuccess : IProtoBuf
{
	public bool HasPlayer;

	private Player _Player;

	public bool HasDeviceInfo;

	private DeviceInfo _DeviceInfo;

	public bool HasDisconnectDuration;

	private float _DisconnectDuration;

	public bool HasReconnectDuration;

	private float _ReconnectDuration;

	public bool HasReconnectType;

	private string _ReconnectType;

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

	public float DisconnectDuration
	{
		get
		{
			return _DisconnectDuration;
		}
		set
		{
			_DisconnectDuration = value;
			HasDisconnectDuration = true;
		}
	}

	public float ReconnectDuration
	{
		get
		{
			return _ReconnectDuration;
		}
		set
		{
			_ReconnectDuration = value;
			HasReconnectDuration = true;
		}
	}

	public string ReconnectType
	{
		get
		{
			return _ReconnectType;
		}
		set
		{
			_ReconnectType = value;
			HasReconnectType = value != null;
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
		if (HasDisconnectDuration)
		{
			hash ^= DisconnectDuration.GetHashCode();
		}
		if (HasReconnectDuration)
		{
			hash ^= ReconnectDuration.GetHashCode();
		}
		if (HasReconnectType)
		{
			hash ^= ReconnectType.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is ReconnectSuccess other))
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
		if (HasDisconnectDuration != other.HasDisconnectDuration || (HasDisconnectDuration && !DisconnectDuration.Equals(other.DisconnectDuration)))
		{
			return false;
		}
		if (HasReconnectDuration != other.HasReconnectDuration || (HasReconnectDuration && !ReconnectDuration.Equals(other.ReconnectDuration)))
		{
			return false;
		}
		if (HasReconnectType != other.HasReconnectType || (HasReconnectType && !ReconnectType.Equals(other.ReconnectType)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static ReconnectSuccess Deserialize(Stream stream, ReconnectSuccess instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static ReconnectSuccess DeserializeLengthDelimited(Stream stream)
	{
		ReconnectSuccess instance = new ReconnectSuccess();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static ReconnectSuccess DeserializeLengthDelimited(Stream stream, ReconnectSuccess instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static ReconnectSuccess Deserialize(Stream stream, ReconnectSuccess instance, long limit)
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
			case 29:
				instance.DisconnectDuration = br.ReadSingle();
				continue;
			case 37:
				instance.ReconnectDuration = br.ReadSingle();
				continue;
			case 42:
				instance.ReconnectType = ProtocolParser.ReadString(stream);
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

	public static void Serialize(Stream stream, ReconnectSuccess instance)
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
		if (instance.HasDisconnectDuration)
		{
			stream.WriteByte(29);
			bw.Write(instance.DisconnectDuration);
		}
		if (instance.HasReconnectDuration)
		{
			stream.WriteByte(37);
			bw.Write(instance.ReconnectDuration);
		}
		if (instance.HasReconnectType)
		{
			stream.WriteByte(42);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.ReconnectType));
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
		if (HasDisconnectDuration)
		{
			size++;
			size += 4;
		}
		if (HasReconnectDuration)
		{
			size++;
			size += 4;
		}
		if (HasReconnectType)
		{
			size++;
			uint byteCount5 = (uint)Encoding.UTF8.GetByteCount(ReconnectType);
			size += ProtocolParser.SizeOfUInt32(byteCount5) + byteCount5;
		}
		return size;
	}
}
