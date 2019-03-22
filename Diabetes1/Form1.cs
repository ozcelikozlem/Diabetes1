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

namespace Diabetes1
{
    public partial class Form1 : Form
    {
        static DataTable data = new DataTable();
        static DataTable data1 = new DataTable();
        double[] mak = { 17, 197, 122, 63, 846, 67.1, 2.42, 81, 1 };
        double[] min = { 0, 0, 0, 0, 0, 0, 0.078, 21, 0 };
        double B2 = 0.1; //cikis bias
        double[] Op1 = new double[4];// ara katman sonuc
        double[] TsOp1 = new double[8];// ara katman sonuc
        double[] Op2=new double[10000]; // cikis sonuc
        double TsOp2;
        double mk = 0.5; //momentum katsayisi
        double ok = 0.5; /// ogrenme katsayisi
        double[] A1hatafaktoru = new double[4];
       
        double ShataFaktoru = 0;
        // giris agirlik
        double[,] W1 = { { 0.1, 0.1, 0.1, 0.1 }, { 0.1, 0.1, 0.1, 0.1 }, { 0.1, 0.1, 0.1, 0.1 }, { 0.1, 0.1, 0.1, 0.1 }, { 0.1, 0.1, 0.1, 0.1 }, { 0.1, 0.1, 0.1, 0.1 }, { 0.1, 0.1, 0.1, 0.1 }, { 0.1, 0.1, 0.1, 0.1 } };
        // ara katman agirlik 
        double[] W2 = { 0.1, 0.1, 0.1, 0.1 };
        //cikis agirlik
        double[,] W1y = new double[8, 4];
        double[] W2y = new double[4];
        double[] W3y = new double[4];
        double[] B1 = { 0.1, 0.1, 0.1, 0.1 };  // 1. ara katman bias
        double[] B1y = new double[4];
        double B2y;

        double[] CW2 = { 0, 0, 0, 0 };
        double[,] CW1 = { { 0, 0, 0, 0 }, { 0, 0, 0, 0 }, { 0, 0, 0, 0 }, { 0, 0, 0, 0 }, { 0, 0, 0, 0 }, { 0, 0, 0, 0 }, { 0, 0, 0, 0 }, { 0, 0, 0, 0 } };

        double CB2 = 0;
        double[] CB1 = { 0, 0, 0, 0 };
        //değişme foktoru
        double sigmoidd(double sonuc)
        {

            double net = (1 / (1 + (Math.Exp(-sonuc))));


            return net;

        }
        static DataTable GenerateTrainingData()
        {


            var rows = 0;


            using (var reader = new StreamReader(File.OpenRead("C:/Users/ozcelikkasim/Desktop/diabetes.csv")))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Substring(0, line.Length).Split(';');

                    foreach (var item in values)
                    {
                        if (string.IsNullOrEmpty(item) || string.IsNullOrWhiteSpace(item))
                        {
                            throw new Exception("Value can't be empty");
                        }

                        if (rows == 0)
                        {
                            data.Columns.Add(item);
                        }
                    }

                    if (rows > 0)
                    {
                        data.Rows.Add(values);
                    }

                    rows++;

                    if (values.Length != data.Columns.Count)
                    {
                        throw new Exception("Row is shorter or longer than title row");
                    }
                }
            }



