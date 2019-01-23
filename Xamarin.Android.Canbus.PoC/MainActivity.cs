using Android.App;
using Android.OS;
using Android.Widget;
using NLog.Config;
using Xamarin.Android.Canbus.Wrapper;

namespace AOBUCan
{
    [Activity(Label = "AOBUCan", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        #region Properties

        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private Canbus _canbus;

        private Button _btnSend;
        private TextView _label;
        private TextView _status;
        private TextView _error;

        #endregion

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Main);

            var AppPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);

            NLog.LogManager.Configuration = new XmlLoggingConfiguration(AppPath + "/NLog.config");
            NLog.LogManager.Configuration.Variables.Remove("basedir");
            NLog.LogManager.Configuration.Variables.Add("basedir", AppPath);

            _logger.Info("NLog configuration loaded.");

            _btnSend = FindViewById<Button>(Resource.Id.btnSend);
            _btnSend.Click += Btn_Click;

            _label = FindViewById<TextView>(Resource.Id.label);
            _status = FindViewById<TextView>(Resource.Id.status);
            _error = FindViewById<TextView>(Resource.Id.error);

            _canbus = new Canbus(250000);
            _canbus.PackageReceive += Canbus_PackageReceive;
            _canbus.CanbusStatusChanged += Canbus_CanbusStatusChanged;
            _canbus.CanbusExceptionOccour += Canbus_CanbusExceptionOccour; ;

            _logger.Info("Canbus created.");
            _logger.Info("Canbus Canbus_PackageReceive started.");
        }

        private void Canbus_CanbusExceptionOccour(object sender, System.Exception e)
        {
            RunOnUiThread(() => {
                _error.Text = "Error: " + e.StackTrace;
                _logger.Error(e.StackTrace);
            });
        }

        private void Canbus_CanbusStatusChanged(object sender, CanbusStatus e)
        {
            RunOnUiThread(()=> {
                _status.Text = "Status: " + e.ToString();

                if (e == CanbusStatus.Package_received)
                {
                    _error.Text = "Error: -";
                }
            });
        }

        private void Canbus_PackageReceive(object sender, CanbusEventArgs e)
        {
            RunOnUiThread(() =>
            {
                string dataString = "";
                for (int x = 0; x < e.Data.Count; x++)
                    dataString += e.Data[x].ToString("X2") + " ";

                var text = "ID: " + e.ID.ToString("X2") + ", Ext: " + e.Extended.ToString() + ", Data: " + dataString.Substring(0, dataString.Length - 1);

                _logger.Info(text);
                _label.Text = text;
            });
        }

        private void Btn_Click(object sender, System.EventArgs e)
        {
            _canbus.Send(0x18FF12, true, new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 });
        }

        protected override void OnDestroy()
        {
            _canbus.Close();

            base.OnDestroy();
        }
    }
}

