using System;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Hardware.Usb;
using Android.OS;
using Android.Runtime;
using Android.Widget;
using AndroidX.AppCompat.App;
using Java.Lang.Reflect;
using Java.Lang;
using Newtonsoft.Json;
using Java.IO;
using System.Reflection.Emit;
using System.Threading;
using Serial;
using FileCatch;

namespace MyApp1
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private SerialPort.SerialPortWrapper.SerialPort _libSerialPort;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            var switchToRndisBtn = FindViewById<Button>(Resource.Id.SwitchToRndis);
            switchToRndisBtn.Click += SwitchToRndis_Click;

            var switchToAndroidControllModeBtn = FindViewById<Button>(Resource.Id.SwitchToAndroidControllMode);
            switchToAndroidControllModeBtn.Click += SwitchToAndroidControllMode_Click;

            var initSerialPortBtn = FindViewById<Button>(Resource.Id.InitSerialPort);
            initSerialPortBtn.Click += InitSerialPort_Click;

            var isToggleUSBTransferMode = ToggleUSBTransferMode();
            if (!isToggleUSBTransferMode)
            {
                ShowAlertDialog($"切換網路共享模式失敗。");
            }
            var cnSock = new ConnectSocket(SwitchToControlMode, InitSerialPort);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
        void RunBitmapOnUIThread()
        {
            ShowAlertDialog($"RunBitmapOnUIThread!");
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

        private void SwitchToRndis_Click(object sender, EventArgs e)
        {
            ShowAlertDialog($"ToggleUSBTransferMode:{ToggleUSBTransferMode()}");
        }

        private void SwitchToAndroidControllMode_Click(object sender, EventArgs e)
        {
            ShowAlertDialog($"SwitchToControlMode:{SwitchToControlMode("1")}");
        }

        private void InitSerialPort_Click(object sender, EventArgs e)
        {
            ShowAlertDialog($"InitSerialPort_Click:{InitSerialPort()}");
        }

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
                   Serial.FlowControl.Software,
                   new Serial.Timeout(50, 50, 50, 50, 50));

                return true;
            }
            catch (Java.Lang.Exception ex)
            {
                ShowAlertDialog(JsonConvert.SerializeObject(ex));
                ex.PrintStackTrace();
            }
            return false;
        }

        
    }
}