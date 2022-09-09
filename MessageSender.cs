using Microsoft.Xna.Framework;
using ServerSideCharacter.Region;
using ServerSideCharacter.ServerCommand;
using Terraria;
using Terraria.ModLoader;

namespace ServerSideCharacter
{
	public class MessageSender
	{

		public static void SyncPlayerHealth(int plr, int to, int from)
		{
			string name = Main.player[plr].name;
			ServerPlayer player = ServerSideCharacter.XmlData.Data[name];
			Main.player[plr].statLife = player.StatLife;
			Main.player[plr].statLifeMax = player.LifeMax;
			ModPacket p = ServerSideCharacter.Instance.GetPacket();
			p.Write((int)SscMessageType.SyncPlayerHealth);
			p.Write((byte)plr);
			p.Write(player.StatLife);
			p.Write(player.LifeMax);
			p.Send(to, from);
		}
		public static void SyncPlayerMana(int plr, int to, int from)
		{
			string name = Main.player[plr].name;
			ServerPlayer player = ServerSideCharacter.XmlData.Data[name];
			Main.player[plr].statMana = player.StatMana;
			Main.player[plr].statManaMax = player.ManaMax;
			ModPacket p = ServerSideCharacter.Instance.GetPacket();
			p.Write((int)SscMessageType.SyncPlayerMana);
			p.Write((byte)plr);
			p.Write(player.StatMana);
			p.Write(player.ManaMax);
			p.Send(to, from);
		}
		public static void SyncPlayerBanks(int plr, int to, int from)
		{
			string name = Main.player[plr].name;
			ServerPlayer player = ServerSideCharacter.XmlData.Data[name];
			Main.player[plr].bank = (Chest)player.Bank.Clone();
			Main.player[plr].bank2 = (Chest)player.Bank2.Clone();
			Main.player[plr].bank3 = (Chest)player.Bank3.Clone();
			ModPacket p = ServerSideCharacter.Instance.GetPacket();
			p.Write((int)SscMessageType.SyncPlayerBank);
			p.Write((byte)plr);
			foreach (Item item in player.Bank.item)
			{
				p.Write(item.type);
				p.Write((short)item.prefix);
				p.Write((short)item.stack);
			}
			foreach (Item item in player.Bank2.item)
			{
				p.Write(item.type);
				p.Write((short)item.prefix);
				p.Write((short)item.stack);
			}
			foreach (Item item in player.Bank3.item)
			{
				p.Write(item.type);
				p.Write((short)item.prefix);
				p.Write((short)item.stack);
			}
			p.Send(to, from);
		}

		public static void SendTeleport(int plr, Vector2 pos)
		{
			ModPacket p = ServerSideCharacter.Instance.GetPacket();
			p.Write((int)SscMessageType.TeleportPalyer);
			p.WriteVector2(pos);
			p.Send(plr);
		}

		public static void SendRequestSave(int plr)
		{
			string name = Main.player[plr].name;
			ModPacket p = ServerSideCharacter.Instance.GetPacket();
			p.Write((int)SscMessageType.RequestSaveData);
			p.Write((byte)plr);
			p.Send();
		}

		public static void SendTimeSet(double time, bool day)
		{
			ModPacket p = ServerSideCharacter.Instance.GetPacket();
			p.Write((int)SscMessageType.SendTimeSet);
			p.Write(time);
			p.Write(day);
			p.Write(Main.sunModY);
			p.Write(Main.moonModY);
			p.Send();
		}

		public static void SendSetPassword(int plr, string password)
		{
			string name = Main.player[plr].name;
			ModPacket p = ServerSideCharacter.Instance.GetPacket();
			p.Write((int)SscMessageType.RequestRegister);
			p.Write((byte)plr);
			p.Write(password);
			p.Send();
		}

		public static void SendLoginPassword(int plr, string password)
		{
			string name = Main.player[plr].name;
			ModPacket p = ServerSideCharacter.Instance.GetPacket();
			p.Write((int)SscMessageType.SendLoginPassword);
			p.Write((byte)plr);
			p.Write(password);
			p.Send();
		}

		public static void SendKillCommand(int plr, int target)
		{
			string name = Main.player[plr].name;
			ModPacket p = ServerSideCharacter.Instance.GetPacket();
			p.Write((int)SscMessageType.KillCommand);
			p.Write((byte)plr);
			p.Write((byte)target);
			p.Send();
		}

		public static void SendTimeCommand(int plr, bool set, int time, bool day)
		{
			string name = Main.player[plr].name;
			ModPacket p = ServerSideCharacter.Instance.GetPacket();
			p.Write((int)SscMessageType.TimeCommand);
			p.Write((byte)plr);
			p.Write(set);
			p.Write(time);
			p.Write(day);
			p.Send();
		}

		public static void SendLockCommand(int plr, int target, int time)
		{
			string name = Main.player[plr].name;
			ModPacket p = ServerSideCharacter.Instance.GetPacket();
			p.Write((int)SscMessageType.LockPlayer);
			p.Write((byte)plr);
			p.Write((byte)target);
			p.Write(time);
			p.Send();
		}

		public static void SendItemCommand(int type)
		{
			ModPacket p = ServerSideCharacter.Instance.GetPacket();
			p.Write((int)SscMessageType.RequestItem);
			p.Write((byte)Main.myPlayer);
			p.Write(type);
			p.Send();
		}

		public static void SendBanItemCommand(int type)
		{
			ModPacket p = ServerSideCharacter.Instance.GetPacket();
			p.Write((int)SscMessageType.BanItemCommand);
			p.Write((byte)Main.myPlayer);
			p.Write(type);
			p.Send();
		}

