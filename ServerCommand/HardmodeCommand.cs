﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria.ModLoader;
using Terraria;

namespace ServerSideCharacter.ServerCommand
{
	public class HardmodeCommand : ModCommand
	{
		public override string Command
		{
			get { return "hardmode"; }
		}

		public override CommandType Type
		{
			get { return CommandType.Chat; }
		}

		public override string Description
		{
			get { return "Toggle world's hard mode"; }
		}

		public override string Usage
		{
			get { return "/hardmode"; }
		}

		public override void Action(CommandCaller caller, string input, string[] args)
		{
			MessageSender.SendToggleHardmode();
		}
	}
}
