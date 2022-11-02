using System;
using System.Drawing;
using System.Windows.Forms;

namespace sena_180201022
{
    public partial class Form1 : Form
    {
        Bitmap SRC_IMG, DEST_IMG;
        public Form1()
        {
            InitializeComponent();
        }
        
        private void button1_Click(object sender, EventArgs e) //load
        {
            openFileDialog1.ShowDialog();
            string File_Name = openFileDialog1.FileName.Trim();
            try
            {
                if (File_Name != "") pictureBox1.Load(File_Name);
                SRC_IMG = new Bitmap(File_Name);
                DEST_IMG = new Bitmap(SRC_IMG.Width, SRC_IMG.Height);
            }
            catch
            {
                MessageBox.Show("Error in file type");
            }
        }

        private void button2_Click(object sender, EventArgs e) //change src as dest
        {
            SRC_IMG = DEST_IMG;
            pictureBox1.Image = SRC_IMG;
        }

        private void button3_Click(object sender, EventArgs e) //gray
        {
            int i, j;
            for(i = 0; i < SRC_IMG.Width; i++)
                for (j = 0; j < SRC_IMG.Height; j++)
                {
                    int RR = SRC_IMG.GetPixel(i, j).R;
                    int GG = SRC_IMG.GetPixel(i, j).G;
                    int BB = SRC_IMG.GetPixel(i, j).B;

                    int Gray = (RR + GG + BB) / 3;
                    DEST_IMG.SetPixel(i,j,Color.FromArgb(Gray, Gray, Gray));
                }
            pictureBox2.Image = DEST_IMG;
        }

        void conv(Bitmap src, ref Bitmap dest, double[,]k, int w_s) //convolution
        {
            int fram = w_s / 2;
            for (int x = fram; x < src.Width - fram; x++)
                for (int y = fram; y < src.Height - fram; y++)
                {
                    double Res_R = 0, Res_G = 0, Res_B = 0;
                    //convolution
                    for (int i = 0; i < w_s; i++)
                        for (int j = 0; j < w_s; j++)
                        {
                            Res_R += k[i, j] * src.GetPixel(x + i - fram, y + j - fram).R;
                            Res_G += k[i, j] * src.GetPixel(x + i - fram, y + j - fram).G;
                            Res_B += k[i, j] * src.GetPixel(x + i - fram, y + j - fram).B;
                        }
                    //assign new img
                    dest.SetPixel(x, y, Color.FromArgb((int)Res_R, (int)Res_G, (int)Res_B));
                }
        }

        private void button4_Click(object sender, EventArgs e) //mean
        {
            int w_size = (int)numericUpDown1.Value;
            double[,] K = new double[w_size, w_size];
            for (int i = 0; i < w_size; i++)
                for (int j = 0; j < w_size; j++)
                    K[i, j] = 1.0 / (w_size * w_size);

            conv(SRC_IMG, ref DEST_IMG, K, w_size);
            pictureBox2.Image = DEST_IMG;
        }

        void bubblesort(ref int []v, int size) //bubblesort for median filter
        {
            int i, j, tmp;
            for (i = 1; i < size; i++)
                for (j = 0; j < size - i; j++)
                    if (v[j] > v[j + 1])
                    {
                        tmp = v[j];
                        v[j] = v[j + 1];
                        v[j + 1] = tmp;
                    }
        }

