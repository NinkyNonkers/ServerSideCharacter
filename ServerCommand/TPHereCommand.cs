﻿using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace ServerSideCharacter.ServerCommand
{
	public class TpHereCommand : ModCommand
	{
		public override string Command
		{
			get { return "tphere"; }
		}

		public override CommandType Type
		{
			get { return CommandType.Chat; }
		}

		public override string Description
		{
			get { return "Teleport a player to your position"; }
		}

		public override string Usage
		{
			get { return "/tphere <player id|player name>"; }
		}

		public override void Action(CommandCaller caller, string input, string[] args)
		{
			int who = Utils.TryGetPlayerId(args[0]);
			if (who == -1)
			{
				Main.NewText("Player not found", Color.Red);
				return;
			}
			ModPacket pack = ServerSideCharacter.Instance.GetPacket();
			pack.Write((int)SscMessageType.TpHereCommand);
			pack.Write((byte)Main.myPlayer);
			pack.Write((byte)who);
			pack.Send();
		}
	}
}
