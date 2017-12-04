using Android.Graphics;

namespace OnePosInventory.Droid
{
	public class RotateBitmap
	{
		public const string TAG = "RotateBitmap";

		public RotateBitmap(Bitmap bitmap)
		{
			Bitmap = bitmap;
		}

		public RotateBitmap(Bitmap bitmap, int rotation)
		{
			Bitmap = bitmap;
			Rotation = rotation % 360;
		}

		public int Rotation
		{
			get;
			set;
		}

		public Bitmap Bitmap
		{
			get;
			set;
		}

		public Matrix GetRotateMatrix()
		{
			var matrix = new Matrix();

			if (Rotation != 0)
			{
				var cx = Bitmap.Width / 2;
				var cy = Bitmap.Height / 2;
				matrix.PreTranslate(-cx, -cy);
				matrix.PostRotate(Rotation);
				matrix.PostTranslate(Width / 2, Height / 2);
			}

			return matrix;
		}

		public bool IsOrientationChanged
		{
			get
			{
				return (Rotation / 90) % 2 != 0;
			}
		}

		public int Height
		{
			get
			{
				if (IsOrientationChanged)
				{
					return Bitmap.Width;
				}
				else
				{
					return Bitmap.Height;
				}
			}
		}

		public int Width
		{
			get
			{
				if (IsOrientationChanged)
				{
					return Bitmap.Height;
				}
				else
				{
					return Bitmap.Width;
				}
			}
		}
	}
}