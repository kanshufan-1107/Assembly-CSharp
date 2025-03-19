using System.IO;
using System.Text;

namespace Blizzard.Telemetry.WTCG.Client;

public class ThirdPartyPurchaseReceiptReceived : IProtoBuf
{
	public bool HasPlayer;

	private Player _Player;

	public bool HasDeviceInfo;

	private DeviceInfo _DeviceInfo;

	public bool HasTransactionId;

	private string _TransactionId;

	public bool HasProductId;

	private string _ProductId;

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
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is ThirdPartyPurchaseReceiptReceived other))
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
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static ThirdPartyPurchaseReceiptReceived Deserialize(Stream stream, ThirdPartyPurchaseReceiptReceived instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static ThirdPartyPurchaseReceiptReceived DeserializeLengthDelimited(Stream stream)
	{
		ThirdPartyPurchaseReceiptReceived instance = new ThirdPartyPurchaseReceiptReceived();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static ThirdPartyPurchaseReceiptReceived DeserializeLengthDelimited(Stream stream, ThirdPartyPurchaseReceiptReceived instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static ThirdPartyPurchaseReceiptReceived Deserialize(Stream stream, ThirdPartyPurchaseReceiptReceived instance, long limit)
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
			case 26:
				instance.TransactionId = ProtocolParser.ReadString(stream);
				continue;
			case 34:
				instance.ProductId = ProtocolParser.ReadString(stream);
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

	public static void Serialize(Stream stream, ThirdPartyPurchaseReceiptReceived instance)
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
		return size;
	}
}
