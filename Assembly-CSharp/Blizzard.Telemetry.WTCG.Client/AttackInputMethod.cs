using System.IO;

namespace Blizzard.Telemetry.WTCG.Client;

public class AttackInputMethod : IProtoBuf
{
	public bool HasDeviceInfo;

	private DeviceInfo _DeviceInfo;

	public bool HasTotalNumAttacks;

	private long _TotalNumAttacks;

	public bool HasTotalClickAttacks;

	private long _TotalClickAttacks;

	public bool HasPercentClickAttacks;

	private int _PercentClickAttacks;

	public bool HasTotalDragAttacks;

	private long _TotalDragAttacks;

	public bool HasPercentDragAttacks;

	private int _PercentDragAttacks;

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

	public long TotalNumAttacks
	{
		get
		{
			return _TotalNumAttacks;
		}
		set
		{
			_TotalNumAttacks = value;
			HasTotalNumAttacks = true;
		}
	}

	public long TotalClickAttacks
	{
		get
		{
			return _TotalClickAttacks;
		}
		set
		{
			_TotalClickAttacks = value;
			HasTotalClickAttacks = true;
		}
	}

	public int PercentClickAttacks
	{
		get
		{
			return _PercentClickAttacks;
		}
		set
		{
			_PercentClickAttacks = value;
			HasPercentClickAttacks = true;
		}
	}

	public long TotalDragAttacks
	{
		get
		{
			return _TotalDragAttacks;
		}
		set
		{
			_TotalDragAttacks = value;
			HasTotalDragAttacks = true;
		}
	}

	public int PercentDragAttacks
	{
		get
		{
			return _PercentDragAttacks;
		}
		set
		{
			_PercentDragAttacks = value;
			HasPercentDragAttacks = true;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasDeviceInfo)
		{
			hash ^= DeviceInfo.GetHashCode();
		}
		if (HasTotalNumAttacks)
		{
			hash ^= TotalNumAttacks.GetHashCode();
		}
		if (HasTotalClickAttacks)
		{
			hash ^= TotalClickAttacks.GetHashCode();
		}
		if (HasPercentClickAttacks)
		{
			hash ^= PercentClickAttacks.GetHashCode();
		}
		if (HasTotalDragAttacks)
		{
			hash ^= TotalDragAttacks.GetHashCode();
		}
		if (HasPercentDragAttacks)
		{
			hash ^= PercentDragAttacks.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is AttackInputMethod other))
		{
			return false;
		}
		if (HasDeviceInfo != other.HasDeviceInfo || (HasDeviceInfo && !DeviceInfo.Equals(other.DeviceInfo)))
		{
			return false;
		}
		if (HasTotalNumAttacks != other.HasTotalNumAttacks || (HasTotalNumAttacks && !TotalNumAttacks.Equals(other.TotalNumAttacks)))
		{
			return false;
		}
		if (HasTotalClickAttacks != other.HasTotalClickAttacks || (HasTotalClickAttacks && !TotalClickAttacks.Equals(other.TotalClickAttacks)))
		{
			return false;
		}
		if (HasPercentClickAttacks != other.HasPercentClickAttacks || (HasPercentClickAttacks && !PercentClickAttacks.Equals(other.PercentClickAttacks)))
		{
			return false;
		}
		if (HasTotalDragAttacks != other.HasTotalDragAttacks || (HasTotalDragAttacks && !TotalDragAttacks.Equals(other.TotalDragAttacks)))
		{
			return false;
		}
		if (HasPercentDragAttacks != other.HasPercentDragAttacks || (HasPercentDragAttacks && !PercentDragAttacks.Equals(other.PercentDragAttacks)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static AttackInputMethod Deserialize(Stream stream, AttackInputMethod instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static AttackInputMethod DeserializeLengthDelimited(Stream stream)
	{
		AttackInputMethod instance = new AttackInputMethod();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static AttackInputMethod DeserializeLengthDelimited(Stream stream, AttackInputMethod instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static AttackInputMethod Deserialize(Stream stream, AttackInputMethod instance, long limit)
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
			case 16:
				instance.TotalNumAttacks = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			case 24:
				instance.TotalClickAttacks = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			case 32:
				instance.PercentClickAttacks = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 40:
				instance.TotalDragAttacks = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			case 48:
				instance.PercentDragAttacks = (int)ProtocolParser.ReadUInt64(stream);
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

	public static void Serialize(Stream stream, AttackInputMethod instance)
	{
		if (instance.HasDeviceInfo)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteUInt32(stream, instance.DeviceInfo.GetSerializedSize());
			DeviceInfo.Serialize(stream, instance.DeviceInfo);
		}
		if (instance.HasTotalNumAttacks)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.TotalNumAttacks);
		}
		if (instance.HasTotalClickAttacks)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.TotalClickAttacks);
		}
		if (instance.HasPercentClickAttacks)
		{
			stream.WriteByte(32);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.PercentClickAttacks);
		}
		if (instance.HasTotalDragAttacks)
		{
			stream.WriteByte(40);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.TotalDragAttacks);
		}
		if (instance.HasPercentDragAttacks)
		{
			stream.WriteByte(48);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.PercentDragAttacks);
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
		if (HasTotalNumAttacks)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)TotalNumAttacks);
		}
		if (HasTotalClickAttacks)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)TotalClickAttacks);
		}
		if (HasPercentClickAttacks)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)PercentClickAttacks);
		}
		if (HasTotalDragAttacks)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)TotalDragAttacks);
		}
		if (HasPercentDragAttacks)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)PercentDragAttacks);
		}
		return size;
	}
}
