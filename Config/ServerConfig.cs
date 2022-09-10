﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using ServerSideCharacter.ServerCommand;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ServerSideCharacter.Config
{
	/// <summary>
	/// Server config file using json
	/// </summary>
	public class ConfigData
	{
		public int MaxRegions = 255;
		public int MaxRegionWidth = 35;
		public int MaxRegionHeigth = 35;
		public int PlayerMaxRegions = 3;
		public bool EnableChestProtection = true;
		public bool AutoProtectChests = true;
		public List<NetItem> StartUpItems;
		public List<NetItem> BannedItems;

		public ConfigData()
		{
			StartUpItems = new List<NetItem>();
			BannedItems = new List<NetItem>();
		}
	}

	public class ServerConfigManager
	{

		public bool ConfigExist
		{
			get
			{
				return _jsonExist;
			}
		}

		public List<NetItem> StartupItems
		{
			get
			{
				return _configData.StartUpItems;
			}
		}

		public int MaxRegionWidth
		{
			get
			{
				return _configData.MaxRegionWidth;
			}
		}


		public int MaxRegionHeigth
		{
			get
			{
				return _configData.MaxRegionHeigth;
			}
		}

		public int MaxRegions
		{
			get
			{
				return _configData.MaxRegions;
			}
		}

		public int PlayerMaxRegions
		{
			get
			{
				return _configData.PlayerMaxRegions;
			}
		}
		public bool EnableChestProtection
		{
			get { return _configData.EnableChestProtection; }
		}
		public bool AutoProtectChests
		{
			get { return _configData.AutoProtectChests; }
		}
		private static string _configPath = "SSC/config.json";

		private ConfigData _configData;

		private bool _jsonExist;



		public ServerConfigManager()
		{
			_jsonExist = File.Exists(_configPath);
			SetConfig();
		}

		private void SetConfig()
		{
			if (!ConfigExist)
			{
				_configData = new ConfigData();
				AddToStartInv(ItemID.ShadewoodSword, 82);
				AddToStartInv(ItemID.IronPickaxe, 83);
				AddToStartInv(ItemID.IronAxe, 81);
				AddToBannedItem(ItemID.IronAxe);
				string data = JsonConvert.SerializeObject(_configData, Formatting.Indented);
				using (StreamWriter sw = new StreamWriter(_configPath))
				{
					sw.Write(data);
				}
				CommandBroadcast.ConsoleMessage("Config file created.");
			}
			else
			{
				using (StreamReader sr = new StreamReader(_configPath))
				{
					string data = sr.ReadToEnd();
					_configData = JsonConvert.DeserializeObject<ConfigData>(data);
				}
			}
			SetUpStartInv();
		}

		private void SetUpStartInv()
		{
			if (ModLoader.Mods.Any(mod => mod.Name == "ThoriumMod"))
			{
				var thorium = ModLoader.Mods.Where(mod => mod.Name == "ThoriumMod");
				//wtf is this shit lmao
				//TODO: find way of getting id through reflection
				//AddToStartInv(thorium.First().Find("FamilyHeirloom"));
			}
		}



		private void AddToStartInv(int type, int prefex = 0)
		{
			Item item = new Item();
			item.SetDefaults(type);
			item.Prefix(prefex);
			_configData.StartUpItems.Add(Utils.ToNetItem(item));
		}

		private void AddToBannedItem(int type)
		{
			_configData.BannedItems.Add(Utils.ToNetItem(type));
		}

		public void Save()
		{
			string data = JsonConvert.SerializeObject(_configData, Formatting.Indented);
			using (StreamWriter sw = new StreamWriter(_configPath))
				sw.Write(data);
		}

		public bool IsItemBanned(Item item, ServerPlayer player)
		{
			if (player.PermissionGroup.GroupName == "spadmin")
			{
				return false;
			}
			bool banned = _configData.BannedItems.Any(nitem => nitem.TheSameItem(item));
			return banned;
		}

		public void ToggleItemBan(int type, ServerPlayer player)
		{
			Item item = new Item();
			item.netDefaults(type);
			if (_configData.BannedItems.Any(nitem => nitem.TheSameItem(item)))
			{
				_configData.BannedItems.RemoveAll(nitem => nitem.TheSameItem(item));
				player.SendSuccessInfo("Now the item " + item.Name + " is unbanned!");
			}
			else
			{
				_configData.BannedItems.Add(Utils.ToNetItem(item));
				player.SendSuccessInfo("You have successfully banned " + item.Name + ".");
			}
		}
	}
}
