using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace FamilyTree.Models.FileSystem.AzureStorage
{
	public class SingleTaskGate
	{
		private readonly AutoResetEvent[] _signals;
		private readonly TimeSpan _timeout;

		public SingleTaskGate(int parallelism, TimeSpan? timeout = null)
		{
			_signals = Enumerable.Range(0, parallelism).Select(i => new AutoResetEvent(true)).ToArray();
			_timeout = timeout ?? TimeSpan.FromSeconds(2);
        }

		public T Execute<T>(Func<T> action, [CallerMemberName] string callerMethodName = "unknown")
		{
			Trace.TraceInformation("Waiting for slot for {0}", callerMethodName);
			var signalIndex = WaitHandle.WaitAny(_signals, _timeout);
			if (signalIndex == WaitHandle.WaitTimeout)
			{
				Trace.TraceWarning("Timeout waiting for slot for {0}", callerMethodName);
				throw new TimeoutException();
			}

			Trace.TraceInformation("Slot acquired for {0}", callerMethodName);
			var signal = _signals[signalIndex];
			try
			{
				return action();
			}
			finally
			{
				signal.Set();
				Trace.TraceInformation("Slot released for {0}", callerMethodName);
			}
		}
	}
}