global using System.Reflection;
global using System.Text;
global using System.Text.Json;
global using System.Text.Json.Serialization;

using Microsoft.Extensions.Configuration;
using Modact.Data.Cache;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;


namespace Modact
{
    public class AppSettings
    {
        public static LogToFile LogModact { get; set; }

        public AppSettings(IConfiguration appSettings)
        {
            this.Json = appSettings;
            this.AppConfig = appSettings.GetSection(nameof(AppConfig)).Get<AppConfig>();
            this.DataConfig = appSettings.GetSection(nameof(DataConfig)).Get<DataConfig>();
            this.ModactConfig = appSettings.GetSection(nameof(ModactConfig)).Get<ModactConfig>();
        }
        public AppConfig AppConfig { get; set; }
        public DataConfig DataConfig { get; set; }

        /// <summary>
        /// In normal case, should use GetModactConfigValue<T>() to get config from database first
        /// </summary>
        public ModactConfig ModactConfig { get; set; }

        public string? AppCode
        {
            get
            {
                try
                {
                    var dllPath = AppConfig.Module[AppConfig.AppInfoModule];

                    Assembly moduleAssembly = Assembly.LoadFrom(dllPath);
                    Type functionType = moduleAssembly.GetType(AppConfig.AppInfoNamespace + ".AppInfo");
                    if (functionType == null)
                    {
                        functionType = Type.GetType(AppConfig.AppInfoNamespace + ".AppInfo");
                    }
                    return functionType.GetProperty("AppCode", BindingFlags.Public | BindingFlags.Static).GetValue(null).ToString();
                }
                catch
                {
                    return AppInfo.AppCode;
                }
            }
        }
        public string? AppName
        {
            get
            {
                try
                {
                    var dllPath = AppConfig.Module[AppConfig.AppInfoModule];

                    Assembly moduleAssembly = Assembly.LoadFrom(dllPath);
                    Type functionType = moduleAssembly.GetType(AppConfig.AppInfoNamespace + ".AppInfo");
                    if (functionType == null)
                    {
                        functionType = Type.GetType(AppConfig.AppInfoNamespace + ".AppInfo");
                    }
                    return functionType.GetProperty("AppName", BindingFlags.Public | BindingFlags.Static).GetValue(null).ToString();
                }
                catch
                {
                    return AppInfo.AppName;
                }
            }
        }
        public string? AppVersionString
        {
            get
            {
                try
                {
                    var dllPath = AppConfig.Module[AppConfig.AppInfoModule];

                    Assembly moduleAssembly = Assembly.LoadFrom(dllPath);
                    Type functionType = moduleAssembly.GetType(AppConfig.AppInfoNamespace + ".AppInfo");
                    if (functionType == null)
                    {
                        functionType = Type.GetType(AppConfig.AppInfoNamespace + ".AppInfo");
                    }
                    return functionType.GetProperty("AppVersionString", BindingFlags.Public | BindingFlags.Static).GetValue(null).ToString();
                }
                catch
                {
                    return AppInfo.AppVersionString;
                }
            }
        }

        #region json_settings_raw_access
        private IConfiguration? Json { get; set; }
        /// <summary>
        ///     
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">Use ":" to split the structure, example "AppConfig:AppId"</param>
        /// <returns></returns>
        private T GetValue<T>(string key)
        {
            return this.Json.GetValue<T>(key);
        }
        /// <summary>
        ///     
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">Use ":" to split the structure, example "AppConfig:AppId"</param>
        /// <returns></returns>
        private T GetValue<T>(string key, T defaultValue)
        {
            return this.Json.GetValue<T>(key, defaultValue);
        }
        #endregion

        public T? Get<T>(string key)
        {
            return Get<T>(key, default(T));
        }
        public T? Get<T>(string key, T defaultValue)
        {
            if (key.Contains(':'))
            {
                return GetModactConfigValue<T>(key, defaultValue);
            }
            else
            {
                return this.GetValue<T>(typeof(AppConfig).Name + ":" + key, defaultValue);
            }
        }

        public T? GetModactConfigValue<T>(string key)
        {
            return GetModactConfigValue<T>(key, default(T));
        }
        public T? GetModactConfigValue<T>(string key, T defaultValue)
        {
            if (DatabaseCache.ConfigCache != null)
            {
                if (DatabaseCache.ConfigCache.ContainsKey(key))
                {
                    return (T?)ConvertValue(typeof(T), DatabaseCache.ConfigCache[key]);
                }
            }
            return this.GetValue<T>(key, defaultValue);
        }
        
        public bool IsApiKeyEnable()
        {
            return GetModactConfigValue<bool>("ModactConfig:ApiKeyOption:ApiKeyEnable");
        }
        public bool IsUserTokenEnable()
        {
            return GetModactConfigValue<bool>("ModactConfig:TokenOption:UserTokenEnable");
        }
        public bool IsDeviceControlEnable()
        {
            return GetModactConfigValue<bool>("Modact.DeviceControl.Enable");
        }
        public bool IsDeviceControlUserEnable()
        {
            return GetModactConfigValue<bool>("Modact.DeviceControl.User.Enable");
        }
        public bool IsDeviceControlPermissionEnable()
        {
            return GetModactConfigValue<bool>("Modact.DeviceControl.Permission.Enable");
        }
        public bool IsLogToFileEnable() 
        {
            return GetModactConfigValue<bool>("ModactConfig:LogOption:LogModactApiOption:LogToFileEnable");
        }
        public bool IsLogToDatabaseEnable()
        {
            return GetModactConfigValue<bool>("ModactConfig:LogOption:LogModactApiOption:LogToDatabaseEnable");
        }
        public bool IsLogToDatabaseFailThenLogToFile()
        {
            return GetModactConfigValue<bool>("ModactConfig:LogOption:LogModactApiOption:LogToDatabaseFailThenLogToFile");
        }
        public bool IsLogOnlyError()
        {
            return GetModactConfigValue<bool>("ModactConfig:LogOption:LogModactApiOption:LogOnlyError");
        }
        public bool IsLogWithInputData()
        {
            return GetModactConfigValue<bool>("ModactConfig:LogOption:LogModactApiOption:LogWithInputData");
        }
        public bool IsLogWithOutputData()
        {
            return GetModactConfigValue<bool>("ModactConfig:LogOption:LogModactApiOption:LogWithOutputData");
        }

