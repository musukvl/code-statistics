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

        [Option("--output-format", CommandOptionType.SingleValue, Description = "Output format (console, csv)")]
        public string OutputFormat { get; set; } = "console";

        [DirectoryExists]
        [Argument(0, Description = "Directory Path")]
        public string RootDirectory { get; set; } = ".";
        
        public CodeStatisticsCommand(FileSystemTraverseFactory walkerFactory)
        {
            _walkerFactory = walkerFactory;
        }
        
        public int OnExecute()
        {
            var knownFileSizes = new HashSet<string> { ".cs", ".sql", ".js", ".go", ".tf", ".ts", ".ps1", ".sh", ".bat", "Dockerfile", ".yml", ".yaml", ".json", ".xml", ".css", ".less", ".scss", ".csproj" };
            var ignoredDirectory = new HashSet<string> { ".git", ".vs", ".idea" };
            var extensionRecords = new Dictionary<string, ExtensionRecord>();
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
                if (!extensionRecords.ContainsKey(extension))
                    extensionRecords.Add(extension, new ExtensionRecord(extension){TotalSize = fileInfo.Length, FilesCount = 1});
                else
                {
                    extensionRecords[extension].TotalSize += fileInfo.Length;
                    extensionRecords[extension].FilesCount += 1;
                }
            }

            switch (OutputFormat)
            {
                case "csv":
                    DisplayCsv(extensionRecords);
                    break;
                default:
                    DisplayConsoleReport(extensionRecords);
                    break;
            }
            return 0;
        }

        private static void DisplayCsv(Dictionary<string, ExtensionRecord> extensionRecords)
        {
            Console.WriteLine($"Extension; Size; Files Count");
            
            foreach (var record in extensionRecords.OrderByDescending(x => x.Value.TotalSize))
            {
                decimal sizeInMb = (decimal)extensionRecords[record.Key].TotalSize / (1024 * 1024);
                Console.WriteLine($"{record.Key}; {sizeInMb:0.000}; {record.Value.FilesCount}");
            }
        }

        private static void DisplayConsoleReport(Dictionary<string, ExtensionRecord> extensionRecords)
        {
            var total = (decimal)extensionRecords.Sum(x => x.Value.TotalSize) / (1024 * 1024);
            
            foreach (var record in extensionRecords.OrderByDescending(x => x.Value.TotalSize))
            {
                decimal sizeInMb = (decimal)extensionRecords[record.Key].TotalSize / (1024 * 1024);
                Console.WriteLine($"{record.Key} {sizeInMb:0.000} MB");
            }

            Console.WriteLine($"Total: {total:0.000} MB");
        }
    }
}
