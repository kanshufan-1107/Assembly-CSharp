using System.IO;

namespace Blizzard.Telemetry.WTCG.Client;

public class MusicVolumeChanged : IProtoBuf
{
	public bool HasPlayer;

	private Player _Player;

	public bool HasDeviceInfo;

	private DeviceInfo _DeviceInfo;

	public bool HasOldVolume;

	private float _OldVolume;

	public bool HasNewVolume;

	private float _NewVolume;

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

	public float OldVolume
	{
		get
		{
			return _OldVolume;
		}
		set
		{
			_OldVolume = value;
			HasOldVolume = true;
		}
	}

	public float NewVolume
	{
		get
		{
			return _NewVolume;
		}
		set
		{
			_NewVolume = value;
			HasNewVolume = true;
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
		if (HasOldVolume)
		{
			hash ^= OldVolume.GetHashCode();
		}
		if (HasNewVolume)
		{
			hash ^= NewVolume.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is MusicVolumeChanged other))
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
		if (HasOldVolume != other.HasOldVolume || (HasOldVolume && !OldVolume.Equals(other.OldVolume)))
		{
			return false;
		}
		if (HasNewVolume != other.HasNewVolume || (HasNewVolume && !NewVolume.Equals(other.NewVolume)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static MusicVolumeChanged Deserialize(Stream stream, MusicVolumeChanged instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static MusicVolumeChanged DeserializeLengthDelimited(Stream stream)
	{
		MusicVolumeChanged instance = new MusicVolumeChanged();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static MusicVolumeChanged DeserializeLengthDelimited(Stream stream, MusicVolumeChanged instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static MusicVolumeChanged Deserialize(Stream stream, MusicVolumeChanged instance, long limit)
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
			case 37:
				instance.OldVolume = br.ReadSingle();
				continue;
			case 45:
				instance.NewVolume = br.ReadSingle();
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

	public static void Serialize(Stream stream, MusicVolumeChanged instance)
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
		if (instance.HasOldVolume)
		{
			stream.WriteByte(37);
			bw.Write(instance.OldVolume);
		}
		if (instance.HasNewVolume)
		{
			stream.WriteByte(45);
			bw.Write(instance.NewVolume);
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
		if (HasOldVolume)
		{
			size++;
			size += 4;
		}
		if (HasNewVolume)
		{
			size++;
			size += 4;
		}
		return size;
	}
}
