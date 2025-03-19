using System.IO;
using System.Text;

namespace Blizzard.Telemetry.WTCG.Client;

public class GameSessionInfo : IProtoBuf
{
	public bool HasGameServerIpAddress;

	private string _GameServerIpAddress;

	public bool HasGameServerPort;

	private uint _GameServerPort;

	public bool HasVersion;

	private string _Version;

	public bool HasGameHandle;

	private uint _GameHandle;

	public bool HasScenarioId;

	private int _ScenarioId;

	public bool HasBrawlLibraryItemId;

	private int _BrawlLibraryItemId;

	public bool HasSeasonId;

	private int _SeasonId;

	public bool HasGameType;

	private GameType _GameType;

	public bool HasFormatType;

	private FormatType _FormatType;

	public bool HasIsReconnect;

	private bool _IsReconnect;

	public bool HasIsSpectating;

	private bool _IsSpectating;

	public bool HasClientHandle;

	private long _ClientHandle;

	public bool HasClientDeckId;

	private long _ClientDeckId;

	public bool HasAiDeckId;

	private long _AiDeckId;

	public bool HasClientHeroCardId;

	private long _ClientHeroCardId;

	public string GameServerIpAddress
	{
		get
		{
			return _GameServerIpAddress;
		}
		set
		{
			_GameServerIpAddress = value;
			HasGameServerIpAddress = value != null;
		}
	}

	public uint GameServerPort
	{
		get
		{
			return _GameServerPort;
		}
		set
		{
			_GameServerPort = value;
			HasGameServerPort = true;
		}
	}

	public string Version
	{
		get
		{
			return _Version;
		}
		set
		{
			_Version = value;
			HasVersion = value != null;
		}
	}

