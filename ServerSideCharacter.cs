#define DEBUGMODE
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Threading;
using Microsoft.Xna.Framework;
using ServerSideCharacter.Config;
using ServerSideCharacter.Config.Group;
using ServerSideCharacter.Extensions;
using ServerSideCharacter.Region;
using ServerSideCharacter.ServerCommand;
using Terraria;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.GameContent.Events;
using Terraria.ID;
using Terraria.IO;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.Social;

namespace ServerSideCharacter
{
	[SuppressMessage("ReSharper", "InvertIf")]
	public class ServerSideCharacter : Mod
	{
		public static ServerSideCharacter Instance;

		public static PlayerData XmlData;

		public static XmlWriter MainWriter;

		public static Thread CheckDisconnect;

		public static string ApiVersion = "V0.3";

		public static List<Command> Commands = new List<Command>();

		public static RegionManager RegionManager = new RegionManager();

		public static ErrorLogger Logger;

		public static string AuthCode = "";

		public static Vector2 TilePos1 = new Vector2();

		public static Vector2 TilePos2 = new Vector2();

		public static ServerConfigManager Config;

		public static MessageChecker MessageChecker;

		public static ChestManager ChestManager;

		public static GroupConfigManager GroupManager;

		public ServerSideCharacter()
		{
			Properties = new ModProperties
			{
				Autoload = true,
				AutoloadSounds = true,
				AutoloadGores = true
			};

		}

		public override bool HijackSendData(int whoAmI, int msgType, int remoteClient, int ignoreClient, NetworkText text, int number, float number2, float number3, float number4, int number5, int number6, int number7)
		{
			switch (msgType)
			{
				case MessageID.WorldData:
					if (Main.netMode != 2) // we will not process this message in client-side
					{
						break;
					}

					var memoryStream = new MemoryStream();
					var binaryWriter = new BinaryWriter(memoryStream);
					var position = binaryWriter.BaseStream.Position;
					binaryWriter.BaseStream.Position += 2L;
					binaryWriter.Write(MessageID.WorldData);

					binaryWriter.Write((int)Main.time);
					BitsByte bb3 = 0;
					bb3[0] = Main.dayTime;
					bb3[1] = Main.bloodMoon;
					bb3[2] = Main.eclipse;
					binaryWriter.Write(bb3);
					binaryWriter.Write((byte)Main.moonPhase);
					binaryWriter.Write((short)Main.maxTilesX);
					binaryWriter.Write((short)Main.maxTilesY);
					binaryWriter.Write((short)Main.spawnTileX);
					binaryWriter.Write((short)Main.spawnTileY);
					binaryWriter.Write((short)Main.worldSurface);
					binaryWriter.Write((short)Main.rockLayer);
					binaryWriter.Write(Main.worldID);
					binaryWriter.Write(Main.worldName);
					binaryWriter.Write(Main.ActiveWorldFileData.UniqueId.ToByteArray());
					binaryWriter.Write(Main.ActiveWorldFileData.WorldGeneratorVersion);
					binaryWriter.Write((byte)Main.moonType);
					binaryWriter.Write((byte)WorldGen.treeBG);
					binaryWriter.Write((byte)WorldGen.corruptBG);
					binaryWriter.Write((byte)WorldGen.jungleBG);
					binaryWriter.Write((byte)WorldGen.snowBG);
					binaryWriter.Write((byte)WorldGen.hallowBG);
					binaryWriter.Write((byte)WorldGen.crimsonBG);
					binaryWriter.Write((byte)WorldGen.desertBG);
					binaryWriter.Write((byte)WorldGen.oceanBG);
					binaryWriter.Write((byte)Main.iceBackStyle);
					binaryWriter.Write((byte)Main.jungleBackStyle);
					binaryWriter.Write((byte)Main.hellBackStyle);
					binaryWriter.Write(Main.windSpeedSet);
					binaryWriter.Write((byte)Main.numClouds);
					for (var k = 0; k < 3; k++)
					{
						binaryWriter.Write(Main.treeX[k]);
					}
					for (var l = 0; l < 4; l++)
					{
						binaryWriter.Write((byte)Main.treeStyle[l]);
					}
					for (var m = 0; m < 3; m++)
					{
						binaryWriter.Write(Main.caveBackX[m]);
					}
					for (var n = 0; n < 4; n++)
					{
						binaryWriter.Write((byte)Main.caveBackStyle[n]);
					}
					if (!Main.raining)
					{
						Main.maxRaining = 0f;
					}
					binaryWriter.Write(Main.maxRaining);
					BitsByte bb4 = 0;
					bb4[0] = WorldGen.shadowOrbSmashed;
					bb4[1] = NPC.downedBoss1;
					bb4[2] = NPC.downedBoss2;
					bb4[3] = NPC.downedBoss3;
					bb4[4] = Main.hardMode;
					bb4[5] = NPC.downedClown;
					bb4[6] = true; // ssc mode here
					bb4[7] = NPC.downedPlantBoss;
					binaryWriter.Write(bb4);
					BitsByte bb5 = 0;
					bb5[0] = NPC.downedMechBoss1;
					bb5[1] = NPC.downedMechBoss2;
					bb5[2] = NPC.downedMechBoss3;
					bb5[3] = NPC.downedMechBossAny;
					bb5[4] = Main.cloudBGActive >= 1f;
					bb5[5] = WorldGen.crimson;
					bb5[6] = Main.pumpkinMoon;
					bb5[7] = Main.snowMoon;
					binaryWriter.Write(bb5);
					BitsByte bb6 = 0;
					bb6[0] = Main.expertMode;
					bb6[1] = Main.fastForwardTime;
					bb6[2] = Main.slimeRain;
					bb6[3] = NPC.downedSlimeKing;
					bb6[4] = NPC.downedQueenBee;
					bb6[5] = NPC.downedFishron;
					bb6[6] = NPC.downedMartians;
					bb6[7] = NPC.downedAncientCultist;
					binaryWriter.Write(bb6);
					BitsByte bb7 = 0;
					bb7[0] = NPC.downedMoonlord;
					bb7[1] = NPC.downedHalloweenKing;
					bb7[2] = NPC.downedHalloweenTree;
					bb7[3] = NPC.downedChristmasIceQueen;
					bb7[4] = NPC.downedChristmasSantank;
					bb7[5] = NPC.downedChristmasTree;
					bb7[6] = NPC.downedGolemBoss;
					bb7[7] = BirthdayParty.PartyIsUp;
					binaryWriter.Write(bb7);
					BitsByte bb8 = 0;
					bb8[0] = NPC.downedPirates;
					bb8[1] = NPC.downedFrost;
					bb8[2] = NPC.downedGoblins;
					bb8[3] = Sandstorm.Happening;
					bb8[4] = DD2Event.Ongoing;
					bb8[5] = DD2Event.DownedInvasionT1;
					bb8[6] = DD2Event.DownedInvasionT2;
					bb8[7] = DD2Event.DownedInvasionT3;
					binaryWriter.Write(bb8);
					binaryWriter.Write((sbyte)Main.invasionType);
					if (!ModNet.AllowVanillaClients)
					{
						// We have to call `WorldIO.SendModData(binaryWriter)` using reflection
						var type = typeof(Main).Assembly.GetType("Terraria.ModLoader.IO.WorldIO");
						var method = type.GetMethod("SendModData", new[] {typeof(BinaryWriter)});
						method.Invoke(null, new object[] {binaryWriter});
					}
					binaryWriter.Write(SocialAPI.Network != null ? SocialAPI.Network.GetLobbyId() : 0UL);
					binaryWriter.Write(Sandstorm.IntendedSeverity);

					var currentPosition = (int)binaryWriter.BaseStream.Position;
					binaryWriter.BaseStream.Position = position;
					binaryWriter.Write((short)currentPosition);
					binaryWriter.BaseStream.Position = currentPosition;
					var data = memoryStream.ToArray();

					binaryWriter.Close();

					if (remoteClient == -1)
					{
						for (var index = 0; index < 256; index++)
						{
							if (index != ignoreClient && (NetMessage.buffer[index].broadcast || Netplay.Clients[index].State >= 3 && msgType == 10) && Netplay.Clients[index].IsConnected())
							{
								try
								{
									NetMessage.buffer[index].spamCount++;
									Main.txMsg++;
									Main.txData += currentPosition;
									Main.txMsgType[msgType]++;
									Main.txDataType[msgType] += currentPosition;
									Netplay.Clients[index].Socket.AsyncSend(data, 0, data.Length,
										Netplay.Clients[index].ServerWriteCallBack);
								}
								catch
								{
									// ignored
								}
							}
						}
					}
					else
					{
						try
						{
							Netplay.Clients[remoteClient].Socket.AsyncSend(data, 0, data.Length,
								Netplay.Clients[remoteClient].ServerWriteCallBack);
						}
						catch
						{
							// ignored
						}
					}

					return true;
			}

			return false;
		}