            // if no rows are entered or data == null, return null
            return data?.Rows.Count > 0 ? data : null;

        }
        static DataTable GenerateTestingData()
        {


            var rows = 0;


            using (var reader = new StreamReader(File.OpenRead("C:/Users/ozcelikkasim/Desktop/testt.csv")))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Substring(0, line.Length).Split(';');

                    foreach (var item in values)
                    {
                        if (string.IsNullOrEmpty(item) || string.IsNullOrWhiteSpace(item))
                        {
                            throw new Exception("Value can't be empty");
                        }

                        if (rows == 0)
                        {
                            data1.Columns.Add(item);
                        }
                    }

                    if (rows > 0)
                    {
                        data1.Rows.Add(values);
                    }

                    rows++;

                    if (values.Length != data1.Columns.Count)
                    {
                        throw new Exception("Row is shorter or longer than title row");
                    }
                }
            }



            // if no rows are entered or data == null, return null
            return data1?.Rows.Count > 0 ? data1 : null;

        }
        double Normalizasyon(double xmin, double xmax, double x)
        {
            return ((x - xmin) / (xmax - xmin));

        }
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {


            GenerateTrainingData();
            GenerateTestingData();
            dataGridView1.DataSource = data;  //egitim
            dataGridView2.DataSource = data1; //test

            int epoch = 0;
            int iterasyon = 0;

            // EĞİTİM
            for (int i = 0; i <300; i++)
            {
                double Ascn1 = 0;//1.Arakatman toplam
               //ouble Ascn2 = 0; //2.Arakatman toplam
                epoch++;
                for (int j = 0; j < 460; j++)
                {

                    iterasyon++;
                    // 1.ara katman sonuclarin hesaplanmasi
                    for (int m = 0; m < 4; m++)//katman
                    {

                        for (int n = 0; n < 8; n++)//giris
                        {
                            double giris = Convert.ToDouble(dataGridView1.Rows[j].Cells[n].Value.ToString());

                            Ascn1 += Normalizasyon(min[n], mak[n], giris) * W1[n, m];



                        }

                        Op1[m] = sigmoidd(Ascn1 + B1[m]);



                    }




                    double Ascn3 = 0;
                    for (int s = 0; s < 4; s++)
                    {


                        Ascn3 += Op1[s] * W2[s];


                    }

                    Op2[j] = sigmoidd(Ascn3 + B2);
                
                    // label1.Text = Op2.ToString();

                    double gercek = Convert.ToDouble(dataGridView1.Rows[j].Cells[8].Value.ToString());
                    if (Op2[j] == gercek)
                    {
                        
                            dataGridView4.Rows.Add();
                            dataGridView4.Rows[i].Cells[0].Value = epoch.ToString();
                            dataGridView4.Rows[i].Cells[1].Value = iterasyon.ToString();
                            dataGridView4.Rows[i].Cells[2].Value = Op2[j].ToString();

                        
                    }
                    else
                    {
                       
                      dataGridView4.Rows.Add();
                        dataGridView4.Rows[i].Cells[0].Value = epoch.ToString();
                       dataGridView4.Rows[i].Cells[1].Value = iterasyon.ToString();
                        dataGridView4.Rows[i].Cells[2].Value = Op2[j].ToString();
                        chart1.ChartAreas[0].AxisY.ScaleView.Zoom(0, 1);
                        chart1.ChartAreas[0].AxisX.ScaleView.Zoom(0, iterasyon);
                        chart1.ChartAreas[0].CursorX.IsUserEnabled = true;
                        chart1.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
                        chart1.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
                        
                            chart1.Series[0].Points.AddXY(j, Op2[j]);
                            chart1.Series[0].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                      


                        ShataFaktoru = (gercek - Op2[j]) * Op2[j] * (1 - Op2[j]);
                            B2y = B2 + (gercek - Op2[j]);
                            B2 = B2y;

                            for (int x = 0; x < 4; x++)
                            {
                                CW2[x] = ok * ShataFaktoru * Op1[x] + mk * CW2[x];

                                A1hatafaktoru[x] = Op1[x] * (1 - Op1[x]) * ShataFaktoru * W2[x];
                                W2y[x] = W2[x] + CW2[x];
                                W2[x] = W2y[x];

                            }

                            for (int t = 0; t < 8; t++)
                            {


                                for (int r = 0; r < 4; r++)
                                {
                                    double giris = Normalizasyon(min[t], mak[t], Convert.ToDouble(dataGridView1.Rows[j].Cells[t].Value.ToString()));


                                    CW1[t, r] = ok * A1hatafaktoru[r] * giris + mk * CW1[t, r];
                                    W1y[t, r] = W1[t, r] + CW1[t, r];
                                    W1[t, r] = W1y[t, r];
                                    CB1[r] = ok * A1hatafaktoru[r] + mk * CB1[r];
                                    B1y[r] = B1[r] + CB1[r];
                                    B1[r] = B1y[r];
                                }

                            }

                        
                    }
                }
            }


            //TEST
            int iterasyon1= 0;
            for (int j = 0; j < 300; j++)
            {
             double  Ascn1 = 0;
                iterasyon1++;
                // 1.ara katman sonuclarin hesaplanmasi
                for (int m = 0; m < 4; m++)//katman
                {

                    for (int n = 0; n < 8; n++)//giris
                    {
                        double giris = Convert.ToDouble(dataGridView2.Rows[j].Cells[n].Value.ToString());

                        Ascn1 += Normalizasyon(min[n], mak[n], giris) * W1[n, m];
                        //      label3.Text = sigmoidd( Ascn1+B1[m]).ToString();


                    }

                   
                    TsOp1[m] = sigmoidd(Ascn1 + B1[m]);
                }




                double Ascn3 = 0;
                for (int s = 0; s < 4; s++)
                {


                    Ascn3 += TsOp1[s] * W2[s];


                }

                double sc = Ascn3 + B2;

                if (sc < 0.5)
                {
                    TsOp2 = 0;

                }
                else
                {

                    TsOp2 = 1;
                }
                
                dataGridView5.Rows.Add();
                // dataGridView10.Rows[i].Cells[0].Value = epoch.ToString();
                dataGridView5.Rows[j].Cells[0].Value = iterasyon1.ToString();
                dataGridView5.Rows[j].Cells[1].Value = TsOp2.ToString();
                dataGridView5.Rows[j].Cells[2].Value = dataGridView2.Rows[j].Cells[8].Value;
             


            }
            double dogru= 0;
            double Ssaglikli = 0;
            double Sdiyabetli = 0;
            double Gsaglikli = 0;
            double Gdiyabetli = 0;
            for (int n = 0; n < 300; n++)
            {
                if (Convert.ToDouble(dataGridView5.Rows[n].Cells[1].Value ) == Convert.ToDouble(dataGridView5.Rows[n].Cells[2].Value))
                {

                    dogru++;


                }

                if (Convert.ToDouble(dataGridView5.Rows[n].Cells[1].Value) ==0 && Convert.ToDouble(dataGridView5.Rows[n].Cells[2].Value)==0)
                {

                    Ssaglikli++;

                }

                if (Convert.ToDouble(dataGridView5.Rows[n].Cells[1].Value) == 1 && Convert.ToDouble(dataGridView5.Rows[n].Cells[2].Value) == 0)
                {

                    Gsaglikli++;

                }

                if (Convert.ToDouble(dataGridView5.Rows[n].Cells[1].Value) == 0 && Convert.ToDouble(dataGridView5.Rows[n].Cells[2].Value) == 1)
                {

                    Sdiyabetli++;

                }
                if (Convert.ToDouble(dataGridView5.Rows[n].Cells[1].Value) == 1 && Convert.ToDouble(dataGridView5.Rows[n].Cells[2].Value) == 1)
                {

                    Gdiyabetli++;

                }
               
            }
            label11.Text = Ssaglikli.ToString();
            label12.Text = Sdiyabetli.ToString();
            label13.Text = Gsaglikli.ToString();
            label14.Text = Gdiyabetli.ToString();

            double dogruluk_orani = 0;
            dogruluk_orani = 100 * dogru / 300;
            label2.Text = "Doğruluk oranı :" + " " + dogruluk_orani.ToString();

        }
    }
}
