using System.IO;
using System.Text;

namespace Blizzard.Telemetry.WTCG.Client;

public class ThirdPartyPurchaseReceiptSubmitFail : IProtoBuf
{
	public enum FailureReason
	{
		INVALID_STATE = 1,
		INVALID_PROVIDER,
		NO_ACTIVE_TRANSACTION,
		NO_THIRD_PARTY_USER_ID
	}

	public bool HasPlayer;

	private Player _Player;

	public bool HasDeviceInfo;

	private DeviceInfo _DeviceInfo;

	public bool HasTransactionId;

	private string _TransactionId;

	public bool HasProductId;

	private string _ProductId;

	public bool HasProvider;

	private string _Provider;

	public bool HasReason;

	private FailureReason _Reason;

	public bool HasInvalidData;

	private string _InvalidData;

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

	public string TransactionId
	{
		get
		{
			return _TransactionId;
		}
		set
		{
			_TransactionId = value;
			HasTransactionId = value != null;
		}
	}

	public string ProductId
	{
		get
		{
			return _ProductId;
		}
		set
		{
			_ProductId = value;
			HasProductId = value != null;
		}
	}

	public string Provider
	{
		get
		{
			return _Provider;
		}
		set
		{
			_Provider = value;
			HasProvider = value != null;
		}
	}

	public FailureReason Reason
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

	public string InvalidData
	{
		get
		{
			return _InvalidData;
		}
		set
		{
			_InvalidData = value;
			HasInvalidData = value != null;
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
		if (HasTransactionId)
		{
			hash ^= TransactionId.GetHashCode();
		}
		if (HasProductId)
		{
			hash ^= ProductId.GetHashCode();
		}
		if (HasProvider)
		{
			hash ^= Provider.GetHashCode();
		}
		if (HasReason)
		{
			hash ^= Reason.GetHashCode();
		}
		if (HasInvalidData)
		{
			hash ^= InvalidData.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is ThirdPartyPurchaseReceiptSubmitFail other))
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
		if (HasTransactionId != other.HasTransactionId || (HasTransactionId && !TransactionId.Equals(other.TransactionId)))
		{
			return false;
		}
		if (HasProductId != other.HasProductId || (HasProductId && !ProductId.Equals(other.ProductId)))
		{
			return false;
		}
		if (HasProvider != other.HasProvider || (HasProvider && !Provider.Equals(other.Provider)))
		{
			return false;
		}
		if (HasReason != other.HasReason || (HasReason && !Reason.Equals(other.Reason)))
		{
			return false;
		}
		if (HasInvalidData != other.HasInvalidData || (HasInvalidData && !InvalidData.Equals(other.InvalidData)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static ThirdPartyPurchaseReceiptSubmitFail Deserialize(Stream stream, ThirdPartyPurchaseReceiptSubmitFail instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static ThirdPartyPurchaseReceiptSubmitFail DeserializeLengthDelimited(Stream stream)
	{
		ThirdPartyPurchaseReceiptSubmitFail instance = new ThirdPartyPurchaseReceiptSubmitFail();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static ThirdPartyPurchaseReceiptSubmitFail DeserializeLengthDelimited(Stream stream, ThirdPartyPurchaseReceiptSubmitFail instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static ThirdPartyPurchaseReceiptSubmitFail Deserialize(Stream stream, ThirdPartyPurchaseReceiptSubmitFail instance, long limit)
	{
		instance.Reason = FailureReason.INVALID_STATE;
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
			case 26:
				instance.TransactionId = ProtocolParser.ReadString(stream);
				continue;
			case 34:
				instance.ProductId = ProtocolParser.ReadString(stream);
				continue;
			case 42:
				instance.Provider = ProtocolParser.ReadString(stream);
				continue;
			case 48:
				instance.Reason = (FailureReason)ProtocolParser.ReadUInt64(stream);
				continue;
			case 58:
				instance.InvalidData = ProtocolParser.ReadString(stream);
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

	public static void Serialize(Stream stream, ThirdPartyPurchaseReceiptSubmitFail instance)
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
		if (instance.HasTransactionId)
		{
			stream.WriteByte(26);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.TransactionId));
		}
		if (instance.HasProductId)
		{
			stream.WriteByte(34);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.ProductId));
		}
		if (instance.HasProvider)
		{
			stream.WriteByte(42);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.Provider));
		}
		if (instance.HasReason)
		{
			stream.WriteByte(48);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.Reason);
		}
		if (instance.HasInvalidData)
		{
			stream.WriteByte(58);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.InvalidData));
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
		if (HasTransactionId)
		{
			size++;
			uint byteCount3 = (uint)Encoding.UTF8.GetByteCount(TransactionId);
			size += ProtocolParser.SizeOfUInt32(byteCount3) + byteCount3;
		}
		if (HasProductId)
		{
			size++;
			uint byteCount4 = (uint)Encoding.UTF8.GetByteCount(ProductId);
			size += ProtocolParser.SizeOfUInt32(byteCount4) + byteCount4;
		}
		if (HasProvider)
		{
			size++;
			uint byteCount5 = (uint)Encoding.UTF8.GetByteCount(Provider);
			size += ProtocolParser.SizeOfUInt32(byteCount5) + byteCount5;
		}
		if (HasReason)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)Reason);
		}
		if (HasInvalidData)
		{
			size++;
			uint byteCount7 = (uint)Encoding.UTF8.GetByteCount(InvalidData);
			size += ProtocolParser.SizeOfUInt32(byteCount7) + byteCount7;
		}
		return size;
	}
}
