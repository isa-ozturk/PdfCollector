using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace PdfCollector.Presentation.Helpers;

public static class MarkdownHelper
{
    public static readonly DependencyProperty MarkdownProperty =
        DependencyProperty.RegisterAttached(
            "Markdown", typeof(string), typeof(MarkdownHelper),
            new PropertyMetadata(null, OnMarkdownChanged));

    public static string GetMarkdown(DependencyObject obj) => (string)obj.GetValue(MarkdownProperty);
    public static void SetMarkdown(DependencyObject obj, string value) => obj.SetValue(MarkdownProperty, value);

    private static void OnMarkdownChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TextBlock tb) return;
        tb.Inlines.Clear();
        if (e.NewValue is not string md || string.IsNullOrWhiteSpace(md)) return;
        foreach (var inline in ParseMarkdown(md))
            tb.Inlines.Add(inline);
    }

    private static IEnumerable<Inline> ParseMarkdown(string md)
    {
        var lines = md.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
        bool firstLine = true;
        bool prevWasEmpty = true;

        foreach (var raw in lines)
        {
            var line = raw.TrimEnd();

            if (string.IsNullOrWhiteSpace(line))
            {
                if (!firstLine) yield return new LineBreak();
                prevWasEmpty = true;
                continue;
            }

            if (!firstLine) yield return new LineBreak();
            firstLine = false;

            // H1
            if (line.StartsWith("# ") && !line.StartsWith("## "))
            {
                if (!prevWasEmpty) yield return new LineBreak();
                var span = new Span { FontSize = 17, FontWeight = FontWeights.Bold };
                foreach (var i in ParseInlines(line.Substring(2))) span.Inlines.Add(i);
                yield return span;
            }
            // H2
            else if (line.StartsWith("## ") && !line.StartsWith("### "))
            {
                if (!prevWasEmpty) yield return new LineBreak();
                var span = new Span
                {
                    FontSize = 14,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = new SolidColorBrush(Color.FromRgb(0x00, 0x78, 0xD4))
                };
                foreach (var i in ParseInlines(line.Substring(3))) span.Inlines.Add(i);
                yield return span;
            }
            // H3
            else if (line.StartsWith("### "))
            {
                var span = new Span { FontSize = 13, FontWeight = FontWeights.SemiBold };
                foreach (var i in ParseInlines(line.Substring(4))) span.Inlines.Add(i);
                yield return span;
            }
            // Bullet
            else if (line.StartsWith("- ") || line.StartsWith("* "))
            {
                yield return new Run("  •  ");
                foreach (var i in ParseInlines(line.Substring(2))) yield return i;
            }
            // Normal
            else
            {
                foreach (var i in ParseInlines(line)) yield return i;
            }

            prevWasEmpty = false;
        }
    }

    private static IEnumerable<Inline> ParseInlines(string text)
    {
        var result = new List<Inline>();
        var sb = new System.Text.StringBuilder();
        int i = 0;

        while (i < text.Length)
        {
            // **bold**
            if (i + 1 < text.Length && text[i] == '*' && text[i + 1] == '*')
            {
                if (sb.Length > 0) { result.Add(new Run(sb.ToString())); sb.Clear(); }
                i += 2;
                var bold = new System.Text.StringBuilder();
                while (i < text.Length && !(i + 1 < text.Length && text[i] == '*' && text[i + 1] == '*'))
                    bold.Append(text[i++]);
                if (i + 1 < text.Length) i += 2;
                result.Add(new Run(bold.ToString()) { FontWeight = FontWeights.SemiBold });
            }
            // `code`
            else if (text[i] == '`')
            {
                if (sb.Length > 0) { result.Add(new Run(sb.ToString())); sb.Clear(); }
                i++;
                var code = new System.Text.StringBuilder();
                while (i < text.Length && text[i] != '`') code.Append(text[i++]);
                if (i < text.Length) i++;
                result.Add(new Run(code.ToString())
                {
                    FontFamily = new FontFamily("Consolas"),
                    Background = new SolidColorBrush(Color.FromRgb(0xED, 0xF2, 0xF8)),
                    Foreground = new SolidColorBrush(Color.FromRgb(0x00, 0x78, 0xD4))
                });
            }
            else
            {
                sb.Append(text[i++]);
            }
        }

        if (sb.Length > 0) result.Add(new Run(sb.ToString()));
        return result;
    }
}
