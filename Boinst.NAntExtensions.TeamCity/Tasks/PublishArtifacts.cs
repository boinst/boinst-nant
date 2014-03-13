namespace Boinst.NAntExtensions.TeamCity
{
    using System;
    using System.IO;
    using System.Text;

    using NAnt.Core;
    using NAnt.Core.Attributes;

    /// <summary>
    /// Publishes artifacts to TeamCity
    /// </summary>
    [TaskName("teamcity-publishartifacts")]
    public class PublishArtifactsTask : Task
    {
        /// <summary>
        /// The path to the artifact
        /// </summary>
        [TaskAttribute("artifact", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public string Artifact
        {
            get;
            set;
        }

        /// <summary>
        /// The artifact target folder
        /// </summary>
        [TaskAttribute("target")]
        public string Target
        {
            get;
            set;
        }

        /// <summary>
        /// The artifact target folder
        /// </summary>
        [TaskAttribute("name")]
        public string ArtifactName
        {
            get;
            set;
        }

        /// <summary>
        /// Publish the artifacts
        /// </summary>
        protected override void ExecuteTask()
        {
            // log the message: Artifact "ArtifactName" is ready for publishing at path "Path" (100MB)
            var message = new StringBuilder("Artifact ");
            if (!string.IsNullOrWhiteSpace(this.ArtifactName))
                message.AppendFormat("\"{0}\" ", this.ArtifactName);
            message.AppendFormat("is ready for publishing at path \"{0}\"", this.Artifact);
            if (File.Exists(this.Artifact))
                message.AppendFormat(" ({0})", GetFileSize(new FileInfo(this.Artifact).Length));
            message.Append(".");
            this.Log(Level.Info, message.ToString());

            if (!InTeamcity())
                return;

            // log the TeamCity control message
            this.Log(Level.Info,
                !string.IsNullOrWhiteSpace(this.Target)
                    ? string.Format("##teamcity[publishArtifacts '{0} => {1}']", Path.GetFullPath(this.Artifact), this.Target)
                    : string.Format("##teamcity[publishArtifacts '{0}']", Path.GetFullPath(this.Artifact)));
        }

        /// <summary>
        /// Are we currently in TeamCity?
        /// <br/>
        /// We don't log teamcity-specific messages if we're not running in TeamCity.
        /// </summary>
        private static bool InTeamcity()
        {
            return !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TEAMCITY_VERSION"));
        }

        private static string GetFileSize(double fileSizeInBytes)
        {
            if (fileSizeInBytes < 1.0) return "0B";
            string[] suffixes = { "B", "KiB", "MiB", "GiB", "TiB", "PiB", "EiB", "ZiB", "YiB" };
            int place = Convert.ToInt32(Math.Floor(Math.Log(fileSizeInBytes, 1024)));
            place = Math.Min(place, suffixes.Length - 1); // don't try to use anything larger than what we have in the suffixes array
            double num = Math.Round(fileSizeInBytes / Math.Pow(1024, place), 1);
            return string.Format("{0}{1}", num, suffixes[place]);
        }
    }
}

