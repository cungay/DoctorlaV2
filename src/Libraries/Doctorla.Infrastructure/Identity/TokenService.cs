using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Doctorla.Application.Common.Exceptions;
using Doctorla.Application.Identity.Tokens;
using Doctorla.Infrastructure.Auth;
using Doctorla.Infrastructure.Auth.Jwt;
using Doctorla.Infrastructure.Multitenancy;
using Doctorla.Shared.Authorization;
using Doctorla.Shared.Multitenancy;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Doctorla.Infrastructure.Identity;

internal class TokenService : ITokenService
{
    private readonly UserManager<ApplicationUser> userManager = null;
    private readonly IStringLocalizer localizer = null;
    private readonly SecuritySettings securitySettings = null;
    private readonly JwtSettings jwtSettings = null;
    private readonly DocTenantInfo? currentTenant = null;

    public TokenService(
        UserManager<ApplicationUser> userManager,
        IOptions<JwtSettings> jwtSettings,
        IStringLocalizer<TokenService> localizer,
        DocTenantInfo? currentTenant,
        IOptions<SecuritySettings> securitySettings)
    {
        this.userManager = userManager;
        this.localizer = localizer;
        this.jwtSettings = jwtSettings.Value;
        this.currentTenant = currentTenant;
        this.securitySettings = securitySettings.Value;
    }

    public async Task<TokenResponse> GetTokenAsync(TokenRequest request, string ipAddress, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(currentTenant?.Id)
            || await userManager.FindByEmailAsync(request.Email.Trim().Normalize()) is not { } user
            || !await userManager.CheckPasswordAsync(user, request.Password))
        {

            throw new UnauthorizedException(localizer["Authentication Failed."]);
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedException(localizer["User Not Active. Please contact the administrator."]);
        }

        if (securitySettings.RequireConfirmedAccount && !user.EmailConfirmed)
        {
            throw new UnauthorizedException(localizer["E-Mail not confirmed."]);
        }

        if (currentTenant.Id != MultitenancyConstants.Root.Id)
        {
            if (!currentTenant.IsActive)
            {
                throw new UnauthorizedException(localizer["Tenant is not Active. Please contact the Application Administrator."]);
            }

            if (DateTime.UtcNow > currentTenant.ValidUpto)
            {
                throw new UnauthorizedException(localizer["Tenant Validity Has Expired. Please contact the Application Administrator."]);
            }
        }

        return await GenerateTokensAndUpdateUser(user, ipAddress);
    }

    public async Task<TokenResponse> RefreshTokenAsync(RefreshTokenRequest request, string ipAddress)
    {
        var userPrincipal = GetPrincipalFromExpiredToken(request.Token);
        string? userEmail = userPrincipal.GetEmail();
        var user = await userManager.FindByEmailAsync(userEmail);
        if (user is null)
        {
            throw new UnauthorizedException(localizer["Authentication Failed."]);
        }

        if (user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            throw new UnauthorizedException(localizer["Invalid Refresh Token."]);
        }

        return await GenerateTokensAndUpdateUser(user, ipAddress);
    }

    private async Task<TokenResponse> GenerateTokensAndUpdateUser(ApplicationUser user, string ipAddress)
    {
        string token = GenerateJwt(user, ipAddress);
        user.RefreshToken = GenerateRefreshToken();
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(jwtSettings.RefreshTokenExpirationInDays);
        await userManager.UpdateAsync(user);
        return new TokenResponse(token, user.RefreshToken, user.RefreshTokenExpiryTime);
    }

    private string GenerateJwt(ApplicationUser user, string ipAddress) =>
        GenerateEncryptedToken(GetSigningCredentials(), GetClaims(user, ipAddress));

    private IEnumerable<Claim> GetClaims(ApplicationUser user, string ipAddress) =>
        new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email),
            new(DocClaims.Fullname, $"{user.FirstName} {user.LastName}"),
            new(ClaimTypes.Name, user.FirstName ?? string.Empty),
            new(ClaimTypes.Surname, user.LastName ?? string.Empty),
            new(DocClaims.IpAddress, ipAddress),
            new(DocClaims.Tenant, currentTenant!.Id),
            new(DocClaims.ImageUrl, user.ImageUrl ?? string.Empty),
            new(ClaimTypes.MobilePhone, user.PhoneNumber ?? string.Empty)
        };

    private string GenerateRefreshToken()
    {
        byte[] randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private string GenerateEncryptedToken(SigningCredentials signingCredentials, IEnumerable<Claim> claims)
    {
        var token = new JwtSecurityToken(
           claims: claims,
           expires: DateTime.UtcNow.AddMinutes(jwtSettings.TokenExpirationInMinutes),
           signingCredentials: signingCredentials);
        var tokenHandler = new JwtSecurityTokenHandler();
        return tokenHandler.WriteToken(token);
    }

    private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
            ValidateIssuer = false,
            ValidateAudience = false,
            RoleClaimType = ClaimTypes.Role,
            ClockSkew = TimeSpan.Zero,
            ValidateLifetime = false
        };
        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
        if (securityToken is not JwtSecurityToken jwtSecurityToken ||
            !jwtSecurityToken.Header.Alg.Equals(
                SecurityAlgorithms.HmacSha256,
                StringComparison.InvariantCultureIgnoreCase))
        {
            throw new UnauthorizedException(localizer["Invalid Token."]);
        }

        return principal;
    }

    private SigningCredentials GetSigningCredentials()
    {
        byte[] secret = Encoding.UTF8.GetBytes(jwtSettings.Key);
        return new SigningCredentials(new SymmetricSecurityKey(secret), SecurityAlgorithms.HmacSha256);
    }
}