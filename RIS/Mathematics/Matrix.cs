// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RIS.Mathematics
{
    public sealed class Matrix
    {
        public event EventHandler<RInformationEventArgs> Information;
        public event EventHandler<RWarningEventArgs> Warning;
        public event EventHandler<RErrorEventArgs> Error;

        public static event EventHandler<RInformationEventArgs> InformationStatic;
        public static event EventHandler<RWarningEventArgs> WarningStatic;
        public static event EventHandler<RErrorEventArgs> ErrorStatic;

        public double this[int rowIndex, int columnIndex]
        {
            get
            {
                return MatrixValues[(rowIndex * Columns) + columnIndex];
            }
            set
            {
                MatrixValues[(rowIndex * Columns) + columnIndex] = value;
            }
        }

        private int[] _pi;
        private double _determinantOfP = 1;

        public Matrix L;
        public Matrix U;
        public readonly double[] MatrixValues;
        public int Rows { get; set; }
        public int Columns { get; set; }
        public bool IsSquare
        {
            get
            {
                return Rows == Columns;
            }
        }

        public Matrix(int rows, int columns)
        {
            Rows = rows;
            Columns = columns;
            MatrixValues = new double[Rows * Columns];
        }
        public Matrix(double[,] matrix, bool rowMajor = true)
        {
            if (rowMajor)
            {
                Rows = matrix.GetLength(0);
                Columns = matrix.GetLength(1);
                MatrixValues = matrix.Cast<double>().ToArray();
            }
            else
            {
                Rows = matrix.GetLength(1);
                Columns = matrix.GetLength(0);
                MatrixValues = new double[Rows * Columns];

                for (int row = 0; row < Rows; row++)
                {
                    for (int col = 0; col < Columns; col++)
                    {
                        this[row, col] = matrix[matrix.GetLowerBound(0) + col, matrix.GetLowerBound(1) + row];
                    }
                }
            }
        }

        public void OnInformation(RInformationEventArgs e)
        {
            OnInformation(this, e);
        }
        public void OnInformation(object sender, RInformationEventArgs e)
        {
            Information?.Invoke(sender, e);
        }

        public void OnWarning(RWarningEventArgs e)
        {
            OnWarning(this, e);
        }
        public void OnWarning(object sender, RWarningEventArgs e)
        {
            Warning?.Invoke(sender, e);
        }

        public void OnError(RErrorEventArgs e)
        {
            OnError(this, e);
        }
        public void OnError(object sender, RErrorEventArgs e)
        {
            Error?.Invoke(sender, e);
        }

        public static void OnInformationStatic(RInformationEventArgs e)
        {
            OnInformationStatic(null, e);
        }
        public static void OnInformationStatic(object sender, RInformationEventArgs e)
        {
            InformationStatic?.Invoke(sender, e);
        }

        public static void OnWarningStatic(RWarningEventArgs e)
        {
            OnWarningStatic(null, e);
        }
        public static void OnWarningStatic(object sender, RWarningEventArgs e)
        {
            WarningStatic?.Invoke(sender, e);
        }

        public static void OnErrorStatic(RErrorEventArgs e)
        {
            OnErrorStatic(null, e);
        }
        public static void OnErrorStatic(object sender, RErrorEventArgs e)
        {
            ErrorStatic?.Invoke(sender, e);
        }

        public Matrix GetColumn(int k)
        {
            Matrix m = new Matrix(Rows, 1);

            for (int i = 0; i < Rows; ++i)
            {
                m[i, 0] = this[i, k];
            }

            return m;
        }

        public void SetColumn(Matrix v, int k)
        {
            for (int i = 0; i < Rows; ++i)
            {
                this[i, k] = v[i, 0];
            }
        }

        public void MakeLU()
        {
            if (!IsSquare)
            {
                var exception = new Exception("The matrix is not square!");
                Events.OnError(this, new RErrorEventArgs(exception, exception.Message));
                OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            L = IdentityMatrix(Rows, Columns);
            U = Duplicate();

            _pi = new int[Rows];
            for (int i = 0; i < Rows; ++i)
            {
                _pi[i] = i;
            }

            double p = 0;
            double pom2;
            int k0 = 0;
            int pom1 = 0;

            for (int k = 0; k < Columns - 1; ++k)
            {
                p = 0;

                for (int i = k; i < Rows; ++i)
                {
                    if (System.Math.Abs(U[i, k]) > p)
                    {
                        p = System.Math.Abs(U[i, k]);
                        k0 = i;
                    }
                }

                if (p == 0)
                {
                    var exception = new Exception("The matrix is singular!");
                    Events.OnError(this, new RErrorEventArgs(exception, exception.Message));
                    OnError(new RErrorEventArgs(exception, exception.Message));
                    throw exception;
                }

                pom1 = _pi[k];
                _pi[k] = _pi[k0];
                _pi[k0] = pom1;

                for (int i = 0; i < k; ++i)
                {
                    pom2 = L[k, i];
                    L[k, i] = L[k0, i];
                    L[k0, i] = pom2;
                }

                if (k != k0)
                {
                    _determinantOfP *= -1;
                }

                for (int i = 0; i < Columns; ++i)
                {
                    pom2 = U[k, i];
                    U[k, i] = U[k0, i];
                    U[k0, i] = pom2;
                }

                for (int i = k + 1; i < Rows; ++i)
                {
                    L[i, k] = U[i, k] / U[k, k];

                    for (int j = k; j < Columns; ++j)
                    {
                        U[i, j] -= (L[i, k] * U[k, j]);
                    }
                }
            }
        }

        public Matrix SolveWith(Matrix v)
        {
            if (Rows != Columns)
            {
                var exception = new Exception("The matrix is not square!");
                Events.OnError(this, new RErrorEventArgs(exception, exception.Message));
                OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            if (Rows != v.Rows)
            {
                var exception = new Exception("Wrong number of results in solution vector!");
                Events.OnError(this, new RErrorEventArgs(exception, exception.Message));
                OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            if (v.Columns != 1)
            {
                var exception = new Exception("The solution vector v must be a column vector");
                Events.OnError(this, new RErrorEventArgs(exception, exception.Message));
                OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            if (L == null)
            {
                MakeLU();
            }

            Matrix b = new Matrix(Rows, 1);

            for (int i = 0; i < Rows; ++i)
            {
                b[i, 0] = v[_pi[i], 0];
            }

            Matrix z = SubsForth(L, b);
            Matrix x = SubsBack(U, z);

            return x;
        }

        //TODO: check for redundancy with MakeLU() and SolveWith()
        public void MakeRref()
        {
            int lead = 0;

            for (int r = 0; r < Rows; r++)
            {
                if (Columns <= lead)
                    break;

                int i = r;
                while (this[i, lead] == 0)
                {
                    ++i;

                    if (i == Rows)
                    {
                        i = r;
                        lead++;

                        if (Columns == lead)
                        {
                            lead--;

                            break;
                        }
                    }
                }

                for (int j = 0; j < Columns; ++j)
                {
                    double temp = this[r, j];
                    this[r, j] = this[i, j];
                    this[i, j] = temp;
                }

                double div = this[r, lead];

                for (int j = 0; j < Columns; ++j)
                {
                    this[r, j] /= div;
                }

                for (int j = 0; j < Rows; ++j)
                {
                    if (j != r)
                    {
                        double sub = this[j, lead];

                        for (int k = 0; k < Columns; ++k)
                        {
                            this[j, k] -= (sub * this[r, k]);
                        }
                    }
                }

                lead++;
            }
        }

        public Matrix Invert()
        {
            if (L == null)
            {
                MakeLU();
            }

            Matrix inv = new Matrix(Rows, Columns);

            for (int i = 0; i < Rows; ++i)
            {
                Matrix Ei = ZeroMatrix(Rows, 1);

                Ei[i, 0] = 1;

                Matrix col = SolveWith(Ei);

                inv.SetColumn(col, i);
            }

            return inv;
        }

        public double Determinant()
        {
            if (L == null)
            {
                MakeLU();
            }

            double det = _determinantOfP;

            for (int i = 0; i < Rows; ++i)
            {
                det *= U[i, i];
            }

            return det;
        }

        public Matrix GetPermutationPtoPi()
        {
            if (L == null)
            {
                MakeLU();
            }

            Matrix matrix = ZeroMatrix(Rows, Columns);
            for (int i = 0; i < Rows; ++i)
            {
                matrix[_pi[i], i] = 1;
            }

            return matrix;
        }

        public Matrix Duplicate()
        {
            Matrix matrix = new Matrix(Rows, Columns);

            for (int i = 0; i < Rows; ++i)
            {
                for (int j = 0; j < Columns; ++j)
                {
                    matrix[i, j] = this[i, j];
                }
            }

            return matrix;
        }

        public static Matrix SubsForth(Matrix a, Matrix b)
        {
            if (a.L == null)
            {
                a.MakeLU();
            }

            int n = a.Rows;
            Matrix x = new Matrix(n, 1);

            for (int i = 0; i < n; ++i)
            {
                x[i, 0] = b[i, 0];

                for (int j = 0; j < i; ++j)
                {
                    x[i, 0] -= a[i, j] * x[j, 0];
                }

                x[i, 0] /= a[i, i];
            }

            return x;
        }

        public static Matrix SubsBack(Matrix a, Matrix b)
        {
            if (a.L == null)
            {
                a.MakeLU();
            }

            int n = a.Rows;
            Matrix x = new Matrix(n, 1);

            for (int i = n - 1; i > -1; i--)
            {
                x[i, 0] = b[i, 0];

                for (int j = n - 1; j > i; j--)
                {
                    x[i, 0] -= a[i, j] * x[j, 0];
                }

                x[i, 0] /= a[i, i];
            }

            return x;
        }

        public static Matrix ZeroMatrix(int rows, int columns)
        {
            Matrix matrix = new Matrix(rows, columns);

            for (int i = 0; i < rows; ++i)
            {
                for (int j = 0; j < columns; ++j)
                {
                    matrix[i, j] = 0;
                }
            }

            return matrix;
        }

        public static Matrix IdentityMatrix(int rows, int columns)
        {
            Matrix matrix = ZeroMatrix(rows, columns);

            for (int i = 0; i < System.Math.Min(rows, columns); ++i)
            {
                matrix[i, i] = 1;
            }

            return matrix;
        }

        public static Matrix RandomMatrix(int rows, int columns, int dispersion)
        {
            Random random = new Random();
            Matrix matrix = new Matrix(rows, columns);

            for (int i = 0; i < rows; ++i)
            {
                for (int j = 0; j < columns; ++j)
                {
                    matrix[i, j] = random.Next(-dispersion, dispersion);
                }
            }

            return matrix;
        }

        public static Matrix Parse(string matrixString)
        {
            string s = NormalizeMatrixString(matrixString);
            string[] rows = Regex.Split(s, "\r\n");
            string[] numbers = rows[0].Split(' ');
            Matrix matrix = new Matrix(rows.Length, numbers.Length);

            try
            {
                for (int i = 0; i < rows.Length; ++i)
                {
                    numbers = rows[i].Split(' ');
                    for (int j = 0; j < numbers.Length; ++j)
                    {
                        matrix[i, j] = double.Parse(numbers[j]);
                    }
                }
            }
            catch (FormatException)
            {
                var exception = new Exception("Wrong input format!");
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                OnErrorStatic(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }
            catch (Exception ex)
            {
                Events.OnError(new RErrorEventArgs(ex, ex.Message));
                OnErrorStatic(new RErrorEventArgs(ex, ex.Message));
                throw;
            }

            return matrix;
        }

        public override string ToString()
        {
            StringBuilder s = new StringBuilder();

            for (int i = 0; i < Rows; ++i)
            {
                for (int j = 0; j < Columns; ++j)
                {
                    s.AppendFormat("{0,5:E2}", this[i, j]).Append(' ');
                }

                s.AppendLine();
            }
            return s.ToString();
        }

        public static Matrix Transpose(Matrix m)
        {
            Matrix t = new Matrix(m.Columns, m.Rows);

            for (int i = 0; i < m.Rows; ++i)
            {
                for (int j = 0; j < m.Columns; ++j)
                {
                    t[j, i] = m[i, j];
                }
            }

            return t;
        }

        public static Matrix Power(Matrix m, int pow)
        {
            if (pow == 0)
                return IdentityMatrix(m.Rows, m.Columns);

            if (pow == 1)
                return m.Duplicate();

            if (pow == -1)
                return m.Invert();

            Matrix x;

            if (pow < 0)
            {
                x = m.Invert(); pow *= -1;
            }
            else
            {
                x = m.Duplicate();
            }

            Matrix result = IdentityMatrix(m.Rows, m.Columns);

            while (pow != 0)
            {
                if ((pow & 1) == 1)
                {
                    result *= x;
                }

                x *= x;
                pow >>= 1;
            }

            return result;
        }

        private static void SafeAplusBintoC(Matrix A, int xa, int ya, Matrix B, int xb, int yb, Matrix C, int size)
        {
            for (int i = 0; i < size; ++i) // rows
            {
                for (int j = 0; j < size; ++j) // columns
                {
                    C[i, j] = 0;

                    if (xa + j < A.Columns && ya + i < A.Rows)
                    {
                        C[i, j] += A[ya + i, xa + j];
                    }

                    if (xb + j < B.Columns && yb + i < B.Rows)
                    {
                        C[i, j] += B[yb + i, xb + j];
                    }
                }
            }
        }

        private static void SafeAminusBintoC(Matrix A, int xa, int ya, Matrix B, int xb, int yb, Matrix C, int size)
        {
            for (int i = 0; i < size; ++i) // rows
            {
                for (int j = 0; j < size; ++j)
                {
                    C[i, j] = 0;

                    if (xa + j < A.Columns && ya + i < A.Rows)
                    {
                        C[i, j] += A[ya + i, xa + j];
                    }

                    if (xb + j < B.Columns && yb + i < B.Rows)
                    {
                        C[i, j] -= B[yb + i, xb + j];
                    }
                }
            }
        }

        private static void SafeACopytoC(Matrix A, int xa, int ya, Matrix C, int size)
        {
            for (int i = 0; i < size; ++i) // rows
            {
                for (int j = 0; j < size; ++j)
                {
                    C[i, j] = 0;

                    if (xa + j < A.Columns && ya + i < A.Rows)
                    {
                        C[i, j] += A[ya + i, xa + j];
                    }
                }
            }
        }

        private static void AplusBintoC(Matrix A, int xa, int ya, Matrix B, int xb, int yb, Matrix C, int size)
        {
            for (int i = 0; i < size; ++i) // rows
            {
                for (int j = 0; j < size; ++j)
                {
                    C[i, j] = A[ya + i, xa + j] + B[yb + i, xb + j];
                }
            }
        }

        private static void AminusBintoC(Matrix A, int xa, int ya, Matrix B, int xb, int yb, Matrix C, int size)
        {
            for (int i = 0; i < size; ++i) // rows
            {
                for (int j = 0; j < size; ++j)
                {
                    C[i, j] = A[ya + i, xa + j] - B[yb + i, xb + j];
                }
            }
        }

        private static void ACopytoC(Matrix A, int xa, int ya, Matrix C, int size)
        {
            for (int i = 0; i < size; ++i) // rows
            {
                for (int j = 0; j < size; ++j)
                {
                    C[i, j] = A[ya + i, xa + j];
                }
            }
        }

        //TODO: assume matrix 2^N x 2^N and then directly call StrassenMultiplyRun(A,B,?,1,?)
        private static Matrix StrassenMultiply(Matrix A, Matrix B)
        {
            if (A.Columns != B.Rows)
            {
                var exception = new Exception("Wrong dimension of matrix!");
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                OnErrorStatic(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            Matrix R;

            int maxSize = System.Math.Max(System.Math.Max(A.Rows, A.Columns), System.Math.Max(B.Rows, B.Columns));

            int size = 1;
            int n = 0;

            while (maxSize > size)
            {
                size *= 2; n++;
            }

            int h = size / 2;


            Matrix[,] mField = new Matrix[n, 9];

            /*
             *  8x8, 8x8, 8x8, ...
             *  4x4, 4x4, 4x4, ...
             *  2x2, 2x2, 2x2, ...
             *  . . .
             */

            int z;
            for (int i = 0; i < n - 4; ++i) // rows
            {
                z = (int)System.Math.Pow(2, n - i - 1);
                for (int j = 0; j < 9; ++j)
                {
                    mField[i, j] = new Matrix(z, z);
                }
            }

            SafeAplusBintoC(A, 0, 0, A, h, h, mField[0, 0], h);
            SafeAplusBintoC(B, 0, 0, B, h, h, mField[0, 1], h);
            StrassenMultiplyRun(mField[0, 0], mField[0, 1], mField[0, 1 + 1], 1, mField); // (A11 + A22) * (B11 + B22);

            SafeAplusBintoC(A, 0, h, A, h, h, mField[0, 0], h);
            SafeACopytoC(B, 0, 0, mField[0, 1], h);
            StrassenMultiplyRun(mField[0, 0], mField[0, 1], mField[0, 1 + 2], 1, mField); // (A21 + A22) * B11;

            SafeACopytoC(A, 0, 0, mField[0, 0], h);
            SafeAminusBintoC(B, h, 0, B, h, h, mField[0, 1], h);
            StrassenMultiplyRun(mField[0, 0], mField[0, 1], mField[0, 1 + 3], 1, mField); //A11 * (B12 - B22);

            SafeACopytoC(A, h, h, mField[0, 0], h);
            SafeAminusBintoC(B, 0, h, B, 0, 0, mField[0, 1], h);
            StrassenMultiplyRun(mField[0, 0], mField[0, 1], mField[0, 1 + 4], 1, mField); //A22 * (B21 - B11);

            SafeAplusBintoC(A, 0, 0, A, h, 0, mField[0, 0], h);
            SafeACopytoC(B, h, h, mField[0, 1], h);
            StrassenMultiplyRun(mField[0, 0], mField[0, 1], mField[0, 1 + 5], 1, mField); //(A11 + A12) * B22;

            SafeAminusBintoC(A, 0, h, A, 0, 0, mField[0, 0], h);
            SafeAplusBintoC(B, 0, 0, B, h, 0, mField[0, 1], h);
            StrassenMultiplyRun(mField[0, 0], mField[0, 1], mField[0, 1 + 6], 1, mField); //(A21 - A11) * (B11 + B12);

            SafeAminusBintoC(A, h, 0, A, h, h, mField[0, 0], h);
            SafeAplusBintoC(B, 0, h, B, h, h, mField[0, 1], h);
            StrassenMultiplyRun(mField[0, 0], mField[0, 1], mField[0, 1 + 7], 1, mField); // (A12 - A22) * (B21 + B22);

            R = new Matrix(A.Rows, B.Columns); // result

            // C11
            for (int i = 0; i < System.Math.Min(h, R.Rows); ++i) // rows
            {
                for (int j = 0; j < System.Math.Min(h, R.Columns); ++j) // cols
                {
                    R[i, j] = mField[0, 1 + 1][i, j] + mField[0, 1 + 4][i, j] - mField[0, 1 + 5][i, j] + mField[0, 1 + 7][i, j];
                }
            }

            // C12
            for (int i = 0; i < System.Math.Min(h, R.Rows); ++i) // rows
            {
                for (int j = h; j < System.Math.Min(2 * h, R.Columns); ++j) // cols
                {
                    R[i, j] = mField[0, 1 + 3][i, j - h] + mField[0, 1 + 5][i, j - h];
                }
            }

            // C21
            for (int i = h; i < System.Math.Min(2 * h, R.Rows); ++i) // rows
            {
                for (int j = 0; j < System.Math.Min(h, R.Columns); ++j) // cols
                {
                    R[i, j] = mField[0, 1 + 2][i - h, j] + mField[0, 1 + 4][i - h, j];
                }
            }

            // C22
            for (int i = h; i < System.Math.Min(2 * h, R.Rows); ++i) // rows
            {
                for (int j = h; j < System.Math.Min(2 * h, R.Columns); ++j) // cols
                {
                    R[i, j] = mField[0, 1 + 1][i - h, j - h] - mField[0, 1 + 2][i - h, j - h] + mField[0, 1 + 3][i - h, j - h] + mField[0, 1 + 6][i - h, j - h];
                }
            }

            return R;
        }
        private static void StrassenMultiplyRun(Matrix A, Matrix B, Matrix C, int l, Matrix[,] f)
        {
            int size = A.Rows;
            int h = size / 2;

            AplusBintoC(A, 0, 0, A, h, h, f[l, 0], h);
            AplusBintoC(B, 0, 0, B, h, h, f[l, 1], h);
            StrassenMultiplyRun(f[l, 0], f[l, 1], f[l, 1 + 1], l + 1, f); // (A11 + A22) * (B11 + B22);

            AplusBintoC(A, 0, h, A, h, h, f[l, 0], h);
            ACopytoC(B, 0, 0, f[l, 1], h);
            StrassenMultiplyRun(f[l, 0], f[l, 1], f[l, 1 + 2], l + 1, f); // (A21 + A22) * B11;

            ACopytoC(A, 0, 0, f[l, 0], h);
            AminusBintoC(B, h, 0, B, h, h, f[l, 1], h);
            StrassenMultiplyRun(f[l, 0], f[l, 1], f[l, 1 + 3], l + 1, f); //A11 * (B12 - B22);

            ACopytoC(A, h, h, f[l, 0], h);
            AminusBintoC(B, 0, h, B, 0, 0, f[l, 1], h);
            StrassenMultiplyRun(f[l, 0], f[l, 1], f[l, 1 + 4], l + 1, f); //A22 * (B21 - B11);

            AplusBintoC(A, 0, 0, A, h, 0, f[l, 0], h);
            ACopytoC(B, h, h, f[l, 1], h);
            StrassenMultiplyRun(f[l, 0], f[l, 1], f[l, 1 + 5], l + 1, f); //(A11 + A12) * B22;

            AminusBintoC(A, 0, h, A, 0, 0, f[l, 0], h);
            AplusBintoC(B, 0, 0, B, h, 0, f[l, 1], h);
            StrassenMultiplyRun(f[l, 0], f[l, 1], f[l, 1 + 6], l + 1, f); //(A21 - A11) * (B11 + B12);

            AminusBintoC(A, h, 0, A, h, h, f[l, 0], h);
            AplusBintoC(B, 0, h, B, h, h, f[l, 1], h);
            StrassenMultiplyRun(f[l, 0], f[l, 1], f[l, 1 + 7], l + 1, f); // (A12 - A22) * (B21 + B22);

            // C11
            for (int i = 0; i < h; ++i) // rows
            {
                for (int j = 0; j < h; ++j) // cols
                {
                    C[i, j] = f[l, 1 + 1][i, j] + f[l, 1 + 4][i, j] - f[l, 1 + 5][i, j] + f[l, 1 + 7][i, j];
                }
            }

            // C12
            for (int i = 0; i < h; ++i) // rows
            {
                for (int j = h; j < size; ++j) // cols
                {
                    C[i, j] = f[l, 1 + 3][i, j - h] + f[l, 1 + 5][i, j - h];
                }
            }

            // C21
            for (int i = h; i < size; ++i) // rows
            {
                for (int j = 0; j < h; ++j) // cols
                {
                    C[i, j] = f[l, 1 + 2][i - h, j] + f[l, 1 + 4][i - h, j];
                }
            }

            // C22
            for (int i = h; i < size; ++i) // rows
            {
                for (int j = h; j < size; ++j) // cols
                {
                    C[i, j] = f[l, 1 + 1][i - h, j - h] - f[l, 1 + 2][i - h, j - h] + f[l, 1 + 3][i - h, j - h] + f[l, 1 + 6][i - h, j - h];
                }
            }
        }
        private static Matrix StupidMultiply(Matrix m1, Matrix m2)
        {
            if (m1.Columns != m2.Rows)
            {
                var exception = new Exception("Wrong dimensions of matrix!");
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                OnErrorStatic(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            Matrix result = ZeroMatrix(m1.Rows, m2.Columns);

            for (int i = 0; i < result.Rows; ++i)
            {
                for (int j = 0; j < result.Columns; ++j)
                {
                    for (int k = 0; k < m1.Columns; ++k)
                    {
                        result[i, j] += m1[i, k] * m2[k, j];
                    }
                }
            }

            return result;
        }

        private static Matrix Multiply(Matrix m1, Matrix m2)
        {
            if (m1.Columns != m2.Rows)
            {
                var exception = new Exception("Wrong dimension of matrix!");
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                OnErrorStatic(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            int maxSize = System.Math.Max(System.Math.Max(m1.Rows, m1.Columns), System.Math.Max(m2.Rows, m2.Columns));

            if (maxSize < 32)
                return StupidMultiply(m1, m2);

            if (!m1.IsSquare || !m2.IsSquare)
                return StupidMultiply(m1, m2);

            double exponent = System.Math.Log(maxSize) / System.Math.Log(2);

            if (Math.AlmostEquals(System.Math.Pow(2, exponent), maxSize, 0.0000001))
            {
                return StrassenMultiply(m1, m2);
            }
            else
            {
                return StupidMultiply(m1, m2);
            }
        }
        private static Matrix Multiply(double n, Matrix m)
        {
            Matrix r = new Matrix(m.Rows, m.Columns);

            for (int i = 0; i < m.Rows; ++i)
            {
                for (int j = 0; j < m.Columns; ++j)
                {
                    r[i, j] = m[i, j] * n;
                }
            }

            return r;
        }
        private static Matrix Add(Matrix m1, Matrix m2)
        {
            if (m1.Rows != m2.Rows || m1.Columns != m2.Columns)
            {
                var exception = new Exception("Matrices must have the same dimensions!");
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                OnErrorStatic(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            Matrix r = new Matrix(m1.Rows, m1.Columns);

            for (int i = 0; i < r.Rows; ++i)
            {
                for (int j = 0; j < r.Columns; ++j)
                {
                    r[i, j] = m1[i, j] + m2[i, j];
                }
            }

            return r;
        }

        public static string NormalizeMatrixString(string matStr)
        {
            while (matStr.IndexOf("  ", StringComparison.Ordinal) != -1)
            {
                matStr = matStr.Replace("  ", " ");
            }

            matStr = matStr.Replace(" \r\n", "\r\n");
            matStr = matStr.Replace("\r\n ", "\r\n");
            matStr = matStr.Replace("\r\n", "|");

            while (matStr.LastIndexOf("|", StringComparison.Ordinal) == (matStr.Length - 1))
            {
                matStr = matStr.Substring(0, matStr.Length - 1);
            }

            matStr = matStr.Replace("|", "\r\n");

            return matStr.Trim();
        }

        public static Matrix operator -(Matrix m)
        {
            return Multiply(-1, m);
        }
        public static Matrix operator +(Matrix m1, Matrix m2)
        {
            return Add(m1, m2);
        }
        public static Matrix operator -(Matrix m1, Matrix m2)
        {
            return Add(m1, -m2);
        }
        public static Matrix operator *(Matrix m1, Matrix m2)
        {
            return Multiply(m1, m2);
        }
        public static Matrix operator *(double n, Matrix m)
        {
            return Multiply(n, m);
        }
    }
}
