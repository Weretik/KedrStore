// System namespaces
global using System;
global using System.Collections.Generic;
global using System.Security.Claims;
global using System.Threading;
global using System.Threading.Tasks;
global using System.Diagnostics;
global using System.Globalization;

// Microsoft namespaces
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using Microsoft.EntityFrameworkCore;

// Application namespaces
global using Application.Common.Behaviors;
global using Application.Common.Logging;
global using Application.Common.Helpers;

// Domain namespaces
global using Domain.Common.Abstractions;

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
