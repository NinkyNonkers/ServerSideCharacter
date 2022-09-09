#define DEBUGMODE

using System;
using System.Linq;
using System.Threading;
using IL.Terraria.Chat;
using Microsoft.Xna.Framework;
using ServerSideCharacter.Region;
using ServerSideCharacter.ServerCommand;
using Terraria;
using Terraria.IO;
using Terraria.Localization;
using Terraria.Social;
using Terraria.WorldBuilding;
using ChatHelper = Terraria.Chat.ChatHelper;

namespace ServerSideCharacter
{
	public class MWorld : ModWorld
	{
		public static bool ServerStarted;


		public override void PostUpdate()
		{
			if (Main.netMode == 2)
			{
				try
				{
					ServerStarted = true;
					for (int i = 0; i < 255; i++)
					{
						if (Main.player[i].active)
						{
							ServerPlayer player = ServerSideCharacter.XmlData.Data[Main.player[i].name];
							player.CopyFrom(Main.player[i]);
						}
					}
					if (Main.time % 180 < 1)
					{
						lock(ServerSideCharacter.XmlData)
						{
							foreach (var player in ServerSideCharacter.XmlData.Data)
							{
								if (player.Value.PrototypePlayer != null)
								{
									int playerId = player.Value.PrototypePlayer.whoAmI;
									if (!player.Value.HasPassword)
									{
										player.Value.ApplyLockBuffs();
										ChatHelper.SendChatMessageToClient(NetworkText.FromLiteral("Welcome! You are new to here. Please use /register <password> to register an account!"), new Color(255, 255, 30, 30), playerId);
									}
									if (player.Value.HasPassword && !player.Value.IsLogin)
									{
										player.Value.ApplyLockBuffs();
										ChatHelper.SendChatMessageToClient(NetworkText.FromLiteral("Welcome! You have already created an account. Please type /login <password> to login!"), new Color(255, 255, 30, 30), playerId);
									}
								}
							}
						}
					}
					if (Main.time % 3600 < 1)
					{
						ThreadPool.QueueUserWorkItem(Do_Save);
					}
					foreach (var player in Main.player.Where(p => p.active))
					{
						if (player.GetServerPlayer().EnteredRegion == null)
						{
							var serverPlayer = player.GetServerPlayer();
							RegionInfo region;
							if (serverPlayer.InAnyRegion(out region))
							{
								serverPlayer.EnteredRegion = region;
								serverPlayer.SendInfo(region.WelcomeInfo());
							}
						}
						else if (player.GetServerPlayer().EnteredRegion != null)
						{
							var serverPlayer = player.GetServerPlayer();
							RegionInfo region;
							if (!serverPlayer.InAnyRegion(out region))
							{
								serverPlayer.SendInfo(serverPlayer.EnteredRegion.LeaveInfo());
								serverPlayer.EnteredRegion = null;
							}
						}
					}
				}
				catch (Exception ex)
				{
					CommandBoardcast.ConsoleError(ex);
					WorldFile.SaveWorld();
					Netplay.Disconnect = true;
					SocialAPI.Shutdown();
				}
			}

		}

		private void Do_Save(object state)
		{
			foreach (var player in ServerSideCharacter.XmlData.Data)
			{
				try
				{
					ServerSideCharacter.MainWriter.SavePlayer(player.Value);
				}
				catch (Exception ex)
				{
					CommandBoardcast.ConsoleError(ex);
				}
			}
			ServerSideCharacter.Config.Save();
			CommandBoardcast.ConsoleSaveInfo();
		}


	}
}
