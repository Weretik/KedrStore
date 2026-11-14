// System namespaces
global using System.Text;
global using System.Xml.Linq;
global using System.Globalization;

// Microsoft namespaces
global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using Microsoft.EntityFrameworkCore.Metadata.Builders;
global using Microsoft.Extensions.Options;
global using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

// Infrastructure namespaces
global using BuildingBlocks.Infrastructure.Migrations;
global using BuildingBlocks.Infrastructure.Notifications;

// Domain namespaces
global using BuildingBlocks.Domain.Abstractions;
global using Catalog.Contracts.Products.GetList;

// Application namespaces
global using Catalog.Application.Features.Products.Queries.GetProductList;

// Services
global using Ardalis.Result;
global using Ardalis.Specification;
global using Ardalis.Specification.EntityFrameworkCore;
global using Mediator;
global using BuildingBlocks.Domain.Results;
