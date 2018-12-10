using System;
using System.IO;
using System.Xml.Serialization;
using AlarmServer.Core.Extensions;

namespace AlarmServer.Core
{
    [XmlRoot(ElementName = "server", Namespace = "as")]
    public class AlarmServerConfiguration
    {
        [XmlArray("registered_clients")]
        [XmlArrayItem("client")]
        public AlarmClient[] RegisteredClients { get; set; }

        [XmlArray("alarm_profiles")]
        [XmlArrayItem("profile")]
        public AlarmProfile[] AlarmProfiles { get; set; }

        [XmlAttribute("port")]
        public int ServerPort { get; set; } = 21012;

        [XmlAttribute("address")]
        public string ServerAddress { get; set; } = "127.0.0.1";

        /// <summary>
        /// Register a new Client
        /// </summary>
        /// <param name="client">Client to register</param>
        public void RegisterClient(AlarmClient client)
        {
            RegisteredClients = RegisteredClients.Add(client);
        }

        /// <summary>
        /// Adds a new Alarm Profile
        /// </summary>
        /// <param name="profile">AlarmProfile to add</param>
        public void AddProfile(AlarmProfile profile)
        {
            AlarmProfiles = AlarmProfiles.Add(profile);
        }

        /// <summary>
        /// Saves the current Configuration
        /// </summary>
        /// <param name="fileName">File location for this Configuration</param>
        public void Save(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                fileName = GetDefaultConfigurationPath();
            if (fileName.Contains("[Default]"))
                fileName = fileName.Replace("[Default]", GetDefaultConfigurationPath());

            if(!Directory.Exists(Path.GetDirectoryName(fileName)))
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(fileName));
                }
                catch { }

            using (StreamWriter writer = new StreamWriter(fileName))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(AlarmServerConfiguration));

                serializer.Serialize(writer, this);

                writer.Flush();
                writer.Close();
            }
        }

        /// <summary>
        /// Loads a Configuration Object from the File
        /// </summary>
        /// <param name="fileName">FileName from the Configuration</param>
        /// <returns>The loaded Configuration Object</returns>
        public static AlarmServerConfiguration Open(string fileName)
        {
            if (fileName.Contains("[Default]"))
                fileName = fileName.Replace("[Default]", GetDefaultConfigurationPath());

            AlarmServerConfiguration loadedConfiguration;
            using (StreamReader reader = new StreamReader(fileName))
            {
                XmlSerializer deserializer = new XmlSerializer(typeof(AlarmServerConfiguration));

                loadedConfiguration = (AlarmServerConfiguration)deserializer.Deserialize(reader);
            }

            return loadedConfiguration;
        }

        /// <summary>
        /// Converts a FileName string explicitly into a AlarmServerConfiguration
        /// </summary>
        /// <param name="fileName">FileName string</param>
        public static explicit operator AlarmServerConfiguration(string fileName)
        {
            if (!File.Exists(fileName))
                return null;

            return Open(fileName);
        }

        /// <summary>
        /// Gets the Default Configuration Path
        /// </summary>
        public static string GetDefaultConfigurationPath()
        {
            string defPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "AlarmServer", "Configuration", "ATXGHCI_20", "config.xml");
            string cfg = (Microsoft.Win32.Registry.GetValue("HKEY_CLASSES_ROOT\\AlarmServer\\Configuration\\DefaultConfig", "", defPath) ?? defPath).ToString();

            if(string.IsNullOrWhiteSpace(cfg))
                return defPath;

            return cfg;
        }
    }
}
