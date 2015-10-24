using System;
using System.Linq;
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

		public T Execute<T>(Func<T> action)
		{
			var signalIndex = WaitHandle.WaitAny(_signals, _timeout);
            if (signalIndex == WaitHandle.WaitTimeout)
				throw new TimeoutException();

			var signal = _signals[signalIndex];
			try
			{
				return action();
			}
			finally
			{
				signal.Set();
			}
		}
	}
}