# NetFind

Library for finding server on local net.

Client
```cs
NetFind.Client client = new NetFind.Client();
Console.WriteLine(client.StartFind(11000)); // return IP server on local net
```

Server
```cs
NetFind.Server server = new NetFind.Server();
server.Start(11000); // listen port and send to client self IP
```

