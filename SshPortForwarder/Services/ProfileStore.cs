using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
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
                return JsonConvert.DeserializeObject<List<TunnelProfile>>(json)
                       ?? new List<TunnelProfile>();
            }
            catch
            {
                return new List<TunnelProfile>();
            }
        }

        public static void Save(List<TunnelProfile> profiles)
        {
            Directory.CreateDirectory(DataDir);
            var json = JsonConvert.SerializeObject(profiles, Formatting.Indented);
            File.WriteAllText(ProfilesFile, json);
        }
    }
}
