using System;
using System.IO;
using System.Text;
using System.Xml;
using ServerSideCharacter.XMLHelper;
using Terraria;

namespace ServerSideCharacter
{
	public class XmlWriter
	{
		public string FilePath { get; set; }

		public XmlDocument XmlDoc;

		public XmlNode PlayerRoot;

		public XmlWriter()
		{

		}

		public XmlWriter(string path)
		{
			FilePath = path;
			if (!File.Exists(path))
			{
				Create();
			}
			else
			{
				XmlDoc = new XmlDocument();
				XmlDoc.Load(path);
				PlayerRoot = XmlDoc.SelectSingleNode("Players");
			}
		}

		public void Create()
		{
			XmlDocument xmlDoc = new XmlDocument();
			XmlNode node = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", "");
			xmlDoc.AppendChild(node);
			//创建根节点    
			XmlNode root = xmlDoc.CreateElement("Players");
			xmlDoc.AppendChild(root);
			XmlDoc = xmlDoc;
			PlayerRoot = root;
		}

		private XmlElement WriteItemInfo(XmlNodeList list, int i, ref int j, ref Item[] slots)
		{
			Item item = slots[i];
			XmlElement node1;
			if (item.type < Main.maxItemTypes)
			{
				node1 = (XmlElement)WriteNext(list, ref j, item.type.ToString());
			}
			else
			{
				node1 = (XmlElement)WriteNext(list, ref j, "$" + item.ModItem.GetType().FullName);
			}
			foreach (var pair in ModDataHooks.ItemExtraInfoTable)
			{
				node1.SetAttribute(pair.Key, pair.Value(slots[i]));
			}
			return node1;
		}

		private XmlElement CreateItemInfo(XmlNode parent, int i, ref Item[] slots, string name)
		{
			Item item = slots[i];
			XmlElement node1;
			if (item.type < Main.maxItemTypes)
			{
				node1 = (XmlElement)NodeHelper.CreateNode(XmlDoc, parent, name + "_" + i,
					item.type.ToString());
			}
			else
			{
				node1 = (XmlElement)NodeHelper.CreateNode(XmlDoc, parent, name + "_" + i,
					"$" + item.ModItem.GetType().FullName);
			}
			foreach (var pair in ModDataHooks.ItemExtraInfoTable)
			{
				node1.SetAttribute(pair.Key, pair.Value(slots[i]));
			}
			return node1;
		}

		private XmlNode WriteNext(XmlNodeList list, ref int i, string toWrite)
		{
			var node = list.Item(i);
			list.Item(i++).InnerText = toWrite;
			return node;
		}

		public void SavePlayer(ServerPlayer player)
		{
			XmlElement targetNode = null;
			foreach (var node in PlayerRoot.ChildNodes)
			{
				XmlElement element = node as XmlElement;
				if (element.GetAttribute("name").Equals(player.Name))
				{
					targetNode = element;
					element.SetAttribute("group", player.PermissionGroup.GroupName);
					foreach (var pair in ModDataHooks.PlayerExtraInfoTable)
					{
						element.SetAttribute(pair.Key, pair.Value(player));
					}
					break;
				}
			}
			if (targetNode == null)
			{
				Console.WriteLine("Creating new Account");
				Write(player);
			}
			else
			{
				var list = targetNode.ChildNodes;
				int j = 0;
				WriteNext(list, ref j, player.HasPassword.ToString());
				WriteNext(list, ref j, player.Password);
				WriteNext(list, ref j, player.LifeMax.ToString());
				WriteNext(list, ref j, player.StatLife.ToString());
				WriteNext(list, ref j, player.ManaMax.ToString());
				WriteNext(list, ref j, player.StatMana.ToString());
				for (int i = 0; i < player.Inventory.Length; i++)
				{
					//TODO: Mod Item check
					var node1 = WriteItemInfo(list, i, ref j, ref player.Inventory);
					//TODO: Additional mod item info
				}
				for (int i = 0; i < player.Armor.Length; i++)
				{
					//TODO: Mod Item check
					var node1 = WriteItemInfo(list, i, ref j, ref player.Armor);
				}
				for (int i = 0; i < player.Dye.Length; i++)
				{
					var node1 = WriteItemInfo(list, i, ref j, ref player.Dye);
				}
				for (int i = 0; i < player.MiscEquips.Length; i++)
				{
					var node1 = WriteItemInfo(list, i, ref j, ref player.MiscEquips);
				}
				for (int i = 0; i < player.MiscDye.Length; i++)
				{
					var node1 = WriteItemInfo(list, i, ref j, ref player.MiscDye);
				}
				for (int i = 0; i < player.Bank.item.Length; i++)
				{
					var node1 = WriteItemInfo(list, i, ref j, ref player.Bank.item);
				}
				for (int i = 0; i < player.Bank2.item.Length; i++)
				{
					var node1 = WriteItemInfo(list, i, ref j, ref player.Bank2.item);
				}
				for (int i = 0; i < player.Bank3.item.Length; i++)
				{
					var node1 = WriteItemInfo(list, i, ref j, ref player.Bank3.item);
				}
				using (XmlTextWriter xtw = new XmlTextWriter(FilePath, Encoding.UTF8))
				{
					xtw.Formatting = Formatting.Indented;
					XmlDoc.Save(xtw);
				}
			}
		}


