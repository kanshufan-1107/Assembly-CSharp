using System.IO;
using System.Text;

namespace Blizzard.Telemetry.WTCG.Client;

public class AttributionInstall : IProtoBuf
{
	public bool HasApplicationId;

	private string _ApplicationId;

	public bool HasDeviceType;

	private string _DeviceType;

	public bool HasFirstInstallDate;

	private ulong _FirstInstallDate;

	public bool HasBundleId;

	private string _BundleId;

	public bool HasReferrer;

	private string _Referrer;

	public bool HasAppleSearchAdsJson;

	private string _AppleSearchAdsJson;

	public bool HasAppleSearchAdsErrorCode;

	private int _AppleSearchAdsErrorCode;

	public bool HasIdentifier;

	private IdentifierInfo _Identifier;

	public string ApplicationId
	{
		get
		{
			return _ApplicationId;
		}
		set
		{
			_ApplicationId = value;
			HasApplicationId = value != null;
		}
	}

	public string DeviceType
	{
		get
		{
			return _DeviceType;
		}
		set
		{
			_DeviceType = value;
			HasDeviceType = value != null;
		}
	}

	public ulong FirstInstallDate
	{
		get
		{
			return _FirstInstallDate;
		}
		set
		{
			_FirstInstallDate = value;
			HasFirstInstallDate = true;
		}
	}

	public string BundleId
	{
		get
		{
			return _BundleId;
		}
		set
		{
			_BundleId = value;
			HasBundleId = value != null;
		}
	}

	public string Referrer
	{
		get
		{
			return _Referrer;
		}
		set
		{
			_Referrer = value;
			HasReferrer = value != null;
		}
	}

	public string AppleSearchAdsJson
	{
		get
		{
			return _AppleSearchAdsJson;
		}
		set
		{
			_AppleSearchAdsJson = value;
			HasAppleSearchAdsJson = value != null;
		}
	}

	public int AppleSearchAdsErrorCode
	{
		get
		{
			return _AppleSearchAdsErrorCode;
		}
		set
		{
			_AppleSearchAdsErrorCode = value;
			HasAppleSearchAdsErrorCode = true;
		}
	}

