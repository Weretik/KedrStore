global using BuildingBlocks.Infrastructure.Migrations;
global using BuildingBlocks.Infrastructure.Seeding;
global using BuildingBlocks.Domain.Abstractions;

global using System.Data;

global using Ardalis.Specification.EntityFrameworkCore;

global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.Metadata.Builders;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;

global using Ardalis.Result;
global using Mediator;

global using Catalog.Contracts.Pricing;
global using Catalog.Contracts.Products.GetList;

global using Sales.Application.Abstractions.Catalog;
global using Sales.Application.Abstractions.Pricing;
global using Sales.Application.Abstractions.Persistence;
global using Sales.Application.Features.Catalog.Queries.GetCatalogList;
global using Sales.Application.Abstractions.DeliveryFailure;
global using Sales.Domain.Customers;
global using Sales.Domain.Orders;
global using Sales.Infrastructure.Persistence.Customers;
global using Sales.Infrastructure.Persistence.OrderDrafts;
global using Sales.Infrastructure.Persistence.Orders;
global using Sales.Infrastructure.Adapters.Catalog;
global using Sales.Infrastructure.Persistence;
global using Sales.Infrastructure.Pricing;
global using BuildingBlocks.Domain.Results;
