using System.IO;
using System.Text;

namespace Blizzard.Telemetry.WTCG.Client;

public class AssetLoaderError : IProtoBuf
{
	public enum AssetBundleErrorReason
	{
		NoError,
		NullGuid,
		UnknownGuid,
		MissingBundle,
		FailedToOpenBundle,
		MissingDependency,
		FailedToGetBundlePath
	}

	public bool HasDeviceInfo;

	private DeviceInfo _DeviceInfo;

	public bool HasAssetGuid;

	private string _AssetGuid;

	public bool HasBundleName;

	private string _BundleName;

	public bool HasReason;

	private AssetBundleErrorReason _Reason;

	public bool HasDetail;

	private string _Detail;

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

	public string AssetGuid
	{
		get
		{
			return _AssetGuid;
		}
		set
		{
			_AssetGuid = value;
			HasAssetGuid = value != null;
		}
	}

	public string BundleName
	{
		get
		{
			return _BundleName;
		}
		set
		{
			_BundleName = value;
			HasBundleName = value != null;
		}
	}

	public AssetBundleErrorReason Reason
	{
		get
		{
			return _Reason;
		}
		set
		{
			_Reason = value;
			HasReason = true;
		}
	}

	public string Detail
	{
		get
		{
			return _Detail;
		}
		set
		{
			_Detail = value;
			HasDetail = value != null;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasDeviceInfo)
		{
			hash ^= DeviceInfo.GetHashCode();
		}
		if (HasAssetGuid)
		{
			hash ^= AssetGuid.GetHashCode();
		}
		if (HasBundleName)
		{
			hash ^= BundleName.GetHashCode();
		}
		if (HasReason)
		{
			hash ^= Reason.GetHashCode();
		}
		if (HasDetail)
		{
			hash ^= Detail.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is AssetLoaderError other))
		{
			return false;
		}
		if (HasDeviceInfo != other.HasDeviceInfo || (HasDeviceInfo && !DeviceInfo.Equals(other.DeviceInfo)))
		{
			return false;
		}
		if (HasAssetGuid != other.HasAssetGuid || (HasAssetGuid && !AssetGuid.Equals(other.AssetGuid)))
		{
			return false;
		}
		if (HasBundleName != other.HasBundleName || (HasBundleName && !BundleName.Equals(other.BundleName)))
		{
			return false;
		}
		if (HasReason != other.HasReason || (HasReason && !Reason.Equals(other.Reason)))
		{
			return false;
		}
		if (HasDetail != other.HasDetail || (HasDetail && !Detail.Equals(other.Detail)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static AssetLoaderError Deserialize(Stream stream, AssetLoaderError instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static AssetLoaderError DeserializeLengthDelimited(Stream stream)
	{
		AssetLoaderError instance = new AssetLoaderError();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static AssetLoaderError DeserializeLengthDelimited(Stream stream, AssetLoaderError instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static AssetLoaderError Deserialize(Stream stream, AssetLoaderError instance, long limit)
	{
		instance.Reason = AssetBundleErrorReason.NoError;
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
				instance.AssetGuid = ProtocolParser.ReadString(stream);
				continue;
			case 26:
				instance.BundleName = ProtocolParser.ReadString(stream);
				continue;
			case 32:
				instance.Reason = (AssetBundleErrorReason)ProtocolParser.ReadUInt64(stream);
				continue;
			case 42:
				instance.Detail = ProtocolParser.ReadString(stream);
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

	public static void Serialize(Stream stream, AssetLoaderError instance)
	{
		if (instance.HasDeviceInfo)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteUInt32(stream, instance.DeviceInfo.GetSerializedSize());
			DeviceInfo.Serialize(stream, instance.DeviceInfo);
		}
		if (instance.HasAssetGuid)
		{
			stream.WriteByte(18);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.AssetGuid));
		}
		if (instance.HasBundleName)
		{
			stream.WriteByte(26);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.BundleName));
		}
		if (instance.HasReason)
		{
			stream.WriteByte(32);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.Reason);
		}
		if (instance.HasDetail)
		{
			stream.WriteByte(42);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.Detail));
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
		if (HasAssetGuid)
		{
			size++;
			uint byteCount2 = (uint)Encoding.UTF8.GetByteCount(AssetGuid);
			size += ProtocolParser.SizeOfUInt32(byteCount2) + byteCount2;
		}
		if (HasBundleName)
		{
			size++;
			uint byteCount3 = (uint)Encoding.UTF8.GetByteCount(BundleName);
			size += ProtocolParser.SizeOfUInt32(byteCount3) + byteCount3;
		}
		if (HasReason)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)Reason);
		}
		if (HasDetail)
		{
			size++;
			uint byteCount5 = (uint)Encoding.UTF8.GetByteCount(Detail);
			size += ProtocolParser.SizeOfUInt32(byteCount5) + byteCount5;
		}
		return size;
	}
}
