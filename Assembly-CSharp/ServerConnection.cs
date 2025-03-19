using System;
using System.Net;
using System.Net.Sockets;
using Blizzard.GameService.SDK.Client.Integration;
using UnityEngine;

public class ServerConnection<PacketType> where PacketType : PacketFormat, new()
{
	private ISocket m_socket;

	private int m_port;

	private ClientConnection<PacketType> m_currentConnection;

	private bool m_listening;

	private object m_lock = new object();

	~ServerConnection()
	{
		Disconnect();
	}

	public bool Open(int port)
	{
		if (m_socket != null)
		{
			return false;
		}
		IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, port);
		try
		{
			m_socket = new SocketAdaptor(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp, canConnect: true);
			m_socket.Bind(endPoint);
			m_socket.Listen(16);
		}
		catch (Exception ex)
		{
			Debug.LogWarning("SeverConnection: error opening inbound connection: " + ex.Message + " (this probably occurred because you have multiple game instances running)");
			m_socket = null;
			return false;
		}
		return Listen();
	}

	public void Disconnect()
	{
		if (m_socket != null && m_socket.Connected)
		{
			m_socket.Shutdown(SocketShutdown.Both);
			m_socket.Close();
		}
	}

	public bool Listen()
	{
		lock (m_lock)
		{
			if (m_listening)
			{
				return true;
			}
			m_listening = true;
		}
		if (m_socket == null)
		{
			return false;
		}
		try
		{
			m_socket.BeginAccept(OnAccept, this);
		}
		catch (Exception ex)
		{
			lock (m_lock)
			{
				m_listening = false;
			}
			Debug.LogError("error listening for incoming connections: " + ex.Message);
			m_socket = null;
			return false;
		}
		return true;
	}

	private static void OnAccept(IAsyncResult ar)
	{
		ServerConnection<PacketType> connection = (ServerConnection<PacketType>)ar.AsyncState;
		if (connection != null && connection.m_socket != null)
		{
			try
			{
				ISocket incoming = connection.m_socket.EndAccept(ar);
				connection.m_currentConnection = new ClientConnection<PacketType>(incoming);
			}
			catch (Exception ex)
			{
				Debug.LogError("error accepting connection: " + ex.Message);
			}
			connection.m_listening = false;
		}
	}

	public ClientConnection<PacketType> GetNextAcceptedConnection()
	{
		if (m_currentConnection != null)
		{
			ClientConnection<PacketType> currentConnection = m_currentConnection;
			m_currentConnection = null;
			return currentConnection;
		}
		Listen();
		return null;
	}
}
