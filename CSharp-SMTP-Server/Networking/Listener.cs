﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace CSharp_SMTP_Server.Networking
{
	public class Listener : IDisposable
	{
		internal readonly List<ClientProcessor> ClientProcessors;
		internal readonly SMTPServer Server;

		private readonly TcpListener _listener;
		private readonly Thread _listenerThread;
		private readonly IPEndPoint _ipEndPoint;
		private readonly bool _secure;
		private bool _dispose;

		internal Listener(IPAddress address, ushort port, SMTPServer s, bool secure)
		{
			Server = s;
			_secure = secure;
			ClientProcessors = new List<ClientProcessor>();

			_ipEndPoint = new IPEndPoint(address, port);
			_listener = new TcpListener(_ipEndPoint);
			_listenerThread = new Thread(Listen)
			{
				Name = "Listening on port " + port,
				IsBackground = true
			};
		}

		internal void Start() => _listenerThread.Start();

		private void Listen()
		{
			_listener.Start(200);
			while (!_dispose)
			{
				var client = _listener.AcceptTcpClient();
				ClientProcessors.Add(new ClientProcessor(client, this, _secure));
			}
		}

		public void Dispose()
		{
			_dispose = true;
			_listener.Stop();

			foreach (var processor in ClientProcessors)
				processor.Dispose();
		}
	}
}
