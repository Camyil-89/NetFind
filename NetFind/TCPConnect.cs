using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetFind
{
	public class Utility
	{
		private static readonly IPEndPoint DefaultLoopbackEndpoint = new IPEndPoint(IPAddress.Loopback, port: 0);

		public static int GetAvailablePort()
		{
			using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
			{
				socket.Bind(DefaultLoopbackEndpoint);
				return ((IPEndPoint)socket.LocalEndPoint).Port;
			}
		}
		public static IPAddress GetLocalIPAddress()
		{
			var host = Dns.GetHostEntry(Dns.GetHostName());
			foreach (var ip in host.AddressList)
			{
				if (ip.AddressFamily == AddressFamily.InterNetwork)
				{
					return ip;
				}
			}
			throw new Exception("No network adapters with an IPv4 address in the system!");
		}
	}
	public class TCPServer
	{
		public int TimeoutSendPacket = 500;
		public int TimeoutBeforeSend = 50;
		public bool DisposeResource = false;


		bool SendPacket = false;
		List<Packet.FindType> BlackList = new List<Packet.FindType>();
		void SenderPacket(UdpClient udpClient, IPEndPoint iPEndPoint, byte[] array)
		{
			SendPacket = true;
			Task.Run(() =>
			{
				Stopwatch stopwatch = new Stopwatch();
				stopwatch.Start();
				while (SendPacket && !DisposeResource && stopwatch.ElapsedMilliseconds < TimeoutSendPacket)
				{
					try
					{
						udpClient.Send(array, array.Length, iPEndPoint);
					}
					catch { }
					Thread.Sleep(TimeoutBeforeSend);
				}
			});
		}
		public TcpListener StartTCPServer(int Port)
		{
			int PortTcpListener = Utility.GetAvailablePort();
			TcpListener tcpListener = new TcpListener(IPAddress.Any, PortTcpListener);

			Task.Run(() =>
			{
				UdpClient listener = new UdpClient(Port);
				IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Any, Port);


				bool CloseSocket = false;

				Guid last_packet_uid = Guid.Empty;
				
				while (!DisposeResource)
				{
					try
					{
						var x = listener.Receive(ref iPEndPoint);
						var packet = Packet.UdpFind.FromByteArray(listener.Receive(ref iPEndPoint));
						if (last_packet_uid != packet.UID)
						{
							BlackList.Clear();
							SendPacket = false;
						}
						if (BlackList.Contains(packet.Type)) continue;
						BlackList.Add(packet.Type);
						SendPacket = false;
						switch (packet.Type)
						{
							case Packet.FindType.Find:
								last_packet_uid = packet.UID;
								packet.Type = Packet.FindType.Connect;
								packet.PortServer = PortTcpListener;
								packet.IPAddressServer = Utility.GetLocalIPAddress();
								SenderPacket(listener,
									new IPEndPoint(packet.IPAddressClient, packet.PortClient),
									Packet.UdpFind.ToByteArray(packet));
								break;
							case Packet.FindType.ConnectTCP:
								SenderPacket(listener,
									new IPEndPoint(packet.IPAddressClient, packet.PortClient),
									Packet.UdpFind.ToByteArray(packet));
								CloseSocket = true;
								break;
						}
					}
					catch
					{
						if (CloseSocket) break;
					}
				}
				DisposeResource = true;
				SendPacket = false;
				listener.Close();
				listener.Dispose();
			});
			return tcpListener;
		}
	}
	public class TCPConnect
	{
		public int TimeoutBeforeSend = 50;
		public bool DisposeResource = false;


		int AvailablePortListener = Utility.GetAvailablePort();
		UdpClient udpFind = new UdpClient();

		List<Packet.FindType> BlackList = new List<Packet.FindType>();

		Packet.UdpFind UdpFindPacket = null;


		void UdpListener(int Port)
		{
			UdpClient listener = new UdpClient(Port);
			IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Any, Port);
			listener.Client.ReceiveTimeout = 10;
			while (!DisposeResource)
			{
				try
				{
					var packet = Packet.UdpFind.FromByteArray(listener.Receive(ref iPEndPoint));
					if (BlackList.Contains(packet.Type)) continue;
					BlackList.Add(packet.Type);
					UdpFindPacket = packet;
				}
				catch { }
			}
			listener.Close();
		}
		public TcpClient FindTCPListener(int PortListener, int Timeout)
		{
			IPEndPoint iPEndPoint = new IPEndPoint(new IPAddress(new byte[] { 255, 255, 255, 255 }), PortListener);
			Stopwatch stopwatch = new Stopwatch();

			Packet.UdpFind Packet_udp = new Packet.UdpFind()
			{
				UID = Guid.NewGuid(),
				Type = Packet.FindType.Find,
				IPAddressClient = Utility.GetLocalIPAddress(),
				PortClient = AvailablePortListener,
			};
			Task.Run(() => { UdpListener(AvailablePortListener); });
			var bytes_packet = Packet.UdpFind.ToByteArray(Packet_udp);

			stopwatch.Start();

			bool StartTCPConnect = false;


			while (stopwatch.ElapsedMilliseconds < Timeout && !DisposeResource)
			{
				try
				{
					udpFind.Send(bytes_packet, bytes_packet.Length, iPEndPoint);
					if (UdpFindPacket != null)
					{
						switch (UdpFindPacket.Type)
						{
							case Packet.FindType.Connect:
								UdpFindPacket.Type = Packet.FindType.ConnectTCP;
								bytes_packet = Packet.UdpFind.ToByteArray(UdpFindPacket);
								udpFind.Send(bytes_packet, bytes_packet.Length, iPEndPoint);
								break;
							case Packet.FindType.ConnectTCP:
								StartTCPConnect = true;
								break;
						}
						if (!StartTCPConnect) UdpFindPacket = null;
					}
				}
				catch { }
				if (StartTCPConnect) break;
				Thread.Sleep(TimeoutBeforeSend);
			}
			DisposeResource = true;
			udpFind.Close();
			if (!StartTCPConnect) throw new Exception("Timeout connect to server!");

			TcpClient tcpClient = new TcpClient();
			tcpClient.Connect(UdpFindPacket.IPAddressServer, UdpFindPacket.PortServer);
			return tcpClient;
		}
	}
}
