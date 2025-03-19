using System.IO;
using System.Text;

namespace Blizzard.Telemetry.WTCG.Client;

public class FlowPerformance : IProtoBuf
{
	public enum FlowType
	{
		SHOP = 1,
		COLLECTION_MANAGER,
		GAME,
		JOURNAL,
		ARENA,
		DUELS,
		ADVENTURE
	}

	public bool HasUniqueId;

	private string _UniqueId;

	public bool HasDeviceInfo;

	private DeviceInfo _DeviceInfo;

	public bool HasPlayer;

	private Player _Player;

	public bool HasFlowType_;

	private FlowType _FlowType_;

	public bool HasAverageFps;

	private float _AverageFps;

	public bool HasDuration;

	private float _Duration;

	public bool HasFpsWarningsThreshold;

	private float _FpsWarningsThreshold;

	public bool HasFpsWarningsTotalOccurences;

	private int _FpsWarningsTotalOccurences;

	public bool HasFpsWarningsTotalTime;

	private float _FpsWarningsTotalTime;

	public bool HasFpsWarningsAverageTime;

	private float _FpsWarningsAverageTime;

	public bool HasFpsWarningsMaxTime;

	private float _FpsWarningsMaxTime;

	public string UniqueId
	{
		get
		{
			return _UniqueId;
		}
		set
		{
			_UniqueId = value;
			HasUniqueId = value != null;
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

	public FlowType FlowType_
	{
		get
		{
			return _FlowType_;
		}
		set
		{
			_FlowType_ = value;
			HasFlowType_ = true;
		}
	}

	public float AverageFps
	{
		get
		{
			return _AverageFps;
		}
		set
		{
			_AverageFps = value;
			HasAverageFps = true;
		}
	}

	public float Duration
	{
		get
		{
			return _Duration;
		}
		set
		{
			_Duration = value;
			HasDuration = true;
		}
	}

	public float FpsWarningsThreshold
	{
		get
		{
			return _FpsWarningsThreshold;
		}
		set
		{
			_FpsWarningsThreshold = value;
			HasFpsWarningsThreshold = true;
		}
	}

	public int FpsWarningsTotalOccurences
	{
		get
		{
			return _FpsWarningsTotalOccurences;
		}
		set
		{
			_FpsWarningsTotalOccurences = value;
			HasFpsWarningsTotalOccurences = true;
		}
	}

	public float FpsWarningsTotalTime
	{
		get
		{
			return _FpsWarningsTotalTime;
		}
		set
		{
			_FpsWarningsTotalTime = value;
			HasFpsWarningsTotalTime = true;
		}
	}

	public float FpsWarningsAverageTime
	{
		get
		{
			return _FpsWarningsAverageTime;
		}
		set
		{
			_FpsWarningsAverageTime = value;
			HasFpsWarningsAverageTime = true;
		}
	}

	public float FpsWarningsMaxTime
	{
		get
		{
			return _FpsWarningsMaxTime;
		}
		set
		{
			_FpsWarningsMaxTime = value;
			HasFpsWarningsMaxTime = true;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasUniqueId)
		{
			hash ^= UniqueId.GetHashCode();
		}
		if (HasDeviceInfo)
		{
			hash ^= DeviceInfo.GetHashCode();
		}
		if (HasPlayer)
		{
			hash ^= Player.GetHashCode();
		}
		if (HasFlowType_)
		{
			hash ^= FlowType_.GetHashCode();
		}
		if (HasAverageFps)
		{
			hash ^= AverageFps.GetHashCode();
		}
		if (HasDuration)
		{
			hash ^= Duration.GetHashCode();
		}
		if (HasFpsWarningsThreshold)
		{
			hash ^= FpsWarningsThreshold.GetHashCode();
		}
		if (HasFpsWarningsTotalOccurences)
		{
			hash ^= FpsWarningsTotalOccurences.GetHashCode();
		}
		if (HasFpsWarningsTotalTime)
		{
			hash ^= FpsWarningsTotalTime.GetHashCode();
		}
		if (HasFpsWarningsAverageTime)
		{
			hash ^= FpsWarningsAverageTime.GetHashCode();
		}
		if (HasFpsWarningsMaxTime)
		{
			hash ^= FpsWarningsMaxTime.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is FlowPerformance other))
		{
			return false;
		}
		if (HasUniqueId != other.HasUniqueId || (HasUniqueId && !UniqueId.Equals(other.UniqueId)))
		{
			return false;
		}
		if (HasDeviceInfo != other.HasDeviceInfo || (HasDeviceInfo && !DeviceInfo.Equals(other.DeviceInfo)))
		{
			return false;
		}
		if (HasPlayer != other.HasPlayer || (HasPlayer && !Player.Equals(other.Player)))
		{
			return false;
		}
		if (HasFlowType_ != other.HasFlowType_ || (HasFlowType_ && !FlowType_.Equals(other.FlowType_)))
		{
			return false;
		}
		if (HasAverageFps != other.HasAverageFps || (HasAverageFps && !AverageFps.Equals(other.AverageFps)))
		{
			return false;
		}
		if (HasDuration != other.HasDuration || (HasDuration && !Duration.Equals(other.Duration)))
		{
			return false;
		}
		if (HasFpsWarningsThreshold != other.HasFpsWarningsThreshold || (HasFpsWarningsThreshold && !FpsWarningsThreshold.Equals(other.FpsWarningsThreshold)))
		{
			return false;
		}
		if (HasFpsWarningsTotalOccurences != other.HasFpsWarningsTotalOccurences || (HasFpsWarningsTotalOccurences && !FpsWarningsTotalOccurences.Equals(other.FpsWarningsTotalOccurences)))
		{
			return false;
		}
		if (HasFpsWarningsTotalTime != other.HasFpsWarningsTotalTime || (HasFpsWarningsTotalTime && !FpsWarningsTotalTime.Equals(other.FpsWarningsTotalTime)))
		{
			return false;
		}
		if (HasFpsWarningsAverageTime != other.HasFpsWarningsAverageTime || (HasFpsWarningsAverageTime && !FpsWarningsAverageTime.Equals(other.FpsWarningsAverageTime)))
		{
			return false;
		}
		if (HasFpsWarningsMaxTime != other.HasFpsWarningsMaxTime || (HasFpsWarningsMaxTime && !FpsWarningsMaxTime.Equals(other.FpsWarningsMaxTime)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static FlowPerformance Deserialize(Stream stream, FlowPerformance instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static FlowPerformance DeserializeLengthDelimited(Stream stream)
	{
		FlowPerformance instance = new FlowPerformance();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static FlowPerformance DeserializeLengthDelimited(Stream stream, FlowPerformance instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static FlowPerformance Deserialize(Stream stream, FlowPerformance instance, long limit)
	{
		BinaryReader br = new BinaryReader(stream);
		instance.FlowType_ = FlowType.SHOP;
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
				instance.UniqueId = ProtocolParser.ReadString(stream);
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
				if (instance.Player == null)
				{
					instance.Player = Player.DeserializeLengthDelimited(stream);
				}
				else
				{
					Player.DeserializeLengthDelimited(stream, instance.Player);
				}
				continue;
			case 32:
				instance.FlowType_ = (FlowType)ProtocolParser.ReadUInt64(stream);
				continue;
			case 45:
				instance.AverageFps = br.ReadSingle();
				continue;
			case 53:
				instance.Duration = br.ReadSingle();
				continue;
			case 61:
				instance.FpsWarningsThreshold = br.ReadSingle();
				continue;
			case 64:
				instance.FpsWarningsTotalOccurences = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 77:
				instance.FpsWarningsTotalTime = br.ReadSingle();
				continue;
			case 85:
				instance.FpsWarningsAverageTime = br.ReadSingle();
				continue;
			case 93:
				instance.FpsWarningsMaxTime = br.ReadSingle();
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

	public static void Serialize(Stream stream, FlowPerformance instance)
	{
		BinaryWriter bw = new BinaryWriter(stream);
		if (instance.HasUniqueId)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.UniqueId));
		}
		if (instance.HasDeviceInfo)
		{
			stream.WriteByte(18);
			ProtocolParser.WriteUInt32(stream, instance.DeviceInfo.GetSerializedSize());
			DeviceInfo.Serialize(stream, instance.DeviceInfo);
		}
		if (instance.HasPlayer)
		{
			stream.WriteByte(26);
			ProtocolParser.WriteUInt32(stream, instance.Player.GetSerializedSize());
			Player.Serialize(stream, instance.Player);
		}
		if (instance.HasFlowType_)
		{
			stream.WriteByte(32);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.FlowType_);
		}
		if (instance.HasAverageFps)
		{
			stream.WriteByte(45);
			bw.Write(instance.AverageFps);
		}
		if (instance.HasDuration)
		{
			stream.WriteByte(53);
			bw.Write(instance.Duration);
		}
		if (instance.HasFpsWarningsThreshold)
		{
			stream.WriteByte(61);
			bw.Write(instance.FpsWarningsThreshold);
		}
		if (instance.HasFpsWarningsTotalOccurences)
		{
			stream.WriteByte(64);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.FpsWarningsTotalOccurences);
		}
		if (instance.HasFpsWarningsTotalTime)
		{
			stream.WriteByte(77);
			bw.Write(instance.FpsWarningsTotalTime);
		}
		if (instance.HasFpsWarningsAverageTime)
		{
			stream.WriteByte(85);
			bw.Write(instance.FpsWarningsAverageTime);
		}
		if (instance.HasFpsWarningsMaxTime)
		{
			stream.WriteByte(93);
			bw.Write(instance.FpsWarningsMaxTime);
		}
	}

	public uint GetSerializedSize()
	{
		uint size = 0u;
		if (HasUniqueId)
		{
			size++;
			uint byteCount1 = (uint)Encoding.UTF8.GetByteCount(UniqueId);
			size += ProtocolParser.SizeOfUInt32(byteCount1) + byteCount1;
		}
		if (HasDeviceInfo)
		{
			size++;
			uint size2 = DeviceInfo.GetSerializedSize();
			size += size2 + ProtocolParser.SizeOfUInt32(size2);
		}
		if (HasPlayer)
		{
			size++;
			uint size3 = Player.GetSerializedSize();
			size += size3 + ProtocolParser.SizeOfUInt32(size3);
		}
		if (HasFlowType_)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)FlowType_);
		}
		if (HasAverageFps)
		{
			size++;
			size += 4;
		}
		if (HasDuration)
		{
			size++;
			size += 4;
		}
		if (HasFpsWarningsThreshold)
		{
			size++;
			size += 4;
		}
		if (HasFpsWarningsTotalOccurences)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)FpsWarningsTotalOccurences);
		}
		if (HasFpsWarningsTotalTime)
		{
			size++;
			size += 4;
		}
		if (HasFpsWarningsAverageTime)
		{
			size++;
			size += 4;
		}
		if (HasFpsWarningsMaxTime)
		{
			size++;
			size += 4;
		}
		return size;
	}
}
