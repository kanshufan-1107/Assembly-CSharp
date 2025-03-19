using System.IO;

namespace Blizzard.Telemetry.WTCG.Client;

public class MASDKImportResult : IProtoBuf
{
	public enum ImportResult
	{
		SUCCESS,
		FAILURE
	}

	public enum ImportType
	{
		GUEST_ACCOUNT_ID,
		AUTH_TOKEN
	}

	public bool HasPlayer;

	private Player _Player;

	public bool HasDeviceInfo;

	private DeviceInfo _DeviceInfo;

	public bool HasResult;

	private ImportResult _Result;

	public bool HasImportType_;

	private ImportType _ImportType_;

	public bool HasErrorCode;

	private int _ErrorCode;

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

	public ImportResult Result
	{
		get
		{
			return _Result;
		}
		set
		{
			_Result = value;
			HasResult = true;
		}
	}

	public ImportType ImportType_
	{
		get
		{
			return _ImportType_;
		}
		set
		{
			_ImportType_ = value;
			HasImportType_ = true;
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
		if (HasResult)
		{
			hash ^= Result.GetHashCode();
		}
		if (HasImportType_)
		{
			hash ^= ImportType_.GetHashCode();
		}
		if (HasErrorCode)
		{
			hash ^= ErrorCode.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is MASDKImportResult other))
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
		if (HasResult != other.HasResult || (HasResult && !Result.Equals(other.Result)))
		{
			return false;
		}
		if (HasImportType_ != other.HasImportType_ || (HasImportType_ && !ImportType_.Equals(other.ImportType_)))
		{
			return false;
		}
		if (HasErrorCode != other.HasErrorCode || (HasErrorCode && !ErrorCode.Equals(other.ErrorCode)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static MASDKImportResult Deserialize(Stream stream, MASDKImportResult instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static MASDKImportResult DeserializeLengthDelimited(Stream stream)
	{
		MASDKImportResult instance = new MASDKImportResult();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static MASDKImportResult DeserializeLengthDelimited(Stream stream, MASDKImportResult instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static MASDKImportResult Deserialize(Stream stream, MASDKImportResult instance, long limit)
	{
		instance.Result = ImportResult.SUCCESS;
		instance.ImportType_ = ImportType.GUEST_ACCOUNT_ID;
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
				instance.Result = (ImportResult)ProtocolParser.ReadUInt64(stream);
				continue;
			case 32:
				instance.ImportType_ = (ImportType)ProtocolParser.ReadUInt64(stream);
				continue;
			case 40:
				instance.ErrorCode = (int)ProtocolParser.ReadUInt64(stream);
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

	public static void Serialize(Stream stream, MASDKImportResult instance)
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
		if (instance.HasResult)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.Result);
		}
		if (instance.HasImportType_)
		{
			stream.WriteByte(32);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.ImportType_);
		}
		if (instance.HasErrorCode)
		{
			stream.WriteByte(40);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.ErrorCode);
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
		if (HasResult)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)Result);
		}
		if (HasImportType_)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)ImportType_);
		}
		if (HasErrorCode)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)ErrorCode);
		}
		return size;
	}
}
