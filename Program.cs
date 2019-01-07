using System;
using System.Diagnostics;
using Telnet;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using CommandLine;

using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Virtual_Port
{
    
    class Program
    {
        static bool _Cancelled = false;
        
       // public static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
       // Initialize the trace source.
        static TraceSource ts = new TraceSource("TraceTest");
        [SwitchAttribute("SourceSwitch", typeof(SourceSwitch))]
        static int Main(string[] args)
        {           

            Console.CancelKeyPress += new ConsoleCancelEventHandler(Console_CancelKeyPress);

            var cmdOptions = Parser.Default.ParseArguments<CmdOptions>(args);
            var ret = cmdOptions.MapResult(
                options => VirtualPort(options),
                _ => -1);
            #if DEBUG
                        Console.WriteLine("Return: " + ret.ToString());
            #endif
                        return ret;
        }

        static int VirtualPort(CmdOptions ops)
        {
            try
            {
                Console.WriteLine(ops.PortName);

                var servicesProvider = BuildDi();
                var server = servicesProvider.GetRequiredService<VirtualSerialPortServer>();
                SourceSwitch sourceSwitch = new SourceSwitch("SourceSwitch", "Verbose");
                ts.Switch = sourceSwitch;
                int idxConsole = ts.Listeners.Add(new NLog.NLogTraceListener());
                ts.Listeners[idxConsole].Name = "nlog";
                if ( ops.Server ) {
                    Console.WriteLine("Starting Server...");
                    server.Start(ops.PortName, ops.ServerTCPPort, ref _Cancelled);
                    // Console.WriteLine("Press ANY key to exit");
                    // Console.ReadKey();
                }
              
            }
            catch (Exception ex)
            { 
                // NLog: catch any exception and log it.
                //logger.Error(ex, "Stopped program because of exception");
                //throw;
                Console.WriteLine("Exception:" + ex.Message);
                return -1;
            }
            finally
            {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                LogManager.Shutdown();
                ts.Flush();
                ts.Close();
            }            
            
            return 0;
        }

        private static ServiceProvider BuildDi()
        {
            return new ServiceCollection()
                .AddLogging(builder => {
                    builder.SetMinimumLevel(LogLevel.Trace);
                    builder.AddNLog(new NLogProviderOptions {
                        CaptureMessageTemplates = true,
                        CaptureMessageProperties = true                        
                    });
                })
                .AddTransient<VirtualSerialPortServer>()
                .BuildServiceProvider();
        }

        static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
    {
        Console.WriteLine("Cancelling");
        if (e.SpecialKey == ConsoleSpecialKey.ControlC)
        {
            _Cancelled = true;
            e.Cancel = true;
        }
    }
 
    }
}
