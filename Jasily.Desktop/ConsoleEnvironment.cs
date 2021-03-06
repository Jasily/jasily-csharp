﻿using System;

namespace Jasily
{
    public class ConsoleEnvironment : IDisposable
    {
        private readonly ConsoleColor foregroundColor;
        private readonly ConsoleColor backgroundColor;

        public ConsoleEnvironment()
        {
            this.foregroundColor = Console.ForegroundColor;
            this.backgroundColor = Console.BackgroundColor;
        }

        public void Dispose()
        {
            Console.ForegroundColor = this.foregroundColor;
            Console.BackgroundColor = this.backgroundColor;
        }
    }
}