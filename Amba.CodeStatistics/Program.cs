using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;

namespace Amba.CodeStatistics;

class Program
{
    static int Main(string[] args)
    {
        var services = new ServiceCollection()
            .AddSingleton<FileSystemTraverseFactory>()
            .AddSingleton<IConsole>(PhysicalConsole.Singleton)
            .BuildServiceProvider();

        var app = new CommandLineApplication<CodeStatisticsCommand>();
        app.Conventions
            .UseDefaultConventions()
            .UseConstructorInjection(services);
        return app.Execute(args);
    }

    [Command("code-statistics", Description = "Generates code statistics")]
    [HelpOption("--help|-h")]
    public class CodeStatisticsCommand
    {
        private readonly FileSystemTraverseFactory _walkerFactory;

        
        [DirectoryExists]
        [Argument(0, Description = "Directory Path")]
        public string RootDirectory { get; set; }
        
        public CodeStatisticsCommand(FileSystemTraverseFactory walkerFactory)
        {
            _walkerFactory = walkerFactory;
        }
        
        public int OnExecute()
        {
            var knownFileSizes = new HashSet<string> { ".cs", ".sql", ".js", ".go", ".tf", ".ts", ".ps1", ".sh", ".bat", "Dockerfile", ".yml", ".yaml", ".json", ".xml", ".css", ".less", ".scss", ".csproj" };
            var ignoredDirectory = new HashSet<string> { ".git", ".vs", ".idea" };
            var sizes = new Dictionary<string, long>();
            var walker = new DirectoryRecursiveWalker();
            walker.FilterDirectory = s =>
            {
                var dirInfo = new DirectoryInfo(s);
                return ignoredDirectory.Contains(dirInfo.Name);
            };
            foreach (var fileInfo in walker.WalkDirectoryTreeSync(RootDirectory))
            {
                var extension = fileInfo.Extension.ToLowerInvariant();
                if (!knownFileSizes.Contains(extension))
                    continue;
                if (!sizes.ContainsKey(extension))
                    sizes.Add(extension, fileInfo.Length);
                else 
                    sizes[extension] += fileInfo.Length;
            }
            
            foreach (var record in sizes.OrderByDescending(x => x.Value))
            {
                decimal sizeInMb = (decimal)sizes[record.Key] / (1024 * 1024);
                Console.WriteLine($"{record.Key} {sizeInMb:N} MB");
            }

            var total = (decimal)sizes.Sum(x => x.Value) / (1024 * 1024);
            
            Console.WriteLine($"Total: {total:N} MB");
            
            return 0;
        }

         
    }
}