		public void Write(ServerPlayer player)
		{

			XmlNode playerNode = XmlDoc.CreateNode(XmlNodeType.Element, "Player", null);
			XmlElement element = playerNode as XmlElement;
			element.SetAttribute("name", player.Name);
			element.SetAttribute("uuid", player.Uuid.ToString());
			element.SetAttribute("group", player.PermissionGroup.GroupName);
			foreach (var pair in ModDataHooks.PlayerExtraInfoTable)
			{
				element.SetAttribute(pair.Key, pair.Value(player));
			}
			NodeHelper.CreateNode(XmlDoc, playerNode, "haspwd", player.HasPassword.ToString());
			NodeHelper.CreateNode(XmlDoc, playerNode, "password", player.Password);
			NodeHelper.CreateNode(XmlDoc, playerNode, "lifeMax", player.LifeMax.ToString());
			NodeHelper.CreateNode(XmlDoc, playerNode, "statlife", player.StatLife.ToString());
			NodeHelper.CreateNode(XmlDoc, playerNode, "manaMax", player.ManaMax.ToString());
			NodeHelper.CreateNode(XmlDoc, playerNode, "statmana", player.StatMana.ToString());
			for (int i = 0; i < player.Inventory.Length; i++)
			{
				//TODO: Mod Item check
				var node1 = CreateItemInfo(playerNode, i, ref player.Inventory, "slot");
				//TODO: Additional mod item info
			}
			for (int i = 0; i < player.Armor.Length; i++)
			{
				//TODO: Mod Item check
				var node1 = CreateItemInfo(playerNode, i, ref player.Armor, "armor");
			}
			for (int i = 0; i < player.Dye.Length; i++)
			{
				var node1 = CreateItemInfo(playerNode, i, ref player.Dye, "dye");
			}
			for (int i = 0; i < player.MiscEquips.Length; i++)
			{
				var node1 = CreateItemInfo(playerNode, i, ref player.MiscEquips, "miscEquips");
			}
			for (int i = 0; i < player.MiscDye.Length; i++)
			{
				var node1 = CreateItemInfo(playerNode, i, ref player.MiscDye, "miscDye");
			}
			for (int i = 0; i < player.Bank.item.Length; i++)
			{
				var node1 = CreateItemInfo(playerNode, i, ref player.Bank.item, "bank");
			}
			for (int i = 0; i < player.Bank2.item.Length; i++)
			{
				var node1 = CreateItemInfo(playerNode, i, ref player.Bank2.item, "bank2");
			}
			for (int i = 0; i < player.Bank3.item.Length; i++)
			{
				var node1 = CreateItemInfo(playerNode, i, ref player.Bank3.item, "bank3");
			}
			PlayerRoot.AppendChild(playerNode);


			using (XmlTextWriter xtw = new XmlTextWriter(FilePath, Encoding.UTF8))
			{
				xtw.Formatting = Formatting.Indented;
				XmlDoc.Save(xtw);
			}
		}
	}
}
