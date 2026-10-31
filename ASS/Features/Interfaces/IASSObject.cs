namespace ASS.Features.Interfaces
{
    using LabApi.Features.Console;

    public interface IASSObject
    {
        public string Key { get; }

        // parent can only go from: null -> null, obj -> null, null -> obj. You can't switch the parent of an object.
        public IASSContainer? Parent { get; protected set; }

        public bool DisplayInHierarchy { get; }

        public bool TrySetParent(IASSContainer? parent, bool overwrite = true)
        {
            if (Parent is null || parent is null)
            {
                Parent = parent;
                return true;
            }

            if (Parent is not null && overwrite)
            {
                Logger.Error($"Invalid Operation: Something tried to set the parent of {ToString()} to {parent} despite {ToString()} already having parent {Parent}");
                return false;
            }

            IASSContainer? oldParent = Parent;
            Parent = parent;

            ASSExtensions.PreviousObjectsForParent.Clear();
            ASSExtensions.PreviousObjectsForParent.Add(this);
            IASSContainer? temp = Parent;

            while (temp is not null)
            {
                if (!ASSExtensions.PreviousObjectsForParent.Add(temp))
                {
                    Parent = oldParent;
                    return false;
                }

                temp = temp.Parent;
            }

            return true;
        }
    }
}