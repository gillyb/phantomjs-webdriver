using System;
using System.Diagnostics;
using System.Threading;

namespace PhantomJsWebDriver
{
	public class PhantomJsWebDriver
	{
		private PhantomJsWebDriverSettings _settings;
		private Process _browser;

		// the maximum time to wait when starting up phantomjs (in milliseconds)
		private int MaxStartupTimeout = 10000;
		
		private PhantomJsStatus _status = PhantomJsStatus.Created;
		public PhantomJsStatus Status
		{
			get { return _status; }
		}

		public PhantomJsWebDriver(PhantomJsWebDriverSettings settings)
		{
			_settings = settings;
		}

		public void Start()
		{
			StartPhantomJs();
		}

		private void StartPhantomJs()
		{
			_status = PhantomJsStatus.WaitingToStart;
			_browser = new Process {
				StartInfo = {
					FileName = _settings.Executable,
					Arguments = "--webdriver=" + _settings.Port,
					RedirectStandardOutput = true,
					UseShellExecute = false
				}
			};

			_browser.OutputDataReceived += BrowserOnOutputDataReceived;
			_browser.Start();
			_browser.BeginOutputReadLine();

			var timer = new Stopwatch();
			timer.Start();
			while (_status == PhantomJsStatus.WaitingToStart && timer.ElapsedMilliseconds <= MaxStartupTimeout)
			{
				Thread.Sleep(250);
			}
			timer.Stop();

			if (_status == PhantomJsStatus.Running)
				return;

			throw new Exception("Could not start phantomJs");
		}

		private void BrowserOnOutputDataReceived(object sender, DataReceivedEventArgs dataReceivedEventArgs)
		{
			if (dataReceivedEventArgs.Data.StartsWith("Ghost Driver running on port"))
				_status = PhantomJsStatus.Running;
		}
	}

	public enum PhantomJsStatus
	{
		Created,
		WaitingToStart,
		Running,
		Closed
	}
}