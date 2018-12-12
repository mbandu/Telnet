using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Diagnostics;

namespace Telnet
{
    public class TelnetClient : TelnetBase
    {
        public TelnetClient()
        {
            _Client = new SocketClient();
            _Client.OnDataReceived += new SocketClient.OnDataReceivedHandler(_Client_OnDataReceived);
            _Client.OnConnected += new SocketClient.OnConnectedHandler(_Client_OnConnected);
            _Client.OnDisconnected += new SocketClient.OnDisconnectedHandler(_Client_OnDisconnected);
            _Options = new Dictionary<byte, TelnetOption>();
        }     

        ~TelnetClient()
        {
        }

        public override event OnDataReceivedHandler OnDataReceived;
        public override event OnConnectedHandler OnConnected;
        public override event OnDisconnectedHandler OnDisconnected;
        public override event OnOptionNegotiatedHandler OnOptionNegotiated; 
        
        private SocketClient _Client;

        public override bool SendToNetwork(byte[] Data)
        {            
            return _Client.Send(Data);
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
            _Client.Send(DataToSend);
        }

        public bool Connect()
        {
            if (_Client == null) return false;

            _Client.Address = Address;
            _Client.Port = Port;
            
            _CurrentTelNetState = TelnetState.Normal;

            return _Client.Connect();
        }

        public void Disconnect()
        {
            _Client.Disconnect();
            _CurrentTelNetState = TelnetState.Normal;
            foreach (KeyValuePair<byte, TelnetOption> op in _Options)
            {
                op.Value.Reset();
            }
        }


        void _Client_OnDataReceived(object sender, byte[] DataStream)
        {
            if (sender.GetType() != typeof(SocketClient))
            {
                Debug.Assert(false);
                return;
            }
            SocketClient client = sender as SocketClient;

            _ProcessIncomingData(client, DataStream);
        }

        void _Client_OnDisconnected(SocketClient sender)
        {
            if (OnDisconnected != null)
                OnDisconnected(this);
        }

        void _Client_OnConnected(SocketClient sender, IPEndPoint RemoteEndPoint)
        {
            if (OnConnected != null)
            {
                OnConnected(this, RemoteEndPoint);
            }

            foreach ( KeyValuePair<byte,TelnetOption> op in _Options)
            {
                _Client.Send(op.Value.GetNegotiationPacket());
            }
        }
    }
}
