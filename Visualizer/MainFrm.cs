using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;
using System.IO.Ports;
using System.Threading;

namespace Visualizer
{
    public partial class MainFrm : Form
    {
        private BackgroundWorker bw;
        private bool isConnected;
        private String selectedPortName;
        private float[] tempMap = new float[8 * 8];
        private float minTemp;
        private float maxTemp;

        private float minTempCorr = -0.5f;
        private float maxTempCorr = +0.5f;

        private int SCALE = 10;

        private static readonly Color[] DEFAULT_COLOR_SCHEME = new Color[]{
            Color.FromArgb(28, 1, 108),
            Color.FromArgb(31, 17, 218),
            Color.FromArgb(50, 111, 238),
            Color.FromArgb(63, 196, 229),
            Color.FromArgb(64, 222, 135),
            Color.FromArgb(192, 240, 14),
            Color.FromArgb(223, 172, 18),
            Color.FromArgb(209, 111, 14),
            Color.FromArgb(210, 50, 28),
            Color.FromArgb(194, 26, 0),
            Color.FromArgb(132, 26, 0)
        };
        // ReSharper disable once UnusedMember.Local
        private static readonly Color[] ALTERNATE_COLOR_SCHEME = new Color[]{
            Color.FromArgb(0, 0, 5),
            Color.FromArgb(7, 1, 97),
            Color.FromArgb(51, 1, 194),
            Color.FromArgb(110, 2, 212),
            Color.FromArgb(158, 6, 150),
            Color.FromArgb(197, 30, 58),
            Color.FromArgb(218, 66, 0),
            Color.FromArgb(237, 137, 0),
            Color.FromArgb(246, 199, 23),
            Color.FromArgb(251, 248, 117),
            Color.FromArgb(252, 254, 253)
        };

        public MainFrm()
        {
            InitializeComponent();
            fillPortsList();
            thermalMap.Image = new Bitmap(8 * SCALE, 8 * SCALE, PixelFormat.Format24bppRgb);
        }

        private void UpdateControls()
        {
            if (isConnected)
            {
                cbSerialPorts.Enabled = false;
                btnConnect.Text = @"Disconnect";
            }
            else
            {
                cbSerialPorts.Enabled = true;
                btnConnect.Text = @"Connect";
            }
        }

