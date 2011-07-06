using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mi.PE.PEFormat
{
    public struct ImageTimestamp
    {
        public static readonly DateTime EpochUTC = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc); 

        public readonly uint SecondsSinceEpochUTC;

        public ImageTimestamp(uint secondsSinceEpochUTC)
        {
            this.SecondsSinceEpochUTC = secondsSinceEpochUTC;
        }

        public ImageTimestamp(DateTime dateTime)
        {
            this.SecondsSinceEpochUTC = checked((uint)((dateTime - EpochUTC).Ticks / TimeSpan.TicksPerSecond));
        }

        public ImageTimestamp(DateTimeOffset dateTime)
        {
            this.SecondsSinceEpochUTC = checked((uint)((dateTime.ToUniversalTime() - EpochUTC).Ticks / TimeSpan.TicksPerSecond));
        }

        public DateTime ToDateTime()
        {
            return EpochUTC.AddSeconds(SecondsSinceEpochUTC);
        }

        public override string ToString()
        {
            return ToDateTime().ToString();
        }
    }
}
