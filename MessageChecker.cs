using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using ServerSideCharacter.GroupManage;
using ServerSideCharacter.Region;
using ServerSideCharacter.ServerCommand;
using Terraria;
using Terraria.Chat;
using Terraria.GameContent.NetModules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.Net;

namespace ServerSideCharacter
{
	public delegate bool MessagePatchDelegate(ref BinaryReader reader, int playerNumber);

	public class MessageChecker
	{
		private Dictionary<int, MessagePatchDelegate> _method;

		public bool CheckMessage(ref byte messageType, ref BinaryReader reader, int playerNumber)
		{
			try
			{
				MessagePatchDelegate mpd;
				if (_method.TryGetValue(messageType, out mpd))
				{
					return mpd(ref reader, playerNumber);
				}
			}
			catch (Exception ex)
			{
				CommandBoardcast.ConsoleError(ex);
			}

			return false;
		}

		public MessageChecker()
		{
			_method = new Dictionary<int, MessagePatchDelegate>
			{
				{ MessageID.SpawnPlayer, PlayerSpawn },
				{ MessageID.ChatText, ChatText },
				{ MessageID.NetModules, HandleNetModules },
				{ MessageID.TileChange, TileChange },
				{ MessageID.PlayerControls, PlayerControls },
				{ MessageID.RequestChestOpen, RequestChestOpen }
			};
		}

		private bool HandleNetModules(ref BinaryReader reader, int playernumber)
		{
			var moduleId = reader.ReadUInt16();
			//LoadNetModule is now used for sending chat text.
			//Read the module ID to determine if this is in fact the text module
			if (Main.netMode == 2)
			{
				if (moduleId == NetManager.Instance.GetId<NetTextModule>())
				{
					//Then deserialize the message from the reader
					var msg = ChatMessage.Deserialize(reader);

					return msg.Text.StartsWith("/", StringComparison.Ordinal);
				}
			}

			return false;
		}

