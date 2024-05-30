﻿namespace TrimTail
{
    internal class Program
    {
        private static readonly SemaphoreSlim sem = new(1);

        private static bool HasTrailingBlanks(in string file_path)
        {
            using var file = File.OpenText(file_path);
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

        private static void RemoveTrailingBlanks(in string file_path)
        {
            if (!HasTrailingBlanks(file_path)) return;

            string temp_path = Path.GetTempFileName();
            using (var orig_file = File.OpenText(file_path))
            using (var temp_file = File.CreateText(temp_path))
            {
                orig_file.BaseStream.Seek(-1, SeekOrigin.End);
                bool end_with_newline = (orig_file.BaseStream.ReadByte() == '\n');

                orig_file.BaseStream.Seek(0, SeekOrigin.Begin);
                orig_file.DiscardBufferedData();

                string? line;
                while ((line = orig_file.ReadLine()) != null)
                {
                    line = line.TrimEnd();
                    if (!orig_file.EndOfStream || end_with_newline)
                    {
                        temp_file.WriteLine(line);
                    }
                    else
                    {
                        temp_file.Write(line);
                    }
                }
            }
            File.Replace(temp_path, file_path, null);
        }

        private static void ProcessDir(string dir_path, HashSet<string> allowed_exts)
        {
            Console.WriteLine($"Processing directory: {dir_path}");

            var files = Directory.EnumerateFiles(dir_path, "*", SearchOption.AllDirectories)
                .AsParallel()
                .Where(file_path => allowed_exts.Contains(Path.GetExtension(file_path).ToLower()));

            Parallel.ForEach(files, file_path =>
            {
                RemoveTrailingBlanks(file_path);
                sem.Wait();
                Console.WriteLine(Path.GetRelativePath(dir_path, file_path));
                sem.Release();
            });
        }

        static void Main(string[] args)
        {
            var start = DateTime.Now;
            HashSet<string> allowed_exts;

            if (args.Length > 0)
            {
                allowed_exts = args.Select(x => x.ToLower()).ToHashSet();
            }
            else
            {
                allowed_exts = [".h", ".c", ".hpp", ".cpp"];
            }

            ProcessDir(Directory.GetCurrentDirectory(), allowed_exts);

            var end = DateTime.Now;
            var duration = (end - start).TotalMilliseconds;
            Console.WriteLine($"Elapsed time: {duration} ms");
        }
    }
}