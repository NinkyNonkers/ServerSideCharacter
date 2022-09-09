﻿using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ServerSideCharacter.ServerCommand
{
	public class KeyCommand : ModCommand
	{
		private static HashSet<int> _keys = new HashSet<int>
		{
			ItemID.HallowedKey,
			ItemID.JungleKey,
			ItemID.CrimsonKey,
			ItemID.CorruptionKey,
			ItemID.FrozenKey,
			ItemID.GoldenKey
		};

		public override string Command
		{
			get { return "key"; }
		}

		public override CommandType Type
		{
			get { return CommandType.Chat; }
		}

		public override string Description
		{
			get { return "Use your biome keys to get weapons"; }
		}

		public override string Usage
		{
			get { return "/key"; }
		}

		private int GetKeyItem(int id)
		{
			if (id == ItemID.JungleKey)
			{
				return ItemID.PiranhaGun;
			}

			if (id == ItemID.HallowedKey)
			{
				return ItemID.RainbowGun;
			}

			if (id == ItemID.CrimsonKey)
			{
				return ItemID.VampireKnives;
			}

			if (id == ItemID.CorruptionKey)
			{
				return ItemID.ScourgeoftheCorruptor;
			}

			if (id == ItemID.FrozenKey)
			{
				return ItemID.StaffoftheFrostHydra;
			}

			if (id == ItemID.GoldenKey)
			{
				int i = Main.rand.Next(5);
				if (i == 0) return ItemID.Handgun;
				if (i == 1) return ItemID.CobaltShield;
				if (i == 2) return ItemID.Muramasa;
				if (i == 3) return ItemID.AquaScepter;
				if (i == 4) return ItemID.BlueMoon;
			}
			return 0;
		}

		public override void Action(CommandCaller caller, string input, string[] args)
		{
			var player = caller.Player;
			bool anyKeys = false;
			for (int i = 0; i < player.inventory.Length; i++)
			{
				if (_keys.Contains(player.inventory[i].type))
				{
					while (player.inventory[i].stack != 0)
					{
						player.inventory[i].stack--;
						player.QuickSpawnItem(new EntitySource_DebugCommand(input), GetKeyItem(player.inventory[i].type));
					}
					player.inventory[i] = new Item();
					anyKeys = true;
				}
			}
			if (!anyKeys)
			{
				Main.NewText("No key in your inventory!", 255, 255, 0);
			}
			else
			{
				Main.NewText("Exchange weapons succeed!", 40, 255, 40);
			}
		}
	}
}
