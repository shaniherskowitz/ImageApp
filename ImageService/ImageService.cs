﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using ImageService;
using ImageService.Server;
using ImageService.Modal;
using ImageService.Controller;
using ImageService.ImageService_Logging.Model;
using ImageService.Model;
using System.Configuration;
using System.Threading;
using ImageService.Logging.Modal;

namespace ImageProject
{

    public enum ServiceState
    {
        SERVICE_STOPPED = 0x00000001,
        SERVICE_START_PENDING = 0x00000002,
        SERVICE_STOP_PENDING = 0x00000003,
        SERVICE_RUNNING = 0x00000004,
        SERVICE_CONTINUE_PENDING = 0x00000005,
        SERVICE_PAUSE_PENDING = 0x00000006,
        SERVICE_PAUSED = 0x00000007,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ServiceStatus
    {
        public int dwServiceType;
        public ServiceState dwCurrentState;
        public int dwControlsAccepted;
        public int dwWin32ExitCode;
        public int dwServiceSpecificExitCode;
        public int dwCheckPoint;
        public int dwWaitHint;
    };

    public partial class ImageService : ServiceBase
    {

        private ImageServer m_imageServer;          // The Image Server
        private IImageServiceModel model;
        private IImageController controller;
        private ILoggingService logging;

        /// <summary>
        /// Creates the ImageService
        /// Param: string[] args - the argument 
        /// </summary>
        public ImageService(string[] args)
        {
            InitializeComponent();
            string eventSourceName = ConfigurationManager.AppSettings["SourceName"];
            string logName = ConfigurationManager.AppSettings["LogName"];

            eventLog1 = new EventLog();
            if (!EventLog.SourceExists(eventSourceName))
            {
                EventLog.CreateEventSource(eventSourceName, logName);
            }
            eventLog1.Source = eventSourceName;
            eventLog1.Log = logName;
        }

        // Here You will Use the App Config!
        /// <summary>
        ///Start the server
        ///Param: string[] args - the command line arguments 
        /// </summary>
        protected override void OnStart(string[] args)
        {
            eventLog1.WriteEntry(ServiceState.SERVICE_RUNNING.ToString());
            // Update the service state to Start Pending.  
            ServiceStatus serviceStatus = new ServiceStatus();
            logging = new LoggingModal();
            logging.MessageRecieved += OnMsg;
            model = new ImageServiceModel(ConfigurationManager.AppSettings["OutputDir"], Int32.Parse(ConfigurationManager.AppSettings["ThumbnailSize"]));
            controller = new ImageController(model);
            m_imageServer = new ImageServer(controller, logging);
            
            CreateHandlers();

            serviceStatus.dwCurrentState = ServiceState.SERVICE_START_PENDING;
            serviceStatus.dwWaitHint = 100000;

            // Update the service state to Running.  
            serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;

            var server = new Thread(m_imageServer.StartServer);
            server.IsBackground = false;
            server.Start();

        }
        
        /// <summary>
        /// Creates the handler
        /// </summary>
        private void CreateHandlers()
        {
            string paths = ConfigurationManager.AppSettings["Handler"];
            IList<string> eachPath = paths.Split(';').Reverse().ToList<string>();
            
            for(int i = 0; i < eachPath.Count; ++i)
            {
                m_imageServer.CreateHandler(eachPath[i]);
            }
        }
        /// <summary>
        /// Stops the server
        /// </summary>
        protected override void OnStop()
        {
            m_imageServer.SendCommandToController();
            eventLog1.WriteEntry(ServiceState.SERVICE_STOPPED.ToString());
        }
        /// <summary>
        ///Logs a message.
        ///Param: Object sender, MessageRecievedEventArgs msg
        /// </summary>
        public void OnMsg(object sender, MessageRecievedEventArgs msg)
        {
            if(msg.Status == MessageTypeEnum.INFO) eventLog1.WriteEntry(msg.Message, EventLogEntryType.Information);
            if (msg.Status == MessageTypeEnum.WARNING) eventLog1.WriteEntry(msg.Message, EventLogEntryType.Warning);
            if (msg.Status == MessageTypeEnum.FAIL) eventLog1.WriteEntry(msg.Message, EventLogEntryType.FailureAudit);

            m_imageServer.SendLog(msg.Status + "," + msg.Message);
        }


    }
}


/* private LoggingModal Logger;

         public ImageService(string[] args)
         {
             InitializeComponent();
             string eventSourceName = "MySource";
             string logName = "MyNewLog";
             if (args.Count() > 0)
             {
                 eventSourceName = args[0];
             }
             if (args.Count() > 1)
             {
                 logName = args[1];
             }
             eventLog1 = new System.Diagnostics.EventLog();
             if (!System.Diagnostics.EventLog.SourceExists(eventSourceName))
             {
                 System.Diagnostics.EventLog.CreateEventSource(eventSourceName, logName);
             }
             eventLog1.Source = eventSourceName;
             eventLog1.Log = logName;
         }

         protected override void OnStart(string[] args)
         {
             // Update the service state to Start Pending.  
             ServiceStatus serviceStatus = new ServiceStatus();
             Logger = new LoggingModal();
             Logger.MessageRecieved += OnMsg;
             serviceStatus.dwCurrentState = ServiceState.SERVICE_START_PENDING;
             serviceStatus.dwWaitHint = 100000;
             SetServiceStatus(this.ServiceHandle, ref serviceStatus);

             eventLog1.WriteEntry("In OnStart");
             System.Timers.Timer timer = new System.Timers.Timer();
             timer.Interval = 60000;
             timer.Elapsed += new System.Timers.ElapsedEventHandler(this.OnTimer);
             timer.Start();

             // Update the service state to Running.  
             serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
             SetServiceStatus(this.ServiceHandle, ref serviceStatus);


         }
         public void OnMsg(object sender, string msg)
         {
             eventLog1.WriteEntry(msg);
         }

         public void OnTimer(Object sender, System.Timers.ElapsedEventArgs args)
         {
             eventLog1.WriteEntry("Monitoring the system", EventLogEntryType.Information, eventId++);
         }

         protected override void OnStop()
         {
             eventLog1.WriteEntry(ServiceState.SERVICE_STOPPED.ToString());
         }

         public enum ServiceState
         {
             SERVICE_STOPPED = 0x00000001,
             SERVICE_START_PENDING = 0x00000002,
             SERVICE_STOP_PENDING = 0x00000003,
             SERVICE_RUNNING = 0x00000004,
             SERVICE_CONTINUE_PENDING = 0x00000005,
             SERVICE_PAUSE_PENDING = 0x00000006,
             SERVICE_PAUSED = 0x00000007,

         }

         [StructLayout(LayoutKind.Sequential)]
         public struct ServiceStatus
         {
             public int dwServiceType;
             public ServiceState dwCurrentState;
             public int dwControlsAccepted;
             public int dwWin32ExitCode;
             public int dwServiceSpecificExitCode;
             public int dwCheckPoint;
             public int dwWaitHint;
         };

         [DllImport("advapi32.dll", SetLastError = true)]
         private static extern bool SetServiceStatus(IntPtr handle, ref ServiceStatus serviceStatus);
     }*/
