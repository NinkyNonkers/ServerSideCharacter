using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace ServerSideCharacter.ServerCommand
{
	public class TeleportCommand : ModCommand
	{
		public override string Command
		{
			get { return "tp"; }
		}

		public override CommandType Type
		{
			get { return CommandType.Chat; }
		}

		public override string Description
		{
			get { return "Teleport to a player"; }
		}

		public override string Usage
		{
			get { return "/tp <player id|player name>"; }
		}

		public override void Action(CommandCaller caller, string input, string[] args)
		{
			args = Utils.ParseArgs(args);
			int who = Utils.TryGetPlayerId(args[0]);
			if (who == -1)
			{
				Main.NewText("Player not found", Color.Red);
				return;
			}
			MessageSender.SendTeleportCommand(Main.myPlayer, who);
		}
	}
}
