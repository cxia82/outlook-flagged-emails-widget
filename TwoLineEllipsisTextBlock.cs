using System.Globalization;
using System.Windows;
using System.Windows.Media;
using WpfControl = System.Windows.Controls.Control;
using WpfSize = System.Windows.Size;
using WpfPoint = System.Windows.Point;

namespace NotificationWidget
{
    public class TwoLineEllipsisTextBlock : WpfControl
    {
        private FormattedText? _cachedFormattedText;
        private double _cachedMaxWidth = -1;

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
                nameof(Text), typeof(string), typeof(TwoLineEllipsisTextBlock),
                new FrameworkPropertyMetadata(string.Empty,
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.AffectsRender));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        protected override WpfSize MeasureOverride(WpfSize constraint)
        {
            double width = double.IsInfinity(constraint.Width) ? 1000 : Math.Max(0, constraint.Width);
            var ft = GetOrCreateFormattedText(width);
            return new WpfSize(Math.Min(width, ft.Width), ft.Height);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            if (ActualWidth > 0)
                drawingContext.DrawText(GetOrCreateFormattedText(ActualWidth), new WpfPoint(0, 0));
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == TextProperty ||
                e.Property == FontFamilyProperty ||
                e.Property == FontStyleProperty ||
                e.Property == FontWeightProperty ||
                e.Property == FontStretchProperty ||
                e.Property == FontSizeProperty ||
                e.Property == ForegroundProperty ||
                e.Property == FlowDirectionProperty)
            {
                InvalidateFormattedTextCache();
            }
        }

        private FormattedText GetOrCreateFormattedText(double maxWidth)
        {
            double normalizedWidth = Math.Max(1, maxWidth);

            if (_cachedFormattedText != null && Math.Abs(_cachedMaxWidth - normalizedWidth) < 0.1)
                return _cachedFormattedText;

            _cachedMaxWidth = normalizedWidth;
            _cachedFormattedText = CreateFormattedText(normalizedWidth);
            return _cachedFormattedText;
        }

        private void InvalidateFormattedTextCache()
        {
            _cachedFormattedText = null;
            _cachedMaxWidth = -1;
        }

        private FormattedText CreateFormattedText(double maxWidth)
        {
            var ft = new FormattedText(
                Text ?? string.Empty,
                CultureInfo.CurrentUICulture,
                FlowDirection,
                new Typeface(FontFamily, FontStyle, FontWeight, FontStretch),
                FontSize,
                Foreground,
                VisualTreeHelper.GetDpi(this).PixelsPerDip)
            {
                MaxTextWidth = Math.Max(1, maxWidth),
                MaxLineCount = 2,
                Trimming = TextTrimming.CharacterEllipsis
            };

            return ft;
        }
    }
}
