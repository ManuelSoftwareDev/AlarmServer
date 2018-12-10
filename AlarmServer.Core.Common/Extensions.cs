using System.Collections.Generic;

namespace AlarmServer.Core.Extensions
{
    public static class Extensions
    {
        public static T[] Add<T>(this T[] client, T add)
        {
            List<T> allList = new List<T>();

            if (client != null)
                allList.AddRange(client);
            allList.Add(add);

            return allList.ToArray();
        }

        public static AlarmProfile FindProfileById(this AlarmProfile[] profile, string id)
        {
            foreach (var profi in profile)
                if (profi.ProfileId == id)
                    return profi;

            return null;
        }

        public static AlarmClient FindClientById(this AlarmClient[] clients, string id)
        {
            foreach(var client in clients)
                if (client.ClientIdentifier == id)
                    return client;

            return null;
        }
    }
}
