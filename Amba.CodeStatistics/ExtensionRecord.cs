namespace Amba.CodeStatistics;

public class ExtensionRecord
{
    public long FilesCount { get; set; }
    public long TotalSize { get; set; }
    public string Extension { get; private set; }
    public ExtensionRecord(string extension)
    {
        Extension = extension;
    }
}
