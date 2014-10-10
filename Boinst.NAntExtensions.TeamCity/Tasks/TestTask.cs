namespace Boinst.NAntExtensions.TeamCity
{
    using System;

    using NAnt.Core;
    using NAnt.Core.Attributes;

    [TaskName("teamcity-test")]
    public class TestTask : TaskContainer
    {
        protected override void ExecuteTask()
        {
            if (TeamCityMessageFormatter.InTeamcity())
                Console.WriteLine(TeamCityMessageFormatter.FormatTestStartedMessage(this.Name));

            try
            {
                base.ExecuteTask();
            }
            catch (BuildException ex)
            {
                if (TeamCityMessageFormatter.InTeamcity())
                {
                    Console.WriteLine(TeamCityMessageFormatter.FormatTestErrorMessage(this.Name, ex.ToString()));
                    Console.WriteLine(TeamCityMessageFormatter.FormatTestFailedMessage(this.Name, ex.Message, ex.ToString()));
                }

                throw;
            }

            if (TeamCityMessageFormatter.InTeamcity())
                Console.WriteLine(TeamCityMessageFormatter.FormatTestFinishedMessage(this.Name));
        }
    }
}
