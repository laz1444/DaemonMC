﻿namespace DaemonMC.Blocks
{
    public class BlackstoneWall : Block
    {
        public BlackstoneWall()
        {
            Name = "minecraft:blackstone_wall";

            States["wall_connection_type_east"] = "none";
            States["wall_connection_type_north"] = "none";
            States["wall_connection_type_south"] = "none";
            States["wall_connection_type_west"] = "none";
            States["wall_post_bit"] = (byte)0;
        }
    }
}
