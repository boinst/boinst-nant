namespace Boinst.NAntExtensions.TeamCity
{
    using NAnt.Core;
    using NAnt.Core.Attributes;

    [TaskName("teamcity-setbuildnumber")]
    public class SetBuildNumberTask : Task
    {
        /// <summary>
        /// The build number
        /// </summary>
        [TaskAttribute("value")]
        public string BuildNumber
        {
            get;
            set;
        }

        protected override void ExecuteTask()
        {
            this.Log(Level.Info, "##teamcity[buildNumber '{0}']", this.BuildNumber);
        }
    }
}
