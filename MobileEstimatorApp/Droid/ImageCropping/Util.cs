using System;
using Android.Graphics;

namespace OnePosInventory.Droid
{
	public class Util
	{
		public static Bitmap RotateImage(Bitmap b, int degrees)
		{
			if (degrees != 0 && b != null)
			{
				var m = new Matrix();
				m.SetRotate(degrees,
						(float)b.Width / 2, (float)b.Height / 2);
				try
				{
					var b2 = Bitmap.CreateBitmap(
							b, 0, 0, b.Width, b.Height, m, true);
					if (b != b2)
					{
						b.Recycle();
						b = b2;
					}
				}
				catch (Java.Lang.OutOfMemoryError)
				{

				}
			}

			return b;
		}

		public static Bitmap Transform(Matrix scaler,
									   Bitmap source,
									   int targetWidth,
									   int targetHeight,
									   bool scaleUp)
		{
			var deltaX = source.Width - targetWidth;
			var deltaY = source.Height - targetHeight;

			if (!scaleUp && (deltaX < 0 || deltaY < 0))
			{
				var b2 = Bitmap.CreateBitmap(targetWidth, targetHeight,
												Bitmap.Config.Argb8888);
				var c = new Canvas(b2);

				var deltaXHalf = Math.Max(0, deltaX / 2);
				var deltaYHalf = Math.Max(0, deltaY / 2);

				var src = new Rect(
					deltaXHalf,
					deltaYHalf,
					deltaXHalf + Math.Min(targetWidth, source.Width),
					deltaYHalf + Math.Min(targetHeight, source.Height));

				var dstX = (targetWidth - src.Width()) / 2;
				var dstY = (targetHeight - src.Height()) / 2;

				var dst = new Rect(
					dstX,
					dstY,
					targetWidth - dstX,
					targetHeight - dstY);

				c.DrawBitmap(source, src, dst, null);
				return b2;
			}

			float bitmapWidthF = source.Width;
			float bitmapHeightF = source.Height;

			var bitmapAspect = bitmapWidthF / bitmapHeightF;
			var viewAspect = (float)targetWidth / targetHeight;

			if (bitmapAspect > viewAspect)
			{
				var scale = targetHeight / bitmapHeightF;
				if (scale < .9F || scale > 1F)
				{
					scaler.SetScale(scale, scale);
				}
				else
				{
					scaler = null;
				}
			}
			else
			{
				var scale = targetWidth / bitmapWidthF;

				if (scale < .9F || scale > 1F)
				{
					scaler.SetScale(scale, scale);
				}
				else
				{
					scaler = null;
				}
			}

			Bitmap b1;

			if (scaler != null)
			{
				b1 = Bitmap.CreateBitmap(source, 0, 0,
										 source.Width, source.Height, scaler, true);
			}
			else
			{
				b1 = source;
			}

			var dx1 = Math.Max(0, b1.Width - targetWidth);
			var dy1 = Math.Max(0, b1.Height - targetHeight);

			var b3 = Bitmap.CreateBitmap(
				b1,
				dx1 / 2,
				dy1 / 2,
				targetWidth,
				targetHeight);

			if (b1 != source)
			{
				b1.Recycle();
			}

			return b3;
		}
	}
}