		public static void SendTeleportCommand(int plr, int target)
		{
			string name = Main.player[plr].name;
			ModPacket p = ServerSideCharacter.Instance.GetPacket();
			p.Write((int)SscMessageType.TpCommand);
			p.Write((byte)plr);
			p.Write((byte)target);
			p.Send();
		}

		public static void SendListCommand(int plr, ListType type, bool all)
		{
			string name = Main.player[plr].name;
			ModPacket p = ServerSideCharacter.Instance.GetPacket();
			p.Write((int)SscMessageType.ListCommand);
			p.Write((byte)plr);
			p.Write((byte)type);
			p.Write(all);
			p.Send();
		}

		public static void SendHelpCommand(int plr)
		{
			ModPacket p = ServerSideCharacter.Instance.GetPacket();
			p.Write((int)SscMessageType.HelpCommand);
			p.Write((byte)plr);
			p.Send();
		}

		public static void SendButcherCommand(int plr)
		{
			string name = Main.player[plr].name;
			ModPacket p = ServerSideCharacter.Instance.GetPacket();
			p.Write((int)SscMessageType.ButcherCommand);
			p.Write((byte)plr);
			p.Send();
		}

		public static void SendAuthRequest(int plr, string code)
		{
			ModPacket p = ServerSideCharacter.Instance.GetPacket();
			p.Write((int)SscMessageType.RequestAuth);
			p.Write((byte)plr);
			p.Write(code);
			p.Send();
		}

		public static void SendSummonCommand(int plr, int type, int number)
		{
			ModPacket p = ServerSideCharacter.Instance.GetPacket();
			p.Write((int)SscMessageType.SummonCommand);
			p.Write((byte)plr);
			p.Write(type);
			p.Write(number);
			p.Send();
		}


		public static void SendSetGroup(int plr, int uuid, string group)
		{
			string name = Main.player[plr].name;
			ModPacket p = ServerSideCharacter.Instance.GetPacket();
			p.Write((int)SscMessageType.RequestSetGroup);
			p.Write((byte)plr);
			p.Write(uuid);
			p.Write(group);
			p.Send();
		}

		public static void SendRegionCreate(int plr, string name)
		{
			ModPacket p = ServerSideCharacter.Instance.GetPacket();
			p.Write((int)SscMessageType.RegionCreateCommand);
			p.Write((byte)plr);
			p.Write(name);
			p.WriteVector2(ServerSideCharacter.TilePos1);
			p.WriteVector2(ServerSideCharacter.TilePos2);
			p.Send();
		}

		public static void SendRegionRemove(int plr, string name)
		{
			ModPacket p = ServerSideCharacter.Instance.GetPacket();
			p.Write((int)SscMessageType.RegionRemoveCommand);
			p.Write((byte)plr);
			p.Write(name);
			p.Send();
		}

		public static void SendRegionShare(int plr, string name, int target)
		{
			ModPacket p = ServerSideCharacter.Instance.GetPacket();
			p.Write((int)SscMessageType.RegionShareCommand);
			p.Write((byte)plr);
			p.Write((byte)target);
			p.Write(name);
			p.Send();
		}

		public static void SendSsc()
		{
			ModPacket p = ServerSideCharacter.Instance.GetPacket();
			p.Write((int)SscMessageType.ServerSideCharacter);
			p.Send();
		}

		public static void SendToggleExpert()
		{
			ModPacket p = ServerSideCharacter.Instance.GetPacket();
			p.Write((int)SscMessageType.ToggleExpert);
			p.Write((byte)Main.myPlayer);
			p.Send();
		}

		public static void SendToggleHardmode()
		{
			ModPacket p = ServerSideCharacter.Instance.GetPacket();
			p.Write((int)SscMessageType.ToggleHardMode);
			p.Write((byte)Main.myPlayer);
			p.Send();
		}

		public static void SendToggleXmas()
		{
			ModPacket p = ServerSideCharacter.Instance.GetPacket();
			p.Write((int)SscMessageType.ToggleHardMode);
			p.Write((byte)Main.myPlayer);
			p.Send();
		}

		public static void SendGeneration(GenerationType type)
		{
			ModPacket p = ServerSideCharacter.Instance.GetPacket();
			p.Write((int)SscMessageType.GenResources);
			p.Write((byte)Main.myPlayer);
			p.Write((byte)type);
			p.Send();
		}

		public static void SendTpProtect(int plr)
		{
			ModPacket p = ServerSideCharacter.Instance.GetPacket();
			p.Write((int)SscMessageType.TpProtect);
			p.Write((byte)Main.myPlayer);
			p.Send();
		}

		public static void SendChestCommand(ChestManager.Pending pending, int plr, string friendName = null)
		{
			ModPacket pack = ServerSideCharacter.Instance.GetPacket();
			pack.Write((int)SscMessageType.ChestCommand);
			pack.Write((byte)plr);
			pack.Write((int)pending);
			if (pending.HasFlag(ChestManager.Pending.AddFriend) || pending.HasFlag(ChestManager.Pending.RemoveFriend))
			{
				Player friend = Utils.TryGetPlayer(friendName);
				if (friend == null || !friend.active)
				{
					Main.NewText("Player not found", Color.Red);
					return;
				}
				if (friend.whoAmI == plr)
				{
					Main.NewText("You cannot add yourself as a friend", Color.Red);
					return;
				}
				pack.Write((byte)friend.whoAmI);

			}
			pack.Send();
		}
	}
}
