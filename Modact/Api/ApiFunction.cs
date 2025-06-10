using System.Data;
using System.Diagnostics;
using System.Text.Json;

namespace Modact
{
    [Serializable]
    public class ApiFunction
    {
        private const string _API_FUN_NAMESPACE = "Modact.API.Fun";

        /// <summary>
        /// Function Id
        /// </summary>
        public string Id { get; set; } = string.Empty;
        /// <summary>
        /// Function Name
        /// </summary>
        public string Fun { get; set; }
        /// <summary>
        /// Function Parameters
        /// </summary>
        public Dictionary<string, object> Params { get; set; }

        [JsonIgnore]
        public List<PermissionAttribute>? AttributePermissionList { get; set; }
        [JsonIgnore]
        public DateTime StartTime { get; set; }
        [JsonIgnore]
        public double ProcessSecond { get; set; }
        [JsonIgnore]
        public double StartCpuUsage { get; set; }
        [JsonIgnore]
        public double EndCpuUsage { get; set; }
        [JsonIgnore]
        public double StartMemoryUsageMB { get; set; }
        [JsonIgnore]
        public double EndMemoryUsageMB { get; set; }
        [JsonIgnore]
        public Type? ApiFunctionType { get; set; }
        [JsonIgnore]
        public string? ApiFunctionModuleKey { get; set; }
        [JsonIgnore]
        public string? ApiFunctionModulePath { get; set; }
        [JsonIgnore]
        public string? ApiFunctionNamespace { get; set; }
        [JsonIgnore]
        public string? ApiFunctionClass { get; set; }
        [JsonIgnore]
        public string? ApiFunctionMethod { get; set; }
        [JsonIgnore]
        public bool Success { get; set; }
        [JsonIgnore]
        public string ErrorMessage { get; set; }

        [JsonIgnore]
        public ApiFunctionResult ApiFunctionResult { get; set; } = new();

        [JsonIgnore]
        public ApiFunctionAccessory ApiFunctionAccessory { get; set; }


        public ApiFunction()
        {
            Success = false;
        }

        public ApiFunction(string function)
        {
            Fun = function;
            Success = false;
        }

        public ApiFunction(string id, string function, Dictionary<string, object> param)
        {
            Id = id;
            Fun = function;
            Params = param;
            Success = false;
        }

        public string GetModuleKey()
        {
            if (string.IsNullOrEmpty(Fun))
            {
                throw new ArgumentNullException($"Get function module null: Id={Id},Fun={Fun}");
            }
            string[] actionPart = Fun.Split('/');
            if (actionPart.Length < 1)
            {
                throw new ArgumentException($"Get function module error: Id={Id},Fun={Fun}");
            }
            return ApiFunctionModuleKey = actionPart[0];
        }
        public string GetModulePath(Dictionary<string, string> moduleDlls = null)
        {
            if (moduleDlls == null)
            {
                return string.Empty;
            }
            var moduleKey = GetModuleKey();
            if (!string.IsNullOrEmpty(moduleKey))
            {
                return ApiFunctionModulePath = moduleDlls[moduleKey];
            }
            return string.Empty;
        }
        public string GetNamespace()
        {
            return ApiFunctionNamespace = _API_FUN_NAMESPACE;
        }
        public string GetClassName()
        {
            if (string.IsNullOrEmpty(Fun))
            {
                throw new ArgumentNullException($"Get API function class null: Id={Id},Fun={Fun}");
            }
            string[] actionPart = Fun.Split('/');
            if (actionPart.Length < 2)
            {
                throw new ArgumentException($"Get API function class error: Id={Id},Fun={Fun}");
            }
            return ApiFunctionClass = actionPart[1];
        }
        public string GetMethodName()
        {
            if (string.IsNullOrEmpty(Fun))
            {
                throw new ArgumentNullException($"Get API function method null: Id={Id},Fun={Fun}");
            }
            string[] actionPart = Fun.Split('/');
            if (actionPart.Length < 3)
            {
                throw new ArgumentException($"Get API function method error: Id={Id},Fun={Fun}");
            }
            return ApiFunctionMethod = actionPart[2];
        }

