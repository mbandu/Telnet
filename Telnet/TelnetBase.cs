using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Diagnostics;

namespace Telnet
{
    public enum TelnetCommand : byte
    {
        SE = 240,               // End of subnegotiation parameters.
        NOP = 241,              // No operation.
        DataMark = 242,         // The data stream portion of a Synch.
        // This should always be accompanied
        // by a TCP Urgent notification.
        Break = 243,            // NVT character BRK.
        InterruptProcess = 244, // The function IP.
        AbortOutput = 245,      // The function AO.
        AreYouThere = 246,      // The function AYT.
        EraseCharacter = 247,   //The function EC.
        EraseLine = 248,        // The function EL.
        GoAhead = 249,          // The GA signal.
        SB = 250,               // Indicates that what follows is subnegotiation of the indicated option.
        WILL = 251,             // Indicates the desire to begin performing, or confirmation that you are now performing, the indicated option.
        WONT = 252,             // Indicates the refusal to perform, or continue performing, the indicated option.
        DO = 253,               // Indicates the request that the other party perform, or confirmation that you are expecting
        // the other party to perform, the indicated option.
        DONT = 254,             // Indicates the demand that the other party stop performing, or confirmation that you are no
        // longer expecting the other party to perform, the indicated option.
        IAC = 255,              // Data Byte 255.
    }

    public enum TelnetState
    {
        Normal,
        IAC,
        WILL,
        WONT,
        DO,
        DONT,
        SB,
        SB_IGNORE,
        SB_IGNORE_IAC,
        SB_OPTION,
    }

    public class TelnetBase
    {
        public delegate void OnConnectedHandler(TelnetBase Sender, IPEndPoint RemoteEndPoint);
        public delegate void OnDisconnectedHandler(TelnetBase Sender);
        public delegate void OnDataReceivedHandler(TelnetBase Sender, byte[] Data);
        public delegate void OnOptionNegotiatedHandler(TelnetBase Sender, byte OptionCode );
        public virtual event OnConnectedHandler OnConnected;
        public virtual event OnDisconnectedHandler OnDisconnected;
        public virtual event OnDataReceivedHandler OnDataReceived;
        public virtual event OnOptionNegotiatedHandler OnOptionNegotiated;
 
        protected TelnetState _CurrentTelNetState;
        protected Dictionary<byte, TelnetOption> _Options;
        private bool _bOptionCodeSaved = false;
        private byte _SavedOptionCode;
        
        public IPAddress Address { get; set; }
        public int Port { get; set; }

