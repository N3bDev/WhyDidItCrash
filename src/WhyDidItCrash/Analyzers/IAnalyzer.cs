using WhyDidItCrash.Models;

namespace WhyDidItCrash.Analyzers;

public interface IAnalyzer
{
    string Name { get; }
    Task<IReadOnlyList<CrashEvent>> AnalyzeAsync(AppConfig config);
}
