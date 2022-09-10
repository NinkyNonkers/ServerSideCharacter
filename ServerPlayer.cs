using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using ServerSideCharacter.GroupManage;
using ServerSideCharacter.Region;
using Terraria;
using Terraria.Chat;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ServerSideCharacter
{
	public class ServerPlayer
	{
		private static int _nextId;

		public bool HasPassword { get; set; }

		public bool IsLogin { get; set; }

		public bool TpProtect { get; set; }

		public string Name { get; set; }

		public string Password { get; set; }

		public int Uuid { get; set; }

		public Group PermissionGroup { get; set; }

		public int LifeMax { get; set; }

		public int StatLife { get; set; }

		public int ManaMax { get; set; }

		public int StatMana { get; set; }

		public Item[] Inventory = new Item[59];

		public Item[] Armor = new Item[20];

		public Item[] Dye = new Item[10];

		public Item[] MiscEquips = new Item[5];

		public Item[] MiscDye = new Item[5];

		public Chest Bank = new Chest(true);

		public Chest Bank2 = new Chest(true);

		public Chest Bank3 = new Chest(true);

		public Player PrototypePlayer { get; set; }

		public RegionInfo EnteredRegion { get; set; }

		public List<RegionInfo> Ownedregion = new List<RegionInfo>();


		private void SetupPlayer()
		{
			for (int i = 0; i < Inventory.Length; i++)
				Inventory[i] = new Item();
			
			for (int i = 0; i < Armor.Length; i++)
				Armor[i] = new Item();
			
			for (int i = 0; i < Dye.Length; i++)
				Dye[i] = new Item();
			
			for (int i = 0; i < MiscEquips.Length; i++)
				MiscEquips[i] = new Item();
			
			for (int i = 0; i < MiscDye.Length; i++)
				MiscDye[i] = new Item();
			
			for (int i = 0; i < Bank.item.Length; i++)
				Bank.item[i] = new Item();
			
			for (int i = 0; i < Bank2.item.Length; i++)
				Bank2.item[i] = new Item();
			
			for (int i = 0; i < Bank3.item.Length; i++)
				Bank3.item[i] = new Item();
		}
		
		public ServerPlayer()
		{
			SetupPlayer();
		}

		public ServerPlayer(Player player)
		{
			SetupPlayer();
			PrototypePlayer = player;
		}

		public void CopyFrom(Player player)
		{
			LifeMax = player.statLifeMax;
			StatLife = player.statLife;
			StatMana = player.statMana;
			ManaMax = player.statManaMax;
			player.inventory.CopyTo(Inventory, 0);
			player.armor.CopyTo(Armor, 0);
			player.dye.CopyTo(Dye, 0);
			player.miscEquips.CopyTo(MiscEquips, 0);
			player.miscDyes.CopyTo(MiscDye, 0);
			Bank = (Chest)player.bank.Clone();
			Bank2 = (Chest)player.bank2.Clone();
			Bank3 = (Chest)player.bank3.Clone();
		}


		public void ApplyLockBuffs(int time = 180)
		{
			ServerSideCharacter.Instance.TryFind("Locked", out ModBuff buff);
			PrototypePlayer.AddBuff(buff.Type, time * 2, false);
			PrototypePlayer.AddBuff(BuffID.Frozen, time, false);
			NetMessage.SendData(MessageID.AddPlayerBuff, PrototypePlayer.whoAmI, -1,
				NetworkText.Empty, PrototypePlayer.whoAmI,
				buff.Type, time * 2);
			NetMessage.SendData(MessageID.AddPlayerBuff, PrototypePlayer.whoAmI, -1,
				NetworkText.Empty, PrototypePlayer.whoAmI,
				BuffID.Frozen, time);
		}

		public void SendSuccessInfo(string msg)
		{
			ChatHelper.SendChatMessageToClient(NetworkText.FromLiteral(msg), new Color(255, 50, 255, 50), PrototypePlayer.whoAmI);
		}
		public void SendInfo(string msg)
		{
			ChatHelper.SendChatMessageToClient(NetworkText.FromLiteral(msg), new Color(255, 255, 255, 0), PrototypePlayer.whoAmI);
		}
		public void SendErrorInfo(string msg)
		{
			ChatHelper.SendChatMessageToClient(NetworkText.FromLiteral(msg), new Color(255, 20, 20, 0), PrototypePlayer.whoAmI);
		}

		//      public static string GenHashCode(string name)
		//{
		//	long hash = name.GetHashCode();
		//	hash += DateTime.Now.ToLongTimeString().GetHashCode() * 233;
		//	short res = (short)(hash % 65536);
		//	return Convert.ToString(res, 16);
		//}

		public static ServerPlayer CreateNewPlayer(Player p)
		{
			ServerPlayer player = new ServerPlayer(p);
			int i = 0;
			foreach (var item in ServerSideCharacter.Config.StartupItems)
			{
				player.Inventory[i++] = Utils.GetItemFromNet(item);
			}
			player.Name = p.name;
			player.Uuid = GetNextId();
			player.HasPassword = false;
			player.PermissionGroup = ServerSideCharacter.GroupManager.Groups["default"];
			player.IsLogin = false;
			player.TpProtect = true;
			player.Password = "";
			player.LifeMax = 100;
			player.StatLife = 100;
			player.ManaMax = 20;
			player.StatMana = 20;
			return player;
		}

		public static void SendInfoToAll(string msg)
		{
			ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(msg), new Color(255, 255, 255));
		}

		public static ServerPlayer FindPlayer(int uuid)
		{
			foreach (var pair in ServerSideCharacter.XmlData.Data)
			{
				if (pair.Value.Uuid == uuid)
				{
					return pair.Value;
				}
			}
			throw new Exception("Cannot find the player!");
		}

		/// <summary>
		/// 如果玩家在任何领地内
		/// </summary>
		/// <param name="region"></param>
		/// <returns></returns>
		public bool InAnyRegion(out RegionInfo region)
		{
			foreach (var reg in ServerSideCharacter.RegionManager.GetList())
			{
				Rectangle worldArea = new Rectangle(reg.Area.X * 16, reg.Area.Y * 16,
					reg.Area.Width * 16, reg.Area.Height * 16);
				if (worldArea.Intersects(PrototypePlayer.Hitbox))
				{
					region = reg;
					return true;
				}
			}
			region = null;
			return false;
		}

		public void SavePlayer()
		{
			CopyFrom(PrototypePlayer);
			ServerSideCharacter.MainWriter.Write(this);
		}

		private static int GetNextId()
		{
			return _nextId++;
		}

		internal static void IncreaseUuid()
		{
			_nextId++;
		}

		internal static void ResetUuid()
		{
			_nextId = 0;
		}

	}
}