        public (Type?, string?) ParseTypeAndMethod(Dictionary<string, string> moduleDlls = null)
        {
            if(this.ApiFunctionAccessory == null)
            {
                this.ApiFunctionAccessory = new ApiFunctionAccessory(this.Id, this.Fun);
            }

            GetModuleKey();
            GetModulePath(moduleDlls);
            GetNamespace();
            GetClassName();
            GetMethodName();

            Type? classType = null;
            Assembly moduleAssembly;
            try
            {
                //className += "," + actionPart[0];
                if (!string.IsNullOrEmpty(this.ApiFunctionModulePath)) //if module not empty, get class from the module dll
                {
                    moduleAssembly = Assembly.LoadFrom(this.ApiFunctionModulePath);
                    if (moduleAssembly != null)
                    {
                        classType = moduleAssembly.GetType(this.ApiFunctionNamespace + "." + this.ApiFunctionClass);
                    }
                }
                else
                {
                    classType = Type.GetType(this.ApiFunctionNamespace + "." + this.ApiFunctionClass);
                }
            }
            catch
            {
                classType = Type.GetType(this.ApiFunctionNamespace + "." + this.ApiFunctionClass);
            }
            if (classType == null)
            {
                throw new NullReferenceException($"Get class type null: Id={Id},Fun={Fun},Class={this.ApiFunctionNamespace}.{this.ApiFunctionClass},DllPath={this.ApiFunctionModulePath}");
            }

            this.ApiFunctionType = classType;
            var methodInfo = GetFunMethodInfo();
            var attributes = methodInfo.GetCustomAttributes(typeof(PermissionAttribute), false);
            if (attributes != null)
            {
                if (attributes.Length > 0) 
                { 
                    this.AttributePermissionList = new List<PermissionAttribute>(); 
                    foreach ( var attr in attributes) 
                    {
                        if (attr is PermissionAttribute)
                        {
                            var attribute = (PermissionAttribute)attr;
                            var sb = new StringBuilder();
                            sb.Append(this.ApiFunctionModuleKey);
                            sb.Append("/");
                            sb.Append(this.ApiFunctionClass);
                            sb.Append("/");
                            sb.Append(attribute.Permission);
                            attribute.Permission = sb.ToString();
                            this.AttributePermissionList.Add(attribute);
                        }
                    }
                }
            }
            return (classType, this.ApiFunctionMethod);
        }

        private object[] ParseApiFunctionParams(MethodInfo methodInfo, object[]? apiParams, JsonSerializerOptions? options)
        {
            if (apiParams == null) { return null; }

            options = options ?? new JsonSerializerOptions();

            var resultParams = new List<object>();
            try
            {
                for (int i = 0; i < methodInfo.GetParameters().Length; i++)
                {
                    var jsonParam = apiParams[i];
                    if (jsonParam is JsonDocument)
                    {
                        jsonParam = ((JsonDocument)jsonParam).RootElement;
                    }
                    if (jsonParam is JsonElement)
                    {
                        JsonElement element = (JsonElement)jsonParam;
                        Type paramType = methodInfo.GetParameters()[i].ParameterType;
                        var dObject = element.Deserialize(paramType, options);
                        if (dObject is JsonDocument)
                        {
                            resultParams.Add(JsonSerializer.Deserialize<dynamic>(((JsonElement)dObject).GetRawText()));
                            //resultParams.Add(((JsonElement)dObject).GetDynamicObject());
                            //ApiFunctionAccessory.AddMessage(ApiMessageType.Info, ((JsonElement)dObject).GetDynamicObject().GetType());
                        }
                        else
                        {
                            resultParams.Add(dObject);
                        }
                    }
                    else
                    {
                        resultParams.Add(jsonParam);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Parse params error: Id={Id}, Fun={Fun}", ex);
            }
            return resultParams.ToArray();
        }

        public MethodInfo GetFunMethodInfo()
        {
            object[] parameters = null;
            int paramsCount = 0;
            if (this.Params != null)
            {
                parameters = this.Params.Values.ToArray();
                paramsCount = parameters.Count();
            }

            List<MethodInfo> methInfos = new List<MethodInfo>();
            methInfos = this.ApiFunctionType.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                        .Where(m => m.Name == this.ApiFunctionMethod && m.GetParameters().Count() == paramsCount).ToList(); //Get the methods of the class
            if (methInfos == null)
            {
                throw new NotImplementedException($"Get methods is null: Id={this.Id},Fun={this.Fun},ParamCount({paramsCount}) => Class={this.ApiFunctionNamespace}.{this.ApiFunctionClass}");
            }
            else if (methInfos.Count == 0)
            {
                throw new NotImplementedException($"Get methods no match: Id={this.Id},Fun={this.Fun},ParamCount({paramsCount}) => Class={this.ApiFunctionNamespace}.{this.ApiFunctionClass}");
            }
            else if (methInfos.Count > 1)
            {
                throw new NotImplementedException($"Get more than 1 methods: Id={this.Id},Fun={this.Fun},ParamCount({paramsCount}) => Class={this.ApiFunctionNamespace}.{this.ApiFunctionClass}");
            }

            return methInfos[0];
        }

        public async Task<object>? Invoke()
        {
            ConstructorInfo consInfo = this.ApiFunctionType.GetConstructor(new Type[] { typeof(ApiFunctionAccessory) }); //Get the class with constructor 1 parameters type: ApiFunctionResource
            if (consInfo == null)
            {
                throw new NotImplementedException($"Constructor not match: Id={Id},Fun={Fun},Class={this.ApiFunctionNamespace}.{this.ApiFunctionClass}");
            }

            object[] parameters = null;
            int paramsCount = 0;
            if (this.Params != null)
            {
                parameters = this.Params.Values.ToArray();
                paramsCount = parameters.Count();
            }

            var methInfo = this.GetFunMethodInfo();

            object obj = consInfo.Invoke(new object[] { this.ApiFunctionAccessory }); //Get class instance
            object? result = null;

            Type returnType = methInfo.ReturnType;
            bool isReturnTypeIsTask = typeof(Task).IsAssignableFrom(returnType);
            bool isReturnTypeIsGenericTask = returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>);

            this.StartTime = DateTime.Now;
            this.StartCpuUsage = 0;
            this.StartMemoryUsageMB = Process.GetCurrentProcess().WorkingSet64 / 1024 / 1024;
            if ( isReturnTypeIsTask || isReturnTypeIsGenericTask)
            {
                Task resultTask = (Task)methInfo.Invoke(obj, ParseApiFunctionParams(methInfo, parameters, ApiFunctionAccessory.JsonSerializerOptions));  //Invode method of instance
                await resultTask;
                PropertyInfo resultProperty = resultTask.GetType().GetProperty("Result");
                result = resultProperty.GetValue(resultTask);
            }
            else
            {
                try
                {
                    result = methInfo.Invoke(obj, ParseApiFunctionParams(methInfo, parameters, ApiFunctionAccessory.JsonSerializerOptions));  //Invode method of instance
                }
                catch (Exception e)
                {
                    //if (this.ApiFunctionResult != null)
                    //{
                    //this.ApiFunctionResult.Data = null; // [Question] If error, data return null better? or error message better?
                    //}
                    if (this.ApiFunctionAccessory != null)
                    {
                        if (this.ApiFunctionAccessory.ApiFunctionMessage != null)
                        {
                            var errMsg = new ApiMessage();
                            errMsg.FunId = this.Id;
                            errMsg.Type = ApiMessageType.Error;
                            errMsg.Code = ApiMessageCode.UNKNOWN.ToString();
                            errMsg.Message = e.Message;
                            errMsg.Detail = e.ToString();
                            if (e.InnerException != null)
                            {
                                errMsg.Message = e.InnerException.Message;
                                errMsg.Detail = e.InnerException.ToString();
                                if (e.InnerException is ApiMessageException)
                                {
                                    errMsg.Code = ((ApiMessageException)e.InnerException).Code ?? string.Empty;
                                }
                                if (e.InnerException is not ApiNoMessageException)
                                {
                                    this.ApiFunctionAccessory.ApiFunctionMessage.Messages.Add(errMsg);
                                }
                            }
                            else if (e is not ApiNoMessageException)
                            {
                                this.ApiFunctionAccessory.ApiFunctionMessage.Messages.Add(errMsg);
                            }
                        }
                    }
                    this.ProcessSecond = (DateTime.Now - this.StartTime).TotalSeconds;
                    this.EndMemoryUsageMB = Process.GetCurrentProcess().WorkingSet64 / 1024 / 1024;
                    this.ApiFunctionResult.Success = false;
                    this.ApiFunctionResult.Data = result;
                    this.Success = false;
                    throw new ApiNoMessageException();
                }
            }

            this.ProcessSecond = (DateTime.Now - this.StartTime).TotalSeconds;
            this.EndMemoryUsageMB = Process.GetCurrentProcess().WorkingSet64 / 1024 / 1024;
            this.ApiFunctionResult.Success = true;
            this.ApiFunctionResult.Data = result;
            this.Success = true;
            return result;
        }

