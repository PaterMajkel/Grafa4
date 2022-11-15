using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using Microsoft.Win32;

namespace Grafa4;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    Bitmap? sourceImage = null;
    Bitmap? imageToEdit = null;
    public MainWindow()
    {
        InitializeComponent();
    }

    private void OpenFile(object sender, RoutedEventArgs e)
    {
        OpenFileDialog openFileDialog = new OpenFileDialog();
        openFileDialog.Filter = "Image files (*.jpg;*.png)|*.jpg;*.png|All files (*.*)|*.*";

        if (openFileDialog.ShowDialog() == true)
        {
            string fileName = openFileDialog.FileName;
            imageToEdit = this.sourceImage = new Bitmap($"{fileName}");
            SourceImage.Source = ImageSourceFromBitmap(this.sourceImage);
        }
    }
    [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
    [return: MarshalAs(UnmanagedType.Bool)]

    public static extern bool DeleteObject([In] IntPtr hObject);

    public ImageSource ImageSourceFromBitmap(Bitmap bmp)
    {
        var handle = bmp.GetHbitmap();
        try
        {
            return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }
        finally { DeleteObject(handle); }
    }

    private void Add_Click(object sender, RoutedEventArgs e)
    {
        if (sourceImage == null)
        {
            MessageBox.Show("You haven't uploaded any files", "Image error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        Bitmap bitmap = new Bitmap(this.sourceImage.Width, this.sourceImage.Height);
        bitmap = (Bitmap)this.imageToEdit.Clone();
        FilteredImage.Source = ImageSourceFromBitmap(Algorithm.RgbOperation(bitmap, new double[] {Double.Parse(String.IsNullOrEmpty(RedValue.Text) ? "0" : RedValue.Text),
        Double.Parse(String.IsNullOrEmpty(GreenValue.Text) ? "0" : GreenValue.Text),
        Double.Parse(String.IsNullOrEmpty(BlueValue.Text) ? "0" : BlueValue.Text)
        }, ProcessType.Adding));
    }

    private void Subtract_Click(object sender, RoutedEventArgs e)
    {
        if (sourceImage == null)
        {
            MessageBox.Show("You haven't uploaded any files", "Image error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        Bitmap bitmap = new Bitmap(this.sourceImage.Width, this.sourceImage.Height);
        bitmap = (Bitmap)this.imageToEdit.Clone();
        FilteredImage.Source = ImageSourceFromBitmap(Algorithm.RgbOperation(bitmap, new double[] {Double.Parse(String.IsNullOrEmpty(RedValue.Text) ? "0" : RedValue.Text),
        Double.Parse(String.IsNullOrEmpty(GreenValue.Text) ? "0" : GreenValue.Text),
        Double.Parse(String.IsNullOrEmpty(BlueValue.Text) ? "0" : BlueValue.Text)
        }, ProcessType.Substracting));
    }

    private void Multiply_Click(object sender, RoutedEventArgs e)
    {
        if (sourceImage == null)
        {
            MessageBox.Show("You haven't uploaded any files", "Image error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        Bitmap bitmap = new Bitmap(this.sourceImage.Width, this.sourceImage.Height);
        bitmap = (Bitmap)this.imageToEdit.Clone();
        FilteredImage.Source = ImageSourceFromBitmap(Algorithm.RgbOperation(bitmap, new double[] {Double.Parse(String.IsNullOrEmpty(RedValue.Text) ? "0" : RedValue.Text),
        Double.Parse(String.IsNullOrEmpty(GreenValue.Text) ? "0" : GreenValue.Text),
        Double.Parse(String.IsNullOrEmpty(BlueValue.Text) ? "0" : BlueValue.Text)
        }, ProcessType.Multiplying));
    }

    private void Divide_Click(object sender, RoutedEventArgs e)
    {
        if (sourceImage == null)
        {
            MessageBox.Show("You haven't uploaded any files", "Image error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        Bitmap bitmap = new Bitmap(this.sourceImage.Width, this.sourceImage.Height);
        bitmap = (Bitmap)this.imageToEdit.Clone();
        FilteredImage.Source = ImageSourceFromBitmap(Algorithm.RgbOperation(bitmap, new double[] {Double.Parse(String.IsNullOrEmpty(RedValue.Text) ? "0" : RedValue.Text),
        Double.Parse(String.IsNullOrEmpty(GreenValue.Text) ? "0" : GreenValue.Text),
        Double.Parse(String.IsNullOrEmpty(BlueValue.Text) ? "0" : BlueValue.Text)
        }, ProcessType.Dividing));
    }

    private void Luminosity_Click(object sender, RoutedEventArgs e)
    {
        if (sourceImage == null)
        {
            MessageBox.Show("You haven't uploaded any files", "Image error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        Bitmap bitmap = new Bitmap(this.sourceImage.Width, this.sourceImage.Height);
        bitmap = (Bitmap)this.imageToEdit.Clone();
        FilteredImage.Source = ImageSourceFromBitmap(Algorithm.AdjustBrightness(bitmap, (int)Range.Value));
    }

    private void Mean_Click(object sender, RoutedEventArgs e)
    {
        if (sourceImage == null)
        {
            MessageBox.Show("You haven't uploaded any files", "Image error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        Bitmap bitmap = new Bitmap(this.sourceImage.Width, this.sourceImage.Height);
        bitmap = (Bitmap)this.imageToEdit.Clone();
        FilteredImage.Source = ImageSourceFromBitmap(Algorithm.LocalMask(bitmap, 9));
    }
    private void Median_Click(object sender, RoutedEventArgs e)
    {
        if (sourceImage == null)
        {
            MessageBox.Show("You haven't uploaded any files", "Image error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        Bitmap bitmap = new Bitmap(this.sourceImage.Width, this.sourceImage.Height);
        bitmap = (Bitmap)this.imageToEdit.Clone();
        FilteredImage.Source = ImageSourceFromBitmap(Algorithm.LocalMask(bitmap, 9, false));
    }

    private void Sobel_Click(object sender, RoutedEventArgs e)
    {
        if (sourceImage == null)
        {
            MessageBox.Show("You haven't uploaded any files", "Image error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        Bitmap bitmap = new Bitmap(this.sourceImage.Width, this.sourceImage.Height);
        bitmap = (Bitmap)this.imageToEdit.Clone();
        double[,] matrixX = new double[,] { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };
        double[,] matrixY = new double[,] { { 1, 2, 1 }, { 0, 0, 0 }, { -1, -2, -1 } };
        FilteredImage.Source = ImageSourceFromBitmap(Algorithm.LinearFilter(bitmap, matrixX, matrixY));
    }

    private void Sharpen_Click(object sender, RoutedEventArgs e)
    {
        if (sourceImage == null)
        {
            MessageBox.Show("You haven't uploaded any files", "Image error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        Bitmap bitmap = new Bitmap(this.sourceImage.Width, this.sourceImage.Height);
        bitmap = (Bitmap)this.imageToEdit.Clone();
        double[,] matrixX = new double[,] { { -1, -1, -1, }, { -1, 9, -1, }, { -1, -1, -1, } };

        FilteredImage.Source = ImageSourceFromBitmap(Algorithm.LinearFilter(bitmap, matrixX, null));
    }

    private void Gaussian_Click(object sender, RoutedEventArgs e)
    {
        if (sourceImage == null)
        {
            MessageBox.Show("You haven't uploaded any files", "Image error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        Bitmap bitmap = new Bitmap(this.sourceImage.Width, this.sourceImage.Height);
        bitmap = (Bitmap)this.imageToEdit.Clone();
        double[,] matrixX = new double[,] { { 1, 2, 1, }, { 2, 4, 2, }, { 1, 2, 1, } };

        FilteredImage.Source = ImageSourceFromBitmap(Algorithm.LinearFilter(bitmap, matrixX, null, 1 / 16.0, true));
    }

    private void Mask_Click(object sender, RoutedEventArgs e)
    {
        if (sourceImage == null)
        {
            MessageBox.Show("You haven't uploaded any files", "Image error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (String.IsNullOrEmpty(TableValues.Text))
            return;
        Bitmap bitmap = new Bitmap(this.sourceImage.Width, this.sourceImage.Height);
        bitmap = (Bitmap)this.imageToEdit.Clone();
        double[] values = TableValues.Text.Split(',')!.Select(double.Parse)!.ToArray();
        int length = (int)Math.Floor(Math.Sqrt(values.Count()));
        double[,] matrixX = new double[length, length];

        for(int i = 0; i < length; i++)
        {
            for (int j = 0; j < length; j++)
            {
                matrixX[i, j] = values[i * length + j];
            }
        }

        FilteredImage.Source = ImageSourceFromBitmap(Algorithm.LinearFilter(bitmap, matrixX, null, 1, false));
    }

    private void Gray1_Click(object sender, RoutedEventArgs e)
    {
        if (sourceImage == null)
        {
            MessageBox.Show("You haven't uploaded any files", "Image error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        Bitmap bitmap = new Bitmap(this.sourceImage.Width, this.sourceImage.Height);
        bitmap = (Bitmap)this.imageToEdit.Clone();
        FilteredImage.Source = ImageSourceFromBitmap(Algorithm.GrayScale1(bitmap));
    }

    private void Gray2_Click(object sender, RoutedEventArgs e)
    {
        if (sourceImage == null)
        {
            MessageBox.Show("You haven't uploaded any files", "Image error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        Bitmap bitmap = new Bitmap(this.sourceImage.Width, this.sourceImage.Height);
        bitmap = (Bitmap)this.imageToEdit.Clone();
        FilteredImage.Source = ImageSourceFromBitmap(Algorithm.GrayScale2(bitmap));
    }
}
