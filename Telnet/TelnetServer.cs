using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Diagnostics;

namespace Telnet
{
    public class TelnetServer: TelnetBase
    {
        public TelnetServer()
        {
            _Server = new SocketServer();
            _Server.OnDataReceived += new SocketServer.OnDataReceivedHandler(_Server_OnDataReceived);
            _Server.OnConnected += new SocketServer.OnConnectedHandler(_Server_OnConnected);
            _Server.OnDisconnected += new SocketServer.OnDisconnectedHandler(_Server_OnDisconnected);
            _Options = new Dictionary<byte, TelnetOption>();
        }


        ~TelnetServer()
        {
        }

        //public delegate void OnDataReceivedHandler(object sender, byte[] Data);
        public override event OnDataReceivedHandler OnDataReceived;
        public override event OnConnectedHandler OnConnected;
        public override event OnDisconnectedHandler OnDisconnected;
        public override event OnOptionNegotiatedHandler OnOptionNegotiated;

        private SocketServer _Server;

        public override bool SendToNetwork(byte[] Data) 
        {
            return _Server.Send(Data);
        }

        protected override void DataReceived(byte[] Data)
        {
            if (OnDataReceived != null)
                OnDataReceived(this, Data);
        }

        protected override void OptionNegotiated(byte OptionCode)
        {
            if (OnOptionNegotiated != null)
                OnOptionNegotiated(this, OptionCode);
        }

        public bool SetOption(byte OptionCode, TelnetOption Option)
        {
            if (Option == null) return false;

            if (_Options.ContainsKey(OptionCode))
                return true;

            _Options.Add(OptionCode, Option);
            Option.OnSendData += new TelnetOption.OnSendDataHandler(Option_OnSendData);
            return true;
        }

        void Option_OnSendData(object sender, byte[] DataToSend)
        {
            _Server.Send(DataToSend);
        }

        public bool Start()
        {
            try
            {
                _Server.Port = Port;
                
                _Server.Start();
            }
            catch (Exception ex)
            {
                throw;
            }

            return true;
        }

        public void Stop()
        {
            try
            {
                _Server.Stop();
            }
            catch (Exception ex)
            {
                throw;
            }

            return;
        }

        void _Server_OnDisconnected(object sender)
        {
            if (OnDisconnected != null)
                OnDisconnected(this);

            foreach (KeyValuePair<byte, TelnetOption> op in _Options)
            {
                op.Value.Reset();
            }
        }

        void _Server_OnConnected(object sender, IPEndPoint RemoteEndPoint)
        {
            if (OnConnected != null) {
                 OnConnected(this, RemoteEndPoint);  
            }
            foreach (KeyValuePair<byte, TelnetOption> op in _Options)
            {
                _Server.Send(op.Value.GetNegotiationPacket());
            }
        }



        void _Server_OnDataReceived(object sender, byte[] DataStream)
        {
            if (sender.GetType() != typeof(SocketClient))
            {
                Debug.Assert(false);
                return;
            }
            SocketClient client = sender as SocketClient;

            _ProcessIncomingData(client, DataStream);
        }


   }
}