        public async Task<object?> InvokeAync()
        {
            try
            {
                await Task.Run(() =>
                {
                    return Invoke();
                });
            }
            catch (ApiNoMessageException e)
            {

            }
            catch (Exception e)
            {
                //if (this.ApiFunctionResult != null)
                //{
                //this.ApiFunctionResult.Data = null; // [Question] If error, data return null better? or error message better?
                //}
                if (this.ApiFunctionAccessory != null)
                {
                    if (this.ApiFunctionAccessory.ApiFunctionMessage != null)
                    {
                        var errMsg = new ApiMessage();
                        errMsg.FunId = this.Id;
                        errMsg.Type = ApiMessageType.Error;
                        errMsg.Code = ApiMessageCode.UNKNOWN.ToString();
                        errMsg.Message = e.Message;
                        errMsg.Detail = e.ToString();
                        if (e.InnerException != null)
                        {
                            errMsg.Message = e.InnerException.Message;
                            errMsg.Detail = e.InnerException.ToString();
                            if (e.InnerException is ApiMessageException)
                            {
                                errMsg.Code = ((ApiMessageException)e).Code ?? string.Empty;
                            }
                            if (e.InnerException is ApiNoMessageException)
                            {
                                return null;
                            }
                        }                        
                        this.ApiFunctionAccessory.ApiFunctionMessage.Messages.Add(errMsg);
                    }
                }
            }
            return null;
        }
    }


    [AttributeUsage(AttributeTargets.Method)]
    public class PermissionAttribute : Attribute
    {
        public bool IsEnablePermission { get; set; }
        public string Permission { get; set; }
        public bool IsNecessary { get; set; }

        public PermissionAttribute(bool isEnablePermission = true)
        {
            IsEnablePermission = isEnablePermission;
        }

        public PermissionAttribute(string permission, bool isNecessary = false)
        {
            if (string.IsNullOrEmpty(permission)) {  throw new ArgumentNullException(nameof(permission)); }
            Permission = permission;
            IsNecessary = isNecessary;
        }
    }
}