	public uint GameHandle
	{
		get
		{
			return _GameHandle;
		}
		set
		{
			_GameHandle = value;
			HasGameHandle = true;
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

	public int BrawlLibraryItemId
	{
		get
		{
			return _BrawlLibraryItemId;
		}
		set
		{
			_BrawlLibraryItemId = value;
			HasBrawlLibraryItemId = true;
		}
	}

	public int SeasonId
	{
		get
		{
			return _SeasonId;
		}
		set
		{
			_SeasonId = value;
			HasSeasonId = true;
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

	public bool IsReconnect
	{
		get
		{
			return _IsReconnect;
		}
		set
		{
			_IsReconnect = value;
			HasIsReconnect = true;
		}
	}

	public bool IsSpectating
	{
		get
		{
			return _IsSpectating;
		}
		set
		{
			_IsSpectating = value;
			HasIsSpectating = true;
		}
	}

	public long ClientHandle
	{
		get
		{
			return _ClientHandle;
		}
		set
		{
			_ClientHandle = value;
			HasClientHandle = true;
		}
	}

	public long ClientDeckId
	{
		get
		{
			return _ClientDeckId;
		}
		set
		{
			_ClientDeckId = value;
			HasClientDeckId = true;
		}
	}

	public long AiDeckId
	{
		get
		{
			return _AiDeckId;
		}
		set
		{
			_AiDeckId = value;
			HasAiDeckId = true;
		}
	}

	public long ClientHeroCardId
	{
		get
		{
			return _ClientHeroCardId;
		}
		set
		{
			_ClientHeroCardId = value;
			HasClientHeroCardId = true;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasGameServerIpAddress)
		{
			hash ^= GameServerIpAddress.GetHashCode();
		}
		if (HasGameServerPort)
		{
			hash ^= GameServerPort.GetHashCode();
		}
		if (HasVersion)
		{
			hash ^= Version.GetHashCode();
		}
		if (HasGameHandle)
		{
			hash ^= GameHandle.GetHashCode();
		}
		if (HasScenarioId)
		{
			hash ^= ScenarioId.GetHashCode();
		}
		if (HasBrawlLibraryItemId)
		{
			hash ^= BrawlLibraryItemId.GetHashCode();
		}
		if (HasSeasonId)
		{
			hash ^= SeasonId.GetHashCode();
		}
		if (HasGameType)
		{
			hash ^= GameType.GetHashCode();
		}
		if (HasFormatType)
		{
			hash ^= FormatType.GetHashCode();
		}
		if (HasIsReconnect)
		{
			hash ^= IsReconnect.GetHashCode();
		}
		if (HasIsSpectating)
		{
			hash ^= IsSpectating.GetHashCode();
		}
		if (HasClientHandle)
		{
			hash ^= ClientHandle.GetHashCode();
		}
		if (HasClientDeckId)
		{
			hash ^= ClientDeckId.GetHashCode();
		}
		if (HasAiDeckId)
		{
			hash ^= AiDeckId.GetHashCode();
		}
		if (HasClientHeroCardId)
		{
			hash ^= ClientHeroCardId.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is GameSessionInfo other))
		{
			return false;
		}
		if (HasGameServerIpAddress != other.HasGameServerIpAddress || (HasGameServerIpAddress && !GameServerIpAddress.Equals(other.GameServerIpAddress)))
		{
			return false;
		}
		if (HasGameServerPort != other.HasGameServerPort || (HasGameServerPort && !GameServerPort.Equals(other.GameServerPort)))
		{
			return false;
		}
		if (HasVersion != other.HasVersion || (HasVersion && !Version.Equals(other.Version)))
		{
			return false;
		}
		if (HasGameHandle != other.HasGameHandle || (HasGameHandle && !GameHandle.Equals(other.GameHandle)))
		{
			return false;
		}
		if (HasScenarioId != other.HasScenarioId || (HasScenarioId && !ScenarioId.Equals(other.ScenarioId)))
		{
			return false;
		}
		if (HasBrawlLibraryItemId != other.HasBrawlLibraryItemId || (HasBrawlLibraryItemId && !BrawlLibraryItemId.Equals(other.BrawlLibraryItemId)))
		{
			return false;
		}
		if (HasSeasonId != other.HasSeasonId || (HasSeasonId && !SeasonId.Equals(other.SeasonId)))
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
		if (HasIsReconnect != other.HasIsReconnect || (HasIsReconnect && !IsReconnect.Equals(other.IsReconnect)))
		{
			return false;
		}
		if (HasIsSpectating != other.HasIsSpectating || (HasIsSpectating && !IsSpectating.Equals(other.IsSpectating)))
		{
			return false;
		}
		if (HasClientHandle != other.HasClientHandle || (HasClientHandle && !ClientHandle.Equals(other.ClientHandle)))
		{
			return false;
		}
		if (HasClientDeckId != other.HasClientDeckId || (HasClientDeckId && !ClientDeckId.Equals(other.ClientDeckId)))
		{
			return false;
		}
		if (HasAiDeckId != other.HasAiDeckId || (HasAiDeckId && !AiDeckId.Equals(other.AiDeckId)))
		{
			return false;
		}
		if (HasClientHeroCardId != other.HasClientHeroCardId || (HasClientHeroCardId && !ClientHeroCardId.Equals(other.ClientHeroCardId)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static GameSessionInfo Deserialize(Stream stream, GameSessionInfo instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static GameSessionInfo DeserializeLengthDelimited(Stream stream)
	{
		GameSessionInfo instance = new GameSessionInfo();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static GameSessionInfo DeserializeLengthDelimited(Stream stream, GameSessionInfo instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static GameSessionInfo Deserialize(Stream stream, GameSessionInfo instance, long limit)
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
				instance.GameServerIpAddress = ProtocolParser.ReadString(stream);
				continue;
			case 16:
				instance.GameServerPort = ProtocolParser.ReadUInt32(stream);
				continue;
			case 26:
				instance.Version = ProtocolParser.ReadString(stream);
				continue;
			case 32:
				instance.GameHandle = ProtocolParser.ReadUInt32(stream);
				continue;
			case 40:
				instance.ScenarioId = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 48:
				instance.BrawlLibraryItemId = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 56:
				instance.SeasonId = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 64:
				instance.GameType = (GameType)ProtocolParser.ReadUInt64(stream);
				continue;
			case 72:
				instance.FormatType = (FormatType)ProtocolParser.ReadUInt64(stream);
				continue;
			case 80:
				instance.IsReconnect = ProtocolParser.ReadBool(stream);
				continue;
			case 88:
				instance.IsSpectating = ProtocolParser.ReadBool(stream);
				continue;
			case 96:
				instance.ClientHandle = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			case 104:
				instance.ClientDeckId = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			case 112:
				instance.AiDeckId = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			case 120:
				instance.ClientHeroCardId = (long)ProtocolParser.ReadUInt64(stream);
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

	public static void Serialize(Stream stream, GameSessionInfo instance)
	{
		if (instance.HasGameServerIpAddress)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.GameServerIpAddress));
		}
		if (instance.HasGameServerPort)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt32(stream, instance.GameServerPort);
		}
		if (instance.HasVersion)
		{
			stream.WriteByte(26);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.Version));
		}
		if (instance.HasGameHandle)
		{
			stream.WriteByte(32);
			ProtocolParser.WriteUInt32(stream, instance.GameHandle);
		}
		if (instance.HasScenarioId)
		{
			stream.WriteByte(40);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.ScenarioId);
		}
		if (instance.HasBrawlLibraryItemId)
		{
			stream.WriteByte(48);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.BrawlLibraryItemId);
		}
		if (instance.HasSeasonId)
		{
			stream.WriteByte(56);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.SeasonId);
		}
		if (instance.HasGameType)
		{
			stream.WriteByte(64);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.GameType);
		}
		if (instance.HasFormatType)
		{
			stream.WriteByte(72);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.FormatType);
		}
		if (instance.HasIsReconnect)
		{
			stream.WriteByte(80);
			ProtocolParser.WriteBool(stream, instance.IsReconnect);
		}
		if (instance.HasIsSpectating)
		{
			stream.WriteByte(88);
			ProtocolParser.WriteBool(stream, instance.IsSpectating);
		}
		if (instance.HasClientHandle)
		{
			stream.WriteByte(96);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.ClientHandle);
		}
		if (instance.HasClientDeckId)
		{
			stream.WriteByte(104);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.ClientDeckId);
		}
		if (instance.HasAiDeckId)
		{
			stream.WriteByte(112);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.AiDeckId);
		}
		if (instance.HasClientHeroCardId)
		{
			stream.WriteByte(120);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.ClientHeroCardId);
		}
	}

	public uint GetSerializedSize()
	{
		uint size = 0u;
		if (HasGameServerIpAddress)
		{
			size++;
			uint byteCount1 = (uint)Encoding.UTF8.GetByteCount(GameServerIpAddress);
			size += ProtocolParser.SizeOfUInt32(byteCount1) + byteCount1;
		}
		if (HasGameServerPort)
		{
			size++;
			size += ProtocolParser.SizeOfUInt32(GameServerPort);
		}
		if (HasVersion)
		{
			size++;
			uint byteCount3 = (uint)Encoding.UTF8.GetByteCount(Version);
			size += ProtocolParser.SizeOfUInt32(byteCount3) + byteCount3;
		}
		if (HasGameHandle)
		{
			size++;
			size += ProtocolParser.SizeOfUInt32(GameHandle);
		}
		if (HasScenarioId)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)ScenarioId);
		}
		if (HasBrawlLibraryItemId)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)BrawlLibraryItemId);
		}
		if (HasSeasonId)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)SeasonId);
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
		if (HasIsReconnect)
		{
			size++;
			size++;
		}
		if (HasIsSpectating)
		{
			size++;
			size++;
		}
		if (HasClientHandle)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)ClientHandle);
		}
		if (HasClientDeckId)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)ClientDeckId);
		}
		if (HasAiDeckId)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)AiDeckId);
		}
		if (HasClientHeroCardId)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)ClientHeroCardId);
		}
		return size;
	}
}
