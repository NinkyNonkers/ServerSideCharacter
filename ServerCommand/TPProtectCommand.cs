using Terraria;
using Terraria.ModLoader;

namespace ServerSideCharacter.ServerCommand
{
	public class TpProtectCommand : ModCommand
	{
		public override string Command
		{
			get { return "tpprotect"; }
		}

		public override CommandType Type
		{
			get { return CommandType.Chat; }
		}

		public override string Description
		{
			get { return "Toggle teleportation protect"; }
		}

		public override string Usage
		{
			get { return "/tpprotect"; }
		}

		public override void Action(CommandCaller caller, string input, string[] args)
		{
			MessageSender.SendTpProtect(Main.myPlayer);
		}
	}
}
