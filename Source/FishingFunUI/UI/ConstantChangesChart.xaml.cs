using LiveCharts;
using LiveCharts.Configurations;
using System;
using System.ComponentModel;
using System.Windows.Controls;

#nullable enable
namespace FishingFun
{
    public class MeasureModel
    {
        public DateTime DateTime { get; set; }
        public double Value { get; set; }
    }

    public partial class ConstantChangesChart : UserControl, INotifyPropertyChanged
    {
        private double _axisMax;
        private double _axisMin;

        public ConstantChangesChart()
        {
            InitializeComponent();
            this.Chart.DisableAnimations = false;

            //To handle live data easily, in this case we built a specialized type
            //the MeasureModel class, it only contains 2 properties
            //DateTime and Value
            //We need to configure LiveCharts to handle MeasureModel class
            //The next code configures MeasureModel  globally, this means
            //that LiveCharts learns to plot MeasureModel and will use this config every time
            //a IChartValues instance uses this type.
            //this code ideally should only run once
            //you can configure series in many ways, learn more at
            //http://lvcharts.net/App/examples/v1/wpf/Types%20and%20Configuration

            var mapper = Mappers.Xy<MeasureModel>()
                .X(model => model.DateTime.Ticks)   //use DateTime.Ticks as X
                .Y(model => model.Value);           //use the value property as Y

            //lets save the mapper globally.
            Charting.For<MeasureModel>(mapper);

            //the values property will store our values array
            ChartValues = new ChartValues<MeasureModel>()
            {
                new MeasureModel{ DateTime=DateTime.Now.AddSeconds(1), Value=0 },
                new MeasureModel{ DateTime=DateTime.Now.AddSeconds(100), Value=0 }
            };
            ChartValues2 = new ChartValues<MeasureModel>()
            {
                new MeasureModel { DateTime = DateTime.Now.AddSeconds(1), Value = 0 },
                new MeasureModel { DateTime = DateTime.Now.AddSeconds(100), Value = 0 }
            };
            ChartValues3 = new ChartValues<MeasureModel>()
            {
                new MeasureModel { DateTime = DateTime.Now.AddSeconds(1), Value = 0 },
                new MeasureModel { DateTime = DateTime.Now.AddSeconds(100), Value = 0 }
            };

            //lets set how to display the X Labels
            DateTimeFormatter = value => "";

            //AxisStep forces the distance between each separator in the X axis
            AxisStep = TimeSpan.FromSeconds(1).Ticks;
            //AxisUnit forces lets the axis know that we are plotting seconds
            //this is not always necessary, but it can prevent wrong labeling
            AxisUnit = TimeSpan.TicksPerSecond;

            SetAxisLimits(DateTime.Now);

            DataContext = this;
        }

        internal void Add(int value)
        {
            var now = DateTime.Now;

            ChartValues.Add(new MeasureModel
            {
                DateTime = now,
                Value = value
            });

            SetAxisLimits(now);
        }

        public void ClearChart()
        {
            this.ChartValues.Clear();
            this.ChartValues2.Clear();
            this.ChartValues3.Clear();

            ChartValues2.Add(new MeasureModel
            {
                DateTime = DateTime.Now.AddSeconds(-12),
                Value = 0
            });

            ChartValues2.Add(new MeasureModel
            {
                DateTime = DateTime.Now.AddSeconds(25),
                Value = 0
            });

            ChartValues3.Add(new MeasureModel
            {
                DateTime = DateTime.Now.AddSeconds(-12),
                Value = -7
            });

            ChartValues3.Add(new MeasureModel
            {
                DateTime = DateTime.Now.AddSeconds(25),
                Value = -7
            });
        }

        public ChartValues<MeasureModel> ChartValues { get; set; }
        public ChartValues<MeasureModel> ChartValues2 { get; set; }

        public ChartValues<MeasureModel> ChartValues3 { get; set; }
        public Func<double, string> DateTimeFormatter { get; set; }
        public double AxisStep { get; set; }
        public double AxisUnit { get; set; }

        public double AxisMax
        {
            get { return _axisMax; }
            set
            {
                _axisMax = value;
                OnPropertyChanged("AxisMax");
            }
        }

        public double AxisMin
        {
            get { return _axisMin; }
            set
            {
                _axisMin = value;
                OnPropertyChanged("AxisMin");
            }
        }

        public void SetAxisLimits(DateTime now)
        {
            AxisMax = now.Ticks + TimeSpan.FromSeconds(1).Ticks; // lets force the axis to be 1 second ahead
            AxisMin = now.Ticks - TimeSpan.FromSeconds(8).Ticks; // and 8 seconds behind
        }

        #region INotifyPropertyChanged implementation

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion INotifyPropertyChanged implementation
    }
}