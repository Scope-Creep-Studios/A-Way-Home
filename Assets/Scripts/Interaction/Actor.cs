using System;

[Flags]
public enum Actor
{
    None  = 0,
    Girl  = 1 << 0,
    Ghost = 1 << 1,
    Both  = Girl | Ghost
}
