using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using AlarmServer.Core;

namespace AlarmServerService
{
    /// <summary>
    /// Service for the <see cref="AlarmServer"/>
    /// </summary>
    public class AlarmServerService : ServiceBase
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private Container components = null;
        private AlarmServer server = null;
        private AlarmServerConfiguration configuration = null;

        public AlarmServerService()
        {
            InitializeComponent();
        }

        static void Main()
        {
            ServiceBase[] ServicesToRun;
            
            ServicesToRun = new ServiceBase[] { new AlarmServerService() };

            ServiceBase.Run(ServicesToRun);
        }

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new Container();
            this.ServiceName = "AlarmServer";
            this.CanPauseAndContinue = false;
            this.AutoLog = true;
            this.CanShutdown = true;
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Boot the Service.
        /// </summary>
        protected override void OnStart(string[] args)
        {
            try
            {
                configuration = (AlarmServerConfiguration)AlarmServerConfiguration.GetDefaultConfigurationPath();
            }
            catch (Exception ex) { this.EventLog.WriteEntry("Stopped by Misconfiguration. Error-Message: " + ex.Message, EventLogEntryType.Error); Stop(); return; }

            string currentLogFile = Path.Combine(Path.GetDirectoryName(AlarmServerConfiguration.GetDefaultConfigurationPath()), "Log at " + DateTime.Now.ToString("yyyy-MM-dd") + ".log");

            Logger.LoadLogFile(currentLogFile);
            Logger.WriteLine("Server starting...");

            server = new AlarmServer(configuration);
            server.StartServer();
            Logger.WriteLine("Server running at " + configuration.ServerAddress + ":" + configuration.ServerPort);
        }

        /// <summary>
        /// Stop this service.
        /// </summary>
        protected override void OnStop()
        {
            Logger.WriteLine("Shutting down Server...");
            Logger.FlushClose();

            server.StopServer();
            server = null;
        }
    }
}