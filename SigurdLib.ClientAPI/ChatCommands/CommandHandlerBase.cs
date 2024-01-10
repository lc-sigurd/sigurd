namespace Sigurd.ClientAPI.ChatCommands
{
    /// <summary>
    /// A command handler which should be extended to register commands.
    /// </summary>
    public abstract class CommandHandler
    {
        /// <summary>
        /// The name of the command. This is the main entry-point to the command.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Aliases for the command, may be <see langword="null" />.
        /// </summary>
        public abstract string[] Aliases { get; }

        /// <summary>
        /// The actual handler for the command.
        /// </summary>
        /// <param name="arguments">An array of space-delimited arguments.</param>
        public abstract void Handle(string[] arguments);

#pragma warning disable
        public CommandHandler() { }
#pragma warning restore
    }
}
