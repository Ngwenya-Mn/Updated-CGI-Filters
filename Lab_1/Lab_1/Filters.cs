using System;
using System.ComponentModel;
using System.Drawing;


namespace Lab_1
{
    abstract class Filters
    {
        protected abstract Color calculateNewPixelColor(Bitmap sourceImage, int x, int y);
        /* public Bitmap processImage(Bitmap sourceImage)
         {
             Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);
             for (int i = 0; i < sourceImage.Width; i++)
             {
                 for (int j = 0; j < sourceImage.Height; j++)
                 {
                     resultImage.SetPixel(i, j, calculateNewPixelColor(sourceImage, i, j));
                 }
             }
             return resultImage;
         }*/
        public virtual Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);
            for (int i = 0; i < sourceImage.Width; i++)
            {
                worker.ReportProgress((int)((float)i / resultImage.Width * 100));//You can put 100.0f

                for (int j = 0; j < sourceImage.Height; j++)
                {
                    resultImage.SetPixel(i, j, calculateNewPixelColor(sourceImage, i, j));
                }
            }
            return resultImage;
        }
        public int Clamp(int value, int min, int max)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;

        }
    }
    class InvertFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);
            Color resultColor = Color.FromArgb(255 - sourceColor.R, 255 - sourceColor.G, 255 - sourceColor.B);
            return resultColor;
        }
    }
    class MatrixFilter : Filters
    {
        protected float[,] kernel = null;
        protected MatrixFilter() { }
        public MatrixFilter(float[,] kernel)
        {
            this.kernel = kernel;
        }

        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int radiusX = kernel.GetLength(1) / 2;
            int radiusY = kernel.GetLength(1) / 2;
            float resultR = 0;
            float resultG = 0;
            float resultB = 0;
            for (int l = -radiusY; l <= radiusY; l++)
                for (int k = -radiusX; k <= radiusX; k++)
                {
                    int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + l, 0, sourceImage.Height - 1);
                    Color neighbourColor = sourceImage.GetPixel(idX, idY);
                    resultR += neighbourColor.R * kernel[k + radiusX, l + radiusY];
                    resultB += neighbourColor.B * kernel[k + radiusX, l + radiusY];
                    resultG += neighbourColor.G * kernel[k + radiusX, l + radiusY];
                }
            return Color.FromArgb(Clamp((int)resultR, 0, 255), Clamp((int)resultG, 0, 255), Clamp((int)resultB, 0, 255));
        }
    }
    class BlurFilter : MatrixFilter
    {
        public BlurFilter()
        {
            int sizeX = 3;
            int sizeY = 3;
            kernel = new float[sizeX, sizeY];
            for (int i = 0; i < sizeX; i++)
                for (int j = 0; j < sizeY; j++)

                    kernel[i, j] = 1.0f / (float)(sizeX * sizeY);
        }
    }
    class GaussianFilter : MatrixFilter
    {
        public void createGaussiankernel(int radius, float sigma)
        {
            int size = 2 * radius + 1;
            kernel = new float[size, size];
            float norm = 0;
            for (int i = -radius; i <= radius; i++)
                for (int j = -radius; j <= radius; j++)
                {
                    kernel[i + radius, j + radius] = (float)(Math.Exp(-((i * i) + (j * j)) / (sigma * sigma)));
                    norm += kernel[i + radius, j + radius];
                }
            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                    kernel[i, j] /= norm;

        }
        public GaussianFilter()
        {
            createGaussiankernel(3, 2);
        }
    }
    class GrayScaleFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);
            int gray = (int)((.36 * sourceColor.R) + (.53 * sourceColor.G) + (.11 * sourceColor.B));
            Color resultColor = Color.FromArgb(gray, gray, gray);
            return resultColor;

        }
    }
    class SepiaFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);
            int red = sourceColor.R;
            int green = sourceColor.G;
            int blue = sourceColor.B;
            int gray = (int)((.36 * red) + (.53 * green) + (.11 * blue));
            int Red = (byte)(gray + 2 * 100);
            int Green = (byte)(gray + 0.5 * 400);
            int Blue = (byte)(gray - 1 * 600);

            if (Red > 255)
            {
                Red = 255;
            }
            else
            {
                red = Red;
            }
            if (Green > 255)
            {
                Green = 255;
            }
            else
            {
                green = Green;
            }
            if (Blue > 255)
            {
                Blue = 255;
            }
            else
            {
                blue = Blue;
            }
            Color resultColor = Color.FromArgb(Red, Green, Blue);
            return resultColor;
        }
    }
    class Thresholdfilter : MatrixFilter
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);
            int red = sourceColor.R;
            int green = sourceColor.G;
            int blue = sourceColor.B;
            int gray = (int)((.36 * red) + (.53 * green) + (.11 * blue));
            if (gray < 128)
            {
                red = 0;
                green = 0;
                blue = 0;
            }
            else
            {
                red = 255;
                green = 255;
                blue = 255;
            }
            Color resultColor = Color.FromArgb(red, green, blue);
            return resultColor;

        }
    }
    class SobelFilter : MatrixFilter
    {
        public SobelFilter()
        {
            kernel = new float[3, 3] { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };
        }
    }
    class SharpenFilter : MatrixFilter
    {
        public SharpenFilter()
        {
            kernel = new float[3, 3] { { 0, -1, 0 }, { -1, 5, -1 }, { 0, -1, 0 } };
        }
    }
    class EmbossFilter : MatrixFilter
    {
        public EmbossFilter()
        {
           // float Brightness = 128.0f;
            kernel = new float[,] { { 0, 1, 0 }, { 1, 0, -1 }, { 0, -1, 0 },};
        }

        public override Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);
            for (int i = 0; i < sourceImage.Width; i++)
            {
                worker.ReportProgress((int)((float)i / resultImage.Width * 100));//You can put 100.0f

                for (int j = 0; j < sourceImage.Height; j++)
                {
                    Color c = calculateNewPixelColor(sourceImage, i, j);
                    int Red = c.R;
                    int Green = c.G;
                    int Blue = c.B;

                    Red = Clamp(Red + 128, 0, 255);
                    Green = Clamp(Green + 128, 0, 255);
                    Blue = Clamp(Blue + 128, 0, 255);
                    Color resultc=Color.FromArgb(Red,Green,Blue);
                    resultImage.SetPixel(i, j, resultc);
                }
            }
            return resultImage;
        }

    }
    class MotionBlurFilter : MatrixFilter
    {
        public MotionBlurFilter()
        {
            kernel = new float[5,5]{ { 1, 0, 0, 0, 0, },
                                     { 0, 1, 0, 0, 0, },
                                     { 0, 0, 1, 0, 0, },
                                     { 0, 0, 0, 1, 0, },
                                     { 0, 0, 0, 0, 1, },
                                   };
        }
    }
}