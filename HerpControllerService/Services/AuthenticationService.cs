using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using HerpControllerService.Database;
using HerpControllerService.Entities;
using HerpControllerService.Enums;
using HerpControllerService.Exceptions;
using HerpControllerService.Models;
using HerpControllerService.Models.API;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace HerpControllerService.Services;

public class AuthenticationService(IConfiguration configuration, HerpControllerDbContext db, ILogger<AuthenticationService> logger)
{
    private const string REFRESH = "REFRESH";

    public async Task<TokenModel> Login(string? encodedAuth)
    {
        if (string.IsNullOrWhiteSpace(encodedAuth))
        {
            throw new HerpControllerException(HttpStatusCode.Unauthorized);
        }
        
        var authParts = DecodeAuth(encodedAuth);

        var username = authParts[0];
        var password = authParts[1];

        var userEntity = await db.Users.FirstOrDefaultAsync(entity => entity.Username == username && entity.Status == UserStatus.ACTIVE);
        if (userEntity == null)
        {
            throw new HerpControllerException(HttpStatusCode.Unauthorized);
        }

        if (HashPassword(password, userEntity.Salt) != userEntity.Password)
        {
            throw new HerpControllerException(HttpStatusCode.Unauthorized);
        }

        var token = GenerateToken(userEntity, false);
        var refreshToken = GenerateToken(userEntity, true);
        
        userEntity.RefreshToken = refreshToken;
        userEntity.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        userEntity.ModifiedAt = DateTime.UtcNow;
        
        await db.SaveChangesAsync();
        
        return new TokenModel
        {
            Token = token,
            RefreshToken = refreshToken
        };
    }

    public async Task<TokenModel> RefreshLogin(string refreshToken)
    {
        var token = new JwtSecurityTokenHandler().ReadJwtToken(refreshToken);

        List<Claim> claims = token.Claims.ToList();

        // Check if refresh token
        if (claims.FirstOrDefault(claim => claim.Type == REFRESH) == null)
        {
            throw new HerpControllerException(HttpStatusCode.Unauthorized, "Invalid token");
        }

        string username;

        try
        {
            username = claims.First(claim => claim.Type == "unique_name").Value;
        }
        catch (Exception)
        {
            throw new HerpControllerException(HttpStatusCode.Unauthorized, "Invalid token");
        }

        var userEntity = await db.Users.FirstOrDefaultAsync(user => user.Username == username && user.Status == UserStatus.ACTIVE);

        if (userEntity == null)
        {
            throw new HerpControllerException(HttpStatusCode.Unauthorized, "Could not find user.");
        }

        if (refreshToken != userEntity.RefreshToken)
        {
            throw new HerpControllerException(HttpStatusCode.Unauthorized, "Invalid/revoked token.");
        }
        
        if (DateTime.UtcNow > userEntity.RefreshTokenExpiry)
        {
            throw new HerpControllerException(HttpStatusCode.Unauthorized, "Token expired.");
        }

        var newToken = GenerateToken(userEntity, false);
        var newRefreshToken = GenerateToken(userEntity, true);
        
        userEntity.RefreshToken = refreshToken;
        userEntity.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        userEntity.ModifiedAt = DateTime.UtcNow;
        
        await db.SaveChangesAsync();

        return new TokenModel
        {
            Token = newToken,
            RefreshToken = newRefreshToken
        };
    }

    public async Task Register(RegisterModel model)
    {
        var username = model.Username;
        var password = model.Password;

        if (string.IsNullOrWhiteSpace(username))
        {
            throw new HerpControllerException(HttpStatusCode.BadRequest, "username is required.");
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new HerpControllerException(HttpStatusCode.BadRequest, "password is required.");
        }

        var userEntity = await db.Users.FirstOrDefaultAsync(entity => entity.Username == username);
        if (userEntity != null)
        {
            throw new HerpControllerException(HttpStatusCode.Unauthorized, "Username already taken.");
        }
        
        //generate new salt + hash and save
        var newSalt = GenerateSalt();
        var hashedPassword = HashPassword(password, newSalt);

        var user = new UserEntity
        {
            Username = username,
            Password = hashedPassword,
            Salt = newSalt,
            Status = UserStatus.ACTIVE,
            CreatedAt = DateTime.UtcNow
        };

        db.Users.Add(user);

        await db.SaveChangesAsync();
    }

    private string GenerateToken(UserEntity user, bool refresh)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenKey = Encoding.ASCII.GetBytes(configuration["JWT:Secret"]!);

        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity([new Claim(ClaimTypes.Name, user.Username)]),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(tokenKey), SecurityAlgorithms.HmacSha256Signature)
        };

        if (refresh)
        {
            tokenDescriptor.Subject = new ClaimsIdentity(tokenDescriptor.Subject.Claims.Append(new Claim(REFRESH, "")));
        }

        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }

    private static string GenerateSalt()
    {
        var bytes = RandomNumberGenerator.GetBytes(128 / 8);
        return Convert.ToBase64String(bytes);
    }

    private static string HashPassword(string password, string salt)
    {
        return Convert.ToBase64String(
            new Rfc2898DeriveBytes(Encoding.UTF8.GetBytes(password), Encoding.UTF8.GetBytes(salt), 10000).GetBytes(24));
    }

    private static string[] DecodeAuth(string encodedAuth)
    {
        // Basic user:pass
        var decodedAuth = Encoding.UTF8.GetString(Convert.FromBase64String(encodedAuth.Split(" ")[1]));
        var authParts = decodedAuth.Split(":");

        if (authParts.Length != 2)
        {
            throw new HerpControllerException(HttpStatusCode.Unauthorized);
        }

        if (authParts.Any(string.IsNullOrWhiteSpace))
        {
            throw new HerpControllerException(HttpStatusCode.Unauthorized);
        }

        return authParts;
    }
}