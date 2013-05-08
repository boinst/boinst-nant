namespace Boinst.NAntExtensions
{
    using System.Collections.Generic;
    using System.Diagnostics;

    using NAnt.Core;
    using NAnt.Core.Attributes;

    /// <summary>
    /// Executes child tasks quietly.
    /// </summary>
    [TaskName("quiet")]
    public class QuietTask : TaskContainer
    {
        /// <summary>
        /// A map used to store the initial "Level" of each of the loggers.
        /// </summary>
        private Dictionary<IBuildListener, Level> logLevelMap;

        /// <summary>
        /// Executes the task.
        /// </summary>
        protected override void ExecuteTask()
        {
            this.StoreLogLevel();
            this.SetLogLevel(Level.Error);
            try
            {
                this.ExecuteChildTasks();
            }
            finally
            {
                this.RestoreLogLevel();
            }
        }

        /// <summary>
        /// Store the initial level of each of the loggers to be restored later.
        /// </summary>
        private void StoreLogLevel()
        {
            Debug.Assert(this.logLevelMap == null, "StoreLogLevel was called twice.");

            this.logLevelMap = new Dictionary<IBuildListener, Level>();

            // Loop through each logger
            foreach (IBuildListener listener in this.Project.BuildListeners)
            {
                var logger = listener as IBuildLogger;
                if (logger == null) continue;
                this.logLevelMap[logger] = logger.Threshold;
            }
        }

        /// <summary>
        /// Assigns the log level on each logger.
        /// </summary>
        /// <param name="level">The new level.</param>
        private void SetLogLevel(Level level)
        {
            Debug.Assert(this.logLevelMap != null, "Call StoreLogLevel first.");

            // Loop through each logger
            foreach (IBuildListener listener in this.Project.BuildListeners)
            {
                var logger = listener as IBuildLogger;
                if (logger != null) logger.Threshold = level;
            }
        }

        /// <summary>
        /// Restore the log level on each logger to its initial level
        /// </summary>
        private void RestoreLogLevel()
        {
            Debug.Assert(this.logLevelMap != null, "Call StoreLogLevel first.");

            foreach (IBuildListener listener in this.Project.BuildListeners)
            {
                var logger = listener as IBuildLogger;
                if (logger != null) logger.Threshold = this.logLevelMap[logger];
            }

            this.logLevelMap = null;
        }
    }
}
