using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Sigurd.ClientAPI.ChatCommands
{
    public static class CommandRegistry
    {
        internal static ConfigEntry<string> CommandPrefix { get; set; }

        internal static List<CommandHandler> CommandHandlers { get; set; } = new List<CommandHandler>();

        public static void RegisterAll()
        {
            MethodBase m = new StackTrace().GetFrame(1).GetMethod();
            Assembly assembly = m.ReflectedType.Assembly;
            foreach (Type type in AccessTools.GetTypesFromAssembly(assembly))
            {
                RegisterAll(type);
            }
        }

        public static void RegisterAll(Type type)
        {
            if (type.IsClass && !type.IsAbstract && typeof(CommandHandler).IsAssignableFrom(type))
            {
                CommandHandler commandHandler = (CommandHandler)Activator.CreateInstance(type);

                if (!CommandHandlers.Any(c => c.Name == commandHandler.Name ||
                        (c.Aliases != null && c.Aliases.Contains(commandHandler.Name)) ||
                        (commandHandler.Aliases != null &&
                            commandHandler.Aliases.Any(a => a == c.Name || (c.Aliases != null && c.Aliases.Contains(a)))))
                    )
                {
                    CommandHandlers.Add(commandHandler);
                }
                else
                {
                    Plugin.Log.LogWarning($"Couldn't register command {commandHandler.Name} ({type.FullName}). Another command uses the same name/aliases");
                }
            }
        }

        public static CommandHandler GetCommandHandler(string command)
        {
            return CommandHandlers.FirstOrDefault(c => c.Name == command || (c.Aliases != null && c.Aliases.Any(a => a == command)));
        }

        public static bool TryGetCommandHandler(string command, out CommandHandler commandHandler)
        {
            commandHandler = GetCommandHandler(command);
            return commandHandler != null;
        }
    }
}
