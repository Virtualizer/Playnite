using System;
using System.Windows;
using System.Windows.Input;

namespace Playnite.FullscreenApp.Markup
{
    public static class MouseExt
    {
        public static void SafeOverrideCursor(Cursor cursor)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                Mouse.OverrideCursor = cursor;
            }));
        }
    }
}
