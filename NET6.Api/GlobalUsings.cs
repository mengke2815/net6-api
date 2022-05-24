﻿global using Autofac;
global using Autofac.Extensions.DependencyInjection;
global using Jaina.EventBus;
global using Microsoft.AspNetCore.Authentication.JwtBearer;
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.Mvc.Controllers;
global using Microsoft.AspNetCore.Mvc.Filters;
global using Microsoft.AspNetCore.Server.Kestrel.Core;
global using Microsoft.AspNetCore.StaticFiles;
global using Microsoft.Extensions.FileProviders;
global using Microsoft.IdentityModel.Tokens;
global using Microsoft.OpenApi.Models;
global using NET6.Api.Attributes;
global using NET6.Api.Filters;
global using NET6.Api.Services;
global using NET6.Domain.Dtos;
global using NET6.Domain.Entities;
global using NET6.Domain.Enums;
global using NET6.Domain.ViewModels;
global using NET6.Infrastructure.Fleck;
global using NET6.Infrastructure.Repositories;
global using NET6.Infrastructure.Tools;
global using Serilog;
global using SqlSugar;
global using Swashbuckle.AspNetCore.SwaggerUI;
global using System.Diagnostics;
global using System.IdentityModel.Tokens.Jwt;
global using System.Net;
global using System.Reflection;
global using System.Security.Claims;
global using System.Text;
global using System.Text.Json.Serialization;
