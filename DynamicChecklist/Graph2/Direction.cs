namespace DynamicChecklist.Graph2
{
    using System;

    [Flags]
    public enum Direction : byte
    {
        None = 0,
        Right = 0x01,
        UpRight = 0x02,
        Up = 0x04,
        UpLeft = 0x08,
        Left = 0x10,
        DownLeft = 0x20,
        Down = 0x40,
        DownRight = 0x80,

        Orthogonal = Right | Up | Left | Down,
        Diagonal = UpRight | UpLeft | DownRight | DownLeft,
        AnyUp = Up | UpRight | UpLeft,
        AnyRight = Right | UpRight | DownRight,
        AnyDown = Down | DownRight | DownLeft,
        AnyLeft = Left | UpLeft | DownLeft,
    }
}
