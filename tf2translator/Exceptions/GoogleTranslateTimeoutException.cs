namespace tf2translator.Exceptions
{
    public class GoogleTranslateTimeoutException : TranslatorException
    {
        public GoogleTranslateTimeoutException() : base("Google translate timeout") { }
    }
}