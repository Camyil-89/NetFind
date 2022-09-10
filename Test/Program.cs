using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Threading;
using System.Net.NetworkInformation;

namespace Test
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Client");

			NetFind.Client client = new NetFind.Client();
			Console.WriteLine(client.StartFind(11000));

			Console.ReadLine();
		}
	}
}
