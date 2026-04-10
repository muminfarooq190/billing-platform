namespace IdentityService.Api.Contracts;

public sealed record CreateUserInvitationRequest(
    string Email,
    string Role,
    Guid? InvitedByUserId,
    int ExpiresInHours = 72);

public sealed record AcceptUserInvitationRequest(
    string Token,
    string Password);
