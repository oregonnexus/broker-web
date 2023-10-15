// Copyright: 2023 Education Nexus Oregon
// Author: Makoa Jacobsen, makoa@makoajacobsen.com

using Microsoft.AspNetCore.Mvc;
using OregonNexus.Broker.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using OregonNexus.Broker.Domain;
using OregonNexus.Broker.SharedKernel;
using static OregonNexus.Broker.Web.Constants.Sessions.SessionKey;

namespace OregonNexus.Broker.Web.Controllers;

[AllowAnonymous]
public class LoginController : AuthenticatedController
{
    private readonly ILogger<LoginController> _logger;
    //private readonly OregonNexus.Broker.Connector.Edupoint.Synergy.Authentication.ThirdPartyApplication _auth;
    //private readonly AuthenticationProvidersLocator? _authProvidersLocator;
    public readonly BrokerDbContext _db;
    private readonly UserManager<IdentityUser<Guid>> _userManager;
    private readonly SignInManager<IdentityUser<Guid>> _signInManager;
    private readonly IRepository<User> _userRepo;

    public LoginController(
        IHttpContextAccessor httpContextAccessor,
        ILogger<LoginController> logger,
        BrokerDbContext db,
        UserManager<IdentityUser<Guid>> userManager,
        SignInManager<IdentityUser<Guid>> signInManager,
        IRepository<User> userRepo) : base(httpContextAccessor)
    {
        _logger = logger;
        _db = db;
        _userManager = userManager;
        _signInManager = signInManager;
        _userRepo = userRepo;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var externalLogins = await _signInManager.GetExternalAuthenticationSchemesAsync();
        return View(externalLogins);
    }

    [HttpGet]
    public async Task<IActionResult> CreateFirstUser()
    {
        var identityUser = new IdentityUser<Guid> { UserName = "luis.ruiz@developers.net", Email = "luis.ruiz@developers.net" }; 
        var result = await _userManager.CreateAsync(identityUser);

        var user = new User()
        {
            Id = identityUser.Id,
            FirstName = "Luis",
            LastName = "Ruiz",
            IsSuperAdmin = true,
            CreatedAt = DateTime.UtcNow,
            AllEducationOrganizations = PermissionType.Write
        };
        _db.Add(user);
        await _db.SaveChangesAsync();
        
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Route("login/externallogin")]
    public IActionResult ExternalLogin(string provider, string? returnUrl)
    {
        // Request a redirect to the external login provider.
        var redirectUrl = Url.Action("ExternalLoginCallback", values: new { returnUrl });
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return new ChallengeResult(provider, properties);
    }

    [HttpGet]
    [Route("login/externallogin")]
    public async Task<IActionResult> ExternalLoginCallback(string? returnUrl, string? remoteError)
    {
        returnUrl = returnUrl ?? Url.Content("~/");
        if (remoteError != null)
        {
            //ErrorMessage = $"Error from external provider: {remoteError}";
            return RedirectToAction("Login", new { ReturnUrl = returnUrl });
        }
        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            //ErrorMessage = "Error loading external login information.";
            return RedirectToAction("Login", new { ReturnUrl = returnUrl });
        }

        // Sign in the user with this external login provider if the user already has a login.
        var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
        if (result.Succeeded)
        {
            var email = info.Principal.Claims.Where(x => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress").FirstOrDefault()!.Value!;
            var user = await _userManager.FindByEmailAsync(email);

            var currentUser = await _userRepo.GetByIdAsync(user?.Id);
            _httpContextAccessor.HttpContext?.Session?.SetObjectAsJson(UserCurrent, currentUser);
            
            _logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info!.Principal.Identity?.Name, info.LoginProvider);

            _httpContextAccessor.HttpContext?.Session?.SetString(LastAccessedKey, $"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}");
            return LocalRedirect(returnUrl);
        }
        if (result.IsLockedOut)
        {
            return RedirectToAction("Lockout");
        }
        else
        {
            // Get user
            var email = info.Principal.Claims.Where(x => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress").FirstOrDefault()!.Value!;
            var user = await _userManager.FindByEmailAsync(email);

            var currentUser = await _userRepo.GetByIdAsync(user?.Id);
            _httpContextAccessor.HttpContext?.Session?.SetObjectAsJson("User.Current", currentUser);

            if (user is null)
            {
                _logger.LogInformation("{Email} not found in database.", email);
                return RedirectToAction("Login");
            }
            
            var loginResult = await _userManager.AddLoginAsync(user, info);
            if (loginResult.Succeeded)
            {
                _logger.LogInformation("Added {Name} logged in with {LoginProvider} provider.", info!.Principal.Identity?.Name, info.LoginProvider);
            }
            result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (result.Succeeded)
            {
                _logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info!.Principal.Identity?.Name, info.LoginProvider);

                _httpContextAccessor.HttpContext?.Session?.SetString(LastAccessedKey, $"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}");
                return LocalRedirect(returnUrl);
            }
            return RedirectToAction("Login");
        }
    }

    [AllowAnonymous]
    [Route("login/logout")]
    public async Task<IActionResult> Logout()
    {
         await _signInManager.SignOutAsync();
        _httpContextAccessor.HttpContext?.Session.Clear();
        _logger.LogInformation("User logged out.");
        return RedirectToAction("Index", "Home");
    }

}

public class LogInViewModel
    {
        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
        public string? ReturnUrl { get; set; }

        public IList<AuthenticationScheme>? ExternalLogins { get; set; }
    }
