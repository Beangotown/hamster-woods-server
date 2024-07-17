using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Microsoft.IdentityModel.Tokens;

namespace HamsterWoods.Commons;

public static class JwtHelper
{
    private static TokenValidationParameters CreateTokenValidationParameters()
    {
        var result = new TokenValidationParameters
        {
            ValidateIssuer = false,

            ValidateAudience = false,

            ValidateIssuerSigningKey = false,

            //IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(SecretKey)),
            //comment this and add this line to fool the validation logic
            SignatureValidator = delegate(string token, TokenValidationParameters parameters)
            {
                var jwt = new JwtSecurityToken(token);

                return jwt;
            },

            RequireExpirationTime = true,
            ValidateLifetime = true,

            ClockSkew = TimeSpan.Zero
        };

        result.RequireSignedTokens = false;

        return result;
    }

    public static string ExtractRelationIdFromToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var relationId = "";
        if (string.IsNullOrEmpty(token))
        {
            return relationId;
        }
        var tokenWithoutPrefix = token.StartsWith("Bearer ")
            ? token.Substring("Bearer ".Length)
            : token;
        try
        {
            tokenHandler.ValidateToken(tokenWithoutPrefix, CreateTokenValidationParameters(), out var validatedToken);

            // Get claims from validated token
            var claims = (JwtSecurityToken)validatedToken;

            // Access individual claims
            relationId = claims.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
        }
        catch (Exception e)
        {
        }

        return relationId;
    }
}