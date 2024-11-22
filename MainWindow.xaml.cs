// Author:  Kyle Chapman
// Modified By: Ritik Sharma
// Created: November 13, 2024
// Updated: November 22, 2024
// Description:
// Form code for a simple WPF whiteboard application.
// This current has a very basic draw functionality, but
// we want to experiment with things like file access and
// additional UI elements.

using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;
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
        private int thickness = 4;

        /// <summary>
        /// Constructor for the form.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        private void updateStatus(string status)
        {
            textStatusBar.Text = status;
        }

        /// <summary>
        /// When the left mouse button is clicked, drawing begins.
        /// </summary>
        private void StartDraw(object sender, MouseButtonEventArgs e)
        {
            isDrawing = true;
            // For a change, this one actually uses the event arguments passed in!
            previousPoint = e.GetPosition(canvasDraw);
            updateStatus("Drawing started...");
        }

        /// <summary>
        /// When the button is unclicked, drawing stops.
        /// </summary>
        private void EndDraw(object sender, MouseButtonEventArgs e)
        {
            isDrawing = false;
            updateStatus("Drawing stopped...");
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
        /// This is a method that allows the contents of the application's Canvas to be written to a .png image file.
        /// </summary>
        private void menuSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog dialog = new SaveFileDialog
                {
                    // Setting filter to multiple formats
                    Filter = "PNG Image|*.png|JPEG Image|*.jpg|Bitmap Image|*.bmp",
                };

                if (dialog.ShowDialog() == true)
                {
                    // Create a bitmap object to store the image. This sets the size and resolution.
                    RenderTargetBitmap renderBitmap = new RenderTargetBitmap((int)canvasDraw.ActualWidth, (int)canvasDraw.ActualHeight, 96d, 96d, PixelFormats.Pbgra32);
                    // Add the contents of the Canvas object to the bitmap.
                    renderBitmap.Render(canvasDraw);

                    BitmapEncoder? encoder = null;

                    // Default format is set to PNG and there are other formats available in case of any failure
                    switch (System.IO.Path.GetExtension(dialog.FileName))
                    {
                        // Prepare a JpegBitmapEncoder object to make a .png file.
                        case ".jpg":
                            encoder = new JpegBitmapEncoder();
                            break;
                        // Prepare a BmpBitmapEncoder object to make a .png file.
                        case ".bmp":
                            encoder = new BmpBitmapEncoder();
                            break;
                        // Prepare a PngBitmapEncoder object to make a .png file.
                        default:
                            encoder = new PngBitmapEncoder();
                            break;
                    }

                    encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
                    // This is a very basic form of saving to a local file called of your choice.

                    using (var stream = new FileStream(dialog.FileName, FileMode.Create))
                    {
                        encoder.Save(stream);
                    }
                    // Updating the Status Bar
                    textStatusBar.Text = "Image saved successfully!";
                }
            }
            // Throwing error message in case of save failure
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// This is a method that allows the contents of the image to be written to the application's Canvas.(This method is done with the help of ChatGPT)
        /// </summary>
        private void menuLoad_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog dialog = new OpenFileDialog
                {
                    // Setting filter to multiple types of image formats\
                    Filter = "Image Files|*.png;*.jpg;*.bmp",
                };

                if (dialog.ShowDialog() == true)
                {
                    ImageBrush brush = new ImageBrush
                    {
                        // Setting image source to selected image
                        ImageSource = new BitmapImage(new Uri(dialog.FileName, UriKind.Absolute))
                    };
                    // Kind of pasting that image on the canvas
                    canvasDraw.Background = brush;
                    // Updating the status bar
                    textStatusBar.Text = "Image loaded successfully!";
                }
            }
            // Throwing error message in case if user selects invalid format.
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// This is a method that allows the contents of the canvas to copied to the clipboard.
        /// </summary>
        private void menuCopy_Click(object sender, RoutedEventArgs e)
        {
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap((int)canvasDraw.ActualWidth, (int)canvasDraw.ActualHeight, 96d, 96d, PixelFormats.Pbgra32);
            renderBitmap.Render(canvasDraw);
            // Copying the current content of canvas to clipboard
            Clipboard.SetImage(renderBitmap);
            // Updating the status bar
            textStatusBar.Text = "Canvas copied to clipboard!";
        }

        /// <summary>
        /// This is a method that allows the contents of the clipboard (Image) to be pasted on the canvas.
        /// </summary>
        private void menuPaste_Click(object sender, RoutedEventArgs e)
        {
            // Checks if clipboard have image in it
            if (Clipboard.ContainsImage())
            {
                // Storing the image in variable
                var image = Clipboard.GetImage();
                // Pasting the image on the canvas
                ImageBrush brush = new ImageBrush { ImageSource = image };
                canvasDraw.Background = brush;
                // Updating status bar
                textStatusBar.Text = "Image pasted from clipboard!";
            }
        }

        /// <summary>
        /// This is a method that allows the contents of the clipboard (Text) to be pasted on the canvas.
        /// </summary>
        private void menuPasteText_Click(object sender, RoutedEventArgs e)
        {
            // Checks if clipboard have text in it
            if (Clipboard.ContainsText())
            {
                // Storing the text in variable
                var text = Clipboard.GetText();
                // Writing the text in the texblock
                TextBlock textBlock = new TextBlock
                {
                    Text = text,
                    Foreground = colour,
                    // Adjust font size based on thickness
                    FontSize = thickness * 5
                };
                // Pasting text on the last coordinates
                Canvas.SetLeft(textBlock, previousPoint.X);
                Canvas.SetTop(textBlock, previousPoint.Y);
                canvasDraw.Children.Add(textBlock);
                // Updating status bar
                textStatusBar.Text = "Text pasted from clipboard!";
            }
        }
        
        /// <summary>
        /// This is a method that allows the user to open a new window.
        /// </summary>
        private void menuNewWindow_Click(object sender, RoutedEventArgs e)
        {
            // Opens a new window
            new MainWindow().Show();
            // Updating the status bar
            textStatusBar.Text = "New window opened!";
        }

        /// <summary>
        /// This is a method that allows the user to set the thickness of the brush.(Got this (Microsoft.VisualBasic.Interaction) from ChatGPT)
        /// </summary>
        private void menuThickness_Click(object sender, RoutedEventArgs e)
        {
            // Setting the thickness to users enetered number
            thickness = int.Parse(Microsoft.VisualBasic.Interaction.InputBox("Enter line thickness:", "Set Thickness", thickness.ToString()));
            // Updating the status bar
            textStatusBar.Text = $"Line thickness set to {thickness}";
        }

        /// <summary>
        /// This is a method that allows the user to set the color of the brush to red.
        /// </summary>
        private void menuRedColour_Click(object sender, RoutedEventArgs e)
        {
            // Simple dialog for colour selection
            colour = Brushes.Red;
            // Updating the status bar
            textStatusBar.Text = "Line colour changed to Red!";
        }

        /// <summary>
        /// This is a method that allows the user to set the color of the brush to black.
        /// </summary>
        private void menuBlackColour_Click(object sender, RoutedEventArgs e)
        {
            // Simple dialog for colour selection
            colour = Brushes.Black;
            // Updating the status bar
            textStatusBar.Text = "Line colour changed to Black!";
        }

        /// <summary>
        /// This is a method that take user to the browser on "How To Draw". (Got it from Stack Overflow Website)
        /// </summary>
        private void menuHowToDraw_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "https://www.google.com/search?q=how+to+draw",
                UseShellExecute = true
            });
        }

        /// <summary>
        /// This is a method that pop ups the message box with About Information.
        /// </summary>
        private void menuAbout_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("COSC Paint\nAuthor: Ritik Sharma\nDate: November 22, 2024", "About");
        }

        /// <summary>
        /// This is a method that pop ups the message box for exiting the application.
        /// </summary>
        private void MenuExit_Click(object sender, RoutedEventArgs e)
        {
            // Show this message box before exiting
            var messageBox = MessageBox.Show("Are you sure you want to exit", "Exit", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            // If user clicks YES then close the application
            if (messageBox == MessageBoxResult.Yes)
            {
                Close();
            }
        }
 
    }
}