﻿namespace tf2translator.Models
{
    public class Log
    {
        public string RawString { get; }

        protected Log(string rawString)
        {
            RawString = rawString;
        }
    }
}
