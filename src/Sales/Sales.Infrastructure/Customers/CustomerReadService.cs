using Sales.Application.Contracts.Customers;
using Sales.Application.Features.Customers.GetById.DTOs;
using Sales.Application.Features.Customers.GetList.DTOs;

namespace Sales.Infrastructure.Customers;

internal sealed class CustomerReadService(SalesDbContext dbContext) : ICustomerReadService
{
    public async Task<Result<PagedResult<List<CustomerListItemDto>>>> GetListAsync(
        CustomerListRequest request,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Counterparties
            .AsNoTracking()
            .OrderBy(counterparty => counterparty.Name)
            .ThenBy(counterparty => counterparty.Id)
            .Select(counterparty => new CustomerListItemDto(
                counterparty.Id,
                counterparty.Name,
                counterparty.Phone));

        var totalRecords = await query.LongCountAsync(cancellationToken);
        var rows = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);
        var totalPages = totalRecords == 0
            ? 0
            : (long)Math.Ceiling(totalRecords / (double)request.PageSize);

        return Result.Success(new PagedResult<List<CustomerListItemDto>>(
            new PagedInfo(request.Page, request.PageSize, totalPages, totalRecords),
            rows));
    }

    public async Task<Result<CustomerDetailDto>> GetByIdAsync(
        string counterpartyId,
        CancellationToken cancellationToken)
    {
        var detail = await dbContext.Counterparties
            .AsNoTracking()
            .Where(counterparty => counterparty.Id == counterpartyId)
            .Select(counterparty => new CustomerDetailDto(
                counterparty.Id,
                counterparty.Name,
                counterparty.Phone,
                counterparty.Email,
                counterparty.DefaultPriceTypeId,
                dbContext.CounterpartyCategoryPriceTypes
                    .AsNoTracking()
                    .Where(rule => rule.CounterpartyId == counterparty.Id)
                    .OrderBy(rule => rule.CategoryId)
                    .Select(rule => new CustomerCategoryPriceTypeDto(rule.CategoryId, rule.PriceTypeId))
                    .ToList()))
            .SingleOrDefaultAsync(cancellationToken);

        return detail is null ? Result.NotFound() : Result.Success(detail);
    }
}
