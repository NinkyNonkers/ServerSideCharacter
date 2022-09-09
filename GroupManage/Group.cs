using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace ServerSideCharacter.GroupManage
{
	public class Group
	{
		[JsonIgnore]
		public string GroupName { get; set; }
		public List<PermissionInfo> Permissions = new List<PermissionInfo>();
		public Color ChatColor;
		public string ChatPrefix = "";

		public Group(string name)
		{
			GroupName = name;
			ChatColor = Color.White;
		}

		public bool IsSuperAdmin()
		{
			return GroupName == "spadmin";
		}

		public bool HasPermission(string name)
		{
			return GroupName == "spadmin" || Permissions.Any(t => t.Name == name);
		}
	}
}
