﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Polyhedra2DZone {

    public class DefferedMatrix {

        protected bool valid;
        protected Matrix4x4[] chainOfMatrices;

        protected Matrix4x4 mergedMatrix;
        protected Matrix4x4 inverseMatrix;

        public void Reset(params Matrix4x4[] chainOfMatrices) {
            this.valid = false;
            this.chainOfMatrices = chainOfMatrices;
        }

        public Matrix4x4 Matrix {
            get {
                CheckValidation();
                return mergedMatrix;
            }
        }
        public Matrix4x4 Inverse {
            get {
                CheckValidation();
                return inverseMatrix;
            }
        }

        public Vector3 TransformPoint(Vector3 p) {
            CheckValidation();
            return mergedMatrix.MultiplyPoint3x4(p);
        }

        public Vector3 TransformVector(Vector3 v) {
            CheckValidation();
            return mergedMatrix.MultiplyVector(v);
        }

        public Vector3 InverseTransformPoint(Vector3 p) {
            CheckValidation();
            return inverseMatrix.MultiplyPoint3x4(p);
        }
        public Vector3 InverseTransformVector(Vector3 p) {
            CheckValidation();
            return inverseMatrix.MultiplyVector(p);
        }

        protected Vector2 Multiply(Matrix4x4 m, float x, float y, float w) {
            return new Vector2(
                            m[0] * x + m[4] * y + m[12] * w,
                            m[1] * x + m[5] * y + m[13] * w);
        }
        protected float Multiply(Matrix4x4 m, float x, float w) {
            return m[0] * x + m[12] * w;
        }
        protected void CheckValidation() {
            if (valid)
                return;
            valid = true;

            mergedMatrix = Matrix4x4.identity;
            foreach (var m in chainOfMatrices)
                this.mergedMatrix *= m;

            inverseMatrix = mergedMatrix.inverse;
        }
    }
}
