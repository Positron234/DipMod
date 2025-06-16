using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

public class PortSettings : INotifyPropertyChanged
{
    public int PortId { get; set; }
    public string SelectedParameter { get; set; }
    public List<string> AvailableParameters { get; set; }

    private double _inputMin = -10;
    private double _inputMax = 10;
    private double _outputMin = 0;
    private double _outputMax = 100;
    private double _offset = 0;

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public double inputMin
    {
        get => _inputMin;
        set
        {
            if (_inputMin != value)
            {
                _inputMin = value;
                OnPropertyChanged();
            }
        }
    }

    public double inputMax
    {
        get => _inputMax;
        set
        {
            if (_inputMax != value)
            {
                _inputMax = value;
                OnPropertyChanged();
            }
        }
    }

    public double outputMin
    {
        get => _outputMin;
        set
        {
            if (_outputMin != value)
            {
                _outputMin = value;
                OnPropertyChanged();
            }
        }
    }

    public double outputMax
    {
        get => _outputMax;
        set
        {
            if (_outputMax != value)
            {
                _outputMax = value;
                OnPropertyChanged();
            }
        }
    }

    public double offset
    {
        get => _offset;
        set
        {
            if (_offset != value)
            {
                _offset = value;
                OnPropertyChanged();
            }
        }
    }

    public PortSettings(int portId, List<string> availableParameters)
    {
        PortId = portId;
        AvailableParameters = availableParameters;
        SelectedParameter = availableParameters.FirstOrDefault();
    }

    public double ConverttoParam(double value)
    {
        return (value - inputMin) / (inputMax - inputMin) * (outputMax - outputMin) + outputMin + offset;
    }
}