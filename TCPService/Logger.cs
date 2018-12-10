using System;
using System.Globalization;
using System.IO;

namespace AlarmServerService
{
    public static class Logger
    {
        public static void LoadLogFile(string logFile)
        {
            try
            {
                if (File.Exists(logFile))
                    File.Delete(logFile);
            }
            catch { }

            FileStream stream = new FileStream(logFile, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
            LogWriter = new StreamWriter(stream, System.Text.Encoding.UTF8);
        }

        static StreamWriter LogWriter = null;

        public static void WriteLine(string line)
        {
            LogWriter.WriteLine("[" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture) + "] " + line);
            Flush();
        }

        public static void Flush()
        {
            LogWriter.Flush();
        }
        public static void FlushClose()
        {
            LogWriter.Flush();
            LogWriter.Close();
            LogWriter.Dispose();
            LogWriter = null;
        }
    }
}
