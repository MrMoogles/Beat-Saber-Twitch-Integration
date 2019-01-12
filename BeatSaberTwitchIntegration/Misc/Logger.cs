﻿using System;
using System.IO;
using System.Reflection;

namespace TwitchIntegrationPlugin.Misc
{
    class Logger
    {
#if DEBUG
        private static StreamWriter logWriter = new StreamWriter("BeatSaberTwitchBot.log") { AutoFlush = true };
#endif

        private static string _assemblyName;
        public static string AssemblyName
        {
            get
            {
                if (string.IsNullOrEmpty(_assemblyName))
                    _assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

                return _assemblyName;
            }
        }

        private static ConsoleColor _lastColor;

        public static void Log(object message)
        {
            _lastColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("[" + AssemblyName + " | LOG] " + message);
            Console.ForegroundColor = _lastColor;
#if DEBUG
            logWriter.WriteLine("[" + AssemblyName + " | LOG] " + message);
#endif
        }

        public static void Warning(object message)
        {
            _lastColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("[" + AssemblyName + " | WARNING] " + message);
            Console.ForegroundColor = _lastColor;
#if DEBUG
            logWriter.WriteLine("[" + AssemblyName + " | WARNING] " + message);
#endif
        }

        public static void Error(object message)
        {
            _lastColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("[" + AssemblyName + " | ERROR] " + message);
            Console.ForegroundColor = _lastColor;
#if DEBUG
            logWriter.WriteLine("[" + AssemblyName + " | ERROR] " + message);
#endif
        }

        public static void Exception(object message)
        {
            _lastColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[" + AssemblyName + " | CRITICAL] " + message);
            Console.ForegroundColor = _lastColor;
#if DEBUG
            logWriter.WriteLine("[" + AssemblyName + " | CRITICAL] " + message);
#endif
        }
    }
}