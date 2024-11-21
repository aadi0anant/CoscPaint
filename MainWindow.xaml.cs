// Author:  Kyle Chapman AND YOU
// Created: November 13, 2024
// Updated: ...
// Description:
// Form code for a simple WPF whiteboard application.
// This current has a very basic draw functionality, but
// we want to experiment with things like file access and
// additional UI elements.

using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CoscPaint
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool isDrawing;
        private Point previousPoint;
        private Brush colour = Brushes.Black;
        private int thickness = 2;

        /// <summary>
        /// Constructor for the form.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// When the left mouse button is clicked, drawing begins.
        /// </summary>
        private void StartDraw(object sender, MouseButtonEventArgs e)
        {
            isDrawing = true;
            // For a change, this one actually uses the event arguments passed in!
            previousPoint = e.GetPosition(canvasDraw);
        }

        /// <summary>
        /// When the button is unclicked, drawing stops.
        /// </summary>
        private void EndDraw(object sender, MouseButtonEventArgs e)
        {
            isDrawing = false;
        }

        /// <summary>
        /// When the mouse moves on the canvas, if the left mouse button is depressed, draw based on current settings.
        /// </summary>
        private void MouseOnCanvas(object sender, MouseEventArgs e)
        {
            // Does nothing if the left mouse button isn't down on the canvas.
            if (isDrawing)
            {
                Point currentPoint = e.GetPosition(canvasDraw);
                Line line = new Line
                {
                    // Get stroke and thickness attributes for the line.
                    Stroke = colour,
                    StrokeThickness = thickness,
                    // Draw from previous "pixel" to the current "pixel".
                    X1 = previousPoint.X,
                    Y1 = previousPoint.Y,
                    X2 = currentPoint.X,
                    Y2 = currentPoint.Y
                };
                // Draw the point on the canvas.
                canvasDraw.Children.Add(line);
                previousPoint = currentPoint;
            }
        }

        /// <summary>
        /// This is a (currently unimplemented) method that allows the contents of the application's Canvas to be written to a .png image file.
        /// </summary>
        private void SaveCanvas()
        {
            // Create a bitmap object to store the image. This sets the size and resolution.
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap((int)canvasDraw.ActualWidth, (int)canvasDraw.ActualHeight, 96d, 96d, PixelFormats.Pbgra32);
            // Add the contents of the Canvas object to the bitmap.
            renderBitmap.Render(canvasDraw);
            // Prepare a PngBitmapEncoder object to make a .png file.
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
            // This is a very basic form of saving to a local file called drawing.png.
            using (var fileStream = new System.IO.FileStream("drawing.png", System.IO.FileMode.Create))
            {
                encoder.Save(fileStream);
            }
        }
    }
}