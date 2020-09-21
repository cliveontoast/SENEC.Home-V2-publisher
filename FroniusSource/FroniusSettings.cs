using System;

namespace FroniusSource
{
    public class FroniusSettings : IFroniusSettings
    {
        public string? IP { get; set; }
    }

    public interface IFroniusSettings
    {
        string? IP { get; set; }
    }
}