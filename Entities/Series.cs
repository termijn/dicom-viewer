using System;

namespace Entities
{
    public abstract class Series
    {
        public string SeriesInstanceUid { get; set; }
        public DateTime Time { get; set; }
    }

    public abstract class Series2D: Series
    {

    }

    public abstract class Series3D: Series
    {

    }
}
