namespace Boinst.NAntExtensions.TeamCity
{
    using System;
    using System.Globalization;

    using NAnt.Core;
    using NAnt.Core.Attributes;

    /// <summary>
    /// The teamcity-progressmessage task echoes a message to the console,
    /// but when running under TeamCity, it uses the appropriate
    /// syntax to correctly report the progress message to 
    /// TeamCity. 
    /// </summary>
    [TaskName("teamcity-progressmessage")]
    public class ProgressMessageTask : Task
    {
        private Level level = Level.Info;

        /// <summary>
        /// The message to output.
        /// </summary>
        [TaskAttribute("message")]
        public string Message
        {
            get;
            set;
        }

        /// <summary>
        /// The logging level with which the message should be output. The default 
        /// is <see cref="Level.Info" />.
        /// </summary>
        /// <remarks>
        /// Provided because the "echo" task provides this property.
        /// </remarks>
        [TaskAttribute("level")]
        public Level MessageLevel
        {
            get
            {
                return this.level;
            }

            set
            {
                if (!Enum.IsDefined(typeof(Level), value))
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "An invalid level {0} was specified.", value));
                this.level = value;
            }
        }


        /// <summary>
        /// Outputs the message to the build log
        /// </summary>
        protected override void ExecuteTask()
        {
            this.Log(this.MessageLevel, this.Message);
            if (InTeamcity()) Console.WriteLine("##teamcity[progressMessage '{0}']", this.Message);
        }

        /// <summary>
        /// Are we currently in TeamCity?
        /// </summary>
        /// <returns>
        /// A boolean flag indicating if we are indeed running under TeamCity.
        /// </returns>
        private static bool InTeamcity()
        {
            return !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TEAMCITY_VERSION"));
        }
    }
}