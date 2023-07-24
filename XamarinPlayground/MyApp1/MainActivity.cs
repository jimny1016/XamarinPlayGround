using System.Linq;
using Android.App;
using Android.Hardware.Usb;
using Android.OS;
using Android.Runtime;
using Android.Widget;
using AndroidX.AppCompat.App;
using Java.Lang.Reflect;
using Java.Lang;
using Newtonsoft.Json;
using Java.IO;
using Serial;
using FileCatch;
using System.Collections.Generic;
using System;

namespace MyApp1
{
    [Activity(Label = "", Theme = "@style/Theme.AppCompat.Light.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private SerialPort.SerialPortWrapper.SerialPort _libSerialPort;
        private bool _isWhite = true;
        private RelativeLayout _layOut;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            RequestWindowFeature(Android.Views.WindowFeatures.NoTitle);
            SetContentView(Resource.Layout.activity_main);

            //var switchToRndisBtn = FindViewById<Button>(Resource.Id.SwitchToRndis);
            //switchToRndisBtn.Click += SwitchToRndis_Click;

            //var switchToAndroidControllModeBtn = FindViewById<Button>(Resource.Id.SwitchToAndroidControllMode);
            //switchToAndroidControllModeBtn.Click += SwitchToAndroidControllMode_Click;

            //var initSerialPortBtn = FindViewById<Button>(Resource.Id.InitSerialPort);
            //initSerialPortBtn.Click += InitSerialPort_Click;

            var isToggleUSBTransferMode = ToggleUSBTransferMode();
            if (!isToggleUSBTransferMode)
            {
                ShowAlertDialog($"切換網路共享模式失敗。");
            }

            _layOut = FindViewById<RelativeLayout>(Resource.Id.fullScreenLayout);
            //var toggleButton = FindViewById<Button>(Resource.Id.toggleButton);
            //toggleButton.Click += (sender, e) => {
            //    SwitchBackGroundColor();
            //};
            var cnSock = new ConnectSocket(SwitchToControlMode, InitSerialPort, SwitchBackGroundColor, SendLEDCommand, CreateColorRGBArray);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
        
        private void ShowAlertDialog(string message)
        {
            // 建立 AlertDialog.Builder
            var builder = new AndroidX.AppCompat.App.AlertDialog.Builder(this);
            builder.SetTitle("提示")
                   .SetMessage(message)
                   .SetPositiveButton("確定", (sender, args) => {
                       // 確定按鈕的回調處理
                       // 可以在這裡執行相應的操作
                   });

            // 建立 AlertDialog
            var alertDialog = builder.Create();

            // 顯示對話框
            alertDialog.Show();
        }

        //private void SwitchToRndis_Click(object sender, EventArgs e)
        //{
        //    ShowAlertDialog($"ToggleUSBTransferMode:{ToggleUSBTransferMode()}");
        //}

        //private void SwitchToAndroidControllMode_Click(object sender, EventArgs e)
        //{
        //    ShowAlertDialog($"SwitchToControlMode:{SwitchToControlMode("1")}");
        //}

        //private void InitSerialPort_Click(object sender, EventArgs e)
        //{
        //    ShowAlertDialog($"InitSerialPort_Click:{InitSerialPort()}");
        //}

        public bool ToggleUSBTransferMode()
        {
            try
            {
                // 取得 UsbManager 類別
                var usbManagerClass = Class.ForName("android.hardware.usb.UsbManager"); 
                var methods = usbManagerClass.GetMethods();
                var setUsbFunctionMethod = methods.Where(m => m.Name == "setCurrentFunction").FirstOrDefault();

                // 呼叫 setUsbFunction 方法開啟 USB 網路共享
                var usbManager = (UsbManager)GetSystemService(UsbService);
                setUsbFunctionMethod.Invoke(usbManager, "rndis", true);
                return true;
            }
            catch (ClassNotFoundException e)
            {
                ShowAlertDialog(JsonConvert.SerializeObject(e));
                e.PrintStackTrace();
            }
            catch (NoSuchMethodException e)
            {
                ShowAlertDialog(JsonConvert.SerializeObject(e));
                e.PrintStackTrace();
            }
            catch (IllegalAccessException e)
            {
                ShowAlertDialog(JsonConvert.SerializeObject(e));
                e.PrintStackTrace();
            }
            catch (InvocationTargetException e)
            {
                ShowAlertDialog(JsonConvert.SerializeObject(e));
                e.PrintStackTrace();
            }
            return false;
        }

        public bool SwitchToControlMode(string value) 
        {
            try
            {
                BufferedWriter fops_2 = new BufferedWriter(new FileWriter("/sys/class/vd_gpio/cust_enjie/vd_enjie_ext_power"));//open node
                fops_2.Write(value);
                fops_2.Close();

                return true;
            } 
            catch (Java.Lang.Exception ex)
            {
                ShowAlertDialog(JsonConvert.SerializeObject(ex));
                ex.PrintStackTrace();
            }
            return false;
        }

        public bool InitSerialPort()
        {
            try
            {
                _libSerialPort = new SerialPort.SerialPortWrapper.SerialPort(
                   "dev/ttyS2",
                   460800,
                   Stopbits.One,
                   Parity.None,
                   ByteSize.EightBits,
                   FlowControl.Software,
                   new Timeout(50, 50, 50, 50, 50));

                return true;
            }
            catch (Java.Lang.Exception ex)
            {
                ShowAlertDialog(JsonConvert.SerializeObject(ex));
                ex.PrintStackTrace();
            }
            return false;
        }

        public void SwitchBackGroundColor()
        {
            RunOnUiThread(() => {
                if (_isWhite)
                {
                    _layOut.SetBackgroundColor(Android.Graphics.Color.Black);
                }
                else
                {
                    _layOut.SetBackgroundColor(Android.Graphics.Color.White);
                }
                _isWhite = !_isWhite;
            });
        }
        private static readonly List<Tuple<int, int>> COMMAND_MAPPING = new List<Tuple<int, int>>()
        {
            Tuple.Create(0, 0),
            Tuple.Create(1, 0),
            Tuple.Create(2, 0),
            Tuple.Create(3, 0),
            Tuple.Create(4, 0),
            Tuple.Create(5, 0),
            Tuple.Create(6, 0),
            Tuple.Create(7, 0),
            Tuple.Create(8, 0),
            Tuple.Create(8, 1),
            Tuple.Create(7, 1),
            Tuple.Create(6, 1),
            Tuple.Create(5, 1),
            Tuple.Create(4, 1),
            Tuple.Create(3, 1),
            Tuple.Create(2, 1),
            Tuple.Create(1, 1),
            Tuple.Create(0, 1),
            Tuple.Create(3, 2),
            Tuple.Create(4, 2),
            Tuple.Create(5, 2),
            Tuple.Create(6, 2),
            Tuple.Create(7, 2),
            Tuple.Create(8, 2),
            Tuple.Create(8, 3),
            Tuple.Create(7, 3),
            Tuple.Create(6, 3),
            Tuple.Create(5, 3),
            Tuple.Create(4, 3),
            Tuple.Create(3, 3),
            Tuple.Create(2, 3),
            Tuple.Create(1, 3),
            Tuple.Create(0, 3),
            Tuple.Create(0, 4),
            Tuple.Create(1, 4),
            Tuple.Create(2, 4),
            Tuple.Create(3, 4),
            Tuple.Create(4, 4),
            Tuple.Create(5, 4),
            Tuple.Create(6, 4),
            Tuple.Create(7, 4),
            Tuple.Create(8, 4),
        };
        public void SendLEDCommand(ColorRGB[,] ColorArray)
        {
            //先根據 ColorArray 做出數據內容
            List<byte> listCommand = new List<byte>
            {
                0xA5,
                0xA7
            };
            for (int i = 0; i < COMMAND_MAPPING.Count; i++)
            {
                ColorRGB color = ColorArray[COMMAND_MAPPING[i].Item1, COMMAND_MAPPING[i].Item2];
                listCommand.Add(color.G);
                listCommand.Add(color.R);
                listCommand.Add(color.B);
            }

            //算入數據長度 目前的 source.Count() + 4 沒有大於 0xFF 可以直接轉換
            var commandCount = Convert.ToByte(listCommand.Count() + 4);

            //使用數據內容算出教驗和
            var commandCacul = CommandCaculddd(listCommand, commandCount);

            //加入教驗和
            listCommand.Add(commandCacul);

            //插入數據長度
            listCommand.Insert(0, commandCount);

            //轉譯
            var result = AdjustBytes(listCommand);

            //插入禎頭
            result.Insert(0, 0x7E);

            //加入結尾
            result.Add(0x7E);
            _libSerialPort.Write(result.ToArray(), result.Count);
        }
        private byte CommandCaculddd(List<byte> source, byte commandCount)
        {
            byte result = 0x00;
            foreach (byte singlebyte in source)
            {
                result += singlebyte;
            }
            result += commandCount;
            return result;
        }
        private List<byte> AdjustBytes(List<byte> source)
        {
            List<byte> result = new List<byte>();

            foreach (byte value in source)
            {
                if (value == 0x7E)
                {
                    result.Add(0x7D);
                    result.Add(0x02);
                    continue;
                }

                if (value == 0x7D)
                {
                    result.Add(0x7D);
                    result.Add(0x01);
                    continue;
                }

                result.Add(value);
            }
            return result;
        }
        public ColorRGB[,] CreateColorRGBArray(ColorRGB color)
        {
            ColorRGB[,] result = new ColorRGB[9, 5];

            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    result[i, j] = color;
                }
            }

            return result;
        }
        //private const int SEND_BUFFER = 132;
        //public void SendColor(ColorRGB[,] ColorArray)
        //{
        //    byte[] sendcode = new byte[SEND_BUFFER];
        //    List<byte> listCommand = new List<byte>();
        //    sendcode[0] = 0x7E;
        //    sendcode[1] = 0x84;
        //    sendcode[2] = 0xA5;
        //    sendcode[3] = 0xA7;
        //    listCommand.Add(0x7E);
        //    listCommand.Add(0x84);
        //    listCommand.Add(0xA5);
        //    listCommand.Add(0xA7);
        //    for (int i = 0; i < COMMAND_MAPPING.Count; i++)
        //    {
        //        ColorRGB color = ColorArray[COMMAND_MAPPING[i].Item1, COMMAND_MAPPING[i].Item2];
        //        sendcode[4 + i * 3] = color.G;
        //        sendcode[5 + i * 3] = color.R;
        //        sendcode[6 + i * 3] = color.B;
        //        if (color.G != 0x7E && color.G != 0x7D)
        //        {
        //            listCommand.Add(color.G);
        //        }
        //        else if (color.G == 0x7E)
        //        {
        //            listCommand.Add(0x7D);
        //            listCommand.Add(0x02);
        //        }
        //        else if (color.G == 0x7D)
        //        {
        //            listCommand.Add(0x7D);
        //            listCommand.Add(0x01);
        //        }
        //        if (color.R != 0x7E && color.R != 0x7D)
        //        {
        //            listCommand.Add(color.R);
        //        }
        //        else if (color.R == 0x7E)
        //        {
        //            listCommand.Add(0x7D);
        //            listCommand.Add(0x02);
        //        }
        //        else if (color.R == 0x7D)
        //        {
        //            listCommand.Add(0x7D);
        //            listCommand.Add(0x01);
        //        }
        //        if (color.B != 0x7E && color.B != 0x7D)
        //        {
        //            listCommand.Add(color.B);
        //        }
        //        else if (color.B == 0x7E)
        //        {
        //            listCommand.Add(0x7D);
        //            listCommand.Add(0x02);
        //        }
        //        else if (color.B == 0x7D)
        //        {
        //            listCommand.Add(0x7D);
        //            listCommand.Add(0x01);
        //        }
        //    }
        //    byte checkSum = CommandCacul(sendcode[1..130]);
        //    if (checkSum == 0x7D)
        //    {
        //        listCommand.Add(0x7D);
        //        listCommand.Add(0x01);
        //    }
        //    else if (checkSum == 0x7E)
        //    {
        //        listCommand.Add(0x7E);
        //        listCommand.Add(0x01);
        //    }
        //    else
        //    {
        //        listCommand.Add(checkSum);
        //    }
        //    listCommand.Add(0x7E);
        //    _libSerialPort.Write(listCommand.ToArray(), listCommand.Count);
        //}
        //private byte CommandCacul(byte[] array)
        //{
        //    byte result = 0x00;
        //    foreach (byte singlebyte in array)
        //    {
        //        result += singlebyte;
        //    }
        //    if (result == 0x7E)
        //    {

        //    }
        //    if (result == 0x7D)
        //    {

        //    }
        //    return result;
        //}
    }

    public class ColorRGB
    {
        public byte R {get; set;}
        public byte G { get; set; }
        public byte B { get; set; }
        public ColorRGB(byte R, byte G, byte B)
        {
            this.R = R;
            this.G = G;
            this.B = B;
        }
    }
}