	public IdentifierInfo Identifier
	{
		get
		{
			return _Identifier;
		}
		set
		{
			_Identifier = value;
			HasIdentifier = value != null;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasApplicationId)
		{
			hash ^= ApplicationId.GetHashCode();
		}
		if (HasDeviceType)
		{
			hash ^= DeviceType.GetHashCode();
		}
		if (HasFirstInstallDate)
		{
			hash ^= FirstInstallDate.GetHashCode();
		}
		if (HasBundleId)
		{
			hash ^= BundleId.GetHashCode();
		}
		if (HasReferrer)
		{
			hash ^= Referrer.GetHashCode();
		}
		if (HasAppleSearchAdsJson)
		{
			hash ^= AppleSearchAdsJson.GetHashCode();
		}
		if (HasAppleSearchAdsErrorCode)
		{
			hash ^= AppleSearchAdsErrorCode.GetHashCode();
		}
		if (HasIdentifier)
		{
			hash ^= Identifier.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is AttributionInstall other))
		{
			return false;
		}
		if (HasApplicationId != other.HasApplicationId || (HasApplicationId && !ApplicationId.Equals(other.ApplicationId)))
		{
			return false;
		}
		if (HasDeviceType != other.HasDeviceType || (HasDeviceType && !DeviceType.Equals(other.DeviceType)))
		{
			return false;
		}
		if (HasFirstInstallDate != other.HasFirstInstallDate || (HasFirstInstallDate && !FirstInstallDate.Equals(other.FirstInstallDate)))
		{
			return false;
		}
		if (HasBundleId != other.HasBundleId || (HasBundleId && !BundleId.Equals(other.BundleId)))
		{
			return false;
		}
		if (HasReferrer != other.HasReferrer || (HasReferrer && !Referrer.Equals(other.Referrer)))
		{
			return false;
		}
		if (HasAppleSearchAdsJson != other.HasAppleSearchAdsJson || (HasAppleSearchAdsJson && !AppleSearchAdsJson.Equals(other.AppleSearchAdsJson)))
		{
			return false;
		}
		if (HasAppleSearchAdsErrorCode != other.HasAppleSearchAdsErrorCode || (HasAppleSearchAdsErrorCode && !AppleSearchAdsErrorCode.Equals(other.AppleSearchAdsErrorCode)))
		{
			return false;
		}
		if (HasIdentifier != other.HasIdentifier || (HasIdentifier && !Identifier.Equals(other.Identifier)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static AttributionInstall Deserialize(Stream stream, AttributionInstall instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static AttributionInstall DeserializeLengthDelimited(Stream stream)
	{
		AttributionInstall instance = new AttributionInstall();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static AttributionInstall DeserializeLengthDelimited(Stream stream, AttributionInstall instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static AttributionInstall Deserialize(Stream stream, AttributionInstall instance, long limit)
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
			if (keyByte == -1)
			{
				if (limit < 0)
				{
					break;
				}
				throw new EndOfStreamException();
			}
			Key key = ProtocolParser.ReadKey((byte)keyByte, stream);
			switch (key.Field)
			{
			case 0u:
				throw new ProtocolBufferException("Invalid field id: 0, something went wrong in the stream");
			case 100u:
				if (key.WireType == Wire.LengthDelimited)
				{
					instance.ApplicationId = ProtocolParser.ReadString(stream);
				}
				break;
			case 101u:
				if (key.WireType == Wire.LengthDelimited)
				{
					instance.DeviceType = ProtocolParser.ReadString(stream);
				}
				break;
			case 102u:
				if (key.WireType == Wire.Varint)
				{
					instance.FirstInstallDate = ProtocolParser.ReadUInt64(stream);
				}
				break;
			case 103u:
				if (key.WireType == Wire.LengthDelimited)
				{
					instance.BundleId = ProtocolParser.ReadString(stream);
				}
				break;
			case 200u:
				if (key.WireType == Wire.LengthDelimited)
				{
					instance.Referrer = ProtocolParser.ReadString(stream);
				}
				break;
			case 300u:
				if (key.WireType == Wire.LengthDelimited)
				{
					instance.AppleSearchAdsJson = ProtocolParser.ReadString(stream);
				}
				break;
			case 301u:
				if (key.WireType == Wire.Varint)
				{
					instance.AppleSearchAdsErrorCode = (int)ProtocolParser.ReadUInt64(stream);
				}
				break;
			case 1000u:
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.Identifier == null)
					{
						instance.Identifier = IdentifierInfo.DeserializeLengthDelimited(stream);
					}
					else
					{
						IdentifierInfo.DeserializeLengthDelimited(stream, instance.Identifier);
					}
				}
				break;
			default:
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public void Serialize(Stream stream)
	{
		Serialize(stream, this);
	}

	public static void Serialize(Stream stream, AttributionInstall instance)
	{
		if (instance.HasApplicationId)
		{
			stream.WriteByte(162);
			stream.WriteByte(6);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.ApplicationId));
		}
		if (instance.HasDeviceType)
		{
			stream.WriteByte(170);
			stream.WriteByte(6);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.DeviceType));
		}
		if (instance.HasFirstInstallDate)
		{
			stream.WriteByte(176);
			stream.WriteByte(6);
			ProtocolParser.WriteUInt64(stream, instance.FirstInstallDate);
		}
		if (instance.HasBundleId)
		{
			stream.WriteByte(186);
			stream.WriteByte(6);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.BundleId));
		}
		if (instance.HasReferrer)
		{
			stream.WriteByte(194);
			stream.WriteByte(12);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.Referrer));
		}
		if (instance.HasAppleSearchAdsJson)
		{
			stream.WriteByte(226);
			stream.WriteByte(18);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.AppleSearchAdsJson));
		}
		if (instance.HasAppleSearchAdsErrorCode)
		{
			stream.WriteByte(232);
			stream.WriteByte(18);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.AppleSearchAdsErrorCode);
		}
		if (instance.HasIdentifier)
		{
			stream.WriteByte(194);
			stream.WriteByte(62);
			ProtocolParser.WriteUInt32(stream, instance.Identifier.GetSerializedSize());
			IdentifierInfo.Serialize(stream, instance.Identifier);
		}
	}

	public uint GetSerializedSize()
	{
		uint size = 0u;
		if (HasApplicationId)
		{
			size += 2;
			uint byteCount100 = (uint)Encoding.UTF8.GetByteCount(ApplicationId);
			size += ProtocolParser.SizeOfUInt32(byteCount100) + byteCount100;
		}
		if (HasDeviceType)
		{
			size += 2;
			uint byteCount101 = (uint)Encoding.UTF8.GetByteCount(DeviceType);
			size += ProtocolParser.SizeOfUInt32(byteCount101) + byteCount101;
		}
		if (HasFirstInstallDate)
		{
			size += 2;
			size += ProtocolParser.SizeOfUInt64(FirstInstallDate);
		}
		if (HasBundleId)
		{
			size += 2;
			uint byteCount103 = (uint)Encoding.UTF8.GetByteCount(BundleId);
			size += ProtocolParser.SizeOfUInt32(byteCount103) + byteCount103;
		}
		if (HasReferrer)
		{
			size += 2;
			uint byteCount200 = (uint)Encoding.UTF8.GetByteCount(Referrer);
			size += ProtocolParser.SizeOfUInt32(byteCount200) + byteCount200;
		}
		if (HasAppleSearchAdsJson)
		{
			size += 2;
			uint byteCount300 = (uint)Encoding.UTF8.GetByteCount(AppleSearchAdsJson);
			size += ProtocolParser.SizeOfUInt32(byteCount300) + byteCount300;
		}
		if (HasAppleSearchAdsErrorCode)
		{
			size += 2;
			size += ProtocolParser.SizeOfUInt64((ulong)AppleSearchAdsErrorCode);
		}
		if (HasIdentifier)
		{
			size += 2;
			uint size1000 = Identifier.GetSerializedSize();
			size += size1000 + ProtocolParser.SizeOfUInt32(size1000);
		}
		return size;
	}
}
