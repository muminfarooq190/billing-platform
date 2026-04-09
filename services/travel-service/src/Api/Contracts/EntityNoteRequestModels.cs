namespace TravelService.Api.Contracts;

public sealed record CreateEntityNoteRequest(string Visibility, string Content);
public sealed record UpdateEntityNoteRequest(string Visibility, string Content);
