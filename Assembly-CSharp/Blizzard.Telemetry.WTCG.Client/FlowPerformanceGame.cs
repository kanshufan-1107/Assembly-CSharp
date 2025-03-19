using System.IO;
using System.Text;

namespace Blizzard.Telemetry.WTCG.Client;

public class FlowPerformanceGame : IProtoBuf
{
	public bool HasFlowId;

	private string _FlowId;

	public bool HasDeviceInfo;

	private DeviceInfo _DeviceInfo;

	public bool HasPlayer;

	private Player _Player;

	public bool HasUuid;

	private string _Uuid;

	public bool HasGameType;

	private GameType _GameType;

	public bool HasFormatType;

	private FormatType _FormatType;

	public bool HasBoardId;

	private int _BoardId;

	public bool HasScenarioId;

	private int _ScenarioId;

	public string FlowId
	{
		get
		{
			return _FlowId;
		}
		set
		{
			_FlowId = value;
			HasFlowId = value != null;
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

	public string Uuid
	{
		get
		{
			return _Uuid;
		}
		set
		{
			_Uuid = value;
			HasUuid = value != null;
		}
	}

	public GameType GameType
	{
		get
		{
			return _GameType;
		}
		set
		{
			_GameType = value;
			HasGameType = true;
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

	public int BoardId
	{
		get
		{
			return _BoardId;
		}
		set
		{
			_BoardId = value;
			HasBoardId = true;
		}
	}

	public int ScenarioId
	{
		get
		{
			return _ScenarioId;
		}
		set
		{
			_ScenarioId = value;
			HasScenarioId = true;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasFlowId)
		{
			hash ^= FlowId.GetHashCode();
		}
		if (HasDeviceInfo)
		{
			hash ^= DeviceInfo.GetHashCode();
		}
		if (HasPlayer)
		{
			hash ^= Player.GetHashCode();
		}
		if (HasUuid)
		{
			hash ^= Uuid.GetHashCode();
		}
		if (HasGameType)
		{
			hash ^= GameType.GetHashCode();
		}
		if (HasFormatType)
		{
			hash ^= FormatType.GetHashCode();
		}
		if (HasBoardId)
		{
			hash ^= BoardId.GetHashCode();
		}
		if (HasScenarioId)
		{
			hash ^= ScenarioId.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is FlowPerformanceGame other))
		{
			return false;
		}
		if (HasFlowId != other.HasFlowId || (HasFlowId && !FlowId.Equals(other.FlowId)))
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
		if (HasUuid != other.HasUuid || (HasUuid && !Uuid.Equals(other.Uuid)))
		{
			return false;
		}
		if (HasGameType != other.HasGameType || (HasGameType && !GameType.Equals(other.GameType)))
		{
			return false;
		}
		if (HasFormatType != other.HasFormatType || (HasFormatType && !FormatType.Equals(other.FormatType)))
		{
			return false;
		}
		if (HasBoardId != other.HasBoardId || (HasBoardId && !BoardId.Equals(other.BoardId)))
		{
			return false;
		}
		if (HasScenarioId != other.HasScenarioId || (HasScenarioId && !ScenarioId.Equals(other.ScenarioId)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static FlowPerformanceGame Deserialize(Stream stream, FlowPerformanceGame instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static FlowPerformanceGame DeserializeLengthDelimited(Stream stream)
	{
		FlowPerformanceGame instance = new FlowPerformanceGame();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static FlowPerformanceGame DeserializeLengthDelimited(Stream stream, FlowPerformanceGame instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static FlowPerformanceGame Deserialize(Stream stream, FlowPerformanceGame instance, long limit)
	{
		instance.GameType = GameType.GT_UNKNOWN;
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
				instance.FlowId = ProtocolParser.ReadString(stream);
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
			case 34:
				instance.Uuid = ProtocolParser.ReadString(stream);
				continue;
			case 40:
				instance.GameType = (GameType)ProtocolParser.ReadUInt64(stream);
				continue;
			case 48:
				instance.FormatType = (FormatType)ProtocolParser.ReadUInt64(stream);
				continue;
			case 56:
				instance.BoardId = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 64:
				instance.ScenarioId = (int)ProtocolParser.ReadUInt64(stream);
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

	public static void Serialize(Stream stream, FlowPerformanceGame instance)
	{
		if (instance.HasFlowId)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.FlowId));
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
		if (instance.HasUuid)
		{
			stream.WriteByte(34);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.Uuid));
		}
		if (instance.HasGameType)
		{
			stream.WriteByte(40);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.GameType);
		}
		if (instance.HasFormatType)
		{
			stream.WriteByte(48);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.FormatType);
		}
		if (instance.HasBoardId)
		{
			stream.WriteByte(56);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.BoardId);
		}
		if (instance.HasScenarioId)
		{
			stream.WriteByte(64);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.ScenarioId);
		}
	}

	public uint GetSerializedSize()
	{
		uint size = 0u;
		if (HasFlowId)
		{
			size++;
			uint byteCount1 = (uint)Encoding.UTF8.GetByteCount(FlowId);
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
		if (HasUuid)
		{
			size++;
			uint byteCount4 = (uint)Encoding.UTF8.GetByteCount(Uuid);
			size += ProtocolParser.SizeOfUInt32(byteCount4) + byteCount4;
		}
		if (HasGameType)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)GameType);
		}
		if (HasFormatType)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)FormatType);
		}
		if (HasBoardId)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)BoardId);
		}
		if (HasScenarioId)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)ScenarioId);
		}
		return size;
	}
}
