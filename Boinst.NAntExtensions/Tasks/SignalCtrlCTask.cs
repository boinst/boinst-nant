namespace Boinst.NAntExtensions
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    using NAnt.Core;
    using NAnt.Core.Attributes;

    /// <summary>
    /// Send a "CTRL+C" signal to a process, and wait for it to terminate
    /// </summary>
    /// <remarks>
    /// Actually sends a "CTRL+C" event to all processes that share the current console.
    /// The Process ID is used only to wait for the process to terminate. 
    /// As such, you can't use this to stop one process while allowing others to continue.
    /// <br/>
    /// Some great information for developing this task was obtained 
    /// <a href="http://stanislavs.org/stopping-command-line-applications-programatically-with-ctrl-c-events-from-net/">from this article</a>.
    /// <br/>
    /// We cannot report the exit code of the terminated process because we don't have
    /// a handle to the original "Process" object that started the process.
    /// </remarks>
    [TaskName("signal-ctrlc")]
    public class SignalCtrlCTask : Task
    {
        /// <summary>
        /// The ID of the Process to send the "ctrl+c" to.
        /// </summary>
        [TaskAttribute("pid", Required = true)]
        public int ProcessId { get; set; }

        /// <summary>
        /// Time in milliseconds to wait for the process to stop.
        /// </summary>
        [TaskAttribute("timeout")]
        public int Timeout { get; set; }

        /// <summary>
        /// Executes the task.
        /// </summary>
        protected override void ExecuteTask()
        {
            this.Log(Level.Info, "Stopping process with PID: " + this.ProcessId);

            try
            {
                Process.GetProcessById(this.ProcessId);
            }
            catch (ArgumentException ex)
            {
                // We get here if there is no process with that ID currently running
                this.Log(Level.Info, ex.Message);
                return;
            }

            this.SendCtrlC(this.ProcessId);
        }

        /// <summary>
        /// Send a CTRL+C event
        /// </summary>
        private void SendCtrlC(int processId)
        {
            var process = Process.GetProcessById(processId);
            
            // Disable Ctrl-C handling for our program.
            // We don't want to respond to the Ctrl-C event ourselves.
            NativeMethods.SetConsoleCtrlHandler(null, true);
            NativeMethods.GenerateConsoleCtrlEvent(NativeMethods.ConsoleCtrlEvent.CTRL_C_EVENT, 0);

            // Must wait here. If we don't wait, and we re-enable Ctrl-C handling below too fast, we might terminate ourselves.
            var timeout = Math.Min(200, this.Timeout == 0 ? int.MaxValue : this.Timeout);
            if (!process.WaitForExit(timeout))
            {
                this.Log(Level.Warning, "Failed to stop process with id " + processId + "! Process will be force-killed.");
                process.Kill();
            }
            
            // Re-enable Ctrl-C handling or any subsequently started programs will inherit the disabled state.
            NativeMethods.SetConsoleCtrlHandler(null, false);
        }

        /// <summary>
        /// Native methods that we call
        /// </summary>
        private static class NativeMethods
        {
            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool GenerateConsoleCtrlEvent(ConsoleCtrlEvent sigevent, int dwProcessGroupId);
           
            [DllImport("kernel32.dll")]
            public static extern bool SetConsoleCtrlHandler(ConsoleCtrlDelegate handlerRoutine, bool add);

            /// <summary>
            /// Delegate type to be used as the Handler Routine for SCCH 
            /// </summary>
            internal delegate Boolean ConsoleCtrlDelegate(ConsoleCtrlEvent ctrlType);

            /// <summary>
            /// Enumerated type for the control messages sent to the handler routine
            /// </summary>
            public enum ConsoleCtrlEvent : uint
            {
                CTRL_C_EVENT = 0,
                CTRL_BREAK_EVENT,
                CTRL_CLOSE_EVENT,
                CTRL_LOGOFF_EVENT = 5,
                CTRL_SHUTDOWN_EVENT
            }
        }
    }
}