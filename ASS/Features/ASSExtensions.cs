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
        private static readonly List<string> PathStack = [];
        private static readonly List<IASSObject> PathStackInitializer = [];
        private static readonly SHA256 Hasher = SHA256.Create();
        private static readonly StringBuilder BuilderNonAlloc = new();

        public static HashSet<IASSObject> IgnoredObjects { get; } = [];

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
            Logger.Debug("Building Settings", Main.Debug);

            PreviousObjectsStack.Clear();

            List<ASSInstance> builtSettings = ListPool<ASSInstance>.Shared.Rent();
            foreach (IASSObject obj in root)
            {
                // properly initialize Path Stack, regardless of wherever the provided object is from.
                if (obj.Parent is not null)
                {
                    Logger.Debug("Initializing Path Stack", Main.Debug);

                    // we use a list for easy reversing + less heap allocation over time.
                    PathStackInitializer.Clear();

                    IASSObject? parent = obj.Parent;

                    while (parent is not null)
                    {
                        PathStackInitializer.Add(parent);
                        parent = parent.Parent;
                    }

                    PathStackInitializer.Reverse();

                    foreach (IASSObject path in PathStackInitializer)
                    {
                        if (IgnoredObjects.Contains(path))
                            continue;

                        PathStack.Add(path.Key);
                    }
                }

                Process(obj);

                PathStack.Clear();
            }

            return builtSettings;

            // a DFS on visible settings.
            void Process(IASSObject obj)
            {
                Logger.Debug($"Processing {obj}", Main.Debug);
                if (obj is ASSTemplate template)
                {
                    int id = template.OverrideId ?? GenerateId(PathStack, template.Key);

                    if (Main.Debug)
                    {
                        Logger.Debug("Generating ID from path:");
                        foreach (string path in PathStack)
                            Logger.Debug(path);

                        Logger.Debug(template.Key);
                    }

                    builtSettings.Add(template.CreateInstance(viewer, id));
                    return;
                }

                if (obj is not IASSContainer container)
                {
                    Logger.Warn($"Unknown ASS object {obj}!");
                    return;
                }

                if (PreviousObjectsStack.Contains(obj))
                    throw new InvalidOperationException("ASS objects cannot reference themselves within their children.");

                bool useStack = !IgnoredObjects.Contains(obj);
                if (useStack)
                    PathStack.Add(container.Key);

                PreviousObjectsStack.Push(obj);

                foreach (IASSObject child in container.GetChildren(viewer))
                    Process(child);

                if (useStack)
                    PathStack.RemoveAt(PathStack.Count - 1);

                PreviousObjectsStack.Pop();
            }
        }
    }
}