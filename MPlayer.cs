using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace ServerSideCharacter
{
	public class MPlayer : ModPlayer
	{
		public bool Locked;

		public bool GodMode = false;

		public override void ResetEffects()
		{
			Locked = false;
		}

		public override void SetControls()
		{
			if (Locked)
			{
				Player.controlJump = false;
				Player.controlDown = false;
				Player.controlLeft = false;
				Player.controlRight = false;
				Player.controlUp = false;
				Player.controlUseItem = false;
				Player.controlUseTile = false;
				Player.controlThrow = false;
				Player.controlHook = false;
				Player.controlMount = false;
				//player.controlInv = false; // With this the users will not be abble to exit the server without login first (Exept by pressing ALT + F4). This is not a good thing
				Player.gravDir = 0f;
				Player.position = Player.oldPosition;
			}
		}

		public override bool PreHurt(bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
		{
			return !GodMode;
		}

		public override void PreUpdate()
		{
			if (GodMode)
				Player.statLife = Player.statLifeMax2;
		}

		public override void OnEnterWorld(Player player)
		{
			if (Main.netMode != 1) return;
			if (!Mod.TryFind("Locked", out ModBuff buff))
			{
				Console.WriteLine("error: couldn't find locked buff");
				return;
			}
			player.AddBuff(buff.Type, 180);
		}
	}
}
