using System;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using AlarmServer.Core;
using AlarmServer.Core.Extensions;

namespace AlarmServerService
{
    /// <summary>
    /// Summary description for TCPSocketListener.
    /// </summary>
    public class AlarmSocketListener
    {
        /// <summary>
        /// Variables that are accessed by other classes indirectly.
        /// </summary>
        private Socket m_clientSocket = null;
        private bool m_stopClient = false;
        private Thread m_clientListenerThread = null;
        private bool m_markedForDeletion = false;

        /// <summary>
        /// Working Variables.
        /// </summary>
        private StringBuilder m_oneLineBuf = new StringBuilder();
        private DateTime m_lastReceiveDateTime;
        private DateTime m_currentReceiveDateTime;
        private AlarmServerConfiguration m_config;

        /// <summary>
        /// Client Socket Listener Constructor.
        /// </summary>
        /// <param name="clientSocket"></param>
        public AlarmSocketListener(Socket clientSocket, AlarmServerConfiguration config)
        {
            m_clientSocket = clientSocket;
            m_config = config;
        }

        /// <summary>
        /// Client SocketListener Destructor.
        /// </summary>
        ~AlarmSocketListener()
        {
            StopSocketListener();
        }

        /// <summary>
        /// Method that starts SocketListener Thread.
        /// </summary>
        public void StartSocketListener()
        {
            if (m_clientSocket != null)
            {
                m_clientListenerThread =
                    new Thread(new ThreadStart(SocketListenerThreadStart));

                m_clientListenerThread.Start();
            }
        }

        /// <summary>
        /// Thread method that does the communication to the client. This 
        /// thread tries to receive from client and if client sends any data
        /// then parses it and again wait for the client data to come in a
        /// loop. The recieve is an indefinite time receive.
        /// </summary>
        private void SocketListenerThreadStart()
        {
            int size = 0;
            Byte[] byteBuffer = new Byte[1024];

            m_lastReceiveDateTime = DateTime.Now;
            m_currentReceiveDateTime = DateTime.Now;

            Timer t = new Timer(new TimerCallback(CheckClient),
                null, 15000, 15000);

            while (!m_stopClient)
            {
                try
                {
                    size = m_clientSocket.Receive(byteBuffer);
                    m_currentReceiveDateTime = DateTime.Now;
                    OnReceiveData(byteBuffer, size);
                }
                catch (SocketException se)
                {
                    m_stopClient = true;
                    m_markedForDeletion = true;
                }
            }
            t.Change(Timeout.Infinite, Timeout.Infinite);
            t = null;
        }

        /// <summary>
        /// Method that stops Client SocketListening Thread.
        /// </summary>
        public void StopSocketListener()
        {
            if (m_clientSocket != null)
            {
                m_stopClient = true;
                m_clientSocket.Close();

                m_clientListenerThread.Join(1000);

                if (m_clientListenerThread.IsAlive)
                {
                    m_clientListenerThread.Abort();
                }
                m_clientListenerThread = null;
                m_clientSocket = null;
                m_markedForDeletion = true;
            }
        }

        /// <summary>
        /// Method that returns the state of this object i.e. whether this
        /// object is marked for deletion or not.
        /// </summary>
        /// <returns></returns>
        public bool IsMarkedForDeletion()
        {
            return m_markedForDeletion;
        }
        
        private void OnReceiveData(Byte[] byteBuffer, int size)
        {
            string data = Encoding.UTF8.GetString(byteBuffer, 0, size);

            if (string.IsNullOrWhiteSpace(data))
                return;
            
            Logger.WriteLine("[IN] TCP Command received. (" + data + ")");
            
            try
            {
                if(!data.Contains(":===:"))
                {
                    Logger.WriteLine("Input has wrong format.");
                    return;
                }

                string[] substrings = data.Split(new[] { ":===:" }, StringSplitOptions.None);
                
                string cl_identifier = substrings[0];
                string al_profile = substrings[1];


                var profile = m_config.AlarmProfiles.FindProfileById(al_profile);
                if (profile == null)
                    return;

                var client = m_config.RegisteredClients.FindClientById(cl_identifier);

                if (client != null)
                {
                    Logger.WriteLine("[IN] Client-ID " + cl_identifier + " identified. Sender is " + client.ClientLabel + ".");
                    Logger.WriteLine("[OUT] Call Response: " + profile.Call());
                    Logger.WriteLine("[OUT] Profile \"" + profile.ProfileName + "\" called."); }
                else
                    Logger.WriteLine("[IN] Identification for " + cl_identifier + " failed.");
            }
            catch(Exception ex) { Logger.WriteLine("[ERROR] " + ex.GetType().FullName + ": " + ex.Message + "\r\n" + ex.StackTrace); }
        }

        private void CheckClient(object o)
        {
            if (m_lastReceiveDateTime.Equals(m_currentReceiveDateTime))
            {
                this.StopSocketListener();
            }
            else
            {
                m_lastReceiveDateTime = m_currentReceiveDateTime;
            }
        }
    }
}