        private void button5_Click(object sender, EventArgs e) //median
        {
            int w_size = (int)numericUpDown1.Value, frame = w_size / 2, v;
            int[] Median_Vector_R = new int[w_size * w_size];
            int[] Median_Vector_G = new int[w_size * w_size];
            int[] Median_Vector_B = new int[w_size * w_size];
            for (int x = frame; x < SRC_IMG.Width - frame; x++)
                for (int y = frame; y < SRC_IMG.Height - frame; y++)
                {
                    v = 0;
                    for (int i = 0; i < w_size; i++)
                        for (int j = 0; j < w_size; j++)
                        {
                            Median_Vector_R[v] = SRC_IMG.GetPixel(x + i - frame, y + j - frame).R;
                            Median_Vector_G[v] = SRC_IMG.GetPixel(x + i - frame, y + j - frame).G;
                            Median_Vector_B[v] = SRC_IMG.GetPixel(x + i - frame, y + j - frame).B;
                            v++;
                        }
                    bubblesort(ref Median_Vector_R, w_size);
                    bubblesort(ref Median_Vector_G, w_size);
                    bubblesort(ref Median_Vector_B, w_size);

                    //pic mid point of sorted vector and assign it to DEST_IMG
                    int RR = Median_Vector_R[w_size * w_size / 2 + 1];
                    int GG = Median_Vector_G[w_size * w_size / 2 + 1];
                    int BB = Median_Vector_B[w_size * w_size / 2 + 1];
                    DEST_IMG.SetPixel(x, y, Color.FromArgb((int)RR, (int)GG, (int)BB));
                }
            pictureBox2.Image = DEST_IMG;
        }

        private void button6_Click(object sender, EventArgs e) //gaussian
        {
            int w_size = (int)numericUpDown1.Value; 
            double[,] K = new double[w_size, w_size];
            for (int i = 0; i < w_size; i++) //when window size=5, x=0 is when i=2
                for (int j = 0; j < w_size; j++)
                {
                    int x = w_size/2 - i, y = w_size / 2 - j;
                    K[i, j] = 1.0 /(2*Math.PI)*Math.Exp(-(x*x + y*y)/2.0);
                }
            conv(SRC_IMG, ref DEST_IMG, K, w_size);
            pictureBox2.Image = DEST_IMG;
        }

        void conv2(Bitmap src, ref Bitmap dest, int[,] k, int w_s)  // convolution without overshooting
        {
            int fram = w_s / 2;
            for (int x = fram; x < src.Width - fram; x++)
                for (int y = fram; y < src.Height - fram; y++)
                {
                    double Res_R = 0, Res_G = 0, Res_B = 0;
                    //convolution
                    for (int i = 0; i < w_s; i++)
                        for (int j = 0; j < w_s; j++)
                        {
                            Res_R += k[i, j] * src.GetPixel(x + i - fram, y + j - fram).R;
                            Res_G += k[i, j] * src.GetPixel(x + i - fram, y + j - fram).G;
                            Res_B += k[i, j] * src.GetPixel(x + i - fram, y + j - fram).B;
                        }
                    if (Res_R > 255) Res_R = 255; if (Res_R < 0) Res_R = 0;
                    if (Res_G > 255) Res_G = 255; if (Res_G < 0) Res_G = 0;
                    if (Res_B > 255) Res_B = 255; if (Res_B < 0) Res_B = 0;
                    //assign new img
                    dest.SetPixel(x, y, Color.FromArgb((int)Res_R, (int)Res_G, (int)Res_B));
                }
        }

        private void button7_Click(object sender, EventArgs e) //sharpen
        {
            const int w_size = 3;
            int[,] K = new int[w_size, w_size] {{ 0,-1, 0},
                                                {-1, 5,-1},
                                                { 0,-1, 0}};

            conv2(SRC_IMG, ref DEST_IMG, K, w_size);
            pictureBox2.Image = DEST_IMG;
        }

        private void button8_Click(object sender, EventArgs e) //sharpen more
        {
            const int w_size = 3;
            int[,] K = new int[w_size, w_size] {{-1,-1,-1},
                                                {-1, 9,-1},
                                                {-1,-1,-1}};

            conv2(SRC_IMG, ref DEST_IMG, K, w_size);
            pictureBox2.Image = DEST_IMG;
        }

