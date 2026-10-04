using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SshPortForwarder.Models;

namespace SshPortForwarder.Services
{
    public static class ProfileStore
    {
        private static readonly string DataDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SshPortForwarder");

        private static readonly string ProfilesFile = Path.Combine(DataDir, "profiles.json");

        public static List<TunnelProfile> Load()
        {
            try
            {
                if (!File.Exists(ProfilesFile))
                    return new List<TunnelProfile>();

                var json = File.ReadAllText(ProfilesFile);
                return Parse(json);
            }
            catch
            {
                return new List<TunnelProfile>();
            }
        }

        private static List<TunnelProfile> Parse(string json)
        {
            var array = JArray.Parse(json);
            foreach (var token in array)
            {
                if (token is not JObject obj) continue;
                MigrateLegacyForward(obj);
            }

            return array.ToObject<List<TunnelProfile>>() ?? new List<TunnelProfile>();
        }

        /// <summary>
        /// Eski tek-port formatını (RemoteHost/RemotePort/LocalPort) Forwards listesine taşır ve
        /// eski alanları kaldırır; böylece dosya ilk kayıtta yeni formata geçer.
        /// </summary>
        private static void MigrateLegacyForward(JObject obj)
        {
            if (obj["RemoteHost"] == null && obj["RemotePort"] == null && obj["LocalPort"] == null)
                return;

            bool hasForwards = obj["Forwards"] is JArray { Count: > 0 };
            if (!hasForwards)
            {
                var migrated = new PortForward
                {
                    Enabled = true,
                    LocalPort = obj.Value<int?>("LocalPort") ?? 8080,
                    RemoteHost = obj.Value<string>("RemoteHost") ?? "127.0.0.1",
                    RemotePort = obj.Value<int?>("RemotePort") ?? 80
                };
                obj["Forwards"] = new JArray(JObject.FromObject(migrated));
            }

            obj.Remove("RemoteHost");
            obj.Remove("RemotePort");
            obj.Remove("LocalPort");
        }

        public static void Save(List<TunnelProfile> profiles)
        {
            Directory.CreateDirectory(DataDir);
            var json = JsonConvert.SerializeObject(profiles, Formatting.Indented);
            File.WriteAllText(ProfilesFile, json);
        }
    }
}
