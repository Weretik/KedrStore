// System namespaces
global using System.Xml;
global using System.Text;
global using System.Xml.Linq;
global using System.Globalization;
global using System.Security.Claims;
global using System.Text.RegularExpressions;

// Microsoft namespaces
global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using Microsoft.AspNetCore.Builder;
global using Microsoft.EntityFrameworkCore.Metadata.Builders;
global using Microsoft.AspNetCore.Identity;
global using Microsoft.Extensions.Options;
global using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
global using Microsoft.AspNetCore.Hosting;
global using Microsoft.Extensions.Hosting;
global using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

// Infrastructure namespaces
global using BuildingBlocks.Infrastructure.DomainEvents;
global using BuildingBlocks.Infrastructure.Extensions;
global using BuildingBlocks.Infrastructure.Migrations;
global using BuildingBlocks.Infrastructure.Seeding;
global using BuildingBlocks.Infrastructure.DependencyInjection;
global using BuildingBlocks.Infrastructure.Services;
global using BuildingBlocks.Infrastructure.Persistence.Configurations;

// Domain namespaces
global using BuildingBlocks.Domain.Abstractions;
global using Domain.Common.Entity;

// Application namespaces
global using Application.Common.Interfaces;
global using Application.Common.Notifications;

// Services
global using Mediator;
global using Ardalis.Specification;
global using Ardalis.Specification.EntityFrameworkCore;
global using SmartEnum.EFCore;
global using Telegram.Bot;
global using Telegram.Bot.Types.Enums;
