using System;
using DMapper.Attributes;
using DMapper.Converters;
using DMapper.Extensions;
using Xunit;

namespace DMapper.Tests
{
    public class UnixEpochConverter : IDMapperPropertyConverter
    {
        public object Convert(object src)
        {
            if (src is long)
            {
                long seconds = System.Convert.ToInt64(src);
                return DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime;
            }
            else if (src is DateTime)
            {
                return ((DateTime)src).ToUniversalTime();
            }
            else
            {
                throw new ArgumentException($"Cannot convert {src.GetType().Name} to DateTime");
            }
        }
    }

    internal class SrcEpoch
    {
        public long CreatedAtEpoch { get; set; }
    }

    internal class DestEpoch
    {
        // Bind to the correct source key **and** apply the converter
        [BindTo("CreatedAtEpoch")]
        [DMapperConverter(typeof(UnixEpochConverter))]
        public DateTime CreatedAt { get; set; }
    }

    public class PropertyConverterTests
    {
        [Fact]
        public void MapTo_ShouldInvoke_Custom_ValueConverter()
        {
            long epoch = 1_718_064_000; // 2024‑06‑11T00:00:00Z
            var src = new SrcEpoch { CreatedAtEpoch = epoch };

            var dest = src.MapTo<DestEpoch>();

            var expected = DateTimeOffset
                .FromUnixTimeSeconds(epoch)
                .UtcDateTime;

            Assert.Equal(expected, dest.CreatedAt);
            Assert.Equal(DateTimeKind.Utc, dest.CreatedAt.Kind);
        }
    }
}