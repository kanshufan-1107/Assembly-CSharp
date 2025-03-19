using System.IO;
using System.Text;

namespace Blizzard.Telemetry.WTCG.Client;

public class BoardVisualStateChanged : IProtoBuf
{
	public bool HasFromBoardState;

	private string _FromBoardState;

	public bool HasToBoardState;

	private string _ToBoardState;

	public bool HasTimeInSeconds;

	private int _TimeInSeconds;

	public string FromBoardState
	{
		get
		{
			return _FromBoardState;
		}
		set
		{
			_FromBoardState = value;
			HasFromBoardState = value != null;
		}
	}

	public string ToBoardState
	{
		get
		{
			return _ToBoardState;
		}
		set
		{
			_ToBoardState = value;
			HasToBoardState = value != null;
		}
	}

	public int TimeInSeconds
	{
		get
		{
			return _TimeInSeconds;
		}
		set
		{
			_TimeInSeconds = value;
			HasTimeInSeconds = true;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasFromBoardState)
		{
			hash ^= FromBoardState.GetHashCode();
		}
		if (HasToBoardState)
		{
			hash ^= ToBoardState.GetHashCode();
		}
		if (HasTimeInSeconds)
		{
			hash ^= TimeInSeconds.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is BoardVisualStateChanged other))
		{
			return false;
		}
		if (HasFromBoardState != other.HasFromBoardState || (HasFromBoardState && !FromBoardState.Equals(other.FromBoardState)))
		{
			return false;
		}
		if (HasToBoardState != other.HasToBoardState || (HasToBoardState && !ToBoardState.Equals(other.ToBoardState)))
		{
			return false;
		}
		if (HasTimeInSeconds != other.HasTimeInSeconds || (HasTimeInSeconds && !TimeInSeconds.Equals(other.TimeInSeconds)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static BoardVisualStateChanged Deserialize(Stream stream, BoardVisualStateChanged instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static BoardVisualStateChanged DeserializeLengthDelimited(Stream stream)
	{
		BoardVisualStateChanged instance = new BoardVisualStateChanged();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static BoardVisualStateChanged DeserializeLengthDelimited(Stream stream, BoardVisualStateChanged instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static BoardVisualStateChanged Deserialize(Stream stream, BoardVisualStateChanged instance, long limit)
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
				instance.FromBoardState = ProtocolParser.ReadString(stream);
				continue;
			case 18:
				instance.ToBoardState = ProtocolParser.ReadString(stream);
				continue;
			case 24:
				instance.TimeInSeconds = (int)ProtocolParser.ReadUInt64(stream);
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

	public static void Serialize(Stream stream, BoardVisualStateChanged instance)
	{
		if (instance.HasFromBoardState)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.FromBoardState));
		}
		if (instance.HasToBoardState)
		{
			stream.WriteByte(18);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.ToBoardState));
		}
		if (instance.HasTimeInSeconds)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.TimeInSeconds);
		}
	}

	public uint GetSerializedSize()
	{
		uint size = 0u;
		if (HasFromBoardState)
		{
			size++;
			uint byteCount1 = (uint)Encoding.UTF8.GetByteCount(FromBoardState);
			size += ProtocolParser.SizeOfUInt32(byteCount1) + byteCount1;
		}
		if (HasToBoardState)
		{
			size++;
			uint byteCount2 = (uint)Encoding.UTF8.GetByteCount(ToBoardState);
			size += ProtocolParser.SizeOfUInt32(byteCount2) + byteCount2;
		}
		if (HasTimeInSeconds)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)TimeInSeconds);
		}
		return size;
	}
}
