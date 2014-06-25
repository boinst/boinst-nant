namespace Boinst.NUnitAddins.TeamCity
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

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
            var name = this.GetSuiteName(testName.Name);
            Console.WriteLine(TeamCityMessageFormatter.FormatSuiteStartedMessage(name));
        }

        public void SuiteFinished(TestResult result)
        {
            var name = this.GetSuiteName(result.Test.TestName.Name);
            Console.WriteLine(TeamCityMessageFormatter.FormatSuiteFinishedMessage(name));
        }

        private string GetSuiteName(string name)
        {
            if (Path.IsPathRooted(name)) return Path.GetFileName(name);
            else return name;
        }

        /// <summary>
        /// Called before a test is started.
        /// </summary>
        public void TestStarted(TestName testName)
        {
            var suites = this.GetSuiteNames(testName.Name).ToArray();

            var testReportingName = suites.First();
            this.currentTest = testReportingName;

            if (suites.Count() != 1)
            {
                foreach (var suite in suites)
                {
                    Console.WriteLine(TeamCityMessageFormatter.FormatSuiteStartedMessage(suite));
                }
            }

            Console.WriteLine(TeamCityMessageFormatter.FormatTestStartedMessage(testReportingName));
            Console.WriteLine(TeamCityMessageFormatter.FormatTestOutputMessage(testReportingName, "Starting test: " + testName.FullName));
        }

        /// <summary>
        /// Called when a test has finished running.
        /// </summary>
        public void TestFinished(TestResult result)
        {
            this.currentTest = null;

            var suites = this.GetSuiteNames(result.Test.TestName.Name).ToArray();

            var testReportingName = suites.First();
            if (!result.IsSuccess) Console.WriteLine(TeamCityMessageFormatter.FormatTestFailedMessage(testReportingName, result.Message, result.Description ?? result.StackTrace));

            Console.WriteLine(TeamCityMessageFormatter.FormatTestOutputMessage(testReportingName, "Test finished: " + result.Test.TestName.Name));
            Console.WriteLine(TeamCityMessageFormatter.FormatTestFinishedMessage(testReportingName));

            if (suites.Count() != 1)
            {
                foreach (var suite in suites.Reverse())
                {
                    Console.WriteLine(TeamCityMessageFormatter.FormatSuiteFinishedMessage(suite));
                }
            }
        }

        /// <summary>
        /// We break a parameterised test name into suite names so that TeamCity
        /// can handle it better.
        /// </summary>
        private IEnumerable<string> GetSuiteNames(string name)
        {
            var parts = name.Split(new[] { "\"", "(", ")", ",", " " }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var part in parts)
            {
                if (!char.IsLetter(part.First())) 
                    yield return "_" + part;
                else 
                    yield return part;
            }

            // This extension allows the user to add a suffix to test names
            // by defining the environment variable NUNIT_TEAMCITY_TEST_SUFFIX.
            string suffix = Environment.GetEnvironmentVariable("NUNIT_TEAMCITY_TEST_SUFFIX");
            if (!string.IsNullOrWhiteSpace(suffix)) yield return suffix;
        }
    }
}