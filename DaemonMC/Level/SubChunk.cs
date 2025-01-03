﻿using fNbt;

namespace DaemonMC.Level
{
    public class SubChunk
    {
        public int version = 8;
        public int storageSize = 0;
        public bool isRuntime = true;
        public int bitsPerBlock = 0;
        public List<NbtCompound> palette = new List<NbtCompound>();
        public byte[] blocks = new byte[40960];
        public uint[] words = new uint[1000];
    }
}