        private void button9_Click(object sender, EventArgs e) //edge 4bit
        {
            const int w_size = 3;
            int[,] K = new int[w_size, w_size] {{ 0,-1, 0},
                                                {-1, 4,-1},
                                                { 0,-1, 0}};

            conv2(SRC_IMG,ref DEST_IMG, K, w_size);
            pictureBox2.Image = DEST_IMG;
        }

        private void button10_Click(object sender, EventArgs e) //edge 8bit
        {
            const int w_size = 3;
            int[,] K = new int[w_size, w_size] {{-1,-1,-1},
                                                {-1, 8,-1},
                                                {-1,-1,-1}};

            conv2(SRC_IMG, ref DEST_IMG, K, w_size);
            pictureBox2.Image = DEST_IMG;
        }

        private void hScrollBar1_Scroll(object sender, ScrollEventArgs e) //brightness
        {
            textBox1.Text = hScrollBar1.Value.ToString();
            for (int x = 0; x < pictureBox1.Image.Width; x++)
                for (int y = 0; y < pictureBox1.Image.Height; y++)
                {
                    int Res_R = hScrollBar1.Value + SRC_IMG.GetPixel(x, y).R;
                    int Res_G = hScrollBar1.Value + SRC_IMG.GetPixel(x, y).G;
                    int Res_B = hScrollBar1.Value + SRC_IMG.GetPixel(x, y).B;

                    if (Res_R > 255) Res_R = 255; if (Res_R < 0) Res_R = 0;
                    if (Res_G > 255) Res_G = 255; if (Res_G < 0) Res_G = 0;
                    if (Res_B > 255) Res_B = 255; if (Res_B < 0) Res_B = 0;
                    //assign new img
                    DEST_IMG.SetPixel(x, y, Color.FromArgb(Res_R, Res_G, Res_B));
                }
            pictureBox2.Image = DEST_IMG;
        }

        private void hScrollBar2_Scroll(object sender, ScrollEventArgs e) //contrast
        {
            textBox2.Text = hScrollBar2.Value.ToString();
            int C = hScrollBar2.Value;
            double F = 259.0 * (C + 255.0) / (255.0 * (259.0 - C));
            for (int x = 0; x < pictureBox1.Image.Width; x++)
                for (int y = 0; y < pictureBox1.Image.Height; y++)
                {
                    double Res_R = F * (SRC_IMG.GetPixel(x, y).R - 128) + 128;
                    double Res_G = F * (SRC_IMG.GetPixel(x, y).G - 128) + 128;
                    double Res_B = F * (SRC_IMG.GetPixel(x, y).B - 128) + 128;

                    if (Res_R > 255) Res_R = 255; if (Res_R < 0) Res_R = 0;
                    if (Res_G > 255) Res_G = 255; if (Res_G < 0) Res_G = 0;
                    if (Res_B > 255) Res_B = 255; if (Res_B < 0) Res_B = 0;
                    //assign new img
                    DEST_IMG.SetPixel(x, y, Color.FromArgb((int)Res_R, (int)Res_G, (int)Res_B));
                }
            pictureBox2.Image = DEST_IMG;
        }

        private void hScrollBar3_Scroll(object sender, ScrollEventArgs e) //gama
        {
            textBox3.Text = ((double)hScrollBar3.Value/100).ToString();
            double C = (double)hScrollBar3.Value/100;
            for (int x = 0; x < pictureBox1.Image.Width; x++)
                for (int y = 0; y < pictureBox1.Image.Height; y++)
                {
                    double FR = SRC_IMG.GetPixel(x, y).R;
                    double FG = SRC_IMG.GetPixel(x, y).G;
                    double FB = SRC_IMG.GetPixel(x, y).B;
                    double Res_R = 255 * Math.Pow(FR / 255, 1 / C);
                    double Res_G = 255 * Math.Pow(FG / 255, 1 / C);
                    double Res_B = 255 * Math.Pow(FB / 255, 1 / C);
                    //assign new img
                    DEST_IMG.SetPixel(x, y, Color.FromArgb((int)Res_R, (int)Res_G, (int)Res_B));
                }
            pictureBox2.Image = DEST_IMG;
        }

