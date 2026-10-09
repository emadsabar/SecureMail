namespace SmimeLifecycle.Domain.Enums;

public enum LifecycleStatus
{
    Eligible = 0,
    CsrGenerated = 1,
    SubmittedToCa = 2,
    Issued = 3,
    Installed = 4,
    PublishedToDirectory = 5,
    DirectorySyncPending = 6,
    Active = 7,
    Renewing = 8,
    Replacing = 9,
    Revoked = 10,
    Expired = 11,
    Failed = 12
}

public enum EnrollmentStatus
{
    Draft = 0,
    AwaitingCsr = 1,
    CsrReceived = 2,
    SubmittedToCa = 3,
    Issued = 4,
    Failed = 5,
    Cancelled = 6
}

public enum DirectoryPublicationStatus
{
    NotStarted = 0,
    InProgress = 1,
    Published = 2,
    Failed = 3
}

public enum OutlookClientProfile
{
    ClassicOutlookWindows = 0,
    NewOutlookWindows = 1,
    OutlookOnTheWeb = 2,
    OutlookMobile = 3,
    ExchangeActiveSync = 4
}

public enum OutlookReadinessStatus
{
    Unknown = 0,
    CertificatePresent = 1,
    SigningReady = 2,
    UnsupportedClient = 3,
    Failed = 4
}

public enum DirectoryAttribute
{
    UserCertificate = 0,
    UserSmimeCertificate = 1
}

public enum AuditActorType
{
    System = 0,
    Administrator = 1,
    HelpDesk = 2,
    Agent = 3,
    User = 4
}
