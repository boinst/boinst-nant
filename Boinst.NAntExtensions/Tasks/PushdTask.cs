namespace Boinst.NAntExtensions
{
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;

    using Boinst.NAntExtensions.Support;

    using NAnt.Core;
    using NAnt.Core.Attributes;

    /// <summary>
    /// Executes child tasks quietly.
    /// </summary>
    [TaskName("pushd")]
    public class PushdTask : TaskContainer
    {
        private string originalDirectoryName;

        private string symlinkName;

        /// <summary>
        /// The name of the property containing the name of the directory to shorten.
        /// </summary>
        [TaskAttribute("property", Required = true)]
        public string DirectoryProperty { get; set; }

        /// <summary>
        /// Executes the task.
        /// </summary>
        protected override void ExecuteTask()
        {
            this.PushDirectory();
            try
            {
                this.ExecuteChildTasks();
            }
            finally
            {
                this.PopDirectory();
            }
        }

        protected override void Initialize()
        {
            base.Initialize();

            if (!Project.Properties.Contains(this.DirectoryProperty))
                throw new BuildException(string.Format("No property \"{0}\" is defined.", this.DirectoryProperty));
        }

        /// <summary>
        /// Store the initial level of each of the loggers to be restored later.
        /// </summary>
        private void PushDirectory()
        {
            this.originalDirectoryName = Project.Properties[this.DirectoryProperty];

            this.symlinkName = this.GetTempDirName(this.originalDirectoryName);

            JunctionPoint.Create(this.symlinkName, Path.GetFullPath(this.originalDirectoryName), true);

            Project.Properties[this.DirectoryProperty] = this.symlinkName;

            Project.Log(Level.Info, "Created symlink to alias path \"{0}\" to path \"{1}\"", this.symlinkName, this.originalDirectoryName);
        }

        /// <summary>
        /// Restore the log level on each logger to its initial level
        /// </summary>
        private void PopDirectory()
        {
            Project.Properties[this.DirectoryProperty] = this.originalDirectoryName;

            JunctionPoint.Delete(this.symlinkName);

            Project.Log(Level.Info, "Removed symlink \"{0}\".", this.symlinkName);
        }

        private string GetTempDirName(string input)
        {
            var inputBytes = Encoding.UTF8.GetBytes(input);
            var hasher = new SHA1Managed();
            var hashBytes = hasher.ComputeHash(inputBytes);
            var dn = BitConverter.ToString(hashBytes).Replace("-", string.Empty).ToLowerInvariant().Substring(0, 8);
            return Path.Combine(Path.GetPathRoot(Environment.SystemDirectory), dn);
        }
    }
}
