using Avalonia;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;

namespace TypefaceUtil.Avalonia.Controls
{
    public class SKPathDrawOperation : ICustomDrawOperation
    {
        private readonly SkiaSharp.SKPath? _path;
        private readonly SkiaSharp.SKPaint? _paint;

        public SKPathDrawOperation(Rect bounds, SkiaSharp.SKPath? path, SkiaSharp.SKPaint? paint)
        {
            _path = path;
            _paint = paint;
            Bounds = bounds;
        }

        public void Dispose()
        {
        }

        public Rect Bounds { get; }

        public bool HitTest(Point p) => Bounds.Contains(p);

        public bool Equals(ICustomDrawOperation? other) => false;

        public void Render(IDrawingContextImpl context)
        {
            var canvas = (context as ISkiaDrawingContextImpl)?.SkCanvas;
            if (canvas is null || _path is null)
            {
                return;
            }

            canvas.Save();
            canvas.DrawPath(_path, _paint);
            canvas.Restore();
        }
    }
}
