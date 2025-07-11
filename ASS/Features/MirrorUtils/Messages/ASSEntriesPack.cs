namespace ASS.Features.MirrorUtils.Messages
{
    using ASS.Features.Settings;
    using Mirror;
    using UserSettings.ServerSpecific;

    public readonly struct ASSEntriesPack(ASSBase[] settings, ServerSpecificSettingBase[]? baseSettings, int version) : NetworkMessage
    {
        public readonly ASSBase[]? Settings = settings;
        public readonly ServerSpecificSettingBase[]? BaseSettings = baseSettings;
        public readonly int Version = version;

        public void Serialize(NetworkWriter writer)
        {
            writer.WriteInt(Version);
            if (Settings == null && BaseSettings == null)
            {
                writer.WriteByte(0);
            }
            else
            {
                writer.WriteByte((byte)((Settings?.Length ?? 0) + (BaseSettings?.Length ?? 0)));
                if (Settings != null)
                {
                    foreach (ASSBase setting in Settings)
                    {
                        writer.WriteByte(ServerSpecificSettingsSync.GetCodeFromType(setting.SSSType));
                        setting.Serialize(writer);
                    }
                }

                if (BaseSettings != null)
                {
                    foreach (ServerSpecificSettingBase setting in BaseSettings)
                    {
                        writer.WriteByte(ServerSpecificSettingsSync.GetCodeFromType(setting.GetType()));
                        setting.SerializeEntry(writer);
                    }
                }
            }
        }
    }
}