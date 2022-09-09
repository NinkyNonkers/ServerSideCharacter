using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using ServerSideCharacter.Items;
using Terraria;
using Terraria.ModLoader;

namespace ServerSideCharacter
{
	public class PlayerData
	{
		public Dictionary<string, ServerPlayer> Data = new Dictionary<string, ServerPlayer>();

		public PlayerData()
		{

		}

		private static string ReadNext(XmlNodeList info, ref int i)
		{
			var xmlNode = info.Item(i++);
			if (xmlNode != null) return xmlNode.InnerText;
			throw new NullReferenceException("XMLNode is null");
		}


		private void TryReadItemInfo(Dictionary<string, Mod> modTable, XmlNodeList info,
			ServerPlayer player, ref int i, int id, ref Item[] slots)
		{
			int type;
			string text = ReadNext(info, ref i);
			//如果是mod物品
			if (text[0] == '$')
			{
				text = text.Substring(1);

				string modName = text.Substring(0, text.IndexOf('.'));
				string itemName = text.Substring(text.LastIndexOf('.') + 1);
				//解析物品id，字典中有mod名字
				if (modTable.ContainsKey(modName))
				{
					type = modTable[modName].ItemType(itemName);
					//如果数据合法
					if (type > 0)
					{
						slots[id].netDefaults(type);
						foreach (var pair in ModDataHooks.ItemExtraInfoTable)
						{
							ModDataHooks.InterpretStringTable[pair.Key].Invoke(
								((info.Item(i - 1) as XmlElement).GetAttribute(pair.Key)),
								slots[id]);
						}
					}
					else
					{
						slots[id].netDefaults(ServerSideCharacter.Instance.ItemType("TestItem"));
						((TestItem)slots[id].ModItem).SetUp(text);
						//物品数据会丢失
					}
				}
				else
				{
					slots[id].netDefaults(ServerSideCharacter.Instance.ItemType("TestItem"));
					((TestItem)slots[id].ModItem).SetUp(text);
					//物品数据会丢失

				}
			}
			else
			{
				type = Convert.ToInt32(text);

				if (type != 0)
				{
					slots[id].netDefaults(type);
				}
				foreach (var pair in ModDataHooks.ItemExtraInfoTable)
				{
					ModDataHooks.InterpretStringTable[pair.Key].Invoke(
						((info.Item(i - 1) as XmlElement).GetAttribute(pair.Key)),
						slots[id]);
				}
			}
		}

		public PlayerData(string path)
		{
			ServerPlayer.ResetUuid();
			if (File.Exists(path))
			{
				XmlReaderSettings settings = new XmlReaderSettings { IgnoreComments = true };
				//忽略文档里面的注释
				XmlDocument xmlDoc = new XmlDocument();
				XmlReader reader = XmlReader.Create(path, settings);
				xmlDoc.Load(reader);
				XmlNode xn = xmlDoc.SelectSingleNode("Players");
				var list = xn.ChildNodes;
				Dictionary<string, Mod> modTable = new Dictionary<string, Mod>();
				foreach (var mod in ModLoader.LoadedMods)
				{
					modTable.Add(mod.Name, mod);
				}
				foreach (var node in list)
				{
					XmlElement pData = (XmlElement)node;
					ServerPlayer player = new ServerPlayer();
					var info = pData.ChildNodes;
					int i = 0;
					player.Name = pData.GetAttribute("name");
					player.Uuid = int.Parse(pData.GetAttribute("uuid"));
					try
					{
						player.PermissionGroup = ServerSideCharacter.GroupManager.Groups[pData.GetAttribute("group")];
					}
					catch
					{
						player.PermissionGroup = ServerSideCharacter.GroupManager.Groups["default"];
					}
					foreach (var pair in ModDataHooks.InterpretPlayerStringTable)
					{
						pair.Value(pData.GetAttribute(pair.Key), player);
					}
					player.HasPassword = Convert.ToBoolean(ReadNext(info, ref i));
					player.Password = ReadNext(info, ref i);
					player.LifeMax = Convert.ToInt32(ReadNext(info, ref i));
					player.StatLife = Convert.ToInt32(ReadNext(info, ref i));
					player.ManaMax = Convert.ToInt32(ReadNext(info, ref i));
					player.StatMana = Convert.ToInt32(ReadNext(info, ref i));
					for (int id = 0; id < player.Inventory.Length; id++)
					{
						TryReadItemInfo(modTable, info, player, ref i, id, ref player.Inventory);
						//player.inventory[id].Prefix(Convert.ToByte((info.Item(i - 1) as XmlElement).GetAttribute("prefix")));
						//player.inventory[id].stack =
						//	Convert.ToInt32((info.Item(i - 1) as XmlElement).GetAttribute("stack"));
					}
					for (int id = 0; id < player.Armor.Length; id++)
					{
						TryReadItemInfo(modTable, info, player, ref i, id, ref player.Armor);
					}
					for (int id = 0; id < player.Dye.Length; id++)
					{
						TryReadItemInfo(modTable, info, player, ref i, id, ref player.Dye);
					}
					for (int id = 0; id < player.MiscEquips.Length; id++)
					{
						TryReadItemInfo(modTable, info, player, ref i, id, ref player.MiscEquips);
					}
					for (int id = 0; id < player.MiscDye.Length; id++)
					{
						TryReadItemInfo(modTable, info, player, ref i, id, ref player.MiscDye);
					}
					for (int id = 0; id < player.Bank.item.Length; id++)
					{
						TryReadItemInfo(modTable, info, player, ref i, id, ref player.Bank.item);
					}
					for (int id = 0; id < player.Bank2.item.Length; id++)
					{
						TryReadItemInfo(modTable, info, player, ref i, id, ref player.Bank2.item);
					}
					for (int id = 0; id < player.Bank3.item.Length; id++)
					{
						TryReadItemInfo(modTable, info, player, ref i, id, ref player.Bank3.item);
					}

					Data.Add(player.Name, player);
					ServerPlayer.IncreaseUuid();
				}
				ServerSideCharacter.MainWriter = new XmlWriter(path);
				reader.Close();
			}
			else
			{
				XmlWriter writer = new XmlWriter(path);
				writer.Create();
				ServerSideCharacter.MainWriter = writer;
			}
		}
	}
}
