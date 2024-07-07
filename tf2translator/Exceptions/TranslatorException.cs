using System;

namespace tf2translator.Exceptions
{
    public class TranslatorException : Exception
    {
        public override string Message { get; }
        
        protected TranslatorException(string message)
        {
            Message = message;
        }
    }
}