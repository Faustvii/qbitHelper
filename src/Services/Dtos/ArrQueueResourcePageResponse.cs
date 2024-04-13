using System.Text.Json.Serialization;

namespace QBitHelper.Services.Dtos;

public class QueueResourcePagingResource
{
    [JsonPropertyName("page")]
    public int Page { get; set; }

    [JsonPropertyName("pageSize")]
    public int PageSize { get; set; }

    [JsonPropertyName("totalRecords")]
    public int TotalRecords { get; set; }

    [JsonPropertyName("sortKey")]
    public string SortKey { get; set; }

    [JsonPropertyName("sortDirection")]
    public string SortDirection { get; set; }

    [JsonPropertyName("records")]
    public QueueRecord[] Records { get; set; }
}

public class QueueRecord
{
    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("sizeleft")]
    public long Sizeleft { get; set; }

    [JsonPropertyName("added")]
    public DateTime Added { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("trackedDownloadStatus")]
    public string TrackedDownloadStatus { get; set; }

    [JsonPropertyName("trackedDownloadState")]
    public string TrackedDownloadState { get; set; }

    [JsonPropertyName("errorMessage")]
    public string ErrorMessage { get; set; }

    [JsonPropertyName("downloadId")]
    public string DownloadId { get; set; }

    [JsonPropertyName("protocol")]
    public string Protocol { get; set; }

    [JsonPropertyName("downloadClient")]
    public string DownloadClient { get; set; }

    [JsonPropertyName("downloadClientHasPostImportCategory")]
    public bool DownloadClientHasPostImportCategory { get; set; }

    [JsonPropertyName("indexer")]
    public string Indexer { get; set; }

    [JsonPropertyName("episodeHasFile")]
    public bool EpisodeHasFile { get; set; }

    [JsonPropertyName("id")]
    public long Id { get; set; }
}
