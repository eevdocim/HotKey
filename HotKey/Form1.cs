using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Diagnostics;
using System.IO;

namespace HotKey
{
    public partial class Form1 : Form
    {
        bool exit = false;
        bool gamemode = false;
        string[] app = new string[12];
        string[] url = new string[12];
        FullSceen fullscreen = new FullSceen();
        private OpenFileDialog openDialog = new OpenFileDialog();

        //название кнопки
        [StructLayout(LayoutKind.Sequential)]
        private struct KBDLLHOOKSTRUCT
        {
            public Keys key;
        }

        private struct MSLLHOOKSTRUCT
        {
            public Point pt;
            public int mouseData;
            /*public int flags;
            public int time;
            public long dwExtraInfo;*/
        }
        //работа с окнами приложений
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ShowWindow(IntPtr hWnd, int showWindowCommand);


        //мышь
        private const int WH_MOUSE_LL = 14;// устанавливает перехват на низком уровне
        private const int WM_MOUSEWHEEL = 0x020A;//колесико прокрутки 120 вверх -120 вниз
        private const int WM_MBUTTONDOWN = 0x0207;




        private LowLevelMouseProcDelegate m_callback_mouse;

        private IntPtr m_hHook_mouse;

        //клава
        private const int WH_KEYBOARD_LL = 13; // устанавливает перехват на низком уровне
        private const int WM_KEYDOWN = 0x0100; // нажатие клавиши 

        //звук
        private const int APPCOMMAND_VOLUME_MUTE = 0x80000;//отключение звука
        private const int APPCOMMAND_VOLUME_UP = 0xA0000;//увеличение громкости
        private const int APPCOMMAND_VOLUME_DOWN = 0x90000;//уменьшение громкости
        private const int WM_APPCOMMAND = 0x319;//выполнение команд


        //звук
        [DllImport("user32.dll")]
        public static extern IntPtr SendMessageW(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        //получение окна
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr GetWindow(IntPtr hWnd, int uCmd);

        //перехват клавиатуры
        private LowLevelKeyboardProcDelegate[] m_callback = new LowLevelKeyboardProcDelegate[16];

        private IntPtr[] m_hHook = new IntPtr[16];

        //установка перехвата мыши 
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProcDelegate lpfn, IntPtr hMod, int dwThreadId);
        //установка видимости курсора
        [DllImport("user32.dll")]
        private static extern int ShowCursor(bool bShow);
        //установка перехвата 
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProcDelegate lpfn, IntPtr hMod, int dwThreadId);

