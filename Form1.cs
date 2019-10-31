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
        List<List<double>> Nodes = new List<List<double>>() { new List<double>(), new List<double>() };

        public Form1()
        {
            InitializeComponent();
        }
    static double Income(double x1, double x2, double x3, double[] coef)
    {
      // x1 = education, x2 = work, x3 = sex
      double result; // the constant
      result = coef[0] + (x1 * coef[1]) + (x2 * coef[2]) + (x3 * coef[3]);
      return result;
    }

    static double RSquared(double[][] data, double[] coef)
    {
      // 'coefficient of determination'
      int rows = data.Length;
      int cols = data[0].Length;

      // 1. compute mean of y
      double ySum = 0.0;
      for (int i = 0; i < rows; ++i)
        ySum += data[i][cols - 1]; // last column
      double yMean = ySum / rows;
 
      // 2. sum of squared residuals & tot sum squares
      double ssr = 0.0;
      double sst = 0.0;
      double y; // actual y value
      double predictedY; // using the coef[] 
      for (int i = 0; i < rows; ++i)
      {
        y = data[i][cols - 1]; // get actual y

        predictedY = coef[0]; // start w/ intercept constant
        for (int j = 0; j < cols - 1; ++j) // j is col of data
          predictedY += coef[j+1] * data[i][j]; // careful
        
        ssr += (y - predictedY) * (y - predictedY);
        sst += (y - yMean) * (y - yMean);
      }

      if (sst == 0.0)
        throw new Exception("All y values equal");
      else
        return 1.0 - (ssr / sst);
     }

    static double[][] DummyData(int rows, int seed)
    {
      // generate dummy data for linear regression problem
      double b0 = 15.0;
      double b1 = 0.8; // education years
      double b2 = 0.5; // work years
      double b3 = -3.0; // sex = 0 male, 1 female
      Random rnd = new Random(seed);

      double[][] result = new double[rows][];
      for (int i = 0; i < rows; ++i)
        result[i] = new double[4];

      for (int i = 0; i < rows; ++i)
      {
        int ed = rnd.Next(12, 17); // 12, 16]
        int work = rnd.Next(10, 31); // [10, 30]
        int sex = rnd.Next(0, 2); // 0 or 1
        double y = b0 + (b1 * ed) + (b2 * work) + (b3 * sex);
        y += 10.0 * rnd.NextDouble() - 5.0; // random [-5 +5]

        result[i][0] = ed;
        result[i][1] = work;
        result[i][2] = sex;
        result[i][3] = y; // income
      }
      return result;
    }

    static double[][] Design(double[][] data)
    {
      // add a leading col of 1.0 values
      int rows = data.Length;
      int cols = data[0].Length;
      double[][] result = MatrixCreate(rows, cols + 1);
      for (int i = 0; i < rows; ++i)
        result[i][0] = 1.0;

      for (int i = 0; i < rows; ++i)
        for (int j = 0; j < cols; ++j)
          result[i][j + 1] = data[i][j];

      return result;
    }

    static double[] Solve(double[][] design)
    {
      // find linear regression coefficients
      // 1. peel off X matrix and Y vector
      int rows = design.Length;
      int cols = design[0].Length;
      double[][] X = MatrixCreate(rows, cols - 1);
      double[][] Y = MatrixCreate(rows, 1); // a column vector

      int j;
      for (int i = 0; i < rows; ++i)
      {
        for (j = 0; j < cols - 1; ++j)
        {
          X[i][j] = design[i][j];
        }
        Y[i][0] = design[i][j]; // last column
      }

      // 2. B = inv(Xt * X) * Xt * y
      double[][] Xt = MatrixTranspose(X);
      double[][] XtX = MatrixProduct(Xt, X);
      double[][] inv = MatrixInverse(XtX);
      double[][] invXt = MatrixProduct(inv, Xt);

      double[][] mResult = MatrixProduct(invXt, Y);
      double[] result = MatrixToVector(mResult);
      return result;
    } // Solve


    static void ShowMatrix(double[][] m, int dec)
    {
      for (int i = 0; i < m.Length; ++i)
      {
        for (int j = 0; j < m[i].Length; ++j)
        {
          Console.Write(m[i][j].ToString("F" + dec) + "  ");
        }
        Console.WriteLine("");
      }
    }

    static void ShowVector(double[] v, int dec)
    {
      for (int i = 0; i < v.Length; ++i)
        Console.Write(v[i].ToString("F" + dec) + "  ");
      Console.WriteLine("");
    }

        private void Button1_Click(object sender, EventArgs e)
        {
            chart1.Series[0].Points.Clear();
            chart1.Series[1].Points.Clear();

            int rows = 10;
            double[][] data = new double[rows][];
            double[,] MatrixA = new double[2, 2];
            MatrixA[0, 0] = rows;
            MatrixA[0, 1] = 0;
            MatrixA[1, 0] = 0;
            MatrixA[1, 1] = 0;
            double[] VectorB = new double[2];
            VectorB[0] = 0;
            VectorB[1] = 0;

            using (StreamReader sstream = new StreamReader("nodes2.in"))
            {
                string line;
                for(int i = 0; i < rows; ++i)
                {
                    line = sstream.ReadLine();
                    Console.WriteLine("Node " + i + ": " + line);

                    data[i] = new double[2];
                    string[] tokens = line.Split();
                    data[i][0] = double.Parse(tokens[0]);
                    data[i][1] = double.Parse(tokens[1]);

                    //Заполнение матрицы A
                    MatrixA[0, 1] += Math.Log(double.Parse(tokens[0]));
                    MatrixA[1, 0] += Math.Log(double.Parse(tokens[0]));
                    MatrixA[1, 1] += Math.Log(double.Parse(tokens[0])) * Math.Log(double.Parse(tokens[0]));
                    Console.WriteLine(MatrixA[0, 1]);
                    Console.WriteLine(MatrixA[1, 0]);
                    Console.WriteLine(MatrixA[1, 1]);
                    //Заполнение матрицы B
                    double xs = (2.0 + 2.9) / 2;
                    double ys = 1.52724;
                    double C = ((0.0000462 * (-0.52634)) - ys * ys) / (0.0000462 - 0.52634 - 2 * ys);
                    Console.WriteLine("C:           "+C);
                    VectorB[0] += Math.Log(double.Parse(tokens[1]) - C + 3);
                    VectorB[1] += Math.Log(double.Parse(tokens[1]) - C + 3) * Math.Log(double.Parse(tokens[0]));
                    Console.WriteLine(VectorB[0]);
                    Console.WriteLine(VectorB[1]);
                    chart1.Series[1].Points.AddXY(double.Parse(tokens[0]), double.Parse(tokens[1]));
                }
            }


            double xxs = (2.0 + 2.9) / 2;
            double yys = 1.52724;
            double CC = ((0.0000462 * (-0.52634)) - yys * yys) / (0.0000462 - 0.52634 - 2 * yys);

            LinearSystem system = new LinearSystem(MatrixA, VectorB);
            Console.WriteLine(system.XVector[0]);
            Console.WriteLine(system.XVector[1]);
            double a = Math.Exp(system.XVector[0]);
            double b = system.XVector[1];
            for (double x = 2.0; x <= 2.9; x += 0.01)
            {
                //chart1.Series[0].Points.AddXY(x, system.XVector[0] + system.XVector[1]*x);
                //chart1.Series[0].Points.AddXY(x, Math.Exp(system.XVector[0]) * Math.Pow(x, Math.Exp(system.XVector[1])));
                chart1.Series[0].Points.AddXY(x, a * Math.Pow(x, b));
            }

            /*
            Console.WriteLine("Creating " + rows + " rows synthetic data");

            //double[][] data = MatrixLoad("..\\..\\IncomeData.txt", true, ',');

            Console.WriteLine("Education-Work-Sex-Income data:\n");
            ShowMatrix(data, 2);

            
            Console.WriteLine("\nCreating design matrix from data");
            double[][] design = Design(data); // 'design matrix'
            Console.WriteLine("Done\n");

            Console.WriteLine("Design matrix:\n");
            ShowMatrix(design, 2);

            Console.WriteLine("\nFinding coefficients using inversion");
            double[] coef = Solve(design); // use design matrix
            Console.WriteLine("Done\n");

            Console.WriteLine("Coefficients are:\n");
            ShowVector(coef, 4);
            Console.WriteLine("");

            Console.WriteLine("Coefficients:\n");
            Console.WriteLine(coef[0]);
            Console.WriteLine(coef[1]);

            for (double x = 0.4; x <= 2.4; x += 0.01)
            {
                chart1.Series[0].Points.AddXY(x, Math.Exp(coef[0] + coef[1]*x));
            }
            */
            /*
            Console.WriteLine("Computing R-squared\n");
            double R2 = RSquared(data, coef); // use initial data
            Console.WriteLine("R-squared = " + R2.ToString("F4"));

            Console.WriteLine("\nPredicting income for ");
            Console.WriteLine("Education = 14");
            Console.WriteLine("Work      = 12");
            Console.WriteLine("Sex       = 0 (male)");

            double y = Income(14, 12, 0, coef);
            Console.WriteLine("\nPredicted income = " + y.ToString("F2"));

            Console.WriteLine("\nEnd linear regression demo\n");
            Console.ReadLine();
            */

            /*
    
            chart1.Series[0].Points.Clear();
            chart1.Series[1].Points.Clear();

           
            int nodesNum = 0;
            using (StreamReader sstream = new StreamReader("nodes.in"))
            {
                string line;
                while ((line = sstream.ReadLine()) != null)
                {
                    ++nodesNum;
                    Console.WriteLine("Node "+nodesNum+": "+line);
                    string[] tokens = line.Split();
                    Nodes[0].Add(double.Parse(tokens[0]));
                    Nodes[1].Add(double.Parse(tokens[1]));

                    chart1.Series[1].Points.AddXY(double.Parse(tokens[0]), double.Parse(tokens[1]));
                }
            }
            */
        }
    }
}
