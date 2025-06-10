using Modact.Data.Models;
using Modact.Data.DAL;

namespace Modact.Data.Cache
{
    public class DatabaseCache
    {
        private static readonly object _locker = new object();

        public static bool IsInited { get; set; } = false;
        public static Dictionary<string, string> AppCache { get; set; }
        public static Dictionary<string, string> ConfigCache { get; set; }
        public static List<string> LicenseCache { get; set; }
        public static List<AppNodeCache> AppNodeCache { get; set; }
        public static Dictionary<string,ConfigJwt> TokenJwtCache { get; set; }

        public static void Refresh(DbHelper dbHelper)
        {
            lock (_locker)
            {
                IsInited = true;

                var appList = (new Dao<DTO_modm_app>(dbHelper)).GetList(new { is_void = false }).ToList();
                AppCache = new Dictionary<string, string>();
                foreach (var app in appList)
                {
                    AppCache.Add(app.app_id, app.app_secret);
                }

                var configList = (new Dao<DTO_modm_config>(dbHelper)).GetList(new { is_void = false }).ToList();
                ConfigCache = new Dictionary<string, string>();
                foreach (var config in configList)
                {
                    ConfigCache.Add(config.config_code, config.config_value);
                }

                var licenseList = (new Dao<DTO_modm_license>(dbHelper)).GetList(new { is_void = false }).ToList();
                LicenseCache = new List<string>();
                foreach (var license in licenseList)
                {
                    LicenseCache.Add(license.license_data);
                }

                var redunList = (new Dao<DTO_modm_appnode>(dbHelper)).GetList(new { is_void = false }).ToList();
                AppNodeCache = new List<AppNodeCache>();
                foreach (var redundancy in redunList)
                {
                    var rdd = new AppNodeCache();
                    rdd.BaseUrl = redundancy.base_url;
                    rdd.IsStandby = redundancy.is_standby;
                    rdd.Priority = redundancy.priority;
                    AppNodeCache.Add(rdd);
                }

                var jwtList = (new Dao<DTO_modm_token_jwt>(dbHelper)).GetList(new { is_void = false }).ToList();
                TokenJwtCache = new Dictionary<string, ConfigJwt>();
                foreach (var jwt in jwtList)
                {
                    var jwtCache = new ConfigJwt();
                    jwtCache.IsAsymmetric = jwt.is_asymmetric;
                    jwtCache.PrivateKey = jwt.private_key_path;
                    jwtCache.PublicKey = jwt.public_key_path;
                    jwtCache.Secret = jwt.secret;
                    jwtCache.Issuer = jwt.issuer;
                    jwtCache.Audience = jwt.audience;
                    jwtCache.ExpireMinute = jwt.expire_minute;
                    TokenJwtCache.Add(jwt.jwt_id, jwtCache);
                }
            }
         }
    }

    public class AppNodeCache
    {
        public string? BaseUrl { get; set; }
        public bool? IsStandby { get; set; }
        public int? Priority { get; set; }
    }
}