        //разблокировка 
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        //Hook handle
        [DllImport("Kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetModuleHandle(IntPtr lpModuleName);

        //вызов следующего захвата
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
        bool wheel = false;

        private delegate IntPtr LowLevelKeyboardProcDelegate(int nCode, IntPtr wParam, IntPtr lParam);

        private delegate IntPtr LowLevelMouseProcDelegate(int nCode, IntPtr wParam, IntPtr lParam);

        public Form1()
        {
            InitializeComponent();
            Console.WriteLine("Start");
            openDialog.Filter = "Application files (*.exe)|*.exe|All files (*.*)|*.*";
            try
            {
                using (StreamReader sr = new StreamReader("config.dat"))
                {
                    for (byte i = 0; i < url.Length; i++)
                    {
                        url[i] = sr.ReadLine();
                    }
                    F1_on.Text = sr.ReadLine();
                    F2_on.Text = sr.ReadLine();
                    F3_on.Text = sr.ReadLine();
                    F4_on.Text = sr.ReadLine();
                    F5_on.Text = sr.ReadLine();
                    F6_on.Text = sr.ReadLine();
                    F7_on.Text = sr.ReadLine();
                    F8_on.Text = sr.ReadLine();
                    F9_on.Text = sr.ReadLine();
                    F10_on.Text = sr.ReadLine();
                    F11_on.Text = sr.ReadLine();
                    F12_on.Text = sr.ReadLine();
                    volume_on.Text = sr.ReadLine();
                    gamemode = Convert.ToBoolean(sr.ReadLine());
                    for (byte i = 0; i < app.Length; i++)
                    {
                        app[i] = sr.ReadLine();
                    }
                    sr.Close();
                }
                if (F1_on.Text == "ON")
                {
                    Console.WriteLine("Start hook F1 key");
                    F1_on.BackColor = Color.Green;
                }
                if (F2_on.Text == "ON")
                {
                    Console.WriteLine("Start hook F2 key");
                    F2_on.BackColor = Color.Green;
                }
                if (F3_on.Text == "ON")
                {
                    Console.WriteLine("Start hook F3 key");
                    F3_on.BackColor = Color.Green;
                }
                if (F4_on.Text == "ON")
                {
                    Console.WriteLine("Start hook F4 key");
                    F4_on.BackColor = Color.Green;
                }
                if (F5_on.Text == "ON")
                {
                    Console.WriteLine("Start hook F5 key");
                    F5_on.BackColor = Color.Green;
                }
                if (F6_on.Text == "ON")
                {
                    Console.WriteLine("Start hook F6 key");
                    F6_on.BackColor = Color.Green;
                }
                if (F7_on.Text == "ON")
                {
                    Console.WriteLine("Start hook F7 key");
                    F7_on.BackColor = Color.Green;
                }
                if (F8_on.Text == "ON")
                {
                    Console.WriteLine("Start hook F8 key");
                    F8_on.BackColor = Color.Green;
                }
                if (F9_on.Text == "ON")
                {
                    Console.WriteLine("Start hook F9 key");
                    F9_on.BackColor = Color.Green;
                }
                if (F10_on.Text == "ON")
                {
                    Console.WriteLine("Start hook F10 key");
                    F10_on.BackColor = Color.Green;
                }
                if (F11_on.Text == "ON")
                {
                    Console.WriteLine("Start hook F11 key");
                    F11_on.BackColor = Color.Green;
                }
                if (F12_on.Text == "ON")
                {
                    Console.WriteLine("Start hook F12 key");
                    F12_on.BackColor = Color.Green;
                }
                if (volume_on.Text == "ON")
                {
                    volume_on.BackColor = Color.Green;
                }
                if (gamemode)
                {
                    Console.WriteLine("Start hook win key");
                    GM_Button.BackColor = Color.Green;
                }
                F1_BOX.Text = app[0];
                F2_BOX.Text = app[1];
                F3_BOX.Text = app[2];
                F4_BOX.Text = app[3];
                F5_BOX.Text = app[4];
                F6_BOX.Text = app[5];
                F7_BOX.Text = app[6];
                F8_BOX.Text = app[7];
                F9_BOX.Text = app[8];
                F10_BOX.Text = app[9];
                F11_BOX.Text = app[10];
                F12_BOX.Text = app[11];
            }
            catch {
                StreamWriter sw = new StreamWriter("config.dat");
                sw.Close();
            }


            m_callback[0] = LowLevelKeyboardHookProc_key;//вызов функции перехвата клавиши

            //настройка перехвата
            m_hHook[0] = SetWindowsHookEx(WH_KEYBOARD_LL, m_callback[0], GetModuleHandle(IntPtr.Zero), 0);
        }
       
        private void Mute()
        {
            SendMessageW(this.Handle, WM_APPCOMMAND, this.Handle,
                (IntPtr)APPCOMMAND_VOLUME_MUTE);
        }

        private void VolDown()
        {
            SendMessageW(this.Handle, WM_APPCOMMAND, this.Handle,
                (IntPtr)APPCOMMAND_VOLUME_DOWN);
        }

        private void VolUp()
        {
            SendMessageW(this.Handle, WM_APPCOMMAND, this.Handle,
                (IntPtr)APPCOMMAND_VOLUME_UP);
        }
        
        //блокировка mouse move
        private IntPtr LowLevelMouseProc_move(int nCode, IntPtr wParam, IntPtr lParam)
        {
            Console.WriteLine("mouse move");
            ShowCursor(true);
            UnhookWindowsHookEx(m_hHook_mouse);
            return CallNextHookEx(m_hHook_mouse, nCode, wParam, lParam);
        }

        //блокировка mouse wheel
        private IntPtr LowLevelMouseProc_wheel(int nCode, IntPtr wParam, IntPtr lParam)
        {
            MSLLHOOKSTRUCT mouseInfo = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
            if(wParam == (IntPtr)519)
            {
                Mute();
            }
            if (wParam == (IntPtr)WM_MOUSEWHEEL)
            {
                if (mouseInfo.mouseData > 0)
                {
                    Console.WriteLine("up");
                    VolUp();
                }
                else
                {
                    Console.WriteLine("down");
                    VolDown();
                }
                Console.WriteLine("press wheel");
                return (IntPtr)1;//возрашает не нажатие клавиши
            }
            return CallNextHookEx(m_hHook_mouse, nCode, wParam, lParam);
        }
        //перехват всей клавиатуры
        private IntPtr LowLevelKeyboardHookProc_key(int nCode, IntPtr wParam, IntPtr lParam)
        {

            if (nCode >= 0)
            {
                KBDLLHOOKSTRUCT keyInfo = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(KBDLLHOOKSTRUCT));
                Console.WriteLine(keyInfo.key);
                if (keyInfo.key == Keys.LMenu && wParam == (IntPtr)260)
                {
                    if (!wheel)
                    {
                        wheel = true;
                        //настройка перехвата
                        m_callback_mouse = LowLevelMouseProc_wheel;//вызов функции перехвата колёсико мыши
                        Console.WriteLine("start wheel");
                        m_hHook_mouse = SetWindowsHookEx(WH_MOUSE_LL, m_callback_mouse, GetModuleHandle(IntPtr.Zero), 0);
                        Console.WriteLine("press alt");
                    }
                }
                else if (wParam == (IntPtr)257)
                {
                    if (wheel)
                    {
                        wheel = false;
                        Console.WriteLine("stop wheel");
                        UnhookWindowsHookEx(m_hHook_mouse);
                    }
                }
                //Нажатие кнопки
                if (wParam == (IntPtr)WM_KEYDOWN)
                    {
                        //windows key
                        if (keyInfo.key == Keys.RWin && fullscreen.isfull() && gamemode || keyInfo.key == Keys.LWin && fullscreen.isfull() && gamemode)
                        {
                            Console.WriteLine("press winkey");
                            return (IntPtr)1;
                        }
                        //увеличение громкости
                        if (keyInfo.key == Keys.Add && !Control.IsKeyLocked(Keys.NumLock))
                        {
                            Console.WriteLine("press plus");
                            VolUp();
                            return (IntPtr)1;
                        }
                        //уменьшение громкости
                        if (keyInfo.key == Keys.Subtract && !Control.IsKeyLocked(Keys.NumLock))
                        {
                            Console.WriteLine("press minus");
                            VolDown();
                            return (IntPtr)1;
                        }
                        //функциональные клавиши
                        var key = this.Controls[keyInfo.key.ToString() + "_on"];
                        if (key != null && !fullscreen.isfull())
                        {
                            Console.WriteLine(key.Text);
                            if (key.Text == "ON")
                            {
                                RunProgramm(int.Parse(keyInfo.key.ToString().ToCharArray()[1].ToString()) - 1);
                                return (IntPtr)1;
                            }
                        }
                    }
            }
            return CallNextHookEx(m_hHook[0], nCode, wParam, lParam);
        }
        
        //запуск программы
        private void RunProgramm(int key)
        {
            try
            {
                if (Process.GetProcessesByName(app[key]).Length == 0)
                {
                    ProcessStartInfo stInfo = new ProcessStartInfo(url[key]);
                    Process proc = new Process();
                    proc.StartInfo = stInfo;
                    proc.Start();
                    app[key] = proc.ProcessName;
                }
                else
                {
                    Process[] procs = Process.GetProcessesByName(app[key]);
                    foreach (Process p in procs)
                    {
                        ShowWindow(p.MainWindowHandle, 3);
                        SetForegroundWindow(p.MainWindowHandle);
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error:" + ex, "OK");
            }
        } 

        //включение перехвата win key
        private void button1_Click(object sender, EventArgs e)
        {
            if (GM_Button.BackColor == Color.Red)
            {
                Console.WriteLine("Start hook win key");
                GM_Button.BackColor = Color.Green;
                gamemode = true;
                SaveSettings();
            }
            else
            {
                Console.WriteLine("Stop hook win key");
                GM_Button.BackColor = Color.Red;
                gamemode = false;
                SaveSettings();
            }
        }
       
        private void volume_on_Click(object sender, EventArgs e)
        {
            if (volume_on.BackColor == Color.Red)
            {
                volume_on.Text = "ON";
                volume_on.BackColor = Color.Green;
                SaveSettings();
            }
            else
            {
                volume_on.Text = "OFF";
                volume_on.BackColor = Color.Red;
                SaveSettings();
            }
        }

        //включение перехвата F1
        private void F1_on_Click(object sender, EventArgs e)
        {
            if (F1_on.BackColor == Color.Red)
            {
                if (url[0] == null || url[0] == "")
                {
                    F1_button_Click(sender, e);
                }
                else
                {
                    Console.WriteLine("Start hook F1 key");
                    F1_on.Text = "ON";
                    F1_on.BackColor = Color.Green;
                    SaveSettings();
                }
            }
            else
            {
                Console.WriteLine("Stop hook F1 key");
                F1_on.Text = "OFF";
                F1_on.BackColor = Color.Red;
                SaveSettings();
            }
        }
       
        //включение перехвата F2
        private void F2_on_Click(object sender, EventArgs e)
        {
            if (F2_on.BackColor == Color.Red)
            {
                if (url[1] == null || url[1] == "")
                {
                    F2_button_Click(sender, e);
                }
                else
                {
                    Console.WriteLine("Start hook F2 key");
                    F2_on.Text = "ON";
                    F2_on.BackColor = Color.Green;
                    SaveSettings();
                }
            }
            else
            {
                Console.WriteLine("Stop hook F2 key");
                F2_on.Text = "OFF";
                F2_on.BackColor = Color.Red;
                SaveSettings();
            }
        }

        //включение перехвата F3
        private void F3_on_Click(object sender, EventArgs e)
        {
            if (F3_on.BackColor == Color.Red)
            {
                if (url[2] == null || url[2] == "")
                {
                    F3_button_Click(sender, e);
                }
                else
                {
                    Console.WriteLine("Start hook F3 key");
                    F3_on.Text = "ON";
                    F3_on.BackColor = Color.Green;
                    SaveSettings();
                }
            }
            else
            {
                Console.WriteLine("Stop hook F3 key");
                F3_on.Text = "OFF";
                F3_on.BackColor = Color.Red;
                SaveSettings();
            }
        }
        
        //включение перехвата F4
        private void F4_on_Click(object sender, EventArgs e)
        {
            if (F4_on.BackColor == Color.Red)
            {
                if (url[3] == null || url[3] == "")
                {
                    F4_button_Click(sender, e);
                }
                else
                {
                    Console.WriteLine("Start hook F4 key");
                    F4_on.Text = "ON";
                    F4_on.BackColor = Color.Green;
                    SaveSettings();
                }
            }
            else
            {
                Console.WriteLine("Stop hook F4 key");
                F4_on.Text = "OFF";
                F4_on.BackColor = Color.Red;
                SaveSettings();
            }
        }

        //включение перехвата F5
        private void F5_on_Click(object sender, EventArgs e)
        {
            if (F5_on.BackColor == Color.Red)
            {
                if (url[4] == null || url[4] == "")
                {
                    F5_button_Click(sender, e);
                }
                else
                {
                    Console.WriteLine("Start hook F5 key");
                    F5_on.Text = "ON";
                    F5_on.BackColor = Color.Green;
                    SaveSettings();
                }
            }
            else
            {
                Console.WriteLine("Stop hook F5 key");
                F5_on.Text = "OFF";
                F5_on.BackColor = Color.Red;
                SaveSettings();
            }
        }

        //включение перехвата F6
        private void F6_on_Click(object sender, EventArgs e)
        {
            if (F6_on.BackColor == Color.Red)
            {
                if (url[5] == null || url[5] == "")
                {
                    F6_button_Click(sender, e);
                }
                else
                {
                    Console.WriteLine("Start hook F6 key");
                    F6_on.Text = "ON";
                    F6_on.BackColor = Color.Green;
                    SaveSettings();
                }
            }
            else
            {
                Console.WriteLine("Stop hook F6 key");
                F6_on.Text = "OFF";
                F6_on.BackColor = Color.Red;
                SaveSettings();
            }
        }

        //включение перехвата F7
        private void F7_on_Click(object sender, EventArgs e)
        {
            if (F7_on.BackColor == Color.Red)
            {
                if (url[6] == null || url[6] == "")
                {
                    F7_button_Click(sender, e);
                }
                else
                {
                    Console.WriteLine("Start hook F7 key");
                    F7_on.Text = "ON";
                    F7_on.BackColor = Color.Green;
                    SaveSettings();
                }
            }
            else
            {
                Console.WriteLine("Stop hook F7 key");
                F7_on.Text = "OFF";
                F7_on.BackColor = Color.Red;
                SaveSettings();
            }
        }
        
        //включение перехвата F8
        private void F8_on_Click(object sender, EventArgs e)
        {
            if (F8_on.BackColor == Color.Red)
            {
                if (url[7] == null || url[7] == "")
                {
                    F8_button_Click(sender, e);
                }
                else
                {
                    Console.WriteLine("Start hook F8 key");
                    F8_on.Text = "ON";
                    F8_on.BackColor = Color.Green;
                    SaveSettings();
                }
            }
            else
            {
                Console.WriteLine("Stop hook F8 key");
                F8_on.Text = "OFF";
                F8_on.BackColor = Color.Red;
                SaveSettings();
            }
        }
        
        //включение перехвата F9
        private void F9_on_Click(object sender, EventArgs e)
        {
            if (F9_on.BackColor == Color.Red)
            {
                if (url[8] == null || url[8] == "")
                {
                    F9_button_Click(sender, e);
                }
                else
                {
                    Console.WriteLine("Start hook F9 key");
                    F9_on.Text = "ON";
                    F9_on.BackColor = Color.Green;
                    SaveSettings();
                }
            }
            else
            {
                Console.WriteLine("Stop hook F9 key");
                F9_on.Text = "OFF";
                F9_on.BackColor = Color.Red;
                SaveSettings();
            }
        }
        
        //включение перехвата F10
        private void F10_on_Click(object sender, EventArgs e)
        {
            if (F10_on.BackColor == Color.Red)
            {
                if (url[9] == null|| url[9] == "")
                {
                    F10_button_Click(sender, e);
                }
                else
                {
                    Console.WriteLine("Start hook F10 key");
                    F10_on.Text = "ON";
                    F10_on.BackColor = Color.Green;
                    SaveSettings();
                }
            }
            else
            {
                Console.WriteLine("Stop hook F10 key");
                F10_on.Text = "OFF";
                F10_on.BackColor = Color.Red;
                SaveSettings();
            }
        }
       
        //включение перехвата F11
        private void F11_on_Click(object sender, EventArgs e)
        {
            if (F11_on.BackColor == Color.Red)
            {
                if (url[10] == null || url[10] == "")
                {
                    F11_button_Click(sender, e);
                }
                else
                {
                    Console.WriteLine("Start hook F11 key");
                    F11_on.Text = "ON";
                    F11_on.BackColor = Color.Green;
                    SaveSettings();
                }
            }
            else
            {
                Console.WriteLine("Stop hook F11 key");
                F11_on.Text = "OFF";
                F11_on.BackColor = Color.Red;
                SaveSettings();
            }
        }
        
        //включение перехвата F12
        private void F12_on_Click(object sender, EventArgs e)
        {
            if (F12_on.BackColor == Color.Red)
            {
                if (url[11] == null || url[11] == "")
                {
                    F12_button_Click(sender, e);
                }
                else
                {
                    Console.WriteLine("Start hook F12 key");
                    F12_on.Text = "ON";
                    F12_on.BackColor = Color.Green;
                    SaveSettings();
                }
            }
            else
            {
                Console.WriteLine("Stop hook F12 key");
                F12_on.Text = "OFF";
                F12_on.BackColor = Color.Red;
                SaveSettings();
            }
        }
        
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!exit)
            {
                e.Cancel = true;
            }
            this.Visible = false;
            
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Visible = true;
        }
        private void SaveSettings()
        {
            using (StreamWriter sw = new StreamWriter("config.dat"))
            {
                for (byte i = 0; i < url.Length; i++)
                {
                    sw.WriteLine(url[i]);
                }
                sw.WriteLine(F1_on.Text);
                sw.WriteLine(F2_on.Text);
                sw.WriteLine(F3_on.Text);
                sw.WriteLine(F4_on.Text);
                sw.WriteLine(F5_on.Text);
                sw.WriteLine(F6_on.Text);
                sw.WriteLine(F7_on.Text);
                sw.WriteLine(F8_on.Text);
                sw.WriteLine(F9_on.Text);
                sw.WriteLine(F10_on.Text);
                sw.WriteLine(F11_on.Text);
                sw.WriteLine(F12_on.Text);
                sw.WriteLine(volume_on.Text);
                sw.WriteLine(gamemode);
                for (byte i = 0; i < app.Length; i++)
                {
                    sw.WriteLine(app[i]);
                }
                sw.Close();
            }
        }
        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            exit = true;
            this.Close();
        }

