using System.IO;
using System.Text;

namespace Blizzard.Telemetry.WTCG.Client;

public class BlizzardCheckoutInitializationResult : IProtoBuf
{
	public bool HasPlayer;

	private Player _Player;

	public bool HasDeviceInfo;

	private DeviceInfo _DeviceInfo;

	public bool HasSuccess;

	private bool _Success;

	public bool HasFailureReason;

	private string _FailureReason;

	public bool HasFailureDetails;

	private string _FailureDetails;

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

	public bool Success
	{
		get
		{
			return _Success;
		}
		set
		{
			_Success = value;
			HasSuccess = true;
		}
	}

	public string FailureReason
	{
		get
		{
			return _FailureReason;
		}
		set
		{
			_FailureReason = value;
			HasFailureReason = value != null;
		}
	}

	public string FailureDetails
	{
		get
		{
			return _FailureDetails;
		}
		set
		{
			_FailureDetails = value;
			HasFailureDetails = value != null;
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
		if (HasSuccess)
		{
			hash ^= Success.GetHashCode();
		}
		if (HasFailureReason)
		{
			hash ^= FailureReason.GetHashCode();
		}
		if (HasFailureDetails)
		{
			hash ^= FailureDetails.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is BlizzardCheckoutInitializationResult other))
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
		if (HasSuccess != other.HasSuccess || (HasSuccess && !Success.Equals(other.Success)))
		{
			return false;
		}
		if (HasFailureReason != other.HasFailureReason || (HasFailureReason && !FailureReason.Equals(other.FailureReason)))
		{
			return false;
		}
		if (HasFailureDetails != other.HasFailureDetails || (HasFailureDetails && !FailureDetails.Equals(other.FailureDetails)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static BlizzardCheckoutInitializationResult Deserialize(Stream stream, BlizzardCheckoutInitializationResult instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static BlizzardCheckoutInitializationResult DeserializeLengthDelimited(Stream stream)
	{
		BlizzardCheckoutInitializationResult instance = new BlizzardCheckoutInitializationResult();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static BlizzardCheckoutInitializationResult DeserializeLengthDelimited(Stream stream, BlizzardCheckoutInitializationResult instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static BlizzardCheckoutInitializationResult Deserialize(Stream stream, BlizzardCheckoutInitializationResult instance, long limit)
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
			case 24:
				instance.Success = ProtocolParser.ReadBool(stream);
				continue;
			case 34:
				instance.FailureReason = ProtocolParser.ReadString(stream);
				continue;
			case 42:
				instance.FailureDetails = ProtocolParser.ReadString(stream);
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

	public static void Serialize(Stream stream, BlizzardCheckoutInitializationResult instance)
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
		if (instance.HasSuccess)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteBool(stream, instance.Success);
		}
		if (instance.HasFailureReason)
		{
			stream.WriteByte(34);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.FailureReason));
		}
		if (instance.HasFailureDetails)
		{
			stream.WriteByte(42);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.FailureDetails));
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
		if (HasSuccess)
		{
			size++;
			size++;
		}
		if (HasFailureReason)
		{
			size++;
			uint byteCount4 = (uint)Encoding.UTF8.GetByteCount(FailureReason);
			size += ProtocolParser.SizeOfUInt32(byteCount4) + byteCount4;
		}
		if (HasFailureDetails)
		{
			size++;
			uint byteCount5 = (uint)Encoding.UTF8.GetByteCount(FailureDetails);
			size += ProtocolParser.SizeOfUInt32(byteCount5) + byteCount5;
		}
		return size;
	}
}
