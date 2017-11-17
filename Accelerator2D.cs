using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gist;

namespace Polyhedra2DZone {

    [ExecuteInEditMode]
    public class Accelerator2D : MonoBehaviour {

        [SerializeField] protected Polygon2D polygon;
        [SerializeField] protected Fringe2D fringe;

        [SerializeField] int subdivision = 10;

        protected Validator validator;
        protected List<Cell> grid;

        #region Unity
        protected void OnEnable() {
            validator = new Validator();
            grid = new List<Cell>();

            if (polygon == null)
                polygon = GetComponent<Polygon2D>();
            if (fringe == null)
                fringe = GetComponent<Fringe2D>();
        }
        protected void OnValidate() {
            validator.Invalidate();
        }
        #endregion

        protected void GenerateGrid() {
            grid.Clear();

        }

        public struct Cell {
            public enum Side { Unknown = 0, Inside, Outside }

            public Side side;
            public Rect grid;
        }
    }
}
