using Com.Canbus.Helper;
using System;
using System.Collections.Generic;

namespace Xamarin.Android.Canbus.Wrapper
{
    /// <summary>
    /// Canbus C# wrapper from JAVA.
    /// </summary>
    public class Canbus
    {
        #region Properties

        public event EventHandler<CanbusEventArgs> PackageReceive;
        public event EventHandler<CanbusStatus> CanbusStatusChanged;
        public event EventHandler<Exception> CanbusExceptionOccour;

        private CanbusJavaWrapper _can;

        private System.Threading.Thread _packageReceiveThread;

        private bool _isRunningpackageReceiveThread = true;
        
        #endregion

        public Canbus(int baudRate)
        {
            _can = new CanbusJavaWrapper(baudRate);

            OnCanbusStatusChanged(CanbusStatus.Initialized);

            _packageReceiveThread = new System.Threading.Thread(() =>
            {
                while (_isRunningpackageReceiveThread)
                {
                    try
                    {
                        CanPacket p = _can.Read();

                        OnPackageReceive(new CanbusEventArgs()
                        {
                            ID = p.Id,
                            Data = new List<byte>(p.Data),
                            Extended = p.Extended
                        });

                        OnCanbusStatusChanged(CanbusStatus.Package_received);
                    }
                    catch (Exception e)
                    {
                        OnCanbusStatusChanged(CanbusStatus.Error);
                        OnCanbusExceptionOccour(e);
                    }
                }
            })
            {
                IsBackground = true
            };
            _packageReceiveThread.Start();
        }

        /// <summary>
        /// Send the package to the Canbus.
        /// </summary>
        /// <param name="id">Package ID</param>
        /// <param name="extended">Extended or Standard.</param>
        /// <param name="data">Package Data</param>
        public void Send(int id, bool extended, byte[] data)
        {
            _can.Send(new CanPacket(id, extended, data));

            OnCanbusStatusChanged(CanbusStatus.Package_sent);
        }

        /// <summary>
        /// Get package while reading.
        /// </summary>
        /// <param name="state"></param>
        public void OnPackageReceive(CanbusEventArgs state)
        {
            PackageReceive?.Invoke(null, state);
        }

        /// <summary>
        /// Status changed.
        /// </summary>
        /// <param name="state"></param>
        public void OnCanbusStatusChanged(CanbusStatus status)
        {
            CanbusStatusChanged?.Invoke(null, status);
        }

        /// <summary>
        /// If any error occur.
        /// </summary>
        /// <param name="state"></param>
        public void OnCanbusExceptionOccour(Exception err)
        {
            CanbusExceptionOccour?.Invoke(null, err);
        }

        /// <summary>
        /// Stop the Canbus Read Thread.
        /// </summary>
        public void StopReceiveData()
        {
            _isRunningpackageReceiveThread = false;
            _packageReceiveThread.Abort();

            OnCanbusStatusChanged(CanbusStatus.OnPackageReceive_Stopped);
        }

        /// <summary>
        /// Start the Canbus Read Thread.
        /// </summary>
        public void StartReceiveData()
        {
            _isRunningpackageReceiveThread = true;

            if (!_packageReceiveThread.IsAlive)
            {
                _packageReceiveThread.IsBackground = true;
                _packageReceiveThread.Start();

                OnCanbusStatusChanged(CanbusStatus.OnPackageReceive_Started);
            }
        }

        /// <summary>
        /// Close the Canbus.
        /// </summary>
        public void Close()
        {
            _can.Close();

            OnCanbusStatusChanged(CanbusStatus.Closed);
        }

        /// <summary>
        /// Dispose the Canbus.
        /// </summary>
        public void Dispose()
        {
            _can.Dispose();

            OnCanbusStatusChanged(CanbusStatus.Disposed);
        }
    }
    
    /// <summary>
    /// Canbus package.
    /// </summary>
    public class CanbusEventArgs : EventArgs
    {
        /// <summary>
        /// Package ID.
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Extended or Standard.
        /// </summary>
        public bool Extended { get; set; }

        /// <summary>
        /// Package Data.
        /// </summary>
        public List<byte> Data { get; set; }
    }

    /// <summary>
    /// Canbus status.
    /// </summary>
    public enum CanbusStatus
    {
        Initialized,
        Package_received,
        Package_sent,
        OnPackageReceive_Stopped,
        OnPackageReceive_Started,
        Disposed,
        Closed,
        Error
    }
}