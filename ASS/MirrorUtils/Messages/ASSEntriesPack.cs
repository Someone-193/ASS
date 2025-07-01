namespace ASS.MirrorUtils.Messages
{
    using System.Collections.Generic;
    using System.Linq;
    using ASS.Settings;
    using Mirror;
    using UserSettings.ServerSpecific;

    public readonly struct ASSEntriesPack(ASSBase[] settings, int version) : NetworkMessage
    {
        public readonly ASSBase[] Settings = settings;
        public readonly int Version = version;

        public void Serialize(NetworkWriter writer)
        {
            writer.WriteInt(Version);
            if (Settings == null)
            {
                writer.WriteByte(0);
            }
            else
            {
                writer.WriteByte((byte)Settings.Count());
                foreach (ASSBase setting in Settings)
                {
                    writer.WriteByte(ServerSpecificSettingsSync.GetCodeFromType(setting.SSSType));
                    setting.Serialize(writer);
                }
            }
        }
    }
}