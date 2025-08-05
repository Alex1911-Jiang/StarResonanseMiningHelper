using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;

namespace StarResonanseMiningHelper
{
    public partial class MainWindow : Window
    {
        private CancellationTokenSource _cts = new CancellationTokenSource();
        private nint hwnd_STAR_RESONANSE = nint.Zero;
        private bool _sendMouseWheel = false;
        private int _delaySecond = 40;

        #region -- 结构定义 --
        [StructLayout(LayoutKind.Sequential)]
        struct INPUT
        {
            public uint type;
            public InputUnion u;
        }

        [StructLayout(LayoutKind.Explicit)]
        struct InputUnion
        {
            [FieldOffset(0)]
            public MOUSEINPUT mi;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public nint dwExtraInfo;
        }

        #endregion -- 结构定义 --

        #region -- 查找窗口 --
        [DllImport("user32.dll", EntryPoint = "FindWindow", CharSet = CharSet.Auto)]
        private static extern nint FindWindow(string lpClassName, string lpWindowName);
        #endregion -- 查找窗口 --

        #region -- 发送按键/鼠标 --

        private const int VK_A = 0x41;
        private const uint WM_KEYDOWN = 0x0100;
        private const uint WM_KEYUP = 0x0101;
        private const uint WM_CHAR = 0x0102;
        private const uint MOUSEEVENTF_WHEEL = 0x0800;
        private const int INPUT_MOUSE = 0;
        private const int WHEEL_DELTA = 120;

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern nint SendMessage(nint hWnd, uint msg, nint wParam, nint lParam);

        [DllImport("user32.dll")]
        private static extern uint MapVirtualKey(uint uCode, uint uMapType);

        #endregion -- 发送按键/鼠标 --

        #region -- 焦点 --

        private const int SW_RESTORE = 9;

        [DllImport("user32.dll")]
        private static extern nint GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(nint hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(nint hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool IsIconic(nint hWnd);

        public static void FocusWindow(nint windowHandle)
        {
            if (IsIconic(windowHandle))
            {
                ShowWindow(windowHandle, SW_RESTORE);
                SetForegroundWindow(windowHandle);
            }
            SetForegroundWindow(windowHandle);
        }
        #endregion -- 焦点 --

        #region -- 鼠标位置 --

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetCursorPos(out Point lpPoint);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetCursorPos(int x, int y);

        #endregion -- 鼠标位置 --

        public MainWindow()
        {
            InitializeComponent();
            _cts.Cancel();

            SendMouseWheelChk.IsCheckedChanged += (s, e) => _sendMouseWheel = SendMouseWheelChk.IsChecked ?? false;
            ClearLogBtn.Click += (s, e) => LogTxb.Clear();
        }

        private void DelaySecondTxb_TextChanged(object? sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(DelaySecondTxb.Text))
            {
                _delaySecond = 40;
                return;
            }

            if (!Regex.IsMatch(DelaySecondTxb.Text, @"^\d*$"))
            {
                var validText = Regex.Replace(DelaySecondTxb.Text, @"[^\d]", "");
                DelaySecondTxb.Text = validText;
            }

            if (!int.TryParse(DelaySecondTxb.Text, out int delaySecond))
            {
                _delaySecond = 40;
                return;
            }
            _delaySecond = delaySecond;
        }

        private void AppendLog(string log)
        {
            if (!LogTxb.CheckAccess())
            {
                Dispatcher.UIThread.InvokeAsync(() => AppendLog(log));
                return;
            }
            LogTxb.Text += ($"{DateTime.Now:HH:mm:ss.fff}    {log}{Environment.NewLine}");
        }

        private void WorkBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (!_cts.IsCancellationRequested)
            {
                _cts.Cancel();
                WorkBtnTxb.Text = "启动";
                StatusLbl.Text = "状态：已停止";
                return;
            }
            _cts = new CancellationTokenSource();
            WorkBtnTxb.Text = "停止";
            StatusLbl.Text = "状态：运行中";
            Task.Factory.StartNew(Work);
        }

        private void Work()
        {
            try
            {
                while (!_cts.IsCancellationRequested && hwnd_STAR_RESONANSE == nint.Zero)
                {
                    hwnd_STAR_RESONANSE = FindWindow("UnityWndClass", "星痕共鸣");
                    if (hwnd_STAR_RESONANSE == nint.Zero)
                    {
                        AppendLog("没有找到星痕共鸣游戏窗口");
                        Task.Delay(500, _cts.Token).GetAwaiter().GetResult();
                    }
                    else
                    {
                        AppendLog("已找到星痕共鸣游戏窗口");
                        break;
                    }
                }

                char keyF = 'F';
                int virtualKey = keyF - 'A' + VK_A;

                uint scanCode = MapVirtualKey((uint)virtualKey, 0);

                while (!_cts.IsCancellationRequested)
                {
                    Point originalMousePoint = Point.Empty;
                    nint currentWindow = GetForegroundWindow();
                    if (currentWindow != hwnd_STAR_RESONANSE)
                    {
                        AppendLog("将焦点设为星痕共鸣窗口");
                        GetCursorPos(out originalMousePoint);
                        FocusWindow(hwnd_STAR_RESONANSE);
                    }

                    if (_sendMouseWheel)
                    {
                        AppendLog("发送滚轮下");
                        int delta = -WHEEL_DELTA;
                        INPUT[] inputs = new INPUT[1];
                        inputs[0].type = INPUT_MOUSE;
                        inputs[0].u.mi = new MOUSEINPUT
                        {
                            dx = 0,
                            dy = 0,
                            mouseData = (uint)delta,
                            dwFlags = MOUSEEVENTF_WHEEL,
                            time = 0,
                            dwExtraInfo = nint.Zero
                        };
                        Task.Delay(20, _cts.Token).GetAwaiter().GetResult();
                        SendInput(1, inputs, Marshal.SizeOf(typeof(INPUT)));
                        Task.Delay(20, _cts.Token).GetAwaiter().GetResult();
                    }

                    AppendLog("发送F按键");
                    nint lParamDown = (nint)(1 | (scanCode << 16));
                    SendMessage(hwnd_STAR_RESONANSE, WM_KEYDOWN, virtualKey, lParamDown);
                    SendMessage(hwnd_STAR_RESONANSE, WM_CHAR, keyF, lParamDown);
                    nint lParamUp = (nint)(1 | (scanCode << 16) | (1 << 30) | (1 << 31));
                    SendMessage(hwnd_STAR_RESONANSE, WM_KEYUP, virtualKey, lParamUp);

                    if (currentWindow != hwnd_STAR_RESONANSE)
                    {
                        AppendLog("归还焦点");
                        FocusWindow(currentWindow);
                    }

                    if (originalMousePoint != Point.Empty)
                    {
                        AppendLog("归还鼠标");
                        SetCursorPos(originalMousePoint.X, originalMousePoint.Y);
                    }

                    Task.Delay(_delaySecond * 1000, _cts.Token).GetAwaiter().GetResult();
                }
            }
            catch (TaskCanceledException)
            {
            }
        }
    }
}