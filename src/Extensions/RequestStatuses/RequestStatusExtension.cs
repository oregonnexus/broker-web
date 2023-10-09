
using OregonNexus.Broker.Domain;

namespace OregonNexus.Broker.Web.Extensions.RequestStatuses;

public static class RequestStatusFriendlyString
{
    public const string Draft = "Draft";
    public const string WaitingToSend = "Waiting To Send";
    public const string Sending = "Sending";
    public const string Sent = "Sent";
    public const string WaitingApproval = "Waiting Approval";
    public const string Approved = "Approved";
    public const string Denied = "Denied";
    public const string Declined = "Declined";
    public const string Received = "Received";
    public const string WaitingToImport = "Waiting To Import";
    public const string Importing = "Importing";
    public const string Imported = "Imported";
}

public static class RequestStatuses
{
    public static string ToFriendlyString(this RequestStatus requestStatus)
    {
        return requestStatus switch
        {
            RequestStatus.Draft => RequestStatusFriendlyString.Draft,
            RequestStatus.WaitingToSend => RequestStatusFriendlyString.WaitingToSend,
            RequestStatus.Sending => RequestStatusFriendlyString.Sending,
            RequestStatus.Sent => RequestStatusFriendlyString.Sent,
            RequestStatus.WaitingApproval => RequestStatusFriendlyString.WaitingApproval,
            RequestStatus.Approved => RequestStatusFriendlyString.Approved,
            RequestStatus.Denied => RequestStatusFriendlyString.Denied,
            RequestStatus.Declined => RequestStatusFriendlyString.Declined,
            RequestStatus.Received => RequestStatusFriendlyString.Received,
            RequestStatus.WaitingToImport => RequestStatusFriendlyString.WaitingToImport,
            RequestStatus.Importing => RequestStatusFriendlyString.Importing,
            RequestStatus.Imported => RequestStatusFriendlyString.Imported,
            _ => throw new ArgumentOutOfRangeException(nameof(requestStatus))
        };
    }
}
