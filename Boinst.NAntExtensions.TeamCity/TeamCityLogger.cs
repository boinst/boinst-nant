namespace Boinst.NAntExtensions.TeamCity
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;

    using NAnt.Core;

    /// <summary>
    /// A logger that writes TeamCity control messages
    /// that allow TeamCity to format logs nicely.
    /// </summary>
    public class TeamCityLogger : IBuildLogger
    { 
        /// <summary>
        /// Holds a stack of reports for all running builds.
        /// </summary>
        private readonly Stack buildReports = new Stack();

        /// <summary>
        /// Tasks for which TeamCity "log blocks" should not be written for.
        /// </summary>
        private readonly List<string> tasksToSkipWritingBlocksFor = new List<string>(new[]
            {
                string.Empty, 
                "echo", 
                "property", 
                "include",
                "if"
            });

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamCityLogger" /> 
        /// class.
        /// </summary>
        public TeamCityLogger()
        {
            this.Threshold = Level.Info;
        }

        public Level Threshold { get; set; }

        public bool EmacsMode { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="TextWriter" /> to which the logger is 
        /// to send its output.
        /// </summary>
        /// <value>
        /// The <see cref="TextWriter" /> to which the logger sends its output.
        /// </value>
        public virtual TextWriter OutputWriter { get; set; }

        /// <summary>
        /// Flushes buffered build events or messages to the underlying storage.
        /// </summary>
        public void Flush()
        {
            if (this.OutputWriter != null) this.OutputWriter.Flush();
        }

        /// <summary>
        /// Signals that a build has started.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="BuildEventArgs" /> object that contains the event data.</param>
        /// <remarks>
        /// This event is fired before any targets have started.
        /// </remarks>
        public virtual void BuildStarted(object sender, BuildEventArgs e)
        {
            this.buildReports.Push(new BuildReport(DateTime.Now));
        }

        /// <summary>
        /// Signals that the last target has finished.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="BuildEventArgs" /> object that contains the event data.</param>
        /// <remarks>
        /// This event will still be fired if an error occurred during the build.
        /// </remarks>
        public virtual void BuildFinished(object sender, BuildEventArgs e)
        {
            Exception error = e.Exception;

            BuildReport report = (BuildReport)this.buildReports.Pop();

            if (error == null)
            {
                this.OutputMessage(Level.Info, string.Empty);
                if (report.Errors == 0 && report.Warnings == 0)
                {
                    this.OutputMessage(Level.Info, "BUILD SUCCEEDED");
                }
                else
                {
                    this.OutputMessage(Level.Info, string.Format(CultureInfo.InvariantCulture,
                        "ERRORS {0}, WARNINGS {1}",
                        report.Errors, report.Warnings));
                }
                this.OutputMessage(Level.Info, string.Empty);
            }
            else
            {
                this.OutputMessage(Level.Error, string.Empty);
                if (report.Errors == 0 && report.Warnings == 0)
                {
                    this.OutputMessage(Level.Error, "BUILD FAILED");
                }
                else
                {
                    this.OutputMessage(Level.Info, string.Format(CultureInfo.InvariantCulture,
                        "ERRORS {0}, WARNINGS {1}",
                        report.Errors, report.Warnings));
                }
                this.OutputMessage(Level.Error, string.Empty);

                if (error is BuildException)
                {
                    if (this.Threshold <= Level.Verbose)
                    {
                        this.OutputMessage(Level.Error, error.ToString());
                    }
                    else
                    {
                        if (error.Message != null)
                        {
                            this.OutputMessage(Level.Error, error.Message);
                        }

                        // output nested exceptions
                        Exception nestedException = error.InnerException;
                        while (nestedException != null && !String.IsNullOrEmpty(nestedException.Message))
                        {
                            this.OutputMessage(Level.Error, nestedException.Message);
                            nestedException = nestedException.InnerException;
                        }
                    }
                }
                else
                {
                    this.OutputMessage(Level.Error, "INTERNAL ERROR");
                    this.OutputMessage(Level.Error, string.Empty);
                    this.OutputMessage(Level.Error, error.ToString());
                }

                this.OutputMessage(Level.Error, string.Empty);
            }

            // output total build time
            TimeSpan buildTime = DateTime.Now - report.StartTime;
            this.OutputMessage(Level.Info, string.Format(CultureInfo.InvariantCulture,
                "Total elapsed time: {0} seconds" + Environment.NewLine,
                Math.Round(buildTime.TotalSeconds, 1)));

            // make sure all messages are written to the underlying storage
            this.Flush();
        }

        /// <summary>
        /// Signals that a target has started.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="BuildEventArgs" /> object that contains the event data.</param>
        public virtual void TargetStarted(object sender, BuildEventArgs e)
        {
            if (e == null || e.Target == null) return;

            Console.Out.WriteLine(TeamCityMessageFormatter.FormatBlockOpenedMessage(e.Target.Name));
            Console.Out.WriteLine(TeamCityMessageFormatter.FormatProgressStartMessage("Target: " + e.Target.Name));
        }

        /// <summary>
        /// Signals that a task has finished.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="BuildEventArgs" /> object that contains the event data.</param>
        /// <remarks>
        /// This event will still be fired if an error occurred during the build.
        /// </remarks>
        public virtual void TargetFinished(object sender, BuildEventArgs e)
        {
            if (e == null || e.Target == null) return;

            Console.Out.WriteLine(TeamCityMessageFormatter.FormatProgressFinishMessage("Target: " + e.Target.Name));
            Console.Out.WriteLine(TeamCityMessageFormatter.FormatBlockClosedMessage(e.Target.Name));
        }

        /// <summary>
        /// Signals that a task has started.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="BuildEventArgs" /> object that contains the event data.</param>
        public virtual void TaskStarted(object sender, BuildEventArgs e)
        {
            if (e == null || e.Task == null) return;
            if (this.ShouldSkipWritingBlocksForTask(e.Task)) return;

            Console.Out.WriteLine(TeamCityMessageFormatter.FormatBlockOpenedMessage(e.Task.Name));
        }

        /// <summary>
        /// Signals that a task has finished.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="BuildEventArgs" /> object that contains the event data.</param>
        /// <remarks>
        /// This event will still be fired if an error occurred during the build.
        /// </remarks>
        public virtual void TaskFinished(object sender, BuildEventArgs e)
        {
            if (e == null || e.Task == null) return;
            if (this.ShouldSkipWritingBlocksForTask(e.Task)) return;

            Console.Out.WriteLine(TeamCityMessageFormatter.FormatBlockClosedMessage(e.Task.Name));
        }

        /// <summary>
        /// For some tasks, we don't want TeamCity to have a dedicated log block for;
        /// these tasks just clutter up the logs otherwise.
        /// </summary>
        /// <param name="task">The task we're checking.</param>
        /// <returns>"true" if the task should not have a block written for it.</returns>
        private bool ShouldSkipWritingBlocksForTask(Task task)
        {
            if (task == null || task.Name == null) return true;
            return this.tasksToSkipWritingBlocksFor.Contains(task.Name.ToLowerInvariant());
        }

        /// <summary>
        /// Signals that a message has been logged.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="BuildEventArgs" /> object that contains the event data.</param>
        /// <remarks>
        /// Only messages with a priority higher or equal to the threshold of 
        /// the logger will actually be output in the build log.
        /// </remarks>
        public virtual void MessageLogged(object sender, BuildEventArgs e)
        {
            if (this.buildReports.Count > 0)
            {
                if (e.MessageLevel == Level.Error)
                {
                    BuildReport report = (BuildReport)this.buildReports.Peek();
                    report.Errors++;
                }
                else if (e.MessageLevel == Level.Warning)
                {
                    BuildReport report = (BuildReport)this.buildReports.Peek();
                    report.Warnings++;
                }
            }

            // output the message
            this.OutputMessage(e);
        }

        /// <summary>
        /// Empty implementation which allows derived classes to receive the
        /// output that is generated in this logger.
        /// </summary>
        /// <param name="message">The message being logged.</param>
        protected virtual void Log(string message)
        {
        }

        /// <summary>
        /// Outputs an indented message to the build log if its priority is 
        /// greater than or equal to the <see cref="Threshold" /> of the 
        /// logger.
        /// </summary>
        /// <param name="messageLevel">The priority of the message to output.</param>
        /// <param name="message">The message to output.</param>
        private void OutputMessage(Level messageLevel, string message)
        {
            this.OutputMessage(CreateBuildEvent(messageLevel, message));
        }

        /// <summary>
        /// Outputs an indented message to the build log if its priority is 
        /// greater than or equal to the <see cref="Threshold" /> of the 
        /// logger.
        /// </summary>
        /// <param name="e">The event to output.</param>
        private void OutputMessage(BuildEventArgs e)
        {
            if (e.MessageLevel < this.Threshold) return;

            string message = TeamCityMessageFormatter.FormatMessage(
                new TeamCityMessage 
                    {
                        Level = e.MessageLevel,
                        ErrorDetails = e.Exception == null ? string.Empty : e.Exception.ToString(),
                        Text = e.Message
                    });
            Console.Out.WriteLine(message);
        }

        private static BuildEventArgs CreateBuildEvent(Level messageLevel, string message)
        {
            return new BuildEventArgs { MessageLevel = messageLevel, Message = message };
        }
    }
}

