using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace ICSharpCode.TreeView
{
    internal static class DotNet35Compat
    {
#if DOTNET35
        public enum TextFormattingMode
        {
            Ideal,
            Display
        }
#endif

        public static readonly DependencyProperty UseLayoutRoundingProperty;
        public static bool GetUseLayoutRounding(DependencyObject obj) { return (bool)obj.GetValue(UseLayoutRoundingProperty); }
        public static void SetUseLayoutRounding(DependencyObject obj, bool value) { obj.SetValue(UseLayoutRoundingProperty, value); }

        public static readonly DependencyProperty TextFormattingModeProperty;
        public static TextFormattingMode GetTextFormattingMode(DependencyObject obj) { return (TextFormattingMode)obj.GetValue(TextFormattingModeProperty); }
        public static void SetTextFormattingMode(DependencyObject obj, TextFormattingMode value) { obj.SetValue(TextFormattingModeProperty, value); }

        static DotNet35Compat()
        {
            UseLayoutRoundingProperty =
                FrameworkElement.
#if DOTNET35
                    SnapsToDevicePixelsProperty;
#else
                    UseLayoutRoundingProperty;
#endif

            TextFormattingModeProperty =
#if DOTNET35
                DependencyProperty.RegisterAttached(
                    "TextFormattingMode",
                    typeof(TextFormattingMode),
                    typeof(DotNet35Compat));
#else
                TextOptions.TextFormattingModeProperty;
#endif
        }

    }
}