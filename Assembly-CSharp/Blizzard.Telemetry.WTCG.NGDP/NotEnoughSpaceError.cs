using System.IO;
using System.Text;

namespace Blizzard.Telemetry.WTCG.NGDP;

public class NotEnoughSpaceError : IProtoBuf
{
	public bool HasAvailableSpace;

	private ulong _AvailableSpace;

	public bool HasExpectedOrgBytes;

	private ulong _ExpectedOrgBytes;

	public bool HasFilesDir;

	private string _FilesDir;

	public bool HasInitial;

	private bool _Initial;

	public bool HasLocaleSwitch;

	private bool _LocaleSwitch;

	public ulong AvailableSpace
	{
		get
		{
			return _AvailableSpace;
		}
		set
		{
			_AvailableSpace = value;
			HasAvailableSpace = true;
		}
	}

	public ulong ExpectedOrgBytes
	{
		get
		{
			return _ExpectedOrgBytes;
		}
		set
		{
			_ExpectedOrgBytes = value;
			HasExpectedOrgBytes = true;
		}
	}

	public string FilesDir
	{
		get
		{
			return _FilesDir;
		}
		set
		{
			_FilesDir = value;
			HasFilesDir = value != null;
		}
	}

	public bool Initial
	{
		get
		{
			return _Initial;
		}
		set
		{
			_Initial = value;
			HasInitial = true;
		}
	}

	public bool LocaleSwitch
	{
		get
		{
			return _LocaleSwitch;
		}
		set
		{
			_LocaleSwitch = value;
			HasLocaleSwitch = true;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasAvailableSpace)
		{
			hash ^= AvailableSpace.GetHashCode();
		}
		if (HasExpectedOrgBytes)
		{
			hash ^= ExpectedOrgBytes.GetHashCode();
		}
		if (HasFilesDir)
		{
			hash ^= FilesDir.GetHashCode();
		}
		if (HasInitial)
		{
			hash ^= Initial.GetHashCode();
		}
		if (HasLocaleSwitch)
		{
			hash ^= LocaleSwitch.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is NotEnoughSpaceError other))
		{
			return false;
		}
		if (HasAvailableSpace != other.HasAvailableSpace || (HasAvailableSpace && !AvailableSpace.Equals(other.AvailableSpace)))
		{
			return false;
		}
		if (HasExpectedOrgBytes != other.HasExpectedOrgBytes || (HasExpectedOrgBytes && !ExpectedOrgBytes.Equals(other.ExpectedOrgBytes)))
		{
			return false;
		}
		if (HasFilesDir != other.HasFilesDir || (HasFilesDir && !FilesDir.Equals(other.FilesDir)))
		{
			return false;
		}
		if (HasInitial != other.HasInitial || (HasInitial && !Initial.Equals(other.Initial)))
		{
			return false;
		}
		if (HasLocaleSwitch != other.HasLocaleSwitch || (HasLocaleSwitch && !LocaleSwitch.Equals(other.LocaleSwitch)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static NotEnoughSpaceError Deserialize(Stream stream, NotEnoughSpaceError instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static NotEnoughSpaceError DeserializeLengthDelimited(Stream stream)
	{
		NotEnoughSpaceError instance = new NotEnoughSpaceError();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static NotEnoughSpaceError DeserializeLengthDelimited(Stream stream, NotEnoughSpaceError instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static NotEnoughSpaceError Deserialize(Stream stream, NotEnoughSpaceError instance, long limit)
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
			case 8:
				instance.AvailableSpace = ProtocolParser.ReadUInt64(stream);
				continue;
			case 32:
				instance.ExpectedOrgBytes = ProtocolParser.ReadUInt64(stream);
				continue;
			case 42:
				instance.FilesDir = ProtocolParser.ReadString(stream);
				continue;
			case 48:
				instance.Initial = ProtocolParser.ReadBool(stream);
				continue;
			case 56:
				instance.LocaleSwitch = ProtocolParser.ReadBool(stream);
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

	public static void Serialize(Stream stream, NotEnoughSpaceError instance)
	{
		if (instance.HasAvailableSpace)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, instance.AvailableSpace);
		}
		if (instance.HasExpectedOrgBytes)
		{
			stream.WriteByte(32);
			ProtocolParser.WriteUInt64(stream, instance.ExpectedOrgBytes);
		}
		if (instance.HasFilesDir)
		{
			stream.WriteByte(42);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.FilesDir));
		}
		if (instance.HasInitial)
		{
			stream.WriteByte(48);
			ProtocolParser.WriteBool(stream, instance.Initial);
		}
		if (instance.HasLocaleSwitch)
		{
			stream.WriteByte(56);
			ProtocolParser.WriteBool(stream, instance.LocaleSwitch);
		}
	}

	public uint GetSerializedSize()
	{
		uint size = 0u;
		if (HasAvailableSpace)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64(AvailableSpace);
		}
		if (HasExpectedOrgBytes)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64(ExpectedOrgBytes);
		}
		if (HasFilesDir)
		{
			size++;
			uint byteCount5 = (uint)Encoding.UTF8.GetByteCount(FilesDir);
			size += ProtocolParser.SizeOfUInt32(byteCount5) + byteCount5;
		}
		if (HasInitial)
		{
			size++;
			size++;
		}
		if (HasLocaleSwitch)
		{
			size++;
			size++;
		}
		return size;
	}
}
