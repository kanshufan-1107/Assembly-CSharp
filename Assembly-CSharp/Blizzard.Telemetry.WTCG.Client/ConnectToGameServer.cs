using System.IO;
using System.Text;

namespace Blizzard.Telemetry.WTCG.Client;

public class ConnectToGameServer : IProtoBuf
{
	public bool HasDeviceInfo;

	private DeviceInfo _DeviceInfo;

	public bool HasPlayer;

	private Player _Player;

	public bool HasResultBnetCode;

	private uint _ResultBnetCode;

	public bool HasResultBnetCodeString;

	private string _ResultBnetCodeString;

	public bool HasTimeSpentMilliseconds;

	private long _TimeSpentMilliseconds;

	public bool HasGameSessionInfo;

	private GameSessionInfo _GameSessionInfo;

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

	public uint ResultBnetCode
	{
		get
		{
			return _ResultBnetCode;
		}
		set
		{
			_ResultBnetCode = value;
			HasResultBnetCode = true;
		}
	}

	public string ResultBnetCodeString
	{
		get
		{
			return _ResultBnetCodeString;
		}
		set
		{
			_ResultBnetCodeString = value;
			HasResultBnetCodeString = value != null;
		}
	}

	public long TimeSpentMilliseconds
	{
		get
		{
			return _TimeSpentMilliseconds;
		}
		set
		{
			_TimeSpentMilliseconds = value;
			HasTimeSpentMilliseconds = true;
		}
	}

	public GameSessionInfo GameSessionInfo
	{
		get
		{
			return _GameSessionInfo;
		}
		set
		{
			_GameSessionInfo = value;
			HasGameSessionInfo = value != null;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasDeviceInfo)
		{
			hash ^= DeviceInfo.GetHashCode();
		}
		if (HasPlayer)
		{
			hash ^= Player.GetHashCode();
		}
		if (HasResultBnetCode)
		{
			hash ^= ResultBnetCode.GetHashCode();
		}
		if (HasResultBnetCodeString)
		{
			hash ^= ResultBnetCodeString.GetHashCode();
		}
		if (HasTimeSpentMilliseconds)
		{
			hash ^= TimeSpentMilliseconds.GetHashCode();
		}
		if (HasGameSessionInfo)
		{
			hash ^= GameSessionInfo.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is ConnectToGameServer other))
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
		if (HasResultBnetCode != other.HasResultBnetCode || (HasResultBnetCode && !ResultBnetCode.Equals(other.ResultBnetCode)))
		{
			return false;
		}
		if (HasResultBnetCodeString != other.HasResultBnetCodeString || (HasResultBnetCodeString && !ResultBnetCodeString.Equals(other.ResultBnetCodeString)))
		{
			return false;
		}
		if (HasTimeSpentMilliseconds != other.HasTimeSpentMilliseconds || (HasTimeSpentMilliseconds && !TimeSpentMilliseconds.Equals(other.TimeSpentMilliseconds)))
		{
			return false;
		}
		if (HasGameSessionInfo != other.HasGameSessionInfo || (HasGameSessionInfo && !GameSessionInfo.Equals(other.GameSessionInfo)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static ConnectToGameServer Deserialize(Stream stream, ConnectToGameServer instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static ConnectToGameServer DeserializeLengthDelimited(Stream stream)
	{
		ConnectToGameServer instance = new ConnectToGameServer();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static ConnectToGameServer DeserializeLengthDelimited(Stream stream, ConnectToGameServer instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static ConnectToGameServer Deserialize(Stream stream, ConnectToGameServer instance, long limit)
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
				if (instance.DeviceInfo == null)
				{
					instance.DeviceInfo = DeviceInfo.DeserializeLengthDelimited(stream);
				}
				else
				{
					DeviceInfo.DeserializeLengthDelimited(stream, instance.DeviceInfo);
				}
				continue;
			case 18:
				if (instance.Player == null)
				{
					instance.Player = Player.DeserializeLengthDelimited(stream);
				}
				else
				{
					Player.DeserializeLengthDelimited(stream, instance.Player);
				}
				continue;
			case 24:
				instance.ResultBnetCode = ProtocolParser.ReadUInt32(stream);
				continue;
			case 34:
				instance.ResultBnetCodeString = ProtocolParser.ReadString(stream);
				continue;
			case 40:
				instance.TimeSpentMilliseconds = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			case 50:
				if (instance.GameSessionInfo == null)
				{
					instance.GameSessionInfo = GameSessionInfo.DeserializeLengthDelimited(stream);
				}
				else
				{
					GameSessionInfo.DeserializeLengthDelimited(stream, instance.GameSessionInfo);
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

	public static void Serialize(Stream stream, ConnectToGameServer instance)
	{
		if (instance.HasDeviceInfo)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteUInt32(stream, instance.DeviceInfo.GetSerializedSize());
			DeviceInfo.Serialize(stream, instance.DeviceInfo);
		}
		if (instance.HasPlayer)
		{
			stream.WriteByte(18);
			ProtocolParser.WriteUInt32(stream, instance.Player.GetSerializedSize());
			Player.Serialize(stream, instance.Player);
		}
		if (instance.HasResultBnetCode)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt32(stream, instance.ResultBnetCode);
		}
		if (instance.HasResultBnetCodeString)
		{
			stream.WriteByte(34);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.ResultBnetCodeString));
		}
		if (instance.HasTimeSpentMilliseconds)
		{
			stream.WriteByte(40);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.TimeSpentMilliseconds);
		}
		if (instance.HasGameSessionInfo)
		{
			stream.WriteByte(50);
			ProtocolParser.WriteUInt32(stream, instance.GameSessionInfo.GetSerializedSize());
			GameSessionInfo.Serialize(stream, instance.GameSessionInfo);
		}
	}

	public uint GetSerializedSize()
	{
		uint size = 0u;
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
		if (HasResultBnetCode)
		{
			size++;
			size += ProtocolParser.SizeOfUInt32(ResultBnetCode);
		}
		if (HasResultBnetCodeString)
		{
			size++;
			uint byteCount4 = (uint)Encoding.UTF8.GetByteCount(ResultBnetCodeString);
			size += ProtocolParser.SizeOfUInt32(byteCount4) + byteCount4;
		}
		if (HasTimeSpentMilliseconds)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)TimeSpentMilliseconds);
		}
		if (HasGameSessionInfo)
		{
			size++;
			uint size6 = GameSessionInfo.GetSerializedSize();
			size += size6 + ProtocolParser.SizeOfUInt32(size6);
		}
		return size;
	}
}
