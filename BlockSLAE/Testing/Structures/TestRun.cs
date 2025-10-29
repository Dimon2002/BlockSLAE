namespace BlockSLAE.Testing.Structures;

public class TestInfo
{
    public string MethodName { get; init; } = string.Empty;
    public int SlaeNumber { get; init; }
    public int ThreadsCount { get; init; }
    public string SmoothingMethod { get; init; } = string.Empty;
    public IList<TestResult> Results { get; } = [];
    public long ElapsedMilliseconds { get; set; }
}