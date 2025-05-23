﻿using System.Numerics;
using DaemonMC.Blocks;

namespace DaemonMC.Network.Bedrock
{
    public class UpdateBlock : Packet
    {
        public override Info.Bedrock Id => Info.Bedrock.UpdateBlock;

        public Vector3 Position { get; set; } = new Vector3();
        public Block Block { get; set; } = new Air();

        protected override void Decode(PacketDecoder decoder)
        {

        }

        protected override void Encode(PacketEncoder encoder)
        {
            encoder.WriteBlockNetPos(Position);
            encoder.WriteVarInt(Block.GetHash());
            encoder.WriteVarInt(720912);
            encoder.WriteVarInt(0);
        }
    }
}
