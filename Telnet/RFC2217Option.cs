using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Telnet
{
    public class RFC2217FlowControl
    {
        public RFC2217FlowControl()
        {
            RtsInbound = false;
            CtsOutbound = false;
            DsrOutbound = false;
            DtrInbound = false;
            DcdOutbound = false;
            XonXoffInbound = false;
            XonXoffOutbound = false;
        }

        public delegate void OnSettingChangedHandler(object sender);
        public event OnSettingChangedHandler OnSettingChanged;

       
        private bool _RtsInbound;
        private bool _CtsOutbound;
        private bool _DsrOutbound;
        private bool _DtrInbound;
        private bool _DcdOutbound;
        private bool _XonXoffInbound;
        private bool _XonXoffOutbound;        

        
        public bool RtsInbound 
        {
            get { return _RtsInbound; }

            set
            {
                _RtsInbound = value;
                if (OnSettingChanged != null)
                    OnSettingChanged(this);
            }
        }
        public bool CtsOutbound
        {
            get { return _CtsOutbound; }

            set
            {
                _CtsOutbound = value;
                if (OnSettingChanged != null)
                    OnSettingChanged(this);
            }
        }
        public bool DsrOutbound
        {
            get { return _DsrOutbound; }

            set
            {
                _DsrOutbound = value;
                if (OnSettingChanged != null)
                    OnSettingChanged(this);
            }
        }
        public bool DtrInbound
        {
            get { return _DtrInbound; }

            set
            {
                _DtrInbound = value;
                if (OnSettingChanged != null)
                    OnSettingChanged(this);
            }
        }
        public bool DcdOutbound
        {
            get { return _DcdOutbound; }

            set
            {
                _DcdOutbound = value;
                if (OnSettingChanged != null)
                    OnSettingChanged(this);
            }
        }
        public bool XonXoffInbound
        {
            get { return _XonXoffInbound; }

            set
            {
                _XonXoffInbound = value;
                if (OnSettingChanged != null)
                    OnSettingChanged(this);
            }
        }
        public bool XonXoffOutbound
        {
            get { return _XonXoffOutbound; }

            set
            {
                _XonXoffOutbound = value;
                if (OnSettingChanged != null)
                    OnSettingChanged(this);
            }
        }

        public void SetNoFlowControl()
        {
            RtsInbound = false;
            CtsOutbound = false;
            DsrOutbound = false;
            DtrInbound = false;
            DcdOutbound = false;
            XonXoffInbound = false;
            XonXoffOutbound = false;

            if (OnSettingChanged != null)
                OnSettingChanged(this);
        }

        public void SetHardwareFlowControl()
        {
            RtsInbound = true;
            CtsOutbound = true;
            DsrOutbound = false;
            DtrInbound = false;
            DcdOutbound = false;
            XonXoffInbound = false;
            XonXoffOutbound = false;
            if (OnSettingChanged != null)
                OnSettingChanged(this);
        }

        public void SetXonXoffFlowControl()
        {
            RtsInbound = false;
            CtsOutbound = false;
            DsrOutbound = false;
            DtrInbound = false;
            DcdOutbound = false;
            XonXoffInbound = true;
            XonXoffOutbound = true;
            if (OnSettingChanged != null)
                OnSettingChanged(this);
        }

        public void SetDtrDsrFlowControl()
        {
            RtsInbound = false;
            CtsOutbound = false;
            DsrOutbound = true;
            DtrInbound = true;
            DcdOutbound = false;
            XonXoffInbound = false;
            XonXoffOutbound = false;
            if (OnSettingChanged != null)
                OnSettingChanged(this);
        }

        public bool IsNoFlowControl()
        {
            if (RtsInbound || CtsOutbound ||
                 DtrInbound || DsrOutbound ||
                 XonXoffInbound || XonXoffOutbound ||
                 DcdOutbound)
                return false;

            return true;
        }

        public bool IsHardwareFlowControl()
        {
            if (RtsInbound && CtsOutbound &&
                 !DtrInbound && !DsrOutbound &&
                 !XonXoffInbound && !XonXoffOutbound && !DcdOutbound)
                return true;

            return false;
        }

        public bool IsDtrDsrFlowControl()
        {
            if (DtrInbound && DsrOutbound &&
                 !RtsInbound && !CtsOutbound &&
                 !XonXoffInbound && !XonXoffOutbound && !DcdOutbound)
                return true;

            return false;
        }

        public bool IsXonXoffFlowControl()
        {
            if (XonXoffInbound && XonXoffOutbound &&
                 !RtsInbound && !CtsOutbound &&
                 !DtrInbound && !DsrOutbound && !DcdOutbound)
                return true;

            return false;
        }
    }

    public class RFC2217Option: TelnetOption
    {
        const int RFC2217OptionCode = 44;

        public enum Command {
            SIGNATURE = 0,
            SET_BAUDRATE = 1,
            SET_DATASIZE = 2,
            SET_PARITY = 3,
            SET_STOPSIZE = 4,
            SET_CONTROL = 5,
            NOTIFY_LINESTATE = 6,
            NOTIFY_MODEMSTATE = 7,
            FLOWCONTROL_SUSPEND = 8,
            FLOWCONTROL_RESUME = 9,
            SET_LINESTATE_MASK = 10,
            SET_MODEMSTATE_MASK = 11,
            PURGE_DATA = 12,
        }
        public enum ControlCommand
        {
            REQUEST_FLOWCONTROL = 0,
            USE_NO_FLOWCONTROL = 1,
            USE_XONXOFF_OUTBOUND_FLOWCONTROL = 2,
            USE_HARDWARE_OUTBOUND_FLOWCONTROL = 3,
            REQUEST_BREAK_STATE = 4,
            SET_BREAK_STATE = 5,
            CLEAR_BREAK_STATE = 6,
            REQUEST_DTR_STATE = 7,
            SET_DTR_STATE = 8,
            CLEAR_DTR_STATE = 9,
            REQUEST_RTS_STATE = 10,
            SET_RTS_STATE = 11,
            CLEAR_RTS_STATE = 12,
            REQUEST_INBOUND_FLOWCONTROL = 13,
            USE_NO_INBOUND_FLOWCONTROL = 14,
            USE_XONXOFF_INBOUND_FLOWCONTROL = 15,
            USE_HARDWARE_INBOUND_FLOWCONTROL = 16,
            USE_DCD_OUTBOUND_FLOWCONTROL = 17,
            USE_DTR_INBOUND_FLOWCONTROL = 18,
            USE_DSR_OUTBOUND_FLOWCONTROL = 19,
        }
        public enum ParityType
        {
            Request = 0,
            None = 1,
            Odd = 2,
            Even = 3,
            Mark = 4,
            Space = 5,
        }
        public enum StopBitsType
        {
            Request = 0,
            One = 1,
            Two = 2,
            OnePointFive = 3,
        }

        [Flags]
        public enum ModemState : byte
        {
            CTS = 0x10,
            DSR = 0x20,
            RI = 0x40,
            RLSD = 0x80,
        }

        [Flags]
        public enum LineState : byte
        {
            DataReady = 0x1,
            Overrun = 0x2,
            ParityError = 0x4,
            FramingError = 0x8,
            BreakDetected = 0x10,
            THRE = 0x20,              // Transfer(Transmit) Holding Register Empty
            TEMT = 0x40,            // Transfer Shift Register (Transmitter) Empty
            TimeoutError = 0x80,
        }

        public delegate void OnLogHandler(string Msg, LogLevel LogLevel);
        public virtual event OnLogHandler OnLog;

        private void _Log(string Msg, LogLevel LogLevel = LogLevel.Information) {
            if ( OnLog != null )
                OnLog(Msg, LogLevel);
        }
        private void _LogError(string Msg) {
            if ( OnLog != null )
                OnLog(Msg, LogLevel.Error);
        }
        private void _LogDebug(string Msg) {
            if ( OnLog != null )
                OnLog(Msg, LogLevel.Debug);
        }        
        private void _LogWarning(string Msg) {
            if ( OnLog != null )
                OnLog(Msg, LogLevel.Warning);
        }
        private void _LogCritical(string Msg) {
            if ( OnLog != null )
                OnLog(Msg, LogLevel.Critical);
        }

        public RFC2217Option(bool bServer)
            : base(RFC2217OptionCode)
        {
            Server = bServer;
            FlowControl = new RFC2217FlowControl();
            FlowControl.OnSettingChanged += new RFC2217FlowControl.OnSettingChangedHandler(FlowControl_OnSettingChanged);
        }
        
        public delegate void OnSignatureChangedHandler(object sender);
        public delegate void OnBaudRateChangedHandler(object sender);
        public delegate void OnDataSizeChangedHandler(object sender);
        public delegate void OnParityChangedHandler(object sender);
        public delegate void OnStopBitsChangedHandler(object sender);
        public delegate void OnModemStateMaskChangedHandler(object sender);
        public delegate void OnLineStateMaskChangedHandler(object sender);
        public delegate void OnSuspendStateChangedHandler(object sender);
        public delegate void OnFlowControlChangedHandler(object sender, RFC2217FlowControl FlowControl);
        public delegate void OnBreakStateChangedHandler(object sender);
        public delegate void OnRtsSignalChangedHandler(object sender);
        public delegate void OnDtrSignalStateChangedHandler(object sender);
        public delegate void OnPurgeDataHandler(object sender, bool PurgeTransmitBuffer, bool PurgeReceiveBuffer );
        public delegate void OnModemStateChangedHandler(object sender, bool Cts, bool Dsr, bool Ri, bool Rlsd);
        public delegate void OnLineStateChangedHandler(  object sender,
                                                         bool DataReady,
                                                         bool Overrun,
                                                         bool ParityError,
                                                         bool FramingError,
                                                         bool BreakDetected,
                                                         bool THRE,
                                                         bool TEMT,
                                                         bool TimeoutError);
        public event OnSignatureChangedHandler OnSignatureChanged;
        public event OnBaudRateChangedHandler OnBaudRateChanged;
        public event OnDataSizeChangedHandler OnDataSizeChanged;
        public event OnParityChangedHandler OnParityChanged;
        public event OnStopBitsChangedHandler OnStopBitsChanged;
        public event OnModemStateMaskChangedHandler OnModemStateMaskChanged;
        public event OnLineStateMaskChangedHandler OnLineStateMaskChanged;
        public event OnSuspendStateChangedHandler OnSuspendStateChanged;
        public event OnFlowControlChangedHandler OnFlowControlChanged;
        public event OnBreakStateChangedHandler OnBreakStateChanged;
        public event OnRtsSignalChangedHandler OnRtsSignalChanged;
        public event OnDtrSignalStateChangedHandler OnDtrSignalStateChanged;
        public event OnPurgeDataHandler OnPurgeData;
        public event OnModemStateChangedHandler OnModemStateChanged;
        public event OnLineStateChangedHandler OnLineStateChanged;


        public override event OnSendDataHandler OnSendData;

        private string _Signature = "N/A";
        private UInt32 _BaudRate = 115200;
        private byte _DataSize = 8;
        private byte _Parity = (byte)ParityType.None;
        private byte _StopBits = (byte)StopBitsType.One;
        private byte _LineStateMask = 0x00;
        private byte _ModemStateMask = 0xFF;
        private bool _Suspend = false;
        private bool _BreakState = false;
        private bool _DtrEnable = false;
        private bool _RtsEnable = false;
        private byte _LastModemState = 0x00;
        private byte _LastLineState = 0x00;

        public bool Server { get; set; }
        public string Signature 
        {
            get
            {
                return _Signature;
            }
            set
            {
                if (!IsNegotiated()) return;

                if (_SetSignature(value))
                    _Signature = value;
            }
        }

        public UInt32 BaudRate 
        {
            get { return _BaudRate; }
            set
            {
                if (!IsNegotiated()) return;

                if (_SetBaudRate(value))
                    _BaudRate = value;
            }
        }

        public byte DataSize
        {
            get { return _DataSize; }
            set
            {
                if (!IsNegotiated()) return;

                if (_DataSize < 5 || _DataSize > 8) throw new ArgumentOutOfRangeException("DataSize out of range.");

                if (_SetDataSize(value))
                    _DataSize = value;
            }
        }

        public byte Parity
        {
            get { return _Parity; }
            set
            {
                if (!IsNegotiated()) return;

                //if (_Parity < 5 || _Parity > 8) throw ArgumentOutOfRangeException("DataSize out of range.");

                if (_SetParity(value))
                    _Parity = value;
            }
        }

        public byte StopBits
        {
            get { return _StopBits; }
            set
            {
                if (!IsNegotiated()) return;

                //if (_StopBits < 5 || _StopBits > 8) throw ArgumentOutOfRangeException("DataSize out of range.");

                if (_SetStopBits(value))
                    _StopBits = value;
            }
        }

        public byte LineStateMask
        {
            get { return _LineStateMask; }
            set
            {
                if (!IsNegotiated()) return;

                //if (_StopBits < 5 || _StopBits > 8) throw ArgumentOutOfRangeException("DataSize out of range.");

                if (_SetLineStateMask(value))
                    _LineStateMask = value;
            }
        }

        public byte ModemStateMask
        {
            get { return _ModemStateMask; }
            set
            {
                if (!IsNegotiated()) return;

                //if (_StopBits < 5 || _StopBits > 8) throw ArgumentOutOfRangeException("DataSize out of range.");

                if (_SetModemStateMask(value))
                    _ModemStateMask = value;
            }
        }

        public bool Suspend
        {
            get { return _Suspend; }
            set
            {
                if (!IsNegotiated()) return;

                //if (_StopBits < 5 || _StopBits > 8) throw ArgumentOutOfRangeException("DataSize out of range.");

                if (_SetSuspend(value))
                    _Suspend = value;
            }
        }
        
        public RFC2217FlowControl FlowControl { get; set; }
        public bool BreakState
        {
            get { return _BreakState; }
            set
            {
                if (!IsNegotiated()) return;

                if (_SetBreakState(value))
                    _BreakState = value;
            }
        }
        public bool DtrEnable
        {
            get { return _DtrEnable; }
            set
            {
                if (!IsNegotiated()) return;

                if (_SetDtrEnable(value))
                    _DtrEnable = value;
            }
        }
        public bool RtsEnable
        {
            get { return _RtsEnable; }
            set
            {
                if (!IsNegotiated()) return;

                if (_SetRtsEnable(value))
                    _RtsEnable = value;
            }
        }

        public override void Reset()
        {
            base.Reset();
        }

        public override bool SendOptionInitailization()
        {
            IsOptionInitializationSent = true;

            _LineStateMask = 0xFF;
            _ModemStateMask = 0xFF;

            SetModemState(false, false, false, false);
            SetLineStatus(false, false, false, false, false, true, true, false);

            _SetBaudRate(BaudRate);
            _SetDataSize(DataSize);
            _SetParity(Parity);
            _SetStopBits(StopBits);
            _SetFlowControl();
            _SetDtrEnable(DtrEnable);
            _SetRtsEnable(RtsEnable);


            return true;
        } 

        public override bool ProcessOptionRelatedCommand( SocketClient client, byte[] Data, int Index, int OptionRelatedDataLength)
        {
            int i = Index;
            Command CurrentCommand;
            List<byte> Response = new List<byte>();
            bool NeedResponse = false;

            //if (Data[i++] != RFC2217OptionCode)
            //{
            //    return false;
            //}
            //OptionRelatedDataLength--;

            CurrentCommand = (Command)(Data[i++] %100);
            OptionRelatedDataLength--;

            switch (CurrentCommand)
            {
                case Command.SIGNATURE:
                    System.Text.Encoding enc = System.Text.Encoding.ASCII;
                    _Signature = enc.GetString(Data, i, OptionRelatedDataLength);
                    _LogDebug("SIGNATURE" + Signature);
                    if (OnSignatureChanged != null)
                        OnSignatureChanged(this);
                    break;
                case Command.SET_BAUDRATE:
                    UInt32 temp;
                    Debug.Assert(OptionRelatedDataLength == 4);                    
                    temp = BitConverter.ToUInt32(Data, i);
                    byte[] revtemp = BitConverter.GetBytes(temp).ToArray();
                    Array.Reverse(revtemp);
                    temp = BitConverter.ToUInt32(revtemp, 0);
                    if (temp != 0)
                    {
                        _BaudRate = temp;
                        _LogDebug("SET_BAUDRATE: " + BaudRate.ToString());
                        if (OnBaudRateChanged != null)
                            OnBaudRateChanged(this);
                    }
                    else
                        NeedResponse = true;
                    break;
                case Command.SET_DATASIZE:
                    Debug.Assert(OptionRelatedDataLength == 1);
                    if (Data[i] != 0)
                    {
                        _DataSize = Data[i];
                        _LogDebug("SET_DATASIZE: " + DataSize.ToString());
                        if (OnDataSizeChanged != null)
                            OnDataSizeChanged(this);
                    }
                    else
                        NeedResponse = true;
                    break;
                case Command.SET_PARITY:
                    Debug.Assert(OptionRelatedDataLength == 1);
                    if (Data[i] != (byte)ParityType.Request)
                    {
                        _Parity = Data[i];
                        _LogDebug("SET_PARITY: " + Parity.ToString());
                        if (OnParityChanged != null)
                            OnParityChanged(this);
                    }
                    else
                        NeedResponse = true;
                    break;
                case Command.SET_STOPSIZE:
                    Debug.Assert(OptionRelatedDataLength == 1);
                    if (Data[i] != (byte)StopBitsType.Request)
                    {
                        _StopBits = Data[i];
                        _LogDebug("SET_STOPSIZE: " + StopBits.ToString());
                        if (OnStopBitsChanged != null)
                            OnStopBitsChanged(this);
                    }
                    else
                        NeedResponse = true;
                    break;
                case Command.SET_CONTROL:
                    
                    Debug.Assert(OptionRelatedDataLength == 1);

                    switch ((ControlCommand)Data[i])
                    {
                        case ControlCommand.REQUEST_FLOWCONTROL:
                            NeedResponse = true;
                            break;
                        case ControlCommand.USE_NO_FLOWCONTROL:
                            FlowControl.SetNoFlowControl();
                            _LogDebug("SET_CONTROL->: USE_NO_FLOWCONTROL");
                            if (OnFlowControlChanged != null)
                                OnFlowControlChanged(this, FlowControl);
                            break;
                        case ControlCommand.USE_XONXOFF_OUTBOUND_FLOWCONTROL:
                            FlowControl.SetXonXoffFlowControl();
                            _LogDebug("SET_CONTROL->: USE_XONXOFF_OUTBOUND_FLOWCONTROL");
                            if (OnFlowControlChanged != null)
                                OnFlowControlChanged(this, FlowControl);
                            break;
                        case ControlCommand.USE_HARDWARE_OUTBOUND_FLOWCONTROL:
                            FlowControl.SetHardwareFlowControl();
                            _LogDebug("SET_CONTROL->: USE_HARDWARE_OUTBOUND_FLOWCONTROL");
                            if (OnFlowControlChanged != null)
                                OnFlowControlChanged(this, FlowControl);
                            break;
                        case ControlCommand.REQUEST_BREAK_STATE:
                            NeedResponse = true;
                            break;
                        case ControlCommand.SET_BREAK_STATE:
                            _BreakState = true;
                            _LogDebug("SET_CONTROL->: SET_BREAK_STATE");
                            if (OnBreakStateChanged != null)
                                OnBreakStateChanged(this);
                            break;
                        case ControlCommand.CLEAR_BREAK_STATE:
                            _BreakState = false;
                            _LogDebug("SET_CONTROL->: CLEAR_BREAK_STATE");
                            if (OnBreakStateChanged != null)
                                OnBreakStateChanged(this);
                            break;
                        case ControlCommand.REQUEST_DTR_STATE:
                            NeedResponse = true;
                            break;
                        case ControlCommand.SET_DTR_STATE:
                            _DtrEnable = true;
                            _LogDebug("SET_CONTROL->: SET_DTR_STATE");
                            if (OnDtrSignalStateChanged != null)
                                OnDtrSignalStateChanged(this);
                            break;
                        case ControlCommand.CLEAR_DTR_STATE:
                            _DtrEnable = false;
                            _LogDebug("SET_CONTROL->: CLEAR_DTR_STATE");
                            if (OnDtrSignalStateChanged != null)
                                OnDtrSignalStateChanged(this);
                            break;
                        case ControlCommand.REQUEST_RTS_STATE:
                            NeedResponse = true;
                            break;
                        case ControlCommand.SET_RTS_STATE:
                            _RtsEnable = true;
                            _LogDebug("SET_CONTROL->: SET_RTS_STATE");
                            if (OnRtsSignalChanged != null)
                                OnRtsSignalChanged(this);
                            break;
                        case ControlCommand.CLEAR_RTS_STATE:
                            _RtsEnable = false;
                            _LogDebug("SET_CONTROL->: CLEAR_RTS_STATE");
                            if (OnRtsSignalChanged != null)
                                OnRtsSignalChanged(this);
                            break;
                        case ControlCommand.REQUEST_INBOUND_FLOWCONTROL:
                            NeedResponse = true;
                            break;
                        case ControlCommand.USE_NO_INBOUND_FLOWCONTROL:
                            FlowControl.RtsInbound = false;
                            FlowControl.DtrInbound = false;
                            FlowControl.XonXoffInbound = false;
                            _LogDebug("SET_CONTROL->: USE_NO_INBOUND_FLOWCONTROL");
                            if (OnFlowControlChanged != null)
                                OnFlowControlChanged(this, FlowControl);
                            break;
                        case ControlCommand.USE_XONXOFF_INBOUND_FLOWCONTROL:
                            FlowControl.XonXoffInbound = true;
                            _LogDebug("SET_CONTROL->: USE_XONXOFF_INBOUND_FLOWCONTROL");
                            if (OnFlowControlChanged != null)
                                OnFlowControlChanged(this, FlowControl);
                            break;
                        case ControlCommand.USE_HARDWARE_INBOUND_FLOWCONTROL:
                            FlowControl.RtsInbound = true;
                            _LogDebug("SET_CONTROL->: USE_HARDWARE_INBOUND_FLOWCONTROL");
                            if (OnFlowControlChanged != null)
                                OnFlowControlChanged(this, FlowControl);
                            break;
                        case ControlCommand.USE_DCD_OUTBOUND_FLOWCONTROL:
                            FlowControl.DcdOutbound = true;
                            _LogDebug("SET_CONTROL->: USE_DCD_OUTBOUND_FLOWCONTROL");
                            if (OnFlowControlChanged != null)
                                OnFlowControlChanged(this, FlowControl);
                            break;
                        case ControlCommand.USE_DTR_INBOUND_FLOWCONTROL:
                            FlowControl.DtrInbound = true;
                            _LogDebug("SET_CONTROL->: USE_DTR_INBOUND_FLOWCONTROL");
                            if (OnFlowControlChanged != null)
                                OnFlowControlChanged(this, FlowControl);
                            break;
                        case ControlCommand.USE_DSR_OUTBOUND_FLOWCONTROL:
                            FlowControl.SetDtrDsrFlowControl();
                            _LogDebug("SET_CONTROL->: USE_DSR_OUTBOUND_FLOWCONTROL");
                            if (OnFlowControlChanged != null)
                                OnFlowControlChanged(this, FlowControl);
                            break;
                        default:
                            break;
                    }
                    break;
                case Command.FLOWCONTROL_SUSPEND:
                    Debug.Assert(OptionRelatedDataLength == 0);
                    _Suspend = true;
                    _LogDebug("FLOWCONTROL_SUSPEND: Flow Suspended");
                    if (OnSuspendStateChanged != null)
                        OnSuspendStateChanged(this);
                    break;
                case Command.FLOWCONTROL_RESUME:
                    Debug.Assert(OptionRelatedDataLength == 0);
                    _Suspend = false;
                    _LogDebug("FLOWCONTROL_SUSPEND: Flow Suspended");
                    if (OnSuspendStateChanged != null)
                        OnSuspendStateChanged(this);
                    break;
                case Command.SET_LINESTATE_MASK:
                    Debug.Assert(OptionRelatedDataLength == 1);
                    _LineStateMask = Data[i];
                    _LogDebug("SET_STOPSIZE: 0x" + LineStateMask.ToString("X"));
                    if (OnLineStateMaskChanged != null)
                        OnLineStateMaskChanged(this);
                    break;
                case Command.SET_MODEMSTATE_MASK:
                    Debug.Assert(OptionRelatedDataLength == 1);
                    _ModemStateMask = Data[i];
                    _LogDebug("SET_STOPSIZE: 0x" + LineStateMask.ToString("X"));
                    if (OnModemStateMaskChanged != null)
                        OnModemStateMaskChanged(this);
                    break;
                case Command.PURGE_DATA:
                    switch(Data[1]) 
                    {
                        case 1:
                            OnPurgeData(this, false, true);
                            break;
                        case 2:
                            OnPurgeData(this, true, false);
                            break;
                        case 3:
                            OnPurgeData(this, true, true);
                            break;
                        default:
                            break;
                    }
                    
                    break;
                case Command.NOTIFY_LINESTATE:    
                    bool DataReady, Overrun, ParityError, FramingError, BreakDetected, THRE, TEMT, TimeoutError;

                    _LastLineState = Data[i];
                    DataReady = (Data[i] & (byte)LineState.DataReady) != 0 ? true : false;
                    Overrun = (Data[i] & (byte)LineState.Overrun) != 0? true:false;
                    ParityError = (Data[i] & (byte)LineState.ParityError) != 0 ? true : false;
                    FramingError = (Data[i] & (byte)LineState.FramingError) != 0 ? true : false;
                    BreakDetected = (Data[i] & (byte)LineState.BreakDetected) != 0 ? true : false;
                    THRE = (Data[i] & (byte)LineState.THRE) != 0 ? true : false;
                    TEMT = (Data[i] & (byte)LineState.TEMT) != 0 ? true : false;
                    TimeoutError = (Data[i] & (byte)LineState.TimeoutError) != 0 ? true : false;

                    _LogDebug("NOTIFY_LINESTATE: DataReady:" + DataReady.ToString() + " Overrun:" + Overrun.ToString() +
                                        " ParityError:" + ParityError.ToString() + " FramingError:" + FramingError.ToString());
                    _LogDebug("                  BreakDetected:" + BreakDetected.ToString() + " THRE:" + THRE.ToString() +
                                        " TEMT:" + TEMT.ToString() + " TimeoutError:" + TimeoutError.ToString());
                    
                    if (OnLineStateChanged != null)
                        OnLineStateChanged(this, DataReady, Overrun, ParityError, FramingError, BreakDetected, THRE, TEMT, TimeoutError);

                    break;
                case Command.NOTIFY_MODEMSTATE:
                    bool Cts, Dsr, Ri, Rlsd;

                    _LastModemState = Data[i];
                    Cts = (Data[i] & (byte)ModemState.CTS) != 0 ? true : false;
                    Dsr = (Data[i] & (byte)ModemState.DSR) != 0 ? true : false;
                    Ri = (Data[i] & (byte)ModemState.RI) != 0 ? true : false;
                    Rlsd = (Data[i] & (byte)ModemState.RLSD) != 0 ? true : false;
                    _LogDebug("NOTIFY_MODEMSTATE: Cts:" + Cts.ToString() + " Dsr:" + Dsr.ToString() +
                                        " Ri:" + Ri.ToString() + " Rlsd:" + Rlsd.ToString());
                    if (OnModemStateChanged != null)
                        OnModemStateChanged(this, Cts, Dsr, Ri, Rlsd);

                    break;
               default:
                    break;
            }

            if (NeedResponse) {
                bool ResponseOK = true;
                Response.Add((byte)Telnet.TelnetCommand.IAC);
                Response.Add((byte)Telnet.TelnetCommand.SB);
                Response.Add((byte)RFC2217OptionCode);
                Response.Add((byte)(CurrentCommand + 100));

                switch (CurrentCommand)
                {
                    case Command.SIGNATURE:
                        foreach (char c in Signature.ToCharArray())
                            Response.Add((byte)c);                        
                        break;
                    case Command.SET_BAUDRATE:
                        Response.AddRange(BitConverter.GetBytes(BaudRate));                        
                        break;
                    case Command.SET_DATASIZE:
                        Response.Add(DataSize);
                        break;
                    case Command.SET_PARITY:
                        Response.Add(Parity);
                        break;
                    case Command.SET_STOPSIZE:
                        Response.Add(StopBits);
                        break;
                    case Command.SET_CONTROL:
                        switch ((ControlCommand)Data[i])
                        {
                            case ControlCommand.REQUEST_FLOWCONTROL:
                                if (FlowControl.CtsOutbound ==false &&
                                    FlowControl.XonXoffOutbound == false &&
                                    FlowControl.DsrOutbound == false &&
                                    FlowControl.DcdOutbound == false)
                                {
                                    Response.Add((byte)ControlCommand.USE_NO_FLOWCONTROL);
                                }
                                else if (FlowControl.CtsOutbound)
                                {
                                    Response.Add((byte)ControlCommand.USE_HARDWARE_OUTBOUND_FLOWCONTROL);
                                }
                                else if (FlowControl.XonXoffOutbound)
                                {
                                    Response.Add((byte)ControlCommand.USE_XONXOFF_OUTBOUND_FLOWCONTROL);
                                }
                                else if (FlowControl.DsrOutbound)
                                {
                                    Response.Add((byte)ControlCommand.USE_DSR_OUTBOUND_FLOWCONTROL);
                                }
                                else if (FlowControl.DcdOutbound )
                                {
                                    Response.Add((byte)ControlCommand.USE_DCD_OUTBOUND_FLOWCONTROL);
                                }
                                
                                break;
                            case ControlCommand.USE_NO_FLOWCONTROL:
                                Response.Add((byte)ControlCommand.USE_NO_FLOWCONTROL);
                                break;
                            case ControlCommand.USE_XONXOFF_OUTBOUND_FLOWCONTROL:
                                Response.Add((byte)ControlCommand.USE_XONXOFF_OUTBOUND_FLOWCONTROL);
                                break;
                            case ControlCommand.USE_HARDWARE_OUTBOUND_FLOWCONTROL:
                                Response.Add((byte)ControlCommand.USE_HARDWARE_OUTBOUND_FLOWCONTROL);
                                break;
                            case ControlCommand.REQUEST_BREAK_STATE:
                                if ( BreakState )
                                    Response.Add((byte)ControlCommand.SET_BREAK_STATE);
                                else
                                    Response.Add((byte)ControlCommand.CLEAR_BREAK_STATE);
                                break;
                            case ControlCommand.SET_BREAK_STATE:
                                Response.Add(Data[i]);
                                break;
                            case ControlCommand.CLEAR_BREAK_STATE:
                                Response.Add(Data[i]);
                                break;
                            case ControlCommand.REQUEST_DTR_STATE:
                                if (DtrEnable)
                                    Response.Add((byte)ControlCommand.SET_DTR_STATE);
                                else
                                    Response.Add((byte)ControlCommand.CLEAR_DTR_STATE);
                                break;
                            case ControlCommand.SET_DTR_STATE:
                                Response.Add(Data[i]);
                                break;
                            case ControlCommand.CLEAR_DTR_STATE:
                                Response.Add(Data[i]);
                                break;
                            case ControlCommand.REQUEST_RTS_STATE:
                                if (DtrEnable)
                                    Response.Add((byte)ControlCommand.SET_RTS_STATE);
                                else
                                    Response.Add((byte)ControlCommand.CLEAR_RTS_STATE);
                                break;
                            case ControlCommand.SET_RTS_STATE:
                                Response.Add(Data[i]);
                                break;
                            case ControlCommand.CLEAR_RTS_STATE:
                                Response.Add(Data[i]);
                                break;
                            case ControlCommand.REQUEST_INBOUND_FLOWCONTROL:
                                if (FlowControl.RtsInbound == false && 
                                    FlowControl.XonXoffInbound == false &&
                                    FlowControl.DtrInbound == false)
                                {
                                    Response.Add((byte)ControlCommand.USE_NO_INBOUND_FLOWCONTROL);
                                }

                                if (FlowControl.RtsInbound)
                                    Response.Add((byte)ControlCommand.USE_HARDWARE_INBOUND_FLOWCONTROL);
                                else if (FlowControl.XonXoffInbound)
                                    Response.Add((byte)ControlCommand.USE_XONXOFF_INBOUND_FLOWCONTROL);
                                else if (FlowControl.DtrInbound)
                                    Response.Add((byte)ControlCommand.USE_DTR_INBOUND_FLOWCONTROL);

                                break;
                            case ControlCommand.USE_NO_INBOUND_FLOWCONTROL:
                                Response.Add((byte)ControlCommand.USE_NO_INBOUND_FLOWCONTROL);
                                break;
                            case ControlCommand.USE_XONXOFF_INBOUND_FLOWCONTROL:
                                Response.Add((byte)ControlCommand.USE_XONXOFF_INBOUND_FLOWCONTROL);
                                break;
                            case ControlCommand.USE_HARDWARE_INBOUND_FLOWCONTROL:
                                Response.Add((byte)ControlCommand.USE_HARDWARE_INBOUND_FLOWCONTROL);
                                break;
                            case ControlCommand.USE_DCD_OUTBOUND_FLOWCONTROL:
                                Response.Add((byte)ControlCommand.USE_DCD_OUTBOUND_FLOWCONTROL);
                                break;
                            case ControlCommand.USE_DTR_INBOUND_FLOWCONTROL:
                                Response.Add((byte)ControlCommand.USE_DTR_INBOUND_FLOWCONTROL);
                                break;
                            case ControlCommand.USE_DSR_OUTBOUND_FLOWCONTROL:
                                Response.Add((byte)ControlCommand.USE_DSR_OUTBOUND_FLOWCONTROL);
                                break;
                            default:
                                break;
                        }

                        break;
                    case Command.NOTIFY_LINESTATE:
                    case Command.NOTIFY_MODEMSTATE:
                    case Command.FLOWCONTROL_SUSPEND:
                    case Command.FLOWCONTROL_RESUME:
                    case Command.SET_LINESTATE_MASK:
                    case Command.SET_MODEMSTATE_MASK:
                    case Command.PURGE_DATA:
                    default:
                        break;
                }

                if (ResponseOK)
                {
                    Response.Add((byte)TelnetCommand.IAC);
                    Response.Add((byte)TelnetCommand.SE);

                    if (client != null)
                        if (client.IsConnected)
                            client.Send(Response.ToArray());
                }
            }
            return false;
        }

        private bool _SetSignature(string Name)
        {
            List<byte> Request = new List<byte>();

            Request.Add((byte)Telnet.TelnetCommand.IAC);
            Request.Add((byte)Telnet.TelnetCommand.SB);
            Request.Add((byte)RFC2217OptionCode);

            if (Server)
            {
                Request.Add((byte)(Command.SIGNATURE + 100));
            }
            else
            {
                Request.Add((byte) Command.SIGNATURE);
            }

            foreach (char c in Name.ToCharArray())
                Request.Add((byte)c);

            Request.Add((byte)TelnetCommand.IAC);
            Request.Add((byte)TelnetCommand.SE);

            if (OnSendData == null) return false;

            OnSendData(this, Request.ToArray());

            return true;
        }

        private bool _SetBaudRate(UInt32 NewBaudRate)
        {
            List<byte> Request = new List<byte>();

            Request.Add((byte)Telnet.TelnetCommand.IAC);
            Request.Add((byte)Telnet.TelnetCommand.SB);
            Request.Add((byte)RFC2217OptionCode);

            if (Server)
            {
                Request.Add((byte)(Command.SET_BAUDRATE + 100));
            }
            else
            {
                Request.Add((byte)Command.SET_BAUDRATE);
            }

            byte[] BaudArr = BitConverter.GetBytes(NewBaudRate);
            Array.Reverse(BaudArr);
            Request.AddRange(BaudArr);

            Request.Add((byte)TelnetCommand.IAC);
            Request.Add((byte)TelnetCommand.SE);

            if (OnSendData == null) return false;

            OnSendData(this, Request.ToArray());

            return true;
        }

        private bool _SetDataSize(byte NewDataSize)
        {
            List<byte> Request = new List<byte>();

            Request.Add((byte)Telnet.TelnetCommand.IAC);
            Request.Add((byte)Telnet.TelnetCommand.SB);
            Request.Add((byte)RFC2217OptionCode);

            if (Server)
            {
                Request.Add((byte)(Command.SET_DATASIZE + 100));
            }
            else
            {
                Request.Add((byte)Command.SET_DATASIZE);
            }

            Request.Add(NewDataSize);

            Request.Add((byte)TelnetCommand.IAC);
            Request.Add((byte)TelnetCommand.SE);

            if (OnSendData == null) return false;

            OnSendData(this, Request.ToArray());

            return true;
        }

        private bool _SetParity(byte NewParity)
        {
            List<byte> Request = new List<byte>();

            Request.Add((byte)Telnet.TelnetCommand.IAC);
            Request.Add((byte)Telnet.TelnetCommand.SB);
            Request.Add((byte)RFC2217OptionCode);

            if (Server)
            {
                Request.Add((byte)(Command.SET_PARITY + 100));
            }
            else
            {
                Request.Add((byte)Command.SET_PARITY);
            }

            Request.Add(NewParity);

            Request.Add((byte)TelnetCommand.IAC);
            Request.Add((byte)TelnetCommand.SE);

            if (OnSendData == null) return false;

            OnSendData(this, Request.ToArray());

            return true;
        }

        private bool _SetStopBits(byte NewStopBits)
        {
            List<byte> Request = new List<byte>();

            Request.Add((byte)Telnet.TelnetCommand.IAC);
            Request.Add((byte)Telnet.TelnetCommand.SB);
            Request.Add((byte)RFC2217OptionCode);

            if (Server)
            {
                Request.Add((byte)(Command.SET_STOPSIZE + 100));
            }
            else
            {
                Request.Add((byte)Command.SET_STOPSIZE);
            }

            Request.Add(NewStopBits);

            Request.Add((byte)TelnetCommand.IAC);
            Request.Add((byte)TelnetCommand.SE);

            if (OnSendData == null) return false;

            OnSendData(this, Request.ToArray());

            return true;
        }

        private bool _SetLineStateMask(byte NewLineStateMask)
        {
            List<byte> Request = new List<byte>();

            Request.Add((byte)Telnet.TelnetCommand.IAC);
            Request.Add((byte)Telnet.TelnetCommand.SB);
            Request.Add((byte)RFC2217OptionCode);

            if (Server)
            {
                Request.Add((byte)(Command.SET_LINESTATE_MASK + 100));
            }
            else
            {
                Request.Add((byte)Command.SET_LINESTATE_MASK);
            }

            Request.Add(NewLineStateMask);

            Request.Add((byte)TelnetCommand.IAC);
            Request.Add((byte)TelnetCommand.SE);

            if (OnSendData == null) return false;

            OnSendData(this, Request.ToArray());

            return true;
        }

        private bool _SetModemStateMask(byte NewModemStateMask)
        {
            List<byte> Request = new List<byte>();

            Request.Add((byte)Telnet.TelnetCommand.IAC);
            Request.Add((byte)Telnet.TelnetCommand.SB);
            Request.Add((byte)RFC2217OptionCode);

            if (Server)
            {
                Request.Add((byte)(Command.SET_MODEMSTATE_MASK + 100));
            }
            else
            {
                Request.Add((byte)Command.SET_MODEMSTATE_MASK);
            }

            Request.Add(NewModemStateMask);

            Request.Add((byte)TelnetCommand.IAC);
            Request.Add((byte)TelnetCommand.SE);

            if (OnSendData == null) return false;

            OnSendData(this, Request.ToArray());

            return true;
        }

        private bool _SetSuspend(bool NewSuspendState)
        {
            List<byte> Request = new List<byte>();

            Request.Add((byte)Telnet.TelnetCommand.IAC);
            Request.Add((byte)Telnet.TelnetCommand.SB);
            Request.Add((byte)RFC2217OptionCode);

            if (Server)
            {
                if (NewSuspendState)
                    Request.Add((byte)(Command.FLOWCONTROL_SUSPEND+100));
                else
                    Request.Add((byte)(Command.FLOWCONTROL_RESUME + 100));
                
            }
            else
            {
                if ( NewSuspendState )
                    Request.Add((byte)Command.FLOWCONTROL_SUSPEND);
                else
                    Request.Add((byte)Command.FLOWCONTROL_RESUME);
            }
            

            Request.Add((byte)TelnetCommand.IAC);
            Request.Add((byte)TelnetCommand.SE);

            if (OnSendData == null) return false;

            OnSendData(this, Request.ToArray());

            return true;
        }

        private bool _SetBreakState(bool NewBreakState)
        {
            List<byte> Request = new List<byte>();

            Request.Add((byte)Telnet.TelnetCommand.IAC);
            Request.Add((byte)Telnet.TelnetCommand.SB);
            Request.Add((byte)RFC2217OptionCode);

            if (Server)
            {
                Request.Add((byte)(Command.SET_CONTROL + 100));                
            }
            else
            {
                Request.Add((byte)Command.SET_CONTROL);
            }

            if ( NewBreakState )
                Request.Add((byte)ControlCommand.SET_BREAK_STATE);
            else
                Request.Add((byte)ControlCommand.CLEAR_BREAK_STATE);

            Request.Add((byte)TelnetCommand.IAC);
            Request.Add((byte)TelnetCommand.SE);

            if (OnSendData == null) return false;

            OnSendData(this, Request.ToArray());

            return true;
        }

        private bool _SetDtrEnable(bool NewDtrState)
        {
            List<byte> Request = new List<byte>();

            Request.Add((byte)Telnet.TelnetCommand.IAC);
            Request.Add((byte)Telnet.TelnetCommand.SB);
            Request.Add((byte)RFC2217OptionCode);

            if (Server)
            {
                Request.Add((byte)(Command.SET_CONTROL + 100));
            }
            else
            {
                Request.Add((byte)Command.SET_CONTROL);
            }

            if (NewDtrState)
                Request.Add((byte)ControlCommand.SET_DTR_STATE);
            else
                Request.Add((byte)ControlCommand.CLEAR_DTR_STATE);

            Request.Add((byte)TelnetCommand.IAC);
            Request.Add((byte)TelnetCommand.SE);

            if (OnSendData == null) return false;

            OnSendData(this, Request.ToArray());

            return true;
        }

        private bool _SetRtsEnable(bool NewRtsState)
        {
            List<byte> Request = new List<byte>();

            Request.Add((byte)Telnet.TelnetCommand.IAC);
            Request.Add((byte)Telnet.TelnetCommand.SB);
            Request.Add((byte)RFC2217OptionCode);

            if (Server)
            {
                Request.Add((byte)(Command.SET_CONTROL + 100));
            }
            else
            {
                Request.Add((byte)Command.SET_CONTROL);
            }

            if (NewRtsState)
                Request.Add((byte)ControlCommand.SET_RTS_STATE);
            else
                Request.Add((byte)ControlCommand.CLEAR_RTS_STATE);

            Request.Add((byte)TelnetCommand.IAC);
            Request.Add((byte)TelnetCommand.SE);

            if (OnSendData == null) return false;

            OnSendData(this, Request.ToArray());

            return true;
        }

        private void FlowControl_OnSettingChanged(object sender)
        {            
            if (Server) return;

            //_SetFlowControl();
        }

        private void _SetFlowControl()
        {
            if (FlowControl.IsNoFlowControl() || FlowControl.IsHardwareFlowControl() ||
                 FlowControl.IsDtrDsrFlowControl() || FlowControl.IsXonXoffFlowControl())
            {
                List<byte> Request = new List<byte>();

                Request.Add((byte)Telnet.TelnetCommand.IAC);
                Request.Add((byte)Telnet.TelnetCommand.SB);
                Request.Add((byte)RFC2217OptionCode);

                if (Server)
                {
                    Request.Add((byte)(Command.SET_CONTROL + 100));
                }
                else
                {
                    Request.Add((byte)Command.SET_CONTROL);
                }

                if (FlowControl.IsNoFlowControl())
                {
                    Request.Add((byte)ControlCommand.USE_NO_FLOWCONTROL);
                }
                else if (FlowControl.IsHardwareFlowControl())
                {
                    Request.Add((byte)ControlCommand.USE_HARDWARE_OUTBOUND_FLOWCONTROL);
                }

                else if (FlowControl.IsDtrDsrFlowControl())
                {
                    Request.Add((byte)ControlCommand.USE_DSR_OUTBOUND_FLOWCONTROL);
                }
                else if (FlowControl.IsXonXoffFlowControl())
                {
                    Request.Add((byte)ControlCommand.USE_XONXOFF_OUTBOUND_FLOWCONTROL);
                }

                Request.Add((byte)TelnetCommand.IAC);
                Request.Add((byte)TelnetCommand.SE);

                if (OnSendData == null) return;

                OnSendData(this, Request.ToArray());

                return;
            }
            else
            {
                if (FlowControl.CtsOutbound)
                {
                    _SetFlowControl(ControlCommand.USE_HARDWARE_OUTBOUND_FLOWCONTROL);
                }

                if (FlowControl.RtsInbound)
                {
                    _SetFlowControl(ControlCommand.USE_HARDWARE_INBOUND_FLOWCONTROL);
                }

                if (FlowControl.DsrOutbound)
                {
                    _SetFlowControl(ControlCommand.USE_DSR_OUTBOUND_FLOWCONTROL);
                }

                if (FlowControl.DtrInbound)
                {
                    _SetFlowControl(ControlCommand.USE_DTR_INBOUND_FLOWCONTROL);
                }

                if (FlowControl.XonXoffOutbound)
                {
                    _SetFlowControl(ControlCommand.USE_XONXOFF_OUTBOUND_FLOWCONTROL);
                }

                if (FlowControl.XonXoffInbound)
                {
                    _SetFlowControl(ControlCommand.USE_XONXOFF_INBOUND_FLOWCONTROL);
                }

                if (FlowControl.DcdOutbound)
                {
                    _SetFlowControl(ControlCommand.USE_DCD_OUTBOUND_FLOWCONTROL);
                }

            }            

        }


        private void _SetFlowControl(ControlCommand NewFlowControl)
        {
            List<byte> Request = new List<byte>();

            Request.Add((byte)Telnet.TelnetCommand.IAC);
            Request.Add((byte)Telnet.TelnetCommand.SB);
            Request.Add((byte)RFC2217OptionCode);

            if (Server)
            {
                Request.Add((byte)(Command.SET_CONTROL + 100));
            }
            else
            {
                Request.Add((byte)Command.SET_CONTROL);
            }

            Request.Add((byte)NewFlowControl);

            Request.Add((byte)TelnetCommand.IAC);
            Request.Add((byte)TelnetCommand.SE);

            if (OnSendData == null) return;

            OnSendData(this, Request.ToArray());

            return;
        }

        // Server only method
        public bool SetModemState(bool Cts, bool Dsr, bool Ri, bool Rlsd)
        {
            if (!Server) return false;

            if (!IsNegotiatedSuccessfully()) return false;

            List<byte> Request = new List<byte>();
            byte NewModemState = 0, DeltaModemState = 0;

            if (Cts) NewModemState |= (byte)ModemState.CTS;
            if (Dsr) NewModemState |= (byte)ModemState.DSR;
            if (Ri) NewModemState |= (byte)ModemState.RI;
            if (Rlsd) NewModemState |= (byte)ModemState.RLSD;

            DeltaModemState = (byte) (((NewModemState ^ (_LastModemState & 0xF0)) >> 4) & 0xFF);

            NewModemState |= DeltaModemState;

            _LastModemState = NewModemState;

            if (ModemStateMask == 0) return true;
           
            Request.Add((byte)Telnet.TelnetCommand.IAC);
            Request.Add((byte)Telnet.TelnetCommand.SB);
            Request.Add((byte)RFC2217OptionCode);

            Request.Add((byte)(Command.NOTIFY_MODEMSTATE + 100));

            Request.Add((byte)(NewModemState & ModemStateMask));


            Request.Add((byte)TelnetCommand.IAC);
            Request.Add((byte)TelnetCommand.SE);

            if (OnSendData == null) return false;

            OnSendData(this, Request.ToArray());

            return true;
        }

        public bool SetLineStatus(bool DataReady, 
                                     bool Overrun,
                                     bool ParityError,
                                     bool FramingError,
                                     bool BreakDetected,
                                     bool THRE,
                                     bool TEMT,
                                     bool TimeoutError)
        {
            if (!Server) return false;

            if (!IsNegotiatedSuccessfully()) return false;

            if (LineStateMask == 0x00) return true;

            List<byte> Request = new List<byte>();
            byte NewLineState = 0; 

            Request.Add((byte)Telnet.TelnetCommand.IAC);
            Request.Add((byte)Telnet.TelnetCommand.SB);
            Request.Add((byte)RFC2217OptionCode);

            Request.Add((byte)(Command.NOTIFY_LINESTATE + 100));

            if (DataReady) NewLineState |= (byte)LineState.DataReady;
            if (Overrun) NewLineState |= (byte)LineState.Overrun;
            if (ParityError) NewLineState |= (byte)LineState.ParityError;
            if (FramingError) NewLineState |= (byte)LineState.FramingError;
            if (BreakDetected) NewLineState |= (byte)LineState.BreakDetected;
            if (THRE) NewLineState |= (byte)LineState.THRE;
            if (TEMT) NewLineState |= (byte)LineState.TEMT;
            if (TimeoutError) NewLineState |= (byte)LineState.TimeoutError;

            _LastLineState = NewLineState;
            Request.Add((byte)(NewLineState & LineStateMask));

            Request.Add((byte)TelnetCommand.IAC);
            Request.Add((byte)TelnetCommand.SE);

            if (OnSendData == null) return false;

            OnSendData(this, Request.ToArray());

            return true;
        }


    }
}
