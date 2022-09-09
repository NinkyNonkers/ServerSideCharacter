using System;
using Terraria.ModLoader;

namespace ServerSideCharacter.Items
{
	public class TestItem : ModItem
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
			DisplayName.SetDefault("Unloaded Item");
			//DisplayName.AddTranslation(GameCulture.Portuguese, "Item Descarregado");
		}

		public void SetUp(string fullName)
		{
			string modName = fullName.Substring(0, fullName.IndexOf('.'));
			string itemName = fullName.Substring(fullName.LastIndexOf('.') + 1);
			Tooltip.SetDefault("Mod: " + modName + Environment.NewLine + "Name: " + itemName);
			//Tooltip.AddTranslation(GameCulture.Portuguese, $"Mod: {modName}{Environment.NewLine}Nome: {itemName}");
			FullName = fullName;
		}
	}
}