        //открытие файла F1
        private void F1_button_Click(object sender, EventArgs e)
        {
            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                url[0] = openDialog.FileName;
                Console.WriteLine("Start hook F1 key");
                F1_on.Text = "ON";
                F1_on.BackColor = Color.Green;
                RunProgramm(0);
                F1_BOX.Text = app[0];
                SaveSettings();
            }
        }

        //открытие файла F2
        private void F2_button_Click(object sender, EventArgs e)
        {
            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                url[1] = openDialog.FileName;
                Console.WriteLine("Start hook F2 key");
                F2_on.Text = "ON";
                F2_on.BackColor = Color.Green;
                RunProgramm(1);
                F2_BOX.Text = app[1];
                SaveSettings();
            }
        }

        //открытие файла F3
        private void F3_button_Click(object sender, EventArgs e)
        {
            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                url[2] = openDialog.FileName;
                Console.WriteLine("Start hook F3 key");
                F3_on.Text = "ON";
                F3_on.BackColor = Color.Green;
                RunProgramm(2);
                F3_BOX.Text = app[2];
                SaveSettings();
            }
        }

        //открытие файла F4
        private void F4_button_Click(object sender, EventArgs e)
        {
            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                url[3] = openDialog.FileName;
                Console.WriteLine("Start hook F4 key");
                F4_on.Text = "ON";
                F4_on.BackColor = Color.Green;
                RunProgramm(3);
                F4_BOX.Text = app[3];
                SaveSettings();
            }
        }

        //открытие файла F5
        private void F5_button_Click(object sender, EventArgs e)
        {
            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                url[4] = openDialog.FileName;
                Console.WriteLine("Start hook F5 key");
                F5_on.Text = "ON";
                F5_on.BackColor = Color.Green;
                RunProgramm(4);
                F5_BOX.Text = app[4];
                SaveSettings();
            }
        }

        //открытие файла F6
        private void F6_button_Click(object sender, EventArgs e)
        {
            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                url[5] = openDialog.FileName;
                Console.WriteLine("Start hook F6 key");
                F6_on.Text = "ON";
                F6_on.BackColor = Color.Green;
                RunProgramm(5);
                F6_BOX.Text = app[5];
                SaveSettings();
            }
        }

        //открытие файла F7
        private void F7_button_Click(object sender, EventArgs e)
        {
            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                url[6] = openDialog.FileName;
                Console.WriteLine("Start hook F7 key");
                F7_on.Text = "ON";
                F7_on.BackColor = Color.Green;
                RunProgramm(6);
                F7_BOX.Text = app[6];
                SaveSettings();
            }
        }

        //открытие файла F8
        private void F8_button_Click(object sender, EventArgs e)
        {
            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                url[7] = openDialog.FileName;
                Console.WriteLine("Start hook F8 key");
                F8_on.Text = "ON";
                F8_on.BackColor = Color.Green;
                RunProgramm(7);
                F8_BOX.Text = app[7];
                SaveSettings();
            }
        }

        //открытие файла F9
        private void F9_button_Click(object sender, EventArgs e)
        {
            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                url[8] = openDialog.FileName;
                Console.WriteLine("Start hook F9 key");
                F9_on.Text = "ON";
                F9_on.BackColor = Color.Green;
                RunProgramm(8);
                F9_BOX.Text = app[8];
                SaveSettings();
            }
        }

        //открытие файла F10
        private void F10_button_Click(object sender, EventArgs e)
        {
            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                url[9] = openDialog.FileName;
                Console.WriteLine("Start hook F10 key");
                F10_on.Text = "ON";
                F10_on.BackColor = Color.Green;
                RunProgramm(9);
                F10_BOX.Text = app[9];
                SaveSettings();
            }
        }

        //открытие файла F11
        private void F11_button_Click(object sender, EventArgs e)
        {
            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                url[10] = openDialog.FileName;
                Console.WriteLine("Start hook F11 key");
                F11_on.Text = "ON";
                F11_on.BackColor = Color.Green;
                RunProgramm(10);
                F11_BOX.Text = app[10];
                SaveSettings();
            }
        }

        //открытие файла F12
        private void F12_button_Click(object sender, EventArgs e)
        {
            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                url[11] = openDialog.FileName;
                Console.WriteLine("Start hook F12 key");
                F12_on.Text = "ON";
                F12_on.BackColor = Color.Green;
                RunProgramm(11);
                F12_BOX.Text = app[11];
                SaveSettings();
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            VolDown();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            VolUp();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Mute();
        }

        private void del_F1_Click(object sender, EventArgs e)
        {
            F1_BOX.Text = "";
            Console.WriteLine("Stop hook F1 key");
            F1_on.Text = "OFF";
            F1_on.BackColor = Color.Red;
            app[0] = "";
            url[0] = "";
            SaveSettings();

        }

        private void del_F2_Click(object sender, EventArgs e)
        {
            F2_BOX.Text = "";
            Console.WriteLine("Stop hook F2 key");
            F2_on.Text = "OFF";
            F2_on.BackColor = Color.Red;
            app[1] = "";
            url[1] = "";
            SaveSettings();
        }

        private void del_F3_Click(object sender, EventArgs e)
        {
            F3_BOX.Text = "";
            Console.WriteLine("Stop hook F3 key");
            F3_on.Text = "OFF";
            F3_on.BackColor = Color.Red;
            app[2] = "";
            url[2] = "";
            SaveSettings();
        }

        private void del_F4_Click(object sender, EventArgs e)
        {
            F4_BOX.Text = "";
            Console.WriteLine("Stop hook F4 key");
            F4_on.Text = "OFF";
            F4_on.BackColor = Color.Red;
            app[3] = "";
            url[3] = "";
            SaveSettings();
        }

        private void del_F5_Click(object sender, EventArgs e)
        {
            F5_BOX.Text = "";
            Console.WriteLine("Stop hook F5 key");
            F5_on.Text = "OFF";
            F5_on.BackColor = Color.Red;
            app[4] = "";
            url[4] = "";
            SaveSettings();
        }

        private void del_F6_Click(object sender, EventArgs e)
        {
            F6_BOX.Text = "";
            Console.WriteLine("Stop hook F6 key");
            F6_on.Text = "OFF";
            F6_on.BackColor = Color.Red;
            app[5] = "";
            url[5] = "";
            SaveSettings();
        }

        private void del_F7_Click(object sender, EventArgs e)
        {
            F7_BOX.Text = "";
            Console.WriteLine("Stop hook F7 key");
            F7_on.Text = "OFF";
            F7_on.BackColor = Color.Red;
            app[6] = "";
            url[6] = "";
            SaveSettings();
        }

        private void del_F8_Click(object sender, EventArgs e)
        { 
            F8_BOX.Text = "";
            Console.WriteLine("Stop hook F8 key");
            F8_on.Text = "OFF";
            F8_on.BackColor = Color.Red;
            app[7] = "";
            url[7] = "";
            SaveSettings();
        }

        private void del_F9_Click(object sender, EventArgs e)
        {
            F9_BOX.Text = "";
            Console.WriteLine("Stop hook F9 key");
            F9_on.Text = "OFF";
            F9_on.BackColor = Color.Red;
            app[8] = "";
            url[8] = "";
            SaveSettings();
        }

        private void del_F10_Click(object sender, EventArgs e)
        {
            F10_BOX.Text = "";
            Console.WriteLine("Stop hook F10 key");
            F10_on.Text = "OFF";
            F10_on.BackColor = Color.Red;
            app[9] = "";
            url[9] = "";
            SaveSettings();
        }

        private void del_F11_Click(object sender, EventArgs e)
        {
            F11_BOX.Text = "";
            Console.WriteLine("Stop hook F11 key");
            F11_on.Text = "OFF";
            F11_on.BackColor = Color.Red;
            app[10] = "";
            url[10] = "";
            SaveSettings();
        }

        private void del_F12_Click(object sender, EventArgs e)
        {
            F12_BOX.Text = "";
            Console.WriteLine("Stop hook F12 key");
            F12_on.Text = "OFF";
            F12_on.BackColor = Color.Red;
            app[11] = "";
            url[11] = "";
            SaveSettings();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            Close();
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        //Таймер timer Hide
        private void timer_hide_cursor_Tick(object sender, EventArgs e)
        {
            Console.WriteLine("Tick");
            ShowCursor(false);
            //настройка перехвата
            m_callback_mouse = LowLevelMouseProc_move;//вызов функции перехвата колёсико мыши
            m_hHook_mouse = SetWindowsHookEx(WH_MOUSE_LL, m_callback_mouse, GetModuleHandle(IntPtr.Zero), 0);
            timer_hide_cursor.Stop();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            UnhookWindowsHookEx(m_hHook[0]);
        }
    }
}