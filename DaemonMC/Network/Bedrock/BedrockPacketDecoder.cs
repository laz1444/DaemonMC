﻿using DaemonMC.Network.RakNet;
using DaemonMC.Utils.Text;

namespace DaemonMC.Network.Bedrock
{
    public class BedrockPacketDecoder
    {
        public static void BedrockDecoder(PacketDecoder decoder)
        {
            if (RakSessionManager.getSession(decoder.clientEp) != null)
            {
                if (RakSessionManager.getSession(decoder.clientEp).initCompression)
                {
                    decoder.ReadByte();
                }
                if (Server.OnlinePlayers.ContainsKey(RakSessionManager.getSession(decoder.clientEp).EntityID))
                {
                    decoder.player = Server.GetPlayer(RakSessionManager.getSession(decoder.clientEp).EntityID);
                }
            }

            while (decoder.readOffset < decoder.buffer.Length)
            {
                var startOffset = decoder.readOffset;
                var size = decoder.ReadVarInt(); //packet size

                var pkid = (Info.Bedrock)decoder.ReadVarInt();
                Log.packetIn(decoder.clientEp, pkid);

                switch (pkid)
                {
                    case Info.Bedrock.RequestNetworkSettings:
                        new RequestNetworkSettings().DecodePacket(decoder, PacketHandler.Bedrock);
                        break;
                    case Info.Bedrock.Login:
                        new Login().DecodePacket(decoder, PacketHandler.Bedrock);
                        break;
                    case Info.Bedrock.PacketViolationWarning:
                        new PacketViolationWarning().DecodePacket(decoder, PacketHandler.Bedrock);
                        break;
                    case Info.Bedrock.ClientCacheStatus:
                        new ClientCacheStatus().DecodePacket(decoder, PacketHandler.Bedrock);
                        break;
                    case Info.Bedrock.ResourcePackClientResponse:
                        new ResourcePackClientResponse().DecodePacket(decoder, PacketHandler.Bedrock);
                        break;
                    case Info.Bedrock.RequestChunkRadius:
                        new RequestChunkRadius().DecodePacket(decoder);
                        break;
                    case Info.Bedrock.MovePlayer:
                        new MovePlayer().DecodePacket(decoder);
                        break;
                    case Info.Bedrock.ServerboundLoadingScreen:
                        new ServerboundLoadingScreen().DecodePacket(decoder);
                        break;
                    case Info.Bedrock.Interact:
                        new Interact().DecodePacket(decoder);
                        break;
                    case Info.Bedrock.TextMessage:
                        new TextMessage().DecodePacket(decoder);
                        break;
                    case Info.Bedrock.PlayerAuthInput:
                        new PlayerAuthInput().DecodePacket(decoder);
                        break;
                    case Info.Bedrock.PlayerSkin:
                        new PlayerSkin().DecodePacket(decoder);
                        break;
                    case Info.Bedrock.ResourcePackChunkRequest:
                        new ResourcePackChunkRequest().DecodePacket(decoder, PacketHandler.Bedrock);
                        break;
                    case Info.Bedrock.CommandRequest:
                        new CommandRequest().DecodePacket(decoder);
                        break;
                    case Info.Bedrock.SetLocalPlayerAsInitialized:
                        new SetLocalPlayerAsInitialized().DecodePacket(decoder);
                        break;
                    case Info.Bedrock.EmoteList:
                        new EmoteList().DecodePacket(decoder);
                        break;
                    case Info.Bedrock.Emote:
                        new Emote().DecodePacket(decoder);
                        break;
                    case Info.Bedrock.Animate:
                        new Animate().DecodePacket(decoder);
                        break;
                    case Info.Bedrock.InventoryTransaction:
                        new InventoryTransaction().DecodePacket(decoder);
                        break;
                    case Info.Bedrock.ModalFormResponse:
                        new ModalFormResponse().DecodePacket(decoder);
                        break;


                    default:
                        Log.error($"[Server] Unknown Bedrock packet: {pkid}");
                        ToDataTypes.HexDump(decoder.buffer, decoder.buffer.Length);
                        break;
                }

                int expectedOffset = startOffset + size;
                if (decoder.readOffset < expectedOffset)
                {
                    Log.warn($"{expectedOffset - decoder.readOffset} bytes left while reading {pkid}.");
                    break;

                }
            }
        }
    }
}