        private void button11_Click(object sender, EventArgs e) //rotate
        {
            double th;
            double.TryParse(textBox4.Text, out th);
            th = th / 180.0 * Math.PI;
            int x0 = pictureBox1.Image.Width / 2, y0 = pictureBox1.Image.Height / 2;
            for (int x = 0; x < pictureBox1.Image.Width; x++)
                for (int y = 0; y < pictureBox1.Image.Height; y++)
                {
                    int x_ = (int)Math.Round(Math.Cos(th) * (x - x0) + Math.Sin(th) * (y - y0));
                    int y_ = (int)Math.Round(-Math.Sin(th) * (x - x0) + Math.Cos(th) * (y - y0));
                    x_ = x0 + x_;
                    y_ = y0 + y_;
                    int PivotR = SRC_IMG.GetPixel(x, y).R;
                    int PivotG = SRC_IMG.GetPixel(x, y).G;
                    int PivotB = SRC_IMG.GetPixel(x, y).B;
                    if (x_ >= 0 && x_ < DEST_IMG.Width && y_ >= 0 && y_ < DEST_IMG.Height)
                        DEST_IMG.SetPixel(x_, y_, Color.FromArgb((int)PivotR, (int)PivotG, (int)PivotB));
                }
            pictureBox2.Image = DEST_IMG;
        }

        private void button12_Click(object sender, EventArgs e) //90
        {
            Bitmap n_bmp = new Bitmap(pictureBox1.Image.Height, pictureBox1.Image.Width);
            for (int x = 0; x < pictureBox1.Image.Width; x++)
                for (int y = 0; y < pictureBox1.Image.Height; y++)
                {
                    int y_ = pictureBox1.Image.Width - 1 - x;
                    int PivotR = SRC_IMG.GetPixel(x, y).R;
                    int PivotG = SRC_IMG.GetPixel(x, y).G;
                    int PivotB = SRC_IMG.GetPixel(x, y).B;
                    n_bmp.SetPixel(y, y_, Color.FromArgb((int)PivotR, (int)PivotG, (int)PivotB));
                }
            pictureBox2.Image = n_bmp;
        }

        private void button13_Click(object sender, EventArgs e) //-90
        {
            Bitmap n_bmp = new Bitmap(pictureBox1.Image.Height, pictureBox1.Image.Width);
            for (int x = 0; x < pictureBox1.Image.Width; x++)
                for (int y = 0; y < pictureBox1.Image.Height; y++)
                {
                    int x_ = pictureBox1.Image.Height - 1 - y;
                    int PivotR = SRC_IMG.GetPixel(x, y).R;
                    int PivotG = SRC_IMG.GetPixel(x, y).G;
                    int PivotB = SRC_IMG.GetPixel(x, y).B;
                    n_bmp.SetPixel(x_, x, Color.FromArgb((int)PivotR, (int)PivotG, (int)PivotB));
                }
            pictureBox2.Image = n_bmp;
        }

        private void button14_Click(object sender, EventArgs e) //180
        {
            for (int x = 0; x < pictureBox1.Image.Width; x++)
                for (int y = 0; y < pictureBox1.Image.Height; y++)
                {
                    int x_ = pictureBox1.Image.Width - 1 - x;
                    int y_ = pictureBox1.Image.Height - 1 - y;
                    int PivotR = SRC_IMG.GetPixel(x, y).R;
                    int PivotG = SRC_IMG.GetPixel(x, y).G;
                    int PivotB = SRC_IMG.GetPixel(x, y).B;
                    DEST_IMG.SetPixel(x_, y_, Color.FromArgb((int)PivotR, (int)PivotG, (int)PivotB));
                }
            pictureBox2.Image = DEST_IMG;
        }

