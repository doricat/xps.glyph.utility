using System;

namespace SharpGlyph
{
    [Flags]
    public enum StyleSimulations
    {
        None = 0,
        BoldSimulation = 1,
        ItalicSimulation = 2,
        BoldItalicSimulation = BoldSimulation | ItalicSimulation
    }
}