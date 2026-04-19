using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SJMagic.Services
{
    public class ImageService
    {
        public bool SaveTransformedImage(string inputPath, string outputPath, double angle, double scaleX, double scaleY)
        {
            try
            {
                // Load original image
                BitmapImage source = new BitmapImage();
                source.BeginInit();
                source.CacheOption = BitmapCacheOption.OnLoad;
                source.UriSource = new Uri(inputPath);
                source.EndInit();

                // Define transformation
                TransformGroup transformGroup = new TransformGroup();
                transformGroup.Children.Add(new RotateTransform(angle));
                transformGroup.Children.Add(new ScaleTransform(scaleX, scaleY));

                // Apply transform to create a new Drawing
                DrawingVisual drawingVisual = new DrawingVisual();
                using (DrawingContext drawingContext = drawingVisual.RenderOpen())
                {
                    // Calculate target size (for rotation, this might change the bounding box size)
                    // For simplicity in this BETA, we use a fixed approach or RenderTargetBitmap
                    // Best way to preserve quality and handle transforms is using TransformedBitmap for simple cases
                    
                    BitmapSource transformedBitmap = new TransformedBitmap(source, transformGroup);
                    drawingContext.DrawImage(transformedBitmap, new Rect(0, 0, transformedBitmap.PixelWidth, transformedBitmap.PixelHeight));
                }

                // Render to a new bitmap
                TransformedBitmap finalBitmap = new TransformedBitmap(source, transformGroup);

                // Save to file
                using (var fileStream = new FileStream(outputPath, FileMode.Create))
                {
                    BitmapEncoder encoder = GetEncoder(inputPath);
                    encoder.Frames.Add(BitmapFrame.Create(finalBitmap));
                    encoder.Save(fileStream);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Image Save Error: {ex.Message}");
                return false;
            }
        }

        private BitmapEncoder GetEncoder(string filePath)
        {
            string ext = Path.GetExtension(filePath).ToLower();
            switch (ext)
            {
                case ".png": return new PngBitmapEncoder();
                case ".bmp": return new BmpBitmapEncoder();
                case ".gif": return new GifBitmapEncoder();
                default: return new JpegBitmapEncoder { QualityLevel = 95 };
            }
        }
    }
}
