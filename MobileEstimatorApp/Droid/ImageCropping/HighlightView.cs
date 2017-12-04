using System;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Support.V4.Content;
using Android.Views;
using Plugin.CurrentActivity;

namespace OnePosInventory.Droid
{
	public class HighlightView
	{
		private readonly View _context;

		public enum ModifyMode
		{
			None,
			Move,
			Grow
		}

		private ModifyMode _mode = ModifyMode.None;

		private RectF _imageRect;
		private RectF _cropRect;
		public Matrix MatrixImage;

		private bool _maintainAspectRatio;
		private float _initialAspectRatio;

		private Drawable _resizeDrawableWidth;
		private Drawable _resizeDrawableHeight;

		private readonly Paint _focusPaint = new Paint();
		private readonly Paint _noFocusPaint = new Paint();
		private readonly Paint _outlinePaint = new Paint();

		[Flags]
		public enum HitPosition
		{
			None,
			GrowLeftEdge,
			GrowRightEdge,
			GrowTopEdge,
			GrowBottomEdge,
			Move
		}

		public HighlightView(View ctx)
		{
			_context = ctx;
		}

		public bool Focused
		{
			get;
			set;
		}

		public bool Hidden
		{
			get;
			set;
		}

		public Rect DrawRect
		{
			get;
			private set;
		}
		public Rect CropRect => new Rect((int)_cropRect.Left, (int)_cropRect.Top,
			(int)_cropRect.Right, (int)_cropRect.Bottom);

		public ModifyMode Mode
		{
			get
			{
				return _mode;
			}
			set
			{
				if (value != _mode)
				{
					_mode = value;
					_context.Invalidate();
				}
			}
		}

		public void HandleMotion(HitPosition edge, float dx, float dy)
		{
			var r = ComputeLayout();
			switch (edge)
			{
				case HitPosition.None:
					return;
				case HitPosition.Move:

					MoveBy(dx * (_cropRect.Width() / r.Width()),
						dy * (_cropRect.Height() / r.Height()));
					break;
				default:
					if (!edge.HasFlag(HitPosition.GrowLeftEdge) && !edge.HasFlag(HitPosition.GrowRightEdge))
					{
						dx = 0;
					}

					if (!edge.HasFlag(HitPosition.GrowTopEdge) && !edge.HasFlag(HitPosition.GrowBottomEdge))
					{
						dy = 0;
					}

					var xDelta = dx * (_cropRect.Width() / r.Width());
					var yDelta = dy * (_cropRect.Height() / r.Height());

					GrowBy((edge.HasFlag(HitPosition.GrowLeftEdge) ? -1 : 1) * xDelta,
						(edge.HasFlag(HitPosition.GrowTopEdge) ? -1 : 1) * yDelta);
					break;
			}
		}

		public void Draw(Canvas canvas)
		{
			if (Hidden)
			{
				return;
			}

			canvas.Save();


			if (!Focused)
			{
				_outlinePaint.Color = Color.White;
				canvas.DrawRect(DrawRect, _outlinePaint);
			}
			else
			{
				var viewDrawingRect = new Rect();
				_context.GetDrawingRect(viewDrawingRect);

				_outlinePaint.Color = Color.White;
				_focusPaint.Color = new Color(50, 50, 50, 125);

				var path = new Path();
				path.AddRect(new RectF(DrawRect), Path.Direction.Cw);

				canvas.ClipPath(path, Region.Op.Difference);
				canvas.DrawRect(viewDrawingRect, _focusPaint);

				canvas.Restore();
				canvas.DrawPath(path, _outlinePaint);

				if (_mode == ModifyMode.Grow)
				{
					var left = DrawRect.Left + 1;
					var right = DrawRect.Right + 1;
					var top = DrawRect.Top + 4;
					var bottom = DrawRect.Bottom + 3;

					var widthWidth = _resizeDrawableWidth.IntrinsicWidth / 2;
					var widthHeight = _resizeDrawableWidth.IntrinsicHeight / 2;
					var heightHeight = _resizeDrawableHeight.IntrinsicHeight / 2;
					var heightWidth = _resizeDrawableHeight.IntrinsicWidth / 2;

					var xMiddle = DrawRect.Left + ((DrawRect.Right - DrawRect.Left) / 2);
					var yMiddle = DrawRect.Top + ((DrawRect.Bottom - DrawRect.Top) / 2);

					_resizeDrawableWidth.SetBounds(left - widthWidth,
												   yMiddle - widthHeight,
												   left + widthWidth,
												   yMiddle + widthHeight);
					_resizeDrawableWidth.Draw(canvas);

					_resizeDrawableWidth.SetBounds(right - widthWidth,
												   yMiddle - widthHeight,
												   right + widthWidth,
												   yMiddle + widthHeight);
					_resizeDrawableWidth.Draw(canvas);

					_resizeDrawableHeight.SetBounds(xMiddle - heightWidth,
													top - heightHeight,
													xMiddle + heightWidth,
													top + heightHeight);
					_resizeDrawableHeight.Draw(canvas);

					_resizeDrawableHeight.SetBounds(xMiddle - heightWidth,
													bottom - heightHeight,
													xMiddle + heightWidth,
													bottom + heightHeight);
					_resizeDrawableHeight.Draw(canvas);
				}
			}
		}

