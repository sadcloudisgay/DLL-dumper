using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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
            // Ask for process name
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Enter the process name: ");
            string processName = Console.ReadLine();
            Console.ResetColor();

            string dumpFolder;

            do
            {
                // Ask for dump folder
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("Enter the dump folder: ");
                dumpFolder = Console.ReadLine();
                Console.ResetColor();

                if (!Directory.Exists(dumpFolder))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("The specified folder does not exist. Please enter a valid folder.");
                    Console.ResetColor();
                }
            }
            while (!Directory.Exists(dumpFolder));

            string initialDumpFolderPath = Path.Combine(dumpFolder, processName);
            if (!Directory.Exists(initialDumpFolderPath))
            {
                Directory.CreateDirectory(initialDumpFolderPath);
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Process found: {processName}\nInitial dump folder set to: {initialDumpFolderPath}\n");
            Console.ResetColor();

            Dictionary<string, string> initialDlls = new Dictionary<string, string>();

            while (true)
            {
                Process[] leagueProcesses = GetProcessesByName(processName);

                if (leagueProcesses.Length > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("\nTargeted process found. ");
                    Console.ResetColor();
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("Scanning for new modules...\n");
                    Console.ResetColor();
                    Console.Title = $"Targeted process: {processName}.exe | Scanning for new modules ... ";

                    string currentScanFolder = initialDumpFolderPath; // Assume no new DLLs initially

                    foreach (Process process in leagueProcesses)
                    {
                        List<string> currentDlls = GetDllsInProcess(process);

                        foreach (string newDllPath in currentDlls)
                        {
                            if (!initialDlls.ContainsKey(newDllPath))
                            {
                                if (currentScanFolder == initialDumpFolderPath)
                                {
                                    // Create a new folder only if a new DLL is found
                                    string scanTimestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                                    currentScanFolder = Path.Combine(dumpFolder, $"Scan_{scanTimestamp}");
                                    Directory.CreateDirectory(currentScanFolder);
                                }

                                try
                                {
                                    string modulePath = Path.Combine(currentScanFolder, Path.GetFileName(newDllPath));
                                    File.Copy(newDllPath, modulePath, true);
                                }
                                catch (Exception ex)
                                {
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine($"Error copying module: {ex.Message}");
                                    Console.ResetColor();
                                }

                                initialDlls[newDllPath] = newDllPath;
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine($"New module dumped to folder: {currentScanFolder}");
                                Console.ResetColor();
                            }
                        }
                    }
                }

                Thread.Sleep(100); // Modify the delay here (1000 = 1 second)
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
