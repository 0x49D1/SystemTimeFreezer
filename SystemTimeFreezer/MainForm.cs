using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SystemTimeFreezer
{
    public partial class MainForm : Form
    {

        [DllImport("kernel32.dll", EntryPoint = "GetSystemTime", SetLastError = true)]
        public extern static void Win32GetSystemTime(ref SystemTime sysTime);

        [DllImport("kernel32.dll", EntryPoint = "SetSystemTime", SetLastError = true)]
        public extern static bool Win32SetSystemTime(ref SystemTime sysTime);

        DateTime dt;

        public event EventHandler Minimize;

        public struct SystemTime
        {
            public ushort Year;
            public ushort Month;
            public ushort DayOfWeek;
            public ushort Day;
            public ushort Hour;
            public ushort Minute;
            public ushort Second;
            public ushort Millisecond;
        };

        SystemTime updatedTime;

        Timer t = new Timer();

        public MainForm()
        {
            // Check program instances
            string proc = Process.GetCurrentProcess().ProcessName;
            Process[] processes = Process.GetProcessesByName(proc);
            if (processes.Length > 1)
            {
                MessageBox.Show("Application is already running");
                return;
            }

            InitializeComponent();

            // Minimize event handler
            this.Minimize += new EventHandler(this.Form_Minimize);
            t.Interval = 2000;
            t.Tick += (s, e) => Win32SetSystemTime(ref updatedTime);

        }

        protected override void WndProc(ref Message msg)
        {
            const int WM_SIZE = 0x0005;
            const int SIZE_MINIMIZED = 1;

            if ((msg.Msg == WM_SIZE) && ((int)msg.WParam == SIZE_MINIMIZED) &&
            (this.Minimize != null))
            {
                this.Minimize(this, EventArgs.Empty);
            }

            base.WndProc(ref msg);
        }

        private void Form_Minimize(object sender, EventArgs e)
        {
            ShowTimer();
        }

        private SystemTime SetSystemTimeStatic(DateTime args)
        {
            dt = args;

            SystemTime updatedTime = new SystemTime();
            updatedTime.Year = (ushort)dt.Year;
            updatedTime.Month = (ushort)dt.Month;
            updatedTime.Day = (ushort)dt.Day;
            // UTC time; it will be modified according to the regional settings of the target computer so the actual hour might differ
            updatedTime.Hour = (ushort)dt.Hour;
            updatedTime.Minute = (ushort)dt.Minute;
            updatedTime.Second = (ushort)dt.Second;
            return updatedTime;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            updatedTime = SetSystemTimeStatic(dtFreeze.Value.ToUniversalTime());

            t.Start();
            btnStop.Enabled = true;
            EnableButtons();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            t.Stop();
            btnStop.Enabled = false;
            EnableButtons();
        }

        private void EnableButtons()
        {
            btnStart.Enabled = !btnStop.Enabled;
        }

        private void showToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowTimer();
        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            ShowTimer();
        }

        private void ShowTimer()
        {
            this.Visible = !this.Visible;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

    }
}
