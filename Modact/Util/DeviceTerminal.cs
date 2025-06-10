using Modact.Data.DAL;
using Modact.Data.Models;

namespace Modact
{
    public class DeviceTerminal
    {
        public static DTO_modm_device? Get(DbHelper appDB, string ip, string macAddr, string machineName, string machineCode)
        {
            var devices = (new Dao<DTO_modm_device>(appDB))
                .GetList(new { is_void = false, ip = ip, mac_address = macAddr, machine_name = machineName, machine_code = machineCode }).ToList();

            if (devices.Count > 0)
            {
                return devices[0];
            }
            return null;
        }

        public static List<string>? GetDeviceUserRole(DbHelper appDB, string deviceId)
        {
            var roles = (new Dao<DTO_modr_device_user>(appDB))
                .GetList(new { is_void = false, device_id = deviceId }).ToList();

            var roleList = new List<string>();

            foreach (var role in roles) 
            {
                roleList.Add(role.role_id);
            }
            return roleList;
        }

        public static List<string>? GetDevicePermission(DbHelper appDB, string deviceId)
        {
            var roles = (new Dao<DTO_modr_device_permission>(appDB))
                .GetList(new { is_void = false, device_id = deviceId }).ToList();

            var roleList = new List<string>();
            foreach (var role in roles)
            {
                roleList.Add(role.role_id);
            }

            var permissions = (new Dao<DTO_modr_role_permission>(appDB))
                .GetList(new { is_void = false, role_id = roleList }).ToList();

            var permissionList = new List<string>();
            foreach (var perm in permissions)
            {
                permissionList.Add(perm.permission_code);
            }

            return permissionList;
        }
    }
}
