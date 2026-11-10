namespace SS.Functions;

/// <summary>Settings under "SlaBreachChecker" (app setting SlaBreachChecker__BatchSize, ...).</summary>
public sealed class SlaBreachCheckerOptions
{
    public const string SectionName = "SlaBreachChecker";

    /// <summary>Most tickets reported per run; the rest are picked up by the next run.</summary>
    public int BatchSize { get; set; } = 100;
}
