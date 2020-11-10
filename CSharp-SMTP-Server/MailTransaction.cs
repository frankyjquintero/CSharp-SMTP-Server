﻿using System;
using System.Collections.Generic;
using System.Net;
using CSharp_SMTP_Server.Networking;
using CSharp_SMTP_Server.Protocol;

namespace CSharp_SMTP_Server
{
	public class MailTransaction : ICloneable
	{
		public MailTransaction()
		{
			DeliverTo = new List<string>();
			AuthenticatedUser = null;
		}

		public string From;
		public string Body;

		public string Subject => Headers != null && Headers.TryGetValue("Subject", out var value) ? value : null;

		/// <summary>
		/// Recipients specified in the transaction.
		/// </summary>
		public List<string> DeliverTo;
		
		/// <summary>
		/// Recipients specified in the header.
		/// </summary>
		public IEnumerable<string> To => ParseAddresses("To");
		
		/// <summary>
		/// Recipients specified in the header (CC).
		/// </summary>
		public IEnumerable<string> Cc => ParseAddresses("Cc");
		
		/// <summary>
		/// Recipients specified in the header (BCC).
		/// </summary>
		public IEnumerable<string> Bcc => ParseAddresses("Bcc");
		
		private IEnumerable<string> ParseAddresses(string header)
		{
			if (Headers == null || !Headers.TryGetValue(header, out var t)) yield break;
			
			while (t.Contains("<"))
			{
				if (!t.Contains(">")) yield break;
				var address = t.Substring(t.IndexOf("<", StringComparison.Ordinal) + 1);
				var i = address.IndexOf(">", StringComparison.Ordinal);
				yield return address.Substring(0, i);
				if (i + 1 >= t.Length) yield break;
				t = t.Substring(i + 1);
			}
		}

		public Dictionary<string, string> Headers;
		
		public EndPoint RemoteEndPoint;

		/// <summary>
		/// Username of authenticated users. Empty if user is not authenticated.
		/// </summary>
		public string AuthenticatedUser;

		public ConnectionEncryption Encryption;

		public object Clone()
		{
			return new MailTransaction()
			{
				AuthenticatedUser = AuthenticatedUser,
				Body = Body,
				From = From,
				Headers = Headers,
				RemoteEndPoint = RemoteEndPoint,
				DeliverTo = DeliverTo
			};
		}
	}
}
