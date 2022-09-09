﻿using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace ServerSideCharacter.ServerCommand
{
	public class RegionCommand : ModCommand
	{
		public override string Command
		{
			get { return "region"; }
		}

		public override CommandType Type
		{
			get { return CommandType.Chat; }
		}

		public override string Description
		{
			get { return "Manage the regions in the server"; }
		}

		public override string Usage
		{
			get { return "/region <create/share/delete> <region name> [player name/player id]"; }
		}

		public override void Action(CommandCaller caller, string input, string[] args)
		{
			args = Utils.ParseArgs(args);
			if (args[0] == "create")
			{
				if ((ServerSideCharacter.TilePos1 != Vector2.Zero) && (ServerSideCharacter.TilePos2 != Vector2.Zero))
				{
					string name = args[1];
					MessageSender.SendRegionCreate(Main.myPlayer, name);
					ServerSideCharacter.TilePos1 = ServerSideCharacter.TilePos2 = Vector2.Zero;
				}
				else
				{
					Main.NewText("Region position is invalid or you haven't select any region", 255, 255, 0);
				}
			}
			else if (args[0] == "share")
			{
				string name = args[1];
				int who = Utils.TryGetPlayerId(args[2]);
				if (who == -1)
				{
					Main.NewText("Player not found", Color.Red);
					return;
				}
				if (who != Main.myPlayer)
				{
					MessageSender.SendRegionShare(Main.myPlayer, name, who);
				}
				else
				{
					Main.NewText("You cannot share region with yourself", 255, 255, 0);
				}
			}
			else if (args[0] == "delete")
			{
				string name = args[1];
				MessageSender.SendRegionRemove(Main.myPlayer, name);
			}
		}
	}
}
