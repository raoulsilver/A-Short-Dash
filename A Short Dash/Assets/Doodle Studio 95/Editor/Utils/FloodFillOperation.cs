using System.Collections.Generic;
using UnityEngine;

namespace DoodleStudio95 {
[System.Serializable]
internal class FloodFillOperation {
	internal enum Type {
      Normal,
      NormalReflected,
      Random,
			RightToLeftSlow,
			Growing
    }
    [SerializeField] List<Vector2> stack;
    [SerializeField] Color32 prevColor;
    [SerializeField] Color32 newColor;
    [SerializeField] SerializableTexture2D Layer;
    [SerializeField] Type type;
		[SerializeField] Vector2 originalPoint;
		[SerializeField] bool repeatX;
		[SerializeField] bool repeatY;

		internal SerializableTexture2D Target { get { return Layer; } }

    static Vector2[] _neighborsOriginal = new Vector2[4] {
      new Vector2(-1, 0),
      new Vector2(0, -1),
      new Vector2(1, 0),
      new Vector2(0, 1)
    };
		Vector2[] neighbors = new Vector2[4];

		static Gradient RAINBOW;

		float _t = 0;

    internal FloodFillOperation(SerializableTexture2D Layer, int x, int y, Color32 prevColor, Color32 newColor, Type type = Type.Normal,
			bool repeatX = false, bool repeatY = false
		) {
        this.Layer = Layer;
        this.prevColor = prevColor;
        this.newColor = newColor;
        this.type = type;
				this.originalPoint = new Vector2(x,y);
				this.repeatX = repeatX;
				this.repeatY = repeatY;
        stack = new List<Vector2>();
        stack.Add(new Vector2(x, y));
    }

		internal bool Advance(int Times = 8000) {
			// Normal fill does it immediately
			/* 
			if (type == Type.Normal) {
				while(stack.Count > 0)
					Next(false);
				return false;
			}
			*/

			if (type == Type.Random) 
				Times = 40;
			else if (type == Type.RightToLeftSlow) 
				Times = 40;
			else if (type == Type.Growing) 
				Times = 20;

			bool remaining = stack.Count > 0;
			if (remaining) {
				int n = Times;
				while (n-- > 0) {
					remaining = Next(n == 0);
				}
			}
			return remaining;
		}

		void HandleRainbowColor(Vector2 p)
		{
			if (!DrawWindow.m_Instance || !DrawWindow.m_Instance.m_FloodFillRainbow)
				return;

			if (RAINBOW == null) {
				RAINBOW = new Gradient();
				var colors = new Color32[] {
					Color.red, Color.yellow, Color.green, Color.cyan, Color.blue, Color.magenta
				};
				var colorKeys = new GradientColorKey[colors.Length];
				var alphaKeys = new GradientAlphaKey[colors.Length];
				for(int i = 0 ; i < colors.Length; i++) {
					colorKeys[i].color = colors[i];
					colorKeys[i].time = alphaKeys[i].time = (float)i / (float)colors.Length;
					alphaKeys[i].alpha = 1;
				}
				RAINBOW.SetKeys(colorKeys, alphaKeys);
			}

			if (type == Type.Random)
				newColor = RAINBOW.Evaluate(Mathf.Repeat(_t / 2000f, 1));
			else if (type == Type.Growing || type == Type.RightToLeftSlow)
				newColor = RAINBOW.Evaluate(Mathf.Repeat(_t / 4000f, 1));
			else
				newColor = RAINBOW.Evaluate(Mathf.Repeat((p.x / Layer.width) * (p.y / Layer.height), 1));
		}

    bool Next(bool Apply = false) {
      if (stack.Count == 0 || Layer == null || Layer.texture == null || _t > Layer.width * Layer.height * 4)
        return false;
      
      var idx = stack.Count - 1;
      var p = stack[idx]; 
      stack.RemoveAt(idx);
			HandleRainbowColor(p);
      Layer.SetPixelFast((int)p.x, (int)p.y, newColor, false);

			System.Array.Copy(_neighborsOriginal, neighbors, _neighborsOriginal.Length);
      
      if (type == Type.Random) {
        DrawUtils.Shuffle(neighbors);
      } else if (type == Type.Growing) {
				System.Array.Sort(neighbors, (a, b) => -Vector2.Distance(p + a, originalPoint).CompareTo(Vector2.Distance(p + b, originalPoint)));
			}
      for (int i = 0; i < neighbors.Length; ++i) {
        int nx = (int)(p.x + neighbors[i].x);
        int ny = (int)(p.y + neighbors[i].y);
				if (repeatX)
					nx = (int)Mathf.Repeat(nx, Layer.width);
				if (repeatY)
					ny = (int)Mathf.Repeat(ny, Layer.height);
        if (nx < 0 || nx >= Layer.width || ny < 0 || ny >= Layer.height)
          continue;
				var col = Layer.GetPixelFast(nx, ny);
				
				// Transparency is defined as being under this threshold, to account for gradients
				const byte threshold = DrawPrefs.FLOOD_FILL_THRESHOLD;
				if (prevColor.a >= threshold) { 
					// "Solid" pixels are painted over, but only if they have the same color components as the starting point
					bool sameColor = col.r == prevColor.r && col.g == prevColor.g && col.b == prevColor.b;
					if (col.a <= threshold || !sameColor)
						continue;
				} else if(prevColor.a < threshold) { 
					// "Transparent" pixels only check for alpha and continue until threshold is hit
					if (col.a > threshold) 
						continue;
				}

				// Add this pixel to the list of pixels to paint
				stack.Add(new Vector2(nx, ny));
      }
      int c = stack.Count;
      if (Apply || c == 0)
        Layer.Apply(true);
			_t++;

      return c > 0;
    }


		// Old methods

		internal static void Fill(SerializableTexture2D Layer, int x, int y, Color32 prevColor, Color32 newColor) {
			//int[] dx = new int[8]{0, 1, 1, 1, 0, -1, -1, -1};
			//int[] dy = new int[8]{-1, -1, 0, 1, 1, 1, 0, -1};
			int[] dx = new int[4] {-1, 0, 1, 0};
			int[] dy = new int[4] {0, -1, 0, 1};
			var stack = new Stack<Vector2>();
			stack.Push(new Vector2(x,y));
			while(stack.Count > 0) {
				var p = stack.Pop();
				Layer.SetPixelFast((int)p.x, (int)p.y, newColor, false);
				for(int i = 0; i < dx.Length; i++) {
					int nx = (int)p.x + dx[i];
					int ny = (int)p.y + dy[i];
					if (nx < 0 || nx >= Layer.width || ny < 0 || ny >= Layer.height || !Layer.GetPixelFast(nx, ny).Equals(prevColor))
						continue;
					stack.Push(new Vector2(nx, ny));
				}
			}
		}
}
}