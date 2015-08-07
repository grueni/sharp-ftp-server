using System;
using System.Net;
using System.Timers;
using System.IO;
using log4net;

namespace SharpServer.Ftp
{
	public class FtpServer : Server<FtpClientConnection>
	{
		private ILog _log = LogManager.GetLogger(typeof(FtpServer));
		private FtpConfig _ftpConfig;
		private DateTime _startTime;
		private Timer _timer;

		public FtpServer(FtpConfig ftpConfig, string logHeader = null) 
			:base(ftpConfig.LocalEndPoints,ftpConfig.UserStore, logHeader)
		{
			_ftpConfig = ftpConfig;
			foreach (var endPoint in ftpConfig.LocalEndPoints)
			{
				FtpPerformanceCounters.Initialize(endPoint.Port);
			}
		}

		public FtpConfig ftpConfig { get { return _ftpConfig; } }

		protected override void OnConnectAttempt()
		{
			FtpPerformanceCounters.IncrementTotalConnectionAttempts();
			base.OnConnectAttempt();
		}

		protected override void OnStart()
		{
			_startTime = DateTime.Now;
			_timer = new Timer(TimeSpan.FromSeconds(1).TotalMilliseconds);
			_timer.Elapsed += new ElapsedEventHandler(_timer_Elapsed);
			_timer.Start();
		}

		protected override void OnStop()
		{
			if (_timer != null)
					_timer.Stop();
		}

		protected override void Dispose(bool disposing)
		{
			FtpClientConnection.PassiveListeners.ReleaseAll();
			if (_timer != null)
					_timer.Dispose();
			base.Dispose(disposing);
		}

		private void _timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			FtpPerformanceCounters.SetFtpServiceUptime(DateTime.Now - _startTime);
		}
	}
}
