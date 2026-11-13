namespace ASS.Features.Instances
{
    using ASS.Features.Templates;

    using LabApi.Features.Wrappers;

    using Mirror;

    public class HeaderInstance : ASSInstance
    {
        public HeaderInstance(Player viewer, int id, HeaderTemplate template)
            : base(viewer, id, template)
        {
            ReducedPadding = template.ReducedPadding;
        }

        public bool ReducedPadding { get; set; }

        internal override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);
            writer.WriteBool(ReducedPadding);
        }
    }
}