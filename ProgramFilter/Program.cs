using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;

namespace ProgramFilter
{
    class Program
    {
        private static readonly string EVENT_HANDLE_NAME = "40AA5C2D-F707-4E45-BECE-7010E9AB82A2";
        private static readonly int TIMER_INTERVAL = 204;
        private static readonly int WAIT_INTERVAL = 102;
        private static Timer _timer;

        private static readonly string CONFIG_FILE_PATH = "killlist.txt";
        private static readonly List<string> _processesToKill = new List<string> { "wegame", "client", "launcher", "dnf" };


        static void Main(string[] args)
        {
            try
            {
                LoadKillList();

                using (var waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, EVENT_HANDLE_NAME, out bool createdNew))
                {
                    if (!createdNew)
                    {
                        Log("通知其他进程停止。");
                        waitHandle.Set();
                        Log("告密者离开了。");
                        return;
                    }

                    _timer = new Timer(OnTimerElapsed, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(TIMER_INTERVAL));

                    bool signaled;
                    do
                    {
                        signaled = waitHandle.WaitOne(TimeSpan.FromMilliseconds(WAIT_INTERVAL));
                        Run();
                    } while (!signaled);

                    Log("收到了自杀的信号。");
                }
            }
            finally
            {
                _timer?.Dispose();
            }
        }

        private static void Log(string message)
        {
            Console.WriteLine(DateTime.Now + ": " + message);
        }

        private static void OnTimerElapsed(object state)
        {
            // Log("计时器已过。Timer elapsed.");
        }

        private static void LoadKillList()
        {
            try
            {
                if (File.Exists(CONFIG_FILE_PATH))
                {
                    var customProcesses = File.ReadAllLines(CONFIG_FILE_PATH)
                        .Where(line => !string.IsNullOrWhiteSpace(line))
                        .Select(line => line.Trim().ToLower());
                    _processesToKill.AddRange(customProcesses);
                }
            }
            catch (Exception ex)
            {
                Log($"读取配置文件失败: {ex.Message}");
            }
        }

        static void Run()
        {
            CheckAndStartProcess("Daemon", @"Daemon.exe");
            foreach (var processName in _processesToKill)
            {
                KillProcess(processName);
            }
        }

        private static void CheckAndStartProcess(string processName, string processPath)
        {
            Process[] ps = Process.GetProcessesByName(processName);
            if (ps.Length <= 0)
            {
                try
                {
                    StartProcess(processPath);
                }
                catch (Exception e)
                {
                    Trace.WriteLine(e.Message);
                }
            }
        }

        private static void KillProcess(string processName)
        {
            foreach (var process in Process.GetProcessesByName(processName))
            {
                try
                {
                    using (process)
                    {
                        process.Kill();
                    }
                }
                catch (Exception e)
                {
                    Trace.TraceError($"结束进程 {processName} 失败: {e}");
                }
            }
        }

        /// <summary>
        /// 启动某个指定的程序
        /// </summary>
        private static void StartProcess(string processName)
        {
            using (Process myProcess = new Process())
            {
                try
                {
                    myProcess.StartInfo.UseShellExecute = false;
                    myProcess.StartInfo.FileName = processName;
                    myProcess.StartInfo.CreateNoWindow = true;
                    myProcess.Start();
                }
                catch (Exception e)
                {
                    Trace.TraceError($"启动进程 {processName} 失败: {e}");
                    throw;
                }
            }
        }
    }
}
