using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace ServerTest
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Server");

			NetFind.Server server = new NetFind.Server();
			server.Start(11000);
			Console.ReadLine();
			Console.ReadLine();
		}
	}
}
