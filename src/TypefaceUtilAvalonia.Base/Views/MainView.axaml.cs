using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace TypefaceUtil.Avalonia.Views;

public partial class MainView : UserControl
{
    private bool _isDragging;
    private Control? _dragControl;
    private Point _startPoint;
    private TranslateTransform? _dragTransform;

    public MainView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void GlyphView_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        _dragControl = sender as Control;
        if (_dragControl is { })
        {
            if (_dragTransform is null)
            {
                _dragTransform = new TranslateTransform();
                _dragControl.RenderTransform = _dragTransform;
            }
            _startPoint = e.GetPosition(this) - new Point(_dragTransform.X, _dragTransform.Y);
            _isDragging = true;
        }
    }

    private void GlyphView_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_isDragging && _dragControl is { })
        {
            _isDragging = false;
            _dragControl = null;
        }
    }

    private void GlyphView_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_isDragging && _dragControl is { } && _dragTransform is { })
        {
            var currentPoint = e.GetPosition(this);
            var delta = currentPoint - _startPoint;
            _dragTransform.X = delta.X;
            _dragTransform.Y = delta.Y;
        }
    }
}

