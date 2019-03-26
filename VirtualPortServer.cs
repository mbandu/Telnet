
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Diagnostics;
#if LINUX
    using RJCP.IO.Ports;
#else
    using System.IO.Ports;
#endif
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Telnet
{
    public class VirtualSerialPortServer: TelnetServer
    {
        // NLogger instance
        private readonly ILogger<VirtualSerialPortServer> _logger;

        // Telnet server
        private Telnet.TelnetServer telnetServer = new TelnetServer();

        // RFC2217 option for Telnet option
        private Telnet.RFC2217Option rfc2217Option = new RFC2217Option(true);

#if LINUX
        private SerialPortStream serialDevice = null;
#else
        private System.IO.Ports.SerialPort serialDevice = null;
#endif
        private bool bCrashed = false;
        public VirtualSerialPortServer(ILogger<VirtualSerialPortServer> logger)
        {
            _logger = logger;
            this.OnLog += LogHandler;

            #if DEBUG
                
                #if LINUX
                    var ports = SerialPortStream.GetPortNames();
                #else
                    var ports = System.IO.Ports.SerialPort.GetPortNames();
                #endif

                bool isTTY = false;
                foreach (var prt in ports)
                {
                    _Log($"Serial name: {prt}");
                    if (prt.Contains("ttyS0"))
                    {
                        isTTY = true;
                    }
                }
                if (!isTTY)
                {
                    Console.WriteLine("No ttyS0 serial port!");
                    return;
                }
            #endif
        }

        void LogHandler(string Msg, LogLevel logLevel) 
        {
            Console.WriteLine(Msg);
            _logger.Log(logLevel, 20, Msg);
        }

        public void Start( string SerialDeviceName, int TelnetTCPPortNumber, ref bool Cancelled )
        {
            try
            {
                telnetServer.OnDataReceived += new TelnetServer.OnDataReceivedHandler(telnetServer_OnDataReceived);
                telnetServer.OnConnected += new TelnetBase.OnConnectedHandler(telnetServer_OnConnected);
                telnetServer.OnDisconnected += new TelnetBase.OnDisconnectedHandler(telnetServer_OnDisconnected);
                telnetServer.OnOptionNegotiated += new TelnetBase.OnOptionNegotiatedHandler(telnetServer_OnOptionNegotiated);
                rfc2217Option.OnBaudRateChanged += new RFC2217Option.OnBaudRateChangedHandler(rfc2217Option_OnBaudRateChanged);
                rfc2217Option.OnBreakStateChanged += new RFC2217Option.OnBreakStateChangedHandler(rfc2217Option_OnBreakStateChanged);
                rfc2217Option.OnDataSizeChanged += new RFC2217Option.OnDataSizeChangedHandler(rfc2217Option_OnDataSizeChanged);
                rfc2217Option.OnDtrSignalStateChanged += new RFC2217Option.OnDtrSignalStateChangedHandler(rfc2217Option_OnDtrSignalStateChanged);
                rfc2217Option.OnFlowControlChanged += new RFC2217Option.OnFlowControlChangedHandler(rfc2217Option_OnFlowControlChanged);
                rfc2217Option.OnLineStateChanged += new RFC2217Option.OnLineStateChangedHandler(rfc2217Option_OnLineStateChanged);
                rfc2217Option.OnLineStateMaskChanged += new RFC2217Option.OnLineStateMaskChangedHandler(rfc2217Option_OnLineStateMaskChanged);
                rfc2217Option.OnModemStateChanged += new RFC2217Option.OnModemStateChangedHandler(rfc2217Option_OnModemStateChanged);
                rfc2217Option.OnModemStateMaskChanged += new RFC2217Option.OnModemStateMaskChangedHandler(rfc2217Option_OnModemStateMaskChanged);
                rfc2217Option.OnParityChanged += new RFC2217Option.OnParityChangedHandler(rfc2217Option_OnParityChanged);
                rfc2217Option.OnPurgeData += new RFC2217Option.OnPurgeDataHandler(rfc2217Option_OnPurgeData);
                rfc2217Option.OnRtsSignalChanged += new RFC2217Option.OnRtsSignalChangedHandler(rfc2217Option_OnRtsSignalChanged);
                rfc2217Option.OnSendData += new TelnetOption.OnSendDataHandler(rfc2217Option_OnSendData);
                rfc2217Option.OnSignatureChanged += new RFC2217Option.OnSignatureChangedHandler(rfc2217Option_OnSignatureChanged);
                rfc2217Option.OnStopBitsChanged += new RFC2217Option.OnStopBitsChangedHandler(rfc2217Option_OnStopBitsChanged);
                rfc2217Option.OnSuspendStateChanged += new RFC2217Option.OnSuspendStateChangedHandler(rfc2217Option_OnSuspendStateChanged);

                #if LINUX
                    _Log("Using SerialPortStream class.");
                    serialDevice = new SerialPortStream(SerialDeviceName);
                    serialDevice.DataReceived += serailDevice_OnReceive;
                    serialDevice.ErrorReceived += serailDevice_OnCommError;
                    serialDevice.PinChanged += serailDevice_OnModemStatusChanged;
                #else
                    _Log("Using System.IO.Ports.SerialPort class.");
                    serialDevice = new System.IO.Ports.SerialPort(SerialDeviceName);
                    serialDevice.DataReceived += new SerialDataReceivedEventHandler(serailDevice_OnReceive);
                    serialDevice.ErrorReceived += new SerialErrorReceivedEventHandler(serailDevice_OnCommError);
                    serialDevice.PinChanged += new SerialPinChangedEventHandler(serailDevice_OnModemStatusChanged);

                #endif
                telnetServer.Port = TelnetTCPPortNumber;
                telnetServer.SetOption(44, rfc2217Option);
                telnetServer.Start();
                
                _Log( "RFC2217 Server Successfully Started");
            }
            catch (Exception ex)
            {
                _LogCritical("RFC2217 Server Start Error.\r\n" + ex.Message + "\r\n" + ex.StackTrace);                
            }
            finally {
                try {
                    while(!Cancelled && !bCrashed){}
                }
                catch(Exception ex) {
                    _Log( "******************Exception:" + ex.Message);                   
                }       
                Shutdown();         
            }
        }

        private void Shutdown() {
           
            _Log( "Shutting down Server...");
            if ( serialDevice.IsOpen )
                serialDevice.Close();
            
            _Log( "Shutting down Server...1");
            telnetServer.Stop();
            bCrashed = true;
            _Log( "Shutting down Server... Done");
        }
        
        void telnetServer_OnConnected(TelnetBase sender, IPEndPoint RemoteEndPoint)
        {
            try {
                bCrashed = false;
                _Log("Opening serial port...");
                serialDevice.Open();
                _Log("Serial port opened successfully.");
            }
            catch(Exception ex) {
                _LogCritical(ex.Message + ex.StackTrace);
                Shutdown();
            }
        }

        void telnetServer_OnDisconnected(TelnetBase sender)
        {
            try {
                _Log("Closing serial port...");
                serialDevice.Close();
                _Log("Serial port closed successfully.");
            }
            catch(Exception ex) {
                _LogCritical(ex.Message + ex.StackTrace);
                Shutdown();
            }
        }

        void telnetServer_OnDataReceived(object sender, byte[] Data)
        {
            System.Text.Encoding enc = System.Text.Encoding.ASCII;
            string str = enc.GetString(Data);
            #if LINUX
                serialDevice.Write(Data);
            #else
                serialDevice.Write(Data, 0, Data.Length);
            #endif
            _Log( "RX=> " + Data.Length.ToString() + " Bytes received" );
        }
        void telnetServer_OnOptionNegotiated(TelnetBase Sender, byte OptionCode)
        {
            _Log( "Option: " + OptionCode.ToString("X") + " is negotiated successfully.\r\n");
            if (OptionCode == 44)
            {
            }
        }

        void rfc2217Option_OnSuspendStateChanged(object sender)
        {
            _Log( "Set Suspend=> " + rfc2217Option.Suspend.ToString() );
        }
        
        void rfc2217Option_OnStopBitsChanged(object sender)
        {            
            _Log( "Set StopBits=> " + ((RFC2217Option.StopBitsType)rfc2217Option.StopBits).ToString() + "(" + rfc2217Option.StopBits.ToString() + ")");
            switch ((RFC2217Option.StopBitsType)rfc2217Option.StopBits)
            {                        
                case RFC2217Option.StopBitsType.One:
                    #if LINUX
                        serialDevice.StopBits = RJCP.IO.Ports.StopBits.One;
                    #else
                        serialDevice.StopBits = System.IO.Ports.StopBits.One;
                    #endif
                    break;
                case RFC2217Option.StopBitsType.Two:
                    #if LINUX
                        serialDevice.StopBits = RJCP.IO.Ports.StopBits.Two;
                    #else
                        serialDevice.StopBits = System.IO.Ports.StopBits.Two;
                    #endif
                    break;
                case RFC2217Option.StopBitsType.OnePointFive:
                    #if LINUX
                        serialDevice.StopBits = RJCP.IO.Ports.StopBits.One5;
                    #else
                        serialDevice.StopBits = System.IO.Ports.StopBits.OnePointFive;
                    #endif
                    break;
                default:
                    break;
            }    
        }

        void rfc2217Option_OnSignatureChanged(object sender)
        {
            _Log( "Set Signature=> " + rfc2217Option.Signature );
        }

        void rfc2217Option_OnSendData(object sender, byte[] DataToSend)
        {
            _Log( "Data to Send=> " + DataToSend.Length.ToString() + " bytes");
        }

        void rfc2217Option_OnRtsSignalChanged(object sender)
        {
            _Log( "Set ModemStateMask=> " + rfc2217Option.RtsEnable.ToString() );
            serialDevice.RtsEnable = rfc2217Option.RtsEnable;
        }

        void rfc2217Option_OnPurgeData(object sender, bool PurgeTransmitBuffer, bool PurgeReceiveBuffer)
        {
            _Log( "Set PurgeData=> TxBuffer:" + PurgeTransmitBuffer.ToString() + " RxBuffer:" + PurgeReceiveBuffer.ToString() );
            // TODO: Purge
        }

        void rfc2217Option_OnParityChanged(object sender)
        {
            _Log( "Set Parity=> " + ((RFC2217Option.ParityType)rfc2217Option.Parity).ToString() + "(" + rfc2217Option.Parity.ToString() + ")");
            switch (((RFC2217Option.ParityType)rfc2217Option.Parity))
            {
                case RFC2217Option.ParityType.None:
                    #if LINUX
                        serialDevice.Parity = RJCP.IO.Ports.Parity.None;
                    #else
                        serialDevice.Parity = System.IO.Ports.Parity.None;
                    #endif
                    break;
                case RFC2217Option.ParityType.Odd:
                    #if LINUX
                        serialDevice.Parity = RJCP.IO.Ports.Parity.Odd;
                    #else
                        serialDevice.Parity = System.IO.Ports.Parity.Odd;
                    #endif
                    break;
                case RFC2217Option.ParityType.Even:
                    #if LINUX
                        serialDevice.Parity = RJCP.IO.Ports.Parity.Even;
                    #else
                        serialDevice.Parity = System.IO.Ports.Parity.Even;
                    #endif
                    break;
                case RFC2217Option.ParityType.Mark:
                    #if LINUX
                        serialDevice.Parity = RJCP.IO.Ports.Parity.Mark;
                    #else
                        serialDevice.Parity = System.IO.Ports.Parity.Mark;
                    #endif
                    break;
                case RFC2217Option.ParityType.Space:
                    #if LINUX
                        serialDevice.Parity = RJCP.IO.Ports.Parity.Space;
                    #else
                        serialDevice.Parity = System.IO.Ports.Parity.Space;
                    #endif
                    break;
                default:
                    break;
            }
        }

        void rfc2217Option_OnModemStateMaskChanged(object sender)
        {
            _Log( "Set ModemStateMask=> " + rfc2217Option.ModemStateMask.ToString() );
        }

        void rfc2217Option_OnModemStateChanged(object sender, bool Cts, bool Dsr, bool Ri, bool Rlsd)
        {
            _Log( "Set ModemState should not come in because its RFC2217 Server. This command only sent from Server to Client\r\n");
        }

        void rfc2217Option_OnLineStateMaskChanged(object sender)
        {
            _Log( "Set LineStateMask=> " + rfc2217Option.LineStateMask.ToString() );
        }

        void rfc2217Option_OnLineStateChanged(object sender, bool DataReady, bool Overrun, bool ParityError, bool FramingError, bool BreakDetected, bool THRE, bool TEMT, bool TimeoutError)
        {
            _Log( "Set LineState should not come in because its RFC2217 server. This command only sent from Server to Client\r\n");
        }

        void rfc2217Option_OnFlowControlChanged(object sender, RFC2217FlowControl FlowControl)
        {
            _Log( "Set FlowControl =>");
            _Log( "  CtsOutbound:" + FlowControl.CtsOutbound );
            _Log( "  RtsInbound:" + FlowControl.RtsInbound );
            _Log( "  DsrOutbound:" + FlowControl.DsrOutbound );
            _Log( "  DtrInbound:" + FlowControl.DtrInbound );
            _Log( "  XonXoffOutbound:" + FlowControl.XonXoffOutbound );
            _Log( "  XonXoffInbound:" + FlowControl.XonXoffInbound );
            _Log( "  DcdOutbound:" + FlowControl.DcdOutbound );
            RJCP.IO.Ports.Handshake handshake = RJCP.IO.Ports.Handshake.None;
            if ( FlowControl.CtsOutbound || FlowControl.RtsInbound) {
                handshake = RJCP.IO.Ports.Handshake.Rts;
            }
            if ( FlowControl.DsrOutbound || FlowControl.DtrInbound) {
                handshake |= RJCP.IO.Ports.Handshake.Dtr;
            }
            if ( FlowControl.XonXoffOutbound || FlowControl.XonXoffInbound) {
                handshake |= RJCP.IO.Ports.Handshake.XOn;
            }
        }

        void rfc2217Option_OnDtrSignalStateChanged(object sender)
        {
            _Log( "Set DTR=> " + rfc2217Option.DtrEnable.ToString() );
            serialDevice.DtrEnable = rfc2217Option.DtrEnable;
            
        }

        void rfc2217Option_OnDataSizeChanged(object sender)
        {
            _Log( "Set DataSize=> " + rfc2217Option.DataSize.ToString() );
            serialDevice.DataBits = rfc2217Option.DataSize;
        }

        void rfc2217Option_OnBreakStateChanged(object sender)
        {
            _Log( "Set Break=> " + rfc2217Option.BreakState.ToString() );
            serialDevice.BreakState = rfc2217Option.BreakState;
        }

        void rfc2217Option_OnBaudRateChanged(object sender)
        {
            _Log( "Set Baudrate=> " + rfc2217Option.BaudRate.ToString() );
            serialDevice.BaudRate = (int)rfc2217Option.BaudRate;
        }
 
        #if LINUX
            void serailDevice_OnCommError(object sender, SerialErrorReceivedEventArgs e)
            {
                rfc2217Option.SetLineStatus(serialDevice.BytesToRead != 0,
                                            (e.EventType & SerialError.Overrun) == SerialError.Overrun, 
                                            (e.EventType & SerialError.RXParity) == SerialError.RXParity, 
                                            (e.EventType & SerialError.Frame) == SerialError.Frame, 
                                            serialDevice.BreakState, false, false, false);  
            }

            void serailDevice_OnModemStatusChanged(object sender, SerialPinChangedEventArgs e)
            {
                rfc2217Option.SetModemState(serialDevice.CtsHolding, 
                                            serialDevice.DsrHolding, 
                                            (e.EventType & SerialPinChange.Ring) == SerialPinChange.Ring, 
                                            serialDevice.CDHolding);
            }

            void serailDevice_OnReceive(object sender, SerialDataReceivedEventArgs e)
            {
                byte[] Data = new byte[serialDevice.BytesToRead];
                serialDevice.Read(Data, 0, Data.Length);

                telnetServer.SendToNetwork(Data);
            }
        #else
            void serailDevice_OnCommError(object sender, SerialErrorReceivedEventArgs e)
            {
                rfc2217Option.SetLineStatus(serialDevice.BytesToRead != 0,
                                            (e.EventType & SerialError.Overrun) == SerialError.Overrun, 
                                            (e.EventType & SerialError.RXParity) == SerialError.RXParity, 
                                            (e.EventType & SerialError.Frame) == SerialError.Frame, 
                                            serialDevice.BreakState, false, false, false);  
            }

            void serailDevice_OnModemStatusChanged(object sender, SerialPinChangedEventArgs e)
            {
                rfc2217Option.SetModemState(serialDevice.CtsHolding, 
                                            serialDevice.DsrHolding, 
                                            (e.EventType & SerialPinChange.Ring) == SerialPinChange.Ring, 
                                            serialDevice.CDHolding);
            }

            void serailDevice_OnReceive(object sender, SerialDataReceivedEventArgs e)
            {
                byte[] Data = new byte[serialDevice.BytesToRead];
                serialDevice.Read(Data, 0, Data.Length);

                telnetServer.SendToNetwork(Data);
            }
        #endif
    }
}