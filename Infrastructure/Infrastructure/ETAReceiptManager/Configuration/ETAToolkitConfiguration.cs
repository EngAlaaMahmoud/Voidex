namespace Infrastructure.ETAReceiptManager.Configuration;

public class ETAToolkitConfiguration
{
    public const string SectionName = "ETAToolkit";
    
    public string Environment { get; set; } = "Testing"; // Testing, Production
    public ETACredentials Credentials { get; set; } = new();
    public InitializeSettings Initialize { get; set; } = new();
}

public class ETACredentials
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string PosSerial { get; set; } = string.Empty;
    public string PosOsVersion { get; set; } = "Windows"; // Can be "linux-based" as shown
    public string PosModelFramework { get; set; } = string.Empty;
    public string PresharedKey { get; set; } = string.Empty;
}

public class InitializeSettings
{
    public bool SaveCredential { get; set; } = true;
    public bool ResumeWithInvalidCache { get; set; } = false;
    public int MaximumSubmissionDocumentCount { get; set; } = 10;
    public float CachLookupDurationInHours { get; set; } = 24.0f;
    
    // Note: Background jobs NOT supported by NuGet toolkit
    // Only supported by Docker container API
    public bool EnableBackgroundJobs { get; set; } = false;
} 