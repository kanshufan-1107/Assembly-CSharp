using System.IO;
using System.Text;

namespace Blizzard.Telemetry.WTCG.Client;

public class AttributionGameRoundEnd : IProtoBuf
{
	public bool HasApplicationId;

	private string _ApplicationId;

	public bool HasDeviceType;

	private string _DeviceType;

	public bool HasFirstInstallDate;

	private ulong _FirstInstallDate;

	public bool HasBundleId;

	private string _BundleId;

	public bool HasGameMode;

	private string _GameMode;

	public bool HasResult;

	private string _Result;

	public bool HasRank;

	private int _Rank;

	public bool HasWild;

	private bool _Wild;

	public bool HasFormatType;

	private FormatType _FormatType;

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

	public string GameMode
	{
		get
		{
			return _GameMode;
		}
		set
		{
			_GameMode = value;
			HasGameMode = value != null;
		}
	}

	public string Result
	{
		get
		{
			return _Result;
		}
		set
		{
			_Result = value;
			HasResult = value != null;
		}
	}

	public int Rank
	{
		get
		{
			return _Rank;
		}
		set
		{
			_Rank = value;
			HasRank = true;
		}
	}

	public bool Wild
	{
		get
		{
			return _Wild;
		}
		set
		{
			_Wild = value;
			HasWild = true;
		}
	}

	public FormatType FormatType
	{
		get
		{
			return _FormatType;
		}
		set
		{
			_FormatType = value;
			HasFormatType = true;
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
		if (HasGameMode)
		{
			hash ^= GameMode.GetHashCode();
		}
		if (HasResult)
		{
			hash ^= Result.GetHashCode();
		}
		if (HasRank)
		{
			hash ^= Rank.GetHashCode();
		}
		if (HasWild)
		{
			hash ^= Wild.GetHashCode();
		}
		if (HasFormatType)
		{
			hash ^= FormatType.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is AttributionGameRoundEnd other))
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
		if (HasGameMode != other.HasGameMode || (HasGameMode && !GameMode.Equals(other.GameMode)))
		{
			return false;
		}
		if (HasResult != other.HasResult || (HasResult && !Result.Equals(other.Result)))
		{
			return false;
		}
		if (HasRank != other.HasRank || (HasRank && !Rank.Equals(other.Rank)))
		{
			return false;
		}
		if (HasWild != other.HasWild || (HasWild && !Wild.Equals(other.Wild)))
		{
			return false;
		}
		if (HasFormatType != other.HasFormatType || (HasFormatType && !FormatType.Equals(other.FormatType)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static AttributionGameRoundEnd Deserialize(Stream stream, AttributionGameRoundEnd instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static AttributionGameRoundEnd DeserializeLengthDelimited(Stream stream)
	{
		AttributionGameRoundEnd instance = new AttributionGameRoundEnd();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static AttributionGameRoundEnd DeserializeLengthDelimited(Stream stream, AttributionGameRoundEnd instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static AttributionGameRoundEnd Deserialize(Stream stream, AttributionGameRoundEnd instance, long limit)
	{
		instance.FormatType = FormatType.FT_UNKNOWN;
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
				instance.GameMode = ProtocolParser.ReadString(stream);
				continue;
			case 18:
				instance.Result = ProtocolParser.ReadString(stream);
				continue;
			case 24:
				instance.Rank = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 32:
				instance.Wild = ProtocolParser.ReadBool(stream);
				continue;
			case 40:
				instance.FormatType = (FormatType)ProtocolParser.ReadUInt64(stream);
				continue;
			default:
			{
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
				default:
					ProtocolParser.SkipKey(stream, key);
					break;
				}
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

	public static void Serialize(Stream stream, AttributionGameRoundEnd instance)
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
		if (instance.HasGameMode)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.GameMode));
		}
		if (instance.HasResult)
		{
			stream.WriteByte(18);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.Result));
		}
		if (instance.HasRank)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.Rank);
		}
		if (instance.HasWild)
		{
			stream.WriteByte(32);
			ProtocolParser.WriteBool(stream, instance.Wild);
		}
		if (instance.HasFormatType)
		{
			stream.WriteByte(40);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.FormatType);
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
		if (HasGameMode)
		{
			size++;
			uint byteCount104 = (uint)Encoding.UTF8.GetByteCount(GameMode);
			size += ProtocolParser.SizeOfUInt32(byteCount104) + byteCount104;
		}
		if (HasResult)
		{
			size++;
			uint byteCount105 = (uint)Encoding.UTF8.GetByteCount(Result);
			size += ProtocolParser.SizeOfUInt32(byteCount105) + byteCount105;
		}
		if (HasRank)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)Rank);
		}
		if (HasWild)
		{
			size++;
			size++;
		}
		if (HasFormatType)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)FormatType);
		}
		return size;
	}
}
