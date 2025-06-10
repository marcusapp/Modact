using Modact.Data.Models;
using Dapper.SimpleCRUD;
using System.Linq;

namespace Modact
{
    public class UserPermission
    {
        private bool _isAdmin = false;
        private bool _isGuest = false;

        public bool IsEnableUserPermission = true;

        public bool IsEnableDevicePermission = false;
        public List<string>? Permissions { get; set; }        
        public List<string>? DevicePermissions { get; set; }
        public List<string>? Roles { get; set; }

        public bool IsAdmin()
        {
            return _isAdmin;
        }
        public bool IsGuest()
        {
            return _isGuest;
        }

        public bool IsGrantedPermission(string permissionCode)
        {
            if (!IsEnableUserPermission) { return true; }
            if (_isAdmin) { return true; }
            if (Permissions == null) { return false; }
            return Permissions.Contains(permissionCode);
        }

        public bool IsGrantedPermission(string funNamePermssion, List<PermissionAttribute>? funAttributePermissionList)
        {
            if (!IsEnableUserPermission) { return true; }
            if (_isAdmin) { return true; }
            if (Permissions == null) { return false; }
            if (funAttributePermissionList == null) { return Permissions.Contains(funNamePermssion); }
            else
            {
                bool hasMatchPermission = false;
                foreach(var item in funAttributePermissionList)
                {
                    if (item.IsNecessary)
                    {
                        if (!Permissions.Contains(item.Permission)) { return false; }
                    }
                    else if (Permissions.Contains(item.Permission) || !item.IsEnablePermission)
                    {
                        hasMatchPermission = true;
                    }
                }
                return hasMatchPermission;
            }            
        }

        public bool IsGrantedDevicePermission(string funNamePermssion, List<PermissionAttribute>? funAttributePermissionList)
        {
            if (!IsEnableDevicePermission) { return true; }
            if (_isAdmin) { return true; }
            if (DevicePermissions == null) { return false; }
            if (funAttributePermissionList == null) { return DevicePermissions.Contains(funNamePermssion); }
            else
            {
                bool hasMatchPermission = false;
                foreach (var item in funAttributePermissionList)
                {
                    if (item.IsNecessary)
                    {
                        if (!DevicePermissions.Contains(item.Permission)) { return false; }
                    }
                    else if (DevicePermissions.Contains(item.Permission) || !item.IsEnablePermission)
                    {
                        hasMatchPermission = true;
                    }
                }
                return hasMatchPermission;
            }
        }

        public UserPermission(bool isEnableUserPermission = false)
        {
            IsEnableUserPermission = isEnableUserPermission;
        }

        public UserPermission(DbHelper? appDB, UserToken token, bool isEnableUserPermission = true)
        {
            IsEnableUserPermission = isEnableUserPermission;

            if (!isEnableUserPermission) { return; }

            if (appDB == null) { return; }

            if (token == null) {
                _isGuest = true;
                var perms = appDB.Connection().GetList<DTO_modr_role_permission>(new { is_void = false, role_id = "GUEST" }).ToList();
                if (perms != null)
                {
                    Permissions = new List<string>();
                    perms.ForEach((r) => Permissions.Add(r.permission_code));
                }
                return;
            }

            this.Roles = new List<string>();

            var userRoles = appDB.Connection().GetList<DTO_modr_role_user>(new { is_void = false, user_id = token.UserId }).ToList();
            userRoles.ForEach((r) => this.Roles.Add(r.role_id));
            
            var usergroupRoles = appDB.Connection().GetList<DTO_modr_role_usergroup>(new { is_void = false, user_group_id = token.UserGroup.ToArray() }).ToList();            
            usergroupRoles.ForEach((r) => this.Roles.Add(r.role_id));

            var rolesDetail = appDB.Connection().GetList<DTO_modm_role>(new { is_void = false, role_id = this.Roles.ToArray() }).ToList();
            rolesDetail.ForEach((r) => { if (r.role_id.ToUpper() == "ADMIN") { _isAdmin = true; return; } });

            if (_isAdmin)
            {
                var perms = appDB.Connection().GetList<DTO_modm_permission>(new { is_void = false }).ToList();
                if (perms != null)
                {
                    Permissions = new List<string>();
                    perms.ForEach((r) => Permissions.Add(r.permission_code));
                }
            }
            else
            {
                var perms = appDB.Connection().GetList<DTO_modr_role_permission>(new { is_void = false, role_id = this.Roles.ToArray() }).ToList();
                if (perms != null)
                {
                    Permissions = new List<string>();
                    perms.ForEach((r) => Permissions.Add(r.permission_code));
                }
            }           
        }

    }

    public class UserPermissionInsideFunction
    {
        private bool _isEnablePermission = true;
        private bool _isAdmin = false;

        private string _moduleKey = string.Empty;
        public List<string>? Permissions { get; set; }

        public bool IsEnablePermission()
        {
            return _isEnablePermission;
        }

        public bool IsGrantedPermission(string permissionCode)
        {
            if (!_isEnablePermission) { return true; }
            if (_isAdmin) { return true; }
            if (Permissions == null) { return false; }
            string insidePermissionCode = permissionCode;
            if (permissionCode[0] != '/')
            {
                insidePermissionCode = "/" + permissionCode;
            }
            insidePermissionCode = _moduleKey + insidePermissionCode;
            return Permissions.Contains(insidePermissionCode);
        }

        public UserPermissionInsideFunction(UserPermission? userPermission, string? moduleKey)
        {
            if (!userPermission.IsEnableUserPermission)
            {
                this._isEnablePermission = false;
                return;
            }
            if (moduleKey != null)
            {
                this._moduleKey = moduleKey;
            }
            Permissions = userPermission.Permissions;
        }
    }
}
