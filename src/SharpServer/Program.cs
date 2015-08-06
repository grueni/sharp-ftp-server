using System;
using System.IO;
using System.Net;
using log4net;
using System.Runtime.Serialization;
using SharpServer.Ftp;

namespace SharpServer
{
    class Program
    {
        protected static ILog _log = LogManager.GetLogger(typeof(Program));

        static void Main(string[] args)
        {

			  try
			  {
				  String ftpconfigxml = "ftpconfig.xml";
				  FtpConfig ftpconfig = (File.Exists(ftpconfigxml)) ? 
					  ftpconfig = Ftp.FtpServer.Deserialize<FtpConfig>(ftpconfigxml)
					  :
					  ftpconfig = new Ftp.FtpConfig(ftproot: "p:\\temp\\ftphome", iPAddressV4: "0.0.0.0", userStore: "userStore.xml");
				  FtpServer.Serialize<FtpConfig>(ftpconfigxml, ftpconfig);
			  
				  using (SharpServer.Ftp.FtpServer s = new FtpServer(ftpconfig))
				  {
					  s.Start();

					  Console.WriteLine("Press any key to stop...");
					  Console.ReadKey(true);
				  }
			  }
			  catch (Exception ex)
			  {
				  Console.WriteLine("{0}\n{1}", ex.Message, ex.StackTrace);
			  }

            return;

            using (Server<SharpServer.Email.ImapClientConnection> imapServer = new Server<SharpServer.Email.ImapClientConnection>(143))
            using (Server<SharpServer.Email.SmtpClientConnection> smtpServer = new Server<SharpServer.Email.SmtpClientConnection>(25))
            {
                smtpServer.Start();
                imapServer.Start();

                Console.WriteLine("Press any key to stop...");
                Console.ReadKey(true);
            }

            return;

            using (Server<SharpServer.Email.Pop3ClientConnection> pop3server = new Server<SharpServer.Email.Pop3ClientConnection>(110))
            using (Server<SharpServer.Email.SmtpClientConnection> smtpServer = new Server<SharpServer.Email.SmtpClientConnection>(25))
            {
                pop3server.Start();
                smtpServer.Start();

                Console.WriteLine("Press any key to stop...");
                Console.ReadKey(true);
            }


            return;

            using (Server<SharpServer.Email.SmtpClientConnection> Server = new Server<SharpServer.Email.SmtpClientConnection>(25))
            {
                Server.Start();

                Console.WriteLine("Press any key to stop...");
                Console.ReadKey(true);
            }

            return;

        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _log.Fatal((Exception)e.ExceptionObject);
        }
    }
}
