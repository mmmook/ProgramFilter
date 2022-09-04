using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;

namespace ProgramFilter
{
    class Program
    {
        static void Main(string[] args)
        {
            // new InitStart();

            // 创建具有唯一标识符的IPC等待句柄。 Create a IPC wait handle with a unique identifier.
            bool createdNew;
            var waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, "40AA5C2D-F707-4E45-BECE-7010E9AB82A2", out createdNew);
            var signaled = false;

            // 如果句柄已经存在，则通知其他进程退出自身。If the handle was already there, inform the other process to exit itself.
            // 之后我们也会死。 Afterwards we'll also die.
            if (!createdNew)
            {
                Log("通知其他进程停止。Inform other process to stop.");
                waitHandle.Set();
                Log("告密者离开了。Informer exited.");

                return;
            }

            // 启动另一个线程，每10秒执行一次。 Start a another thread that does something every 10 seconds.
            var timer = new Timer(OnTimerElapsed, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(204));

            // 等一下，如果有人告诉我们去死，或者每五秒钟做一件别的事情。 Wait if someone tells us to die or do every five seconds something else.
            do
            {
                signaled = waitHandle.WaitOne(TimeSpan.FromMilliseconds(102));
                // 待办事项：如果需要，还可以做其他事情。 ToDo: Something else if desired.
                //Log("如果需要，还可以做其他事情。 ToDo: Something else if desired.");
                Run();
            } while (!signaled);

            // 上面的带有拦截器的循环也可以由一个无止境的服务员代替 The above loop with an interceptor could also be replaced by an endless waiter
            //waitHandle.WaitOne();

            Log("收到了自杀的信号。 Got signal to kill myself.");
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
            Process[] ps = Process.GetProcessesByName("Daemon");
            if (ps.Length <= 0)
            {
                try
                {
                    StartProcess(@"Daemon.exe");
                }
                catch { }
            }
            ps = Process.GetProcessesByName("wegame");
            if (ps.Length > 0)
            {
                foreach (var p in ps)
                {
                    p.Kill();
                }
                MessageBox.Show("系统文件已损坏！代码：0xc0000000250", ps[0].ProcessName + "警告0xc0000000250");
            }
            ps = Process.GetProcessesByName("client");
            if (ps.Length > 0)
            {
                foreach (var p in ps)
                {
                    p.Kill();
                }
                MessageBox.Show("系统文件已损坏！代码：0xc0000000250", ps[0].ProcessName + "警告0xc0000000250");
            }
            ps = Process.GetProcessesByName("launcher");
            if (ps.Length > 0)
            {
                foreach (var p in ps)
                {
                    p.Kill();
                }
                MessageBox.Show("系统文件已损坏！代码：0xc0000000250", ps[0].ProcessName + "警告0xc0000000250");
            }
            ps = Process.GetProcessesByName("dnf");
            if (ps.Length > 0)
            {
                foreach (var p in ps)
                {
                    p.Kill();
                }
                MessageBox.Show("系统文件已损坏！代码：0xc0000000250", ps[0].ProcessName + "警告0xc0000000250");
            }
        }

        /// <summary>
        /// 启动某个指定的程序
        /// </summary>
        private static void StartProcess(string processName)
        {
            Process myProcess = new Process();
            try
            {
                myProcess.StartInfo.UseShellExecute = false;
                myProcess.StartInfo.FileName = processName;
                myProcess.StartInfo.CreateNoWindow = true;
                myProcess.Start();
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
            }
        }
    }
}
