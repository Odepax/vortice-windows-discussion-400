// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System;
using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using SharpGen.Runtime;
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
		private ID2D1SolidColorBrush backgroundBrush;

		public TestApplication()
            : base(false)
        {
            _d2dFactory = D2D1CreateFactory<ID2D1Factory1>();

            CreateResources();
        }

        public override void Dispose()
        {
            if (backgroundBrush != null) { backgroundBrush.Dispose(); }

            _renderTarget.Dispose();
            _d2dFactory.Dispose();
        }

        private void CreateResources()
        {
            if (_renderTarget != null) { _renderTarget.Dispose(); }
            if (backgroundBrush != null) { backgroundBrush.Dispose(); }

            HwndRenderTargetProperties wtp = new();
            wtp.Hwnd = MainWindow!.Handle;
            wtp.PixelSize = MainWindow!.ClientSize;
            wtp.PresentOptions = PresentOptions.None;
            _renderTarget = _d2dFactory.CreateHwndRenderTarget(
                new RenderTargetProperties(),
                wtp);

            backgroundBrush = _renderTarget.CreateSolidColorBrush(Colors.Red);
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

            _renderTarget.FillRectangle(new RectangleF(8, 8, 64, 64), backgroundBrush);

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
