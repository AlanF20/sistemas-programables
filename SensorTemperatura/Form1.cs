using System;
using System.Drawing;
using System.IO.Ports;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace SensorTemperatura
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                // Configuración inicial del puerto serie
                serialPort1.PortName = "COM5";
                serialPort1.BaudRate = 9600;
                serialPort1.Open();
                serialPort1.DataReceived += new SerialDataReceivedEventHandler(serialPort1_DataReceived);

                // Configuración del temporizador
                timer1.Interval = 500; // Reducir el intervalo a 500 milisegundos
                timer1.Start();

                // Inicializar DataGridView
                InitializeDataGridView();

                // Inicializar Chart
                InitializeChart();

                Log("Aplicación iniciada correctamente.");
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message);
                Log("Error al iniciar la aplicación: " + error.Message);
            }
        }

        private void InitializeDataGridView()
        {
            dataGridView1.ColumnCount = 2;
            dataGridView1.Columns[0].Name = "Timestamp";
            dataGridView1.Columns[1].Name = "Log Message";
        }

        private void InitializeChart()
        {
            chart1.Series.Clear();
            var series = new Series
            {
                Name = "Temperature",
                Color = Color.Blue,
                ChartType = SeriesChartType.Line
            };
            chart1.Series.Add(series);
            chart1.ChartAreas[0].AxisX.LabelStyle.Format = "HH:mm:ss";
            chart1.ChartAreas[0].AxisY.Title = "Temperature (C)";
            chart1.ChartAreas[0].AxisX.Title = "Time";
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                timer1.Stop();
                serialPort1.Close();
                Log("Puerto serie cerrado.");
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
                Log("Error al cerrar el puerto serie: " + err.Message);
            }
        }

        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string data = serialPort1.ReadLine();
                Log("Datos recibidos: " + data);

                string[] parts = data.Split(',');

                if (parts.Length == 2)
                {
                    UpdateUI(parts[0], parts[1]);
                }
                else
                {
                    Log("Error: Los datos recibidos no tienen el formato esperado.");
                }
            }
            catch (Exception ex)
            {
                Log("Error en el manejo de datos recibidos: " + ex.Message);
            }
        }

        private void UpdateUI(string tempStr, string motorState)
        {
            if (lblTemperatura.InvokeRequired || lblMotor.InvokeRequired)
            {
                this.Invoke(new Action<string, string>(UpdateUI), new object[] { tempStr, motorState });
                return;
            }

            lblTemperatura.Text = tempStr + " C";

            if (double.Parse(tempStr) >= 33)
            {
                lblMotor.Text = "MOTOR ON";
                lblMotor.BackColor = Color.Green;
            }
            else
            {
                lblMotor.Text = "MOTOR OFF";
                lblMotor.BackColor = Color.Red;
            }

            // Actualizar gráfico
            UpdateChart(tempStr);
        }

        private void UpdateChart(string tempStr)
        {
            if (chart1.InvokeRequired)
            {
                chart1.Invoke(new Action<string>(UpdateChart), new object[] { tempStr });
                return;
            }

            double temp;
            if (double.TryParse(tempStr, out temp))
            {
                chart1.Series["Temperature"].Points.AddXY(DateTime.Now, temp);
                if (chart1.Series["Temperature"].Points.Count > 100)
                {
                    chart1.Series["Temperature"].Points.RemoveAt(0);
                }
                chart1.ChartAreas[0].RecalculateAxesScale();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                Log("Solicitando actualización al Arduino.");
                serialPort1.WriteLine("GET_DATA");
            }
            catch (Exception ex)
            {
                Log("Error al solicitar actualización al Arduino: " + ex.Message);
            }
        }

        private void Log(string message)
        {
            if (txtLogs.InvokeRequired)
            {
                txtLogs.Invoke(new Action<string>(Log), new object[] { message });
            }
            else
            {
                txtLogs.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " - " + message + Environment.NewLine);
                dataGridView1.Rows.Add(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), message);
            }
        }

        private void lblTemperatura_Click(object sender, EventArgs e)
        {
            // Método sin uso, se puede eliminar
        }

        private void lblMotor_Click(object sender, EventArgs e)
        {
            // Método sin uso, se puede eliminar
        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }
    }
}
