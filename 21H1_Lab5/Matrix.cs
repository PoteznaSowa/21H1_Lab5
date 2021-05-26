using System;

namespace _21H1_Lab5 {
	class Matrix {
		readonly double[,] matrix;  // Безпосередньо сам вміст матриці.
		readonly Random _rng = new(0);

		public int Rows => matrix.GetLength(0);
		public int Columns => matrix.GetLength(1);

		public double this[int row, int col] {
			get => matrix[row, col];
			set => matrix[row, col] = value;
		}

		public Matrix(int rows, int columns) {
			// Матриці не повинні бути або нульового або
			// від'ємного розміру, або занадто великі.
			if(rows is < 1 or > 20) {
				throw new ArgumentOutOfRangeException(nameof(rows));
			}
			if(columns is < 1 or > 20) {
				throw new ArgumentOutOfRangeException(nameof(columns));
			}

			matrix = new double[rows, columns];
		}

		public void Randomize() {
			// Заповнити числами від -1 до 1 включно.
			for(int i = 0; i < matrix.GetLength(0); i++) {
				for(int j = 0; j < matrix.GetLength(1); j++) {
					matrix[i, j] = _rng.Next(-1, 2);
				}
			}
		}

		public static Matrix operator *(Matrix left, Matrix right) {
			// Знайти добуток матриць.
			int l_cols = left.matrix.GetLength(1);
			int r_rows = right.matrix.GetLength(0);
			if(l_cols != r_rows) {
				throw new ArgumentException(message:
					"Кількість стовпців першої матриці не дорівнює кількості рядків другої!"
					);
			}

			int l_rows = left.matrix.GetLength(0);
			int r_cols = right.matrix.GetLength(1);
			Matrix result = new(l_rows, r_cols);
			for(int i = 0; i < l_rows; i++) {
				for(int j = 0; j < r_cols; j++) {
					result.matrix[i, j] = ScalarProduct(left, right, i, j);
				}
			}

			return result;
		}
		static double ScalarProduct(Matrix a, Matrix b, int row, int column) {
			// Обчислити скалярний добуток
			// вектор-рядків матриці A та вектор-стовпців матриці B.
			double result = 0;
			for(int i = 0; i < a.matrix.GetLength(0); i++) {
				result += a.matrix[row, i] * b.matrix[i, column];
			}

			return result;
		}

		static unsafe double Det(double* rmX, int n) {
			double* mtx_u_ii;
			double* mtx_ii_j;
			double* mtx_end = rmX + n * (n - 1);
			double* mtx_u_ii_j = null;
			double val;
			double det = 1;
			int d = 0;

			// rmX указывает на (i,i) элемент на каждом шаге и называется ведущим
			for(double* mtx_ii_end = rmX + n; rmX < mtx_end; rmX += n + 1, mtx_ii_end += n, d++) {
				// Ищем максимальный элемент в столбце(под ведущим) 

				//Ищем максимальный элемент и его позицию
				val = Math.Abs(*(mtx_ii_j = rmX));
				for(mtx_u_ii = rmX + n; mtx_u_ii < mtx_end; mtx_u_ii += n) {
					if(val < Math.Abs(*mtx_u_ii)) {
						val = Math.Abs(*(mtx_ii_j = mtx_u_ii));
					}
				}

				if(val == 0) {
					//Если максимальный эдемент = 0 -> матрица вырожденная
					return double.NaN;
				} else if(mtx_ii_j != rmX) {
					//Если ведущий элемент не является максимальным -
					//делаем перестановку строк и меняем знак определителя
					det = -det;
					for(mtx_u_ii = rmX; mtx_u_ii < mtx_ii_end; mtx_ii_j++, mtx_u_ii++) {
						val = *mtx_u_ii;
						*mtx_u_ii = *mtx_ii_j;
						*mtx_ii_j = val;
					}
				}

				//Обнуляем элементы под ведущим
				for(mtx_u_ii = rmX + n, mtx_u_ii_j = mtx_end + n; mtx_u_ii < mtx_u_ii_j; mtx_u_ii += d) {
					val = *(mtx_u_ii++) / *rmX;
					for(mtx_ii_j = rmX + 1; mtx_ii_j < mtx_ii_end; mtx_u_ii++, mtx_ii_j++) {
						*mtx_u_ii -= *mtx_ii_j * val;
					}
				}
				det *= *rmX;
			}
			return det *= *rmX;
		}
		public double Determinant() {
			return Determinant(matrix);
		}
		static unsafe double Determinant(double[,] matrix) {
			int n = matrix.GetLength(0);
			if(n != matrix.GetLength(1)) {
				throw new InvalidOperationException(
					"Матриця не є квадратною, тому вона не має визначника."
					);
			}

			double[] temp = new double[matrix.Length];
			Buffer.BlockCopy(matrix, 0, temp, 0, temp.Length * sizeof(double));
			fixed(double* pm = &temp[0]) {
				return Det(pm, n);
			}
		}

		public Matrix GetTranspose() {
			int rows = matrix.GetLength(1);
			int columns = matrix.GetLength(0);
			Matrix result = new(rows, columns);
			for(int i = 0; i < rows; i++) {
				for(int j = 0; j < columns; j++) {
					result.matrix[i, j] = matrix[j, i];
				}
			}

			return result;
		}

		public Matrix GetInverse() {
			int m = matrix.GetLength(0);
			int n = matrix.GetLength(1);
			if(m != n) {
				throw new InvalidOperationException(message:
					"Обернену матрицю можна зробити тільки з квадратної."
					);
			}

			double det = Determinant();

			if(det == 0) {
				return null;
			}

			Matrix transposed = GetTranspose();
			Matrix DetMat = new(n, n);
			int size = matrix.GetLength(0);
			for(int i = 0; i < n; i++) {
				for(int j = 0; j < n; j++) {
					double[,] smallMatrix = new double[size - 1, size - 1];

					int row = 0;
					int col = 0;
					for(int k = 0; k < size; k++) {
						for(int l = 0; l < size; l++) {
							if(k == i || l == j) {
								continue;
							}

							smallMatrix[row, col] = transposed.matrix[k, l];
							col++;
						}
						if(col == smallMatrix.GetLength(0)) {
							row++;
							col = 0;
						}
					}

					DetMat.matrix[i, j] =
						(((i - j) & 1) == 0  // Перевірка парності суми номера рядка та номера стовпця.
						? Determinant(smallMatrix)
						: -Determinant(smallMatrix))
						/ det;
				}
			}

			return DetMat;
		}
	}
}
