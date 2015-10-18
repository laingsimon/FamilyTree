using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Web;

namespace FamilyTree.Models
{
	public class CapturingTraceListener : TraceListener
	{
		private readonly List<string> _messages = new List<string>();

		public override void Write(string message)
		{ }

		public override void WriteLine(string message)
		{ }

		public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
		{
			_messages.Add(string.Format("{0}: {1}", eventType, message));
		}

		public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
		{
			TraceEvent(eventCache, source, eventType, id, string.Format(format, args));
		}

		public void WriteToResponse(System.Web.HttpResponse response)
		{
			if (_messages.Count == 0)
				return;

			response.StatusCode = 200;
			response.ContentType = "text/plain";

			using (var writer = new StreamWriter(response.OutputStream))
			{
				foreach (var message in _messages)
					writer.WriteLine(message);
			}

			response.End();
		}
	}
}