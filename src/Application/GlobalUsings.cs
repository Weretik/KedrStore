// System namespaces
global using System;
global using System.Collections.Generic;
global using System.Reflection;
global using System.Security.Claims;
global using System.Threading;
global using System.Threading.Tasks;
global using System.Diagnostics;
global using System.Linq.Expressions;
global using System.Globalization;

// Microsoft namespaces
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.ChangeTracking;
global using Microsoft.EntityFrameworkCore.Infrastructure;

// Application namespaces
global using Application.Extensions;
global using Application.Common.Behaviors;
global using Application.Common.Sorting;
global using Application.Common.Paging;
global using Application.Common.Logging;
global using Application.Catalog.DTOs;
global using Application.Catalog.Sorting;

// Domain namespaces
global using Domain.Common.Abstractions;
global using Domain.Catalog.Entities;
global using Domain.Catalog.ValueObjects;
global using Domain.Catalog.Repositories;

// External libraries
global using Mediator;
global using FluentValidation;
global using Ardalis.Specification;
global using Ardalis.Specification.EntityFrameworkCore;
global using Ardalis.Result;
global using Ardalis.Result.FluentValidation;