		public override bool HijackGetData(ref byte messageType, ref BinaryReader reader, int playerNumber)
		{
			return MessageChecker.CheckMessage(ref messageType, ref reader, playerNumber);
		}



		public static void SyncConnectedPlayer(int plr)
		{
			SyncOnePlayer(plr, -1, plr);
			for (int i = 0; i < 255; i++)
			{
				if (plr != i && Main.player[i].active)
				{
					SyncOnePlayer(i, plr, -1);
				}
			}
			SendNpcHousesAndTravelShop(plr);
			SendAnglerQuest(plr);
			EnsureLocalPlayerIsPresent();
		}

		private static void SyncOnePlayer(int plr, int toWho, int fromWho)
		{
			int num = 0;
			if (Main.player[plr].active)
			{
				num = 1;
			}
			if (Netplay.Clients[plr].State == 10)
			{
				NetMessage.SendData(MessageID.PlayerActive, -1, -1, NetworkText.Empty, plr, num);
				NetMessage.SendData(MessageID.SyncPlayer, -1, -1, NetworkText.FromLiteral(Main.player[plr].name), plr);
				NetMessage.SendData(MessageID.PlayerControls, -1, -1, NetworkText.Empty, plr);
				MessageSender.SyncPlayerHealth(plr, -1, -1);
				NetMessage.SendData(MessageID.PlayerPvP, -1, -1, NetworkText.Empty, plr);
				NetMessage.SendData(MessageID.PlayerTeam, -1, -1, NetworkText.Empty, plr);
				MessageSender.SyncPlayerMana(plr, -1, -1);
				NetMessage.SendData(MessageID.PlayerBuffs, -1, -1, NetworkText.Empty, plr);

				string name = Main.player[plr].name;
				ServerPlayer player = XmlData.Data[name];
				player.Inventory.CopyTo(Main.player[plr].inventory, 0);
				player.Armor.CopyTo(Main.player[plr].armor, 0);
				player.Dye.CopyTo(Main.player[plr].dye, 0);
				player.MiscEquips.CopyTo(Main.player[plr].miscEquips, 0);
				player.MiscDye.CopyTo(Main.player[plr].miscDyes, 0);
				Main.player[plr].trashItem = new Item();
				player.PrototypePlayer = Main.player[plr];


				if (toWho == -1)
				{
					player.IsLogin = false;
					player.ApplyLockBuffs();
				}

				for (int i = 0; i < 59; i++)
				{
					NetMessage.SendData(MessageID.SyncEquipment, -1, -1, NetworkText.FromLiteral(Main.player[plr].inventory[i].Name), plr, i, Main.player[plr].inventory[i].prefix);
				}
				for (int j = 0; j < Main.player[plr].armor.Length; j++)
				{
					NetMessage.SendData(MessageID.SyncEquipment, -1, -1, NetworkText.FromLiteral(Main.player[plr].armor[j].Name), plr, (59 + j), Main.player[plr].armor[j].prefix);
				}
				for (int k = 0; k < Main.player[plr].dye.Length; k++)
				{
					NetMessage.SendData(MessageID.SyncEquipment, -1, -1, NetworkText.FromLiteral(Main.player[plr].dye[k].Name), plr, (58 + Main.player[plr].armor.Length + 1 + k), Main.player[plr].dye[k].prefix);
				}
				for (int l = 0; l < Main.player[plr].miscEquips.Length; l++)
				{
					NetMessage.SendData(MessageID.SyncEquipment, -1, -1, NetworkText.Empty, plr, 58 + Main.player[plr].armor.Length + Main.player[plr].dye.Length + 1 + l, Main.player[plr].miscEquips[l].prefix);
				}
				for (int m = 0; m < Main.player[plr].miscDyes.Length; m++)
				{
					NetMessage.SendData(MessageID.SyncEquipment, -1, -1, NetworkText.Empty, plr, 58 + Main.player[plr].armor.Length + Main.player[plr].dye.Length + Main.player[plr].miscEquips.Length + 1 + m, Main.player[plr].miscDyes[m].prefix);
				}
				NetMessage.SendData(MessageID.SyncEquipment, -1, -1, NetworkText.FromLiteral(Main.player[plr].trashItem.Name),
					plr, 58 + Main.player[plr].armor.Length + Main.player[plr].dye.Length +
					Main.player[plr].miscEquips.Length + 7, Main.player[plr].trashItem.prefix);
				MessageSender.SyncPlayerBanks(plr, -1, -1);
				PlayerHooks.SyncPlayer(Main.player[plr], toWho, fromWho, false);
				if (!Netplay.Clients[plr].IsAnnouncementCompleted)
				{
					Netplay.Clients[plr].IsAnnouncementCompleted = true;
					ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(Main.player[plr].name + " joined the Game. Welcome!"), new Color(255, 255, 240, 20), plr);
					if (Main.dedServ)
					{
						Console.WriteLine(Main.player[plr].name + " joined the Game. Welcome!");
					}
				}
			}
			else
			{
				num = 0;
				NetMessage.SendData(MessageID.PlayerActive, -1, plr, NetworkText.Empty, plr, num);
				if (Netplay.Clients[plr].IsAnnouncementCompleted)
				{
					Netplay.Clients[plr].IsAnnouncementCompleted = false;
					ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(Netplay.Clients[plr].Name + " lefted the Game!"), new Color(255, 255, 240, 20), plr);
					if (Main.dedServ)
					{
						Console.WriteLine(Netplay.Clients[plr].Name + " lefted the Game!");
					}
					Netplay.Clients[plr].Name = "Anonymous";
				}
			}
		}

		private static void SendNpcHousesAndTravelShop(int plr)
		{
			bool flag = false;
			for (int i = 0; i < 200; i++)
			{
				if (Main.npc[i].active && Main.npc[i].townNPC && NPC.TypeToHeadIndex(Main.npc[i].type) != -1)
				{
					if (!flag && Main.npc[i].type == 368)
					{
						flag = true;
					}
					int num = 0;
					if (Main.npc[i].homeless)
					{
						num = 1;
					}
					NetMessage.SendData(MessageID.NPCHome, plr, -1, NetworkText.Empty, i, Main.npc[i].homeTileX, Main.npc[i].homeTileY, num);
				}
			}
			if (flag)
			{
				NetMessage.SendTravelShop(plr);
			}
		}

		public static void SendAnglerQuest(int remoteClient)
		{
			if (Main.netMode != 2)
			{
				return;
			}
			if (remoteClient == -1)
			{
				for (int i = 0; i < 255; i++)
				{
					if (Netplay.Clients[i].State == 10)
					{
						NetMessage.SendData(MessageID.AnglerQuest, i, -1, NetworkText.FromLiteral(Main.player[i].name), Main.anglerQuest);
					}
				}
				return;
			}
			if (Netplay.Clients[remoteClient].State == 10)
			{
				NetMessage.SendData(MessageID.AnglerQuest, remoteClient, -1, NetworkText.FromLiteral(Main.player[remoteClient].name), Main.anglerQuest);
			}
		}

		private static void EnsureLocalPlayerIsPresent()
		{
			if (!Main.autoShutdown)
			{
				return;
			}
			bool flag = false;
			for (int i = 0; i < 255; i++)
			{
				if (Netplay.Clients[i].State == 10 && Netplay.Clients[i].Socket.GetRemoteAddress().IsLocalHost())
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				Console.WriteLine(Language.GetTextValue("Net.ServerAutoShutdown"));
				WorldFile.SaveWorld();
				Netplay.Disconnect = true;
			}
		}


		public override void PostSetupContent()
		{
			MessageChecker = new MessageChecker();
			if (Main.dedServ)
			{
				SetupDefaults();

				if (!File.Exists("SSC/authcode"))
				{
					string authcode = Convert.ToString((Main.rand.Next(300000) + DateTime.Now.Millisecond) % 65536 + 65535, 16);
					AuthCode = authcode;
					using (StreamWriter sw = new StreamWriter("SSC/authcode"))
					{
						sw.WriteLine(authcode);
					}
				}
				else
				{
					using (StreamReader sr = new StreamReader("SSC/authcode"))
					{
						AuthCode = sr.ReadLine();
					}
				}



				XmlData = new PlayerData("SSC/datas.xml");
				RegionManager.ReadRegionInfo();
				CommandBoardcast.ConsoleMessage("Data loaded!");
				CommandBoardcast.ConsoleMessage("You can type /auth " + AuthCode + " to become super admin");

				CheckDisconnect = new Thread(() =>
				{
					while (!Netplay.Disconnect)
					{

						Thread.Sleep(100);
					}
					lock (this)
					{
						foreach (var player in XmlData.Data)
						{
							try
							{
								MainWriter.SavePlayer(player.Value);
							}
							catch (Exception ex)
							{
								Console.WriteLine(ex);
							}

						}
						RegionManager.WriteRegionInfo();
						Config.Save();
						Utils.SaveChestInfo();
						GroupManager.Save();
						CommandBoardcast.ConsoleMessage("\nOn Server Close: Saved all datas!");
						Logger.Dispose();
					}
				});
				CheckDisconnect.Start();
			}
		}

		public override void Load()
		{
			Instance = this;
			if (Main.dedServ)
			{
				Main.ServerSideCharacter = true;
				Console.WriteLine("[ServerSideCharacter Mod, Author: DXTsT	Version: " + ApiVersion + "]");
			}
		}

		public override void HandlePacket(BinaryReader reader, int whoAmI)
		{
			SscMessageType msgType = (SscMessageType)reader.ReadInt32();
			try
			{
				if (msgType == SscMessageType.SyncPlayerHealth)
				{
					int id = reader.ReadByte();

					if (id == Main.myPlayer && !Main.ServerSideCharacter)
					{
						return;
					}
					Player player = Main.player[id];
					player.statLife = reader.ReadInt32();
					player.statLifeMax = reader.ReadInt32();
					if (player.statLifeMax < 100)
					{
						player.statLifeMax = 100;
					}
					player.dead = player.statLife <= 0;
				}
				else if (msgType == SscMessageType.SyncPlayerMana)
				{
					int id = reader.ReadByte();
					if (Main.myPlayer == id && !Main.ServerSideCharacter)
					{
						return;
					}
					int statMana = reader.ReadInt32();
					int statManaMax = reader.ReadInt32();
					Main.player[id].statMana = statMana;
					Main.player[id].statManaMax = statManaMax;
				}
				else if (msgType == SscMessageType.SyncPlayerBank)
				{
					int id = reader.ReadByte();
					if ((id == Main.myPlayer) && !Main.ServerSideCharacter && !Main.player[id].IsStackingItems())
					{
						return;
					}
					Player player = Main.player[id];
					lock (player)
					{
						foreach (Item item in player.bank.item)
						{
							int type = reader.ReadInt32();
							int prefix = reader.ReadInt16();
							int stack = reader.ReadInt16();
							item.SetDefaults(type);
							item.Prefix(prefix);
							item.stack = stack;
						}
						foreach (Item item in player.bank2.item)
						{
							int type = reader.ReadInt32();
							int prefix = reader.ReadInt16();
							int stack = reader.ReadInt16();
							item.SetDefaults(type);
							item.Prefix(prefix);
							item.stack = stack;
						}
						foreach (Item item in player.bank3.item)
						{
							int type = reader.ReadInt32();
							int prefix = reader.ReadInt16();
							int stack = reader.ReadInt16();
							item.SetDefaults(type);
							item.Prefix(prefix);
							item.stack = stack;
						}
					}
				}
				else if (msgType == SscMessageType.RequestSaveData)
				{
					int plr = reader.ReadByte();
					Player p = Main.player[plr];
					ServerPlayer player = XmlData.Data[p.name];
					player.CopyFrom(Main.player[plr]);
					try
					{
						MainWriter.SavePlayer(player);
					}
					catch (Exception ex)
					{
						Console.WriteLine(ex);
					}

#if DEBUGMODE
					CommandBoardcast.ConsoleSavePlayer(player);
#endif
				}
				else if (msgType == SscMessageType.RequestRegister)
				{
					int plr = reader.ReadByte();
					string password = reader.ReadString();
					Player p = Main.player[plr];
					ServerPlayer player = XmlData.Data[p.name];
					if (player.HasPassword)
					{
						ChatHelper.SendChatMessageToClient(NetworkText.FromLiteral("You cannot register twice!"), new Color(255, 255, 0, 0), plr);
						return;
					}

					lock (player)
					{
						player.HasPassword = true;
						player.Password = Md5Crypto.ComputeMd5(password);

						ChatHelper.SendChatMessageToClient(NetworkText.FromLiteral(
							$"You have successfully set your password as {password}. Remember it!"), new Color(255, 50, 255, 50), plr);
					}
				}
				else if (msgType == SscMessageType.SendLoginPassword)
				{
					int plr = reader.ReadByte();
					string password = reader.ReadString();
					Player p = Main.player[plr];
					ServerPlayer player = XmlData.Data[p.name];

					if (!player.HasPassword)
					{
						ChatHelper.SendChatMessageToClient(NetworkText.FromLiteral("You should first register an account use /register <password> !"), new Color(255, 255, 0, 0), plr);
					}
					else
					{
						password = Md5Crypto.ComputeMd5(password);
						if (password.Equals(player.Password))
						{
							lock (player)
							{
								player.IsLogin = true;
							}
							ChatHelper.SendChatMessageToClient(NetworkText.FromLiteral(
								$"You have successfully logged in as {player.Name}"), new Color(255, 50, 255, 50), plr);
							ChatHelper.SendChatMessageToClient(NetworkText.FromLiteral("Please wait for a few seconds and then you can move"), new Color(255, 255, 255, 0), plr);
						}
						else
						{
							ChatHelper.SendChatMessageToClient(NetworkText.FromLiteral("Wrong password! Please try again!"), new Color(255, 255, 20, 0), plr);
						}
					}
				}
				else if (msgType == SscMessageType.KillCommand)
				{
					int plr = reader.ReadByte();
					int target = reader.ReadByte();
					Player p = Main.player[plr];
					ServerPlayer player = XmlData.Data[p.name];
					if (!player.IsLogin) return;
					if (player.PermissionGroup.HasPermission("kill"))
					{
						Main.player[target].KillMe(PlayerDeathReason.ByCustomReason(" was killed by absolute power!"),
							23333, 0);
						NetMessage.SendPlayerDeath(target, PlayerDeathReason.ByCustomReason(" was killed by absolute power!"),
							23333, 0, false);
					}
					else
					{
						ChatHelper.SendChatMessageToClient(NetworkText.FromLiteral("You don't have the permission to this command."), new Color(255, 255, 20, 0), plr);
					}
				}
				else if (msgType == SscMessageType.ListCommand)
				{
					List(reader, whoAmI);

				}
				else if (msgType == SscMessageType.RequestSetGroup)
				{
					int plr = reader.ReadByte();
					int uuid = reader.ReadInt32();
					string group = reader.ReadString();
					Player p = Main.player[plr];
					ServerPlayer player = XmlData.Data[p.name];
					if (!player.IsLogin) return;
					if (player.PermissionGroup.HasPermission("group"))
					{
						try
						{
							ServerPlayer targetPlayer = ServerPlayer.FindPlayer(uuid);
							targetPlayer.PermissionGroup = GroupManager.Groups[group];
							ChatHelper.SendChatMessageToClient(NetworkText.FromLiteral(
								$"Successfully set {targetPlayer.Name} to group '{group}'"), new Color(255, 50, 255, 50), plr);
						}
						catch
						{
							ChatHelper.SendChatMessageToClient(NetworkText.FromLiteral("Cannot find this player or group name invalid!"), new Color(255, 255, 20, 0), plr);
						}
					}
					else
					{
						ChatHelper.SendChatMessageToClient(NetworkText.FromLiteral("You don't have the permission to this command."), new Color(255, 255, 20, 0), plr);
					}
				}
				else if (msgType == SscMessageType.LockPlayer)
				{
					int plr = reader.ReadByte();
					int target = reader.ReadByte();
					int time = reader.ReadInt32();
					Player p = Main.player[plr];
					Player target0 = Main.player[target];
					ServerPlayer target1 = XmlData.Data[target0.name];
					ServerPlayer player = XmlData.Data[p.name];
					if (!player.IsLogin) return;
					if (player.PermissionGroup.HasPermission("lock"))
					{
						target1.ApplyLockBuffs(time);
						ChatHelper.SendChatMessageToClient(NetworkText.FromLiteral(
							$"You have successfully locked {target1.Name} for {time} frames"), new Color(255, 50, 255, 50), plr);
					}
					else
					{
						ChatHelper.SendChatMessageToClient(NetworkText.FromLiteral("You don't have the permission to this command."), new Color(255, 255, 20, 0), plr);
					}
				}
				else if (msgType == SscMessageType.ButcherCommand)
				{
					int plr = reader.ReadByte();
					Player p = Main.player[plr];
					ServerPlayer player = XmlData.Data[p.name];
					if (!player.IsLogin) return;
					if (player.PermissionGroup.HasPermission("butcher"))
					{
						int kills = 0;
						for (int i = 0; i < Main.npc.Length; i++)
						{
							if (Main.npc[i].active && (!Main.npc[i].townNPC && Main.npc[i].netID != NPCID.TargetDummy))
							{
								Main.npc[i].StrikeNPC(100000000, 0, 0);
								NetMessage.SendData(MessageID.StrikeNPC, -1, -1, NetworkText.Empty, i, 10000000);
								kills++;
							}
						}
						ServerPlayer.SendInfoToAll($"{player.Name} butchered {kills} NPCs.");
					}
					else
					{
						player.SendErrorInfo("You don't have the permission to this command.");
					}
				}
				else if (msgType == SscMessageType.TpCommand)
				{
					int plr = reader.ReadByte();
					int target = reader.ReadByte();
					Player p = Main.player[plr];
					ServerPlayer player = XmlData.Data[p.name];
					ServerPlayer targetPlayer = XmlData.Data[Main.player[target].name];
					if (!player.IsLogin || target == plr) return;
					if (player.PermissionGroup.HasPermission("tp"))
					{
						if (targetPlayer.PrototypePlayer != null && targetPlayer.PrototypePlayer.active)
						{
							if (targetPlayer.TpProtect)
							{
								player.SendErrorInfo("��Ҳ��������˴���");
							}
							else
							{
								p.Teleport(Main.player[target].position);
								MessageSender.SendTeleport(plr, Main.player[target].position);
								player.SendInfo("You have teleproted to " + targetPlayer.Name);
								targetPlayer.SendInfo(player.Name + " has teleproted to you!");
							}
						}
						else
						{
							player.SendErrorInfo("Cannot find this player");
						}
					}
					else
					{
						player.SendErrorInfo("You don't have the permission to this command.");
					}
				}
				else if (msgType == SscMessageType.TimeCommand)
				{
					int plr = reader.ReadByte();
					bool set = reader.ReadBoolean();
					int time = reader.ReadInt32();
					bool day = reader.ReadBoolean();
					Player p = Main.player[plr];
					ServerPlayer player = XmlData.Data[p.name];
					if (!player.IsLogin) return;
					if (player.PermissionGroup.HasPermission("time"))
					{
						if (!set)
						{
							double time1 = GetTime();
							player.SendInfo(string.Format("The current time is {0}:{1:D2}.",
								(int)Math.Floor(time1), (int)Math.Round((time1 % 1.0) * 60.0)));
						}
						else
						{
							Main.time = time;
							Main.dayTime = day;
							MessageSender.SendTimeSet(Main.time, Main.dayTime);
							double time1 = GetTime();
							player.SendInfo(string.Format("{0} set the time to {1}:{2:D2}.", player.Name,
								(int)Math.Floor(time1), (int)Math.Round((time1 % 1.0) * 60.0)));
						}
					}
					else
					{
						player.SendErrorInfo("You don't have the permission to this command.");
					}

				}
				else if (msgType == SscMessageType.SendTimeSet)
				{
					double time = reader.ReadDouble();
					bool day = reader.ReadBoolean();
					short sunY = reader.ReadInt16();
					short moonY = reader.ReadInt16();
					if (Main.netMode == 1)
					{
						Main.time = time;
						Main.dayTime = day;
						Main.sunModY = sunY;
						Main.moonModY = moonY;
					}
				}
				else if (msgType == SscMessageType.HelpCommand)
				{
					int plr = reader.ReadByte();
					StringBuilder sb = new StringBuilder();
					sb.Append("Current commands:\n");
					Player p = Main.player[plr];
					ServerPlayer player = XmlData.Data[p.name];

					foreach (var command in Commands)
					{
						if (player.PermissionGroup.HasPermission(command.Name))
						{
							sb.AppendLine("/" + command.Name + " [" + command.Description + "]  ");
						}
					}
					player.SendInfo(sb.ToString());
				}
				else if (msgType == SscMessageType.RequestItem)
				{
					int plr = reader.ReadByte();
					int type = reader.ReadInt32();
					Player p = Main.player[plr];
					ServerPlayer player = XmlData.Data[p.name];
					if (!player.IsLogin) return;
					if (player.PermissionGroup.HasPermission("item"))
					{
						Item item = new Item();
						item.netDefaults(type);
						Item.NewItem(new EntitySource_WorldGen(),p.position, Vector2.Zero, type, item.maxStack);
						player.SendInfo($"Sever has give you {item.maxStack} {Lang.GetItemNameValue(type)}");
					}
					else
					{
						player.SendErrorInfo("You don't have the permission to this command.");
					}
				}
				else if (msgType == SscMessageType.TeleportPalyer)
				{

					Vector2 dest = reader.ReadVector2();
					if (Main.netMode == 1)
					{
						Main.LocalPlayer.Teleport(dest);
					}
				}
				else if (msgType == SscMessageType.RequestAuth)
				{
					int plr = reader.ReadByte();
					string code = reader.ReadString();
					Player p = Main.player[plr];
					CommandBoardcast.ConsoleMessage(p.name + " has tried to auth with code " + code);
					if (code.Equals(AuthCode))
					{
						ServerPlayer targetPlayer = p.GetServerPlayer();
						targetPlayer.PermissionGroup = GroupManager.Groups["spadmin"];
						targetPlayer.SendSuccessInfo("You have successfully auth as SuperAdmin");
					}
				}
				else if (msgType == SscMessageType.SummonCommand)
				{
					SummonNpc(reader, whoAmI);
				}
				else if (msgType == SscMessageType.ToggleGodMode)
				{
					ToggleGodMode(reader, whoAmI);
				}
				else if (msgType == SscMessageType.SetGodMode)
				{
					Main.LocalPlayer.GetModPlayer<MPlayer>().GodMode = reader.ReadBoolean();
				}
				else if (msgType == SscMessageType.TpHereCommand)
				{
					TpHere(reader, whoAmI);
				}
				else if (msgType == SscMessageType.RegionCreateCommand)
				{
					RegionCreate(reader, whoAmI);
				}
				else if (msgType == SscMessageType.RegionRemoveCommand)
				{
					RegionRemove(reader, whoAmI);
				}
				else if (msgType == SscMessageType.ServerSideCharacter)
				{
					Main.ServerSideCharacter = true;
				}
				else if (msgType == SscMessageType.ToggleExpert)
				{
					ToggleExpert(reader, whoAmI);
				}
				else if (msgType == SscMessageType.ToggleHardMode)
				{
					ToggleHardmode(reader, whoAmI);
				}
				else if (msgType == SscMessageType.RegionShareCommand)
				{
					RegionShare(reader, whoAmI);
				}
				else if (msgType == SscMessageType.BanItemCommand)
				{
					BanItem(reader, whoAmI);
				}
				else if (msgType == SscMessageType.GenResources)
				{
					GenResources(reader, whoAmI);
				}
				else if (msgType == SscMessageType.ChestCommand)
				{
					int plr = reader.ReadByte();
					ServerPlayer player = Main.player[plr].GetServerPlayer();
					if (!player.IsLogin)
						return;
					if (!Config.EnableChestProtection)
					{
						player.SendErrorInfo("Chest protection isn't enabled on this server");
						return;
					}
					ChestManager.Pending pending = (ChestManager.Pending)reader.ReadInt32();
					ServerPlayer friend = null;
					switch (pending)
					{
						case ChestManager.Pending.AddFriend:
							friend = Main.player[reader.ReadByte()].GetServerPlayer();
							ChestManager.AddPending(player, ChestManager.Pending.AddFriend, friend);
							break;
						case ChestManager.Pending.RemoveFriend:
							friend = Main.player[reader.ReadByte()].GetServerPlayer();
							ChestManager.AddPending(player, ChestManager.Pending.RemoveFriend, friend);
							break;
						case ChestManager.Pending.Public:
							ChestManager.AddPending(player, ChestManager.Pending.Public);
							break;
						case ChestManager.Pending.UnPublic:
							ChestManager.AddPending(player, ChestManager.Pending.UnPublic);
							break;
						case ChestManager.Pending.Protect:
							ChestManager.AddPending(player, ChestManager.Pending.Protect);
							break;
						case ChestManager.Pending.DeProtect:
							ChestManager.AddPending(player, ChestManager.Pending.DeProtect);
							break;
						case ChestManager.Pending.Info:
							ChestManager.AddPending(player, ChestManager.Pending.Info);
							break;
						default:
							Console.WriteLine("[ChestCommand] Invalid argument!");
							return;
					}
					player.SendSuccessInfo("Open a chest do apply the changes");

				}
				else if (msgType == SscMessageType.TpProtect)
				{
					TpProtect(reader, whoAmI);
				}
				else
				{
					Console.WriteLine("Unexpected message type!");
				}
			}
			catch (Exception ex)
			{
				CommandBoardcast.ConsoleError(ex);
			}
		}

		private void TpProtect(BinaryReader reader, int whoAmI)
		{
			int plr = reader.ReadByte();
			ServerPlayer player = Main.player[plr].GetServerPlayer();
			player.TpProtect = !player.TpProtect;
			player.SendSuccessInfo("���ͱ�������" + (player.TpProtect ? "����" : "����"));
		}

		private void GenResources(BinaryReader reader, int whoAmI)
		{
			int plr = reader.ReadByte();
			GenerationType type = (GenerationType)reader.ReadByte();
			Player p = Main.player[plr];
			ServerPlayer player = XmlData.Data[p.name];
			if (!player.IsLogin) return;
			if (player.PermissionGroup.HasPermission("gen-res"))
			{
				switch (type)
				{
					case GenerationType.Tree:
						WorldGen.AddTrees();
						player.SendSuccessInfo("Generated trees!");
						break;
					case GenerationType.Chest:
						break;
					case GenerationType.Ore:
						break;
					case GenerationType.Trap:
						break;
				}
			}
			else
			{
				player.SendErrorInfo("You don't have the permission to access this command.");
			}
		}

		private void BanItem(BinaryReader reader, int whoAmI)
		{
			int plr = reader.ReadByte();
			int type = reader.ReadInt32();
			Player p = Main.player[plr];
			ServerPlayer player = XmlData.Data[p.name];
			if (!player.IsLogin) return;
			if (player.PermissionGroup.HasPermission("ban-item"))
			{
				Config.ToggleItemBan(type, player);
			}
			else
			{
				player.SendErrorInfo("You don't have the permission to access this command.");
			}
		}

		private void RegionShare(BinaryReader reader, int whoAmI)
		{
			int plr = reader.ReadByte();
			int target = reader.ReadByte();
			string name = reader.ReadString();
			Player p = Main.player[plr];
			ServerPlayer player = XmlData.Data[p.name];
			ServerPlayer targetplayer = XmlData.Data[Main.player[target].name];
			if (!player.IsLogin) return;
			if (player.PermissionGroup.HasPermission("region-share"))
			{
				RegionManager.ShareRegion(player, targetplayer, name);
			}
			else
			{
				player.SendErrorInfo("You don't have the permission to this command.");
			}
		}

		private void List(BinaryReader reader, int whoAmI)
		{
			int plr = reader.ReadByte();
			ListType type = (ListType)reader.ReadByte();
			bool all = reader.ReadBoolean();
			Player p = Main.player[plr];
			ServerPlayer player = XmlData.Data[p.name];
			if (!player.IsLogin) return;
			if (all && player.PermissionGroup.HasPermission("ls -al"))
			{
				try
				{
					StringBuilder sb = new StringBuilder();
					if (type == ListType.ListPlayers)
					{
						sb.AppendLine("Player ID    Name    UUID    Permission Group    LifeMax");
						foreach (var pla in XmlData.Data)
						{
							Player player1 = pla.Value.PrototypePlayer;
							string line = string.Concat(
								player1 != null && player1.active ? player1.whoAmI.ToString() : "N/A",
								"    ",
								pla.Value.Name,
								"    ",
								pla.Value.Uuid,
								"    ",
								pla.Value.PermissionGroup.GroupName,
								"    ",
								pla.Value.LifeMax,
								"    "
								);
							sb.AppendLine(line);
						}
					}
					else if (type == ListType.ListRegions)
					{
						sb.AppendLine("RegionName    Owner    Region Area");
						foreach (var region in RegionManager.GetList())
						{
							string line = string.Concat(
								region.Name,
								"    ",
								region.Owner.Name,
								"    ",
								region.Area.ToString()
								);
							sb.AppendLine(line);
						}
					}
					else if (type == ListType.ListGroups)
					{
						int i = 1;
						foreach (var group in GroupManager.Groups)
						{
							sb.AppendLine(string.Format("{0}. Group Name: {1}  Chat Prefix: {2}\n   Permissions:",
								i, group.Key, group.Value.ChatPrefix));
							sb.AppendLine("{");
							foreach (var perm in group.Value.Permissions)
							{
								sb.AppendLine("  " + perm.Name);
							}
							sb.AppendLine("}");
							i++;
						}
					}
					ChatHelper.SendChatMessageToClient(NetworkText.FromLiteral(sb.ToString()), new Color(255, 255, 255, 0), plr);
				}
				catch (Exception ex)
				{
					CommandBoardcast.ConsoleError(ex);
				}
			}
			else if (!all && player.PermissionGroup.HasPermission("ls"))
			{
				StringBuilder sb = new StringBuilder();
				if (type == ListType.ListPlayers)
				{
					sb.AppendLine("Player ID    Name    Permission Group");
					foreach (var pla in Main.player)
					{
						if (pla.active)
						{
							string line = string.Concat(
								pla.whoAmI,
								"    ",
								pla.name,
								"    ",
								pla.GetServerPlayer().PermissionGroup.GroupName
								);
							sb.AppendLine(line);
						}
					}
				}
				else if (type == ListType.ListRegions)
				{
					sb.AppendLine("Region Name    Region Area");
					foreach (var region in RegionManager.GetList())
					{
						string line = string.Concat(
							region.Name,
							"    ",
							region.Area.ToString()
							);
						sb.AppendLine(line);
					}
				}
				else if (type == ListType.ListGroups)
				{
					sb.AppendLine("Your Permissions: ");
					sb.AppendLine("{");
					foreach (var permission in player.PermissionGroup.Permissions)
					{
						sb.AppendLine("   " + permission.Name);
					}
					sb.AppendLine("}");
				}
				ChatHelper.SendChatMessageToClient(NetworkText.FromLiteral(sb.ToString()), new Color(255, 255, 255, 0), plr);
			}
			else
			{
				ChatHelper.SendChatMessageToClient(NetworkText.FromLiteral("You don't have the permission to this command."), new Color(255, 255, 20, 0), plr);
			}
		}



		private void ToggleHardmode(BinaryReader reader, int whoAmI)
		{
			int plr = reader.ReadByte();
			Player p = Main.player[plr];
			ServerPlayer player = p.GetServerPlayer();
			if (!player.IsLogin) return;
			if (player.PermissionGroup.HasPermission("hardmode"))
			{
				if (Main.hardMode)
				{
					Main.hardMode = false;
					NetMessage.SendData(MessageID.WorldData);
					ServerPlayer.SendInfoToAll("Hardmode is now off.");
				}
				else
				{
					WorldGen.StartHardmode();
					ServerPlayer.SendInfoToAll("Hardmode is now on.");
				}
			}
		}

		private void ToggleExpert(BinaryReader reader, int whoAmI)
		{
			int plr = reader.ReadByte();
			Player p = Main.player[plr];
			ServerPlayer player = p.GetServerPlayer();
			if (!player.IsLogin) return;
			if (player.PermissionGroup.HasPermission("expert"))
			{
				Main.expertMode = !Main.expertMode;
				NetMessage.SendData(MessageID.WorldData);
				ServerPlayer.SendInfoToAll("Server " + (Main.expertMode ? "now" : "no longer") + " in Expert Mode");
			}
		}

		private static void RegionRemove(BinaryReader reader, int whoAmI)
		{
			int plr = reader.ReadByte();
			string name = reader.ReadString();
			Player p = Main.player[plr];
			ServerPlayer player = p.GetServerPlayer();
			if (!player.IsLogin) return;
			if (player.PermissionGroup.HasPermission("region-remove"))
			{
				if (RegionManager.RemoveRegionWithName(name))
				{
					player.SendSuccessInfo("You have successfully removed region '" + name + "'");
				}
				else
				{
					player.SendErrorInfo("The region does not exist!");
				}
			}
		}

		private static void RegionCreate(BinaryReader reader, int whoAmI)
		{
			int plr = reader.ReadByte();
			string name = reader.ReadString();
			Vector2 p1 = reader.ReadVector2();
			Vector2 p2 = reader.ReadVector2();
			Player p = Main.player[plr];
			ServerPlayer player = p.GetServerPlayer();
			if (!player.IsLogin) return;
			if (player.PermissionGroup.HasPermission("region-create"))
			{
				int width = (int)Math.Abs(p1.X - p2.X);
				int height = (int)Math.Abs(p1.Y - p2.Y);
				Vector2 realPos = Math.Abs(p2.X - width - p1.X) < 0.01f ? p1 : p2;
				Rectangle regionArea = new Rectangle((int)realPos.X, (int)realPos.Y, width, height);
				if (RegionManager.ValidRegion(player, name, regionArea) && name.Length >= 3)
				{
					RegionManager.CreateNewRegion(regionArea, name, player);
					RegionManager.WriteRegionInfo();
					player.SendSuccessInfo("You have successfully created a region named: " + name);
				}
				else
				{
					player.SendErrorInfo("Sorry, but this name has been occupied or you have too many regions!");
				}
			}
			else
			{
				player.SendErrorInfo("You don't have the permission to this command.");
			}
		}

		private static void TpHere(BinaryReader reader, int whoAmI)
		{
			int plr = reader.ReadByte();
			int t = reader.ReadByte();
			Player p = Main.player[plr];
			Player target = Main.player[t];
			ServerPlayer player = XmlData.Data[p.name];
			ServerPlayer tar = XmlData.Data[target.name];
			if (player.PermissionGroup.HasPermission("tphere"))
			{
				MessageSender.SendTeleport(t, p.position);
				player.SendInfo("You have teleported " + tar.Name + " to your position");
				player.SendInfo("You have been forced teleport to " + player.Name);
			}
			else
			{
				player.SendErrorInfo("You don't have the permission to this command.");
			}
		}

		private void ToggleGodMode(BinaryReader reader, int whoAmI)
		{
			int plr = reader.ReadByte();
			Player p = Main.player[plr];
			ServerPlayer player = XmlData.Data[p.name];
			if (player.PermissionGroup.HasPermission("god"))
			{
				p.GetModPlayer<MPlayer>().GodMode = !p.GetModPlayer<MPlayer>().GodMode;
				ModPacket pack = GetPacket();
				pack.Write((int)SscMessageType.SetGodMode);
				pack.Write(p.GetModPlayer<MPlayer>().GodMode);
				pack.Send(plr);
				player.SendInfo("God mode is " + (p.GetModPlayer<MPlayer>().GodMode ? "actived!" : "disactived!"));
			}
			else
			{
				player.SendErrorInfo("You don't have the permission to this command.");
			}
		}

		private static void SetupDefaults()
		{
			Logger = new ErrorLogger("ServerLog.txt", false);
			//GroupType.SetupGroups();

			//��Ʒ��Ϣ��ȡ��ʽ���?
			ModDataHooks.BuildItemDataHook("prefix",
				item => item.prefix.ToString(),
				(str, item) =>
				{
					item.prefix = Convert.ToByte(str);
				});
			ModDataHooks.BuildItemDataHook("stack",
				item => item.stack.ToString(),
				(str, item) =>
				{
					item.stack = Convert.ToInt32(str);
				});
			ModDataHooks.BuildItemDataHook("fav",
				item => item.favorited.ToString(),
				(str, item) =>
				{
					item.favorited = Convert.ToBoolean(str);
				});
			if (!Directory.Exists("SSC"))
			{
				Directory.CreateDirectory("SSC");
			}
			Config = new ServerConfigManager();
			GroupManager = new GroupConfigManager();
			ChestManager = Utils.LoadChestInfo();
			if (!File.Exists("SSC/datas.xml"))
			{
				string save = Path.Combine("SSC", "datas.xml");
				XmlWriter writer = new XmlWriter(save);
				writer.Create();
				Player tmp = new Player { name = "DXTsT" };
				ServerPlayer newPlayer = ServerPlayer.CreateNewPlayer(tmp);
				writer.Write(newPlayer);
				MainWriter = writer;
				Console.WriteLine("Saved data: " + save);
			}
		}





		private static double GetTime()
		{
			double time1 = Main.time / 3600.0;
			time1 += 4.5;
			if (!Main.dayTime)
				time1 += 15.0;
			time1 = time1 % 24.0;
			return time1;
		}

		public static bool CheckSpawn(int x, int y)
		{
			Vector2 tile = new Vector2(x, y);
			Vector2 spawn = new Vector2(Main.spawnTileX, Main.spawnTileY);
			return Vector2.Distance(spawn, tile) <= 10;
		}

		public static void SummonNpc(BinaryReader reader, int whoAmI)
		{
			int plr = reader.ReadByte();
			int type = reader.ReadInt32();
			int number = reader.ReadInt32();
			Player p = Main.player[plr];
			ServerPlayer player = XmlData.Data[p.name];
			try
			{
				if (!player.IsLogin) return;
				if (player.PermissionGroup.HasPermission("sm"))
				{
					if (number > 200) number = 200;
					if (type >= 1 && type < Main.maxNPCTypes && type != 113)
					{
						for (int i = 0; i < number; i++)
						{
							int spawnTileX;
							int spawnTileY;
							GetRandomClearTileWithInRange((int)(p.Center.X) / 16, (int)(p.Center.Y) / 16, 50, 30, out spawnTileX,
																		 out spawnTileY);
							int npcid = NPC.NewNPC(new EntitySource_WorldGen(),spawnTileX * 16, spawnTileY * 16, type);
							// This is for special slimes
							Main.npc[npcid].SetDefaults(type);
						}
						ServerPlayer.SendInfoToAll(string.Format("{0} summoned {1} {2}(s)",
						player.Name, number, Lang.GetNPCNameValue(type)));
					}
					else
					{
						player.SendErrorInfo("Invalid mob type!");
					}
				}
				else
				{
					player.SendErrorInfo("You don't have the permission to this command.");
				}
			}
			catch (Exception ex)
			{
				CommandBoardcast.ConsoleError(ex);
			}
		}
		private static void GetRandomClearTileWithInRange(int startTileX, int startTileY, int tileXRange, int tileYRange,
			out int tileX, out int tileY)
		{
			int j = 0;
			do
			{
				if (j == 100)
				{
					tileX = startTileX;
					tileY = startTileY;
					break;
				}
				tileX = startTileX + Main.rand.Next(tileXRange * -1, tileXRange);
				tileY = startTileY + Main.rand.Next(tileYRange * -1, tileYRange);
				j++;
			} while (TilePlacementValid(tileX, tileY) && TileSolid(tileX, tileY));
		}

		private static bool TilePlacementValid(int tileX, int tileY)
		{
			return tileX >= 0 && tileX < Main.maxTilesX && tileY >= 0 && tileY < Main.maxTilesY;
		}

		private static bool TileSolid(int tileX, int tileY)
		{
			return TilePlacementValid(tileX, tileY) && Main.tile[tileX, tileY] != null &&
				Main.tile[tileX, tileY].active() && Main.tileSolid[Main.tile[tileX, tileY].TileType] &&
				!Main.tile[tileX, tileY].inActive() && !Main.tile[tileX, tileY].IsHalfBlock &&
				Main.tile[tileX, tileY].Slope == 0 && Main.tile[tileX, tileY].TileType != TileID.Bubble;
		}
	}
}
