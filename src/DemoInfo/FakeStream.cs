using System;
using System.IO;

namespace DemoInfo
{
    public class FakeStream: Stream
    {
        public FakeStream()
        {
            
        }

        public override bool CanRead => false;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => 0;

        public override long Position {get;set;}

        public override void Flush() { }

        public override int Read(byte[] buffer, int offset, int count) => 1;

        public override long Seek(long offset, SeekOrigin origin) => 1;

        public override void SetLength(long value) { }

        public override void Write(byte[] buffer, int offset, int count) { }
    }
}