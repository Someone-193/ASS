namespace ASS.Features.MirrorUtils.Messages
{
    using System.Collections.Generic;

    using ASS.Features.Instances;

    using Mirror;

    using UserSettings.ServerSpecific;

    // make sure all callers of ctor use rented lists.
    public readonly struct ASSEntriesPack(List<ASSInstance> instances, ServerSpecificSettingBase[]? baseSettings, int version) : NetworkMessage
    {
        public readonly List<ASSInstance> Instances = instances;
        public readonly ServerSpecificSettingBase[]? BaseSettings = baseSettings;
        public readonly int Version = version;

        public void Serialize(NetworkWriter writer)
        {
            writer.WriteInt(Version);
            if (Instances.Count is 0 && BaseSettings is null)
            {
                writer.WriteByte(0);
            }
            else
            {
                writer.WriteByte((byte)(Instances.Count + (BaseSettings?.Length ?? 0)));
                foreach (ASSInstance instance in Instances)
                {
                    instance.TimesTriggered = 0;

                    writer.WriteByte(ServerSpecificSettingsSync.GetCodeFromType(instance.Template.SSSType));
                    instance.Serialize(writer);
                }

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