using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Telnet
{


    public class TelnetOption
    {
        public static byte[] GetNegotiationResponseWill( byte OptionCode ) 
        {
            byte[] Response = new byte[] { (byte) TelnetCommand.IAC,
                                           (byte) TelnetCommand.WILL,
                                           OptionCode };

            return Response; 
        }

        public static byte[] GetNegotiationResponseWont(byte OptionCode)
        {
            byte[] Response = new byte[] { (byte) TelnetCommand.IAC,
                                           (byte) TelnetCommand.WONT,
                                           OptionCode };

            return Response;
        }

        public static byte[] GetNegotiationResponseDo(byte OptionCode)
        {
            byte[] Response = new byte[] { (byte) TelnetCommand.IAC,
                                           (byte) TelnetCommand.DO,
                                           OptionCode };

            return Response;
        }

        public static byte[] GetNegotiationResponseDont(byte OptionCode)
        {
            byte[] Response = new byte[] { (byte) TelnetCommand.IAC,
                                           (byte) TelnetCommand.DONT,
                                           OptionCode };

            return Response;
        }

        public enum OptionNegotiationType
        {
            WILL,
            WONT,
            DO,
            DONT
        }

        public TelnetOption(byte Option)
        {
            _bOptionSupported = true;
            SendDoDont = true;
            OptionCode = Option;
            Reset();
        }
                
        TelnetOption(byte Option, bool bSupported )
        {
            _bOptionSupported = bSupported;
            SendDoDont = true;
            OptionCode = Option;
            Reset();
        }

        public delegate void OnSendDataHandler(object sender, byte[] DataToSend);
        public virtual event OnSendDataHandler OnSendData;

        bool _bOptionSupported;
        bool _bWILL = false;
        bool _bDO = false;
        
        public bool SendWillWont { get; set; }
        public bool SendDoDont { get; set; }
        public byte OptionCode { get; private set; }
        public bool IsOptionInitializationSent { get; protected set; }

        public bool RespondedWillWont { get; set; }
        public bool RespondedDoDont { get; set; }

        public virtual void Reset()
        {
            _bDO = _bOptionSupported;
            _bWILL = _bOptionSupported;
            RespondedWillWont = false;
            RespondedDoDont = false;
            IsOptionInitializationSent = false;
        }

        public bool IsNegotiated() { return (RespondedWillWont || RespondedDoDont); }

        public bool IsNegotiatedSuccessfully() 
        {
            if (RespondedWillWont || RespondedDoDont)
            {
                if ((SendDoDont && RespondedWillWont) && _bWILL)
                    return true;
                if (SendWillWont && RespondedDoDont && _bDO)
                    return true;
            }

            return false;
        }

        public byte[] GetNegotiationPacket()
        {
            byte[] ResponsePacket = null;

            if (SendDoDont)
            {
                if ( _bDO )
                    ResponsePacket = TelnetOption.GetNegotiationResponseDo(OptionCode);
                else
                    ResponsePacket = TelnetOption.GetNegotiationResponseDont(OptionCode);
            }
            else if ( SendWillWont)
            {
                if (_bWILL)
                    ResponsePacket = TelnetOption.GetNegotiationResponseWill(OptionCode);
                else
                    ResponsePacket = TelnetOption.GetNegotiationResponseWont(OptionCode);
            }

            return ResponsePacket;
        }

        public byte[] GetNegotiationResponsePacket(OptionNegotiationType RequestType)
        {
            byte[] ResponsePacket = null;

            switch (RequestType)
            {
                case OptionNegotiationType.WILL:
                    if (RespondedWillWont == false)
                    {
                        if (_bWILL)
                        {
                            ResponsePacket = TelnetOption.GetNegotiationResponseDo(OptionCode);

                        }
                        else
                        {
                            ResponsePacket = TelnetOption.GetNegotiationResponseDont(OptionCode);
                        }
                        RespondedDoDont = true;
                    }
                    break;
                case OptionNegotiationType.WONT:
                    if (RespondedWillWont == false)
                    {
                        _bWILL = false;
                        ResponsePacket = TelnetOption.GetNegotiationResponseDont(OptionCode);
                        RespondedDoDont = true;
                    }
                    break;
                case OptionNegotiationType.DO:
                    if (RespondedDoDont == false)
                    {
                        if (_bDO)
                        {
                            ResponsePacket = TelnetOption.GetNegotiationResponseWill(OptionCode);

                        }
                        else
                        {
                            ResponsePacket = TelnetOption.GetNegotiationResponseWont(OptionCode);
                        }
                        RespondedWillWont = true;
                    }
                    break;
                case OptionNegotiationType.DONT:
                    if (RespondedDoDont == false)
                    {
                        _bDO = false;
                        ResponsePacket = TelnetOption.GetNegotiationResponseWont(OptionCode);
                        RespondedWillWont = true;
                    }
                    break;
                default:
                    break;
            }

            return ResponsePacket;
        }

        public virtual bool SendOptionInitailization()
        {
            IsOptionInitializationSent = true;
            return true;
        }

        public virtual bool ProcessOptionRelatedCommand(SocketClient client, byte[] Data, int Index, int OptionRelatedDataLength)
        {
            return true;
        }
    }
}
