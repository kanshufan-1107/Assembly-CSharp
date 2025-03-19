using System.IO;
using System.Text;

namespace Blizzard.Telemetry.WTCG.Client;

public class FlowPerformanceBattlegrounds : IProtoBuf
{
	public bool HasFlowId;

	private string _FlowId;

	public bool HasGameUuid;

	private string _GameUuid;

	public bool HasTotalRounds;

	private int _TotalRounds;

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

	public string GameUuid
	{
		get
		{
			return _GameUuid;
		}
		set
		{
			_GameUuid = value;
			HasGameUuid = value != null;
		}
	}

	public int TotalRounds
	{
		get
		{
			return _TotalRounds;
		}
		set
		{
			_TotalRounds = value;
			HasTotalRounds = true;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasFlowId)
		{
			hash ^= FlowId.GetHashCode();
		}
		if (HasGameUuid)
		{
			hash ^= GameUuid.GetHashCode();
		}
		if (HasTotalRounds)
		{
			hash ^= TotalRounds.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is FlowPerformanceBattlegrounds other))
		{
			return false;
		}
		if (HasFlowId != other.HasFlowId || (HasFlowId && !FlowId.Equals(other.FlowId)))
		{
			return false;
		}
		if (HasGameUuid != other.HasGameUuid || (HasGameUuid && !GameUuid.Equals(other.GameUuid)))
		{
			return false;
		}
		if (HasTotalRounds != other.HasTotalRounds || (HasTotalRounds && !TotalRounds.Equals(other.TotalRounds)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static FlowPerformanceBattlegrounds Deserialize(Stream stream, FlowPerformanceBattlegrounds instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static FlowPerformanceBattlegrounds DeserializeLengthDelimited(Stream stream)
	{
		FlowPerformanceBattlegrounds instance = new FlowPerformanceBattlegrounds();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static FlowPerformanceBattlegrounds DeserializeLengthDelimited(Stream stream, FlowPerformanceBattlegrounds instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static FlowPerformanceBattlegrounds Deserialize(Stream stream, FlowPerformanceBattlegrounds instance, long limit)
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
				instance.FlowId = ProtocolParser.ReadString(stream);
				continue;
			case 34:
				instance.GameUuid = ProtocolParser.ReadString(stream);
				continue;
			case 16:
				instance.TotalRounds = (int)ProtocolParser.ReadUInt64(stream);
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

	public static void Serialize(Stream stream, FlowPerformanceBattlegrounds instance)
	{
		if (instance.HasFlowId)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.FlowId));
		}
		if (instance.HasGameUuid)
		{
			stream.WriteByte(34);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.GameUuid));
		}
		if (instance.HasTotalRounds)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.TotalRounds);
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
		if (HasGameUuid)
		{
			size++;
			uint byteCount4 = (uint)Encoding.UTF8.GetByteCount(GameUuid);
			size += ProtocolParser.SizeOfUInt32(byteCount4) + byteCount4;
		}
		if (HasTotalRounds)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)TotalRounds);
		}
		return size;
	}
}
