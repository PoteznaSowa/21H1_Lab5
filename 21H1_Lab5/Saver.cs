﻿using System;
using System.IO;

namespace _21H1_Lab5 {
	class Saver {
		const int number_of_ptrs = 16;
		const int ptr_table_size = sizeof(ushort) * number_of_ptrs;
		//const int arr_max_size = 20;

		const int file_size = ptr_table_size;

		static Saver _instance = null;

		public static Saver Instance() {
			if(_instance == null) {
				_instance = new Saver();
			}

			return _instance;
		}

		FileStream _file = null;
		void CloseFile() {
			Count = -1;
			if(_file != null) {
				_file.Flush(true);
			}
		}

		public Saver() => Count = -1;

		~Saver() => CloseFile();

		static int SizeOfMatrix(int rows, int columns)
			=> 2 + sizeof(double) * rows * columns;

		public int Count {
			get; private set;
		}
		void SetCount() {
			Count = 0;
			_file.Position = 0;
			BinaryReader reader = new(_file);
			for(int i = 0; i < number_of_ptrs; i++) {
				if(reader.ReadUInt16() != 0) {
					Count++;
				} else {
					return;
				}
			}
		}

		public void OpenFile(string path, bool clear) {
			CloseFile();
			if(!clear && File.Exists(path)) {
				_file = File.Open(path, FileMode.Open);
				SetCount();
			} else {
				_file = File.Create(path);
				Clear();
			}
		}

		public bool Add(Matrix matrix) {
			if(Count is (-1) or >= 12) {
				return false;
			}
			if(Count == 0) {
				WriteToFile(matrix, Count++, ptr_table_size);
				return true;
			}

			int size = SizeOfMatrix(matrix.Rows, matrix.Columns);
			_file.Position = 0;
			BinaryReader reader = new(_file);
			int[] ptrs = new int[Count + 1];
			for(int i = 1; i <= Count; i++) {
				ptrs[i] = reader.ReadUInt16();
			}

			Array.Sort(ptrs);
			int newptr = 0;
			for(int i = 0; i <= Count; i++) {
				if(i == 0) {
					newptr = ptr_table_size;
				} else {
					newptr = ptrs[i];
					_file.Position = newptr;
					newptr += SizeOfMatrix(reader.ReadByte(), reader.ReadByte());
				}
				if(i == Count || newptr + size <= ptrs[i + 1]) {
					break;
				}
			}
			WriteToFile(matrix, Count++, newptr);
			return true;
		}
		void WriteToFile(Matrix matrix, int num, int ptr) {
			_file.Position = num << 1;
			BinaryWriter writer = new(_file);
			writer.Write((ushort)ptr);
			_file.Position = ptr;
			int rows = matrix.Rows;
			int cols = matrix.Columns;
			writer.Write((byte)rows);
			writer.Write((byte)cols);
			for(int i = 0; i < rows; i++) {
				for(int j = 0; j < cols; j++) {
					writer.Write(matrix[i, j]);
				}
			}
			_file.Flush();
		}

		public Matrix this[int number] {
			get {
				if(number >= Count || number >= number_of_ptrs) {
					throw new ArgumentOutOfRangeException(nameof(number));
				}
				_file.Position = number << 1;
				BinaryReader reader = new(_file);
				_file.Position = reader.ReadUInt16();
				int rows = reader.ReadByte();
				int cols = reader.ReadByte();
				Matrix matrix = new(rows, cols);
				for(int i = 0; i < rows; i++) {
					for(int j = 0; j < cols; j++) {
						matrix[i, j] = reader.ReadDouble();
					}
				}

				return matrix;
			}
			set {
				if(number >= Count || number >= number_of_ptrs) {
					throw new ArgumentOutOfRangeException(nameof(number));
				}
				int rows = value.Rows;
				int cols = value.Columns;
				int size = SizeOfMatrix(rows, cols);
				_file.Position = number << 1;
				BinaryReader reader = new(_file);
				int ptr = reader.ReadUInt16();
				_file.Position = ptr;
				if(SizeOfMatrix(reader.ReadByte(), reader.ReadByte()) >= size) {
					WriteToFile(value, number, ptr);
					return;
				}

				int[] ptrs = new int[Count];
				_file.Position = 0;
				for(int i = 0; i < Count; i++) {
					ptrs[i] = reader.ReadUInt16();
				}

				Array.Sort(ptrs);
				ptr = ptrs[^1];
				_file.Position = ptr;
				int newptr = ptr + SizeOfMatrix(_file.ReadByte(), _file.ReadByte());
				WriteToFile(value, number, newptr);
			}
		}

		public void Remove(int number) {
			int[] ptrs = new int[Count];
			BinaryReader reader = new(_file);
			for(int i = 0; i < Count; i++) {
				ptrs[i] = reader.ReadUInt16();
			}

			int idx = number;
			do {
				ptrs[idx] = ptrs[idx + 1];
			} while(++idx < Count - 1);
			ptrs[idx] = 0;
			BinaryWriter writer = new(_file);
			for(int i = 0; i < Count; i++) {
				writer.Write((ushort)ptrs[i]);
			}

			Count--;
			_file.Flush();
		}

		public void Defragment() {
			byte[][] arrays = new byte[Count][];
			int[] ptrs = new int[Count];
			BinaryReader reader = new(_file);
			_file.Position = 0;
			for(int i = 0; i < Count; i++) {
				ptrs[i] = reader.ReadUInt16();
			}

			int idx = 0;
			foreach(int ptr in ptrs) {
				_file.Position = ptr;
				int rows = _file.ReadByte();
				int cols = _file.ReadByte();
				_file.Position -= 2;
				arrays[idx++] = reader.ReadBytes(SizeOfMatrix(rows, cols));
			}
			int newptr = ptr_table_size;
			_file.Position = 0;
			_file.SetLength(file_size);
			BinaryWriter writer = new(_file);
			foreach(byte[] array in arrays) {
				writer.Write((ushort)newptr);
				newptr += array.Length;
			}
			_file.Position = ptr_table_size;
			foreach(byte[] array in arrays) {
				_file.Write(array);
			}
			_file.Flush();
		}

		public void Clear() {
			_file.Position = 0;
			_file.SetLength(file_size);
			_file.Write(new byte[sizeof(ushort) * number_of_ptrs]);
			Count = 0;
			_file.Flush();
		}
	}
}
