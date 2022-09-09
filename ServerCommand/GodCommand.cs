﻿using Terraria;
using Terraria.ModLoader;

namespace ServerSideCharacter.ServerCommand
{
	public class GodCommand : ModCommand
	{
		public override string Command
		{
			get { return "god"; }
		}

		public override CommandType Type
		{
			get { return CommandType.Chat; }
		}

		public override string Description
		{
			get { return "Toggle player's god mode"; }
		}

		public override string Usage
		{
			get { return "/god"; }
		}

		public override void Action(CommandCaller caller, string input, string[] args)
		{
			ModPacket pack = ServerSideCharacter.Instance.GetPacket();
			pack.Write((int)SscMessageType.ToggleGodMode);
			pack.Write((byte)Main.myPlayer);
			pack.Send();
		}
	}
}
