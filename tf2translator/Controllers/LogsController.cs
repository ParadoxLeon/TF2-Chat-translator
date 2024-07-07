﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using tf2translator.Enums;
using tf2translator.EventArgs;
using tf2translator.Exceptions;
using tf2translator.Models;

namespace tf2translator.Controllers
{
    internal static class LogsController
    {
        public static LinkedList<Log> Logs { get; set; }
        public static List<Chat> Chats { get; set; }

        public static async Task LoadLogsAsync(int amount)
        {
            await Task.Run(() => LoadLogs(amount));
        }

        /**
         * <summary>
         *  Loads the amount rows from the console.log file. stores the output in Chats & Commands
         * </summary>
         * <param name="amount"></param>
        */
        private static void LoadLogs(int amount)
        {
            var logs = new LinkedList<Log>();

            List<string> lines;
            try
            {
                lines = GetLastLines(amount);
                MainWindow.Succeeded(null, new TranslatorExceptionEventArgs { Exception = new LogfileNotFoundException() });
            }
            catch (TranslatorException e)
            {
                MainWindow.ErrorEncountered(null, new TranslatorExceptionEventArgs { Exception = e });
                return;
            }

            if (lines.Count != 0)
            {
                var (rawStrings, chatTypes, names, rawMessage) = LineCleaner(lines);

                for (var i = 0; i < rawStrings.Count; i++)
                {  
                        logs.AddLast(new Chat(rawStrings[i], chatTypes[i], names[i], rawMessage[i]));
                }

                /* if logs where found in the file. */
                if(logs.Last != null)
                {
                    var addList = new List<Log>();
                    
                    /*
                        loop backwards over the array with the discovered logs.
                        Check for each node if the node is identical to the last added node in the system.
                        When the last added node was found, add all nodes that were looped over already because they are new.
                    */
                    for (var node = logs.Last; node != null; node = node.Previous)
                    {
                        if(Logs.Last == null)
                        {
                            addList.Insert(0, node.Value);
                        }
                        else if (!Compare(Logs.Last, node))
                        {
                            addList.Insert(0, node.Value);
                        }
                        else
                        {
                            break;
                        }
                    }
                    
                    foreach(var log in addList)
                    {
                        SaveLog(log);
                    }
                }
            }

            static void SaveLog(Log log)
            {
                Logs.AddLast(log);
                switch (log)
                {
                    case Chat chat:

                        Chats.Insert(0, chat);

                        if (chat.Translation.Message == chat.Message || chat.Translation.Message == "---") return;                        
                        
                        break;
                }
            }
            
            /*
             * Compares 2 nodes based on value and if necessary, the 3 previous logs.
             */
            static bool Compare(LinkedListNode<Log> lastAddedNode, LinkedListNode<Log> newNode)
            {
                var lastAddedNodeRelative = lastAddedNode;
                var newNodeRelative = newNode;

                if (lastAddedNodeRelative.Value.RawString != newNodeRelative.Value.RawString) return false;
                
                for (var i = 0; i < 3; i++)
                {
                    lastAddedNodeRelative = lastAddedNodeRelative.Previous;
                    newNodeRelative = newNodeRelative.Previous;

                    if (lastAddedNodeRelative == null || newNodeRelative == null) return true;
                    if (lastAddedNodeRelative.Value.RawString != newNodeRelative.Value.RawString) return false;
                }
                
                return true;
            }
        }

        /*
         * Reads the last <param name="amount"></param> lines from the console.log file.
         */
        private static List<string> GetLastLines(int amount)
        {
            string logFilePath = $@"{Properties.Settings.Default.Path}\tf\console.log";
            List<string> consoleLines = new List<string>();

            // Define the maximum number of retries
            int maxRetries = 3;

            for (int retry = 0; retry < maxRetries; retry++)
            {
                try
                {
                    using (var fs = new FileStream(logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        if (fs.Length == 0)
                        {
                            // File is empty, return an empty list
                            return consoleLines;
                        }

                        var count = 0;
                        var buffer = new byte[1];

                        fs.Seek(0, SeekOrigin.End);

                        while (count < amount)
                        {
                            fs.Seek(-1, SeekOrigin.Current);
                            fs.Read(buffer, 0, 1);
                            if (buffer[0] == '\n') count++;

                            // fs.Read() advances the position, so we need to go back again
                            fs.Seek(-1, SeekOrigin.Current);
                        }

                        // Go past the last '\n'
                        fs.Seek(1, SeekOrigin.Current);

                        using (var sr = new StreamReader(fs))
                        {
                            string line;
                            while ((line = sr.ReadLine()) != null)
                            {
                                consoleLines.Add(line);
                            }
                        }

                        return consoleLines;
                    }
                }
                catch (IOException ex)
                {
                    if (retry < maxRetries - 1)
                    {
                        // Wait for a short time before retrying (adjust the sleep duration as needed)
                        Thread.Sleep(1000);
                    }
                    else
                    {
                        // Handle the exception after exhausting retries
                        Console.WriteLine($"Error reading the file: {ex.Message}");
                    }
                }
            }

            return consoleLines;
        }

        /*
         * <summary>
         * Helper function for LoadLogs
         * function that checks takes a list with console.log lines and attempts to find chat messages in them.
         * </summary>
         * <param name="lines">List of </param>
         * <returns>A tuple with 4 lists of cleaned and validated data.</returns>
         */
        private static (List<string> rawStrings, List<ChatType> chatTypes, List<string> names, List<string> messages) LineCleaner(IEnumerable<string> lines)
        {
            var returnRawStrings = new List<string>();
            var returnChatTypes = new List<ChatType>();
            var returnNames = new List<string>();
            var returnMessages = new List<string>();

            foreach (var l in lines)
            {
                /* filter out the massage */
                if (!l.Contains(" :  "))
                    continue;

                var temp = l.Split(new[] { " :  " }, 2, StringSplitOptions.None);

                var chatType = ChatType.All;
                var namePart = temp[0].Trim();
                var messagePart = temp[1].Trim();

                // use regular expression to remove the date and time pattern
                namePart = Regex.Replace(namePart, @"\d{1,2}/\d{1,2} \d{1,2}:\d{1,2}:\d{1,2}", "").Trim();

                //Comment when testing
                if (namePart.StartsWith("(TEAM)"))
                    namePart = namePart.Substring(6).Trim();


                /* removal of [DEAD] */
                if (namePart.EndsWith("[DEAD]"))
                    namePart = namePart.Substring(0, namePart.Length - 6).Trim();
              
                returnRawStrings.Add(l);
                returnChatTypes.Add(chatType);
                returnNames.Add(namePart);
                returnMessages.Add(messagePart);
            }

            return (rawStrings: returnRawStrings, chatTypes: returnChatTypes, names: returnNames, messages: returnMessages);
        }
    }
}
