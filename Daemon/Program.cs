using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Security.AccessControl;
using System.Threading;

namespace Daemon
{
    class Program
    {
        private const string EVENT_HANDLE_NAME = "4193578F-46F2-4053-BB10-355E028DABC1";
        private const int TIMER_INTERVAL = 204;
        private const int WAIT_INTERVAL = 102;

        static void Main(string[] args)
        {
            new InitStart();

            try
            {
                using (var waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, EVENT_HANDLE_NAME, out bool createdNew))
                {
                    if (!createdNew)
                    {
                        Log("通知其他进程停止。");
                        waitHandle.Set();
                        Log("告密者离开了。");
                        return;
                    }

                    using (var timer = new Timer(OnTimerElapsed, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(TIMER_INTERVAL)))
                    {
                        var signaled = false;
                        do
                        {
                            signaled = waitHandle.WaitOne(TimeSpan.FromMilliseconds(WAIT_INTERVAL));
                            Run();
                        } while (!signaled);

                        Log("收到了自杀的信号。");
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"程序发生致命错误: {ex}");
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

        static void Run()
        {
            try
            {
                Process[] p = Process.GetProcessesByName("ProgramFilter");
                if (p.Length <= 0)
                {
                    StartProcess(@"ProgramFilter.exe");
                }
            }
            catch (Exception e)
            {
                Log($"运行时发生错误: {e.Message}");
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

    public class InitStart
    {
        public InitStart()
        {
            Init();
        }

        public void Init()
        {
            // 开机自启动
            var starupPath = GetType().Assembly.Location;//获得程序路径其他方式也可以
            try
            {
                var fileName = starupPath;
                var shortFileName = fileName.Substring(fileName.LastIndexOf('\\') + 1).Replace(".exe", "");
                string regeditRunPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
                //打开子键节点
                var myReg = Registry.LocalMachine.OpenSubKey(
                    regeditRunPath, RegistryKeyPermissionCheck.ReadWriteSubTree,
                    RegistryRights.FullControl);
                if (myReg == null)
                {
                    //如果子键节点不存在，则创建之
                    myReg = Registry.LocalMachine.CreateSubKey(regeditRunPath);
                }
                if (myReg != null && myReg.GetValue(shortFileName) != null)
                {
                    //在注册表中设置自启动程序
                    myReg.DeleteValue(shortFileName);
                    myReg.SetValue(shortFileName, fileName);
                }
                else if (myReg != null && myReg.GetValue(shortFileName) == null)
                {
                    myReg.SetValue(shortFileName, fileName);
                }
                
                myReg.Close();
                myReg.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"注册表操作失败: {ex}");
                throw;
            }
        }
    }

}
