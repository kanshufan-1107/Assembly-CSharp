using System.IO;
using System.Text;

namespace Blizzard.Telemetry.WTCG.Client;

public class SeamlessReconnectStart : IProtoBuf
{
	public bool HasPlayer;

	private Player _Player;

	public bool HasDeviceInfo;

	private DeviceInfo _DeviceInfo;

	public bool HasDisconnectReason;

	private string _DisconnectReason;

	public bool HasSecSinceLastResume;

	private int _SecSinceLastResume;

	public bool HasSecSpentPaused;

	private int _SecSpentPaused;

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

	public string DisconnectReason
	{
		get
		{
			return _DisconnectReason;
		}
		set
		{
			_DisconnectReason = value;
			HasDisconnectReason = value != null;
		}
	}

	public int SecSinceLastResume
	{
		get
		{
			return _SecSinceLastResume;
		}
		set
		{
			_SecSinceLastResume = value;
			HasSecSinceLastResume = true;
		}
	}

	public int SecSpentPaused
	{
		get
		{
			return _SecSpentPaused;
		}
		set
		{
			_SecSpentPaused = value;
			HasSecSpentPaused = true;
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
		if (HasDisconnectReason)
		{
			hash ^= DisconnectReason.GetHashCode();
		}
		if (HasSecSinceLastResume)
		{
			hash ^= SecSinceLastResume.GetHashCode();
		}
		if (HasSecSpentPaused)
		{
			hash ^= SecSpentPaused.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is SeamlessReconnectStart other))
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
		if (HasDisconnectReason != other.HasDisconnectReason || (HasDisconnectReason && !DisconnectReason.Equals(other.DisconnectReason)))
		{
			return false;
		}
		if (HasSecSinceLastResume != other.HasSecSinceLastResume || (HasSecSinceLastResume && !SecSinceLastResume.Equals(other.SecSinceLastResume)))
		{
			return false;
		}
		if (HasSecSpentPaused != other.HasSecSpentPaused || (HasSecSpentPaused && !SecSpentPaused.Equals(other.SecSpentPaused)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static SeamlessReconnectStart Deserialize(Stream stream, SeamlessReconnectStart instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static SeamlessReconnectStart DeserializeLengthDelimited(Stream stream)
	{
		SeamlessReconnectStart instance = new SeamlessReconnectStart();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static SeamlessReconnectStart DeserializeLengthDelimited(Stream stream, SeamlessReconnectStart instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static SeamlessReconnectStart Deserialize(Stream stream, SeamlessReconnectStart instance, long limit)
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
				instance.DisconnectReason = ProtocolParser.ReadString(stream);
				continue;
			case 32:
				instance.SecSinceLastResume = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 40:
				instance.SecSpentPaused = (int)ProtocolParser.ReadUInt64(stream);
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

	public static void Serialize(Stream stream, SeamlessReconnectStart instance)
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
		if (instance.HasDisconnectReason)
		{
			stream.WriteByte(26);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.DisconnectReason));
		}
		if (instance.HasSecSinceLastResume)
		{
			stream.WriteByte(32);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.SecSinceLastResume);
		}
		if (instance.HasSecSpentPaused)
		{
			stream.WriteByte(40);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.SecSpentPaused);
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
		if (HasDisconnectReason)
		{
			size++;
			uint byteCount3 = (uint)Encoding.UTF8.GetByteCount(DisconnectReason);
			size += ProtocolParser.SizeOfUInt32(byteCount3) + byteCount3;
		}
		if (HasSecSinceLastResume)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)SecSinceLastResume);
		}
		if (HasSecSpentPaused)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)SecSpentPaused);
		}
		return size;
	}
}
