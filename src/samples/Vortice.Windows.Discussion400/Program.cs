// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System;
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
        private ID2D1DeviceContext _deviceContext;

        private Vortice.Mathematics.Color4 bgcolor = new(0.1f, 0.1f, 0.1f, 1.0f);
        private ID2D1SolidColorBrush redBrush;
        private ID2D1SolidColorBrush greenBrush;
        private ID2D1BitmapRenderTarget OffscreenBuffer;
        private TextHightlightShader TextHightlightShader;

        public TestApplication()
            : base(false)
        {
            _d2dFactory = D2D1CreateFactory<ID2D1Factory1>();
            TextHightlightShader.Register(_d2dFactory);

            CreateResources();
        }

        public override void Dispose()
        {
            if (redBrush != null) { redBrush.Dispose(); }
            if (greenBrush != null) { greenBrush.Dispose(); }
            if (OffscreenBuffer != null) { OffscreenBuffer.Dispose(); }
            if (TextHightlightShader != null) { TextHightlightShader.Dispose(); }

            _deviceContext.Dispose();
            _renderTarget.Dispose();
            _d2dFactory.Dispose();
        }

        private void CreateResources()
        {
            if (_deviceContext != null) { _deviceContext.Dispose(); }
            if (_renderTarget != null) { _renderTarget.Dispose(); }
            if (redBrush != null) { redBrush.Dispose(); }
            if (greenBrush != null) { greenBrush.Dispose(); }
            if (OffscreenBuffer != null) { OffscreenBuffer.Dispose(); }
            if (TextHightlightShader != null) { TextHightlightShader.Dispose(); }

            HwndRenderTargetProperties wtp = new();
            wtp.Hwnd = MainWindow!.Handle;
            wtp.PixelSize = MainWindow!.ClientSize;
            wtp.PresentOptions = PresentOptions.None;
            _renderTarget = _d2dFactory.CreateHwndRenderTarget(
                new RenderTargetProperties(),
                wtp);
            _deviceContext = _renderTarget.QueryInterface<ID2D1DeviceContext>();

            redBrush = _renderTarget.CreateSolidColorBrush(Colors.Red);
            greenBrush = _renderTarget.CreateSolidColorBrush(Colors.Green);
            OffscreenBuffer = _renderTarget.CreateCompatibleRenderTarget(
                desiredSize: new SizeF(64, 64),
                desiredPixelSize: new Size(64, 64),
                desiredFormat: _renderTarget.PixelFormat,
                options: CompatibleRenderTargetOptions.None
            );
            TextHightlightShader = new(_deviceContext);
            TextHightlightShader.SetInput(0, OffscreenBuffer.Bitmap, invalidate: true); // 'Dis useless actually: the shader doesn't use its input.
                                                                                        // But hey, one step at a time, right?
        }

        protected override void InitializeBeforeRun()
        {
        }

        protected override void OnKeyboardEvent(KeyboardKey key, bool pressed)
        {
        }

        protected override void OnDraw(int width, int height)
        {
            GC.Collect();                                                            // Forcing garbage collection is essential to reproduce the bug.
                                                                                     // The sample doesn't trigger garbage collection in a timely manner otherwise...

            _renderTarget.BeginDraw();
            _renderTarget.Clear(bgcolor);

            _renderTarget.FillRectangle(new RectangleF(8, 8, 64, 64), redBrush);     // The red rectangle is a normal draw.

            OffscreenBuffer.BeginDraw();
            OffscreenBuffer.Clear(Colors.Transparent);
            OffscreenBuffer.FillRectangle(new RectangleF(0, 0, 64, 64), greenBrush); // The green rectangle is a draw from an offscreen canvas.
            OffscreenBuffer.EndDraw();

            _renderTarget.DrawBitmap(                                                // The green rectangle is a draw from an offscreen canvas.
                bitmap: OffscreenBuffer.Bitmap,
                opacity: 1,
                interpolationMode: BitmapInterpolationMode.Linear,
                sourceRectangle: new RectangleF(0, 0, 64, 64),
                destinationRectangle: new RectangleF(8 + 64 + 8, 8, 64, 64)
            );

            _deviceContext.DrawImage(                                                // The blue rectangle is a draw from the shader.
                image: TextHightlightShader.Output,
                compositeMode: CompositeMode.SourceOver,
                interpolationMode: InterpolationMode.NearestNeighbor,
                targetOffset: new Vector2(8 + 64 + 8 + 64 + 8, 8)
            );

            if (_renderTarget.EndDraw() == Vortice.Direct2D1.ResultCode.RecreateTarget)
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