        private void fillPortsList()
        {
            var ports = SerialPort.GetPortNames().ToList();
            cbSerialPorts.Items.Clear();
            ports.ForEach(x => cbSerialPorts.Items.Add(x));
            if (cbSerialPorts.Items.Count > 0)
            {
                cbSerialPorts.SelectedIndex = 0;
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (!isConnected)
            {
                if (cbSerialPorts.SelectedItem != null)
                {
                    selectedPortName = cbSerialPorts.SelectedItem.ToString();
                    isConnected = true;
                    StartProcess();
                }
            }
            else
            {
                bw?.CancelAsync();
            }
            UpdateControls();
        }

        private void StartProcess()
        {
            bw = new BackgroundWorker { WorkerReportsProgress = true, WorkerSupportsCancellation = true };

            bw.DoWork += delegate (object o, DoWorkEventArgs args)
            {
                var worker = (BackgroundWorker)o;

                var serialPort = new SerialPort
                {
                    PortName = selectedPortName,
                    BaudRate = 9600,
                    Parity = Parity.None,
                    DataBits = 8,
                    StopBits = StopBits.One,
                    Handshake = Handshake.None,
                    ReadTimeout = 1000,
                    WriteTimeout = 1000
                };
                serialPort.Open();

                while (!worker.CancellationPending)
                {
                    serialPort.Write("r");
                    string tempLine = serialPort.ReadLine();
                    if (tempLine.Length >= 64 * 6)
                    {
                        for (int i = 0; i < 64; i++)
                        {
                            tempMap[i] = float.Parse(tempLine.Substring(i * 6, 6));
                        }
                        worker.ReportProgress(0);
                        Thread.Sleep(5);
                    }
                }

                serialPort.Close();

                if (worker.CancellationPending)
                {
                    args.Cancel = true;
                }

            };

            bw.RunWorkerCompleted += delegate
            {
                isConnected = false;
                UpdateControls();
            };

            bw.ProgressChanged += delegate (object o, ProgressChangedEventArgs args)
            {
                findMinAndMaxTemp();
                drawThermalMap(SCALE, (Bitmap)thermalMap.Image);
                thermalMap.Invalidate();
                thermalMap.Refresh();
                Application.DoEvents();
            };

            bw.RunWorkerAsync();
        }

        private void drawThermalMap(int scale, Bitmap bitmap)
        {
            int res = 8;
            int line = 0;
            int row = 0;
            int interpRes =  res * scale;
            Color[] colors = new Color[interpRes * interpRes];

            //interpolate temperature map
            float[] interpTempMap = interpolateTempMap(tempMap, scale);

            for (int i = 0; i < interpRes * interpRes; i++)
            {
                colors[i] = calculateColor(DEFAULT_COLOR_SCHEME, interpTempMap[i], minTemp + minTempCorr, maxTemp + maxTempCorr);
            }
            while (line < interpRes)
            {
                while (row < interpRes)
                {
                    bitmap.SetPixel(row, line, colors[line * interpRes + row]);
                    row++;
                }
                row = 0;
                line++;
            }
        }

        private float[] interpolateTempMap(float[] srcMap, int scale)
        {
            float tmp, u, t, d1, d2, d3, d4;
            float p1, p2, p3, p4;
            int originRes = 8;
            int newRes = originRes * scale;
            int idx = 0;

            float[] resultMap = new float[newRes * newRes];

            for (int j = 0; j < newRes; j++)
            {
                tmp = (float)j / (newRes - 1) * (originRes - 1);
                int h = (int)tmp;
                if (h >= originRes - 1)
                {
                    h = originRes - 2;
                }
                u = tmp - h;

                idx = (j * newRes);

                for (int i = 0; i < newRes; i++)
                {
                    tmp = (float)(i) / (float)(newRes - 1) * (originRes - 1);
                    int w = (int)tmp;
                    if (w >= originRes - 1)
                    {
                        w = originRes - 2;
                    }
                    t = tmp - w;

                    d1 = (1 - t) * (1 - u);
                    d2 = t * (1 - u);
                    d3 = t * u;
                    d4 = (1 - t) * u;

                    p1 = srcMap[h * originRes + w];
                    p2 = srcMap[h * originRes + w + 1];
                    p3 = srcMap[(h + 1) * originRes + w + 1];
                    p4 = srcMap[(h + 1) * originRes + w];

                    float temp = p1 * d1 + p2 * d2 + p3 * d3 + p4 * d4;
                    resultMap[idx] = temp;

                    idx++;
                }
            }

            return resultMap;
        }

        private void findMinAndMaxTemp()
        {
            this.minTemp = 1000;
            this.maxTemp = -100;
            for (int i = 0; i < 64; i++)
            {
                if (tempMap[i] < minTemp)
                {
                    minTemp = tempMap[i];
                }
                if (tempMap[i] > maxTemp)
                {
                    maxTemp = tempMap[i];
                }
            }
        }


        private byte calculateRGB(byte rgb1, byte rgb2, float t1, float step, float t)
        {
            return (byte)(rgb1 + (((t - t1) / step) * (rgb2 - rgb1)));
        }

        private Color calculateColor(Color[] colorScheme, float temperature, float min, float max)
        {
            Color val;
            float step = (max - min) / (colorScheme.Length - 1);
            if (temperature < min)
            {
                val = colorScheme[0];
            }
            else if (temperature >= max)
            {
                val = colorScheme[colorScheme.Count() - 1];
            }
            else
            {
                int step1 = (int)((temperature - min) / step);
                int step2 = step1 + 1;
                Color col1 = colorScheme[step1];
                Color col2 = colorScheme[step2];
                byte red = calculateRGB(col1.R, col2.R, (min + step1 * step), step, temperature);
                byte green = calculateRGB(col1.G, col2.G, (min + step1 * step), step, temperature);
                byte blue = calculateRGB(col1.B, col2.B, (min + step1 * step), step, temperature);
                val = Color.FromArgb(red, green, blue);
            }
            return val;
        }

        private void thermalMap_Paint(object sender, PaintEventArgs e)
        {
            
        }

        private void MainFrm_Paint(object sender, PaintEventArgs e)
        {
            
        }
    }
}
