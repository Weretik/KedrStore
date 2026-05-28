global using BuildingBlocks.Infrastructure.Migrations;
global using BuildingBlocks.Domain.Abstractions;

global using Ardalis.Specification.EntityFrameworkCore;

global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.Metadata.Builders;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Options;

global using Ardalis.Result;
global using Mediator;

global using Catalog.Contracts.Pricing;
global using Catalog.Contracts.Products.GetList;

global using Sales.Application.Contracts.Catalog;
global using Sales.Application.Contracts.Pricing;
global using Sales.Application.Contracts.Persistence;
global using Sales.Application.Features.Catalog.GetList.DTOs;
global using Sales.Domain.Customers.Entities;
global using Sales.Infrastructure.Catalog;
global using Sales.Infrastructure.DataBase;
global using Sales.Infrastructure.Pricing;
global using Sales.Infrastructure.Repositories;
