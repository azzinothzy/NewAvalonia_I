using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Collections.Generic;
using OxyPlot;
using Avalonia;
using Avalonia.Threading;
using LiveChartsCore.Kernel;
using System.Timers;


namespace AvaloniaMvvmApp2.ViewModels
{
    
    public partial class MainWindowViewModel : ViewModelBase
    {
        #pragma warning disable CA1822 // Mark members as static
        public string Greeting => "Welcome to Avalonia!";
        #pragma warning restore CA1822 // Mark members as static
        public List<byte> BufferList { get; private set; } = new List<byte>(6000);
        public byte[] BinaryData {  get;  set; }
        bool ReceiveSign;
        public ISeries[] Series { get; set; }
        private readonly Random _random = new Random();
        private readonly Timer _timer;

        [ObservableProperty]
        private string selectedPort;
        [ObservableProperty]
        private ObservableCollection<DataPoint> chartSeries1 = new ObservableCollection<DataPoint>();
       
        [ObservableProperty]
        private ObservableCollection<DataPoint> chartSeries2 = new ObservableCollection<DataPoint>();

        [ObservableProperty]
        private ObservableCollection<ISeries> series2;

        public ObservableCollection<string> AvailablePorts { get; } = new ObservableCollection<string>();

        private SerialPort ?_serialPort;

        public MainWindowViewModel()
        {
            selectedPort = string.Empty;
            AvailablePorts.Clear();
            var ports = SerialPort.GetPortNames(); // 获取系统中所有可用的串口号
            foreach (var port in ports)
            {
                AvailablePorts.Add(port);
            }

            Series = new ISeries[]
            {
                 new LineSeries<DataPoint?>
                 {
                     Values=chartSeries1,
                     Mapping = (dataPoint, chartPoint) =>new(dataPoint.Argument, dataPoint.Value),
                // Mapping = (dataPoint, chartPoint) =>
                //{
                  
                //    chartPoint.Coordinate = new LiveChartsCore.Kernel.Sketches.Coordinate(dataPoint.Argument, dataPoint.Value);
                //},







                     LineSmoothness=0.9,
                     GeometrySize = 0,      // 设置点的大小为 0，隐藏点
        GeometryFill = null,   // 不填充点的内部
        GeometryStroke = null, // 不描边点
        Fill = null            // 不填充线条下方区域
                 }
    };
            var values = new ObservableCollection<double>();
            Series2 = new ObservableCollection<ISeries>
        {
            new LineSeries<double> { Values = values ,
                 LineSmoothness=0.9,
                     GeometrySize = 0,      // 设置点的大小为 0，隐藏点
        GeometryFill = null,   // 不填充点的内部
        GeometryStroke = null, // 不描边点
        Fill = null           
            }
        };

         
            _timer = new Timer(100);
            _timer.Elapsed += (sender, e) => UpdateData(values);

        }

        [RelayCommand]
        public void StartUpdating()
        {
            _timer.Start();
           // Console.WriteLine("开启");
        }

        // 更新数据点
        private void UpdateData(ObservableCollection<double> values)
        {
            // 在UI线程中更新数据
            Dispatcher.UIThread.InvokeAsync(() =>
            {
              
                values.Clear();

           
                for (int i = 0; i < 2048; i++)
                {
                    double randomValue = Math.Round(_random.NextDouble() * 5, 2); // 生成 0-5 范围的两位小数
                    values.Add(randomValue);
                }
            });
        }

        [RelayCommand]
        private void ConnectToPort()
        {
            if (string.IsNullOrEmpty(SelectedPort))
            {
                Console.WriteLine("请选择一个串口号！");
                return;
            }

            try
            {
                // 初始化并打开串口
                _serialPort = new SerialPort(SelectedPort, 512000, Parity.None, 8, StopBits.One);
                _serialPort.DataReceived += DataReceivedHandler4;
                _serialPort.Open();
            
                Console.WriteLine($"串口 {SelectedPort} 已连接。");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"无法连接到串口 {SelectedPort}：{ex.Message}");
            }
        }

