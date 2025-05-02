using System;
using System.Collections.Generic;
using DMapper.Extensions;
using Xunit;

namespace DMapper.Tests
{
    public class DateTimeConversionTests
    {
        // ---------------------------------------------------------------------
        // 1) DateTimeOffset  ->  DateTime
        // ---------------------------------------------------------------------
        private class SrcDto1
        {
            public DateTimeOffset CreatedAt { get; set; }
        }
        private class Dest1
        {
            public DateTime CreatedAt { get; set; }
        }

        [Fact]
        public void MapTo_ShouldConvert_DateTimeOffset_To_DateTime()
        {
            var dto = new DateTimeOffset(2025, 01, 01, 12, 34, 56, TimeSpan.FromHours(+2)); // UTC+2
            var src = new SrcDto1 { CreatedAt = dto };

            var dest = src.MapTo<Dest1>();

            Assert.Equal(dto.UtcDateTime, dest.CreatedAt);
            Assert.Equal(DateTimeKind.Utc, dest.CreatedAt.Kind);
        }

        // ---------------------------------------------------------------------
        // 2) DateTime  ->  DateTimeOffset
        // ---------------------------------------------------------------------
        private class SrcDto2
        {
            public DateTime HappenedOn { get; set; }
        }
        private class Dest2
        {
            public DateTimeOffset HappenedOn { get; set; }
        }

        [Fact]
        public void MapTo_ShouldConvert_DateTime_To_DateTimeOffset()
        {
            var when = new DateTime(2024, 12, 31, 23, 59, 59, DateTimeKind.Utc);
            var src  = new SrcDto2 { HappenedOn = when };

            var dest = src.MapTo<Dest2>();

            Assert.Equal(new DateTimeOffset(when, TimeSpan.Zero), dest.HappenedOn);
        }

        // ---------------------------------------------------------------------
        // 3) Collection element conversion
        // ---------------------------------------------------------------------
        private class SrcDto3
        {
            public List<DateTimeOffset> Values { get; set; }
        }
        private class Dest3
        {
            public List<DateTime> Values { get; set; }
        }

        [Fact]
        public void MapTo_ShouldConvert_Collections_Of_DateTimeOffset()
        {
            var dto1 = new DateTimeOffset(2025, 05, 01,  0,  0, 0, TimeSpan.Zero);
            var dto2 = new DateTimeOffset(2025, 05, 02, 15, 30, 0, TimeSpan.FromHours(+3));
            var src  = new SrcDto3 { Values = new List<DateTimeOffset> { dto1, dto2 } };

            var dest = src.MapTo<Dest3>();

            Assert.Equal(dto1.UtcDateTime, dest.Values[0]);
            Assert.Equal(dto2.UtcDateTime, dest.Values[1]);
        }
    }
}
