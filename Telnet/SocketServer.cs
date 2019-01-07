using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Telnet
{
    public class SocketServer
    {
        public SocketServer()
        {
        }

        ~SocketServer()
        {
            Stop();
            if ( _StartThread != null )
                _StartThread.Join();
        }

        AutoResetEvent _evtConnectDone = new AutoResetEvent(false);
        AutoResetEvent _evtReadDone = new AutoResetEvent(false);
        AutoResetEvent _evtSendDone = new AutoResetEvent(false);
        private TcpListener _ServerListener;
        private List<SocketClient> _Connections = new List<SocketClient>();
        Thread _StartThread;

        public delegate void OnConnectedHandler(object sender, IPEndPoint RemoteEndPoint);
        public delegate void OnDisconnectedHandler(object sender);
        public delegate void OnDataReceivedHandler(object sender, byte[] Data);
        public delegate void OnLogHandler(string Msg, LogLevel LogLevel);
        public event OnConnectedHandler OnConnected;
        public event OnDisconnectedHandler OnDisconnected;
        public event OnDataReceivedHandler OnDataReceived;
        public virtual event OnLogHandler OnLog;

        protected void _Log(string Msg, LogLevel LogLevel = LogLevel.Information) {
            if ( OnLog != null )
                OnLog(Msg, LogLevel);
        }
        protected void _LogError(string Msg) {
            if ( OnLog != null )
                OnLog(Msg, LogLevel.Error);
        }
        protected void _LogDebug(string Msg) {
            if ( OnLog != null )
                OnLog(Msg, LogLevel.Debug);
        }        
        protected void _LogWarning(string Msg) {
            if ( OnLog != null )
                OnLog(Msg, LogLevel.Warning);
        }
        protected void _LogCritical(string Msg) {
            if ( OnLog != null )
                OnLog(Msg, LogLevel.Critical);
        }
        public int Port { get; set; }
        public TimeSpan ConnectionTimeout { get; set; }
        public TimeSpan ReceiveTimeout { get; set; }
        public TimeSpan SendTimeout { get; set; }
        public int ReceiveBufferSize { get; set; }
        public int SendBufferSize { get; set; }

        private bool bStopThread = false;

        public bool Start()
        {
            try
            {
                _ServerListener = new TcpListener(IPAddress.Any, Port);
                _ServerListener.Start();
                _StartThread = new Thread(_StartThreadFunction);
                _StartThread.IsBackground = true;
                bStopThread = false;
                _StartThread.Start(this);
            }
            catch(Exception ex)
            {
                throw ex;
            }

            return true;
        }

        public bool Stop()
        {
            bStopThread = true;
            _Log("SocketServer::Stop() 1");
            foreach (SocketClient c in _Connections)
            {
                c.Disconnect();
            }
            _Log("SocketServer::Stop() 2");
            if ( _ServerListener != null )
                _ServerListener.Server.Disconnect(true);//Close();

            _Log("SocketServer::Stop() 3");
            //if (_StartThread != null && _StartThread.IsAlive)
            //{
            //    if (_StartThread.Join(new TimeSpan(0, 0, 5)) == false)
            //        _StartThread.Abort();
            //}

            return true;
        }

        public bool Send(string Data)
        {
            foreach (SocketClient c in _Connections)
            {
                c.Send(Data);
            }
            return true;
        }

        public bool Send(byte[] Data)
        {
            foreach (SocketClient c in _Connections)
            {
                c.Send(Data);
            }
            return true;
        }
        

        private void _StartThreadFunction(object obj)
        {
            SocketServer s = (SocketServer)obj;

            try
            {
 
                while (!s.bStopThread)
                {
                    SocketClient client;
                    TcpClient c = s._ServerListener.AcceptTcpClient();
                    
                    Debug.WriteLine("Connection Accepted from: " + c.Client.RemoteEndPoint.ToString());

                    c.SendTimeout = (int)s.SendTimeout.TotalMilliseconds;
                    c.ReceiveTimeout = (int)s.ReceiveTimeout.Milliseconds;
                   // c.ReceiveBufferSize = s.ReceiveBufferSize;
                    //c.SendBufferSize = s.SendBufferSize;

                    client = new SocketClient(c);
                    s._Connections.Add(client);
                    client.Connect();
                    client.OnDataReceived += new SocketClient.OnDataReceivedHandler(client_OnDataReceived);
                    client.OnConnected += new SocketClient.OnConnectedHandler(client_OnConnected);
                    client.OnDisconnected += new SocketClient.OnDisconnectedHandler(client_OnDisconnected);

                    if (OnConnected != null)
                        OnConnected((object)c, (IPEndPoint) c.Client.RemoteEndPoint);
                }

            }
            catch (SocketException ex)
            {
                Debug.WriteLine(ex.Message);                
            }
            catch (NullReferenceException ex)
            {
                Debug.WriteLine(ex.Message);
            }
            catch (ObjectDisposedException ex)
            {
                Debug.WriteLine(ex.Message);
            }
            catch (ApplicationException ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally
            {
            }
        }

        void client_OnDisconnected(object sender)
        {
            _Connections.Remove((SocketClient)sender);

            if (OnDisconnected != null)
                OnDisconnected(this);

            if (_Connections.Count == 0)
            {
                //if (_ServerListener != null)
                //    _ServerListener.Stop();
            }
        }

        void client_OnConnected(object sender, IPEndPoint RemoteEndPoint)
        {
            if (OnConnected != null)
                OnConnected(this, RemoteEndPoint);
        }

        void client_OnDataReceived(object sender, byte[] Data)
        {
            if (OnDataReceived != null)
            {
                OnDataReceived(sender, Data);
            }

        }
       
    }
}
