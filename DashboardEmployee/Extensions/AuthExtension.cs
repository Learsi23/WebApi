using DashboardEmployee.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Identity.Client.NativeInterop;
using System.Net;
using System.Security.Principal;
using System.Threading.RateLimiting;
using System.Timers;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DashboardEmployee.Extensions;

public static class AuthExtension
{

    public const string LoginRateLimitPolicy = "login";

    public static IServiceCollection AddAuth(this IServiceCollection services)
    {
        // Identity: users, password hashing, roles and lockout, stored with EF Core.
        services.AddIdentityCore<IdentityUser>(options =>
        {
            options.User.RequireUniqueEmail = true;
            options.Password.RequiredLength = 10;
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        })
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<AppDbContext>()
        .AddSignInManager();
        // The login is remembered in an HttpOnly cookie that JavaScript cannot read.
        services.AddAuthentication(IdentityConstants.ApplicationScheme)
        .AddIdentityCookies();


        services.ConfigureApplicationCookie(opt =>
        {
            opt.Cookie.Name = "dashboard.auth";
            opt.Cookie.HttpOnly = true;
            opt.Cookie.SameSite = SameSiteMode.Strict;
            opt.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // HTTPS-only cookie in production
            opt.ExpireTimeSpan = TimeSpan.FromHours(8);
            opt.SlidingExpiration = true;

            // An API has no login page: answer 401/403 instead of redirecting to /Account/Login.

            opt.Events.OnRedirectToLogin = context =>
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            };
            opt.Events.OnRedirectToAccessDenied = context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            };
        });

        services.AddAuthorization(opt =>
        {
        // Secure by default: every endpoint needs a logged-in user unless it says [AllowAnonymous].


            opt.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        });
        // Max 5 login attempts per minute per IP address: slows down password guessing.

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy(LoginRateLimitPolicy, httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1)
            }));
        });



        return services;
    }
//        *********************************
//**************Varför, del för del:********************
//        *********************************
//AddIdentityCore(inte AddIdentity) : bara det ett API behöver, utan Razor-sidor för inloggning.

//Lösenord minst 10 tecken.Identity kräver dessutom versal, gemen, siffra och specialtecken.Längd är
//det som skyddar mest.
//Kontolåsning: efter 5 fel lösenord är kontot låst i 5 minuter.

//Cookien:
//HttpOnly : JavaScript kan inte läsa den.
//SameSite= Strict : skydd mot CSRF.
//SecurePolicy= SameAsRequest : på HTTPS i produktion skickas cookien bara krypterat. Lokalt fungerar
//vanlig http.
//Giltig 8 timmar.
//SlidingExpiration förlänger tiden medan du använder appen.

//OnRedirectToLogin → 401: Identity är byggt för webbsidor och vill skicka dig till /Account/Login.Ett
//API ska i stället svara 401 (inte inloggad) eller 403 (inloggad men saknar behörighet).

//FallbackPolicy : säkert som standard.Varje endpoint kräver inloggning, även de du skriver i
//framtiden.Du måste aktivt skriva //[AllowAnonymous] för att öppna något.Glömmer du det är
//endpointen stängd, inte öppen.

//Rate limiting: högst 5 inloggningsförsök per minut och IP-adress, sedan 429. Det bromsar den som
//försöker gissa lösenord.


}