        public virtual bool SendToNetwork(byte[] Data) { return false; }

 
        protected void _ProcessIncomingData(SocketClient client, byte[] DataStream)
        {
            List<byte> RxData = new List<byte>();
            int iCount = 0;
            Debug.WriteLine("Incoming => " + DataStream.Length.ToString());
            lock (this)
            {

                for (int i = 0; i < DataStream.Length; i++)
                {
                    byte Data = DataStream[i];

                    iCount++;
                    Debug.Assert(i <= DataStream.Length);
                    switch (_CurrentTelNetState)
                    {
                        case TelnetState.Normal:
                            {
                                if (Data != (byte)TelnetCommand.IAC)
                                    RxData.Add(Data);
                                else
                                    _CurrentTelNetState = TelnetState.IAC;
                            }
                            break;
                        case TelnetState.IAC:
                            {
                                switch ((TelnetCommand)Data)
                                {
                                    case TelnetCommand.SE:
                                        _CurrentTelNetState = TelnetState.Normal;
                                        break;
                                    case TelnetCommand.NOP:
                                        break;
                                    case TelnetCommand.DataMark:
                                        break;
                                    case TelnetCommand.Break:
                                        break;
                                    case TelnetCommand.InterruptProcess:
                                        break;
                                    case TelnetCommand.AbortOutput:
                                        break;
                                    case TelnetCommand.AreYouThere:
                                        break;
                                    case TelnetCommand.EraseCharacter:
                                        break;
                                    case TelnetCommand.EraseLine:
                                        break;
                                    case TelnetCommand.GoAhead:
                                        break;
                                    case TelnetCommand.SB:
                                        _CurrentTelNetState = TelnetState.SB;
                                        break;
                                    case TelnetCommand.WILL:
                                        _CurrentTelNetState = TelnetState.WILL;
                                        break;
                                    case TelnetCommand.WONT:
                                        _CurrentTelNetState = TelnetState.WONT;
                                        break;
                                    case TelnetCommand.DO:
                                        _CurrentTelNetState = TelnetState.DO;
                                        break;
                                    case TelnetCommand.DONT:
                                        _CurrentTelNetState = TelnetState.DONT;
                                        break;
                                    case TelnetCommand.IAC:
                                        RxData.Add(Data);
                                        break;
                                    default:
                                        break;
                                }
                            }
                            break;
                        case TelnetState.WILL:
                            _ProcessWillCommand(Data);
                            _CurrentTelNetState = TelnetState.Normal;
                            break;
                        case TelnetState.WONT:
                            _ProcessWontCommand(Data);
                            _CurrentTelNetState = TelnetState.Normal;
                            break;
                        case TelnetState.DO:
                            _ProcessDoCommand(Data);
                            _CurrentTelNetState = TelnetState.Normal;
                            break;
                        case TelnetState.DONT:
                            _ProcessDontCommand(Data);
                            _CurrentTelNetState = TelnetState.Normal;
                            break;
                        case TelnetState.SB:
                            _CurrentTelNetState = TelnetState.SB_OPTION;
                            _bOptionCodeSaved = true;
                            _SavedOptionCode = Data;
                            break;
                        case TelnetState.SB_IGNORE:
                            if (Data == (byte)TelnetCommand.SE)
                                _CurrentTelNetState = TelnetState.Normal;
                            break;
                        case TelnetState.SB_IGNORE_IAC:
                            break;
                        case TelnetState.SB_OPTION:
                            _bOptionCodeSaved = false;
                            if (_Options.ContainsKey(_SavedOptionCode) == false)
                            {
                                _CurrentTelNetState = TelnetState.SB_IGNORE;
                                break;
                            }
                            int ProcessedLength = 0;
                            if (_ProcessOption(client, _SavedOptionCode, DataStream, i, out ProcessedLength) == false)
                            {
                                _CurrentTelNetState = TelnetState.SB_IGNORE;
                                break;
                            }
                            i += ProcessedLength -1;
                            break;
                        default:
                            break;
                    }
                }

            }
            if (RxData.Count != 0)
            {
                DataReceived(RxData.ToArray());
            }
        }

        protected virtual void DataReceived ( byte[] Data )
        {
            if (OnDataReceived != null)
               OnDataReceived(this,Data);
        }

        protected virtual void OptionNegotiated(byte OptionCode)
        {
            if (OnOptionNegotiated != null)
                OnOptionNegotiated(this, OptionCode);
        }

        protected void _ProcessWillCommand(byte Option)
        {
            if (_Options.ContainsKey(Option) == false)
            {
                Debug.WriteLine("(WILL?)OptionCode: " + Option.ToString() + "(0x" + Option.ToString("X") + ") not supported.");
                SendToNetwork(TelnetOption.GetNegotiationResponseDont(Option));
                return;
            }
            if (!_Options[Option].IsNegotiatedSuccessfully())
            {
                Debug.WriteLine("(WILL?)OptionCode: " + Option.ToString() + "(0x" + Option.ToString("X") + ") negotiated OK.");
                byte[] Response = _Options[Option].GetNegotiationResponsePacket(TelnetOption.OptionNegotiationType.WILL);
                if (Response != null)
                    SendToNetwork(Response);
            }

            if (_Options[Option].IsNegotiatedSuccessfully() && !_Options[Option].IsOptionInitializationSent)
            {
                OptionNegotiated(Option);
                _Options[Option].SendOptionInitailization();
            }
        }

