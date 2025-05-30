﻿namespace DaemonMC.Items
{
    public abstract class Item
    {
        public string Name { get; protected set; } = "minecraft:air";
        public short Id { get; protected set; } = 0;
        public int Version { get; protected set; } = 0;
        public bool ComponentBased { get; protected set; } = false;
        public ushort Count { get; set; } = 1;
        public int Aux { get; set; } = 0;

        public Item Clone()
        {
            return (Item)this.MemberwiseClone();
        }
    }
}
