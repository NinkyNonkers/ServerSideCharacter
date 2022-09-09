using Terraria;

namespace ServerSideCharacter.Config
{
	public class NetItem
	{
		public int ItemId;
		public int Prefix;
		public bool IsModItem;
		public bool IsFavorite;
		public string ModName;
		public string ItemName;

		public bool TheSameItem(Item item)
		{
			if (item.ModItem != null)
			{
				NetItem tmp = Utils.ToNetItem(item);
				return tmp.ModName == ModName && tmp.ItemName == ItemName;
			}

			return item.type == ItemId;
		}
	}
}
