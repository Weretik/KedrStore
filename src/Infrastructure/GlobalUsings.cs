// System namespaces
global using System.Xml.Linq;
global using System.Globalization;
global using System.Security.Claims;

// Microsoft namespaces
global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using Microsoft.AspNetCore.Builder;
global using Microsoft.EntityFrameworkCore.Metadata.Builders;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.AspNetCore.Identity;
global using Microsoft.Extensions.Options;
global using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.Storage;
global using Microsoft.AspNetCore.Hosting;
global using Microsoft.Extensions.Hosting;

// Infrastructure namespaces
global using Infrastructure.Catalog;
global using Infrastructure.Catalog.Repositories;
global using Infrastructure.Catalog.Seeders;
global using Infrastructure.Identity;
global using Infrastructure.Identity.Entities;
global using Infrastructure.Identity.Seeders;
global using Infrastructure.Shared.Services;
global using Infrastructure.Identity.Configuration;
global using Infrastructure.Identity.Utils;
global using Infrastructure.Identity.Security;
global using Infrastructure.Identity.Interfaces;
global using Infrastructure.Catalog.Persistence;
global using Infrastructure.Catalog.Interfaces;
global using Infrastructure.Shared.Abstractions;

// Domain namespaces
global using Domain.Catalog.Entities;
global using Domain.Catalog.ValueObjects;
global using Domain.Identity.Constants;
global using Domain.Catalog.Repositories;

// Application namespaces
global using Application.Common.Abstractions.Events;
global using Application.Common.Abstractions.Security;
global using Application.Interfaces;
global using Application.Catalog.Interfaces;
global using Application.Common.Errors;
