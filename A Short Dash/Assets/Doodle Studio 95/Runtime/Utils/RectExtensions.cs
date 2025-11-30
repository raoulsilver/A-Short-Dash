using UnityEngine;

namespace DoodleStudio95 {
internal static class RectExtensions {

	internal static Rect ScaleCentered(this Rect rect, float amountX, float amountY) {
		var center = new Vector2(rect.center.x, rect.center.y);
		if (amountX >= -1 && amountX <= 1) amountX = rect.width * (1-amountX);
		if (amountY >= -1 && amountY <= 1) amountY = rect.height * (1-amountY);
		rect.width = rect.width - amountX;
		rect.height = rect.height - amountY;
		rect.center = center;
		return rect;
	}
	internal static Rect ScaleCentered(this Rect rect, float amount) {
		return rect.ScaleCentered(amount, amount);
	}
	// Gets an array of rects that are distributed evenly and centered on the source rect
	internal static Rect[] GetEvenRectsHorizontal(this Rect sourcerect, int items, Vector2? margin = null, bool square = false) {
		if (margin == null) margin = Vector2.zero;
		var rs = new Rect[items];
		var size = new Vector2(sourcerect.width / (float)items - margin.GetValueOrDefault().x * 2, sourcerect.height - margin.GetValueOrDefault().y * 2);
		if (square) {
			size.x = size.y = sourcerect.height - margin.GetValueOrDefault().x;
		}
		for(int i = 0; i < items; i++) {
			rs[i] = new Rect(0, 0, size.x, size.y);
			rs[i].center = new Vector2(sourcerect.x + sourcerect.width * ((float)(i+1) / (float)items) - size.x * .5f, sourcerect.center.y);
		}
		return rs;
	}

	internal static Rect[] GetEvenRectsHorizontalNoAlloc(this Rect sourcerect, ref Rect[] rects, int items, Vector2? margin = null, bool square = false) {
		if (margin == null) margin = Vector2.zero;
		System.Array.Resize(ref rects, items);
		var rs = rects;
		var size = new Vector2(sourcerect.width / (float)items - margin.GetValueOrDefault().x * 2, sourcerect.height - margin.GetValueOrDefault().y * 2);
		if (square) {
			size.x = size.y = sourcerect.height - margin.GetValueOrDefault().x;
		}
		for(int i = 0; i < items; i++) {
			rs[i] = new Rect(0, 0, size.x, size.y);
			rs[i].center = new Vector2(sourcerect.x + sourcerect.width * ((float)(i+1) / (float)items) - size.x * .5f, sourcerect.center.y);
		}
		return rs;
	}

	// Returns a rect that fits in the target while mantaining aspect ratio
	internal static Rect ScaleToFit(this Rect rect, Rect target) {
		Rect r = Rect.zero;
		float aspectWidth = target.width / rect.width;
		float aspectHeight = target.height / rect.height;
		float aspectRatio = Mathf.Min(aspectWidth, aspectHeight);

		r.width = rect.width * aspectRatio;
		r.height = rect.height * aspectRatio;
		// r.x = (target.width - r.width) / 2.0f;
		// r.y = (target.height - r.height) / 2.0f;
		r.center = new Vector2(target.center.x, target.center.y);
		return r;
	}

	internal static Rect Offset(this Rect rect, float x = 0, float y = 0, float width = 0, float height = 0) {
		rect.x += x;
		rect.y += y;
		rect.width += width;
		rect.height += height;
		return rect;
	}

	internal static Rect CropToFit(this Rect rect, Rect target) {
		if (rect.x < 0) {
      int d = (int)Mathf.Abs(rect.x);
      rect.x = 0;
      rect.width -= d;
    }
		if (rect.y < 0) {
      int d = (int)Mathf.Abs(rect.y);
      rect.y = 0;
      rect.height -= d;
    }
		if (rect.x + rect.width >= target.width) {
      int d = (int)(rect.x + rect.width - target.width);
      rect.width -= d;
    }
		if (rect.y + rect.height >= target.height) {
      int d = (int)(rect.y + rect.height - target.height);
      rect.height -= d;
    }
		return rect;
	}

	internal static bool Contains(this Rect rect, float x, float y) { return rect.Contains(new Vector2(x, y)); }

}
}