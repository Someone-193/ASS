namespace ASS.Features
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;

    using ASS.Features.Instances;
    using ASS.Features.Interfaces;
    using ASS.Features.Templates;

    using LabApi.Features.Console;
    using LabApi.Features.Wrappers;

    using NorthwoodLib.Pools;

    public static class ASSExtensions
    {
        private static readonly Stack<IASSObject> PreviousObjectsStack = new();
        private static readonly Stack<string> PathStack = new();
        private static readonly List<string> PathStackInitializer = [];
        private static readonly SHA256 Hasher = SHA256.Create();
        private static readonly StringBuilder BuilderNonAlloc = new();

        internal static HashSet<IASSObject> PreviousObjectsForParent { get; } = [];

        public static int GenerateId(params string[] path)
        {
            BuilderNonAlloc.Clear();

            for (int i = 0; i < path.Length; i++)
                BuilderNonAlloc.Append(path[i].Length).Append(':').Append(path[i]);

            string combined = BuilderNonAlloc.ToString().Normalize(NormalizationForm.FormC);

            byte[] hash = Hasher.ComputeHash(Encoding.UTF8.GetBytes(combined));
            return BitConverter.ToInt32(hash, 0);
        }

        public static int GenerateId(IEnumerable<string> path)
        {
            BuilderNonAlloc.Clear();

            foreach (string part in path)
                BuilderNonAlloc.Append(part.Length).Append(':').Append(part);

            string combined = BuilderNonAlloc.ToString().Normalize(NormalizationForm.FormC);

            byte[] hash = Hasher.ComputeHash(Encoding.UTF8.GetBytes(combined));
            return BitConverter.ToInt32(hash, 0);
        }

        public static int GenerateId(IEnumerable<string> path, string key) => GenerateId(path.Append(key));

        // internal because it returns a pooled list.
        internal static List<ASSInstance> BuildSettings(IEnumerable<IASSObject> root, Player viewer)
        {
            PreviousObjectsStack.Clear();

            List<ASSInstance> builtSettings = ListPool<ASSInstance>.Shared.Rent();
            foreach (IASSObject obj in root)
            {
                // properly initialize PathStack, regardless of wherever the provided object is from.
                if (obj.Parent is not null)
                {
                    // we use a list for easy reversing + less heap allocation over time.
                    PathStackInitializer.Clear();

                    IASSObject? parent = obj.Parent;

                    while (parent is not null)
                    {
                        PathStackInitializer.Add(parent.Key);
                        parent = parent.Parent;
                    }

                    PathStackInitializer.Reverse();
                    foreach (string key in PathStackInitializer)
                        PathStack.Push(key);
                }

                Process(obj);

                PathStack.Clear();
            }

            return builtSettings;

            // a DFS on visible settings.
            void Process(IASSObject obj)
            {
                if (obj is not IASSContainer container)
                    return;

                if (PreviousObjectsStack.Contains(obj))
                    throw new InvalidOperationException("ASS objects cannot reference themselves within their children.");

                PathStack.Push(container.Key);
                PreviousObjectsStack.Push(obj);

                foreach (IASSObject child in container.GetChildren(viewer))
                {
                    if (child is ASSTemplate template)
                    {
                        int id;
                        if (template.OverrideId.HasValue)
                            id = template.OverrideId.Value;
                        else if (template.Key is not null)
                            id = GenerateId(PathStack, template.Key);
                        else
                        {
                            Logger.Error("Failed to send settings.\nAll visible ASS Templates must have a usable ID when sending settings!");
                            continue;
                        }

                        builtSettings.Add(template.CreateInstance(viewer, id));
                    }
                    else
                        Process(child);
                }

                PathStack.Pop();
                PreviousObjectsStack.Pop();
            }
        }
    }
}