        private void button15_Click(object sender, EventArgs e) //vertical
        {
            for (int x = 0; x < pictureBox1.Image.Width; x++)
                for (int y = 0; y < pictureBox1.Image.Height; y++)
                {
                    int y_ = pictureBox1.Image.Height - 1 - y;
                    int PivotR = SRC_IMG.GetPixel(x, y).R;
                    int PivotG = SRC_IMG.GetPixel(x, y).G;
                    int PivotB = SRC_IMG.GetPixel(x, y).B;
                    DEST_IMG.SetPixel(x, y_, Color.FromArgb((int)PivotR, (int)PivotG, (int)PivotB));
                }
            pictureBox2.Image = DEST_IMG;
        }

        private void button16_Click(object sender, EventArgs e) //horizontal
        {
            for (int x = 0; x < pictureBox1.Image.Width; x++)
                for (int y = 0; y < pictureBox1.Image.Height; y++)
                {
                    int x_ = pictureBox1.Image.Width - 1 - x;
                    int PivotR = SRC_IMG.GetPixel(x, y).R;
                    int PivotG = SRC_IMG.GetPixel(x, y).G;
                    int PivotB = SRC_IMG.GetPixel(x, y).B;
                    DEST_IMG.SetPixel(x_, y, Color.FromArgb((int)PivotR, (int)PivotG, (int)PivotB));
                }
            pictureBox2.Image = DEST_IMG;
        }

        private void button17_Click(object sender, EventArgs e) //upscale
        {
            for (int x = 1; x < (pictureBox1.Image.Width - 2) / 2; x++)
                for (int y = 1; y < (pictureBox1.Image.Height - 2) / 2; y++)
                {
                    int PivotR = SRC_IMG.GetPixel(x, y).R;
                    int PivotG = SRC_IMG.GetPixel(x, y).G;
                    int PivotB = SRC_IMG.GetPixel(x, y).B;
                    DEST_IMG.SetPixel(x * 2    , y * 2    , Color.FromArgb(PivotR, PivotG, PivotB));
                    DEST_IMG.SetPixel(x * 2 + 1, y * 2    , Color.FromArgb(PivotR, PivotG, PivotB));
                    DEST_IMG.SetPixel(x * 2    , y * 2 + 1, Color.FromArgb(PivotR, PivotG, PivotB));
                    DEST_IMG.SetPixel(x * 2 + 1, y * 2 + 1, Color.FromArgb(PivotR, PivotG, PivotB));
                }
            pictureBox2.Image = DEST_IMG;

        }

        private void button18_Click(object sender, EventArgs e) //downscale
        {
            DEST_IMG.SetResolution(pictureBox1.Image.Width / 2, pictureBox1.Image.Height / 2);
            for (int x = 1; x < pictureBox1.Image.Width - 2; x += 2)
                for (int y = 1; y < pictureBox1.Image.Height - 2; y += 2)
                {
                    int PivotR = (SRC_IMG.GetPixel(x, y).R + SRC_IMG.GetPixel(x + 1, y).R + SRC_IMG.GetPixel(x, y + 1).R + SRC_IMG.GetPixel(x + 1, y + 1).R) / 4;
                    int PivotG = (SRC_IMG.GetPixel(x, y).G + SRC_IMG.GetPixel(x + 1, y).G + SRC_IMG.GetPixel(x, y + 1).G + SRC_IMG.GetPixel(x + 1, y + 1).G) / 4;
                    int PivotB = (SRC_IMG.GetPixel(x, y).B + SRC_IMG.GetPixel(x + 1, y).B + SRC_IMG.GetPixel(x, y + 1).B + SRC_IMG.GetPixel(x + 1, y + 1).B) / 4;
                    DEST_IMG.SetPixel(x / 2, y / 2, Color.FromArgb(PivotR, PivotG, PivotB));
                }
            pictureBox2.Image = DEST_IMG;
        }
    }
}
