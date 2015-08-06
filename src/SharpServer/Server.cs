using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using log4net;
using System.Xml;
using System.IO;

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

        public Server(int port, string logHeader = null)
            : this(IPAddress.Any, port, logHeader)
        {
        }

        public Server(IPAddress ipAddress, int port, string logHeader = null)
            : this(new IPEndPoint[] { new IPEndPoint(ipAddress, port) }, logHeader)
        {
        }

		  public Server(IPEndPoint[] localEndPoints, String userStore, String logHeader = null)
		  {
			  _userStore = userStore;
			  _localEndPoints = new List<IPEndPoint>(localEndPoints);
			  _logHeader = logHeader;
			  _log.DebugFormat("_userStore={0}", _userStore);
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

        private void HandleAcceptTcpClient(IAsyncResult result)
        {
            OnConnectAttempt();

            TcpListener listener = result.AsyncState as TcpListener;

            if (_listening)
            {
                listener.BeginAcceptTcpClient(HandleAcceptTcpClient, listener);

                TcpClient client = listener.EndAcceptTcpClient(result);
					 _log.DebugFormat("_userStore={0}", _userStore);
					 var connection = new T() { UserStore = _userStore};

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
            _disposing = true;

            if (!_disposed)
            {
                if (disposing)
                {
                    Stop();

                    lock (_listLock)
                    {
                        foreach (var connection in _state)
                        {
                            if (connection != null)
                                connection.Dispose();
                        }

                        _state = null;
                    }
                }
            }

            _disposed = true;
        }

		  public static T Deserialize<T>(string fileName)
		  {
			  T t;
			  using (var reader = new FileStream(fileName, FileMode.Open))
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
			  using (var filestream = new FileStream(fileName, FileMode.Create))
			  using (var writer = XmlWriter.Create(filestream, settings))
			  {
				  var serializer = new DataContractSerializer(typeof(T));
				  serializer.WriteObject(writer, t);
				  writer.Close();
				  filestream.Close();
			  }
			  //using (var writer = new FileStream(fileName, FileMode.Create))
			  //{
			  //   var serializer = new DataContractSerializer(typeof(T));
			  //   serializer.WriteObject(writer, t);
			  //   writer.Close();
			  //}
		  }

	 }
}