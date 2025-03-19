using System.IO;

namespace Blizzard.Telemetry.WTCG.Client;

public class InitialClientStateOutOfOrder : IProtoBuf
{
	public bool HasCountNotificationsAchieve;

	private int _CountNotificationsAchieve;

	public bool HasCountNotificationsNotice;

	private int _CountNotificationsNotice;

	public bool HasCountNotificationsCollection;

	private int _CountNotificationsCollection;

	public bool HasCountNotificationsCurrency;

	private int _CountNotificationsCurrency;

	public bool HasCountNotificationsBooster;

	private int _CountNotificationsBooster;

	public bool HasCountNotificationsHeroxp;

	private int _CountNotificationsHeroxp;

	public bool HasCountNotificationsPlayerRecord;

	private int _CountNotificationsPlayerRecord;

	public bool HasCountNotificationsArenaSession;

	private int _CountNotificationsArenaSession;

	public bool HasCountNotificationsCardBack;

	private int _CountNotificationsCardBack;

	public bool HasDeviceInfo;

	private DeviceInfo _DeviceInfo;

	public int CountNotificationsAchieve
	{
		get
		{
			return _CountNotificationsAchieve;
		}
		set
		{
			_CountNotificationsAchieve = value;
			HasCountNotificationsAchieve = true;
		}
	}

	public int CountNotificationsNotice
	{
		get
		{
			return _CountNotificationsNotice;
		}
		set
		{
			_CountNotificationsNotice = value;
			HasCountNotificationsNotice = true;
		}
	}

	public int CountNotificationsCollection
	{
		get
		{
			return _CountNotificationsCollection;
		}
		set
		{
			_CountNotificationsCollection = value;
			HasCountNotificationsCollection = true;
		}
	}

	public int CountNotificationsCurrency
	{
		get
		{
			return _CountNotificationsCurrency;
		}
		set
		{
			_CountNotificationsCurrency = value;
			HasCountNotificationsCurrency = true;
		}
	}

	public int CountNotificationsBooster
	{
		get
		{
			return _CountNotificationsBooster;
		}
		set
		{
			_CountNotificationsBooster = value;
			HasCountNotificationsBooster = true;
		}
	}

	public int CountNotificationsHeroxp
	{
		get
		{
			return _CountNotificationsHeroxp;
		}
		set
		{
			_CountNotificationsHeroxp = value;
			HasCountNotificationsHeroxp = true;
		}
	}

	public int CountNotificationsPlayerRecord
	{
		get
		{
			return _CountNotificationsPlayerRecord;
		}
		set
		{
			_CountNotificationsPlayerRecord = value;
			HasCountNotificationsPlayerRecord = true;
		}
	}

	public int CountNotificationsArenaSession
	{
		get
		{
			return _CountNotificationsArenaSession;
		}
		set
		{
			_CountNotificationsArenaSession = value;
			HasCountNotificationsArenaSession = true;
		}
	}

