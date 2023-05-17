// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using Vortice;
using Vortice.DCommon;
using Vortice.Direct2D1;
using Vortice.DirectWrite;
using Vortice.Mathematics;
using static Vortice.Direct2D1.D2D1;
using static Vortice.DirectWrite.DWrite;

namespace Vortice.Windows.Discussion400;

public static class Program
{
    private class TestApplication : Application
    {
        private readonly ID2D1Factory1 _d2dFactory;
        private ID2D1HwndRenderTarget _renderTarget;

        private Vortice.Mathematics.Color4 bgcolor = new(0.1f, 0.1f, 0.1f, 1.0f);
        private ID2D1SolidColorBrush redBrush;
        private ID2D1SolidColorBrush greenBrush;
        private ID2D1BitmapRenderTarget OffscreenBuffer;

        // Effect<TextHightlightShader> TextHightlightShader;
        // public GameContent() => TextHightlightShader = new(context => new(context)) { Inputs = new[] { () => OffscreenBuffer.Resource.Bitmap } };

        public TestApplication()
            : base(false)
        {
            _d2dFactory = D2D1CreateFactory<ID2D1Factory1>();

            CreateResources();
        }

        public override void Dispose()
        {
            if (redBrush != null) { redBrush.Dispose(); }
            if (greenBrush != null) { greenBrush.Dispose(); }
            if (OffscreenBuffer != null) { OffscreenBuffer.Dispose(); }

            _renderTarget.Dispose();
            _d2dFactory.Dispose();
        }

        private void CreateResources()
        {
            if (_renderTarget != null) { _renderTarget.Dispose(); }
            if (redBrush != null) { redBrush.Dispose(); }
            if (greenBrush != null) { greenBrush.Dispose(); }
            if (OffscreenBuffer != null) { OffscreenBuffer.Dispose(); }

            HwndRenderTargetProperties wtp = new();
            wtp.Hwnd = MainWindow!.Handle;
            wtp.PixelSize = MainWindow!.ClientSize;
            wtp.PresentOptions = PresentOptions.None;
            _renderTarget = _d2dFactory.CreateHwndRenderTarget(
                new RenderTargetProperties(),
                wtp);

            redBrush = _renderTarget.CreateSolidColorBrush(Colors.Red);
            greenBrush = _renderTarget.CreateSolidColorBrush(Colors.Green);
            OffscreenBuffer = _renderTarget.CreateCompatibleRenderTarget(
                desiredSize: new SizeF(64, 64),
                desiredPixelSize: new Size(64, 64),
                desiredFormat: _renderTarget.PixelFormat,
                options: CompatibleRenderTargetOptions.None
            );
        }

        protected override void InitializeBeforeRun()
        {
        }

        protected override void OnKeyboardEvent(KeyboardKey key, bool pressed)
        {
        }

        protected override void OnDraw(int width, int height)
        {
            _renderTarget.BeginDraw();
            _renderTarget.Clear(bgcolor);

            _renderTarget.FillRectangle(new RectangleF(8, 8, 64, 64), redBrush);

            OffscreenBuffer.BeginDraw();
            OffscreenBuffer.Clear(Colors.Transparent);
            OffscreenBuffer.FillRectangle(new RectangleF(0, 0, 64, 64), greenBrush);
            OffscreenBuffer.EndDraw();

			_renderTarget.DrawBitmap(
                bitmap: OffscreenBuffer.Bitmap,
                opacity: 1,
                interpolationMode: BitmapInterpolationMode.Linear,
                sourceRectangle: new RectangleF(0, 0, 64, 64),
                destinationRectangle: new RectangleF(8 + 64 + 8, 8, 64, 64)
            );

			// _renderTarget.DrawImage(
			//     image: TextHightlightShader.Resource.Output,
			//     compositeMode: CompositeMode.SourceOver,
			//     interpolationMode: InterpolationMode.NearestNeighbor,
			//     targetOffset: new Vector2(Font_ttf.MeasureWidth(text) + 8 - 8, lineIndex * lineHeight - 8)
			// );

			try
            {
                _renderTarget.EndDraw();
            }
            catch
            {
                CreateResources();
            }
        }
    }

    public static void Main()
    {
        using var app = new TestApplication();
        app.Run();
    }
}
