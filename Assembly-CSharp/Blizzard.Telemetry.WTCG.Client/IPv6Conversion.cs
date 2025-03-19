using System.IO;
using System.Text;

namespace Blizzard.Telemetry.WTCG.Client;

public class IPv6Conversion : IProtoBuf
{
	public bool HasDeviceInfo;

	private DeviceInfo _DeviceInfo;

	public bool HasPlayer;

	private Player _Player;

	public bool HasIpv6;

	private string _Ipv6;

	public bool HasIpv4;

	private string _Ipv4;

	public bool HasConnectSuccess;

	private bool _ConnectSuccess;

	public bool HasOnCellular;

	private bool _OnCellular;

	public bool HasAvailableIpv6;

	private bool _AvailableIpv6;

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

	public string Ipv6
	{
		get
		{
			return _Ipv6;
		}
		set
		{
			_Ipv6 = value;
			HasIpv6 = value != null;
		}
	}

	public string Ipv4
	{
		get
		{
			return _Ipv4;
		}
		set
		{
			_Ipv4 = value;
			HasIpv4 = value != null;
		}
	}

	public bool ConnectSuccess
	{
		get
		{
			return _ConnectSuccess;
		}
		set
		{
			_ConnectSuccess = value;
			HasConnectSuccess = true;
		}
	}

	public bool OnCellular
	{
		get
		{
			return _OnCellular;
		}
		set
		{
			_OnCellular = value;
			HasOnCellular = true;
		}
	}

	public bool AvailableIpv6
	{
		get
		{
			return _AvailableIpv6;
		}
		set
		{
			_AvailableIpv6 = value;
			HasAvailableIpv6 = true;
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
		if (HasIpv6)
		{
			hash ^= Ipv6.GetHashCode();
		}
		if (HasIpv4)
		{
			hash ^= Ipv4.GetHashCode();
		}
		if (HasConnectSuccess)
		{
			hash ^= ConnectSuccess.GetHashCode();
		}
		if (HasOnCellular)
		{
			hash ^= OnCellular.GetHashCode();
		}
		if (HasAvailableIpv6)
		{
			hash ^= AvailableIpv6.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is IPv6Conversion other))
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
		if (HasIpv6 != other.HasIpv6 || (HasIpv6 && !Ipv6.Equals(other.Ipv6)))
		{
			return false;
		}
		if (HasIpv4 != other.HasIpv4 || (HasIpv4 && !Ipv4.Equals(other.Ipv4)))
		{
			return false;
		}
		if (HasConnectSuccess != other.HasConnectSuccess || (HasConnectSuccess && !ConnectSuccess.Equals(other.ConnectSuccess)))
		{
			return false;
		}
		if (HasOnCellular != other.HasOnCellular || (HasOnCellular && !OnCellular.Equals(other.OnCellular)))
		{
			return false;
		}
		if (HasAvailableIpv6 != other.HasAvailableIpv6 || (HasAvailableIpv6 && !AvailableIpv6.Equals(other.AvailableIpv6)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static IPv6Conversion Deserialize(Stream stream, IPv6Conversion instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static IPv6Conversion DeserializeLengthDelimited(Stream stream)
	{
		IPv6Conversion instance = new IPv6Conversion();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static IPv6Conversion DeserializeLengthDelimited(Stream stream, IPv6Conversion instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static IPv6Conversion Deserialize(Stream stream, IPv6Conversion instance, long limit)
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
			case 26:
				instance.Ipv6 = ProtocolParser.ReadString(stream);
				continue;
			case 34:
				instance.Ipv4 = ProtocolParser.ReadString(stream);
				continue;
			case 40:
				instance.ConnectSuccess = ProtocolParser.ReadBool(stream);
				continue;
			case 48:
				instance.OnCellular = ProtocolParser.ReadBool(stream);
				continue;
			case 56:
				instance.AvailableIpv6 = ProtocolParser.ReadBool(stream);
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

	public static void Serialize(Stream stream, IPv6Conversion instance)
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
		if (instance.HasIpv6)
		{
			stream.WriteByte(26);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.Ipv6));
		}
		if (instance.HasIpv4)
		{
			stream.WriteByte(34);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.Ipv4));
		}
		if (instance.HasConnectSuccess)
		{
			stream.WriteByte(40);
			ProtocolParser.WriteBool(stream, instance.ConnectSuccess);
		}
		if (instance.HasOnCellular)
		{
			stream.WriteByte(48);
			ProtocolParser.WriteBool(stream, instance.OnCellular);
		}
		if (instance.HasAvailableIpv6)
		{
			stream.WriteByte(56);
			ProtocolParser.WriteBool(stream, instance.AvailableIpv6);
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
		if (HasIpv6)
		{
			size++;
			uint byteCount3 = (uint)Encoding.UTF8.GetByteCount(Ipv6);
			size += ProtocolParser.SizeOfUInt32(byteCount3) + byteCount3;
		}
		if (HasIpv4)
		{
			size++;
			uint byteCount4 = (uint)Encoding.UTF8.GetByteCount(Ipv4);
			size += ProtocolParser.SizeOfUInt32(byteCount4) + byteCount4;
		}
		if (HasConnectSuccess)
		{
			size++;
			size++;
		}
		if (HasOnCellular)
		{
			size++;
			size++;
		}
		if (HasAvailableIpv6)
		{
			size++;
			size++;
		}
		return size;
	}
}
