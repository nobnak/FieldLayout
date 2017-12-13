using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Polyhedra2DZone.SpacePartition {

    public class UniformGrid2D<T> : IEnumerable<T> {
        public enum WrapIndexEnum { Clamp = 0, Repeat }

        public const int BIG_NUMBER = 100000;

        protected T[] data = new T[0];
        protected int subdivision;

        protected WrapIndexEnum wrapMode;
        protected Vector2 min;
        protected Vector2 cellSize;

        protected Matrix4x4 quantizationMat;
        protected Vector2 quantizationScale;
        protected Vector2 quantizationOffset;

        public void Init(Vector2 min, Vector2 cellSize, WrapIndexEnum wrapMode = WrapIndexEnum.Clamp) {
            Clear();
            this.min = min;
            this.cellSize = cellSize;
            this.wrapMode = wrapMode;
            UpdateQuantizationMatrix();
        }

        public T[] Data { get { return data; } }
        public int Subdivision {
            get { return subdivision; }
            set {
                var nextValue = Mathf.Max(value, 0);
                Resize(nextValue);
            }
        }

        public WrapIndexEnum WrapMode { get { return wrapMode; } set { wrapMode = value; } }
        public Vector2 Min {
            get { return min; }
            set {
                min = value;
                UpdateQuantizationMatrix();
            }
        }
        public Vector2 CellSize {
            get { return cellSize; }
            set {
                cellSize = value;
                UpdateQuantizationMatrix();
            }
        }
        
        public T this[int x, int y] {
            get {
                var i = ToLinearIndex(x, y);
                return data[i];
            }
            set {
                var i = ToLinearIndex(x, y);
                data[i] = value;
            }
        }
        public T this[Vector2 p] {
            get {
                int x, y;
                Quantize(p, out x, out y);
                WrapIndex(ref x, ref y);
                return this[x, y];
            }
            set {
                int x, y;
                Quantize(p, out x, out y);
                WrapIndex(ref x, ref y);
                this[x, y] = value;
            }
        }

        public void Clear() {
            System.Array.Clear(data, 0, data.Length);
        }
        public int ToLinearIndex(int x, int y) {
            return x + y * subdivision;
        }
        public void WrapIndex(ref int x, ref int y) {
            switch (wrapMode) {
                case WrapIndexEnum.Clamp:
                    x = (x < 0 ? 0 : (x < subdivision ? x : subdivision - 1));
                    y = (y < 0 ? 0 : (y < subdivision ? y : subdivision - 1));
                    break;
                case WrapIndexEnum.Repeat:
                    x = (x % subdivision) + (x < 0 ? subdivision : 0);
                    y = (y % subdivision) + (y < 0 ? subdivision : 0);
                    break;
            }
        }
        public void Quantize(Vector2 p, out int x, out int y) {

#if NOT_OPTIMIZED
            var pquantized = quantizationMat.MultiplyPoint3x4(p);
            x = Mathf.FloorToInt(pquantized.x);
            y = Mathf.FloorToInt(pquantized.y);
#else
            var fx = quantizationScale.x * p.x + quantizationOffset.x;
            var fy = quantizationScale.y * p.y + quantizationOffset.y;
            x = (int)(fx + BIG_NUMBER) - BIG_NUMBER;
            y = (int)(fy + BIG_NUMBER) - BIG_NUMBER;
#endif
        }



#region IEnumerator
        public IEnumerator<T> GetEnumerator() {
            foreach (var c in data)
                yield return c;
        }
        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
#endregion

        protected void UpdateQuantizationMatrix() {
            var invSize = new Vector2(1f / cellSize.x, 1f / cellSize.y);
#if NOT_OPTIMIZED
            quantizationMat = Matrix4x4.TRS(
                Vector2.Scale(-min, invSize), Quaternion.identity, invSize);
#else
            quantizationOffset = Vector2.Scale(-min, invSize);
            quantizationScale = invSize;
#endif
        }
        protected void Resize(int nextValue) {
            if (nextValue != subdivision) {
                subdivision = nextValue;
                System.Array.Resize(ref data, subdivision * subdivision);
            }
        }
    }
}
