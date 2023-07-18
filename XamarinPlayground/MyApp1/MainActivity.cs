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

namespace MyApp1
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        Button btn;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            btn = FindViewById<Button>(Resource.Id.big);
            btn.Click += Button_Click;
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


        private void Button_Click(object sender, System.EventArgs e)
        {
            btn.Text = "Clicked";

            var result = ToggleUSBTransferMode();
            ShowAlertDialog(result.ToString());
            
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
    }
}