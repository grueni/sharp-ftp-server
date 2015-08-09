using System;
using System.IO;
using System.Net;
using log4net;
using System.Runtime.Serialization;
using SharpServer.Ftp;
using Topshelf;

namespace SharpServer
{
	class Program
	{
		protected static ILog _log = LogManager.GetLogger(typeof(Program));

		static void Main(string[] args)
		{

			try
			{
				HostFactory.Run(x =>
				{
					x.Service<ServiceStarter>();
					x.RunAsLocalSystem();
					x.StartAutomaticallyDelayed();
					x.UseLog4Net();
					x.EnableServiceRecovery(rc =>
						 {
							 rc.RestartService(1); // restart the service after 1 minute
							 rc.OnCrashOnly();
						 });
					x.SetDescription("BJS FTP Service");
					x.SetDisplayName("BJSFTPService");
					x.SetServiceName("BJSFTPService");
				});                                               
				
			}
			catch (Exception ex)
			{
				Console.WriteLine("{0}\n{1}", ex.Message, ex.StackTrace);
			}
			return;
		}

		static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			_log.Fatal((Exception)e.ExceptionObject);
		}

	}

	public class ServiceStarter : ServiceControl
	{
		protected static ILog _log = LogManager.GetLogger(typeof(ServiceStarter));
		String _ftpconfigxml = "ftpconfig.xml";
		FtpServer _ftpServer = null;

		public ServiceStarter() { }

		public Boolean Start(HostControl hostControl) 
		{
			FtpConfig ftpconfig = (File.Exists(_ftpconfigxml)) ?
				ftpconfig = Ftp.FtpServer.Deserialize<FtpConfig>(_ftpconfigxml)
				:
				ftpconfig = new Ftp.FtpConfig(ftproot: "p:\\temp\\ftphome", iPAddressV4: "0.0.0.0", userStore: "userStore.xml");
			_log.DebugFormat("ftpconfig={0}", ftpconfig.ToString());
			FtpServer.Serialize<FtpConfig>(_ftpconfigxml, ftpconfig);
			_ftpServer = new FtpServer(ftpconfig);
			Boolean rc = _ftpServer.Start();
			return rc;
		}
		public Boolean Stop(HostControl hostControl)
		{
			Boolean rc = _ftpServer.Stop();
			_ftpServer = null;
			return rc;
		}
	}

}
