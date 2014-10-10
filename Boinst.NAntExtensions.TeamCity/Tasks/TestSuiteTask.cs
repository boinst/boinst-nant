namespace Boinst.NAntExtensions.TeamCity
{
    using System;

    using NAnt.Core;
    using NAnt.Core.Attributes;

    /// <summary>
    /// Report to TeamCity that the enclosing tasks form a TestSuite
    /// </summary>
    [TaskName("teamcity-testsuite")]
    public class TestSuiteTask : TaskContainer
    {
        protected override void ExecuteTask()
        {
            if (TeamCityMessageFormatter.InTeamcity())
                Console.WriteLine(TeamCityMessageFormatter.FormatSuiteStartedMessage(this.Name));

            try
            {
                base.ExecuteTask();
            }
            finally
            {
                if (TeamCityMessageFormatter.InTeamcity())
                    Console.WriteLine(TeamCityMessageFormatter.FormatSuiteFinishedMessage(this.Name));
            }
        }
    }
}
