using DipMod.Command;
using DipMod.Model;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;


namespace DipMod.ViewModel
{
    internal class ViewModelApp:INotifyPropertyChanged
    {
        Modbus md;
        private bool Connect = false;
        private bool Online = false;
        ObservableCollection<AnalogPort> analogPorts;
        ObservableCollection<AnalogPort> analogPortsOnline;
        ObservableCollection<DigitalPort> digitalPorts;
        private RelayCommand establishConnection;
        AnalogPort Aport;
        DigitalPort Dport;
        public Func<double, string> AxisXFormatterr { get; set; }
        public double TimeStep { get; set; }
        readonly List <string> parameter =new List<string> { "Не подключено","Температура", "Влажность", "Давление","Углекислый газ", "Дым","Напряжение", "Освещеность" };
        internal ViewModelApp()
        {
            SeriesCollection = new SeriesCollection();
            AxisXFormatterr = value => new DateTime((long)value).ToString("HH:mm:ss");
            SeriesCollectionOnline = new SeriesCollection();
            PortSettings = new ObservableCollection<PortSettings>
        {
            new PortSettings(1, parameter),
            new PortSettings(2, parameter),
            new PortSettings(3, parameter),
            new PortSettings(4, parameter),
            new PortSettings(5, parameter),
            new PortSettings(6, parameter),
            new PortSettings(7, parameter),
            new PortSettings(8, parameter)
        };
            LoadSettings();
        }
        public void LoadSettings()
        {
            if (File.Exists("portSettings.json"))
            {
                var json = File.ReadAllText("portSettings.json");
                var loadedSettings = JsonSerializer.Deserialize<ObservableCollection<PortSettings>>(json);
                if (loadedSettings != null)
                {
                    PortSettings = loadedSettings;
                }
            }
        }
        public List<string> Parameter
        {
            get
            {
                return parameter;
            }
        }
        private Func<double, string> _axisXFormatter;
        public Func<double, string> AxisXFormatter
        {
            get { return _axisXFormatter; }
            set
            {
                _axisXFormatter = value;
                OnPropertyChanged(nameof(AxisXFormatter));
            }
        }
        private AxesCollection _axesX = new AxesCollection();
        public AxesCollection AxesX
        {
            get => _axesX;
            set
            {
                _axesX = value;
                OnPropertyChanged(nameof(AxesX));
            }
        }
        public RelayCommand EstablishConnection
        {
            get {
                return establishConnection ?? (establishConnection = new RelayCommand(obj =>
            {
                try
                {  
                    md = new Modbus(ipadress,machine);
                    Connect = true;
                    Task List = new Task(Listening);
                    IsEnabled = true;
                    List.Start();
                    
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            ));  }
        }
        private RelayCommand establishDisconnection;
        public RelayCommand EstablishDisconnection
        {
            get
            {
                return establishDisconnection ?? (establishDisconnection = new RelayCommand(obj =>
                {
                    try
                    {
                        Discon();
                        IsEnabled = false;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            ));
            }
        }
        
        private RelayCommand getallanalog;
        public RelayCommand GetAllAnalog
        {
            get {
                return getallanalog ?? (getallanalog = new RelayCommand(obj =>
            {
                try
                {
                    bool read = true;
                    while (read)
                    {
                        analogPorts = md.db.GetDataAnalogLast();
                        if (analogPorts != null)
                        {
                            read = false;
                        }
                        
                    }

                    Aport1 = analogPorts.FirstOrDefault(port => port.IDPort == 0).value;
                    Aport2 = analogPorts.FirstOrDefault(port => port.IDPort == 1).value;
                    Aport3 = analogPorts.FirstOrDefault(port => port.IDPort == 2).value;
                    Aport4 = analogPorts.FirstOrDefault(port => port.IDPort == 3).value;
                    Aport5 = analogPorts.FirstOrDefault(port => port.IDPort == 4).value;
                    Aport6 = analogPorts.FirstOrDefault(port => port.IDPort == 5).value;
                    Aport7 = analogPorts.FirstOrDefault(port => port.IDPort == 6).value;
                    Aport8 = analogPorts.FirstOrDefault(port => port.IDPort == 7).value;
                    LastTimeFroze = analogPorts.FirstOrDefault(port => port.IDPort == 0).TimeFroze;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            )); }
        
        }
        private DateTime lasttimefroze;
        public DateTime LastTimeFroze
        {
            get { return lasttimefroze; }
            set
            {
                lasttimefroze = value;
                OnPropertyChanged(nameof(LastTimeFroze));
            }
        }
        
        private RelayCommand getalldigital;
        public RelayCommand GetAllDigital
        {
            get
            {
                return getalldigital ?? (getalldigital = new RelayCommand(obj =>
                {
                    try
                    {
                        bool read = true;
                        while (read)
                        {
                            digitalPorts = md.db.GetDataDigitalLast();
                            if (digitalPorts != null)
                            {
                                read = false;
                            }

                        }

                        Dport1 = digitalPorts.FirstOrDefault(port => port.IDPort == 0).value;
                        Dport2 = digitalPorts.FirstOrDefault(port => port.IDPort == 1).value;
                        LastTimeFroze = digitalPorts.FirstOrDefault(port => port.IDPort == 1).TimeFroze;

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            ));
            }

        }
       
        private RelayCommand getvalueanalog;
        public RelayCommand GetValueAnalog
        {
            get
            {
                return getvalueanalog ?? (getvalueanalog = new RelayCommand(param => ValueGetAnalog(param)));
            }
        }
        void ValueGetAnalog(object param)
        {
            string porte = (string)param;
            int portint = int.Parse(porte);
            try
            {
                switch(portint)
                {
                        case 0:
                        {
                            bool read = true;
                            
                            while (read)
                            {
                                Aport = md.db.GetAnalogPortToIdPort(portint);
                                if (Aport != null)
                                {
                                    read = false;
                                }
                            }
                            Aport1 = Aport.value;
                            LastTimeFroze = Aport.TimeFroze;
                            break;
                        }
                        case 1:
                        {
                            bool read = true;

                            while (read)
                            {
                                Aport = md.db.GetAnalogPortToIdPort(portint);
                                if (Aport != null)
                                {
                                    read = false;
                                }
                            }
                            Aport2 = Aport.value;
                            LastTimeFroze = Aport.TimeFroze;
                            break;
                            
                        }
                         case 2:
                        {
                            bool read = true;
                            while (read)
                            {
                                Aport = md.db.GetAnalogPortToIdPort(portint);
                                if (Aport != null)
                                {
                                    read = false;
                                }
                            }
                            Aport3 = Aport.value;
                            LastTimeFroze = Aport.TimeFroze;
                            break;
                        }
                        case 3:
                        {
                            bool read = true;
                            while (read)
                            {
                                Aport = md.db.GetAnalogPortToIdPort(portint);
                                if (Aport != null)
                                {
                                    read = false;
                                }
                            }
                            Aport4 = Aport.value;
                            LastTimeFroze = Aport.TimeFroze;
                            break;
                        }
                        case 4:
                        {
                            bool read = true;
                            while (read)
                            {
                                Aport = md.db.GetAnalogPortToIdPort(portint);
                                if (Aport != null)
                                {
                                    read = false;
                                }
                            }
                            Aport5 = Aport.value;
                            LastTimeFroze = Aport.TimeFroze;
                            break;
                        }
                        case 5:
                        {
                            bool read = true;
                            while (read)
                            {
                                Aport = md.db.GetAnalogPortToIdPort(portint);
                                if (Aport != null)
                                {
                                    read = false;
                                }
                            }
                            Aport6 = Aport.value;
                            LastTimeFroze = Aport.TimeFroze;
                            break;
                        }
                        case 6:
                        {
                            bool read = true;
                            while (read)
                            {
                                Aport = md.db.GetAnalogPortToIdPort(portint);
                                if (Aport != null)
                                {
                                    read = false;
                                }
                            }
                            Aport7 = Aport.value;
                            LastTimeFroze = Aport.TimeFroze;
                            break;
                        }
                        case 7:
                        {
                            bool read = true;
                            while (read)
                            {
                                Aport = md.db.GetAnalogPortToIdPort(portint);
                                if (Aport != null)
                                {
                                    read = false;
                                }
                            }
                            Aport8 = Aport.value;
                            LastTimeFroze = Aport.TimeFroze;
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private RelayCommand getvaluedigital;
        public RelayCommand GetValueDigital
        {
            get
            {
                return getvaluedigital ?? (getvaluedigital = new RelayCommand(param => ValueGetDigital(param)));
            }
        }
        void ValueGetDigital(object param)
        {
            string porte = (string)param;
            int portint = int.Parse(porte);
            try
            {
                switch (portint)
                {
                    case 0:
                        {
                            bool read = true;

                            while (read)
                            {
                                Dport = md.db.GetDigitalPortToIdPort(portint);
                                if (Dport != null)
                                {
                                    read = false;
                                }
                            }
                            Dport1 = Dport.value;
                            LastTimeFroze = Dport.TimeFroze;
                            break;
                        }
                    case 1:
                        {
                            bool read = true;

                            while (read)
                            {
                                Dport = md.db.GetDigitalPortToIdPort(portint);
                                if (Dport != null)
                                {
                                    read = false;
                                }
                            }
                            Dport2 = Dport.value;
                            LastTimeFroze = Dport.TimeFroze;
                            break;

                        }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private RelayCommand getdatavalue;
        private RelayCommand getdatavalueonline;
        public RelayCommand GetDataValue
        {
            get
            {
                return getdatavalue ?? (getdatavalue = new RelayCommand(obj =>
                {
                    try
                    {
                        bool read = true;
                        while (read)
                        {
                            _analogPorts = md.db.GetDataAnalog(starttime, stoptime);
                            if (_analogPorts != null)
                            {
                                read = false;
                            }
                        }
                        
                        UpdateSeriesCollection();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            ));
            }
        }
        private RelayCommand getdatavalueall;
        public RelayCommand GetDataValueAll
        {
            get
            {
                return getdatavalueall ?? (getdatavalueall = new RelayCommand(obj =>
                {
                    try
                    {
                        bool read = true;
                        while (read)
                        {
                            _analogPorts = md.db.GetDataAnalog();
                            if (_analogPorts != null)
                            {
                                read = false;
                            }
                        }

                        UpdateSeriesCollection();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            ));
            }
        }
        private ObservableCollection<AnalogPort> _analogPorts;
        public ObservableCollection<AnalogPort> AnalogPorts
        {
            get => _analogPorts;
            set
            {
                if (_analogPorts != value)
                {
                    _analogPorts = value;
                    OnPropertyChanged(nameof(AnalogPorts));
                    UpdateSeriesCollection();
                }
            }
        }
        private ObservableCollection<PortSettings> _portSettings;

        public ObservableCollection<PortSettings> PortSettings
        {
            get => _portSettings;
            set
            {
                _portSettings = value;
                OnPropertyChanged(nameof(PortSettings));
            }
        }
        private RelayCommand savecommand;
        public RelayCommand SaveCommand
        {
            get
            {
                return savecommand ?? (savecommand = new RelayCommand(obj =>
                {
                    var json = JsonSerializer.Serialize(PortSettings);
                    File.WriteAllText("portSettings.json", json);
                }
                    ));
            }
        }
        private void UpdateSeriesCollection()
        {
            SeriesCollection.Clear();

            if (AnalogPorts.Count == 0 || AnalogPorts == null)
            {
                Exception ex = new Exception("В данном отрезке времени нет значений");
                throw ex;
            }

            if (selectparamdb == "Не подключено")
            {
                Exception ex = new Exception("Выберете соответствующий показатель");
                throw ex;
            }

            var groupedByPort = AnalogPorts.GroupBy(port => port.IDPort);

            foreach (var group in groupedByPort)
            {
                var port = PortSettings.FirstOrDefault(n => n.PortId == group.Key + 1);
                if (port.SelectedParameter == selectparamdb)
                {
                    var lineSeries = new LineSeries
                    {
                        Title = $"Port {port.PortId}",
                        Values = new ChartValues<DateTimePoint>(
                            group.Select(p => new DateTimePoint(p.TimeFroze, port.ConverttoParam(p.value)))
                        )
                    };
                    SeriesCollection.Add(lineSeries);
                }
            }

            // Вычисление диапазона времени
            var minTime = AnalogPorts.Min(p => p.TimeFroze).Ticks;
            var maxTime = AnalogPorts.Max(p => p.TimeFroze).Ticks;
            var range = TimeSpan.FromTicks(maxTime - minTime);


            // Логика для вычисления шага времени
            long stepTicks;
            if (range.TotalDays > 1)
            {
                stepTicks = TimeSpan.FromHours(6).Ticks;
                AxisXFormatter = value => new DateTime((long)value).ToString("dd.MM HH:mm");
            }
            else if (range.TotalHours > 6)
            {
                stepTicks = TimeSpan.FromHours(1).Ticks;
                AxisXFormatter = value => new DateTime((long)value).ToString("HH:mm");
            }
            else if (range.TotalHours > 1)
            {
                stepTicks = TimeSpan.FromMinutes(15).Ticks;
                AxisXFormatter = value => new DateTime((long)value).ToString("HH:mm");
            }
            else if (range.TotalMinutes > 30)
            {
                stepTicks = TimeSpan.FromMinutes(10).Ticks;
                AxisXFormatter = value => new DateTime((long)value).ToString("HH:mm");
            }
            else if (range.TotalMinutes > 10)
            {
                stepTicks = TimeSpan.FromMinutes(4).Ticks;
                AxisXFormatter = value => new DateTime((long)value).ToString("HH:mm:ss");
            }
            else if (range.TotalMinutes > 1)
            {
                stepTicks = TimeSpan.FromSeconds(30).Ticks;
                AxisXFormatter = value => new DateTime((long)value).ToString("HH:mm:ss");
            }
            else
            {
                stepTicks = TimeSpan.FromSeconds(10).Ticks;
                AxisXFormatter = value => new DateTime((long)value).ToString("HH:mm:ss");
            }

            // Обновление оси X
            AxesX = new AxesCollection
    {
        new Axis
        {
            Title = "Time",
            LabelFormatter = AxisXFormatter,
            MinValue = minTime,
            MaxValue = maxTime,
            LabelsRotation = 45,
            Separator = new Separator
            {
                Step = stepTicks,
                IsEnabled = true
            }
        }
    };

            OnPropertyChanged(nameof(SeriesCollection));
            OnPropertyChanged(nameof(AxesX));
            OnPropertyChanged(nameof(AxisXFormatter));
        }

        private async void UpdateSeriesCollectionOnline()
        {
            while (Online)
            {
                analogPortsOnline = md.GetAnalogOnline();
                var groupedByPort = analogPortsOnline.GroupBy(port => port.IDPort);

                Application.Current.Dispatcher.Invoke(() =>
                {
                foreach (var group in groupedByPort)
                {
                        var lineSeries = SeriesCollectionOnline.OfType<LineSeries>().FirstOrDefault(series => series.Title == $"Port {group.Key}");

                        if (lineSeries == null)
                        {
                            lineSeries = new LineSeries
                            {
                                Title = $"Port {group.Key}",
                                Values = new ChartValues<DateTimePoint>()
                            };
                            SeriesCollectionOnline.Add(lineSeries);
                        }

                        // Обновляем значения
                        var lineValues = (ChartValues<DateTimePoint>)lineSeries.Values;
                        lineValues.AddRange(group.Select(p => new DateTimePoint(p.TimeFroze, p.value)));
                    }
                });
                await Task.Delay(1000);
            }
        }
        public RelayCommand GetDataValueOnline
        {
            get
            {
                return getdatavalueonline ?? (getdatavalueonline = new RelayCommand(obj =>
                {
                    try
                    {
                        Online = !Online;
                        if (Online)
                        {
                            SeriesCollectionOnline.Clear();
                            Task OnlineTask = new Task(UpdateSeriesCollectionOnline);
                            OnlineTask.Start();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            ));
            }
        }

        public SeriesCollection SeriesCollection { get; set; }
        public SeriesCollection SeriesCollectionOnline { get; set; }

        private string selectparamdb;
        public string SelectParamDB
        {
            get { return selectparamdb; }
            set
            {
                selectparamdb = value;
                OnPropertyChanged(nameof(selectparamdb));
            }
        }

        void Discon()
        {
            Connect = false;
            md.Disconnect();
        }
        private string ipadress;
        public string Ipadress
        {
            get { return ipadress; }
            set
            {
                ipadress = value;
                OnPropertyChanged(nameof(Ipadress));
            }
        }
        private int machine=1;
        public int Machine
        {
            get { return machine; }
            set
            {
                machine = value;
                OnPropertyChanged(nameof(Machine));
            }
        }
        private DateTime starttime = DateTime.Now;
        public DateTime Starttime
        {
            get { return starttime; }
            set
            {
                starttime = value;
                OnPropertyChanged(nameof(starttime));
            }
        }
        private DateTime stoptime = DateTime.Now;
        public DateTime Stoptime
        {
            get { return stoptime; }
            set
            {
                stoptime = value;
                OnPropertyChanged(nameof(stoptime));
            }
        }
        private float aport1;
        public float Aport1
        {
            get { return aport1; }
            set
            {
                aport1 = value;
                OnPropertyChanged(nameof(aport1));
            }
        }
        private float aport2;
        public float Aport2
        {
            get { return aport2; }
            set
            {
                aport2 = value;
                OnPropertyChanged(nameof(aport2));
            }
        }
        private float aport3;
        public float Aport3
        {
            get { return aport3; }
            set
            {
                aport3 = value;
                OnPropertyChanged(nameof(aport3));
            }
        }
        private float aport4;
        public float Aport4
        {
            get { return aport4; }
            set
            {
                aport4 = value;
                OnPropertyChanged(nameof(aport4));
            }
        }
        private float aport5;
        public float Aport5
        {
            get { return aport5; }
            set
            {
                aport5 = value;
                OnPropertyChanged(nameof(aport5));
            }
        }
        private float aport6;
        public float Aport6
        {
            get { return aport6; }
            set
            {
                aport6 = value;
                OnPropertyChanged(nameof(aport6));
            }
        }
        private float aport7;
        public float Aport7
        {
            get { return aport7; }
            set
            {
                aport7 = value;
                OnPropertyChanged(nameof(aport7));
            }
        }
        private float aport8;
        public float Aport8
        {
            get { return aport8; }
            set
            {
                aport8 = value;
                OnPropertyChanged(nameof(aport8));
            }
        }
        private bool dport1;
        public bool Dport1
        {
            get { return dport1; }
            set
            {
                dport1 = value;
                OnPropertyChanged(nameof(dport1));
            }
        }
        private bool dport2;
        public bool Dport2
        {
            get { return dport2; }
            set
            {
                dport2 = value;
                OnPropertyChanged(nameof(dport2));
            }
        }
        private bool isenabled=false;
        public bool IsEnabled
        {
            get { return isenabled; }
            set
            {
                isenabled = value;
                OnPropertyChanged(nameof(isenabled));
            }
        }
        void Listening()
        {
            while (Connect) {
                Task Writing = new Task(() => { md.Start(); });
                Writing.Start();
                Task.Delay(1000).Wait();
            }
            

        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
