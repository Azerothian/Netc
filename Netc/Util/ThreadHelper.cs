using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Netc.Util
{
	public abstract class ThreadHelper : IDisposable
	{
		public int ThreadSleep = 10;
		public bool Active = true;
		private bool _isTerminating = false;
		private bool _slept = false;
		public Thread _thread = null;
		public DateTime LastRun { get; set; }

		public ThreadHelper()
		{
			_thread = new Thread(Worker);
			LastRun = DateTime.Now;
		}
		private void Worker()
		{
			do
			{
				if (Active)
				{
					ThreadWorker(DateTime.Now - LastRun);
					LastRun = DateTime.Now;
				}
				if (!_slept)
				{
					Sleep();
				}
				_slept = false;
			} while (!_isTerminating);
		}

		public void Start()
		{
			Active = true;
			if (!_thread.IsAlive)
			{
				_thread.Start();
			}

		}
		public void Sleep()
		{
			Thread.Sleep(ThreadSleep);
			_slept = true;
		}
		public void CancelNextSleep()
		{
			_slept = true;
		}
		public void Pause()
		{
			Active = false;
		}

		public void Stop()
		{
			_isTerminating = true;
		}

		public abstract void ThreadWorker(TimeSpan timeDiff);



		public void Dispose()
		{
			Stop();
		}
	}
}
