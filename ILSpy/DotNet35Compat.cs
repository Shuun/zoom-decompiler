using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace ICSharpCode.ILSpy
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
                    UseLayoutRoundingProperty.AddOwner(typeof(DotNet35Compat));
#endif

            TextFormattingModeProperty =
#if DOTNET35
                DependencyProperty.RegisterAttached(
                    "TextFormattingMode",
                    typeof(TextFormattingMode),
                    typeof(DotNet35Compat));
#else
                TextOptions.TextFormattingModeProperty.AddOwner(typeof(DotNet35Compat));
#endif
        }
    }
}

#if DOTNET35
namespace System
{
    public sealed class Tuple<T1, T2>
    {
        public readonly T1 Item1;
        public readonly T2 Item2;

        public Tuple(T1 item1, T2 item2)
        {
            this.Item1 = item1;
            this.Item2 = item2;
        }
    }

    public sealed class Tuple<T1, T2, T3>
    {
        public readonly T1 Item1;
        public readonly T2 Item2;
        public readonly T3 Item3;

        public Tuple(T1 item1, T2 item2, T3 item3)
        {
            this.Item1 = item1;
            this.Item2 = item2;
            this.Item3 = item3;
        }
    }

    public static class Tuple
    {
        public static Tuple<T1, T2> Create<T1, T2>(T1 item1, T2 item2)
        {
            return new Tuple<T1, T2>(item1, item2);
        }
    }
}
#endif