using System;
using System.IO.Ports;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;
using OxyPlot.Wpf;
namespace UltrasonicSensorVisualizer
{
    using OxyPlot;
    using OxyPlot.Series;
    using OxyPlot.Axes;
    using System;

    public static class StaticPlotter
    {
        private static PlotModel plotModel;
        private static LineSeries series;
        private static DateTime startTime;

        static StaticPlotter()
        {
            InitializePlot();
        }

        private static void InitializePlot()
        {
            plotModel = new PlotModel { Title = "Distance Measurement Over Time" };
            series = new LineSeries { Title = "Distance" };

            plotModel.Series.Add(series);
            plotModel.Axes.Add(new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                StringFormat = "HH:mm:ss",
                Title = "Time",
                IsZoomEnabled = false,
                IsPanEnabled = false
            });
            plotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Distance (mm)",
                Minimum = 0,
                IsZoomEnabled = false,
                IsPanEnabled = false
            });

            startTime = DateTime.Now;
        }

        public static void AddDataPoint(double distance)
        {
            var timeSpan = DateTime.Now - startTime;
            series.Points.Add(new DataPoint(DateTimeAxis.ToDouble(DateTime.Now), distance));
            plotModel.InvalidatePlot(true);
            ExportPlot();
        }

        private static void ExportPlot()
        {
            var pngExporter = new PngExporter { Width = 600, Height = 400 };
            pngExporter.ExportToFile(plotModel, "plot.png");
        }
    }

    class Program
    {


   
    static void Main(string[] args)
        {
            SerialPort mySerialPort = new SerialPort("COM6", 9600); // Adjust COM port

            mySerialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
            mySerialPort.Open();

            Console.WriteLine("Press any key to close the application.");
            Console.ReadKey();
            mySerialPort.Close();
        }

        private static void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (sender as SerialPort);
            string indata = sp.ReadLine();
            Console.WriteLine("Distance: " + indata + "mm");
            if (double.TryParse(indata.Replace("Distance: ", "").Replace("mm", ""), out double val))
            {
                Thread staThread = new Thread(() => StaticPlotter.AddDataPoint(val));
                staThread.SetApartmentState(ApartmentState.STA);
                staThread.Start();
                staThread.Join(); // Optional: Wait for the thread to finish if necessary
            }
        }

    }

}
