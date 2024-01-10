using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Sigurd.ClientAPI.ChatCommands
{
    /// <summary>
    /// Handles registering and unregistering command handlers.
    /// </summary>
    public static class CommandRegistry
    {
        internal static ConfigEntry<string> CommandPrefix { get; set; }

        internal static List<CommandHandler> CommandHandlers { get; set; } = new List<CommandHandler>();

        /// <summary>
        /// Registers all command handlers from the caller's <see cref="Assembly"/>.
        /// </summary>
        public static void RegisterAll()
        {
            MethodBase m = new StackTrace().GetFrame(1).GetMethod();
            Assembly assembly = m.ReflectedType.Assembly;
            foreach (Type type in AccessTools.GetTypesFromAssembly(assembly))
            {
                Register(type);
            }
        }

        /// <summary>
        /// Registers the command handler at the given <see cref="Type"/>, if it is one.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> to register as a command handler. Must extend <see cref="CommandHandler"/>.</param>
        public static void Register(Type type)
        {
            if (type.IsClass && !type.IsAbstract && typeof(CommandHandler).IsAssignableFrom(type))
            {
                CommandHandler commandHandler = (CommandHandler)Activator.CreateInstance(type);

                if (!TryGetCommandHandler(commandHandler.Name, out CommandHandler _) &&
                    (commandHandler.Aliases == null || !commandHandler.Aliases.Any(a => TryGetCommandHandler(a, out CommandHandler _))))
                {
                    CommandHandlers.Add(commandHandler);
                }
                else
                {
                    Plugin.Log.LogWarning($"Couldn't register command {commandHandler.Name} ({type.FullName}). Another command uses the same name/aliases");
                }
            }
        }

        /// <summary>
        /// Unregisters all command handlers from the caller's <see cref="Assembly"/>.
        /// </summary>
        public static void UnregisterAll()
        {
            MethodBase m = new StackTrace().GetFrame(1).GetMethod();
            Assembly assembly = m.ReflectedType.Assembly;
            foreach (Type type in AccessTools.GetTypesFromAssembly(assembly))
            {
                Unregister(type);
            }
        }

        /// <summary>
        /// Unregisters the command handler at the given <see cref="Type"/>, if it is one.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> to unregister.</param>
        public static void Unregister(Type type)
        {
            if (type.IsClass && !type.IsAbstract && typeof(CommandHandler).IsAssignableFrom(type))
            {
                foreach (CommandHandler commandHandler in CommandHandlers)
                {
                    if (commandHandler.GetType() == type)
                    {
                        CommandHandlers.Remove(commandHandler);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Gets a <see cref="CommandHandler"/> with the given name or alias.
        /// </summary>
        /// <param name="command">The name or alias to get the <see cref="CommandHandler"/> of.</param>
        /// <returns>The <see cref="CommandHandler"/>, or <see langword="null" /> if it doesn't exist.</returns>
        public static CommandHandler GetCommandHandler(string command)
        {
            return CommandHandlers.FirstOrDefault(c => c.Name == command || (c.Aliases != null && c.Aliases.Any(a => a == command)));
        }

        /// <summary>
        /// Tries to get a <see cref="CommandHandler"/> with the given name or alias.
        /// </summary>
        /// <param name="command">The name or alias to get the <see cref="CommandHandler"/> of.</param>
        /// <param name="commandHandler">Outputs a <see cref="CommandHandler"/>, or <see langword="null" /> if it doesn't exist.</param>
        /// <returns><see langword="true" /> if a <see cref="CommandHandler"/> is found, <see langword="false" /> otherwise.</returns>
        public static bool TryGetCommandHandler(string command, out CommandHandler commandHandler)
        {
            commandHandler = GetCommandHandler(command);
            return commandHandler != null;
        }
    }
}
