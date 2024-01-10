namespace Sigurd.ClientAPI.ChatCommands
{
    public abstract class CommandHandler
    {
        public abstract string Name { get; }

        public abstract string[] Aliases { get; }

        public abstract void Handle(string[] arguments);

        public CommandHandler() { }
    }
}
