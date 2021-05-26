using System;

using _21H1_Lab5;

// Уперше: програма без методу Main!
Matrix m1 = new(20, 20);
m1.Randomize();
Matrix m2 = new(20, 20);
m2.Randomize();
Matrix m3 = new(20, 20);
m3.Randomize();
Matrix m4 = new(20, 20);
m4.Randomize();
Matrix m5 = new(20, 20);
m5.Randomize();

Saver saver = Saver.Instance();
saver.OpenFile("MATRICES.DAT", true);
saver.Add(m1);
saver.Add(m2);
saver.Add(m3);
saver.Add(m4);
saver.Add(m5);
Matrix dets = new(1, 5);

dets[0, 0] = m1.Determinant();
dets[0, 1] = m2.Determinant();
dets[0, 2] = m3.Determinant();
dets[0, 3] = m4.Determinant();
dets[0, 4] = m5.Determinant();
saver.Add(dets);

m1 *= m2;
saver.Add(m1);
m1 *= m3;
saver.Add(m1);
m1 *= m4;
saver.Add(m1);
m1 *= m5;
saver.Add(m1);

saver.Add(m1.GetInverse());

m1 = new(10, 10);
m1.Randomize();
saver[0] = m1;
m2 = new(10, 10);
m2.Randomize();
saver[1] = m2;
m3 = new(10, 10);
m3.Randomize();
saver[2] = m3;
m4 = new(10, 10);
m4.Randomize();
saver[3] = m4;
m5 = new(10, 10);
m5.Randomize();
saver[4] = m5;

saver.Add(m1 * m2 * m3 * m4 * m5);
saver.Defragment();
Console.WriteLine(saver.Count);

while(Console.KeyAvailable) {
	Console.ReadKey();
}
Console.WriteLine("Роботу програми завершено. Натиснiть Return, щоб продовжити...");
while(Console.ReadKey(true).Key != ConsoleKey.Enter) { }
