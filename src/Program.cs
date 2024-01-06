// Copyright: 2023 Education Nexus Oregon
// Author: Makoa Jacobsen, makoa@makoajacobsen.com

using Microsoft.EntityFrameworkCore;
using OregonNexus.Broker.Data;
using MediatR;
using Autofac;
using OregonNexus.Broker.SharedKernel;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using InertiaAdapter.Extensions;
using OregonNexus.Broker.Web;
using OregonNexus.Broker.Web.Services;
using System.Reflection;
using OregonNexus.Broker.Domain;
using OregonNexus.Broker.Service;
using Microsoft.AspNetCore.Authentication;
using OregonNexus.Broker.Web.Extensions.Routes;
using OregonNexus.Broker.Web.Services.PayloadContents;
using static OregonNexus.Broker.Web.Constants.Claims.CustomClaimType;
using src.Services.Tokens;
//using src.Services.Students;
using src.Services.Shared;
using Microsoft.Extensions.Caching.Memory;

var builder = WebApplication.CreateBuilder(args);

// Add Autofac
//builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

// Add services to the container.
builder.Services.AddHttpContextAccessor();
//builder.Services.AddScoped<ScopedHttpContext>();
builder.Services.AddMediatR(typeof(Program).Assembly);

switch (builder.Configuration["DatabaseProvider"])
{
    case DbProviderType.MsSql:
        builder.Services.AddDbContext<BrokerDbContext, MsSqlDbContext>();
        break;

    case DbProviderType.PostgreSql:
        builder.Services.AddDbContext<BrokerDbContext, PostgresDbContext>();
        break;
}

builder.Services.AddScoped(typeof(EfRepository<>));
builder.Services.AddScoped(typeof(CachedRepository<>));

builder.Services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
builder.Services.AddScoped(typeof(IReadRepository<>), typeof(CachedRepository<>));

builder.Services.AddScoped(typeof(IMemoryCache), typeof(MemoryCache));
builder.Services.AddScoped(typeof(IMediator), typeof(Mediator));

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
foreach (var assembly in Assembly.GetExecutingAssembly().GetExportedTypes().Where(t => String.Equals(t.Namespace, "OregonNexus.Broker.Web.Helpers", StringComparison.Ordinal)).ToArray())
{
    builder.Services.AddScoped(assembly, assembly);
}

builder.Services.AddIdentity<IdentityUser<Guid>, IdentityRole<Guid>>(options =>
{
    options.User.RequireUniqueEmail = false;
})
.AddEntityFrameworkStores<BrokerDbContext>()
.AddTokenProvider<DataProtectorTokenProvider<IdentityUser<Guid>>>(TokenOptions.DefaultProvider);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IPayloadContentService, PayloadContentService>();
builder.Services.AddScoped<ITokenService, TokenService>();
//builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IClientService, ClientService>();

builder.Services.ConfigureApplicationCookie(options => 
{
    options.AccessDeniedPath = "/AccessDenied";
    options.Cookie.Name = "OregonNexus.Broker.Identity";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.ExpireTimeSpan = TimeSpan.FromHours(4);
    options.LoginPath = "/Login";
    // ReturnUrlParameter requires 
    //using Microsoft.AspNetCore.Authentication.Cookies;
    options.ReturnUrlParameter = CookieAuthenticationDefaults.ReturnUrlParameter;
    options.SlidingExpiration = true;
});


builder.Services.AddSession(options =>
{
    options.Cookie.Name = "OregonNexus.Broker.Session";
    options.IdleTimeout = TimeSpan.FromHours(4);
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.IsEssential = true;
});

builder.Services.AddAuthentication()
    .AddGoogle(googleOptions =>
    {
        googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
        googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
    });
//     .AddMicrosoftAccount(microsoftOptions =>
//     {
//         microsoftOptions.ClientId = builder.Configuration["Authentication:Microsoft:ClientId"]!;
//         microsoftOptions.ClientSecret = builder.Configuration["Authentication:Microsoft:ClientSecret"]!;
//     }
// );

builder.Services.AddAuthorization(options => {
    options.AddPolicy("SuperAdmin",
      policy => policy.RequireClaim("SuperAdmin", "true")
    );

    options.AddPolicy("AllEducationOrganizations",
      policy => policy.RequireClaim("AllEducationOrganizations", PermissionType.Read.ToString(), PermissionType.Write.ToString())
    );

    options.AddPolicy("TransferRecords",
      policy => policy.RequireClaim("TransferRecords", "true")
    );

    options.AddPolicy(TransferIncomingRecords,
      policy => policy.RequireClaim(TransferIncomingRecords, "true")
    );
        options.AddPolicy(TransferOutgoingRecords,
      policy => policy.RequireClaim(TransferOutgoingRecords, "true")
    );
});

builder.Services.AddTransient<IClaimsTransformation, BrokerClaimsTransformation>();

builder.Services.AddControllersWithViews();
builder.Services.AddInertia();
builder.Services.AddHttpClient();

builder.Services.AddScoped<ICurrentUser, CurrentUserService>();

builder.Services.AddConnectorLoader();
builder.Services.AddConnectorDependencies();

builder.Services.AddBrokerServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseInertia();

app.UseHttpMethodOverride(new HttpMethodOverrideOptions()
{
    FormFieldName = "_METHOD"
});

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

//app.UseMiddleware<ScopedHttpContextMiddleware>();

app.MapControllerRoutes("organizations", "EducationOrganizations");
app.MapControllerRoutes("incoming-requests", "Incoming");
app.MapControllerRoutes("outgoing-requests", "Outgoing");
app.MapControllerRoutes("users", "Users");
app.MapControllerRoutes("roles", "UserRoles");
app.MapControllerRoutes("settings", "Settings");
app.MapControllerRoutes("login", "Login");
app.MapControllerRoutes("focus", "Focus");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");



app.Run();
