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
        public static string StringJoin<T>(string separator, IEnumerable<T> elements)
        {
#if DOTNET35
            return string.Join(separator, elements.Select(e => e != null ? e.ToString() : null).ToArray());
#else
		    return string.Join(separator, elements);
#endif
        }

        public static IEnumerable<U> SafeCast<T, U>(this IEnumerable<T> elements)
            where T : class, U
            where U : class
        {
#if DOTNET35
            foreach (T item in elements)
                yield return item;
#else
		    return elements;
#endif
        }

        public static Predicate<U> SafeCast<T, U>(this Predicate<T> predicate)
            where U : class, T
            where T : class
        {
#if DOTNET35
            return e => predicate(e);
#else
		    return predicate;
#endif
        }

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