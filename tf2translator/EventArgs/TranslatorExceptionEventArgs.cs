using System;
using tf2translator.Exceptions;

namespace tf2translator.EventArgs
{
    public class TranslatorExceptionEventArgs : System.EventArgs
    {
        public TranslatorException Exception;
    }
}