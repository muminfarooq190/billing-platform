using System.Text;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TravelService.Application.Queries.ReportBookings;
using TravelService.Application.Queries.SearchTravel;

namespace TravelService.Api.Controllers;

[ApiController]
[Route("travel")]
public sealed class ReportsController(IMediator mediator, ITenantContext tenantContext) : ControllerBase
{
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var results = await mediator.Send(new SearchTravelQuery(tenantContext.TenantId, q, page, pageSize), cancellationToken);
        return Ok(results);
    }

    [HttpGet("reports/bookings")]
    public async Task<IActionResult> ReportBookings([FromQuery] string? status = null, [FromQuery] string? destination = null, CancellationToken cancellationToken = default)
    {
        var rows = await mediator.Send(new ReportBookingsQuery(tenantContext.TenantId, status, destination), cancellationToken);
        return Ok(rows);
    }

    [HttpGet("export/bookings.csv")]
    public async Task<IActionResult> ExportBookings([FromQuery] string? status = null, [FromQuery] string? destination = null, CancellationToken cancellationToken = default)
    {
        var rows = await mediator.Send(new ReportBookingsQuery(tenantContext.TenantId, status, destination), cancellationToken);
        var sb = new StringBuilder();
        sb.AppendLine("bookingId,bookingNumber,title,destination,status,currency,totalSellAmount,travelDate,returnDate,travellers");
        foreach (var row in rows)
        {
            sb.AppendLine($"{row.BookingId},{Escape(row.BookingNumber)},{Escape(row.Title)},{Escape(row.Destination)},{row.Status},{row.Currency},{row.TotalSellAmount},{row.TravelDate:O},{row.ReturnDate:O},{row.Travellers}");
        }

        return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", "bookings-report.csv");
    }

    private static string Escape(string value)
    {
        if (value.Contains(',') || value.Contains('"'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }
}
