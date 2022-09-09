using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace ServerSideCharacter.Items
{
	public class RegionItem : ModItem
	{
		public string FullName;

		public override void SetDefaults()
		{
			Item.height = 32;
			Item.width = 32;
			Item.rare = 10;
			Item.expert = true;
			Item.value = 0;
			Item.useTime = 30;
			Item.useAnimation = 30;
			Item.useStyle = 4;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Region Item");

			//DisplayName.AddTranslation(GameCulture.Chinese, "È¦µØÎïÆ·");

		}

		public override bool AltFunctionUse(Player player)
		{
			return true;
		}
		public override bool? UseItem(Player player)
		{
			
			if (player.altFunctionUse != 2 && Main.mouseLeftRelease)
			{
				Vector2 tilePos = new Vector2(Player.tileTargetX, Player.tileTargetY);
				ServerSideCharacter.TilePos1 = tilePos;
				Main.NewText($"Selected tile positon 1 at ({tilePos.X}, {tilePos.Y})");
			}
			else if (player.altFunctionUse == 2 && Main.mouseRightRelease)
			{
				Vector2 tilePos = new Vector2(Player.tileTargetX, Player.tileTargetY);
				ServerSideCharacter.TilePos2 = tilePos;
				Main.NewText($"Selected tile positon 2 at ({tilePos.X}, {tilePos.Y})");
			}
			return true;
		}

	}
}