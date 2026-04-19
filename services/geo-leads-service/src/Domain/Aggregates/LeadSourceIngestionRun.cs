namespace GeoLeadsService.Domain.Aggregates;

public sealed class LeadSourceIngestionRun
{
    private LeadSourceIngestionRun() { }

    public LeadSourceIngestionRun(string sourceName)
    {
        Id = Guid.NewGuid();
        SourceName = sourceName;
        Status = "Started";
        StartedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public string SourceName { get; private set; } = string.Empty;
    public string Status { get; private set; } = string.Empty;
    public int FetchedCount { get; private set; }
    public string? ErrorMessage { get; private set; }
    public DateTimeOffset StartedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }

    public void Complete(int fetchedCount)
    {
        Status = "Completed";
        FetchedCount = fetchedCount;
        CompletedAt = DateTimeOffset.UtcNow;
        ErrorMessage = null;
    }

    public void Fail(string errorMessage)
    {
        Status = "Failed";
        ErrorMessage = errorMessage;
        CompletedAt = DateTimeOffset.UtcNow;
    }
}
