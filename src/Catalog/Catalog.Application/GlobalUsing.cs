// System namespaces
global using System;
global using System.Collections.Generic;
global using System.Security.Claims;
global using System.Threading;
global using System.Threading.Tasks;
global using System.Diagnostics;
global using System.Globalization;
global using Microsoft.Extensions.Options;

// Microsoft namespaces
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.Configuration;

//  Catalog.Domain namespaces
global using Catalog.Domain.Categories;
global using Catalog.Domain.Categories.Content;
global using Catalog.Domain.PriceTypes;
global using Catalog.Domain.Products;
global using Catalog.Domain.Products.Media;
global using Catalog.Domain.Products.Prices;
global using Catalog.Domain.Slugs;

// BuildingBlocks namespaces
global using BuildingBlocks.Application.Behaviors;
global using BuildingBlocks.Application.Logging;
global using BuildingBlocks.Application.Helpers;
global using BuildingBlocks.Domain.Abstractions;

// External libraries
global using Mediator;
global using FluentValidation;
global using FluentValidation.Results;
global using Ardalis.Specification;
global using Ardalis.Result;
global using Ardalis.Result.FluentValidation;
global using PhoneNumbers;
global using NodaMoney;
global using Newtonsoft.Json;
global using Slugify;
global using Hangfire;
