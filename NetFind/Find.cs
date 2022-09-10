﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
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
		public static UnicastIPAddressInformation GetIPAddressInformation()
		{
			NetworkInterface[] networks = NetworkInterface.GetAllNetworkInterfaces();
			var activeAdapter = networks.First(x => x.NetworkInterfaceType != NetworkInterfaceType.Loopback
								&& x.NetworkInterfaceType != NetworkInterfaceType.Tunnel
								&& x.OperationalStatus == OperationalStatus.Up
								&& x.Name.StartsWith("vEthernet") == false);
			foreach (var item in activeAdapter.GetIPProperties().UnicastAddresses)
			{
				if (item.Address.AddressFamily == AddressFamily.InterNetwork)
					return item;
			}
			throw new Exception("No network adapters with an IPv4 address in the system!");
		}
		public static IPAddress GetLocalIPAddress()
		{
			NetworkInterface[] networks = NetworkInterface.GetAllNetworkInterfaces();
			var activeAdapter = networks.First(x => x.NetworkInterfaceType != NetworkInterfaceType.Loopback
								&& x.NetworkInterfaceType != NetworkInterfaceType.Tunnel
								&& x.OperationalStatus == OperationalStatus.Up
								&& x.Name.StartsWith("vEthernet") == false);
			foreach (var item in activeAdapter.GetIPProperties().UnicastAddresses)
			{
				if (item.Address.AddressFamily == AddressFamily.InterNetwork)
					return item.Address;
			}
			throw new Exception("No network adapters with an IPv4 address in the system!");
		}
	}
	public class Server
	{
		private UdpClient udpClient { get; set; }
		private bool Work = true;

		public void Stop() => Work = false;
		public void Start(int Port)
		{
			IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Any, Port);
			udpClient = new UdpClient(iPEndPoint);

			var ip = Utility.GetLocalIPAddress();

			Packet.UdpFind udpFind = new Packet.UdpFind();
			udpFind.Type = Packet.FindType.ConnectAddress;
			udpFind.IPAddressServer = ip.Address;

			byte[] packet_data = Packet.UdpFind.ToByteArray(udpFind);

			UdpClient sender = new UdpClient();

			while (Work)
			{
				try
				{
					byte[] data = udpClient.Receive(ref iPEndPoint);

					var packet = Packet.UdpFind.FromByteArray(data);


					IPEndPoint iPEndPoint1 = new IPEndPoint(iPEndPoint.Address, Port + 1);

					sender.Send(packet_data, packet_data.Length, iPEndPoint1);
				}
				catch { }
			}
		}




	}
	public class Client
	{
		UdpClient udpClient;
		private bool Find = true;
		private void Send()
		{
			Packet.UdpFind udpFind = new Packet.UdpFind();
			udpFind.Type = Packet.FindType.Find;

			while (Find)
			{
				try
				{
					byte[] data = Packet.UdpFind.ToByteArray(udpFind);
					udpClient.Send(data, data.Length);
				}
				catch { }
			}
			udpClient.Dispose();
		}
		public IPAddress StartFind(int Port, int TimeoutWaitConnection = 5000)
		{
			var addressInfo = Utility.GetIPAddressInformation();
			List<byte> address = new List<byte>();
			var x = addressInfo.Address.GetAddressBytes();
			for (int i = 0; i < x.Length; i++)
			{
				if (addressInfo.IPv4Mask.GetAddressBytes()[i] == 0)
					address.Add(255);
				else
					address.Add(x[i]);
			}
			IPEndPoint endPoint = new IPEndPoint(new IPAddress(address.ToArray()), Port);

			udpClient = new UdpClient();
			udpClient.EnableBroadcast = true;
			udpClient.Connect(endPoint);

			Task.Run(() => { Send(); });

			IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Any, Port + 1);
			UdpClient client = new UdpClient(iPEndPoint);
			client.Client.ReceiveTimeout = 1000;
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			while (stopwatch.ElapsedMilliseconds < TimeoutWaitConnection)
			{
				try
				{
					byte[] data = client.Receive(ref iPEndPoint);
					var packet = Packet.UdpFind.FromByteArray(data);
					if (packet.Type == Packet.FindType.ConnectAddress && packet.IPAddressServer != null)
					{
						client.Dispose();
						Find = false;
						return new IPAddress(packet.IPAddressServer);
					}
				}
				catch { }
			}
			Find = false;
			client.Dispose();
			return null;
		}
	}

}