		public HitPosition GetHit(float x, float y)
		{
			var r = ComputeLayout();
			const float hysteresis = 20F;
			var retval = HitPosition.None;

			var verticalCheck = (y >= r.Top - hysteresis) && (y < r.Bottom + hysteresis);
			var horizCheck = (x >= r.Left - hysteresis) && (x < r.Right + hysteresis);


			if ((Math.Abs(r.Left - x) < hysteresis) && verticalCheck)
			{
				retval |= HitPosition.GrowLeftEdge;
			}

			if ((Math.Abs(r.Right - x) < hysteresis) && verticalCheck)
			{
				retval |= HitPosition.GrowRightEdge;
			}

			if ((Math.Abs(r.Top - y) < hysteresis) && horizCheck)
			{
				retval |= HitPosition.GrowTopEdge;
			}

			if ((Math.Abs(r.Bottom - y) < hysteresis) && horizCheck)
			{
				retval |= HitPosition.GrowBottomEdge;
			}


			if (retval == HitPosition.None && r.Contains((int)x, (int)y))
			{
				retval = HitPosition.Move;
			}

			return retval;
		}

		public void Invalidate()
		{
			DrawRect = ComputeLayout();
		}

		public void Setup(Matrix m, Rect imageRect, RectF cropRect, bool maintainAspectRatio)
		{
			MatrixImage = new Matrix(m);

			_cropRect = cropRect;
			_imageRect = new RectF(imageRect);
			_maintainAspectRatio = maintainAspectRatio;

			_initialAspectRatio = cropRect.Width() / cropRect.Height();
			DrawRect = ComputeLayout();

			_focusPaint.SetARGB(125, 50, 50, 50);
			_noFocusPaint.SetARGB(125, 50, 50, 50);
			_outlinePaint.StrokeWidth = 3;
			_outlinePaint.SetStyle(Paint.Style.Stroke);
			_outlinePaint.AntiAlias = true;

			_mode = ModifyMode.None;
			Init();
		}

		private void Init()
		{
			_resizeDrawableWidth = ContextCompat.GetDrawable(CrossCurrentActivity.Current.Activity, Resource.Drawable.camera_crop_width);
			_resizeDrawableHeight = ContextCompat.GetDrawable(CrossCurrentActivity.Current.Activity, Resource.Drawable.camera_crop_height);
		}

		private void MoveBy(float dx, float dy)
		{
			var invalRect = new Rect(DrawRect);

			_cropRect.Offset(dx, dy);

			_cropRect.Offset(
				Math.Max(0, _imageRect.Left - _cropRect.Left),
				Math.Max(0, _imageRect.Top - _cropRect.Top));

			_cropRect.Offset(
				Math.Min(0, _imageRect.Right - _cropRect.Right),
				Math.Min(0, _imageRect.Bottom - _cropRect.Bottom));

			DrawRect = ComputeLayout();
			invalRect.Union(DrawRect);
			invalRect.Inset(-10, -10);
			_context.Invalidate(invalRect);
		}

		private void GrowBy(float dx, float dy)
		{
			if (_maintainAspectRatio)
			{
				if (Math.Abs(dx) > double.Epsilon)
				{
					dy = dx / _initialAspectRatio;
				}
				else if (Math.Abs(dy) > double.Epsilon)
				{
					dx = dy * _initialAspectRatio;
				}
			}
			var r = new RectF(_cropRect);
			if (dx > 0F && r.Width() + 2 * dx > _imageRect.Width())
			{
				var adjustment = (_imageRect.Width() - r.Width()) / 2F;
				dx = adjustment;
				if (_maintainAspectRatio)
				{
					dy = dx / _initialAspectRatio;
				}
			}
			if (dy > 0F && r.Height() + 2 * dy > _imageRect.Height())
			{
				var adjustment = (_imageRect.Height() - r.Height()) / 2F;
				dy = adjustment;
				if (_maintainAspectRatio)
				{
					dx = dy * _initialAspectRatio;
				}
			}

			r.Inset(-dx, -dy);

			var widthCap = 25F;
			if (r.Width() < widthCap)
			{
				r.Inset(-(widthCap - r.Width()) / 2F, 0F);
			}
			var heightCap = _maintainAspectRatio
				? (widthCap / _initialAspectRatio)
					: widthCap;
			if (r.Height() < heightCap)
			{
				r.Inset(0F, -(heightCap - r.Height()) / 2F);
			}

			if (r.Left < _imageRect.Left)
			{
				r.Offset(_imageRect.Left - r.Left, 0F);
			}
			else if (r.Right > _imageRect.Right)
			{
				r.Offset(-(r.Right - _imageRect.Right), 0);
			}
			if (r.Top < _imageRect.Top)
			{
				r.Offset(0F, _imageRect.Top - r.Top);
			}
			else if (r.Bottom > _imageRect.Bottom)
			{
				r.Offset(0F, -(r.Bottom - _imageRect.Bottom));
			}

			_cropRect.Set(r);
			DrawRect = ComputeLayout();
			_context.Invalidate();
		}

		private Rect ComputeLayout()
		{
			var r = new RectF(_cropRect.Left, _cropRect.Top,
								_cropRect.Right, _cropRect.Bottom);
			MatrixImage.MapRect(r);
			return new Rect((int)Math.Round(r.Left), (int)Math.Round(r.Top),
							(int)Math.Round(r.Right), (int)Math.Round(r.Bottom));
		}
	}
}