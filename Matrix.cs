using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NumApproximation
{
    public partial class Form1
    {
        // ===== Matrix routines

        static public double[][] MatrixCreate(int rows, int cols)
        {
            // allocates/creates a matrix initialized to all 0.0
            // do error checking here
            double[][] result = new double[rows][];
            for (int i = 0; i < rows; ++i)
                result[i] = new double[cols];
            return result;
        }

        // -------------------------------------------------------------

        static public double[][] MatrixRandom(int rows, int cols,
          double minVal, double maxVal, int seed)
        {
            // return a matrix with random values
            Random ran = new Random(seed);
            double[][] result = MatrixCreate(rows, cols);
            for (int i = 0; i < rows; ++i)
                for (int j = 0; j < cols; ++j)
                    result[i][j] = (maxVal - minVal) *
                      ran.NextDouble() + minVal;
            return result;
        }

        // -------------------------------------------------------------

        static public double[][] MatrixLoad(string file, bool header,
          char sep)
        {
            // load a matrix from a text file
            string line = "";
            string[] tokens = null;
            int ct = 0;
            int rows, cols;
            // determined # rows and cols
            System.IO.FileStream ifs =
              new System.IO.FileStream(file, System.IO.FileMode.Open);
            System.IO.StreamReader sr =
              new System.IO.StreamReader(ifs);
            while ((line = sr.ReadLine()) != null)
            {
                ++ct;
                tokens = line.Split(sep); // do validation here
            }
            sr.Close(); ifs.Close();
            if (header == true)
                rows = ct - 1;
            else
                rows = ct;
            cols = tokens.Length;
            double[][] result = MatrixCreate(rows, cols);

            // load
            int i = 0; // row index
            ifs = new System.IO.FileStream(file, System.IO.FileMode.Open);
            sr = new System.IO.StreamReader(ifs);

            if (header == true)
                line = sr.ReadLine();  // consume header
            while ((line = sr.ReadLine()) != null)
            {
                tokens = line.Split(sep);
                for (int j = 0; j < cols; ++j)
                    result[i][j] = double.Parse(tokens[j]);
                ++i; // next row
            }
            sr.Close(); ifs.Close();
            return result;
        }

        // -------------------------------------------------------------

        static public double[] MatrixToVector(double[][] matrix)
        {
            // single column matrix to vector
            int rows = matrix.Length;
            int cols = matrix[0].Length;
            if (cols != 1)
                throw new Exception("Bad matrix");
            double[] result = new double[rows];
            for (int i = 0; i < rows; ++i)
                result[i] = matrix[i][0];
            return result;
        }

        // -------------------------------------------------------------

        static public double[][] MatrixIdentity(int n)
        {
            // return an n x n Identity matrix
            double[][] result = MatrixCreate(n, n);
            for (int i = 0; i < n; ++i)
                result[i][i] = 1.0;

            return result;
        }

        // -------------------------------------------------------------

        static public string MatrixAsString(double[][] matrix, int dec)
        {
            string s = "";
            for (int i = 0; i < matrix.Length; ++i)
            {
                for (int j = 0; j < matrix[i].Length; ++j)
                    s += matrix[i][j].ToString("F" + dec).PadLeft(8) + " ";
                s += Environment.NewLine;
            }
            return s;
        }

        // -------------------------------------------------------------

        static public bool MatrixAreEqual(double[][] matrixA,
          double[][] matrixB, double epsilon)
        {
            // true if all values in matrixA == corresponding values in matrixB
            int aRows = matrixA.Length; int aCols = matrixA[0].Length;
            int bRows = matrixB.Length; int bCols = matrixB[0].Length;
            if (aRows != bRows || aCols != bCols)
                throw new Exception("Non-conformable matrices in MatrixAreEqual");

            for (int i = 0; i < aRows; ++i) // each row of A and B
                for (int j = 0; j < aCols; ++j) // each col of A and B
                                                //if (matrixA[i][j] != matrixB[i][j])
                    if (Math.Abs(matrixA[i][j] - matrixB[i][j]) > epsilon)
                        return false;
            return true;
        }

        // -------------------------------------------------------------

        static public double[][] MatrixProduct(double[][] matrixA, double[][] matrixB)
        {
            int aRows = matrixA.Length; int aCols = matrixA[0].Length;
            int bRows = matrixB.Length; int bCols = matrixB[0].Length;
            if (aCols != bRows)
                throw new Exception("Non-conformable matrices in MatrixProduct");

            double[][] result = MatrixCreate(aRows, bCols);

            for (int i = 0; i < aRows; ++i) // each row of A
                for (int j = 0; j < bCols; ++j) // each col of B
                    for (int k = 0; k < aCols; ++k) // could use k < bRows
                        result[i][j] += matrixA[i][k] * matrixB[k][j];

            //Parallel.For(0, aRows, i =>
            //  {
            //    for (int j = 0; j < bCols; ++j) // each col of B
            //      for (int k = 0; k < aCols; ++k) // could use k < bRows
            //        result[i][j] += matrixA[i][k] * matrixB[k][j];
            //  }
            //);

            return result;
        }

        // -------------------------------------------------------------

        static public double[] MatrixVectorProduct(double[][] matrix, double[] vector)
        {
            // result of multiplying an n x m matrix by a m x 1 column vector (yielding an n x 1 column vector)
            int mRows = matrix.Length; int mCols = matrix[0].Length;
            int vRows = vector.Length;
            if (mCols != vRows)
                throw new Exception("Non-conformable matrix and vector in MatrixVectorProduct");
            double[] result = new double[mRows]; // an n x m matrix times a m x 1 column vector is a n x 1 column vector
            for (int i = 0; i < mRows; ++i)
                for (int j = 0; j < mCols; ++j)
                    result[i] += matrix[i][j] * vector[j];
            return result;
        }

        // -------------------------------------------------------------

        static public double[][] MatrixDecompose(double[][] matrix, out int[] perm,
          out int toggle)
        {
            // Doolittle LUP decomposition with partial pivoting.
            // returns: result is L (with 1s on diagonal) and U;
            // perm holds row permutations; toggle is +1 or -1 (even or odd)
            int rows = matrix.Length;
            int cols = matrix[0].Length;
            if (rows != cols)
                throw new Exception("Non-square mattrix");

            int n = rows; // convenience

            double[][] result = MatrixDuplicate(matrix); // 

            perm = new int[n]; // set up row permutation result
            for (int i = 0; i < n; ++i) { perm[i] = i; }

            toggle = 1; // toggle tracks row swaps

            for (int j = 0; j < n - 1; ++j) // each column
            {
                double colMax = Math.Abs(result[j][j]);
                int pRow = j;
                //for (int i = j + 1; i < n; ++i) // deprecated
                //{
                //  if (result[i][j] > colMax)
                //  {
                //    colMax = result[i][j];
                //    pRow = i;
                //  }
                //}

                for (int i = j + 1; i < n; ++i) // reader Matt V needed this:
                {
                    if (Math.Abs(result[i][j]) > colMax)
                    {
                        colMax = Math.Abs(result[i][j]);
                        pRow = i;
                    }
                }
                // Not sure if this approach is needed always, or not.

                if (pRow != j) // if largest value not on pivot, swap rows
                {
                    double[] rowPtr = result[pRow];
                    result[pRow] = result[j];
                    result[j] = rowPtr;

                    int tmp = perm[pRow]; // and swap perm info
                    perm[pRow] = perm[j];
                    perm[j] = tmp;

                    toggle = -toggle; // adjust the row-swap toggle
                }

                // -------------------------------------------------------------
                // This part added later (not in original code) 
                // and replaces the 'return null' below.
                // if there is a 0 on the diagonal, find a good row 
                // from i = j+1 down that doesn't have
                // a 0 in column j, and swap that good row with row j

                if (result[j][j] == 0.0)
                {
                    // find a good row to swap
                    int goodRow = -1;
                    for (int row = j + 1; row < n; ++row)
                    {
                        if (result[row][j] != 0.0)
                            goodRow = row;
                    }

                    if (goodRow == -1)
                        throw new Exception("Cannot use Doolittle's method");

                    // swap rows so 0.0 no longer on diagonal
                    double[] rowPtr = result[goodRow];
                    result[goodRow] = result[j];
                    result[j] = rowPtr;

                    int tmp = perm[goodRow]; // and swap perm info
                    perm[goodRow] = perm[j];
                    perm[j] = tmp;

                    toggle = -toggle; // adjust the row-swap toggle
                }
                // -------------------------------------------------------------

                //if (Math.Abs(result[j][j]) < 1.0E-20) // deprecated
                //  return null; // consider a throw

                for (int i = j + 1; i < n; ++i)
                {
                    result[i][j] /= result[j][j];
                    for (int k = j + 1; k < n; ++k)
                    {
                        result[i][k] -= result[i][j] * result[j][k];
                    }
                }

            } // main j column loop

            return result;
        } // MatrixDecompose

        // -------------------------------------------------------------

        static public double[][] MatrixInverse(double[][] matrix)
        {
            int n = matrix.Length;
            double[][] result = MatrixDuplicate(matrix);

            int[] perm;
            int toggle;
            double[][] lum = MatrixDecompose(matrix, out perm, out toggle);
            if (lum == null)
                throw new Exception("Unable to compute inverse");

            double[] b = new double[n];
            for (int i = 0; i < n; ++i)
            {
                for (int j = 0; j < n; ++j)
                {
                    if (i == perm[j])
                        b[j] = 1.0;
                    else
                        b[j] = 0.0;
                }

                double[] x = HelperSolve(lum, b); // use decomposition

                for (int j = 0; j < n; ++j)
                    result[j][i] = x[j];
            }
            return result;
        }

        // -------------------------------------------------------------

        static public double[][] MatrixTranspose(double[][] matrix)
        {
            int rows = matrix.Length;
            int cols = matrix[0].Length;
            double[][] result = MatrixCreate(cols, rows); // note indexing
            for (int i = 0; i < rows; ++i)
            {
                for (int j = 0; j < cols; ++j)
                {
                    result[j][i] = matrix[i][j];
                }
            }
            return result;
        } // TransposeMatrix

        // -------------------------------------------------------------

        static public double MatrixDeterminant(double[][] matrix)
        {
            int[] perm;
            int toggle;
            double[][] lum = MatrixDecompose(matrix, out perm, out toggle);
            if (lum == null)
                throw new Exception("Unable to compute MatrixDeterminant");
            double result = toggle;
            for (int i = 0; i < lum.Length; ++i)
                result *= lum[i][i];
            return result;
        }

        // -------------------------------------------------------------

        static public double[] HelperSolve(double[][] luMatrix, double[] b)
        {
            // before calling this helper, permute b using the perm array
            // from MatrixDecompose that generated luMatrix
            int n = luMatrix.Length;
            double[] x = new double[n];
            b.CopyTo(x, 0);

            for (int i = 1; i < n; ++i)
            {
                double sum = x[i];
                for (int j = 0; j < i; ++j)
                    sum -= luMatrix[i][j] * x[j];
                x[i] = sum;
            }

            x[n - 1] /= luMatrix[n - 1][n - 1];
            for (int i = n - 2; i >= 0; --i)
            {
                double sum = x[i];
                for (int j = i + 1; j < n; ++j)
                    sum -= luMatrix[i][j] * x[j];
                x[i] = sum / luMatrix[i][i];
            }

            return x;
        }

        // -------------------------------------------------------------

        //static double[] SystemSolve(double[][] A, double[] b)
        //{
        //  // Solve Ax = b
        //  int n = A.Length;

        //  // 1. decompose A
        //  int[] perm;
        //  int toggle;
        //  double[][] luMatrix = MatrixDecompose(A, out perm, out toggle);
        //  if (luMatrix == null)
        //    return null;

        //  // 2. permute b according to perm[] into bp
        //  double[] bp = new double[b.Length];
        //  for (int i = 0; i < n; ++i)
        //    bp[i] = b[perm[i]];

        //  // 3. call helper
        //  double[] x = HelperSolve(luMatrix, bp);
        //  return x;
        //} // SystemSolve

        // -------------------------------------------------------------

        static public double[][] MatrixDuplicate(double[][] matrix)
        {
            // allocates/creates a duplicate of a matrix
            double[][] result = MatrixCreate(matrix.Length, matrix[0].Length);
            for (int i = 0; i < matrix.Length; ++i) // copy the values
                for (int j = 0; j < matrix[i].Length; ++j)
                    result[i][j] = matrix[i][j];
            return result;
        }

        // -------------------------------------------------------------

        static public double[][] ExtractLower(double[][] matrix)
        {
            // lower part of a Doolittle decomp (1.0s on diagonal, 0.0s in upper)
            int rows = matrix.Length; int cols = matrix[0].Length;
            double[][] result = MatrixCreate(rows, cols);
            for (int i = 0; i < rows; ++i)
            {
                for (int j = 0; j < cols; ++j)
                {
                    if (i == j)
                        result[i][j] = 1.0;
                    else if (i > j)
                        result[i][j] = matrix[i][j];
                }
            }
            return result;
        }

        static public double[][] ExtractUpper(double[][] matrix)
        {
            // upper part of a Doolittle decomp (0.0s in the strictly lower part)
            int rows = matrix.Length; int cols = matrix[0].Length;
            double[][] result = MatrixCreate(rows, cols);
            for (int i = 0; i < rows; ++i)
            {
                for (int j = 0; j < cols; ++j)
                {
                    if (i <= j)
                        result[i][j] = matrix[i][j];
                }
            }
            return result;
        }

        // -------------------------------------------------------------

        static public double[][] PermArrayToMatrix(int[] perm)
        {
            // convert Doolittle perm array to corresponding perm matrix
            int n = perm.Length;
            double[][] result = MatrixCreate(n, n);
            for (int i = 0; i < n; ++i)
                result[i][perm[i]] = 1.0;
            return result;
        }

        static public double[][] UnPermute(double[][] luProduct, int[] perm)
        {
            // unpermute product of Doolittle lower * upper matrix according to perm[]
            // no real use except to demo LU decomposition, or for consistency testing
            double[][] result = MatrixDuplicate(luProduct);

            int[] unperm = new int[perm.Length];
            for (int i = 0; i < perm.Length; ++i)
                unperm[perm[i]] = i;

            for (int r = 0; r < luProduct.Length; ++r)
                result[r] = luProduct[unperm[r]];

            return result;
        } // UnPermute
    }
}