        protected void _ProcessWontCommand(byte Option)
        {

            if (_Options.ContainsKey(Option) == false)
            {
                Debug.WriteLine("(WONT?)OptionCode: " + Option.ToString() + "(0x" + Option.ToString("X") + ") not supported.");
                SendToNetwork(TelnetOption.GetNegotiationResponseDont(Option));
                return;
            }
            if (!_Options[Option].IsNegotiatedSuccessfully())
            {
                Debug.WriteLine("(WONT?)OptionCode: " + Option.ToString() + "(0x" + Option.ToString("X") + ") negotiated OK.");
                byte[] Response = _Options[Option].GetNegotiationResponsePacket(TelnetOption.OptionNegotiationType.WONT);
                if (Response != null)
                    SendToNetwork(Response);
            }
        }

        protected void _ProcessDoCommand(byte Option)
        {
            if (_Options.ContainsKey(Option) == false)
            {
                Debug.WriteLine("(DO?)OptionCode: " + Option.ToString() + "(0x" + Option.ToString("X") + ") not supported.");
                SendToNetwork(TelnetOption.GetNegotiationResponseWont(Option));
                return;
            }
            if (!_Options[Option].IsNegotiatedSuccessfully())
            {
                Debug.WriteLine("(DO?)OptionCode: " + Option.ToString() + "(0x" + Option.ToString("X") + ") negotiated OK.");
                byte[] Response = _Options[Option].GetNegotiationResponsePacket(TelnetOption.OptionNegotiationType.DO);
                if (Response != null)
                    SendToNetwork(Response);
            }
            if (_Options[Option].IsNegotiatedSuccessfully() && !_Options[Option].IsOptionInitializationSent)
            {
                OptionNegotiated(Option);
                _Options[Option].SendOptionInitailization();
            }
        }

        protected void _ProcessDontCommand(byte Option)
        {
            if (_Options.ContainsKey(Option) == false)
            {
                Debug.WriteLine("(DONT?)OptionCode: " + Option.ToString() + "(0x" + Option.ToString("X") + ") not supported.");
                SendToNetwork(TelnetOption.GetNegotiationResponseWont(Option));
                return;
            }
            if (!_Options[Option].IsNegotiatedSuccessfully())
            {
                Debug.WriteLine("(DONT?)OptionCode: " + Option.ToString() + "(0x" + Option.ToString("X") + ") negotiated OK.");
                byte[] Response = _Options[Option].GetNegotiationResponsePacket(TelnetOption.OptionNegotiationType.DONT);
                if (Response != null)
                    SendToNetwork(Response);
            }
        }

        protected bool _ProcessOption(SocketClient client, byte Option, byte[] OptionData, int Index, out int ProcessedLength)
        {
            bool OptionOK = false;
            byte Data;
            TelnetState InternalState = TelnetState.Normal;
            int OptionRelatedDataLength = 0, i;

            ProcessedLength = 0;

            for( i = Index; i < OptionData.Length; i++ )
            {
                Data = OptionData[i];
                switch (InternalState)
                {
                    case TelnetState.IAC:
                        if (Data == (byte)TelnetCommand.SE)
                        {
                            OptionOK = true;
                            break;
                        }
                        InternalState = TelnetState.Normal;
                        break;
                    default:
                        if (Data == (byte)TelnetCommand.IAC)
                        {
                            InternalState = TelnetState.IAC;
                        }

                        break;
                }

                if (OptionOK == true) break;
            }

            if (OptionOK == false) return false;

            OptionRelatedDataLength = (i - Index) - 1;

            ProcessedLength = OptionRelatedDataLength;

            if (!_Options[Option].ProcessOptionRelatedCommand(client, OptionData, Index, OptionRelatedDataLength))
            {
                return false;
            }

            return OptionOK;
        }
    }
}
