namespace TrimTail
{
    public class Program
    {
        private static readonly SemaphoreSlim sem = new(1);

        public static bool HasTrailingBlanks(in string filePath)
        {
            using var file = File.OpenText(filePath);
            string? line;
            while ((line = file.ReadLine()) != null)
            {
                if (!string.IsNullOrEmpty(line) && char.IsWhiteSpace(line[^1]))
                {
                    return true;
                }
            }

            return false;
        }

        public static void RemoveTrailingBlanks(in string filePath)
        {
            if (!HasTrailingBlanks(filePath)) return;

            string tempPath = Path.GetTempFileName();
            using (var origFile = File.OpenText(filePath))
            using (var tempFile = File.CreateText(tempPath))
            {
                origFile.BaseStream.Seek(-1, SeekOrigin.End);
                bool endWithNewline = (origFile.BaseStream.ReadByte() == '\n');

                origFile.BaseStream.Seek(0, SeekOrigin.Begin);
                origFile.DiscardBufferedData();

                string? line;
                while ((line = origFile.ReadLine()) != null)
                {
                    line = line.TrimEnd();
                    if (!origFile.EndOfStream || endWithNewline)
                    {
                        tempFile.WriteLine(line);
                    }
                    else
                    {
                        tempFile.Write(line);
                    }
                }
            }
            try
            {
                File.Replace(tempPath, filePath, null);
            }
            catch (IOException)
            {
                File.Copy(tempPath, filePath, true);
                File.Delete(tempPath);
            }
        }

        public static void ProcessDir(string dirPath, HashSet<string> allowedExts)
        {
            Console.WriteLine($"Processing directory: {dirPath}");

            var files = Directory.EnumerateFiles(dirPath, "*", SearchOption.AllDirectories)
                .AsParallel()
                .Where(filePath => allowedExts.Contains(Path.GetExtension(filePath).ToLower()));

            Parallel.ForEach(files, filePath =>
            {
                RemoveTrailingBlanks(filePath);
                sem.Wait();
                Console.WriteLine(Path.GetRelativePath(dirPath, filePath));
                sem.Release();
            });
        }

        static void Main(string[] args)
        {
            var start = DateTime.Now;
            HashSet<string> allowedExts;

            if (args.Length > 0)
            {
                allowedExts = args.Select(x => x.ToLower()).ToHashSet();
            }
            else
            {
                allowedExts = [".h", ".c", ".hpp", ".cpp"];
            }

            ProcessDir(Directory.GetCurrentDirectory(), allowedExts);

            var end = DateTime.Now;
            var duration = (end - start).TotalMilliseconds;
            Console.WriteLine($"Elapsed time: {duration} ms");
        }
    }
}