        public JsonSerializerOptions GetJsonSerializerOptions()
        {
            JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
            JsonSerializerOptions.PropertyNamingPolicy = null;
            JsonSerializerOptions.IncludeFields = true;
            JsonSerializerOptions.WriteIndented = true;
            JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            JsonSerializerOptions.Converters.Add(new DateTimeJsonConverter(GetDateTimeToStringFormat(DateTimeToStringFormat.DateTimeMillisecond)));
            return JsonSerializerOptions;
        }
        public string GetDateTimeToStringFormat(DateTimeToStringFormat format)
        {
            string result = string.Empty;
            switch (format)
            {
                case DateTimeToStringFormat.Date:
                    return GetModactConfigValue<string>("ModactConfig:DateTimeToStringFormatOption:Date", "yyyy-MM-dd");
                case DateTimeToStringFormat.Time:
                    return GetModactConfigValue<string>("ModactConfig:DateTimeToStringFormatOption:Time", "HH:mm:ss");
                case DateTimeToStringFormat.DateTime:
                    return GetModactConfigValue<string>("ModactConfig:DateTimeToStringFormatOption:DateTime", "yyyy-MM-dd HH:mm:ss");
                case DateTimeToStringFormat.DateTimeMillisecond:
                    return GetModactConfigValue<string>("ModactConfig:DateTimeToStringFormatOption:DateTimeMillisecond", "yyyy-MM-dd HH:mm:ss.fff");
                default:
                    return GetModactConfigValue<string>("ModactConfig:DateTimeToStringFormatOption:DateTime", "yyyy-MM-dd HH:mm:ss.fff");
            }
            throw new Exception($"Cannot find format config by [{format}]");
        }
        public string GetAppSecret(string appId)
        {
            if (string.IsNullOrEmpty(appId))
            {
                throw new Exception($"API Key error: App ID null or empty.");
            }

            try
            {
                if (DatabaseCache.AppCache != null)
                {
                    return DatabaseCache.AppCache[appId];
                }
                else
                {
                    return ModactConfig.ApiKeyOption.ApiKeyList[appId];

                }
            }
            catch (Exception ex)
            {
                throw new Exception($"API Key error: App ID '{appId}' invalid.", ex);
            }            
        }
        public ConfigJwt GetConfigJwt(string jwtId)
        {
            if (string.IsNullOrEmpty(jwtId))
            {
                throw new Exception($"JWT config error: null or empty.");
            }

            try
            {
                if (DatabaseCache.TokenJwtCache != null)
                {
                    return DatabaseCache.TokenJwtCache[jwtId];
                }
                else
                {
                    return this.ModactConfig.TokenOption.Jwt[jwtId];

                }
            }
            catch (Exception ex)
            {
                throw new Exception($"JWT config error: ID '{jwtId}' invalid.", ex);
            }            
        }
        public List<ConfigJwt> GetConfigJwtList()
        {
            var list = new List<ConfigJwt>();
            if (DatabaseCache.TokenJwtCache != null)
            {
                if (DatabaseCache.TokenJwtCache.Count > 0)
                {
                    foreach (var jwtPair in DatabaseCache.TokenJwtCache)
                    {
                        list.Add(jwtPair.Value);
                    }
                }
            }
            else
            {
                if (ModactConfig.TokenOption.Jwt.Count > 0)
                {
                    foreach (var jwtPair in ModactConfig.TokenOption.Jwt)
                    {
                        list.Add(jwtPair.Value);
                    }
                }

            }
            return list;
        }

        private bool TryConvertValue(
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
            Type type,
            string value, out object? result, out Exception? error)
        {
            error = null;
            result = null;
            if (type == typeof(bool))
            {
                result = value.ModactConfigAsBOOL();
                return true;
            }

            if (type == typeof(DateTime))
            {
                result = value.ModactConfigAsDATETIME2();
                return true;
            }

            if (type == typeof(int))
            {
                result = value.ModactConfigAsINT();
                return true;
            }

            if (type == typeof(decimal))
            {
                result = value.ModactConfigAsDECIMAL();
                return true;
            }

            if (type == typeof(object))
            {
                result = value;
                return true;
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                if (string.IsNullOrEmpty(value))
                {
                    return true;
                }
                return TryConvertValue(Nullable.GetUnderlyingType(type)!, value, out result, out error);
            }

            TypeConverter converter = TypeDescriptor.GetConverter(type);
            if (converter.CanConvertFrom(typeof(string)))
            {
                try
                {
                    result = converter.ConvertFromInvariantString(value);
                }
                catch (Exception ex)
                {
                    error = new InvalidOperationException("Error convert value: " + type, ex);
                }
                return true;
            }

            if (type == typeof(byte[]))
            {
                try
                {
                    result = Convert.FromBase64String(value);
                }
                catch (FormatException ex)
                {
                    error = new InvalidOperationException("Error convert value; " + type, ex);
                }
                return true;
            }

            return false;
        }

        private object? ConvertValue(
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
            Type type,
            string value)
        {
            TryConvertValue(type, value, out object? result, out Exception? error);
            if (error != null)
            {
                throw error;
            }
            return result;
        }
    }


}
