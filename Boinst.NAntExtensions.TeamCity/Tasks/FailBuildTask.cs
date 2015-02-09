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
    /// <a href="https://confluence.jetbrains.com/display/TCD8/Build+Script+Interaction+with+TeamCity#BuildScriptInteractionwithTeamCity-ReportingBuildProblems" >see this page</a>.
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
            this.Log(Level.Error, "##teamcity[buildProblem description='{0}']", TeamCityMessageFormatter.EscapeInvalidCharacters(this.Message));
        }
    }
}