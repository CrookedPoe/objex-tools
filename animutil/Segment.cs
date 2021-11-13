using System;

namespace animutil
{
    public class SegmentAddress
    {
        public int Segment;
        public int Address;

        public SegmentAddress(UInt32 o)
        {
            Segment = (int)(o & 0xFF000000) >> 24;
            Address = (int)(o & 0x00FFFFFF);
        }

        public SegmentAddress(string _o)
        {
            UInt32 o = Convert.ToUInt32(_o);
            Segment = (int)(o & 0xFF000000) >> 24;
            Address = (int)(o & 0x00FFFFFF);  
        }

        public SegmentAddress(string _o, int radix)
        {
            UInt32 o = Convert.ToUInt32(_o, radix);
            Segment = (int)(o & 0xFF000000) >> 24;
            Address = (int)(o & 0x00FFFFFF);  
        }

        public override string ToString()
        {
            return $"{Segment.ToString("X2")}{Address.ToString("X6")}";
        }
    }
}