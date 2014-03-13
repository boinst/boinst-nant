namespace Boinst.NAntExtensions.TeamCity
{
    using NAnt.Core;
    using NAnt.Core.Attributes;

    /// <summary>
    /// Fail a TeamCity build.
    /// </summary>
    /// <remarks>
    /// Invoking this task causes TeamCity to consider a build "failed"
    /// without stopping it immediately.
    /// </remarks>
    [TaskName("teamcity-failbuild")]
    public class FailBuildTask : Task
    {
        /// <summary>
        /// The message to incorporate
        /// </summary>
        [TaskAttribute("message")]
        public string Message
        {
            get;
            set;
        }

        protected override void ExecuteTask()
        {
            this.Log(Level.Error, "##teamcity[buildStatus status='FAILURE' text='{0}']", TeamCityMessageFormatter.EscapeInvalidCharacters(this.Message));
        }
    }
}