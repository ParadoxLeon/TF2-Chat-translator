using System;

namespace tf2translator.Exceptions
{
    public class LogfileNotFoundException : TranslatorException
    {
        public LogfileNotFoundException() : base("Logfile not found") { }
    }
}