using System;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace MobileEstimatorApp.Droid
{
    public abstract class ImageViewTouchBase : ImageView
    {
        protected Matrix baseMatrix = new Matrix();
        protected Matrix suppMatrix = new Matrix();
        private Matrix displayMatrix = new Matrix();

        private float[] matrixValues = new float[9];

        protected RotateBitmap bitmapDisplayed = new RotateBitmap(null);

        private int thisWidth = -1;
        private int thisHeight = -1;
        const float SCALE_RATE = 1.25F;

        private float maxZoom;

        private Handler handler = new Handler();

        private Action onLayoutRunnable = null;


        protected ImageViewTouchBase(Context context)
            : base(context)
        {
            Init();
        }

        protected ImageViewTouchBase(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
            Init();
        }

        private void Init()
        {
            SetScaleType(ImageView.ScaleType.Matrix);
        }

        private void setImageBitmap(Bitmap bitmap, int rotation)
        {
            base.SetImageBitmap(bitmap);
            var d = Drawable;
            d?.SetDither(true);

            Bitmap old = bitmapDisplayed.Bitmap;
            bitmapDisplayed.Bitmap = bitmap;
            bitmapDisplayed.Rotation = rotation;
        }

        public void Clear()
        {
            SetImageBitmapResetBase(null, true);
        }
        public void SetImageBitmapResetBase(Bitmap bitmap, bool resetSupp)
        {
            SetImageRotateBitmapResetBase(new RotateBitmap(bitmap), resetSupp);
        }

        public void SetImageRotateBitmapResetBase(RotateBitmap bitmap, bool resetSupp)
        {
            int viewWidth = Width;

            if (viewWidth <= 0)
            {
                onLayoutRunnable = () =>
                {
                    SetImageRotateBitmapResetBase(bitmap, resetSupp);
                };

                return;
            }

            if (bitmap.Bitmap != null)
            {
                GetProperBaseMatrix(bitmap, baseMatrix);
                setImageBitmap(bitmap.Bitmap, bitmap.Rotation);
            }
            else
            {
                baseMatrix.Reset();
                base.SetImageBitmap(null);
            }

            if (resetSupp)
            {
                suppMatrix.Reset();
            }
            ImageMatrix = GetImageViewMatrix();
            this.maxZoom = CalculateMaxZoom();
        }


        protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
        {
            this.IvLeft = left;
            this.IvRight = right;
            this.IvTop = top;
            this.IvBottom = bottom;

            thisWidth = right - left;
            thisHeight = bottom - top;

            var r = onLayoutRunnable;

            if (r != null)
            {
                onLayoutRunnable = null;
                r();
            }

            if (bitmapDisplayed.Bitmap != null)
            {
                GetProperBaseMatrix(bitmapDisplayed, baseMatrix);
                ImageMatrix = GetImageViewMatrix();
            }
        }

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            if (keyCode == Keycode.Back && GetScale() > 1.0f)
            {
                ZoomTo(1.0f);
                return true;
            }

            return base.OnKeyDown(keyCode, e);
        }

        public override void SetImageBitmap(Bitmap bm)
        {
            setImageBitmap(bm, 0);
        }

        public int IvLeft { get; private set; }

        public int IvRight { get; private set; }

        public int IvTop { get; private set; }

        public int IvBottom { get; private set; }

        protected float GetValue(Matrix matrix, int whichValue)
        {
            matrix.GetValues(matrixValues);
            return matrixValues[whichValue];
        }

        protected float GetScale(Matrix matrix)
        {
            return GetValue(matrix, Matrix.MscaleX);
        }

        protected float GetScale()
        {
            return GetScale(suppMatrix);
        }

        private void GetProperBaseMatrix(RotateBitmap bitmap, Matrix matrix)
        {
            float viewWidth = Width;
            float viewHeight = Height;

            float w = bitmap.Width;
            float h = bitmap.Height;
            int rotation = bitmap.Rotation;
            matrix.Reset();

            float widthScale = Math.Min(viewWidth / w, 2.0f);
            float heightScale = Math.Min(viewHeight / h, 2.0f);
            float scale = Math.Min(widthScale, heightScale);

            matrix.PostConcat(bitmap.GetRotateMatrix());
            matrix.PostScale(scale, scale);

            matrix.PostTranslate(
                (viewWidth - w * scale) / 2F,
                (viewHeight - h * scale) / 2F);
        }

        protected Matrix GetImageViewMatrix()
        {
            displayMatrix.Set(baseMatrix);
            displayMatrix.PostConcat(suppMatrix);

            return displayMatrix;
        }

        protected float CalculateMaxZoom()
        {
            if (bitmapDisplayed.Bitmap == null)
            {
                return 1F;
            }

            float fw = (float)bitmapDisplayed.Width / (float)thisWidth;
            float fh = (float)bitmapDisplayed.Height / (float)thisHeight;
            float max = Math.Max(fw, fh) * 4;

            return max;
        }

        protected virtual void ZoomTo(float scale, float centerX, float centerY)
        {
            if (scale > maxZoom)
            {
                scale = maxZoom;
            }

            float oldScale = GetScale();
            float deltaScale = scale / oldScale;

            suppMatrix.PostScale(deltaScale, deltaScale, centerX, centerY);
            ImageMatrix = GetImageViewMatrix();
            Center(true, true);
        }

        protected void ZoomTo(float scale, float centerX,
                               float centerY, float durationMs)
        {
            float incrementPerMs = (scale - GetScale()) / durationMs;
            float oldScale = GetScale();

            long startTime = System.Environment.TickCount;

            Action anim = null;

            anim = () =>
            {
                long now = System.Environment.TickCount;
                float currentMs = Math.Min(durationMs, now - startTime);
                float target = oldScale + (incrementPerMs * currentMs);
                ZoomTo(target, centerX, centerY);

                if (currentMs < durationMs)
                {
                    handler.Post(anim);
                }
            };

            handler.Post(anim);
        }

        protected void ZoomTo(float scale)
        {
            float cx = Width / 2F;
            float cy = Height / 2F;

            ZoomTo(scale, cx, cy);
        }

        protected virtual void ZoomIn()
        {
            ZoomIn(SCALE_RATE);
        }

        protected virtual void ZoomOut()
        {
            ZoomOut(SCALE_RATE);
        }

        protected virtual void ZoomIn(float rate)
        {
            if (GetScale() >= maxZoom)
            {
                return;
            }

            if (bitmapDisplayed.Bitmap == null)
            {
                return;
            }

            float cx = Width / 2F;
            float cy = Height / 2F;

            suppMatrix.PostScale(rate, rate, cx, cy);
            ImageMatrix = GetImageViewMatrix();
        }

        protected void ZoomOut(float rate)
        {
            if (bitmapDisplayed.Bitmap == null)
            {
                return;
            }

            float cx = Width / 2F;
            float cy = Height / 2F;

            Matrix tmp = new Matrix(suppMatrix);
            tmp.PostScale(1F / rate, 1F / rate, cx, cy);

            if (GetScale(tmp) < 1F)
            {
                suppMatrix.SetScale(1F, 1F, cx, cy);
            }
            else
            {
                suppMatrix.PostScale(1F / rate, 1F / rate, cx, cy);
            }

            ImageMatrix = GetImageViewMatrix();
            Center(true, true);
        }

        protected virtual void PostTranslate(float dx, float dy)
        {
            suppMatrix.PostTranslate(dx, dy);
        }

        protected void PanBy(float dx, float dy)
        {
            PostTranslate(dx, dy);
            ImageMatrix = GetImageViewMatrix();
        }

        protected void Center(bool horizontal, bool vertical)
        {
            if (bitmapDisplayed.Bitmap == null)
            {
                return;
            }

            Matrix m = GetImageViewMatrix();

            RectF rect = new RectF(0, 0,
                                   bitmapDisplayed.Bitmap.Width,
                                   bitmapDisplayed.Bitmap.Height);

            m.MapRect(rect);

            float height = rect.Height();
            float width = rect.Width();

            float deltaX = 0, deltaY = 0;

            if (vertical)
            {
                int viewHeight = Height;
                if (height < viewHeight)
                {
                    deltaY = (viewHeight - height) / 2 - rect.Top;
                }
                else if (rect.Top > 0)
                {
                    deltaY = -rect.Top;
                }
                else if (rect.Bottom < viewHeight)
                {
                    deltaY = Height - rect.Bottom;
                }
            }

            if (horizontal)
            {
                int viewWidth = Width;
                if (width < viewWidth)
                {
                    deltaX = (viewWidth - width) / 2 - rect.Left;
                }
                else if (rect.Left > 0)
                {
                    deltaX = -rect.Left;
                }
                else if (rect.Right < viewWidth)
                {
                    deltaX = viewWidth - rect.Right;
                }
            }

            PostTranslate(deltaX, deltaY);
            ImageMatrix = GetImageViewMatrix();
        }
    }
}

