using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace NumApproximation
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            chart1.Series[0].Points.Clear();
            chart1.Series[1].Points.Clear();

            int nodesNum; // Количество узлов
            double[,] nodes; // Координаты узлов
            double[,] MatrixA = new double[2, 2]; // Матрица коэффициентов СЛАУ
            double[] VectorB = new double[2]; // Столбец свобоных членов СЛАУ

            string inputFilePath = "nodes.in";
            using (StreamReader sstream = new StreamReader(inputFilePath))
            {
                string line;
                // Количество узлов
                line = sstream.ReadLine();
                nodesNum = int.Parse(line);
                Console.WriteLine("In file " + inputFilePath + " discovered " + nodesNum + " nodes.");
                // Объявление массива
                nodes = new double[nodesNum, 2];
                MatrixA[0, 0] = nodesNum; // Заполнение массива коэффциентов

                // Запись данныых в массив
                for (int i = 0; i < nodesNum; ++i)
                {
                    line = sstream.ReadLine();
                    Console.WriteLine("Node " + i + ": " + line);
                    string[] tokens = line.Split();
                    nodes[i, 0] = double.Parse(tokens[0]);
                    nodes[i, 1] = double.Parse(tokens[1]);

                    /* Заполнение массива коэффциентов и свободных членов СЛАУ */
                    MatrixA[0, 1] += Math.Log(nodes[i, 0]); // ln(x)
                    MatrixA[1, 0] += Math.Log(nodes[i, 0]); // ln(x)
                    MatrixA[1, 1] += Math.Pow(Math.Log(nodes[i, 0]), 2); // ln^2(x)

                    VectorB[0] += Math.Log(nodes[i, 1]); // ln(y)
                    VectorB[1] += Math.Log(nodes[i, 1]) * Math.Log(nodes[i, 0]); // ln(y) * ln(x)

                    /* Рисование узлов в чарте */
                    chart1.Series[1].Points.AddXY(nodes[i, 0], nodes[i, 1]);
                }
            }

            /* Отладочный вывод */
            Console.Write("MatrixA: ");
            foreach (double element in MatrixA)
            {
                Console.Write("{0:f4}\t", element);
            }
            Console.Write("\nVectorB: ");
            foreach (double element in VectorB)
            {
                Console.Write("{0:f4}\t", element);
            }
            Console.WriteLine();

            /* Решение СЛАУ */
            LinearSystem system = new LinearSystem(MatrixA, VectorB); // Создание класса СЛАУ
            Console.Write("a = {0:f5}\n", system.XVector[0]);
            Console.Write("b = {0:f5}\n", system.XVector[1]);

            /* Рисование графика аппроксимации */
            for (double x = nodes[0,0]; x <= nodes[nodesNum - 1, 0]; x += 0.01) // От первого до последнего x в nodes
            {
                chart1.Series[0].Points.AddXY(x, Math.Exp(system.XVector[0]) * Math.Pow(x, system.XVector[1])); // e^a * x^b
            }

        }
    }
}
