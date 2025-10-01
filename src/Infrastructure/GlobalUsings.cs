// System namespaces
global using System.Xml.Linq;
global using System.Globalization;
global using System.Security.Claims;

global using Mediator;

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
global using Microsoft.EntityFrameworkCore.Storage;
global using Microsoft.AspNetCore.Hosting;
global using Microsoft.Extensions.Hosting;

// Infrastructure namespaces
global using Infrastructure.Catalog;
global using Infrastructure.Catalog.Seeders;
global using Infrastructure.Catalog.Persistence;
global using Infrastructure.Catalog.Interfaces;
global using Infrastructure.Catalog.Migrations;
global using Infrastructure.Catalog.Notifications;

global using Infrastructure.Identity;
global using Infrastructure.Identity.Entities;
global using Infrastructure.Identity.Seeders;
global using Infrastructure.Identity.Configuration;
global using Infrastructure.Identity.Utils;
global using Infrastructure.Identity.Security;
global using Infrastructure.Identity.Interfaces;
global using Infrastructure.Identity.Migrations;
global using Infrastructure.Identity.Persistence;

global using Infrastructure.Common.Abstractions;
global using Infrastructure.Common.Events;
global using Infrastructure.Common.Services;

// Domain namespaces
global using Domain.Common.Abstractions;
global using Domain.Identity.Constants;
global using Domain.Catalog.Entities;
global using Domain.Catalog.ValueObjects;


// Application namespaces
global using Application.Catalog.Interfaces;
global using Application.Catalog.Notifications;
global using Application.Common.Interfaces;
global using Application.Common.Notifications;
global using Application.Identity.Interfaces;

// Ardalis
global using Ardalis.Specification;
global using Ardalis.Specification.EntityFrameworkCore;

// Telegram
global using Telegram.Bot;
global using Telegram.Bot.Types.Enums;
