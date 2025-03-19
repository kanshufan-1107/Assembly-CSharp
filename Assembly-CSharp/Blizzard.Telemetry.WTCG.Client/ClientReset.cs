using System.IO;

namespace Blizzard.Telemetry.WTCG.Client;

public class ClientReset : IProtoBuf
{
	public bool HasDeviceInfo;

	private DeviceInfo _DeviceInfo;

	public bool HasForceLogin;

	private bool _ForceLogin;

	public bool HasForceNoAccountTutorial;

	private bool _ForceNoAccountTutorial;

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

	public bool ForceLogin
	{
		get
		{
			return _ForceLogin;
		}
		set
		{
			_ForceLogin = value;
			HasForceLogin = true;
		}
	}

	public bool ForceNoAccountTutorial
	{
		get
		{
			return _ForceNoAccountTutorial;
		}
		set
		{
			_ForceNoAccountTutorial = value;
			HasForceNoAccountTutorial = true;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasDeviceInfo)
		{
			hash ^= DeviceInfo.GetHashCode();
		}
		if (HasForceLogin)
		{
			hash ^= ForceLogin.GetHashCode();
		}
		if (HasForceNoAccountTutorial)
		{
			hash ^= ForceNoAccountTutorial.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is ClientReset other))
		{
			return false;
		}
		if (HasDeviceInfo != other.HasDeviceInfo || (HasDeviceInfo && !DeviceInfo.Equals(other.DeviceInfo)))
		{
			return false;
		}
		if (HasForceLogin != other.HasForceLogin || (HasForceLogin && !ForceLogin.Equals(other.ForceLogin)))
		{
			return false;
		}
		if (HasForceNoAccountTutorial != other.HasForceNoAccountTutorial || (HasForceNoAccountTutorial && !ForceNoAccountTutorial.Equals(other.ForceNoAccountTutorial)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static ClientReset Deserialize(Stream stream, ClientReset instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static ClientReset DeserializeLengthDelimited(Stream stream)
	{
		ClientReset instance = new ClientReset();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static ClientReset DeserializeLengthDelimited(Stream stream, ClientReset instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static ClientReset Deserialize(Stream stream, ClientReset instance, long limit)
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
				instance.ForceLogin = ProtocolParser.ReadBool(stream);
				continue;
			case 24:
				instance.ForceNoAccountTutorial = ProtocolParser.ReadBool(stream);
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

	public static void Serialize(Stream stream, ClientReset instance)
	{
		if (instance.HasDeviceInfo)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteUInt32(stream, instance.DeviceInfo.GetSerializedSize());
			DeviceInfo.Serialize(stream, instance.DeviceInfo);
		}
		if (instance.HasForceLogin)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteBool(stream, instance.ForceLogin);
		}
		if (instance.HasForceNoAccountTutorial)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteBool(stream, instance.ForceNoAccountTutorial);
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
		if (HasForceLogin)
		{
			size++;
			size++;
		}
		if (HasForceNoAccountTutorial)
		{
			size++;
			size++;
		}
		return size;
	}
}
