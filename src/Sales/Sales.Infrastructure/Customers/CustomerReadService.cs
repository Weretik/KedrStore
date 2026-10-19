using Catalog.Application.Contracts.Persistence;
using Sales.Application.Contracts.Customers;
using Sales.Application.Features.Customers.GetById.DTOs;
using Sales.Application.Features.Customers.GetList.DTOs;

namespace Sales.Infrastructure.Customers;

internal sealed class CustomerReadService(
    SalesDbContext dbContext,
    ICatalogReferenceDataReader catalogReferenceDataReader) : ICustomerReadService
{
    public async Task<Result<PagedResult<List<CustomerListItemDto>>>> GetListAsync(
        CustomerListRequest request,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Counterparties.AsNoTracking();

        var searchTerm = request.SearchTerm?.Trim().ToLower();
        if (!string.IsNullOrEmpty(searchTerm))
            query = query.Where(counterparty =>
                counterparty.Id.ToLower().Contains(searchTerm) ||
                counterparty.Name.ToLower().Contains(searchTerm) ||
                (counterparty.Phone != null && counterparty.Phone.ToLower().Contains(searchTerm)));

        var counterpartyId = request.CounterpartyId?.Trim().ToLower();
        if (!string.IsNullOrEmpty(counterpartyId))
            query = query.Where(counterparty => counterparty.Id.ToLower().Contains(counterpartyId));

        var name = request.Name?.Trim().ToLower();
        if (!string.IsNullOrEmpty(name))
            query = query.Where(counterparty => counterparty.Name.ToLower().Contains(name));

        var phone = request.Phone?.Trim().ToLower();
        if (!string.IsNullOrEmpty(phone))
            query = query.Where(counterparty => counterparty.Phone != null && counterparty.Phone.ToLower().Contains(phone));

        var totalRecords = await query.LongCountAsync(cancellationToken);
        var ordered = request.Sort switch
        {
            "-name" => query.OrderByDescending(counterparty => counterparty.Name).ThenBy(counterparty => counterparty.Id),
            "counterpartyId" => query.OrderBy(counterparty => counterparty.Id),
            "-counterpartyId" => query.OrderByDescending(counterparty => counterparty.Id),
            "phone" => query.OrderBy(counterparty => counterparty.Phone).ThenBy(counterparty => counterparty.Id),
            "-phone" => query.OrderByDescending(counterparty => counterparty.Phone).ThenBy(counterparty => counterparty.Id),
            _ => query.OrderBy(counterparty => counterparty.Name).ThenBy(counterparty => counterparty.Id)
        };
        var rows = await ordered
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(counterparty => new CustomerListItemDto(
                counterparty.Id,
                counterparty.Name,
                counterparty.Phone))
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

        if (detail is null)
            return Result.NotFound();

        var categoryIds = detail.CategoryPriceTypes.Select(rule => rule.CategoryId).Distinct().ToArray();
        var priceTypeIds = detail.CategoryPriceTypes
            .Select(rule => rule.PriceTypeId)
            .Append(detail.DefaultPriceTypeId)
            .Distinct()
            .ToArray();
        var categoryNames = await catalogReferenceDataReader.GetCategoryNamesAsync(categoryIds, cancellationToken);
        var priceTypeNames = await catalogReferenceDataReader.GetPriceTypeNamesAsync(priceTypeIds, cancellationToken);

        return Result.Success(detail with
        {
            DefaultPriceTypeName = priceTypeNames.GetValueOrDefault(detail.DefaultPriceTypeId),
            CategoryPriceTypes = detail.CategoryPriceTypes
                .Select(rule => rule with
                {
                    CategoryName = categoryNames.GetValueOrDefault(rule.CategoryId),
                    PriceTypeName = priceTypeNames.GetValueOrDefault(rule.PriceTypeId)
                })
                .ToList()
        });
    }
}
