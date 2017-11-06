using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using BitMiracle.LibTiff.Classic;

namespace ImageSegment_Thresholding
{
    class ImageSegment
    {
        // this method is used to compute the probabilities 
        private static float ProbabilityIm(int start, int end, int[] hist)
        {
            int sum = 0;
            int i;
            for (i = start; i <= end; i++)
                sum += hist[i];

            return (float)sum;
        }

        // this method is used to compute the mean values (mu)
        private static float MeanIm(int start, int end, int[] hist)
        {
            int sum = 0;
            int i;
            for (i = start; i <= end; i++)
                sum += i * hist[i];

            return (float)sum;
        }

        // this method used to finds the maximum element in a vector
        private static int findMaximum(float[] vec, int n)
        {
            float maxVec = 0;
            int idx = 0;
            int i;

            for (i = 1; i < n - 1; i++)
            {
                if (vec[i] > maxVec)
                {
                    maxVec = vec[i];
                    idx = i;
                }
            }
            return idx;
        }

        // method for find otsu threshold return threshold
        public int getSimpleOtsuThreshold(int[] hist)
        {
            byte threshold = 0;
            float[] data_vet = new float[256];
            data_vet.Initialize();
            float p1, p2, p12;
            int k;

            // loop through all possible thresholds values and maximize between class variance
            for (k = 1; k != 255; k++)
            {
                p1 = ProbabilityIm(0, k, hist);
                p2 = ProbabilityIm(k + 1, 255, hist);
                p12 = p1 * p2;
                if (p12 == 0)
                    p12 = 1;
                float diff = (MeanIm(0, k, hist) * p2) - (MeanIm(k + 1, 255, hist) * p1);
                data_vet[k] = (float)diff * diff / p12;
            }

            // find maximum threshold
            threshold = (byte)findMaximum(data_vet, 256);

            return threshold;
        }

        // Thresholding and file writing routine
        public void threshold(ushort[,] input_imagedata, int thresh, int height, int width, Bitmap Segment_bitmap2,ref double min1, ref double stddev)
        {
            byte[,] imagedataotsu = new byte[height, width];
            int[] seg_imag = new int[height * width];
            byte Threshold = 0;
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    int val = (int)input_imagedata[i, j] / 256;
                    if (val > thresh)
                    {
                        Threshold = 0;
                        seg_imag[i * width + j] = 0;
                    }
                    else
                    {
                        Threshold = 255;
                        seg_imag[i * width + j] = (int)input_imagedata[i, j];
                    }
                    imagedataotsu[i, j] = Threshold;
                }
            }
            min1 = Mean(seg_imag);
            stddev = StDev(seg_imag);
            
            using (Tiff output = Tiff.Open("SegmentImage.tif", "w"))
            {
                if (output == null)
                    return;
                byte[] scanline = new byte[width * sizeof(ushort)];
                output.SetField(TiffTag.IMAGEWIDTH, width);
                output.SetField(TiffTag.IMAGELENGTH, height);
                output.SetField(TiffTag.BITSPERSAMPLE, 8);

                for (int i = 0; i < height; i++)
                {
                    // unpack all ushorts to bytes
                    Buffer.BlockCopy(imagedataotsu, i * width * sizeof(byte), scanline, 0, width * sizeof(byte));

                    output.WriteScanline(scanline, i);
                }
            }
        }
        
        /// Calculates the mean of an array of values
        public double Mean(int[] v)
        {
            double sum = 0.0;

            for (int i = 0; i < v.Length; i++)
            {
                sum += v[i];
            }

            return sum / v.Length;
        }

        /// Calculates the variance of an array of values
        public double Variance(int[] v)
        {
            double mean = Mean(v);
            double sum = 0.0;

            for (int i = 0; i < v.Length; i++)
            {
                sum += (v[i] - mean) * (v[i] - mean);
            }

            int denom = v.Length - 1;
            if (v.Length <= 1)
                denom = v.Length;

            return sum / denom;
        }

        /// Calculates the standard deviation of an array of values
        public double StDev(int[] v)
        {
            return Math.Sqrt(Variance(v));
        }
    }
}
