using System.IO;

namespace Blizzard.Telemetry.WTCG.Client;

public class EndGameScreenInit : IProtoBuf
{
	public bool HasPlayer;

	private Player _Player;

	public bool HasDeviceInfo;

	private DeviceInfo _DeviceInfo;

	public bool HasElapsedTime;

	private float _ElapsedTime;

	public bool HasMedalInfoRetryCount;

	private int _MedalInfoRetryCount;

	public bool HasMedalInfoRetriesTimedOut;

	private bool _MedalInfoRetriesTimedOut;

	public bool HasShowRankedReward;

	private bool _ShowRankedReward;

	public bool HasShowCardBackProgress;

	private bool _ShowCardBackProgress;

	public bool HasOtherRewardCount;

	private int _OtherRewardCount;

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

	public float ElapsedTime
	{
		get
		{
			return _ElapsedTime;
		}
		set
		{
			_ElapsedTime = value;
			HasElapsedTime = true;
		}
	}

	public int MedalInfoRetryCount
	{
		get
		{
			return _MedalInfoRetryCount;
		}
		set
		{
			_MedalInfoRetryCount = value;
			HasMedalInfoRetryCount = true;
		}
	}

	public bool MedalInfoRetriesTimedOut
	{
		get
		{
			return _MedalInfoRetriesTimedOut;
		}
		set
		{
			_MedalInfoRetriesTimedOut = value;
			HasMedalInfoRetriesTimedOut = true;
		}
	}

	public bool ShowRankedReward
	{
		get
		{
			return _ShowRankedReward;
		}
		set
		{
			_ShowRankedReward = value;
			HasShowRankedReward = true;
		}
	}

	public bool ShowCardBackProgress
	{
		get
		{
			return _ShowCardBackProgress;
		}
		set
		{
			_ShowCardBackProgress = value;
			HasShowCardBackProgress = true;
		}
	}

	public int OtherRewardCount
	{
		get
		{
			return _OtherRewardCount;
		}
		set
		{
			_OtherRewardCount = value;
			HasOtherRewardCount = true;
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
		if (HasElapsedTime)
		{
			hash ^= ElapsedTime.GetHashCode();
		}
		if (HasMedalInfoRetryCount)
		{
			hash ^= MedalInfoRetryCount.GetHashCode();
		}
		if (HasMedalInfoRetriesTimedOut)
		{
			hash ^= MedalInfoRetriesTimedOut.GetHashCode();
		}
		if (HasShowRankedReward)
		{
			hash ^= ShowRankedReward.GetHashCode();
		}
		if (HasShowCardBackProgress)
		{
			hash ^= ShowCardBackProgress.GetHashCode();
		}
		if (HasOtherRewardCount)
		{
			hash ^= OtherRewardCount.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is EndGameScreenInit other))
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
		if (HasElapsedTime != other.HasElapsedTime || (HasElapsedTime && !ElapsedTime.Equals(other.ElapsedTime)))
		{
			return false;
		}
		if (HasMedalInfoRetryCount != other.HasMedalInfoRetryCount || (HasMedalInfoRetryCount && !MedalInfoRetryCount.Equals(other.MedalInfoRetryCount)))
		{
			return false;
		}
		if (HasMedalInfoRetriesTimedOut != other.HasMedalInfoRetriesTimedOut || (HasMedalInfoRetriesTimedOut && !MedalInfoRetriesTimedOut.Equals(other.MedalInfoRetriesTimedOut)))
		{
			return false;
		}
		if (HasShowRankedReward != other.HasShowRankedReward || (HasShowRankedReward && !ShowRankedReward.Equals(other.ShowRankedReward)))
		{
			return false;
		}
		if (HasShowCardBackProgress != other.HasShowCardBackProgress || (HasShowCardBackProgress && !ShowCardBackProgress.Equals(other.ShowCardBackProgress)))
		{
			return false;
		}
		if (HasOtherRewardCount != other.HasOtherRewardCount || (HasOtherRewardCount && !OtherRewardCount.Equals(other.OtherRewardCount)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static EndGameScreenInit Deserialize(Stream stream, EndGameScreenInit instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static EndGameScreenInit DeserializeLengthDelimited(Stream stream)
	{
		EndGameScreenInit instance = new EndGameScreenInit();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static EndGameScreenInit DeserializeLengthDelimited(Stream stream, EndGameScreenInit instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static EndGameScreenInit Deserialize(Stream stream, EndGameScreenInit instance, long limit)
	{
		BinaryReader br = new BinaryReader(stream);
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
			case 29:
				instance.ElapsedTime = br.ReadSingle();
				continue;
			case 32:
				instance.MedalInfoRetryCount = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 40:
				instance.MedalInfoRetriesTimedOut = ProtocolParser.ReadBool(stream);
				continue;
			case 48:
				instance.ShowRankedReward = ProtocolParser.ReadBool(stream);
				continue;
			case 56:
				instance.ShowCardBackProgress = ProtocolParser.ReadBool(stream);
				continue;
			case 64:
				instance.OtherRewardCount = (int)ProtocolParser.ReadUInt64(stream);
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

	public static void Serialize(Stream stream, EndGameScreenInit instance)
	{
		BinaryWriter bw = new BinaryWriter(stream);
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
		if (instance.HasElapsedTime)
		{
			stream.WriteByte(29);
			bw.Write(instance.ElapsedTime);
		}
		if (instance.HasMedalInfoRetryCount)
		{
			stream.WriteByte(32);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.MedalInfoRetryCount);
		}
		if (instance.HasMedalInfoRetriesTimedOut)
		{
			stream.WriteByte(40);
			ProtocolParser.WriteBool(stream, instance.MedalInfoRetriesTimedOut);
		}
		if (instance.HasShowRankedReward)
		{
			stream.WriteByte(48);
			ProtocolParser.WriteBool(stream, instance.ShowRankedReward);
		}
		if (instance.HasShowCardBackProgress)
		{
			stream.WriteByte(56);
			ProtocolParser.WriteBool(stream, instance.ShowCardBackProgress);
		}
		if (instance.HasOtherRewardCount)
		{
			stream.WriteByte(64);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.OtherRewardCount);
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
		if (HasElapsedTime)
		{
			size++;
			size += 4;
		}
		if (HasMedalInfoRetryCount)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)MedalInfoRetryCount);
		}
		if (HasMedalInfoRetriesTimedOut)
		{
			size++;
			size++;
		}
		if (HasShowRankedReward)
		{
			size++;
			size++;
		}
		if (HasShowCardBackProgress)
		{
			size++;
			size++;
		}
		if (HasOtherRewardCount)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)OtherRewardCount);
		}
		return size;
	}
}
