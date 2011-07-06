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
            long ticksFromEpoch = (dateTime - EpochUTC).Ticks;

            // rounding half-second and more up
            long secondsFromEpoch = (ticksFromEpoch + TimeSpan.TicksPerSecond / 2) / TimeSpan.TicksPerSecond;

            this.SecondsSinceEpochUTC = checked((uint)secondsFromEpoch);
        }

        public ImageTimestamp(DateTimeOffset dateTime)
            : this(dateTime.ToUniversalTime().DateTime)
        {
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