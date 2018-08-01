using System;
using System.Collections.Generic;
using StardewModdingAPI.Events;

namespace SkillPrestige.Commands
{
    /// <summary>
    /// Represents the command that sets experience levels for a player.
    /// </summary>
    /// // ReSharper disable once UnusedMember.Global - referenced via reflection
    internal class CheckForModCommand : SkillPrestigeCommand
    {

        public CheckForModCommand() : base("checkformod", "Checks for the existence of a mod | checkformod <namespace> <modname>", GetArgumentDescriptions()) { }

        private static IEnumerable<string> GetArgumentDescriptions()
        {
            return new[]
            {
                "<namespace> The namespace of the mod <modname> The name of the mod"
            };
        }

        protected override bool TestingCommand => true;

        protected override void ApplyCommandEffect(string[] args)
        {
            if (args.Length <= 1)
            {
                SkillPrestigeMod.LogMonitor.Log("<namespace> and <modname> must be specified");
                return;
            }
            var namespaceArgument = args[0];
            var modNameArgument = args[1];
            SkillPrestigeMod.LogMonitor.Log($"mod {modNameArgument} {(Type.GetType($"{namespaceArgument}.{modNameArgument}, {namespaceArgument}") == null ? "not " : string.Empty)}found.");
        }
    }
}
