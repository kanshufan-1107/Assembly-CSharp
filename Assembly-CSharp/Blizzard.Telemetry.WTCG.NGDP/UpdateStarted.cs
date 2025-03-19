using System.IO;
using System.Text;

namespace Blizzard.Telemetry.WTCG.NGDP;

public class UpdateStarted : IProtoBuf
{
	public bool HasInstalledVersion;

	private string _InstalledVersion;

	public bool HasTextureFormat;

	private string _TextureFormat;

	public bool HasDataPath;

	private string _DataPath;

	public bool HasAvailableSpaceMB;

	private float _AvailableSpaceMB;

	public string InstalledVersion
	{
		get
		{
			return _InstalledVersion;
		}
		set
		{
			_InstalledVersion = value;
			HasInstalledVersion = value != null;
		}
	}

	public string TextureFormat
	{
		get
		{
			return _TextureFormat;
		}
		set
		{
			_TextureFormat = value;
			HasTextureFormat = value != null;
		}
	}

	public string DataPath
	{
		get
		{
			return _DataPath;
		}
		set
		{
			_DataPath = value;
			HasDataPath = value != null;
		}
	}

	public float AvailableSpaceMB
	{
		get
		{
			return _AvailableSpaceMB;
		}
		set
		{
			_AvailableSpaceMB = value;
			HasAvailableSpaceMB = true;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasInstalledVersion)
		{
			hash ^= InstalledVersion.GetHashCode();
		}
		if (HasTextureFormat)
		{
			hash ^= TextureFormat.GetHashCode();
		}
		if (HasDataPath)
		{
			hash ^= DataPath.GetHashCode();
		}
		if (HasAvailableSpaceMB)
		{
			hash ^= AvailableSpaceMB.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is UpdateStarted other))
		{
			return false;
		}
		if (HasInstalledVersion != other.HasInstalledVersion || (HasInstalledVersion && !InstalledVersion.Equals(other.InstalledVersion)))
		{
			return false;
		}
		if (HasTextureFormat != other.HasTextureFormat || (HasTextureFormat && !TextureFormat.Equals(other.TextureFormat)))
		{
			return false;
		}
		if (HasDataPath != other.HasDataPath || (HasDataPath && !DataPath.Equals(other.DataPath)))
		{
			return false;
		}
		if (HasAvailableSpaceMB != other.HasAvailableSpaceMB || (HasAvailableSpaceMB && !AvailableSpaceMB.Equals(other.AvailableSpaceMB)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static UpdateStarted Deserialize(Stream stream, UpdateStarted instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static UpdateStarted DeserializeLengthDelimited(Stream stream)
	{
		UpdateStarted instance = new UpdateStarted();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static UpdateStarted DeserializeLengthDelimited(Stream stream, UpdateStarted instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static UpdateStarted Deserialize(Stream stream, UpdateStarted instance, long limit)
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
				instance.InstalledVersion = ProtocolParser.ReadString(stream);
				continue;
			case 18:
				instance.TextureFormat = ProtocolParser.ReadString(stream);
				continue;
			case 26:
				instance.DataPath = ProtocolParser.ReadString(stream);
				continue;
			case 37:
				instance.AvailableSpaceMB = br.ReadSingle();
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

	public static void Serialize(Stream stream, UpdateStarted instance)
	{
		BinaryWriter bw = new BinaryWriter(stream);
		if (instance.HasInstalledVersion)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.InstalledVersion));
		}
		if (instance.HasTextureFormat)
		{
			stream.WriteByte(18);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.TextureFormat));
		}
		if (instance.HasDataPath)
		{
			stream.WriteByte(26);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.DataPath));
		}
		if (instance.HasAvailableSpaceMB)
		{
			stream.WriteByte(37);
			bw.Write(instance.AvailableSpaceMB);
		}
	}

	public uint GetSerializedSize()
	{
		uint size = 0u;
		if (HasInstalledVersion)
		{
			size++;
			uint byteCount1 = (uint)Encoding.UTF8.GetByteCount(InstalledVersion);
			size += ProtocolParser.SizeOfUInt32(byteCount1) + byteCount1;
		}
		if (HasTextureFormat)
		{
			size++;
			uint byteCount2 = (uint)Encoding.UTF8.GetByteCount(TextureFormat);
			size += ProtocolParser.SizeOfUInt32(byteCount2) + byteCount2;
		}
		if (HasDataPath)
		{
			size++;
			uint byteCount3 = (uint)Encoding.UTF8.GetByteCount(DataPath);
			size += ProtocolParser.SizeOfUInt32(byteCount3) + byteCount3;
		}
		if (HasAvailableSpaceMB)
		{
			size++;
			size += 4;
		}
		return size;
	}
}
