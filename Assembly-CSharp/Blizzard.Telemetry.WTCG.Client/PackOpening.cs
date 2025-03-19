using System.IO;

namespace Blizzard.Telemetry.WTCG.Client;

public class PackOpening : IProtoBuf
{
	public bool HasDeviceInfo;

	private DeviceInfo _DeviceInfo;

	public bool HasPlayer;

	private Player _Player;

	public bool HasTimeToRegisterPackOpening;

	private float _TimeToRegisterPackOpening;

	public bool HasTimeTillAnimationStart;

	private float _TimeTillAnimationStart;

	public bool HasPackTypeId;

	private int _PackTypeId;

	public bool HasPacksOpened;

	private int _PacksOpened;

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

	public float TimeToRegisterPackOpening
	{
		get
		{
			return _TimeToRegisterPackOpening;
		}
		set
		{
			_TimeToRegisterPackOpening = value;
			HasTimeToRegisterPackOpening = true;
		}
	}

	public float TimeTillAnimationStart
	{
		get
		{
			return _TimeTillAnimationStart;
		}
		set
		{
			_TimeTillAnimationStart = value;
			HasTimeTillAnimationStart = true;
		}
	}

	public int PackTypeId
	{
		get
		{
			return _PackTypeId;
		}
		set
		{
			_PackTypeId = value;
			HasPackTypeId = true;
		}
	}

	public int PacksOpened
	{
		get
		{
			return _PacksOpened;
		}
		set
		{
			_PacksOpened = value;
			HasPacksOpened = true;
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
		if (HasTimeToRegisterPackOpening)
		{
			hash ^= TimeToRegisterPackOpening.GetHashCode();
		}
		if (HasTimeTillAnimationStart)
		{
			hash ^= TimeTillAnimationStart.GetHashCode();
		}
		if (HasPackTypeId)
		{
			hash ^= PackTypeId.GetHashCode();
		}
		if (HasPacksOpened)
		{
			hash ^= PacksOpened.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is PackOpening other))
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
		if (HasTimeToRegisterPackOpening != other.HasTimeToRegisterPackOpening || (HasTimeToRegisterPackOpening && !TimeToRegisterPackOpening.Equals(other.TimeToRegisterPackOpening)))
		{
			return false;
		}
		if (HasTimeTillAnimationStart != other.HasTimeTillAnimationStart || (HasTimeTillAnimationStart && !TimeTillAnimationStart.Equals(other.TimeTillAnimationStart)))
		{
			return false;
		}
		if (HasPackTypeId != other.HasPackTypeId || (HasPackTypeId && !PackTypeId.Equals(other.PackTypeId)))
		{
			return false;
		}
		if (HasPacksOpened != other.HasPacksOpened || (HasPacksOpened && !PacksOpened.Equals(other.PacksOpened)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static PackOpening Deserialize(Stream stream, PackOpening instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static PackOpening DeserializeLengthDelimited(Stream stream)
	{
		PackOpening instance = new PackOpening();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static PackOpening DeserializeLengthDelimited(Stream stream, PackOpening instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static PackOpening Deserialize(Stream stream, PackOpening instance, long limit)
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
			case 29:
				instance.TimeToRegisterPackOpening = br.ReadSingle();
				continue;
			case 37:
				instance.TimeTillAnimationStart = br.ReadSingle();
				continue;
			case 40:
				instance.PackTypeId = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 48:
				instance.PacksOpened = (int)ProtocolParser.ReadUInt64(stream);
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

	public static void Serialize(Stream stream, PackOpening instance)
	{
		BinaryWriter bw = new BinaryWriter(stream);
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
		if (instance.HasTimeToRegisterPackOpening)
		{
			stream.WriteByte(29);
			bw.Write(instance.TimeToRegisterPackOpening);
		}
		if (instance.HasTimeTillAnimationStart)
		{
			stream.WriteByte(37);
			bw.Write(instance.TimeTillAnimationStart);
		}
		if (instance.HasPackTypeId)
		{
			stream.WriteByte(40);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.PackTypeId);
		}
		if (instance.HasPacksOpened)
		{
			stream.WriteByte(48);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.PacksOpened);
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
		if (HasTimeToRegisterPackOpening)
		{
			size++;
			size += 4;
		}
		if (HasTimeTillAnimationStart)
		{
			size++;
			size += 4;
		}
		if (HasPackTypeId)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)PackTypeId);
		}
		if (HasPacksOpened)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)PacksOpened);
		}
		return size;
	}
}
