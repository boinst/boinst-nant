namespace Boinst.NUnitAddins.TeamCity
{
    using System;

    using NUnit.Core;

    /// <summary>
    /// Listens for test start/finish messages, and reports status to TeamCity.
    /// </summary>
    public class TeamCityTestReporterListener : EventListener
    {
        private string lastTest;

        public void RunStarted(string name, int testCount) {}
        public void RunFinished(TestResult result) {}
        public void RunFinished(Exception exception) { }
        public void UnhandledException(Exception exception) { }
        public void TestOutput(TestOutput testOutput)
        {
            if (this.lastTest == null) return;

            if (testOutput.Type == TestOutputType.Error)
                Console.WriteLine(TeamCityMessageFormatter.FormatTestErrorMessage(this.lastTest, testOutput.Text));
            else if (testOutput.Type == TestOutputType.Log || testOutput.Type == TestOutputType.Out)
                Console.WriteLine(TeamCityMessageFormatter.FormatTestOutputMessage(this.lastTest, testOutput.Text));
        }

        public void SuiteStarted(TestName testName)
        {
            Console.WriteLine("##teamcity[testSuiteStarted name='{0}']", testName.FullName);
        }

        public void SuiteFinished(TestResult result)
        {
            Console.WriteLine("##teamcity[testSuiteFinished name='{0}']", result.Test.TestName.FullName);
        }

        public void TestStarted(TestName testName)
        {
            var testReportingName = this.RenameTest(testName.FullName);
            this.lastTest = testReportingName;
            Console.WriteLine("##teamcity[testStarted name='{0}']", testReportingName);
        }

        public void TestFinished(TestResult result)
        {
            this.lastTest = null;

            var testReportingName = this.RenameTest(result.Test.TestName.FullName);
            if (result.IsFailure) Console.WriteLine(TeamCityMessageFormatter.FormatTestFailedMessage(testReportingName, result.Message, result.Description));

            Console.WriteLine(TeamCityMessageFormatter.FormatTestFinishedMessage(testReportingName));
        }

        public string RenameTest(string testname)
        {
            string suffix = Environment.GetEnvironmentVariable("NUNIT_TEAMCITY_TEST_SUFFIX");
            return string.IsNullOrWhiteSpace(suffix) ? testname : testname + suffix;
        }
    }
}