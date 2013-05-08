namespace Boinst.NAntExtensions
{
    using System.Security.AccessControl;
    using System.Security.Principal;
    using System.Threading;

    using NAnt.Core;
    using NAnt.Core.Attributes;

    /// <summary>
    /// Acquire a mutex, and execute any child tasks between locking and unlocking the mutex
    /// </summary>
    [TaskName("mutex")]
    public class MutexTask : TaskContainer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MutexTask"/> class.
        /// </summary>
        public MutexTask()
        {
            this.Global = true;
            this.Timeout = -1;
        }

        /// <summary>
        /// The name to use for the mutex.
        /// </summary>
        [TaskAttribute("name", Required = true)]
        public string MutexName { get; set; }

        /// <summary>
        /// Use a global mutex.
        /// Defaults to "true".
        /// </summary>
        [TaskAttribute("global", Required = false)]
        public bool Global { get; set; }

        /// <summary>
        /// The maximum amount of time to wait for the mutex,
        /// expressed in milliseconds. 
        /// Defaults to no time-out.
        /// </summary>
        [TaskAttribute("timeout", Required = false)]
        [Int32Validator(-1, int.MaxValue)]
        public int Timeout { get; set; }

        /// <summary>
        /// Executes the task.
        /// </summary>
        protected override void ExecuteTask()
        {
            if (this.MutexName.StartsWith("Global\\"))
                throw new BuildException("Do not prefix the mutex ID with \"Global\\\". A global mutex is obtained by setting the property \"global\" to \"true\" (which is the default).");

            // A global mutex requires the prefix "Global\\"
            string mname = !this.Global ? this.MutexName : string.Format("Global\\{0}", this.MutexName);

            var mutex = new Mutex(false, mname);

            // A true global mutex needs some security settings
            if (this.Global)
            {
                var allowEveryoneRule = new MutexAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), MutexRights.FullControl, AccessControlType.Allow);
                var securitySettings = new MutexSecurity();
                securitySettings.AddAccessRule(allowEveryoneRule);
                mutex.SetAccessControl(securitySettings);
            }

            try
            {
                if (!mutex.WaitOne(this.Timeout)) throw new BuildException("Timeout waiting on mutex \"" + mname + "\".");
            }
            catch (AbandonedMutexException)
            {
                this.Log(Level.Warning, "A previous process abandoned mutex \"{0}\"", mname);
            }

            try
            {
                this.ExecuteChildTasks();
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }
    }
}
