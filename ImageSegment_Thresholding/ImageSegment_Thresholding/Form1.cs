using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BitMiracle.LibTiff.Classic;

namespace ImageSegment_Thresholding
{
    public partial class Form1 : Form
    {
         private ImageSegment ImageThreshold = new ImageSegment();

        // only for display this rountine is used but actual processing is on 16bit Data.
        Bitmap input_bitmap, Segment_bitmap2;
        ushort[,] input_imagedata;
        int[] input_imag;
        Image file;
        int[] histogrampoint = new int[256];
        int width; 
        int height;
        int bitpersample;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult d = openFileDialog1.ShowDialog();

            //DialogResult dr = openFileDialog1.ShowDialog();
            if (d == DialogResult.OK)
            {
                file = Image.FromFile(openFileDialog1.FileName);
                input_bitmap = new Bitmap(openFileDialog1.FileName);
                pictureBox1.Image = file;

                // loading tiff images BitMiracle LiBTiff Nuget reference
                using (Tiff tiff = Tiff.Open(openFileDialog1.FileName, "r"))
                {
                    width = tiff.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();
                    height = tiff.GetField(TiffTag.IMAGELENGTH)[0].ToInt();
                    bitpersample = tiff.GetField(TiffTag.BITSPERSAMPLE)[0].ToInt();
                    int sampleperpixel = tiff.GetField(TiffTag.SAMPLESPERPIXEL)[0].ToInt();
                    int photometric = tiff.GetField(TiffTag.PHOTOMETRIC)[0].ToInt();

                    byte[] scanline = new byte[width * sizeof(ushort)];
                    input_imagedata = new ushort[height, width];

                    for (int i = 0; i < height; i++)
                    {
                        tiff.ReadScanline(scanline, i);
                        // pack all bytes to ushorts
                        Buffer.BlockCopy(scanline, 0, input_imagedata, i * width * sizeof(ushort), scanline.Length);
                    }
                }
            }
        }

        private void histogram_Click(object sender, EventArgs e)
        {
            int value = 0;
            // Select the bin width as 16Bit pixel depth
            int binsize = (int)Math.Pow(2,bitpersample) / 256;
            textBox2.Text = binsize.ToString();

            input_imag = new int[height*width];
            // Image Max/Min calculation
            ushort ImMin = ushort.MaxValue;
            ushort ImMax = ushort.MinValue;
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    int val = input_imagedata[i, j];
                    input_imag[i*width+j] = val;
                    if (ImMax < val)
                        ImMax = (ushort)val;
                    if (ImMin > val)
                        ImMin = (ushort)val;
                }
            }

            histogrampoint.Initialize();

            for (int v = 0; v < 256; v++)
            {
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        value = input_imagedata[i, j];

                        if (value >= v * binsize && value < (v + 1) * binsize)
                        {
                            histogrampoint[v]++;
                        }
                    }
                }
            }
            double min1 = ImageThreshold.Mean(input_imag);
            double stddev = ImageThreshold.StDev(input_imag);
            textBox3.Text = ImMin.ToString();
            textBox5.Text = ImMax.ToString();
            textBox4.Text = min1.ToString();
            textBox6.Text = stddev.ToString();
       
            int otsuThreshold= ImageThreshold.getSimpleOtsuThreshold(histogrampoint);
            textBox1.Text = otsuThreshold.ToString();
            textBox7.Text = otsuThreshold.ToString();
            trackBar1.Value = otsuThreshold;
        }
      

        private void otsu_segment_Click(object sender, EventArgs e)
        {
            int thresh = int.Parse(textBox7.Text);
            double min1=0;
            double stddev=0;
            ImageThreshold.threshold(input_imagedata, thresh, height, width, Segment_bitmap2, ref min1, ref stddev);
            textBox9.Text = min1.ToString();
            textBox11.Text = stddev.ToString();
            file = Image.FromFile("SegmentImage.tif");
            pictureBox2.Image = file;
            pictureBox2.Update();
            pictureBox2.Refresh();
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            textBox7.Text = trackBar1.Value.ToString();
        }
    }
}
