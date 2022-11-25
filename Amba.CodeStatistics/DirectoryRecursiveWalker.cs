using System.Collections.Concurrent;

namespace Amba.CodeStatistics;

public class DirectoryRecursiveWalker
{
    public Func<string, bool> FilterDirectory { get; set; } = x => false; 
    public IEnumerable<FileInfo> WalkDirectoryTreeSync(string root)
    {
        Stack<string> dirs = new(20);
        if (!Directory.Exists(root))
        {
            throw new ArgumentException();
        }
        dirs.Push(root);
        while (dirs.Any())
        {
            string currentDir = dirs.Pop();
            
            var subDirs = GetSubDirectories(currentDir);
            if (subDirs != null)
            {
                foreach (string dir in subDirs)
                {
                    if (FilterDirectory != null && FilterDirectory(dir))
                        continue;
                    dirs.Push(dir);
                }
            }

            string[]? files = GetFiles(currentDir);
            if (files == null)
            {
                continue;
            }

            foreach (string file in files)
            {
                FileInfo fi;
                try
                {
                    fi = new System.IO.FileInfo(file);
                }
                catch (System.IO.FileNotFoundException e)
                {
                    Console.WriteLine(e.Message);
                    continue;
                }
                yield return fi;
            }
            
        }
    }

    private string[]? GetFiles(string currentDir)
    {
        try
        {
            var files = System.IO.Directory.GetFiles(currentDir);
            return files;
        }
        catch (UnauthorizedAccessException e)
        {
            Console.WriteLine(e.Message);
            return null;
        }
    }

    string[]? GetSubDirectories(string currentDir)
    {
        try
        {
            var subDirs = System.IO.Directory.GetDirectories(currentDir);
            return subDirs;
        }
        catch (UnauthorizedAccessException e)
        {
            Console.WriteLine(e.Message);
            return null;
        }
        catch (System.IO.DirectoryNotFoundException e)
        {
            Console.WriteLine(e.Message);
            return null;
        }
    }
}
