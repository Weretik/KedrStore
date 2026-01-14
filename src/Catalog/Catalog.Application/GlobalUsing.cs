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
