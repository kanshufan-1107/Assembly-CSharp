using System.IO;
using System.Text;

namespace Blizzard.Telemetry.WTCG.Client;

public class IgnorableBattleNetError : IProtoBuf
{
	public bool HasDeviceInfo;

	private DeviceInfo _DeviceInfo;

	public bool HasErrorCode;

	private int _ErrorCode;

	public bool HasDescription;

	private string _Description;

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

	public int ErrorCode
	{
		get
		{
			return _ErrorCode;
		}
		set
		{
			_ErrorCode = value;
			HasErrorCode = true;
		}
	}

	public string Description
	{
		get
		{
			return _Description;
		}
		set
		{
			_Description = value;
			HasDescription = value != null;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasDeviceInfo)
		{
			hash ^= DeviceInfo.GetHashCode();
		}
		if (HasErrorCode)
		{
			hash ^= ErrorCode.GetHashCode();
		}
		if (HasDescription)
		{
			hash ^= Description.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is IgnorableBattleNetError other))
		{
			return false;
		}
		if (HasDeviceInfo != other.HasDeviceInfo || (HasDeviceInfo && !DeviceInfo.Equals(other.DeviceInfo)))
		{
			return false;
		}
		if (HasErrorCode != other.HasErrorCode || (HasErrorCode && !ErrorCode.Equals(other.ErrorCode)))
		{
			return false;
		}
		if (HasDescription != other.HasDescription || (HasDescription && !Description.Equals(other.Description)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static IgnorableBattleNetError Deserialize(Stream stream, IgnorableBattleNetError instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static IgnorableBattleNetError DeserializeLengthDelimited(Stream stream)
	{
		IgnorableBattleNetError instance = new IgnorableBattleNetError();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static IgnorableBattleNetError DeserializeLengthDelimited(Stream stream, IgnorableBattleNetError instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static IgnorableBattleNetError Deserialize(Stream stream, IgnorableBattleNetError instance, long limit)
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
				instance.ErrorCode = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 26:
				instance.Description = ProtocolParser.ReadString(stream);
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

	public static void Serialize(Stream stream, IgnorableBattleNetError instance)
	{
		if (instance.HasDeviceInfo)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteUInt32(stream, instance.DeviceInfo.GetSerializedSize());
			DeviceInfo.Serialize(stream, instance.DeviceInfo);
		}
		if (instance.HasErrorCode)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.ErrorCode);
		}
		if (instance.HasDescription)
		{
			stream.WriteByte(26);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.Description));
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
		if (HasErrorCode)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)ErrorCode);
		}
		if (HasDescription)
		{
			size++;
			uint byteCount3 = (uint)Encoding.UTF8.GetByteCount(Description);
			size += ProtocolParser.SizeOfUInt32(byteCount3) + byteCount3;
		}
		return size;
	}
}
