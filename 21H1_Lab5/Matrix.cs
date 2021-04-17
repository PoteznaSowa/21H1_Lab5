using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace _21H1_Lab5 {
	class Matrix {
		readonly int[,] matrix;  // Безпосередньо сам вміст матриці.
		readonly Random _rng = new(0);

		public int Rows => matrix.GetLength(0);
		public int Columns => matrix.GetLength(1);

		public int this[int row, int col] {
			get => matrix[row, col];
			set => matrix[row, col] = value;
		}

		public Matrix(int rows, int columns) {
			// Матриці не повинні бути або нульового або
			// від'ємного розміру, або занадто великі.
			if(rows is < 1 or > 20)
				throw new ArgumentOutOfRangeException(nameof(rows));
			if(columns is < 1 or > 20)
				throw new ArgumentOutOfRangeException(nameof(columns));
			matrix = new int[rows, columns];
		}

		public void Randomize() {
			// Заповнити числами від -1 до 1 включно.
			for(int i = 0; i < matrix.GetLength(0); i++)
				for(int j = 0; j < matrix.GetLength(1); j++)
					matrix[i, j] = _rng.Next(-1, 2);
		}

		public static Matrix operator *(Matrix left, Matrix right) {
			// Знайти добуток матриць.
			int l_cols = left.matrix.GetLength(1);
			int r_rows = right.matrix.GetLength(0);
			if(l_cols != r_rows)
				throw new ArgumentException(message:
					"Кількість стовпців першої матриці не дорівнює кількості рядків другої!"
					);
			int l_rows = left.matrix.GetLength(0);
			int r_cols = right.matrix.GetLength(1);
			Matrix result = new(l_rows, r_cols);
			for(int i = 0; i < l_rows; i++)
				for(int j = 0; j < r_cols; j++)
					result.matrix[i, j] = ScalarProduct(left, right, i, j);
			return result;
		}
		static int ScalarProduct(Matrix a, Matrix b, int row, int column) {
			// Обчислити скалярний добуток
			// вектор-рядків матриці A та вектор-стовпців матриці B.
			int result = 0;
			for(int i = 0; i < a.matrix.GetLength(0); i++)
				result += a.matrix[row, i] * b.matrix[i, column];
			return result;
		}

		public int Determinant() {
			int size = matrix.GetLength(0);
			return size != matrix.GetLength(1)
				? throw new InvalidOperationException(
					"Матриця не є квадратною, тому вона не має визначника."
					)
				: DeterminantOfMatrix(matrix, size, size);
		}
		static int DeterminantOfMatrix(int[,] mat, int n, int size) {
			int[,] A = mat;

			// Base cases
			switch(n) {
			case 1:
				return A[0, 0];
			case 2:
				return A[0, 0] * A[1, 1] - A[0, 1] * A[1, 0];
			case 3:
				return 0
					+ A[0, 0] * A[1, 1] * A[2, 2]
					+ A[1, 0] * A[2, 1] * A[0, 2]
					+ A[2, 0] * A[0, 1] * A[1, 2]

					- A[2, 0] * A[1, 1] * A[0, 2]
					- A[2, 1] * A[1, 2] * A[0, 0]
					- A[2, 2] * A[1, 0] * A[0, 1]
					;
			}

			int D = 0; // Initialize result

			// To store cofactors
			int[,] temp = new int[size, size];

			// To store sign multiplier
			int sign = 1;

			// Iterate for each element
			// of first row
			for(int f = 0; f < n; f++) {
				// Getting Cofactor of mat[0][f]
				GetCofactor(mat, temp, 0, f, n);
				D += sign * mat[0, f]
					 * DeterminantOfMatrix(temp, n - 1, size);

				// terms are to be added with
				// alternate sign
				sign = -sign;
			}
			return D;
		}
		static void GetCofactor(int[,] mat, int[,] temp, int p, int q, int n) {
			int i = 0, j = 0;

			// Looping for each element of
			// the matrix
			for(int row = 0; row < n; row++) {
				for(int col = 0; col < n; col++) {

					// Copying into temporary matrix
					// only those element which are
					// not in given row and column
					if(row != p && col != q) {
						temp[i, j++] = mat[row, col];

						// Row is filled, so increase
						// row index and reset col
						// index
						if(j == n - 1) {
							j = 0;
							i++;
						}
					}
				}
			}
		}

		public Matrix GetTranspose() {
			int rows = matrix.GetLength(1);
			int columns = matrix.GetLength(0);
			Matrix result = new(rows, columns);
			for(int i = 0; i < rows; i++)
				for(int j = 0; j < columns; j++)
					result.matrix[i, j] = matrix[j, i];
			return result;
		}

		public Matrix GetInverse() {
			int rows = matrix.GetLength(0);
			int columns = matrix.GetLength(1);
			if(rows != columns)
				throw new InvalidOperationException(message:
					"Обернену матрицю можна зробити тільки з квадратної."
					);
			int N = rows;

			// Find determinant of [,]A
			int det = Determinant();
			if(det == 0)
				throw new InvalidOperationException(message:
					"Визначник матриці дорівнює нулю. Не можна знайти обернену матрицю."
					);

			int[,] A = matrix;
			Matrix result = new(rows, columns);
			int[,] inverse = result.matrix;

			// Find adjoint
			int[,] adj = new int[N, N];
			Adjoint(A, adj);

			// Find Inverse using formula "inverse(A) = adj(A)/det(A)"
			for(int i = 0; i < N; i++)
				for(int j = 0; j < N; j++)
					inverse[i, j] = adj[i, j] / det;

			return result;
		}
		// Function to get adjoint of A[N,N] in adj[N,N].
		static void Adjoint(int[,] A, int[,] adj) {
			int N = A.GetLength(0);
			if(N == 1) {
				adj[0, 0] = 1;
				return;
			}

			// temp is used to store cofactors of [,]A
			int[,] temp = new int[N, N];

			for(int i = 0; i < N; i++) {
				for(int j = 0; j < N; j++) {
					// Get cofactor of A[i,j]
					GetCofactor(A, temp, i, j, N);

					// sign of adj[j,i] positive if sum of row
					// and column indexes is even.
					int sign = (((i + j) & 1) == 0) ? 1 : -1;

					// Interchanging rows and columns to get the
					// transpose of the cofactor matrix
					adj[j, i] = sign * DeterminantOfMatrix(temp, N - 1, N);
				}
			}
		}
	}
}
