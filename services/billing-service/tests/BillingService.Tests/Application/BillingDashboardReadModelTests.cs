using BillingService.Application.ReadModels;
using FluentAssertions;

namespace BillingService.Tests.Application;

public sealed class BillingDashboardReadModelTests
{
    [Fact]
    public void ReadModel_ShouldExposeFrontendDashboardFields()
    {
        var model = new BillingDashboardReadModel
        {
            TotalRevenue = 1000m,
            OutstandingAmount = 200m,
            OverdueAmount = 50m,
            PaidInvoicesCount = 4,
            UnpaidInvoicesCount = 2,
            Currency = "USD"
        };

        model.TotalRevenue.Should().Be(1000m);
        model.OutstandingAmount.Should().Be(200m);
        model.OverdueAmount.Should().Be(50m);
        model.PaidInvoicesCount.Should().Be(4);
        model.UnpaidInvoicesCount.Should().Be(2);
        model.Currency.Should().Be("USD");
    }
}