		private bool RequestChestOpen(ref BinaryReader reader, int playerNumber)
		{
			if (!ServerSideCharacter.Config.EnableChestProtection)
				return false;
			if (Main.netMode == 2)
			{
				int x = reader.ReadInt16();
				int y = reader.ReadInt16();
				int id = Chest.FindChest(x, y);
				Player player = Main.player[playerNumber];
				ServerPlayer sPlayer = player.GetServerPlayer();
				ChestManager.Pending pending = ServerSideCharacter.ChestManager.GetPendings(sPlayer);
				switch (pending)
				{
					case ChestManager.Pending.AddFriend:
						if (ServerSideCharacter.ChestManager.IsOwner(id, sPlayer))
						{
							ServerPlayer friend = ServerSideCharacter.ChestManager.GetFriendP(sPlayer);
							ServerSideCharacter.ChestManager.AddFriend(id, friend);
							sPlayer.SendSuccessInfo($"{friend.Name} can open this chest now");
						}
						else
							sPlayer.SendErrorInfo("You are not the owner of this chest");
						break;
					case ChestManager.Pending.RemoveFriend:
						if (ServerSideCharacter.ChestManager.IsOwner(id, sPlayer))
						{
							ServerPlayer friend = ServerSideCharacter.ChestManager.GetFriendP(sPlayer);
							ServerSideCharacter.ChestManager.RemoveFriend(id, friend);
							sPlayer.SendSuccessInfo($"{friend.Name} can't open this chest now");
						}
						else
							sPlayer.SendErrorInfo("You are not the owner of this chest");
						break;
					case ChestManager.Pending.Public:
						if (ServerSideCharacter.ChestManager.IsOwner(id, sPlayer))
						{
							if (!ServerSideCharacter.ChestManager.IsPublic(id))
							{
								ServerSideCharacter.ChestManager.SetOwner(id, sPlayer.Uuid, true);
								sPlayer.SendSuccessInfo("This chest is now Public");
							}
							else
								sPlayer.SendErrorInfo("This chest is already public");

						}
						else
							sPlayer.SendErrorInfo("You are not the owner of this chest");
						break;
					case ChestManager.Pending.UnPublic:
						if (ServerSideCharacter.ChestManager.IsOwner(id, sPlayer))
						{
							if (ServerSideCharacter.ChestManager.IsPublic(id))
							{
								ServerSideCharacter.ChestManager.SetOwner(id, sPlayer.Uuid, false);
								sPlayer.SendSuccessInfo("This chest is not Public anymore");
							}
							else
								sPlayer.SendErrorInfo("This chest is not public");
						}
						else
							sPlayer.SendErrorInfo("You are not the owner of this chest");
						break;
					case ChestManager.Pending.Protect:
						if (ServerSideCharacter.ChestManager.IsNull(id))
						{
							ServerSideCharacter.ChestManager.SetOwner(id, sPlayer.Uuid, false);
							sPlayer.SendSuccessInfo("You now own this chest");
						}
						else if (ServerSideCharacter.ChestManager.IsOwner(id, sPlayer))
							sPlayer.SendErrorInfo("You already protected this chest");
						else
							sPlayer.SendErrorInfo("This chest as already been protected by other player");
						break;
					case ChestManager.Pending.DeProtect:
						if (ServerSideCharacter.ChestManager.IsOwner(id, sPlayer))
						{
							ServerSideCharacter.ChestManager.SetOwner(id, -1, false);
							sPlayer.SendSuccessInfo("This chest is no longer yours");
						}
						else if (ServerSideCharacter.ChestManager.IsNull(id))
							sPlayer.SendErrorInfo("This chest don't have a owner");
						else
							sPlayer.SendErrorInfo("You are not the owner of this chest");
						break;
					case ChestManager.Pending.Info:
						if (ServerSideCharacter.ChestManager.IsOwner(id, sPlayer))
						{
							ChestInfo chest = ServerSideCharacter.ChestManager.ChestInfo[id];
							StringBuilder info = new StringBuilder();
							if (sPlayer.PermissionGroup.HasPermission("chest"))
								info.AppendLine($"Owner: {ServerPlayer.FindPlayer(chest.OwnerId).Name}"); //For Admins
							info.AppendLine($"Public Chest: {chest.IsPublic.ToString().ToUpper()}");
							info.AppendLine($"Friends ({chest.Friends.Count.ToString()}): {string.Join(", ", chest.Friends.ToArray().Take(10).Select(uuid => ServerPlayer.FindPlayer(uuid).Name))}");
							sPlayer.SendInfo(info.ToString());
						}
						else if (ServerSideCharacter.ChestManager.IsNull(id))
							sPlayer.SendErrorInfo("This chest don't have a owner");
						else
							sPlayer.SendErrorInfo("You are not the owner of this chest");
						break;
					default:
						if (ServerSideCharacter.ChestManager.IsNull(id))
						{
							if (ServerSideCharacter.Config.AutoProtectChests)
							{
								ServerSideCharacter.ChestManager.SetOwner(id, sPlayer.Uuid, false);
								sPlayer.SendSuccessInfo("You now own this chest");
							}
							else
								sPlayer.SendErrorInfo("Use '/chest protect' to become the owner of this chest");
							return false;
						}

						if (ServerSideCharacter.ChestManager.CanOpen(id, sPlayer))
						{
							return false;
						}

						sPlayer.SendErrorInfo("You cannot open this chest");
						break;
				}
				ServerSideCharacter.ChestManager.RemovePending(sPlayer, pending);

			}
			return true;
		}

		private bool PlayerControls(ref BinaryReader reader, int playerNumber)
		{
			if (Main.netMode == 2)
			{
				byte plr = reader.ReadByte();
				BitsByte control = reader.ReadByte();
				BitsByte pulley = reader.ReadByte();
				byte item = reader.ReadByte();
				var pos = reader.ReadVector2();
				Player player = Main.player[playerNumber];
				ServerPlayer sPlayer = player.GetServerPlayer();
				if (pulley[2])
				{
					var vel = reader.ReadVector2();
				}
				if (ServerSideCharacter.Config.IsItemBanned(sPlayer.PrototypePlayer.inventory[item], sPlayer))
				{
					sPlayer.ApplyLockBuffs();
					sPlayer.SendErrorInfo("You used a banned item: " + player.inventory[item].Name);
				}
			}
			return false;
		}

