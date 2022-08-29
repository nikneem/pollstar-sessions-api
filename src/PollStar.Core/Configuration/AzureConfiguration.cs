namespace PollStar.Core.Configuration;

public class AzureConfiguration
{
    public const string SectionName = "Azure";
    public string StorageAccount { get; set; } = default!;
    public string StorageKey { get; set; } = default!;
    public string WebPubSub { get; set; } = default!;
    public string PollStarHub { get; set; } = default!;
}