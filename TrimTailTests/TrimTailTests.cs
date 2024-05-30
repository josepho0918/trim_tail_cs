using TrimTail;

namespace TrimTailTests
{
    [TestClass]
    public class TrimTailTests
    {
        [TestMethod]
        public void TestHasTrailingBlanks()
        {
            const string filePath = "test.txt";

            File.WriteAllText(filePath, "Test w/ trailing spaces    ");
            Assert.IsTrue(Program.HasTrailingBlanks(filePath));
            File.Delete(filePath);

            File.WriteAllText(filePath, "Test w/ trailing tabs\t\t");
            Assert.IsTrue(Program.HasTrailingBlanks(filePath));
            File.Delete(filePath);

            File.WriteAllText(filePath, "Test w/o trailing blanks");
            Assert.IsFalse(Program.HasTrailingBlanks(filePath));
            File.Delete(filePath);
        }

        [TestMethod]
        public void TestRemoveTrailingBlanks()
        {
            const string filePath = "test.txt";
            string content;

            File.WriteAllText(filePath, "Test w/ trailing blanks    ");
            Program.RemoveTrailingBlanks(filePath);
            content = File.ReadAllText(filePath);
            Assert.AreEqual("Test w/ trailing blanks", content);
            File.Delete(filePath);

            File.WriteAllText(filePath, "Test w/ trailing blanks and newline    \r\n");
            Program.RemoveTrailingBlanks(filePath);
            content = File.ReadAllText(filePath);
            Assert.AreEqual("Test w/ trailing blanks and newline\r\n", content);
            File.Delete(filePath);
        }

        [TestMethod]
        public void TestProcessDir()
        {
            const string dirPath = "testDir";
            Directory.CreateDirectory(dirPath);
            File.WriteAllText(Path.Combine(dirPath, "test1.txt"), "This is a test with trailing blanks    ");
            File.WriteAllText(Path.Combine(dirPath, "test2.txt"), "Another test with trailing blanks    ");
            Program.ProcessDir(dirPath, [".txt"]);
            var files = Directory.EnumerateFiles(dirPath, "*", SearchOption.AllDirectories);
            Assert.IsTrue(files.All(filePath => !File.ReadAllText(filePath).EndsWith(' ')));
            Directory.Delete(dirPath, true);
        }
    }
}