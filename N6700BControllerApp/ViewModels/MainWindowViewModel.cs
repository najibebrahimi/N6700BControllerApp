using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Diagnostics.Metrics;
using Tmds.DBus.Protocol;
using NEP.Instrument.N6700B;
using System;
using System.Runtime.InteropServices;
using System.IO;
using static System.Net.Mime.MediaTypeNames;
using Avalonia.Logging;

namespace N6700BControllerApp.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private const double MinVoltage = 0.0;
        private const double MaxVoltage = 20.0;
        private const double MinCurrent = 0.0002; 

        private const double MaxCurrent = 3.0;

        

        [ObservableProperty]
        private string ipAddress = "";

        public bool IsDisconnected => !IsConnected;

        [ObservableProperty]
        private double voltage = 3.0;

        [ObservableProperty]
        private string validationMessage = "";


        //[ObservableProperty]
        //private double current = 0.5;

        private double current;
        public double Current
        {
            get => current;
            set
            {

                if (value < 0)
                    current = 0;
                else if (value > 100)
                    current = 100;
                else
                    current = value;

                OnPropertyChanged(nameof(Current));
            }
        }


        [ObservableProperty]
        private bool isOutputOn = false;

        [ObservableProperty]
        private double measuredVoltage;
        [ObservableProperty]
        private double measuredCurrent;
        [ObservableProperty]
        private double measuredPower;

        public MainWindowViewModel()
        {
            IpAddress = "10.1.2.150";
            //demo testsekvens
            //instrument = new instrument();
            //instrument.Initialize() //starta upp resurser
            //instrument.Connect("10.1.2.23");
            //instrument.SetVoltage(100);
        }

        [RelayCommand]
        public void ReadLiveMeasurements()
        {
            if (instrument == null)
            {
                this.AddLog("Not connected to power supply.");
               
                return;
            }

            try
            {
                ERROR error = new();

                
                MeasuredVoltage = instrument.MeasureVoltage(1, out error);
                MeasuredCurrent = instrument.MeasureCurrent(1, out error);

               
                MeasuredPower = MeasuredVoltage * MeasuredCurrent;

                
                this.AddLog($"Live Read: {MeasuredVoltage} V, {MeasuredCurrent} A, {MeasuredPower} W");
            }
            catch (Exception ex)
            {
                Logs.Add($"Measurement error: {ex.Message}");
            }

        }

        //[ObservableProperty]
        //private const double MinVoltage = 0.0


        [ObservableProperty]
        private string outputStatus = "OFF";
        public ObservableCollection<string> Logs { get; } = new();

        [RelayCommand]
        public void SaveLog()
        {
            string filePath = "Log.txt";
            File.WriteAllLines(filePath, Logs);
            Logs.Add($"Log saved to {filePath}");

        }

        public void AddLog(string message)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            Logs.Add($"[{timestamp}] {message}");

            

            //Application.Current.Dispatcher.Invoke(() =>
            //{
            //    if (Application.Current.MainWindow is MainWindow window)
            //    {
            //        window.LogTextBox.ScrollToEnd();
            //    }
            //});

        }

        //ConnectionStatusColor = IsOutputOn? "Green" : "Red"; 

        //public string SetVoltageButtonText => $"Set Voltage ({Voltage}V)";
        //[ObservableProperty]
        //private string _connectionStatus = "Disconnected";

        private N6700B? instrument { get; set; } 


        [ObservableProperty]
        private bool isConnected = false;



        public string ConnectionStatus => IsConnected ? " Connected" : " Disconnected";
        public IBrush ConnectionStatusColor => IsConnected ? Brushes.Green : Brushes.Red;

        public string ConnectionButtonText => IsConnected ? "Disconnect" : "Connect";

        //public IBrush OutputStatusColor => IsOutputOn ? Brushes.Green : Brushes.Red;

        [RelayCommand]
        public void ToggleConnection()
        {
            //1. Verifiera att IpAddress är en korrekt address
            //2. Instansiera ett nytt objekt av N6700B (instrument = new n6700b(ipadderss)
            //3. använd try catch på 2.
            //4. Fånga eventuella fel och hantera dessa, om anslutningen lyckas ska IsConnected tilldelas och bli true
            //5. Om en anslutning redan är aktiv så ska .Close anropas

            //ERROR error;

            if (string.IsNullOrWhiteSpace(IpAddress))
            {
                Logs.Add("IP Adress saknas. Ange en giltig adress");
                return;
            }
            //this.ReadVoltage = instrument.ReadVoltage(0, out error);
            if (IsConnected && instrument != null)
            {
                try
                {
                    instrument.Close();
                    Logs.Add("Disconnected successfully");
                }
                catch (Exception ex)
                {
                    Logs.Add($"Disconnect Error:{ex.Message}");
                }
                finally
                {
                    IsConnected = false;
                    instrument = null;

                    OnPropertyChanged(nameof(ConnectionStatus));
                    OnPropertyChanged(nameof(ConnectionButtonText));
                    OnPropertyChanged(nameof(ConnectionStatusColor));
                }

                return;
            }

            try
            {
                string connectionString = $@"TCPIP0::{IpAddress}::5025::SOCKET";
                instrument = new N6700B(connectionString);
                ERROR error = new();

                instrument.Initialize(out error);

                if (error.errorCode != 0)
                {
                    //Logs.Add($"Fel vid anslutnig: {error.errorMsg} (kod {error.errorCode})");
                    Logs.Add(GetFriendlyErrorMessage(error));
                    return;
                }
                IsConnected = true;
                Logs.Add($"Ansluten till N6700B via {IpAddress}");
            }
            catch(Exception ex)
            {
                IsConnected = false;

                ValidationMessage = $"❌ Exception: {ex.Message}";
                AddLog(ValidationMessage);
                instrument = null;
            }

            OnPropertyChanged(nameof(ConnectionStatus));
            OnPropertyChanged(nameof(ConnectionButtonText));
            OnPropertyChanged(nameof(ConnectionStatusColor));
            OnPropertyChanged(nameof(IsDisconnected));




            //else
            //{
            //    if (instrument == null)
            //    {
            //        AddLog("Instrument object is null after instansiering");
            //        //string timestamp = DateTime.Now.ToString("HH:mm:s");
            //        //Logs.Add($"[{timestamp}] {message}");

            //        return;
            //    }
            //    try
            //    {
            //        //instrument = new N6700B("TCPIP0::10.1.2.150::5025::SOCKET");//TCPIP0::{IP ADDRESS HERE}::5025::SOCKET
            //        instrument = new N6700B($@"TCPIP0::{IpAddress}::5025::SOCKET");//TCPIP0::{IP ADDRESS HERE}::5025::SOCKET

            //        //try
            //        //{
            //        instrument.Initialize(out error);
            //        //Logs.Add("Error: No connection to power supply");
            //        //}
            //        //catch (Exception)
            //        //{
            //        //   Logs.Add("Error: Could not Initialize Instrument");
            //        //   return;
            //        //};

            //        //if (instrument == null)
            //        //{
            //        //    Logs.Add("Error: No connection to power supply");
            //        //    return;
            //        //}


            //        if (error.errorOccurred)
            //        {
            //            Logs.Add($"Initialization failed: {error}");
            //            instrument = null;
            //            IsConnected = false;
            //            return;
            //        }
            //        IsConnected = true;
            //        Logs.Add("Connected and intialized successfully.");





            //        // change status

            //        //IsConnected = true;





            //        //isOutputOn = !isOutputOn;
            //        //N6700B.State outputState = isOutputOn ? N6700B.State.ON : N6700B.State.OFF;

            //        //instrument.SetOutputState(1, outputState, out error);


            //    }
            //    catch (Exception ex)
            //    {
            //        Logs.Add($"Connection Error: {ex.Message}");
            //        instrument = null;
            //        IsConnected = false;


            //    }
            //}

            ////if (instrument == null) 
            ////{
            ////    Logs.Add("Instrument object is null! Cannot initiallize");
            ////    return;
            ////    //instrument = new N6700B(IpAddress);

            ////}

            ////ERROR error = new();
            ////try
            ////{

            ////    instrument = new N6700B(IpAddress);
            ////    instrument.Initialize(out error);

            ////}
            ////catch (Exception e)
            ////{
            ////    //om du inte lyckas ansluta, visa felmeddelandet i loggen
            ////    Logs.Add($@"code: {error.errorCode}\nMessage: {error.errorMsg}\nException:{e.Message}");
            ////}


            //////N6700B instrument = new N6700B(IpAddress);
            ////ERROR error = new ERROR();
            ////instrument.Initialize(out error);
            ////Logs.Add()


            ////IsConnected = !IsConnected;
            //OnPropertyChanged(nameof(ConnectionStatus));
            //OnPropertyChanged(nameof(ConnectionStatusColor));
            //OnPropertyChanged(nameof(ConnectionButtonText));
            //OnPropertyChanged(nameof(IsDisconnected));

        }

        private string GetFriendlyErrorMessage(ERROR error)
        {
            if (error.errorCode == 0 && string.IsNullOrWhiteSpace(error.errorMsg))
                return "Okänt fel.";

            return error.errorCode switch
            {
                -222 => $"Fel {error.errorCode}: Värdet är utanför tillåtet intervall.",
                -410 => $"Fel {error.errorCode}: Enheten svarade inte i tid.",
                -350 => $"Fel {error.errorCode}: Intern kö är full.",
                _ => $"Fel {error.errorCode}: {error.errorMsg}"
            };
        }


        [RelayCommand]
        public void SetVoltage()
        {
            ERROR error = new ERROR();
            try
            {
                if (instrument == null) 
                {
                    AddLog("Error: No connection to power supply");
                    return;
                
                }
                if (Voltage < MinVoltage || Voltage > MaxVoltage)
                {
                    ValidationMessage = $"Invalid voltage value. Must be between {MinVoltage}V and {MaxVoltage}V.";
                    return;
                }

                instrument.SetVoltage(1, this.Voltage, out error);
            }
            catch(Exception ex)
            {
                Logs.Add(ex.Message);
                Logs.Add(error.errorCode.ToString());
                Logs.Add(error.errorMsg);

                //string userFriendly = $"❌ Fel: {ex.Message}";

                //if (error.errorCode != 0 || !string.IsNullOrWhiteSpace(error.errorMsg))
                //    userFriendly += $" ({error.errorCode}: {error.errorMsg})";

                //Logs.Add(userFriendly);
                //ValidationMessage = userFriendly;


            }
            OnPropertyChanged(nameof(Voltage));
            //instrument.SetVoltage(Voltage);
        }

        [RelayCommand]
        public void SetCurrent()
        {
            ERROR error = new ERROR();

            try
            {
               if (instrument == null)
                {
                    AddLog("Error: No connection to power supply");
                    //Logs.Add(ValidationMessage);
                    return;
                }

                if (Current < MinCurrent || Current > MaxCurrent)
                {
                    ValidationMessage = $"Invalid current value. Must be between {MinCurrent}A and {MaxCurrent}A.";
                    return;
                }



                instrument.SetCurrent(1, this.Current, out error);

            }
            catch (Exception ex) 
            {
                Logs.Add(ex.Message);
                Logs.Add(error.errorCode.ToString());
                Logs.Add(error.errorMsg);



                //string message = $"❌ Fel: {ex.Message}";


                //if (error.errorCode != 0 || !string.IsNullOrWhiteSpace(error.errorMsg))
                //{
                //    message += $"\n {GetFriendlyErrorMessage(error)}";
                //}


                //ValidationMessage = message;
                //AddLog(message);



            }

            //ERROR error = new();
            //instrument.SetCurrent(0, Current, out error);
            //OnPropertyChanged(nameof(Current));


        }

        //[RelayCommand]
        //public void ToggleOutput()
        //{
        //    IsOutputOn = !IsOutputOn;
        //    OutputStatus = IsOutputOn ? "ON" : "OFF";

        //    OnPropertyChanged(nameof(OutputStatusColor));

        //}
        [RelayCommand]
        private void ToggleOutput()
        {
            ERROR error = new ERROR();

            try
            {
                if (instrument == null)
                {
                    AddLog("Error: No connection to power supply");
                    return;
                }


                //isOutputOn = !isOutputOn;
                N6700B.State outputState = isOutputOn ? N6700B.State.ON : N6700B.State.OFF;
                instrument.SetOutputState(1, outputState, out error);

            }
            catch (Exception ex)
            {
                Logs.Add(ex.Message);
                Logs.Add(error.errorCode.ToString());
                Logs.Add(error.errorMsg);


            }

            //ERROR error = new ERROR();
            //IsConnected = !IsConnected;
            //N6700B.State outputState = isOutputOn ? N6700B.State.ON : N6700B.State.OFF;
            //instrument.SetOutputState(0, outputState, out ERROR);
            //OnPropertyChanged(nameof(OutputColor));
            //ReadLiveMeasuremen();

        }

    }

}


