using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace dumper
{
    internal class Program
    {
        private const int PROCESS_QUERY_INFORMATION = 0x0400;
        private const int PROCESS_VM_READ = 0x0010;

        static void Main(string[] args)
        {
            string processName = ""; // Enter the process name here

            Console.WriteLine("Waiting for process to start...");
            List<string> initialDlls = new List<string>();

            while (true)
            {
                Process[] leagueProcesses = GetProcessesByName(processName);

                if (leagueProcesses.Length > 0)
                {
                    Console.WriteLine("Targeted process started. Scanning for new modules...");

                    bool isFirstScan = initialDlls.Count == 0; // check if it's the first scan

                    foreach (Process process in leagueProcesses)
                    {
                        string dumpFolder = @"C:\Users\USER\Desktop\rlbot crack\league\dmp\" + Path.GetFileNameWithoutExtension(process.MainModule.FileName);

                        List<string> currentDlls = GetDllsInProcess(process);

                        List<string> newDlls = isFirstScan ? currentDlls : currentDlls.Except(initialDlls).ToList();

                        if (newDlls.Count > 0)
                        {
                            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                            string newDllFolder = Path.Combine(dumpFolder, "New_dlls : " + timestamp);

                            if (!Directory.Exists(newDllFolder))
                            {
                                Directory.CreateDirectory(newDllFolder);
                            }

                            foreach (string newDllPath in newDlls)
                            {
                                try
                                {
                                    string modulePath = Path.Combine(newDllFolder, Path.GetFileName(newDllPath));
                                    File.Copy(newDllPath, modulePath, true);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Error copying module: {ex.Message}");
                                }
                            }

                            Console.WriteLine($"New modules dumped to folder: {newDllFolder}");
                        }

                        if (isFirstScan)
                        {
                            initialDlls.AddRange(currentDlls);
                        }
                    }
                }

                Thread.Sleep(100); // modify the delay here (1000 = 1 second)
            }
        }

        static Process[] GetProcessesByName(string processName)
        {
            return Process.GetProcessesByName(processName);
        }

        static List<string> GetDllsInProcess(Process process)
        {
            List<string> dlls = new List<string>();

            foreach (ProcessModule module in process.Modules)
            {
                if (module.FileName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                {
                    dlls.Add(module.FileName);
                }
            }

            return dlls;
        }
    }
}

// the name "LeagueProcesses" was because this started as a league cheat dumper, but it works for any process