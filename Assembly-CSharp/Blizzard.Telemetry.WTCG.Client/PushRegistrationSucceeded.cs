using System.IO;
using System.Text;

namespace Blizzard.Telemetry.WTCG.Client;

public class PushRegistrationSucceeded : IProtoBuf
{
	public bool HasDeviceToken;

	private string _DeviceToken;

	public string DeviceToken
	{
		get
		{
			return _DeviceToken;
		}
		set
		{
			_DeviceToken = value;
			HasDeviceToken = value != null;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasDeviceToken)
		{
			hash ^= DeviceToken.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is PushRegistrationSucceeded other))
		{
			return false;
		}
		if (HasDeviceToken != other.HasDeviceToken || (HasDeviceToken && !DeviceToken.Equals(other.DeviceToken)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static PushRegistrationSucceeded Deserialize(Stream stream, PushRegistrationSucceeded instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static PushRegistrationSucceeded DeserializeLengthDelimited(Stream stream)
	{
		PushRegistrationSucceeded instance = new PushRegistrationSucceeded();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static PushRegistrationSucceeded DeserializeLengthDelimited(Stream stream, PushRegistrationSucceeded instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static PushRegistrationSucceeded Deserialize(Stream stream, PushRegistrationSucceeded instance, long limit)
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
			case 82:
				instance.DeviceToken = ProtocolParser.ReadString(stream);
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

	public static void Serialize(Stream stream, PushRegistrationSucceeded instance)
	{
		if (instance.HasDeviceToken)
		{
			stream.WriteByte(82);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.DeviceToken));
		}
	}

	public uint GetSerializedSize()
	{
		uint size = 0u;
		if (HasDeviceToken)
		{
			size++;
			uint byteCount10 = (uint)Encoding.UTF8.GetByteCount(DeviceToken);
			size += ProtocolParser.SizeOfUInt32(byteCount10) + byteCount10;
		}
		return size;
	}
}
