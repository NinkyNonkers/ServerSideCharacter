using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace ServerSideCharacter.Region
{
	public class RegionInfo
	{
		public string Name { get; set; }
		public ServerPlayer Owner { get; set; }
		public List<int> SharedOwner = new List<int>();
		public Rectangle Area { get; set; }

		public RegionInfo(string name, ServerPlayer player, Rectangle rect)
		{
			Name = name;
			Owner = player;
			Area = rect;
		}

		public string WelcomeInfo()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine($"Welcome to region '{Name}'!");
			sb.AppendLine($"*Region Owner: {Owner.Name}");
			sb.AppendLine($"*Region Area: {Area.ToString()}");
			return sb.ToString();
		}

		public string LeaveInfo()
		{
			return $"You have left '{Name}'";
		}
	}
}
