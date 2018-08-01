using System;
using System.Collections.Generic;
using System.Linq;
using SkillPrestige.Logging;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace SkillPrestige.Commands
{
    /// <summary>
    /// Represents a command called in the SMAPI console interface.
    /// </summary>
    internal abstract class SkillPrestigeCommand
    {
        /// <summary>
        /// The name used to call the command in the console.
        /// </summary>
        private string Name { get; }

        /// <summary>
        /// The help description of the command.
        /// </summary>
        private string Description { get; }

        /// <summary>
        /// Whether or not the command is used only in test mode.
        /// </summary>
        protected abstract bool TestingCommand { get; }

        protected SkillPrestigeCommand(string name, string description, IEnumerable<string> argumentDescriptions = null)
        {
            Name = name;
            Description = description;
            if (argumentDescriptions != null)
            {
                Description += "\n" + string.Join("\n", argumentDescriptions);
            }
        }

        /// <summary>
        /// Applies the effect of a command when it is called from the console.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected abstract void ApplyCommandEffect(string[] arguments);

        /// <summary>
        /// Registers all commands found in the system.
        /// </summary>
        /// <param name="testCommands">Whether or not you wish to only register testing commands.</param>
        public static void RegisterCommands(bool testCommands, IModHelper modHelper)
        {
            var concreteCommands = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypesSafely())
                .Where(x => x.IsSubclassOf(typeof(SkillPrestigeCommand)) && !x.IsAbstract);
            foreach (var commandType in concreteCommands)
            {
                var command = (SkillPrestigeCommand)Activator.CreateInstance(commandType);
                if (!(testCommands ^ command.TestingCommand))
                {
                    Logger.LogInformation($"Registering {command.Name} command...");
                    modHelper.ConsoleCommands.Add(command.Name, command.Description, (cmdText, arguments) =>
                    {
                        command.ApplyCommandEffect(arguments);
                    });
                    Logger.LogInformation($"{command.Name} command registered.");
                }
            }
        }
    }
}
