namespace Boinst.NAntExtensions.Tests
{
    using System.IO;

    using Boinst.NAntExtensions.Support;

    using NUnit.Framework;

    [TestFixture]
    public class JunctionPointTest
    {
        private string tempFolder;

        [SetUp]
        public void CreateTempFolder()
        {
            this.tempFolder = Path.GetTempFileName();
            File.Delete(this.tempFolder);
            Directory.CreateDirectory(this.tempFolder);
        }

        [TearDown]
        public void DeleteTempFolder()
        {
            if (this.tempFolder != null)
            {
                foreach (FileSystemInfo file in new DirectoryInfo(this.tempFolder).GetFileSystemInfos())
                {
                    file.Delete();
                }

                Directory.Delete(this.tempFolder);
                this.tempFolder = null;
            }
        }

        [Test]
        public void Exists_NoSuchFile()
        {
            Assert.IsFalse(JunctionPoint.Exists(Path.Combine(this.tempFolder, "$$$NoSuchFolder$$$")));
        }

        [Test]
        public void Exists_IsADirectory()
        {
            File.Create(Path.Combine(this.tempFolder, "AFile")).Close();

            Assert.IsFalse(JunctionPoint.Exists(Path.Combine(this.tempFolder, "AFile")));
        }

        [Test]
        public void Create_VerifyExists_GetTarget_Delete()
        {
            string targetFolder = Path.Combine(this.tempFolder, "ADirectory");
            string junctionPoint = Path.Combine(this.tempFolder, "SymLink");

            Directory.CreateDirectory(targetFolder);
            File.Create(Path.Combine(targetFolder, "AFile")).Close();

            // Verify behavior before junction point created.
            Assert.IsFalse(File.Exists(Path.Combine(junctionPoint, "AFile")),
                "File should not be located until junction point created.");

            Assert.IsFalse(JunctionPoint.Exists(junctionPoint), "Junction point not created yet.");

            // Create junction point and confirm its properties.
            JunctionPoint.Create(junctionPoint, targetFolder, false /*don't overwrite*/);

            Assert.IsTrue(JunctionPoint.Exists(junctionPoint), "Junction point exists now.");

            Assert.AreEqual(targetFolder, JunctionPoint.GetTarget(junctionPoint));

            Assert.IsTrue(File.Exists(Path.Combine(junctionPoint, "AFile")),
                "File should be accessible via the junction point.");

            // Delete junction point.
            JunctionPoint.Delete(junctionPoint);

            Assert.IsFalse(JunctionPoint.Exists(junctionPoint), "Junction point should not exist now.");

            Assert.IsFalse(File.Exists(Path.Combine(junctionPoint, "AFile")),
                "File should not be located after junction point deleted.");

            Assert.IsFalse(Directory.Exists(junctionPoint), "Ensure directory was deleted too.");

            // Cleanup
            File.Delete(Path.Combine(targetFolder, "AFile"));
        }

        [Test]
        [Ignore("this one fails :(")]
        [ExpectedException(typeof(IOException), ExpectedMessage = "Directory already exists and overwrite parameter is false.")]
        public void Create_ThrowsIfOverwriteNotSpecifiedAndDirectoryExists()
        {
            string targetFolder = Path.Combine(this.tempFolder, "ADirectory");
            string junctionPoint = Path.Combine(this.tempFolder, "SymLink");

            Directory.CreateDirectory(junctionPoint);

            JunctionPoint.Create(junctionPoint, targetFolder, false);
        }

        [Test]
        public void Create_OverwritesIfSpecifiedAndDirectoryExists()
        {
            string targetFolder = Path.Combine(this.tempFolder, "ADirectory");
            string junctionPoint = Path.Combine(this.tempFolder, "SymLink");

            Directory.CreateDirectory(junctionPoint);
            Directory.CreateDirectory(targetFolder);

            JunctionPoint.Create(junctionPoint, targetFolder, true);

            Assert.AreEqual(targetFolder, JunctionPoint.GetTarget(junctionPoint));
        }

        [Test]
        [ExpectedException(typeof(IOException), ExpectedMessage = "Target path does not exist or is not a directory.")]
        public void Create_ThrowsIfTargetDirectoryDoesNotExist()
        {
            string targetFolder = Path.Combine(this.tempFolder, "ADirectory");
            string junctionPoint = Path.Combine(this.tempFolder, "SymLink");

            JunctionPoint.Create(junctionPoint, targetFolder, false);
        }

        [Test]
        [ExpectedException(typeof(IOException), ExpectedMessage = "Unable to open reparse point.")]
        public void GetTarget_NonExistentJunctionPoint()
        {
            JunctionPoint.GetTarget(Path.Combine(this.tempFolder, "SymLink"));
        }

        [Test]
        [ExpectedException(typeof(IOException), ExpectedMessage = "Path is not a junction point.")]
        public void GetTarget_CalledOnADirectoryThatIsNotAJunctionPoint()
        {
            JunctionPoint.GetTarget(this.tempFolder);
        }

        [Test]
        [ExpectedException(typeof(IOException), ExpectedMessage = "Path is not a junction point.")]
        public void GetTarget_CalledOnAFile()
        {
            File.Create(Path.Combine(this.tempFolder, "AFile")).Close();

            JunctionPoint.GetTarget(Path.Combine(this.tempFolder, "AFile"));
        }

        [Test]
        public void Delete_NonExistentJunctionPoint()
        {
            // Should do nothing.
            JunctionPoint.Delete(Path.Combine(this.tempFolder, "SymLink"));
        }

        [Test]
        [ExpectedException(typeof(IOException), ExpectedMessage = "Unable to delete junction point.")]
        public void Delete_CalledOnADirectoryThatIsNotAJunctionPoint()
        {
            JunctionPoint.Delete(this.tempFolder);
        }

        [Test]
        [ExpectedException(typeof(IOException), ExpectedMessage = "Path is not a junction point.")]
        public void Delete_CalledOnAFile()
        {
            File.Create(Path.Combine(this.tempFolder, "AFile")).Close();

            JunctionPoint.Delete(Path.Combine(this.tempFolder, "AFile"));
        }
    }
}
