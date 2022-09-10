#define DEBUGMODE

using System.IO;
using Terraria;
using Terraria.GameContent.Events;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.Social;

namespace ServerSideCharacter
{
	public class SSCSystem : ModSystem
	{
		public static bool ServerStarted;

		public override bool HijackGetData(ref byte messageType, ref BinaryReader reader, int playerNumber)
		{
			return ServerSideCharacter.MessageChecker.CheckMessage(ref messageType, ref reader, playerNumber);
		}
		
		public override bool HijackSendData(int whoAmI, int msgType, int remoteClient, int ignoreClient, NetworkText text, int number, float number2, float number3, float number4, int number5, int number6, int number7)
        		{
        			switch (msgType)
        			{
        				case MessageID.WorldData:
        					if (Main.netMode != 2) // we will not process this message in client-side
        						break;
        					
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
        					binaryWriter.Write((byte)WorldGen.treeBG1);
        					binaryWriter.Write((byte)WorldGen.treeBG2);
        					binaryWriter.Write((byte)WorldGen.treeBG3);
        					binaryWriter.Write((byte)WorldGen.treeBG4);
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
	                            binaryWriter.Write(Main.treeX[k]);
        					
        					for (var l = 0; l < 4; l++)
	                            binaryWriter.Write((byte)Main.treeStyle[l]);
        					
        					for (var m = 0; m < 3; m++)
	                            binaryWriter.Write(Main.caveBackX[m]);
        					
        					for (var n = 0; n < 4; n++)
	                            binaryWriter.Write((byte)Main.caveBackStyle[n]);
        					
        					if (!Main.raining)
	                            Main.maxRaining = 0f;
        					
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
		
	}
}
