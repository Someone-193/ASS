namespace ASS.Features.Instances.Displays
{
    using ASS.Features.Templates;

    using LabApi.Features.Wrappers;

    public abstract class ASSDisplayInstance : ASSInstance
    {
        protected ASSDisplayInstance(Player viewer, int id, ASSTemplate template)
            : base(viewer, id, template)
        {
        }

        protected override bool IsServerOnly => true;
    }
}