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
using Terraria.ID;
using Terraria.IO;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ServerSideCharacter
{
	[SuppressMessage("ReSharper", "InvertIf")]
	public class ServerSideCharacter : Mod
	{
		public static ServerSideCharacter Instance { get; private set; }

		public static PlayerData XmlData { get; private set; }

		public static XmlWriter MainWriter { get; set; }

		public static Thread CheckDisconnect { get; private set; }

		public static string ApiVersion { get; } = "V0.3";

		public static List<Command> Commands { get; } = new List<Command>();

		public static RegionManager RegionManager { get; } = new RegionManager();

		public static ErrorLogger Logger { get; private set; }

		public static string AuthCode = "";

		public static Vector2 TilePos1 = new Vector2();

		public static Vector2 TilePos2 = new Vector2();

		public static ServerConfigManager Config { get; private set; }

		public static MessageChecker MessageChecker { get; private set; }

		public static ChestManager ChestManager { get; private set; }

		public static GroupConfigManager GroupManager { get; private set; }

		public ServerSideCharacter()
		{
			/*Properties = new ModProperties
			{
				Autoload = true,
				AutoloadSounds = true,
				AutoloadGores = true
			};*/

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
				num = 1;
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
					NetMessage.SendData(MessageID.SyncEquipment, -1, -1, NetworkText.FromLiteral(Main.player[plr].inventory[i].Name), plr, i, Main.player[plr].inventory[i].prefix);
				
				for (int j = 0; j < Main.player[plr].armor.Length; j++)
					NetMessage.SendData(MessageID.SyncEquipment, -1, -1, NetworkText.FromLiteral(Main.player[plr].armor[j].Name), plr, (59 + j), Main.player[plr].armor[j].prefix);
				
				for (int k = 0; k < Main.player[plr].dye.Length; k++)
					NetMessage.SendData(MessageID.SyncEquipment, -1, -1, NetworkText.FromLiteral(Main.player[plr].dye[k].Name), plr, (58 + Main.player[plr].armor.Length + 1 + k), Main.player[plr].dye[k].prefix);
				
				for (int l = 0; l < Main.player[plr].miscEquips.Length; l++)
					NetMessage.SendData(MessageID.SyncEquipment, -1, -1, NetworkText.Empty, plr, 58 + Main.player[plr].armor.Length + Main.player[plr].dye.Length + 1 + l, Main.player[plr].miscEquips[l].prefix);
				
				for (int m = 0; m < Main.player[plr].miscDyes.Length; 
					NetMessage.SendData(MessageID.SyncEquipment, -1, -1, NetworkText.Empty, plr, 58 + Main.player[plr].armor.Length + Main.player[plr].dye.Length + Main.player[plr].miscEquips.Length + 1 + m, Main.player[plr].miscDyes[m].prefix));
				
				NetMessage.SendData(MessageID.SyncEquipment, -1, -1, NetworkText.FromLiteral(Main.player[plr].trashItem.Name),
					plr, 58 + Main.player[plr].armor.Length + Main.player[plr].dye.Length +
					Main.player[plr].miscEquips.Length + 7, Main.player[plr].trashItem.prefix);
				MessageSender.SyncPlayerBanks(plr, -1, -1);
				PlayerLoader.SyncPlayer(Main.player[plr], toWho, fromWho, false);
				if (!Netplay.Clients[plr].IsAnnouncementCompleted)
				{
					Netplay.Clients[plr].IsAnnouncementCompleted = true;
					ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(Main.player[plr].name + " joined the Game. Welcome!"), new Color(255, 255, 240, 20), plr);
					if (Main.dedServ)
						Console.WriteLine(Main.player[plr].name + " joined the Game. Welcome!");
				}
			}
			else
			{
				num = 0;
				NetMessage.SendData(MessageID.PlayerActive, -1, plr, NetworkText.Empty, plr, num);
				if (Netplay.Clients[plr].IsAnnouncementCompleted)
				{
					Netplay.Clients[plr].IsAnnouncementCompleted = false;
					ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(Netplay.Clients[plr].Name + " left the Game!"), new Color(255, 255, 240, 20), plr);
					if (Main.dedServ)
						Console.WriteLine(Netplay.Clients[plr].Name + " left the Game!");
					Netplay.Clients[plr].Name = "Anonymous";
				}
			}
		}

		private static void SendNpcHousesAndTravelShop(int plr)
		{
			bool flag = false;
			for (int i = 0; i < 200; i++)
			{
				if (Main.npc[i].active && Main.npc[i].townNPC && NPC.TypeToDefaultHeadIndex(Main.npc[i].type) != -1)
				{
					if (!flag && Main.npc[i].type == 368)
						flag = true;
					int num = 0;
					if (Main.npc[i].homeless)
						num = 1;
					NetMessage.SendData(MessageID.NPCHome, plr, -1, NetworkText.Empty, i, Main.npc[i].homeTileX, Main.npc[i].homeTileY, num);
				}
			}
			if (flag)
				NetMessage.SendTravelShop(plr);
		}

		public static void SendAnglerQuest(int remoteClient)
		{
			if (Main.netMode != 2)
				return;
			if (remoteClient == -1)
			{
				for (int i = 0; i < 255; i++)
				{
					if (Netplay.Clients[i].State == 10)
						NetMessage.SendData(MessageID.AnglerQuest, i, -1, NetworkText.FromLiteral(Main.player[i].name), Main.anglerQuest);
				}
				return;
			}
			if (Netplay.Clients[remoteClient].State == 10)
				NetMessage.SendData(MessageID.AnglerQuest, remoteClient, -1, NetworkText.FromLiteral(Main.player[remoteClient].name), Main.anglerQuest);
		}

		private static void EnsureLocalPlayerIsPresent()
		{
			if (!Main.autoShutdown)
				return;
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
						sw.WriteLine(authcode);
				}
				else
				{
					using (StreamReader sr = new StreamReader("SSC/authcode"))
						AuthCode = sr.ReadLine();
				}
				
				XmlData = new PlayerData("SSC/datas.xml");
				RegionManager.ReadRegionInfo();
				CommandBroadcast.ConsoleMessage("Data loaded!");
				CommandBroadcast.ConsoleMessage("You can type /auth " + AuthCode + " to become super admin");

				CheckDisconnect = new Thread(() =>
				{
					while (!Netplay.Disconnect)
						Thread.Sleep(100);
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
						CommandBroadcast.ConsoleMessage("\nOn Server Close: Saved all datas!");
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
				switch (msgType)
				{
					case SscMessageType.SyncPlayerHealth:
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
							player.statLifeMax = 100;
						player.dead = player.statLife <= 0;
						break;
					}
					case SscMessageType.SyncPlayerMana:
					{
						int id = reader.ReadByte();
						if (Main.myPlayer == id && !Main.ServerSideCharacter)
							return;
						int statMana = reader.ReadInt32();
						int statManaMax = reader.ReadInt32();
						Main.player[id].statMana = statMana;
						Main.player[id].statManaMax = statManaMax;
						break;
					}
					case SscMessageType.SyncPlayerBank:
					{
						int id = reader.ReadByte();
						if ((id == Main.myPlayer) && !Main.ServerSideCharacter && !Main.player[id].IsStackingItems())
							return;
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

						break;
					}
					case SscMessageType.RequestSaveData:
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
						CommandBroadcast.ConsoleSavePlayer(player);
#endif
						break;
					}
					case SscMessageType.RequestRegister:
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

						break;
					}
					case SscMessageType.SendLoginPassword:
					{
						int plr = reader.ReadByte();
						string password = reader.ReadString();
						Player p = Main.player[plr];
						ServerPlayer player = XmlData.Data[p.name];

						if (!player.HasPassword)
							ChatHelper.SendChatMessageToClient(NetworkText.FromLiteral("You should first register an account use /register <password> !"), new Color(255, 255, 0, 0), plr);
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

						break;
					}
					case SscMessageType.KillCommand:
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
							ChatHelper.SendChatMessageToClient(NetworkText.FromLiteral("You don't have the permission to this command."), new Color(255, 255, 20, 0), plr);
						
						break;
					}
					case SscMessageType.ListCommand:
						List(reader, whoAmI);
						break;
					case SscMessageType.RequestSetGroup:
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
							ChatHelper.SendChatMessageToClient(NetworkText.FromLiteral("You don't have the permission to this command."), new Color(255, 255, 20, 0), plr);
						break;
					}
					case SscMessageType.LockPlayer:
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
							ChatHelper.SendChatMessageToClient(NetworkText.FromLiteral("You don't have the permission to this command."), new Color(255, 255, 20, 0), plr);
						break;
					}
					case SscMessageType.ButcherCommand:
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
							player.SendErrorInfo("You don't have the permission to this command.");

						break;
					}
					case SscMessageType.TpCommand:
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
									player.SendErrorInfo("��Ҳ��������˴���");
								else
								{
									p.Teleport(Main.player[target].position);
									MessageSender.SendTeleport(plr, Main.player[target].position);
									player.SendInfo("You have teleproted to " + targetPlayer.Name);
									targetPlayer.SendInfo(player.Name + " has teleproted to you!");
								}
							}
							else
								player.SendErrorInfo("Cannot find this player");
						}
						else
							player.SendErrorInfo("You don't have the permission to this command.");

						break;
					}
					case SscMessageType.TimeCommand:
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
							player.SendErrorInfo("You don't have the permission to this command.");

						break;
					}
					case SscMessageType.SendTimeSet:
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

						break;
					}
					case SscMessageType.HelpCommand:
					{
						int plr = reader.ReadByte();
						StringBuilder sb = new StringBuilder();
						sb.Append("Current commands:\n");
						Player p = Main.player[plr];
						ServerPlayer player = XmlData.Data[p.name];

						foreach (var command in Commands)
						{
							if (player.PermissionGroup.HasPermission(command.Name))
								sb.AppendLine("/" + command.Name + " [" + command.Description + "]  ");
						}
						player.SendInfo(sb.ToString());
						break;
					}
					case SscMessageType.RequestItem:
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
							player.SendErrorInfo("You don't have the permission to this command.");

						break;
					}
					case SscMessageType.TeleportPalyer:
					{
						Vector2 dest = reader.ReadVector2();
						if (Main.netMode == 1)
							Main.LocalPlayer.Teleport(dest);
						break;
					}
					case SscMessageType.RequestAuth:
					{
						int plr = reader.ReadByte();
						string code = reader.ReadString();
						Player p = Main.player[plr];
						CommandBroadcast.ConsoleMessage(p.name + " has tried to auth with code " + code);
						if (code.Equals(AuthCode))
						{
							ServerPlayer targetPlayer = p.GetServerPlayer();
							targetPlayer.PermissionGroup = GroupManager.Groups["spadmin"];
							targetPlayer.SendSuccessInfo("You have successfully auth as SuperAdmin");
						}

						break;
					}
					case SscMessageType.SummonCommand:
						SummonNpc(reader);
						break;
					case SscMessageType.ToggleGodMode:
						ToggleGodMode(reader, whoAmI);
						break;
					case SscMessageType.SetGodMode:
						Main.LocalPlayer.GetModPlayer<MPlayer>().GodMode = reader.ReadBoolean();
						break;
					case SscMessageType.TpHereCommand:
						TpHere(reader, whoAmI);
						break;
					case SscMessageType.RegionCreateCommand:
						RegionCreate(reader, whoAmI);
						break;
					case SscMessageType.RegionRemoveCommand:
						RegionRemove(reader, whoAmI);
						break;
					case SscMessageType.ServerSideCharacter:
						Main.ServerSideCharacter = true;
						break;
					case SscMessageType.ToggleExpert:
						ToggleExpert(reader, whoAmI);
						break;
					case SscMessageType.ToggleHardMode:
						ToggleHardmode(reader, whoAmI);
						break;
					case SscMessageType.RegionShareCommand:
						RegionShare(reader);
						break;
					case SscMessageType.BanItemCommand:
						BanItem(reader);
						break;
					case SscMessageType.GenResources:
						GenResources(reader);
						break;
					case SscMessageType.ChestCommand:
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
						ServerPlayer friend;
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
						break;
					}
					case SscMessageType.TpProtect:
						TpProtect(reader);
						break;
					default:
						Console.WriteLine("Unexpected message type!");
						break;
				}
			}
			catch (Exception ex)
			{
				CommandBroadcast.ConsoleError(ex);
			}
		}

		private void TpProtect(BinaryReader reader)
		{
			int plr = reader.ReadByte();
			ServerPlayer player = Main.player[plr].GetServerPlayer();
			player.TpProtect = !player.TpProtect;
			player.SendSuccessInfo("���ͱ�������" + (player.TpProtect ? "����" : "����"));
		}

		private void GenResources(BinaryReader reader)
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

		private void BanItem(BinaryReader reader)
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

		private void RegionShare(BinaryReader reader)
		{
			int plr = reader.ReadByte();
			int target = reader.ReadByte();
			string name = reader.ReadString();
			Player p = Main.player[plr];
			ServerPlayer player = XmlData.Data[p.name];
			ServerPlayer targetplayer = XmlData.Data[Main.player[target].name];
			if (!player.IsLogin) return;
			if (player.PermissionGroup.HasPermission("region-share"))
				RegionManager.ShareRegion(player, targetplayer, name);
			else
				player.SendErrorInfo("You don't have the permission to this command.");
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
					CommandBroadcast.ConsoleError(ex);
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
				ChatHelper.SendChatMessageToClient(NetworkText.FromLiteral("You don't have the permission to this command."), new Color(255, 255, 20, 0), plr);
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
				Main.GameMode = Main.GameMode == 3 ? 2 : 3;
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
					player.SendSuccessInfo("You have successfully removed region '" + name + "'");
				else
					player.SendErrorInfo("The region does not exist!");
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
					player.SendErrorInfo("Sorry, but this name has been occupied or you have too many regions!");
			}
			else
				player.SendErrorInfo("You don't have the permission to this command.");
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
				player.SendErrorInfo("You don't have the permission to this command.");
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
				player.SendErrorInfo("You don't have the permission to this command.");
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
			time1 %= 24.0;
			return time1;
		}

		public static bool CheckSpawn(int x, int y)
		{
			Vector2 tile = new Vector2(x, y);
			Vector2 spawn = new Vector2(Main.spawnTileX, Main.spawnTileY);
			return Vector2.Distance(spawn, tile) <= 10;
		}

		public static void SummonNpc(BinaryReader reader)
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
							GetRandomClearTileWithInRange((int)(p.Center.X) / 16, (int)(p.Center.Y) / 16, 50, 30, out var spawnTileX,
																		 out var spawnTileY);
							int npcid = NPC.NewNPC(new EntitySource_WorldGen(),spawnTileX * 16, spawnTileY * 16, type);
							// This is for special slimes
							Main.npc[npcid].SetDefaults(type);
						}
						ServerPlayer.SendInfoToAll(string.Format("{0} summoned {1} {2}(s)",
						player.Name, number, Lang.GetNPCNameValue(type)));
					}
					else
						player.SendErrorInfo("Invalid mob type!");
					
				}
				else
					player.SendErrorInfo("You don't have the permission to this command.");
				
			}
			catch (Exception ex)
			{
				CommandBroadcast.ConsoleError(ex);
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
			} 
			while (TilePlacementValid(tileX, tileY) && TileSolid(tileX, tileY));
		}

		private static bool TilePlacementValid(int tileX, int tileY)
		{
			return tileX >= 0 && tileX < Main.maxTilesX && tileY >= 0 && tileY < Main.maxTilesY;
		}

		private static bool TileSolid(int tileX, int tileY)
		{
			return TilePlacementValid(tileX, tileY) && Main.tile[tileX, tileY] != null &&
				Main.tile[tileX, tileY].HasTile && Main.tileSolid[Main.tile[tileX, tileY].TileType] &&
				!Main.tile[tileX, tileY].IsActuated && !Main.tile[tileX, tileY].IsHalfBlock &&
				Main.tile[tileX, tileY].Slope == 0 && Main.tile[tileX, tileY].TileType != TileID.Bubble;
		}
	}
}
