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

		static void Main(String[] args)
		{
//			String log4netconfig = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SharpServer.log4net");

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
//					x.SetDescription("BJS FTP Service");
//					x.SetDisplayName("BJSFTPService");
//					x.SetServiceName("BJSFTPService");
//					x.UseLog4Net(log4netconfig);
				});                                               
				
			}
			catch (Exception ex)
			{
				Console.WriteLine("{0}\n{1}", ex.Message, ex.StackTrace);
			}
			return;
		}

// nicht benutzt, als Erinnerungsstütze		
		static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			_log.Fatal((Exception)e.ExceptionObject);
			
		}

	}

	public class ServiceStarter : ServiceControl
	{
		protected static ILog _log = LogManager.GetLogger(typeof(ServiceStarter));
		String _ftpconfigxml = @"config\ftpconfig.xml";
		FtpServer _ftpServer = null;

		public ServiceStarter() {
		}

		public Boolean Start(HostControl hostControl) 
		{
			var ftpconfig = (File.Exists(_ftpconfigxml)) ?
				Ftp.FtpServer.Deserialize<FtpConfig>(_ftpconfigxml)
				:
				new Ftp.FtpConfig(ftproot: "p:\\temp\\ftphome", iPAddressV4: "0.0.0.0", userStore: "userStore.xml");
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
