using tf2translator.Enums;

namespace tf2translator.Models
{
    public abstract class Command : Log
    {
        public ChatType ChatType { get; }
        public string Name { get; }
        protected string Message { get; }
        protected string LangParam { get; }
        public bool Executed { get; set; }

        protected Command(string rawLog, ChatType chatType, string name, string message, string langParam = null) : base(rawLog)
        {
            ChatType = chatType;
            Name = name;
            Message = message;
            LangParam = langParam;
            Executed = false;
        }


        public virtual void Execute() { }
    }
}