	public int CountNotificationsCardBack
	{
		get
		{
			return _CountNotificationsCardBack;
		}
		set
		{
			_CountNotificationsCardBack = value;
			HasCountNotificationsCardBack = true;
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

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasCountNotificationsAchieve)
		{
			hash ^= CountNotificationsAchieve.GetHashCode();
		}
		if (HasCountNotificationsNotice)
		{
			hash ^= CountNotificationsNotice.GetHashCode();
		}
		if (HasCountNotificationsCollection)
		{
			hash ^= CountNotificationsCollection.GetHashCode();
		}
		if (HasCountNotificationsCurrency)
		{
			hash ^= CountNotificationsCurrency.GetHashCode();
		}
		if (HasCountNotificationsBooster)
		{
			hash ^= CountNotificationsBooster.GetHashCode();
		}
		if (HasCountNotificationsHeroxp)
		{
			hash ^= CountNotificationsHeroxp.GetHashCode();
		}
		if (HasCountNotificationsPlayerRecord)
		{
			hash ^= CountNotificationsPlayerRecord.GetHashCode();
		}
		if (HasCountNotificationsArenaSession)
		{
			hash ^= CountNotificationsArenaSession.GetHashCode();
		}
		if (HasCountNotificationsCardBack)
		{
			hash ^= CountNotificationsCardBack.GetHashCode();
		}
		if (HasDeviceInfo)
		{
			hash ^= DeviceInfo.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is InitialClientStateOutOfOrder other))
		{
			return false;
		}
		if (HasCountNotificationsAchieve != other.HasCountNotificationsAchieve || (HasCountNotificationsAchieve && !CountNotificationsAchieve.Equals(other.CountNotificationsAchieve)))
		{
			return false;
		}
		if (HasCountNotificationsNotice != other.HasCountNotificationsNotice || (HasCountNotificationsNotice && !CountNotificationsNotice.Equals(other.CountNotificationsNotice)))
		{
			return false;
		}
		if (HasCountNotificationsCollection != other.HasCountNotificationsCollection || (HasCountNotificationsCollection && !CountNotificationsCollection.Equals(other.CountNotificationsCollection)))
		{
			return false;
		}
		if (HasCountNotificationsCurrency != other.HasCountNotificationsCurrency || (HasCountNotificationsCurrency && !CountNotificationsCurrency.Equals(other.CountNotificationsCurrency)))
		{
			return false;
		}
		if (HasCountNotificationsBooster != other.HasCountNotificationsBooster || (HasCountNotificationsBooster && !CountNotificationsBooster.Equals(other.CountNotificationsBooster)))
		{
			return false;
		}
		if (HasCountNotificationsHeroxp != other.HasCountNotificationsHeroxp || (HasCountNotificationsHeroxp && !CountNotificationsHeroxp.Equals(other.CountNotificationsHeroxp)))
		{
			return false;
		}
		if (HasCountNotificationsPlayerRecord != other.HasCountNotificationsPlayerRecord || (HasCountNotificationsPlayerRecord && !CountNotificationsPlayerRecord.Equals(other.CountNotificationsPlayerRecord)))
		{
			return false;
		}
		if (HasCountNotificationsArenaSession != other.HasCountNotificationsArenaSession || (HasCountNotificationsArenaSession && !CountNotificationsArenaSession.Equals(other.CountNotificationsArenaSession)))
		{
			return false;
		}
		if (HasCountNotificationsCardBack != other.HasCountNotificationsCardBack || (HasCountNotificationsCardBack && !CountNotificationsCardBack.Equals(other.CountNotificationsCardBack)))
		{
			return false;
		}
		if (HasDeviceInfo != other.HasDeviceInfo || (HasDeviceInfo && !DeviceInfo.Equals(other.DeviceInfo)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static InitialClientStateOutOfOrder Deserialize(Stream stream, InitialClientStateOutOfOrder instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static InitialClientStateOutOfOrder DeserializeLengthDelimited(Stream stream)
	{
		InitialClientStateOutOfOrder instance = new InitialClientStateOutOfOrder();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static InitialClientStateOutOfOrder DeserializeLengthDelimited(Stream stream, InitialClientStateOutOfOrder instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static InitialClientStateOutOfOrder Deserialize(Stream stream, InitialClientStateOutOfOrder instance, long limit)
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
			case 16:
				instance.CountNotificationsAchieve = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 24:
				instance.CountNotificationsNotice = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 32:
				instance.CountNotificationsCollection = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 40:
				instance.CountNotificationsCurrency = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 48:
				instance.CountNotificationsBooster = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 56:
				instance.CountNotificationsHeroxp = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 64:
				instance.CountNotificationsPlayerRecord = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 72:
				instance.CountNotificationsArenaSession = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 80:
				instance.CountNotificationsCardBack = (int)ProtocolParser.ReadUInt64(stream);
				continue;
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

	public static void Serialize(Stream stream, InitialClientStateOutOfOrder instance)
	{
		if (instance.HasCountNotificationsAchieve)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.CountNotificationsAchieve);
		}
		if (instance.HasCountNotificationsNotice)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.CountNotificationsNotice);
		}
		if (instance.HasCountNotificationsCollection)
		{
			stream.WriteByte(32);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.CountNotificationsCollection);
		}
		if (instance.HasCountNotificationsCurrency)
		{
			stream.WriteByte(40);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.CountNotificationsCurrency);
		}
		if (instance.HasCountNotificationsBooster)
		{
			stream.WriteByte(48);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.CountNotificationsBooster);
		}
		if (instance.HasCountNotificationsHeroxp)
		{
			stream.WriteByte(56);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.CountNotificationsHeroxp);
		}
		if (instance.HasCountNotificationsPlayerRecord)
		{
			stream.WriteByte(64);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.CountNotificationsPlayerRecord);
		}
		if (instance.HasCountNotificationsArenaSession)
		{
			stream.WriteByte(72);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.CountNotificationsArenaSession);
		}
		if (instance.HasCountNotificationsCardBack)
		{
			stream.WriteByte(80);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.CountNotificationsCardBack);
		}
		if (instance.HasDeviceInfo)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteUInt32(stream, instance.DeviceInfo.GetSerializedSize());
			DeviceInfo.Serialize(stream, instance.DeviceInfo);
		}
	}

	public uint GetSerializedSize()
	{
		uint size = 0u;
		if (HasCountNotificationsAchieve)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)CountNotificationsAchieve);
		}
		if (HasCountNotificationsNotice)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)CountNotificationsNotice);
		}
		if (HasCountNotificationsCollection)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)CountNotificationsCollection);
		}
		if (HasCountNotificationsCurrency)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)CountNotificationsCurrency);
		}
		if (HasCountNotificationsBooster)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)CountNotificationsBooster);
		}
		if (HasCountNotificationsHeroxp)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)CountNotificationsHeroxp);
		}
		if (HasCountNotificationsPlayerRecord)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)CountNotificationsPlayerRecord);
		}
		if (HasCountNotificationsArenaSession)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)CountNotificationsArenaSession);
		}
		if (HasCountNotificationsCardBack)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)CountNotificationsCardBack);
		}
		if (HasDeviceInfo)
		{
			size++;
			uint size2 = DeviceInfo.GetSerializedSize();
			size += size2 + ProtocolParser.SizeOfUInt32(size2);
		}
		return size;
	}
}
