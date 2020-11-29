using DemoInfo.DP;
using DemoInfo.DT;
using DemoInfo.Messages;
using DemoInfo.ST;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace DemoInfo
{
    public class IdleDemoParser : DemoParser
	{

        public IdleDemoParser(Stream input): base(input)
        {

        }

        public new bool ParseHeader()
        {
            return true;
        }
        public new bool ParseNextTick() 
        {
            return true;
        }
    }
}
