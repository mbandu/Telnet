using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;
using System.Net.Sockets;

namespace Telnet
{
    public class SocketClient 
    {
        public SocketClient()
        {
           
            ReceiveTimeout = new TimeSpan(0,0,0);
            SendTimeout = new TimeSpan(0, 0, 0);
            ReceiveBufferSize = 8192;
            SendBufferSize = 8192;
        }

        public SocketClient( TcpClient client )
        {
            _Client = client;
        }

        AutoResetEvent _evtConnectDone = new AutoResetEvent(false);
        AutoResetEvent _evtStartRead = new AutoResetEvent(false);
        AutoResetEvent _evtReadDone = new AutoResetEvent(false);
        AutoResetEvent _evtSendDone = new AutoResetEvent(false);
        private System.Net.Sockets.TcpClient _Client = null;
        Thread _ReadThread;

        public delegate void OnConnectedHandler(SocketClient sender, IPEndPoint RemoteEndPoint);
        public delegate void OnDisconnectedHandler(SocketClient sender);
        public delegate void OnDataReceivedHandler(SocketClient sender, byte[] Data);
        public event OnConnectedHandler OnConnected;
        public event OnDisconnectedHandler OnDisconnected;
        public event OnDataReceivedHandler OnDataReceived;
                
        public IPAddress Address { get; set; }
        public int Port { get; set; }
        public TimeSpan ConnectionTimeout { get; set; }
        public TimeSpan ReceiveTimeout { get; set; }
        public TimeSpan SendTimeout { get; set; }
        public int ReceiveBufferSize { get; set; }
        public int SendBufferSize { get; set; }

        public bool IsConnected { 
            get {
                if (_Client == null) return false;
                return _Client.Connected; 
            } 
        }


        public bool Connect()
        {
            if (_Client != null && _Client.Connected)
            {
           
                _ReadThread = new Thread(_ReadThreadFunction);
                _ReadThread.IsBackground = true;
                _ReadThread.Start(this);
                _evtStartRead.Set();
                return true;

            }

            if (_Client == null)
                _Client = new TcpClient();

            _Client.SendTimeout = (int)SendTimeout.TotalMilliseconds;
            _Client.ReceiveTimeout = (int)ReceiveTimeout.Milliseconds;
            _Client.ReceiveBufferSize = ReceiveBufferSize;
            _Client.SendBufferSize = SendBufferSize;

            _Client.BeginConnect(Address, Port, _ConnectCallback, _Client );

            _ReadThread = new Thread(_ReadThreadFunction);
            _ReadThread.IsBackground = true;
            _ReadThread.Start(this);
           var connSuccess = _evtConnectDone.WaitOne(TimeSpan.FromSeconds(10.0));
           if (_Client.Connected == false || !connSuccess)
            {
                if (!connSuccess)
                {
                    Console.WriteLine("Connection timeout. (>10sec)");
                }
                   
                if (_ReadThread.IsAlive)
                {
                    try
                    {
                        _evtStartRead.Set();
                        _ReadThread.Join(new TimeSpan(0, 0, 5));
                            
                    }
                    catch (Exception ex)
                    {
                    }
                }
                return false;
            }
            return true;
        }

        public bool Disconnect()
        {
            if (_Client != null)
            {
                try
                {
                    if (_Client.Connected)
                        _Client.GetStream().Close();
                
                    //if (_ReadThread != null && _ReadThread.IsAlive)
                    //{
                    //    _ReadThread.Join(new TimeSpan(0, 0, 5));
                            
                    //}
}
                catch (Exception ex)
                {
                }
                _Client.Close();
                _Client = null;
            }
            return true;
        }

