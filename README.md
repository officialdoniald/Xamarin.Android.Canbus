# Xamarin.Android.Canbus

Usage:

Create Canbus:

Canbus _canbus = _canbus = new Canbus(250000);
_canbus.PackageReceive += Canbus_PackageReceive;
_canbus.CanbusStatusChanged += Canbus_CanbusStatusChanged;
_canbus.CanbusExceptionOccour += Canbus_CanbusExceptionOccour;

Read from the Canbus:

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

Sending to the Canbus:

_canbus.Send(0x18FF12, true, new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 });

More information and Example: Xamarin.Android.Canbus.PoC Project.
