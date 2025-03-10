﻿using DaemonMC.Network.Enumerations;
using DaemonMC.Utils.Game;

namespace DaemonMC.Network.Bedrock
{
    public class SetActorData : Packet
    {
        public override Info.Bedrock Id => Info.Bedrock.SetActorData;

        public long EntityId { get; set; } = 0;
        public Dictionary<ActorData, Metadata> Metadata { get; set; } = new Dictionary<ActorData, Metadata>();
        public long Tick { get; set; } = 0;

        protected override void Decode(PacketDecoder decoder)
        {

        }

        protected override void Encode(PacketEncoder encoder)
        {
            encoder.WriteVarLong((ulong) EntityId);
            encoder.WriteMetadata(Metadata);
            encoder.WriteVarInt(0);
            encoder.WriteVarInt(0); //todo here
            encoder.WriteVarLong((ulong) Tick);
        }
    }
}
