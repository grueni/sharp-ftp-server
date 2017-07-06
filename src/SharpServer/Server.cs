using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using log4net;
using System.Xml;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace SharpServer
{
    public class Server<T> : IDisposable where T : ClientConnection, new()
    {
        private static readonly object _listLock = new object();

        private ILog _log = LogManager.GetLogger(typeof(Server<T>));

        private List<T> _state;

        private List<TcpListener> _listeners;

        private bool _disposed = false;
        private bool _disposing = false;
        private bool _listening = false;
        private List<IPEndPoint> _localEndPoints;
        private string _logHeader;
		private String _userStore;
		internal X509Certificate2 _ServerCertificate = null;
        private int _minPort;
        private int _maxPort;

        public Server(int port, string logHeader = null)
            : this(IPAddress.Any, port, logHeader)
        {
        }

        public Server(IPAddress ipAddress, int port, string logHeader = null)
            : this(new IPEndPoint[] { new IPEndPoint(ipAddress, port) }, logHeader)
        {
        }

		  public Server(IPEndPoint[] localEndPoints, String userStore, int minPort=1024, int maxPort=65535, String certificatePath = null, String certificatePassword=null,String logHeader = null)
		  {
            _userStore = userStore;
            _localEndPoints = new List<IPEndPoint>(localEndPoints);
            _logHeader = logHeader;
            _minPort = minPort;
            _maxPort = maxPort;
            _log.DebugFormat("_userStore={0} certificatePath={1} _minPort={2} _maxPort={3}", _userStore, certificatePath,_minPort,_maxPort);
            if (!String.IsNullOrEmpty(certificatePath) && File.Exists(certificatePath))
            {
	            _ServerCertificate = String.IsNullOrEmpty(certificatePassword)
		            ? new X509Certificate2(certificatePath)
		            : new X509Certificate2(certificatePath, certificatePassword);
            }
		  }

		  public Server(IPEndPoint[] localEndPoints, string logHeader = null)
		  {
			  _localEndPoints = new List<IPEndPoint>(localEndPoints);
			  _logHeader = logHeader;
		  }

		  public Boolean Start()
        {
			  Boolean rc = true;
            if (_disposed)
                throw new ObjectDisposedException("AsyncServer");

			_log.Info("# Starting Server");
			_state = new List<T>();
            _listeners = new List<TcpListener>();

            foreach (var localEndPoint in _localEndPoints)
            {
				String text = String.Format("This local end point is currently in use AddressFamily={0} Address={1} Port={2}",
							localEndPoint.AddressFamily, localEndPoint.Address, localEndPoint.Port);
				TcpListener listener;
                try
                {
					listener = new TcpListener(localEndPoint);
					listener.Start();
					listener.BeginAcceptTcpClient(HandleAcceptTcpClient, listener);
					_listeners.Add(listener);
				}
                catch (SocketException ex)
                {
					_log.ErrorFormat(text);
					Dispose();
					rc = false;
				}
            }
			if (rc) {
				_listening = true;
				OnStart();
			}
			return rc;
        }

		  public Boolean Stop()
        {
			  Boolean rc = true;
			  _log.Info("# Stopping Server");
            _listening = false;

            foreach (var listener in _listeners)
            {
                listener.Stop();
            }

            _listeners.Clear();

            OnStop();
				return rc;
		  }

        protected virtual void OnStart()
        {
        }

        protected virtual void OnStop()
        {
        }

        protected virtual void OnConnectAttempt()
        {
        }

		  // prüfen, ob das wirklich so funktioniert: nonblocking BeginAcceptTcpClient, gefolgt von blocking EndAcceptTcpClient
		 // trennen in 2 Methoden??
        private void HandleAcceptTcpClient(IAsyncResult result)
        {
            OnConnectAttempt();

            var listener = result.AsyncState as TcpListener;

            if (listener.Server.IsBound)
            {
				var client = listener.EndAcceptTcpClient(result);

				listener.BeginAcceptTcpClient(HandleAcceptTcpClient, listener);

				_log.DebugFormat("_userStore={0}", _userStore);
				var connection = new T() { UserStore = _userStore, ServerCertificate = _ServerCertificate, minPort = _minPort, maxPort = _maxPort };

                connection.Disposed += new EventHandler<EventArgs>(AsyncClientConnection_Disposed);

                connection.HandleClient(client);

                lock (_listLock)
                    _state.Add(connection);
            }
        }

        private void AsyncClientConnection_Disposed(object sender, EventArgs e)
        {
            // Prevent removing if we are disposing of this object. The list will be cleaned up in Dispose(bool).
            if (!_disposing)
            {
                T connection = (T)sender;

                lock (_listLock)
                    _state.Remove(connection);
            }
        }

        public void Dispose()
        {
            _disposing = true;
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
			  _log.DebugFormat("drin");
            _disposing = true;

            if (!_disposed)
            {
                if (disposing)
                {
                    Stop();
						  _log.DebugFormat("nach stop");

                    lock (_listLock)
                    {
                        foreach (var connection in _state)
                        {
                            if (connection != null)
                                connection.Dispose();
                        }
								_log.DebugFormat("nach connection");

                        _state = null;
                    }
                }
            }

            _disposed = true;
        }

// hier einbauen: FileShare.ReadWrite, keine Delete
// StreamReader verwenden und aus diesem serializer füttern, falls serializer.ReadObject nicht streamreader verwendet
		  public static T Deserialize<T>(string fileName)
		  {
			  T t;
			  using (var reader = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Delete | FileShare.Read | FileShare.Write))
			  {
				  var serializer = new DataContractSerializer(typeof(T));
				  t = (T)serializer.ReadObject(reader);
				  reader.Close();
			  }
			  return t;
		 }
		  public static void Serialize<T>(string fileName, T t)
		  {
			  var settings = new XmlWriterSettings()
			  {
				  Indent = true,
				  IndentChars = "\t"
			  };
			  using (var filestream = File.Open(fileName, FileMode.Create, FileAccess.Write, FileShare.Read))
			  using (var writer = XmlWriter.Create(filestream, settings))
			  {
				  var serializer = new DataContractSerializer(typeof(T));
				  serializer.WriteObject(writer, t);
				  writer.Close();
				  filestream.Close();
			  }
		  }

	 }
}