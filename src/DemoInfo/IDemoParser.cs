using System;

namespace DemoInfo
{
    public interface IDemoParser
    {
        bool ParseNextTick();
        bool ParseHeader();
    }
}