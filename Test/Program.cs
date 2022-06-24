using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Threading;

namespace Test
{
	class Program
	{
		static void Test(int count)
		{
			bool Server = false;
			bool Client = false;
			NetFind.TCPServer tCPServer = new NetFind.TCPServer();
			NetFind.TCPConnect tCPConnect = new NetFind.TCPConnect();
			tCPConnect.TimeoutBeforeSend = 50;
			tCPServer.TimeoutBeforeSend = 50;

			TcpClient tcpClientServer = new TcpClient();
			Task.Run(() => {
				var listener = tCPServer.StartTCPServer(32000 + count);
				listener.Start();
				tcpClientServer = listener.AcceptTcpClient();
				//Console.WriteLine($"Server start: {x.Client.LocalEndPoint}");
				//Console.WriteLine($"Server client: {x.Client.RemoteEndPoint}");
				Server = true;
			});
			var client = tCPConnect.FindTCPListener(32000 + count, 2000);
			Client = true;

			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			while (!Client || !Server)
			{
				if (stopwatch.ElapsedMilliseconds > 500)
				{
					Console.WriteLine($"{count} TIMEOUT");
					try
					{
						Console.WriteLine($"\t\t[{tcpClientServer.Client.RemoteEndPoint}]" +
					$" [{tcpClientServer.Client.LocalEndPoint}]" +
					$" [{client.Client.RemoteEndPoint}]" +
					$" [{client.Client.LocalEndPoint}]");
					} catch (Exception e) { Console.WriteLine(e); }
					
					break;
				}
				Thread.Sleep(1);
			}

			if (tcpClientServer.Connected && client.Connected)
			{
				//Console.WriteLine($"Test {count}: true [{tcpClientServer.Client.RemoteEndPoint}]" +
				//	$" [{tcpClientServer.Client.LocalEndPoint}]" +
				//	$" [{client.Client.RemoteEndPoint}]" +
				//	$" [{client.Client.LocalEndPoint}]");
			}
			else
			{

				Console.WriteLine($"Test {count}: false");
				Console.WriteLine($"\t\t{tcpClientServer}");
				Console.WriteLine($"\t\t{client}");
			}
			tcpClientServer.Close();
			tcpClientServer.Dispose();
			client.Close();
			client.Dispose();
			//Console.WriteLine($"client start: {client.Client.LocalEndPoint}");
			//Console.WriteLine($"client: {client.Client.RemoteEndPoint}");
		}
		static void Main(string[] args)
		{
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();

			//for (int i = 0; i < 100; i++)
			//{
			//	Test(i);
			//	if (i % 25 == 0) Console.WriteLine($"{i} TESTS; time {stopwatch.Elapsed.TotalSeconds} sec.");
			//	//Thread.Sleep(400);
			//}
			
			NetFind.TCPConnect tCPConnect = new NetFind.TCPConnect();
			tCPConnect.TimeoutBeforeSend = 50;
			try
			{
				Console.WriteLine("start client find");
				var x =tCPConnect.FindTCPListener(32000, 10000);
				Console.WriteLine($"LocalEndPoint: {x.Client.LocalEndPoint}\nRemoteEndPoint: {x.Client.RemoteEndPoint}");
				while (true)
				{
					Thread.Sleep(100);
				}
			}
			catch (Exception e) { Console.WriteLine(e); }
			Console.WriteLine($"END TEST {stopwatch.ElapsedMilliseconds}");
			Console.ReadLine();
		}
	}
}