		private bool TileChange(ref BinaryReader reader, int playerNumber)
		{
			if (Main.netMode == 2)
			{
				try
				{
					Player p = Main.player[playerNumber];
					ServerPlayer player = p.GetServerPlayer();
					int action = reader.ReadByte();
					short x = reader.ReadInt16();
					short y = reader.ReadInt16();
					short type = reader.ReadInt16();
					int style = reader.ReadByte();
					if (ServerSideCharacter.CheckSpawn(x, y) && player.PermissionGroup.GroupName != "spadmin")
					{
						player.SendErrorInfo("Warning: Spawn is protected from change");
						NetMessage.SendTileSquare(-1, x, y, 4);
						return true;
					}

					if (ServerSideCharacter.RegionManager.CheckRegion(x, y, player))
					{
						player.SendErrorInfo("Warning: You don't have permission to change this tile");
						NetMessage.SendTileSquare(-1, x, y, 4);
						return true;
					}

					if (player.PermissionGroup.GroupName == "criminal")
					{
						player.SendErrorInfo("Warning: Criminals cannot change tiles");
						NetMessage.SendTileSquare(-1, x, y, 4);
						return true;
					}
				}
				catch (Exception ex)
				{
					CommandBoardcast.ConsoleError(ex);
				}
			}
			return false;
		}

		private bool ChatText(ref BinaryReader reader, int playerNumber)
		{
			int playerId = reader.ReadByte();
			if (Main.netMode == 2)
			{
				playerId = playerNumber;
			}
			Color c = reader.ReadRGB();
			if (Main.netMode == 2)
			{
				c = new Color(255, 255, 255);
			}
			string text = reader.ReadString();
			if (Main.netMode == 1)
			{
				string text2 = text.Substring(text.IndexOf('>') + 1);
				if (playerId < 255)
				{
					//TODO; find chat length
					Main.player[playerId].chatOverhead.NewMessage(text2, 2 / 2);
				}
				Main.NewTextMultiline(text, false, c);
			}
			else
			{
				Player p = Main.player[playerId];
				ServerPlayer player = p.GetServerPlayer();
				Group group = player.PermissionGroup;
				string prefix = "[" + group.ChatPrefix + "] ";
				c = group.ChatColor;
				NetMessage.SendData(25, -1, -1, NetworkText.FromLiteral(prefix + "<" + p.name + "> " + text), playerId, c.R, c.G, c.B);
				if (Main.dedServ)
				{
					Console.WriteLine("{0}<" + Main.player[playerId].name + "> " + text, prefix);
				}
			}
			return true;
		}

		private bool PlayerSpawn(ref BinaryReader reader, int playerNumber)
		{
			int id = reader.ReadByte();
			if (Main.netMode == 2)
			{
				id = playerNumber;
			}
			Player player = Main.player[id];
			player.SpawnX = reader.ReadInt16();
			player.SpawnY = reader.ReadInt16();
			player.Spawn(PlayerSpawnContext.SpawningIntoWorld);
			if (id == Main.myPlayer && Main.netMode != 2)
			{
				Main.ActivePlayerFileData.StartPlayTimer();
				Player.Hooks.EnterWorld(Main.myPlayer);
			}
			if (Main.netMode != 2 || Netplay.Clients[playerNumber].State < 3)
			{
				return true;
			}
			//如果数据中没有玩家的信息
			if (!ServerSideCharacter.XmlData.Data.ContainsKey(Main.player[playerNumber].name))
			{
				try
				{
					//创建新的玩家数据
					ServerPlayer serverPlayer = ServerPlayer.CreateNewPlayer(Main.player[playerNumber]);
					serverPlayer.PrototypePlayer = Main.player[playerNumber];
					ServerSideCharacter.XmlData.Data.Add(Main.player[playerNumber].name, serverPlayer);
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex);
				}
			}
			if (Netplay.Clients[playerNumber].State == 3)
			{
				Netplay.Clients[playerNumber].State = 10;
				NetMessage.greetPlayer(playerNumber);
				NetMessage.buffer[playerNumber].broadcast = true;
				ServerSideCharacter.SyncConnectedPlayer(playerNumber);
				NetMessage.SendData(MessageID.SpawnPlayer, -1, playerNumber, NetworkText.Empty, playerNumber);
				NetMessage.SendData(MessageID.AnglerQuest, playerNumber, -1, NetworkText.FromLiteral(Main.player[playerNumber].name), Main.anglerQuest);
				return true;
			}
			NetMessage.SendData(MessageID.SpawnPlayer, -1, playerNumber, NetworkText.Empty, playerNumber);
			return true;
		}
	}
}
