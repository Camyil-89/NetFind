using System;

namespace ServerTest
{
	class Program
	{
		static void Main(string[] args)
		{
			NetFind.TCPServer tCPServer = new NetFind.TCPServer();
			tCPServer.TimeoutBeforeSend = 50;
			var x = tCPServer.StartTCPServer(32000);
			x.Start();
			while (true)
			{
				Console.WriteLine("Start server");
				var cl = x.AcceptTcpClient();
				Console.WriteLine(cl.Client.RemoteEndPoint);
			}
			Console.ReadLine();
		}
	}
}