        private void DataReceivedHandler4(object sender, SerialDataReceivedEventArgs e)
        {
            System.IO.Ports.SerialPort sp = (System.IO.Ports.SerialPort)sender;
            string portName = sp.PortName;

            int num = _serialPort.BytesToRead;
            byte[] ByteNum = new byte[num];
            _serialPort.Read(ByteNum, 0, num);
            BufferList.AddRange(ByteNum);
            while (BufferList.Count >= 9)
            {
                if (BufferList[0] == 0X46 && BufferList[1] == 0x42)
                {
                    int ByteCout =BufferList[2];//数据长度  1024/8
                    int ByteCout2 = ByteCout * 32 + 9 + 38;
                    if (BufferList.Count < ByteCout * 32 + 9 + 38)
                    {
                        break;
                    }
                    else
                    {
                        if (BufferList[ByteCout * 32 + 9 + 37] == 0X0A)
                        {
                            byte[] frame = new byte[BufferList.Count];

                            BufferList.CopyTo(0, frame, 0, ByteCout2);
                            BinaryData = frame;
                            ReceiveSign = true;


                            BufferList.RemoveRange(0, ByteCout2);

                        }
                        else
                        {
                            
                            BufferList.RemoveRange(0, ByteCout2);

                        }

                    }

                }
                else if (BufferList[0] == 0X46 &&BufferList[1] == 0x51)
                {
                    int ByteCout = BufferList[2];//数据长度  

                    if (BufferList.Count < ByteCout * 32 + 9)
                    {
                        break;
                    }
                    else
                    {
                        if (BufferList[ByteCout * 32 + 8] == 0X0A)
                        {
                            byte[] frame = new byte[BufferList.Count];

                           BufferList.CopyTo(0, frame, 0, BufferList.Count);
                           BinaryData = frame;
                           BufferList.RemoveRange(0,BufferList.Count);
                        }
                        else
                        {
                           BufferList.RemoveRange(0,BufferList.Count);
                        }

                    }
                }
                else if (BufferList[0] == 0X46 &&BufferList[1] == 0xCF)
                {
                    int ByteCout = BufferList[2];//数据长度 

                    if (BufferList.Count < ByteCout + 5)
                    {
                        break;
                    }
                    else
                    {
                        if (BufferList[ByteCout + 4] == 0X0A)
                        {
                            byte[] frame = new byte[BufferList.Count];

                            BufferList.CopyTo(0, frame, 0, BufferList.Count);
                            BinaryData = frame;
                            BufferList.RemoveRange(0, ByteCout + 5);
                        }
                        else
                        {

                            BufferList.RemoveRange(0, ByteCout + 5);
                        }

                    }
                }
                else
                {
                  
                    BufferList.RemoveAt(0);

                }
                if (ReceiveSign)
                {
                    byte[] bytedata = BinaryData;

                    int chartCount = 0;
                    Int16 dataA, dataB;
                    double dataAA, dataBB;

                    ObservableCollection<DataPoint> chartSeries1;
                    chartSeries1 = new ObservableCollection<DataPoint>();
                    ObservableCollection<DataPoint> chartSeries2;
                    chartSeries2 = new ObservableCollection<DataPoint>();
                    double max_value1, min_value1, max_value2, min_value2;
                  
                    dataA = (Int16)((bytedata[3] << 8 | bytedata[4]));
                    max_value1 = ((double)dataA / 3200);
                    min_value1 = ((double)dataA / 3200);
                    dataA = (Int16)((bytedata[5] << 8 | bytedata[6]));
                    max_value2 = ((double)dataA / 3200);
                    min_value2 = ((double)dataA / 3200);
                    Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        ChartSeries1.Clear();
                    });
                    for (int i = 3; i < (bytedata[2] * 32 + 4); i += 4)
                        {
                            try
                            {
                                dataA = (Int16)((bytedata[i] << 8) | bytedata[i + 1]);
                                dataAA = (double)dataA / 3200;
                                if (dataAA > max_value1)
                                {
                                    max_value1 = dataAA;
                                }
                                else if (dataAA < min_value1)
                                {
                                    min_value1 = dataAA;
                                }

                                chartCount++;

                                dataB = (Int16)((bytedata[i + 2] << 8) | bytedata[i + 3]);
                                dataBB = (double)dataB / 3200;
                                if (dataBB > max_value2)
                                {
                                    max_value2 = dataBB;
                                }
                                else if (dataBB < min_value2)
                                {
                                    min_value2 = dataBB;
                                }

                            //  if (serialPortServo.IsSelected)
                            Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                    chartSeries1.Add(new DataPoint(chartCount, dataAA));
                                    chartSeries2.Add(new DataPoint(chartCount, dataBB));
                                    ChartSeries1.Add(new DataPoint(chartCount, dataAA));

                             });



                            }
                            catch (Exception ex)
                            {


                            }
                        }
                 
                 

                  
                 
                }


            }
        }

        [RelayCommand]
        private void GenerateRandomData()
        {
          
        }
    }

    public class DataPoint
    {
        public double Argument { get; set; } // X 轴值
        public double Value { get; set; } // Y 轴值

        public DataPoint(double argument, double value)
        {
            Argument = argument;
            Value = value;
        }
    }
    //你好
    //1-1
    //1-2
}
