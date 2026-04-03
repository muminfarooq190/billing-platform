using BillingService.Domain.Aggregates;
using BillingService.Domain.Enums;
using BillingService.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Infrastructure.Persistence.Repositories;

public sealed class InvoiceRepository(BillingDbContext dbContext) : IInvoiceRepository
{
    public Task AddAsync(Invoice invoice, CancellationToken cancellationToken) => dbContext.Invoices.AddAsync(invoice, cancellationToken).AsTask();

    public Task<Invoice?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => dbContext.Invoices.SingleOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, cancellationToken);

    public Task UpdateAsync(Invoice invoice, CancellationToken cancellationToken)
    {
        dbContext.Invoices.Update(invoice);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<Invoice>> ListOverdueCandidatesAsync(DateTimeOffset utcNow, CancellationToken cancellationToken)
    {
        return await dbContext.Invoices.Where(x => x.Status == InvoiceStatus.Issued && x.DueDate < utcNow && x.DeletedAt == null).ToListAsync(cancellationToken);
    }
}
