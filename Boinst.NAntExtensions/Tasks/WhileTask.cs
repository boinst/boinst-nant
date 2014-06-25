namespace Boinst.NAntExtensions
{
    using System.Threading;

    using NAnt.Core;
    using NAnt.Core.Attributes;

    /// <summary>
    /// "while"
    /// </summary>
    [TaskName("while")]
    public class WhileTask : TaskContainer
    {
        private string test;
        private int? iterations;
        private string iterator;
        private bool untilSuccess = false;
        private int sleep = 0;

        private BuildException storedException;

        /// <summary>
        /// The NAnt expression that should be used for the iterated item.
        /// </summary>
        [TaskAttribute("test", Required = false, ExpandProperties = false)]
        public string Test
        {
            get { return this.test; }
            set { this.test = value; }
        }

        /// <summary>
        /// The NAnt property name that should be used for the iterated item.
        /// </summary>
        [TaskAttribute("iterator", Required = false)]
        public string Iterator
        {
            get { return this.iterator; }
            set
            {
                this.iterator = value;
                if (string.IsNullOrWhiteSpace(this.iterator))
                    return;
                if (this.Properties.IsReadOnlyProperty(this.iterator))
                    throw new BuildException("Property is readonly! : " + this.test, this.Location);
            }
        }

        /// <summary>
        /// Maximum number of iterations to run
        /// </summary>
        [TaskAttribute("iterations", Required = false)]
        public int Iterations
        {
            get { return this.iterations.GetValueOrDefault(); }
            set { this.iterations = value; }
        }

        /// <summary>
        /// Seconds to sleep for between iterations
        /// </summary>
        [TaskAttribute("sleep", Required = false)]
        public double Sleep
        {
            get { return ((double)this.sleep) / 1000; }
            set { this.sleep = (int)(value * 1000.0); }
        }

        /// <summary>
        /// If "untilsuccess" is "true", the task will loop until the child tasks
        /// run successfully.
        /// </summary>
        [TaskAttribute("untilsuccess", Required = false)]
        [BooleanValidator]
        public bool UntilSuccess
        {
            get { return this.untilSuccess; }
            set { this.untilSuccess = value; }
        }

        /// <summary>
        /// Checks the limits 
        /// </summary>
        /// <param name="executioncount"></param>
        /// <returns></returns>
        private bool Loop(int executioncount)
        {
            bool loop = true;

            // Evaluate test. If "test" evaluates to "true", we can stop execution.
            if (!string.IsNullOrWhiteSpace(this.test))
            {
                string testExpanded = this.Properties.ExpandProperties(this.test, this.Location);
                if (!bool.TryParse(testExpanded, out loop))
                    throw new BuildException(string.Format("Expression {0} could not be evaluated as boolean True or False. It's value is \"{1}\"", this.test, testExpanded), this.Location);
            }

            // Check number of iterations
            if (this.iterations.HasValue && executioncount + 1 == this.iterations.Value)
                loop = false;

            if (!loop && this.untilSuccess)
                throw new BuildException("The task did not complete successfully.", this.Location, this.storedException);

            return loop;
        }

        protected override void ExecuteTask()
        {
            if (string.IsNullOrWhiteSpace(this.test) && !this.iterations.HasValue && !this.untilSuccess)
                throw new BuildException("Property, Iterations or UntilSuccess (or a combination) must be defined.");

            for (int executioncount = 0; this.Loop(executioncount); executioncount++)
            {
                // Sleep if a sleep time has been specified.
                // but not in the first iteration.
                if (this.sleep != 0 && executioncount != 0)
                {
                    this.Log(Level.Info, string.Format("Sleeping for {0:0.0} seconds.", this.sleep / 1000.0));
                    Thread.Sleep(this.sleep);
                }

                // store the iterator value so it can be accessed by the script
                if (!string.IsNullOrWhiteSpace(this.iterator))
                    this.Project.Properties[this.iterator] = (executioncount + 1).ToString();

                if (!this.untilSuccess)
                {
                    this.ExecuteChildTasks(); // If not "until success", then any exception is a failure condition.
                }
                else
                {
                    try
                    {
                        this.ExecuteChildTasks();
                        return; // If "until success", then we're successful at this point and can stop.
                    }
                    catch (BuildException ex)
                    {
                        this.storedException = ex;
                    }
                }
            }
        }
    }
}
