﻿using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Jasily.Windows
{
    public sealed class ClipboardMonitor
    {
        private const int WM_CLIPBOARDUPDATE = 0x031D;

        [DllImport("user32.dll")]
        public static extern bool AddClipboardFormatListener(IntPtr hwnd);
        [DllImport("user32.dll")]
        public static extern bool RemoveClipboardFormatListener(IntPtr hwnd);

        private WindowInteropHelper windowHelper;
        private HwndSource hwndSource;
        private readonly Window handleSource;

        public ClipboardMonitor(Window window)
        {
            this.handleSource = window ?? throw new ArgumentNullException(nameof(window));
        }

        public void Begin()
        {
            this.windowHelper = new WindowInteropHelper(this.handleSource);
            AddClipboardFormatListener(this.windowHelper.Handle);

            this.hwndSource = HwndSource.FromHwnd(this.windowHelper.Handle);
            if (this.hwndSource == null) throw new InvalidOperationException();
            this.hwndSource.AddHook(this.WndProc);
        }

        public void Stop()
        {
            if (this.hwndSource == null) throw new InvalidOperationException();
            this.hwndSource.RemoveHook(this.WndProc);
            this.hwndSource = null;

            RemoveClipboardFormatListener(this.windowHelper.Handle);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_CLIPBOARDUPDATE)
            {
                this.ClipboardUpdated?.Invoke(this);
            }

            return IntPtr.Zero;
        }

        public event EventHandler ClipboardUpdated;
    }
}