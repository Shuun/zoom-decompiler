using System;

public static class Goto
{
    private static bool value;

    public static void Run()
    {
        while (Goto.value)
        {
            while (Goto.value)
            {
                if (Goto.value)
                {
                    goto IL_2A;
                }
            }
        }
    IL_2A:
        if (Goto.value)
        {
            Console.WriteLine("");
        }
    }
}