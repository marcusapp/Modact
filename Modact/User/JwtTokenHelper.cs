using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;

namespace Modact
{
    public partial class JwtTokenHelper
    {
        public static bool IsValidToken(string? token, ConfigJwt jwtConfig)
        {
            if(jwtConfig.IsAsymmetric == true)
            {
                return IsValidTokenAsymmetric(token, jwtConfig);
            }
            else
            {
                return IsValidTokenSymmetric(token, jwtConfig); 
            }
        }
        public static bool IsValidTokenAsymmetric(string? token, ConfigJwt jwtConfig)
        {
            if (string.IsNullOrEmpty(token)) { throw new Exception("Token empty."); }

            string keyPath = Path.Combine(AppInfo.AppPath, jwtConfig.PublicKey);
            if (!File.Exists(keyPath))
            {
                throw new Exception("Public key not exist for validate token.");
            }
            var key = File.ReadAllText(keyPath);
            var rsa = RSA.Create();
            rsa.ImportFromPem(key.AsSpan());

            if (token.IndexOf("Bearer ") == 0)
            {
                token = token.Substring("Bearer ".Length);
            }
            var tokenHandler = new JwtSecurityTokenHandler();
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidIssuer = jwtConfig.Issuer,
                ValidAudience = jwtConfig.Audience,
                IssuerSigningKey = new RsaSecurityKey(rsa),
            }, out SecurityToken validatedToken);
            return true;
        }
        public static bool IsValidTokenSymmetric(string? token, ConfigJwt jwtConfig)
        {
            if (string.IsNullOrEmpty(token)) { throw new Exception("Token empty."); }

            var key = Encoding.UTF8.GetBytes(jwtConfig.Secret);
            var mySecurityKey = new SymmetricSecurityKey(key);

            if (token.IndexOf("Bearer ") == 0)
            {
                token = token.Substring("Bearer ".Length);
            }
            var tokenHandler = new JwtSecurityTokenHandler();
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidIssuer = jwtConfig.Issuer,
                ValidAudience = jwtConfig.Audience,
                IssuerSigningKey = mySecurityKey,
            }, out SecurityToken validatedToken);
            return true;
        }

        public static string GenerateObjectToken<T>(T obj, ConfigJwt jwtConfig, DateTime? currentTime = null)
        {
            if (jwtConfig.IsAsymmetric == true)
            {
                return GenerateObjectTokenAsymmetric(obj, jwtConfig, currentTime, SecurityAlgorithms.RsaSha256);
            }
            else
            {
                return GenerateObjectTokenSymmetric(obj, jwtConfig, currentTime, SecurityAlgorithms.HmacSha256Signature);
            }
        }
        public static string GenerateObjectTokenAsymmetric<T>(T obj, ConfigJwt jwtConfig, DateTime? currentTime = null, string securityAlgorithms = SecurityAlgorithms.RsaSha256)
        {
            currentTime ??= DateTime.Now;

            string keyPath = Path.Combine(AppInfo.AppPath, jwtConfig.PrivateKey);
            if (!File.Exists(keyPath))
            {
                throw new Exception("Private key not exist for generate token. \"" + keyPath + "\"");
            }
            var key = File.ReadAllText(keyPath);
            var rsa = RSA.Create();
            rsa.ImportFromPem(key.AsSpan());
            var signingCredentials = new SigningCredentials(new RsaSecurityKey(rsa), securityAlgorithms);

            var claimsIdentity = new ClaimsIdentity(new[] {
                new Claim("obj", obj.ToStringJson())
            });

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = claimsIdentity,
                Issuer = jwtConfig.Issuer,
                Audience = jwtConfig.Audience,
                Expires = currentTime.Value.AddMinutes(jwtConfig.ExpireMinute ?? 0),
                SigningCredentials = signingCredentials,

            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        public static string GenerateObjectTokenSymmetric<T>(T obj, ConfigJwt jwtConfig, DateTime? currentTime = null, string securityAlgorithms = SecurityAlgorithms.HmacSha256Signature)
        {
            currentTime ??= DateTime.Now;

            var key = Encoding.UTF8.GetBytes(jwtConfig.Secret);
            var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(key), securityAlgorithms);

            var claimsIdentity = new ClaimsIdentity(new[] {
                new Claim("obj", obj.ToStringJson())
            });

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = claimsIdentity,
                Issuer = jwtConfig.Issuer,
                Audience = jwtConfig.Audience,
                Expires = currentTime.Value.AddMinutes(jwtConfig.ExpireMinute ?? 0),
                SigningCredentials = signingCredentials,

            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public static JwtObjectToken? GetToken(string? token)
        {
            if (string.IsNullOrEmpty(token)) { return null; }

            var tokenParts = token.Split('.');
            if (tokenParts.Length > 1)
            {
                JsonSerializerOptions options = new();
                return JsonSerializer.Deserialize<JwtObjectToken>(tokenParts[1].Base64Decode());
            }
            return default;
        }

        public static T? GetObjectInToken<T>(string? token)
        {
            if (string.IsNullOrEmpty(token)) { return default; }

            var tokenParts = token.Split('.');
            if (tokenParts.Length > 1)
            {
                var json = JsonSerializer.Deserialize<JwtObjectToken>(tokenParts[1].Base64Decode());

                JsonSerializerOptions options = new();
                options.Converters.Add(new JsonStringEnumConverter());
                T o = JsonSerializer.Deserialize<T>(json.obj, options);
                return o;
            }
            return default;
        }

        //public static List<string> GetUserPermissionInToken(string token, bool checkValid = false, ConfigJwt jwtPermissionConfig = null)
        //{
        //    if (string.IsNullOrEmpty(token)) { return null; }
        //    bool isValid = true;
        //    if (checkValid)
        //    {
        //        if (!IsValidTokenSymmetric(token, jwtPermissionConfig))
        //        {
        //            isValid = false;
        //        }

        //    }
        //    if (!isValid) { return null; }

        //    var tokenParts = token.Split('.');
        //    if (tokenParts.Length > 1)
        //    {
        //        var json = JsonSerializer.Deserialize<JwtPermissionToken>(tokenParts[1].Base64Decode());
        //        var obj = JsonSerializer.Deserialize<List<string>>(json.UserPermission);
        //        return obj;
        //    }
        //    return null;
        //}

    }

    //[Serializable]
    //public class JwtPermissionToken
    //{
    //    public string UserPermission { get; set; }
    //    public int nbf { get; set; }
    //    public int exp { get; set; }
    //    public int iat { get; set; }
    //    public string iss { get; set; }
    //    public string aud { get; set; }
    //}

    [Serializable]
    public class JwtObjectToken
    {
        public string obj { get; set; }
        public int nbf { get; set; }
        public int exp { get; set; }
        public int iat { get; set; }
        public string iss { get; set; }
        public string aud { get; set; }
    }
}