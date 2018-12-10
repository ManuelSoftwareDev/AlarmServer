using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace AlarmServer.ConfigurationTool
{
    class Program
    {
        /// <summary>
        /// Default Main-Entry-Point
        /// </summary>
        static void Main(string[] args)
        {
            Console.Title = "AlarmServer Configuration Tool v1";
            if (args.Length == 0)
                InlineMode();
            else if (args.Length > 0)
            {
                StringBuilder singleLine = new StringBuilder();

                foreach (var part in args)
                    singleLine.Append(part.Trim() + " ");

                string singleLineStr = singleLine.ToString().Trim();

                Console.WriteLine(singleLineStr);

                foreach (var newPart in singleLineStr.Split('-'))
                    if (!string.IsNullOrWhiteSpace(newPart))
                        RunCommand(newPart);
            }
        }

        /// <summary>
        /// Inline-Mode Activator
        /// </summary>
        static void InlineMode(bool first = true)
        {
            if (first)
            {
                Console.WriteLine("Inline Mode - AlarmServer ConfigurationTool started");
                Console.WriteLine("Please input an Command and Continue with enter.");
            }

            var left = Console.CursorLeft;
            var top = Console.CursorTop;
            string command = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(command))
            {
                Console.SetCursorPosition(left, top);
                InlineMode(false);
                return;
            }
            if (command == "clear")
            {
                Console.Clear();
                InlineMode(true);
                return;
            }
            if (command == "exit")
                return;

            RunCommand(command.Trim());

            InlineMode(false);
        }

        /// <summary>
        /// Last Loaded Configuration
        /// </summary>
        static Core.AlarmServerConfiguration LoadedConfiguration { get; set; }

        /// <summary>
        /// Last Loaded Configuration's File Path
        /// </summary>
        static string LastLoadedConfigurationPath { get; set; }

        /// <summary>
        /// Runs a Command from the registered Pool
        /// </summary>
        static void RunCommand(string command)
        {
            try
            {
                string singleCommand = (command.Contains(" ") ? command.Split(' ').FirstOrDefault() : command).ToLower().Trim();
                if (singleCommand.StartsWith("--"))
                    singleCommand = singleCommand.Remove(0, 2).Trim();
                if (command == "inline")
                    InlineMode(true);
                else
                if (!Commands.ContainsKey(singleCommand))
                {
                    Console.WriteLine("Unknown Command \"" + singleCommand + "\"");
                    RunCommand("help");
                    return;
                }
                else
                    Commands[singleCommand](command);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.GetType().FullName + ": " + ex.Message);
#if DEBUG
                Console.WriteLine(ex.StackTrace);
#endif
            }
        }

        /// <summary>
        /// Registered Commands
        /// </summary>
        static Dictionary<string, Action<string>> Commands = new Dictionary<string, Action<string>>()
        {
            {
                "cfg.load", (command) =>
                {
                    string file = command.Remove(0, 8).Trim().Replace("[Default]", Core.AlarmServerConfiguration.GetDefaultConfigurationPath());

                    if(!File.Exists(file))
                        throw new FileNotFoundException();

                    LoadedConfiguration = (Core.AlarmServerConfiguration)file;
                    LastLoadedConfigurationPath = file;
                    Console.WriteLine("Config@\"" + file + "\" loaded.");
                }
            },
            {
                "cfg.new", (command) =>
                {
                    LoadedConfiguration = new Core.AlarmServerConfiguration();
                    LastLoadedConfigurationPath = Core.AlarmServerConfiguration.GetDefaultConfigurationPath();
                    Console.WriteLine("Config@\"[Default]\" created.");
                }
            },
            {
                "help", (command) =>
                {
                    Console.WriteLine("--- HELP ---");
                    Console.WriteLine("cfg.new - Creates a new Configuration");
                    Console.WriteLine("cfg.load <FileName> - Loads the Config from the File");
                    Console.WriteLine("cfg.save <FileName> - Saves the Config to the File");
                    Console.WriteLine("cfg.regclient <Name> = <ID> - Register a Alarm-Client");
                    Console.WriteLine("cfg.addprofile <Id>;<Name>;<Target>;<UserAgent> - Register a Alarm-Profile");
                    Console.WriteLine("cfg.setport <Port> - Set the Server's Port");
                    Console.WriteLine("cfg.setaddr <Address> - Set the Server's Address");
                    Console.WriteLine("cfg.full() [>> <FileName>] - Dumps the Config out. Optionally save it to File");
                    Console.WriteLine();
                }
            },
            {
                "cfg.save", (command) =>
                {
                    string file = "";
                    if(!command.Contains(" "))
                        file = LastLoadedConfigurationPath;
                    else
                        file = command.Remove(0, 8).Trim();

                    LastLoadedConfigurationPath = file;
                    LoadedConfiguration.Save(file);
                    Console.WriteLine("Config@\"" + file + "\" saved.");
                }
            },
            {
                "cfg.setport", (command) =>
                {
                    int port = int.Parse(command.Replace("cfg.setport", "").Trim());

                    if(port < 0 || port > 65535)
                        throw new ArgumentOutOfRangeException("port");

                    LoadedConfiguration.ServerPort = port;
                    Console.WriteLine("Config@\"" + LastLoadedConfigurationPath + "\" Server-Port set to " + port);
                }
            },
            {
                "cfg.setaddr", (command) =>
                {
                    var address = command.Replace("cfg.setaddr", "").Trim().Replace("localhost", "127.0.0.1");
                    if(!IPAddress.TryParse(address, out IPAddress addrParsed))
                        throw new FormatException();

                    LoadedConfiguration.ServerAddress = addrParsed.ToString();
                    Console.WriteLine("Config@\"" + LastLoadedConfigurationPath + "\" Server-Address set to " + addrParsed.ToString());
                }
            },
            {
                "cfg.regclient", (command) =>
                {
                    string clientData = command.Remove(0, 13).Trim();
                    string[] parts = clientData.Split('=');

                    Core.AlarmClient client = new Core.AlarmClient();

                    client.ClientLabel = parts[0];
                    client.ClientIdentifier = parts[1];

                    LoadedConfiguration.RegisterClient(client);
                    Console.WriteLine("Config@\"" + LastLoadedConfigurationPath + "\" Client \"" + client.ClientLabel + "\" registered.");
                }
            },
            {
                "cfg.addprofile", (command) =>
                {
                    string clientData = command.Remove(0, 14).Trim();
                    string[] parts = clientData.Split(';');

                    Core.AlarmProfile profile = new Core.AlarmProfile();

                    profile.ProfileId = parts[0];
                    profile.ProfileName = parts[1];
                    profile.ProfileTarget = parts[2];
                    profile.RequestAgent = parts[3];

                    LoadedConfiguration.AddProfile(profile);
                    Console.WriteLine("Config@\"" + LastLoadedConfigurationPath + "\" Profile \"" + profile.ProfileName + "\" created.");
                }
            },
            {
                "cfg.full()", (command) =>
                {
                    List<string> dump = new List<string>();
                    dump.Add("-- DUMP BEGIN --");
                    dump.Add("Configuration File v1");
                    dump.Add("");
                    dump.Add("Full Server-Address: " + LoadedConfiguration.ServerAddress + ":" + LoadedConfiguration.ServerPort);
                    if(LoadedConfiguration.RegisteredClients != null)
                    {
                        dump.Add("Registered Clients (" + LoadedConfiguration.RegisteredClients.Length + ")");
                        for(int i = 1; i <= LoadedConfiguration.RegisteredClients.Length; i++)
                        {
                            var client = LoadedConfiguration.RegisteredClients[i-1];
                            dump.Add(string.Format("[{0}] {1} = {2}", i, client.ClientLabel, client.ClientIdentifier));
                        }
                    }
                    dump.Add("");
                    if(LoadedConfiguration.AlarmProfiles != null)
                    {
                        dump.Add("Registered Alarm Profiles (" + LoadedConfiguration.AlarmProfiles.Length + ")");
                        for(int i = 1; i <= LoadedConfiguration.AlarmProfiles.Length; i++)
                        {
                            var profile = LoadedConfiguration.AlarmProfiles[i - 1];
                            dump.Add(string.Format("[{0}] {2} ({1}) = {3}", i, profile.ProfileId, profile.ProfileName, profile.ProfileTarget));
                        }
                    }
                    dump.Add("-- DUMP END --");

                    foreach(var line in dump)
                        Console.WriteLine(line);

                    string commandWithout = command.Remove(0, 10).Trim();
                    if(commandWithout.StartsWith(">>"))
                    {
                        string file = "";
                        File.WriteAllLines(file = commandWithout.Remove(0,2).Trim(), dump.ToArray());

                        Console.WriteLine("Dump saved to File @\"" + file + "\"");
                    }
                }
            }
        };
    }
}
