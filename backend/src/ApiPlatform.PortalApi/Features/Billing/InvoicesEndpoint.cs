using System.Security.Claims;
using ApiPlatform.PortalApi.Domain.Entities;
using ApiPlatform.PortalApi.Features.Shared;
using ApiPlatform.PortalApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ApiPlatform.PortalApi.Features.Billing;

public static class InvoicesEndpoint
{
    public static void MapInvoiceEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/billing/invoices", List).RequireAuthorization();
        endpoints.MapGet("/billing/invoices/{invoiceId:guid}", Get).RequireAuthorization();
        endpoints.MapPost("/billing/invoices/{invoiceId:guid}/pay", Pay).RequireAuthorization();
    }

    private static async Task<IResult> List(ClaimsPrincipal principal, PortalDbContext db)
    {
        Guid organizationId = EndpointHelpers.OrganizationId(principal);
        List<InvoiceSummaryResponse> invoices = await db.Invoices
            .Where(invoice => invoice.OrganizationId == organizationId)
            .OrderByDescending(invoice => invoice.PeriodStart)
            .Select(invoice => new InvoiceSummaryResponse(
                invoice.Id,
                invoice.Number,
                invoice.PeriodStart,
                invoice.PeriodEnd,
                invoice.Status.ToString(),
                invoice.TotalAmount,
                invoice.DueAt,
                invoice.PaidAt))
            .ToListAsync();

        return Results.Ok(invoices);
    }

    private static async Task<IResult> Get(
        ClaimsPrincipal principal,
        Guid invoiceId,
        PortalDbContext db)
    {
        Guid organizationId = EndpointHelpers.OrganizationId(principal);
        Invoice? invoice = await db.Invoices
            .Include(item => item.Lines)
            .SingleOrDefaultAsync(item =>
                item.Id == invoiceId &&
                item.OrganizationId == organizationId);

        return invoice is null
            ? Results.NotFound()
            : Results.Ok(ToDetail(invoice));
    }

    private static async Task<IResult> Pay(
        ClaimsPrincipal principal,
        Guid invoiceId,
        PortalDbContext db)
    {
        Guid organizationId = EndpointHelpers.OrganizationId(principal);
        Invoice? invoice = await db.Invoices
            .Include(item => item.Lines)
            .SingleOrDefaultAsync(item =>
                item.Id == invoiceId &&
                item.OrganizationId == organizationId);
        if (invoice is null)
        {
            return Results.NotFound();
        }

        invoice.MarkPaid(DateTime.UtcNow);
        await db.SaveChangesAsync();
        return Results.Ok(ToDetail(invoice));
    }

    private static InvoiceDetailResponse ToDetail(Invoice invoice)
    {
        var lines = invoice.Lines
            .OrderByDescending(line => line.Amount)
            .Select(line => new InvoiceLineResponse(
                line.Id,
                line.ApiId,
                line.Api,
                line.Endpoint,
                line.Requests,
                line.Errors,
                line.BillableRequests,
                line.PricePerRequest,
                line.PriceEffectiveFrom,
                line.Amount))
            .ToList();

        return new InvoiceDetailResponse(
            invoice.Id,
            invoice.Number,
            invoice.PeriodStart,
            invoice.PeriodEnd,
            invoice.Status.ToString(),
            invoice.TotalAmount,
            invoice.IssuedAt,
            invoice.DueAt,
            invoice.PaidAt,
            lines);
    }
}
