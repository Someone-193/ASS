namespace ASS.Features.MirrorUtils.Messages
{
    using System.Collections.Generic;

    using ASS.Features.Settings;

    using Mirror;

    using NorthwoodLib.Pools;

    using UserSettings.ServerSpecific;

    // make sure all callers of ctor use rented lists.
    public readonly struct ASSEntriesPack(List<ASSBase> settings, ServerSpecificSettingBase[]? baseSettings, int version) : NetworkMessage
    {
        public readonly List<ASSBase> Settings = settings;
        public readonly ServerSpecificSettingBase[]? BaseSettings = baseSettings;
        public readonly int Version = version;

        public void Serialize(NetworkWriter writer)
        {
            writer.WriteInt(Version);
            if (Settings.Count is 0 && BaseSettings is null)
            {
                writer.WriteByte(0);
            }
            else
            {
                writer.WriteByte((byte)(Settings.Count + (BaseSettings?.Length ?? 0)));
                foreach (ASSBase setting in Settings)
                {
                    writer.WriteByte(ServerSpecificSettingsSync.GetCodeFromType(setting.SSSType));
                    setting.Serialize(writer);
                }

                ListPool<ASSBase>.Shared.Return(Settings);

                if (BaseSettings is not null)
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