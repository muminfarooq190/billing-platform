using Dapper;
using MediatR;
using TravelService.Application.Abstractions;
using TravelService.Domain.Exceptions;
using TravelService.Domain.Repositories;

namespace TravelService.Application.Queries.ListBookingDocuments;

public sealed class ListBookingDocumentsQueryHandler(
    IReadDbConnectionFactory connectionFactory,
    IBookingRepository bookingRepository,
    IFileStorage fileStorage) : IRequestHandler<ListBookingDocumentsQuery, IReadOnlyList<BookingDocumentReadModel>>
{
    public async Task<IReadOnlyList<BookingDocumentReadModel>> Handle(ListBookingDocumentsQuery request, CancellationToken cancellationToken)
    {
        var booking = await bookingRepository.GetByIdAsync(request.BookingId, cancellationToken)
            ?? throw new DomainException($"Booking {request.BookingId} not found.");

        if (booking.TenantId != request.TenantId)
            throw new DomainException("Booking does not belong to the active tenant.");

        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken) as IAsyncDisposable;
        var dbConnection = (System.Data.IDbConnection)connection!;
        var rows = await dbConnection.QueryAsync<FlatBookingDocumentRow>(
            @"SELECT id,
                      booking_id AS BookingId,
                      traveler_id AS TravelerId,
                      tenant_id AS TenantId,
                      original_file_name AS OriginalFileName,
                      content_type AS ContentType,
                      size_bytes AS SizeBytes,
                      document_type AS DocumentType,
                      is_customer_visible AS IsCustomerVisible,
                      description AS Description,
                      storage_key AS StorageKey,
                      created_at AS CreatedAt
               FROM booking_documents
               WHERE booking_id = @BookingId AND tenant_id = @TenantId AND deleted_at IS NULL
               ORDER BY created_at DESC",
            new { request.BookingId, request.TenantId });

        var results = new List<BookingDocumentReadModel>();
        foreach (var row in rows)
        {
            var readUrl = await fileStorage.GetSignedReadUrlAsync(row.StorageKey, TimeSpan.FromHours(1), cancellationToken);
            results.Add(new BookingDocumentReadModel(row.Id, row.BookingId, row.TravelerId, row.TenantId, row.OriginalFileName, row.ContentType, row.SizeBytes, row.DocumentType, row.IsCustomerVisible, row.Description, readUrl, row.CreatedAt));
        }

        return results;
    }

    private sealed record FlatBookingDocumentRow(
        Guid Id,
        Guid BookingId,
        Guid? TravelerId,
        Guid TenantId,
        string OriginalFileName,
        string ContentType,
        long SizeBytes,
        string DocumentType,
        bool IsCustomerVisible,
        string? Description,
        string StorageKey,
        DateTimeOffset CreatedAt);
}
