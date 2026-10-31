namespace ASS.DebugUtils
{
    extern alias oldSystem;

    using System.Collections.Generic;
    using System.Reflection;
    using System.Text;

    using ASS.Features;
    using UnityEngine;
    using StackTrace = System.Diagnostics.StackTrace;

    // logs stack traces when Record(); gets called too much. Nice for lettings devs know what method is making ASS call SendToPlayer a gazillion times.
    internal class SpamDetector
    {
        private static readonly StringBuilder Builder = new();

        private readonly int threshold;
        private readonly long windowTicks;
        private readonly string methodName;
        private readonly HashSet<string> seenStacks = [];

        private int windowStart;
        private int count;

        public SpamDetector(string methodName, int threshold, long windowTicks)
        {
            this.methodName = methodName;
            this.threshold = threshold;
            this.windowTicks = windowTicks;
        }

        // 10 times per second seems suspicious.
        public static SpamDetector SendToPlayerTracker { get; } = new(nameof(ASSNetworking.SendCustomToPlayer), 10, 60);

        public void Record()
        {
            int now = Time.frameCount;

            if (now - windowStart > windowTicks)
            {
                windowStart = now;
                count = 0;
            }

            count++;

            if (count < threshold)
                return;

            string key = CaptureStack();
            if (seenStacks.Add(key))
                LabApi.Features.Console.Logger.Warn($"Suspicious spam detected for {methodName}. Trace:\n{key}");
        }

        private static string CaptureStack(int maxFrames = 10)
        {
            Builder.Clear();

            StackTrace trace = new(2, false);

            for (int i = Mathf.Min(trace.FrameCount, maxFrames) - 1; i >= 0; i--)
            {
                MethodBase? method = trace.GetFrame(i)?.GetMethod();
                if (method is null)
                    continue;

                Builder
                    .Append("-> ")
                    .Append(method.DeclaringType?.FullName)
                    .Append('.')
                    .AppendLine(method.Name);
            }

            return Builder.ToString();
        }
    }
}