using System.Collections.Generic;

namespace AlarmServerService
{
    public static class Extensions
    {
        public static T[] Add<T>(this T[] client, T add)
        {
            List<T> allList = new List<T>();

            allList.AddRange(client);
            allList.Add(add);

            return allList.ToArray();
        }

    }
}
