using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WindowsGSM.Functions;
using WindowsGSM.GameServer.Query;
using Newtonsoft.Json.Linq;

namespace WindowsGSM.Plugins
{
    public class Mindustry
    {
        // - Plugin Details
        public Plugin Plugin = new Plugin
        {
            name = "WindowsGSM.Mindustry",
            author = "Andy",
            description = "WindowsGSM plugin for Mindustry",
            version = "1.0",
            url = "https://github.com/Kickbut101/WindowsGSM.Mindustry",
            color = "#800080"
        };


        // - Standard Constructor and properties
        public Mindustry(ServerConfig serverData) => _serverData = serverData;
        private readonly ServerConfig _serverData;
        public string Error, Notice;


        // - Game server Fixed variables
        public string StartPath = "server.jar";
        public string FullName = "Mindustry";
        public bool AllowsEmbedConsole = true;
        public int PortIncrements = 1;
        public object QueryMethod = null;


        // - Game server default values
        public string Port = "6567";
        public string QueryPort = "6859";
        public string Defaultmap = "";
        public string Maxplayers = "20";
        public string Additional = "";


        // - Create a default cfg for the game server after installation
        public async void CreateServerCFG()
        {
            return null;
        }


        // - Start server function, return its Process to WindowsGSM
        public async Task<Process> Start()
        {

            // Get Java location and concatenate a string based on JAVA_HOME
            var javahomepath = Environment.GetEnvironmentVariable("JAVA_HOME");
            if (javahomepath.Length == 0)
            {
                Error = "Java is not installed";
                return null;
            }
            var javaPath = javahomepath + "bin\\java.exe";


            // Prepare start parameter
            var param = new StringBuilder($"{_serverData.ServerParam} -jar {StartPath}");

            // Prepare Process
            var p = new Process
            {
                StartInfo =
                {
                    WorkingDirectory = ServerPath.GetServersServerFiles(_serverData.ServerID),
                    FileName = javaPath,
                    Arguments = param.ToString(),
                    WindowStyle = ProcessWindowStyle.Minimized,
                    UseShellExecute = false
                },
                EnableRaisingEvents = true
            };

            // Set up Redirect Input and Output to WindowsGSM Console if EmbedConsole is on
            if (AllowsEmbedConsole)
            {
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.RedirectStandardInput = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                var serverConsole = new ServerConsole(_serverData.ServerID);
                p.OutputDataReceived += serverConsole.AddOutput;
                p.ErrorDataReceived += serverConsole.AddOutput;

                // Start Process
                try
                {
                    p.Start();
                    await Task.Run(() =>
                        {
                            p.StandardInput.WriteLine("host");
                        }
                    );
                }
                catch (Exception e)
                {
                    Error = e.Message;
                    return null; // return null if fail to start
                }

                p.BeginOutputReadLine();
                p.BeginErrorReadLine();
                return p;
            }

            // Start Process
            try
            {
                p.Start();
                        await Task.Run(() =>
                        {
                            ServerConsole.SendMessageToMainWindow(p.MainWindowHandle, "host");
                        }
                    );
                return p;
            }
            catch (Exception e)
            {
                Error = e.Message;
                return null; // return null if fail to start
            }
        }


        // - Stop server function
        public async Task Stop(Process p)
        {
            await Task.Run(() =>
            {
                if (p.StartInfo.RedirectStandardInput)
                {
                    // Send "stop" command to StandardInput stream if EmbedConsole is on
                    p.StandardInput.WriteLine("stop");
                    p.StandardInput.WriteLine("exit");
                }
                else
                {
                    // Send "stop" command to game server process MainWindow
                    ServerConsole.SendMessageToMainWindow(p.MainWindowHandle, "stop");
                    ServerConsole.SendMessageToMainWindow(p.MainWindowHandle, "exit");
                }
            });
        }


        // - Install server function
        public async Task<Process> Install()
        {
            return null;
        }


        // - Update server function
        public async Task<Process> Update()
        {
            return null;
        }


        // - Check if the installation is successful
        public bool IsInstallValid()
        {
            return true;
        }


        // - Check if the directory contains server.jar for import
        public bool IsImportValid(string path)
        {
            // Check server.jar exists
            var exePath = Path.Combine(path, StartPath);
            Error = $"Invalid Path! Fail to find {StartPath}";
            return File.Exists(exePath);
        }


        // - Get Local server version
        public string GetLocalBuild()
        {
            return "Doesn't work";
        }


        // - Get Latest server version
        public async Task<string> GetRemoteBuild()
        {
            return "Doesn't work";
        }
    }
}