        public bool Send(string Data)
        {
            try
            {
                NetworkStream nStream = _Client.GetStream();
                if (Data == null)
                    throw new ArgumentNullException();

                if (Data.Length == 0)
                    return false;

                if (nStream.CanWrite)
                {

                    byte[] Buffer = Encoding.ASCII.GetBytes(Data);
                    nStream.BeginWrite(Buffer,
                                        0,
                                        Buffer.Length,
                                        new AsyncCallback(_WriteCallback),
                                        this);

                    _evtSendDone.WaitOne();
                }
                else
                {
                    Console.WriteLine("Sorry.  You cannot write to this NetworkStream.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message + "\r\n" + ex.StackTrace);
                return false;
            }

            return true;
        }

        public bool Send(byte[] Data)
        {
            try
            {
                NetworkStream nStream = _Client.GetStream();

                if (Data == null)
                    throw new ArgumentNullException();

                if (Data.Length == 0)
                    return false;

                if (nStream.CanWrite)
                {
                    nStream.BeginWrite(Data,
                                        0,
                                        Data.Length,
                                        new AsyncCallback(_WriteCallback),
                                        this);

                    if (_evtSendDone.WaitOne(5000) == false)
                    {
                        //Console.WriteLine("Telnet sending timed out.");
                        _evtSendDone.Set();
                        return false;
                    }
                }
                else
                {
                    Console.WriteLine("Sorry.  You cannot write to this NetworkStream.");
                    return false;
                }
            }
            catch (ObjectDisposedException ex)
            {
            }
            catch (ApplicationException ex)
            {
            }
            catch (Exception ex)
            {
            }
            finally
            {
            }
            return true;
        }

        public static void _WriteCallback(IAsyncResult ar)
        {
            SocketClient c = (SocketClient)ar.AsyncState;
            c._Client.GetStream().EndWrite(ar);
            c._evtSendDone.Set();

        }

        public void _ConnectCallback(IAsyncResult ar)
        {
            try
            {
                TcpClient t = (TcpClient)ar.AsyncState;
                t.EndConnect(ar);
                _evtConnectDone.Set();
                _evtStartRead.Set();
                if (OnConnected != null)
                    OnConnected(this, (IPEndPoint)_Client.Client.RemoteEndPoint);
            }
            catch (Exception Ex)
            {
                _evtConnectDone.Set();
                _evtStartRead.Set();
                Debug.WriteLine(Ex.Message + "\r\n" + Ex.StackTrace);
            }
            finally
            {
                

            }
        }

        private void _ReadThreadFunction(object obj)
        {
            SocketClient c = (SocketClient)obj;

            try
            {
                _evtStartRead.WaitOne();
                NetworkStream nStream = c._Client.GetStream();
                while (c._Client.Connected)
                {
                    if (c._Client.Available == 0)
                    {
                        Thread.Sleep(100);

                        bool b = !(c._Client.Client.Poll(1, SelectMode.SelectRead) && c._Client.Available == 0);
                        if (b == false)
                        {                            
                            break;
                        }
                        continue;
                    }
                    if (nStream.CanRead)
                    {
                        int iReadLen = 0;
                        int toRead = c._Client.Available;
                        if (toRead == 0)
                            toRead = c._Client.ReceiveBufferSize;
                        byte[] myReadBuffer = new byte[c._Client.Available];

                        iReadLen = nStream.Read(myReadBuffer, 0, myReadBuffer.Length);
                        
                        if ( iReadLen != 0 ) 
                            if (c.OnDataReceived != null)
                                c.OnDataReceived(c, myReadBuffer);
                        //nStream.BeginRead(myReadBuffer,
                        //                    0,
                        //                    myReadBuffer.Length,
                        //                    new AsyncCallback(_ReadCallback),
                        //                    c);

                        //c._evtReadDone.WaitOne();

                    }
                    else
                    {
                        Console.WriteLine("Sorry.  You cannot read from this NetworkStream.");
                    }
                }
            }
            catch (NullReferenceException ex)
            {
            }
            catch (ObjectDisposedException ex)
            {
            }
            catch (Exception ex)
            {
            }
            finally
            {
                if (OnDisconnected != null)
                    OnDisconnected(this);

                if (_Client != null)
                {
                    _Client.Close();
                    _Client = null;
                }
                
            }
        }

        //public static void _ReadCallback(IAsyncResult ar)
        //{
        //    Client c = (Client)ar.AsyncState;
            
        //    int iRead = c._Client.GetStream().EndRead(ar);
            
        //    c._evtReadDone.Set();
        //}


    }
}
