using DashboardEmployee.Dtos;
using DashboardEmployee.Extensions;
using DashboardEmployee.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace DashboardEmployee.Controllers
{
  
    [ApiController]
    [Route("api/Auth")]
    [Produces("application/json")]
    public sealed class AuthController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager, IValidator<LoginRequest> validator) : ApiControllerBase
    {

        [HttpPost("login")]
        [AllowAnonymous]
        [EnableRateLimiting(AuthExtension.LoginRateLimitPolicy)]
        [ProducesResponseType<CurrentUserResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<ActionResult<CurrentUserResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default)
        {
            var validation = await validator.ValidateAsync(request, ct);
            if (!validation.IsValid)
                return ValidationProblem(validation);
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user is null)
                return InvalidCredentials();

            // Sets the auth cookie on success. lockoutOnFailure: 5 wrong passwords lock the account for 5 minutes.

            var result = await signInManager.PasswordSignInAsync(user, request.Password, isPersistent:false, lockoutOnFailure: true);
            
            if(!result.Succeeded)
                return InvalidCredentials();

            return Ok(await ToResponseAsync(user));
        }

        [HttpPost("logout")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return NoContent();
        }

        [HttpGet("me")]
        [ProducesResponseType<CurrentUserResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<CurrentUserResponse>> Me()
        {
            var user = await userManager.GetUserAsync(User);
            if (user is null)
                return Unauthorized();
            return Ok(await ToResponseAsync(user));
        }


        // Same answer for "unknown email", "wrong password" and "locked out":
        // an attacker cannot use the login form to find out which emails exist.

        private ObjectResult InvalidCredentials() => Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Login failed", detail: "Invalid email or password.");
        private async Task<CurrentUserResponse> ToResponseAsync(IdentityUser user) => new(user.Email!, [.. await userManager.GetRolesAsync(user)]);

    }
}
