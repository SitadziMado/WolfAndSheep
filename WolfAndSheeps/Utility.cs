using System;

namespace WolfAndSheeps
{
    public static class Utility
    {
        public static void Assert(bool expression, string message = "Ошибка.")
        {
            if (!expression)
                throw new ArgumentException(message);
        }
    }
}
