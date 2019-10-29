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

        private void Button1_Click(object sender, EventArgs e)
        {
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
        }
    }
}
