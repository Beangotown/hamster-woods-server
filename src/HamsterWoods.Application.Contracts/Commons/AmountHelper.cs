using System;

namespace HamsterWoods.Commons;

public static class AmountHelper
{
    public static long GetAmount(int amount, int pow)
    {
        return (long)(amount * Math.Pow(10, pow));
    }
}