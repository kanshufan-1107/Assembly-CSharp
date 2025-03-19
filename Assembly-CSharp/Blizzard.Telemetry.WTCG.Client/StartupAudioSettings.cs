using System.IO;

namespace Blizzard.Telemetry.WTCG.Client;

public class StartupAudioSettings : IProtoBuf
{
	public bool HasDeviceInfo;

	private DeviceInfo _DeviceInfo;

	public bool HasDeviceMuted;

	private bool _DeviceMuted;

	public bool HasDeviceVolume;

	private float _DeviceVolume;

	public bool HasMasterVolume;

	private float _MasterVolume;

	public bool HasMusicVolume;

	private float _MusicVolume;

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

	public bool DeviceMuted
	{
		get
		{
			return _DeviceMuted;
		}
		set
		{
			_DeviceMuted = value;
			HasDeviceMuted = true;
		}
	}

	public float DeviceVolume
	{
		get
		{
			return _DeviceVolume;
		}
		set
		{
			_DeviceVolume = value;
			HasDeviceVolume = true;
		}
	}

	public float MasterVolume
	{
		get
		{
			return _MasterVolume;
		}
		set
		{
			_MasterVolume = value;
			HasMasterVolume = true;
		}
	}

	public float MusicVolume
	{
		get
		{
			return _MusicVolume;
		}
		set
		{
			_MusicVolume = value;
			HasMusicVolume = true;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasDeviceInfo)
		{
			hash ^= DeviceInfo.GetHashCode();
		}
		if (HasDeviceMuted)
		{
			hash ^= DeviceMuted.GetHashCode();
		}
		if (HasDeviceVolume)
		{
			hash ^= DeviceVolume.GetHashCode();
		}
		if (HasMasterVolume)
		{
			hash ^= MasterVolume.GetHashCode();
		}
		if (HasMusicVolume)
		{
			hash ^= MusicVolume.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is StartupAudioSettings other))
		{
			return false;
		}
		if (HasDeviceInfo != other.HasDeviceInfo || (HasDeviceInfo && !DeviceInfo.Equals(other.DeviceInfo)))
		{
			return false;
		}
		if (HasDeviceMuted != other.HasDeviceMuted || (HasDeviceMuted && !DeviceMuted.Equals(other.DeviceMuted)))
		{
			return false;
		}
		if (HasDeviceVolume != other.HasDeviceVolume || (HasDeviceVolume && !DeviceVolume.Equals(other.DeviceVolume)))
		{
			return false;
		}
		if (HasMasterVolume != other.HasMasterVolume || (HasMasterVolume && !MasterVolume.Equals(other.MasterVolume)))
		{
			return false;
		}
		if (HasMusicVolume != other.HasMusicVolume || (HasMusicVolume && !MusicVolume.Equals(other.MusicVolume)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static StartupAudioSettings Deserialize(Stream stream, StartupAudioSettings instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static StartupAudioSettings DeserializeLengthDelimited(Stream stream)
	{
		StartupAudioSettings instance = new StartupAudioSettings();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static StartupAudioSettings DeserializeLengthDelimited(Stream stream, StartupAudioSettings instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static StartupAudioSettings Deserialize(Stream stream, StartupAudioSettings instance, long limit)
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
			case 16:
				instance.DeviceMuted = ProtocolParser.ReadBool(stream);
				continue;
			case 29:
				instance.DeviceVolume = br.ReadSingle();
				continue;
			case 37:
				instance.MasterVolume = br.ReadSingle();
				continue;
			case 45:
				instance.MusicVolume = br.ReadSingle();
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

	public static void Serialize(Stream stream, StartupAudioSettings instance)
	{
		BinaryWriter bw = new BinaryWriter(stream);
		if (instance.HasDeviceInfo)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteUInt32(stream, instance.DeviceInfo.GetSerializedSize());
			DeviceInfo.Serialize(stream, instance.DeviceInfo);
		}
		if (instance.HasDeviceMuted)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteBool(stream, instance.DeviceMuted);
		}
		if (instance.HasDeviceVolume)
		{
			stream.WriteByte(29);
			bw.Write(instance.DeviceVolume);
		}
		if (instance.HasMasterVolume)
		{
			stream.WriteByte(37);
			bw.Write(instance.MasterVolume);
		}
		if (instance.HasMusicVolume)
		{
			stream.WriteByte(45);
			bw.Write(instance.MusicVolume);
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
		if (HasDeviceMuted)
		{
			size++;
			size++;
		}
		if (HasDeviceVolume)
		{
			size++;
			size += 4;
		}
		if (HasMasterVolume)
		{
			size++;
			size += 4;
		}
		if (HasMusicVolume)
		{
			size++;
			size += 4;
		}
		return size;
	}
}
