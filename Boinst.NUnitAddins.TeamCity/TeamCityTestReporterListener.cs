namespace Boinst.NUnitAddins.TeamCity
{
    using System;

    using NUnit.Core;

    /// <summary>
    /// Listens for test start/finish messages, and reports status to TeamCity.
    /// </summary>
    public class TeamCityTestReporterListener : EventListener
    {
        private string currentTest;

        public void RunStarted(string name, int testCount) {}
        public void RunFinished(TestResult result) {}
        public void RunFinished(Exception exception) { }
        public void UnhandledException(Exception exception) { }

        public void TestOutput(TestOutput testOutput)
        {
            if (this.currentTest == null) return;

            // Ignore TeamCity control messages. These are probably our own messages anyway.
            if (testOutput.Text.ToLowerInvariant().Contains("##teamcity")) return;

            // Ignore blank messages, or messages that consist of empty lines.
            if (string.IsNullOrWhiteSpace(testOutput.Text)) return;

            if (testOutput.Type == TestOutputType.Error)
                Console.WriteLine(TeamCityMessageFormatter.FormatTestErrorMessage(this.currentTest, testOutput.Text.Trim()));
            else if (testOutput.Type != TestOutputType.Trace)
                Console.WriteLine(TeamCityMessageFormatter.FormatTestOutputMessage(this.currentTest, testOutput.Text.Trim()));
        }

        public void SuiteStarted(TestName testName)
        {
            Console.WriteLine("##teamcity[testSuiteStarted name='{0}']", testName.FullName);
        }

        public void SuiteFinished(TestResult result)
        {
            Console.WriteLine("##teamcity[testSuiteFinished name='{0}']", result.Test.TestName.FullName);
        }

        /// <summary>
        /// Called before a test is started.
        /// </summary>
        public void TestStarted(TestName testName)
        {
            var testReportingName = this.RenameTest(testName.FullName);
            this.currentTest = testReportingName;
            Console.WriteLine(TeamCityMessageFormatter.FormatTestStartedMessage(testReportingName));
        }

        /// <summary>
        /// Called when a test has finished running.
        /// </summary>
        public void TestFinished(TestResult result)
        {
            this.currentTest = null;

            var testReportingName = this.RenameTest(result.Test.TestName.FullName);
            if (!result.IsSuccess) Console.WriteLine(TeamCityMessageFormatter.FormatTestFailedMessage(testReportingName, result.Message, result.Description ?? result.StackTrace));
            
            Console.WriteLine(TeamCityMessageFormatter.FormatTestFinishedMessage(testReportingName));
        }

        /// <summary>
        /// This extension allows the user to add a suffix to test names
        /// by defining the environment variable NUNIT_TEAMCITY_TEST_SUFFIX.
        /// </summary>
        public string RenameTest(string testname)
        {
            string suffix = Environment.GetEnvironmentVariable("NUNIT_TEAMCITY_TEST_SUFFIX");
            return string.IsNullOrWhiteSpace(suffix) ? testname : testname + suffix;
        }
    }
}