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
#if DOTNET35
                DependencyProperty.RegisterAttached(
                    "UseLayoutRounding",
                    typeof(bool),
                    typeof(DotNet35Compat),
                    new PropertyMetadata(false, (sender, e) => ((FrameworkElement)sender).SnapsToDevicePixels = (bool)e.NewValue));
#else
                FrameworkElement.UseLayoutRoundingProperty.AddOwner(typeof(DotNet35Compat));
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


#if DOTNET35
        public static void AddRange<TBase, TDerived>(this List<TBase> items, IEnumerable<TDerived> addItems)
            where TDerived : TBase
        {
            if (addItems is IEnumerable<TBase>)
                items.AddRange((IEnumerable<TBase>)addItems);
            else
                items.AddRange(addItems.OfType<TDerived>());
        }
#endif

#if DOTNET35
        public static void CopyTo(this System.IO.Stream source, System.IO.Stream target)
        {
            while (true)
            {
                int b = source.ReadByte();
                if (b < 0)
                    return;
                target.WriteByte((byte)b);
            }
        }
#endif

#if DOTNET35
        public static void Restart(this System.Diagnostics.Stopwatch sw)
        {
            if(sw.IsRunning)
                sw.Stop();

            sw.Start();
        }
#endif

        public static string PathCombine(params string[] parts)
        {
#if DOTNET35
            return parts.Aggregate(System.IO.Path.Combine);
#else
            return System.IO.Path.Combine(parts);
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

namespace System.Runtime.CompilerServices
{
    internal class ConditionalWeakTable<K,V> : Dictionary<K,V>
    {

    }
}
#endif