using System.IO;
using System.Text;

namespace Blizzard.Telemetry.WTCG.Client;

public class Player : IProtoBuf
{
	public enum BnetRegionEnum
	{
		REGION_UNINITIALIZED = -1,
		REGION_UNKNOWN = 0,
		REGION_US = 1,
		REGION_EU = 2,
		REGION_KR = 3,
		REGION_TW = 4,
		REGION_CN = 5,
		REGION_LIVE_VERIFICATION = 40,
		REGION_PTR_LOC = 41,
		REGION_DEV = 60,
		REGION_PTR = 98
	}

	public bool HasBattleNetIdLo;

	private long _BattleNetIdLo;

	public bool HasGameAccountId;

	private long _GameAccountId;

	public bool HasBnetRegion;

	private string _BnetRegion;

	public bool HasLocale;

	private string _Locale;

	public bool HasBnetGameRegion;

	private BnetRegionEnum _BnetGameRegion;

	public long BattleNetIdLo
	{
		get
		{
			return _BattleNetIdLo;
		}
		set
		{
			_BattleNetIdLo = value;
			HasBattleNetIdLo = true;
		}
	}

	public long GameAccountId
	{
		get
		{
			return _GameAccountId;
		}
		set
		{
			_GameAccountId = value;
			HasGameAccountId = true;
		}
	}

	public string BnetRegion
	{
		get
		{
			return _BnetRegion;
		}
		set
		{
			_BnetRegion = value;
			HasBnetRegion = value != null;
		}
	}

	public string Locale
	{
		get
		{
			return _Locale;
		}
		set
		{
			_Locale = value;
			HasLocale = value != null;
		}
	}

	public BnetRegionEnum BnetGameRegion
	{
		get
		{
			return _BnetGameRegion;
		}
		set
		{
			_BnetGameRegion = value;
			HasBnetGameRegion = true;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasBattleNetIdLo)
		{
			hash ^= BattleNetIdLo.GetHashCode();
		}
		if (HasGameAccountId)
		{
			hash ^= GameAccountId.GetHashCode();
		}
		if (HasBnetRegion)
		{
			hash ^= BnetRegion.GetHashCode();
		}
		if (HasLocale)
		{
			hash ^= Locale.GetHashCode();
		}
		if (HasBnetGameRegion)
		{
			hash ^= BnetGameRegion.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is Player other))
		{
			return false;
		}
		if (HasBattleNetIdLo != other.HasBattleNetIdLo || (HasBattleNetIdLo && !BattleNetIdLo.Equals(other.BattleNetIdLo)))
		{
			return false;
		}
		if (HasGameAccountId != other.HasGameAccountId || (HasGameAccountId && !GameAccountId.Equals(other.GameAccountId)))
		{
			return false;
		}
		if (HasBnetRegion != other.HasBnetRegion || (HasBnetRegion && !BnetRegion.Equals(other.BnetRegion)))
		{
			return false;
		}
		if (HasLocale != other.HasLocale || (HasLocale && !Locale.Equals(other.Locale)))
		{
			return false;
		}
		if (HasBnetGameRegion != other.HasBnetGameRegion || (HasBnetGameRegion && !BnetGameRegion.Equals(other.BnetGameRegion)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static Player Deserialize(Stream stream, Player instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static Player DeserializeLengthDelimited(Stream stream)
	{
		Player instance = new Player();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static Player DeserializeLengthDelimited(Stream stream, Player instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static Player Deserialize(Stream stream, Player instance, long limit)
	{
		instance.BnetGameRegion = BnetRegionEnum.REGION_UNINITIALIZED;
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
			case 8:
				instance.BattleNetIdLo = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			case 16:
				instance.GameAccountId = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			case 26:
				instance.BnetRegion = ProtocolParser.ReadString(stream);
				continue;
			case 34:
				instance.Locale = ProtocolParser.ReadString(stream);
				continue;
			case 40:
				instance.BnetGameRegion = (BnetRegionEnum)ProtocolParser.ReadUInt64(stream);
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

	public static void Serialize(Stream stream, Player instance)
	{
		if (instance.HasBattleNetIdLo)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.BattleNetIdLo);
		}
		if (instance.HasGameAccountId)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.GameAccountId);
		}
		if (instance.HasBnetRegion)
		{
			stream.WriteByte(26);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.BnetRegion));
		}
		if (instance.HasLocale)
		{
			stream.WriteByte(34);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.Locale));
		}
		if (instance.HasBnetGameRegion)
		{
			stream.WriteByte(40);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.BnetGameRegion);
		}
	}

	public uint GetSerializedSize()
	{
		uint size = 0u;
		if (HasBattleNetIdLo)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)BattleNetIdLo);
		}
		if (HasGameAccountId)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)GameAccountId);
		}
		if (HasBnetRegion)
		{
			size++;
			uint byteCount3 = (uint)Encoding.UTF8.GetByteCount(BnetRegion);
			size += ProtocolParser.SizeOfUInt32(byteCount3) + byteCount3;
		}
		if (HasLocale)
		{
			size++;
			uint byteCount4 = (uint)Encoding.UTF8.GetByteCount(Locale);
			size += ProtocolParser.SizeOfUInt32(byteCount4) + byteCount4;
		}
		if (HasBnetGameRegion)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)BnetGameRegion);
		}
		return size;
	}
}
