﻿using EntityModel;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using ReactiveUI;

namespace QPAS
{
    public class PerformanceReportPageViewModel : ViewModelBase
    {
        private string _toggleStratsText;
        private string _toggleTagsText;

        internal IDBContext Context;
        internal MainViewModel Parent;
        private string _toggleInstrumentsText;
        private IDataSourcer _datasourcer;
        private BacktestSource _backtestSource;

        public ReportSettings ReportSettings { get; set; }

        public TradeFilterSettings TradeFilterSettings { get; set; }

        public ObservableCollection<CheckListItem<Strategy>> Strategies { get; }

        public ObservableCollection<CheckListItem<Tag>> Tags { get; }

        public ObservableCollection<CheckListItem<Instrument>> Instruments { get; }

        public ObservableCollection<Benchmark> Benchmarks { get; }
        public ObservableCollection<QDMS.Instrument> BacktestSeries { get; }

        public BacktestSource BacktestSource
        {
            get => _backtestSource;
            set => this.RaiseAndSetIfChanged(ref _backtestSource, value);
        }

        public string ToggleStratsText
        {
            get => _toggleStratsText;
            set => this.RaiseAndSetIfChanged(ref _toggleStratsText, value);
        }

        public string ToggleTagsText
        {
            get => _toggleTagsText;
            set => this.RaiseAndSetIfChanged(ref _toggleTagsText, value);
        }

        public string ToggleInstrumentsText
        {
            get => _toggleInstrumentsText;
            set => this.RaiseAndSetIfChanged(ref _toggleInstrumentsText, value);
        }

        public IDataSourcer Datasourcer
        {
            get => _datasourcer;
            private set => this.RaiseAndSetIfChanged(ref _datasourcer, value);
        }

        public ICommand ToggleAllStrategies { get; set; }

        public ICommand ToggleAllTags { get; set; }

        public ICommand ToggleAllInstruments { get; set; }

        public ICommand GenerateReport { get; set; }

        public PerformanceReportPageViewModel(IDBContext context, IDialogCoordinator dialogService, MainViewModel parent, IDataSourcer datasourcer)
            : base(dialogService)
        {
            Context = context;
            Parent = parent;
            Datasourcer = datasourcer;

            ReportSettings = new ReportSettings();
            TradeFilterSettings = new TradeFilterSettings(Context);

            ToggleTagsText = "Select All";
            ToggleStratsText = "Select All";
            ToggleInstrumentsText = "Deselect All";

            Strategies = new ObservableCollection<CheckListItem<Strategy>>();
            Tags = new ObservableCollection<CheckListItem<Tag>>();
            Instruments = new ObservableCollection<CheckListItem<Instrument>>();
            Benchmarks = new ObservableCollection<Benchmark>();
            BacktestSeries = new ObservableCollection<QDMS.Instrument>();

            CreateCommands();
        }

        private void CreateCommands()
        {
            ToggleAllTags = new RelayCommand(ToggleTags);
            ToggleAllStrategies = new RelayCommand(ToggleStrats);
            ToggleAllInstruments = new RelayCommand(ToggleInstruments);
            GenerateReport = new RelayCommand(GenReport);
        }

        private void ToggleStrats()
        {
            if (ToggleStratsText == "Select All")
            {
                foreach (CheckListItem<Strategy> item in Strategies)
                {
                    item.IsChecked = true;
                }
                ToggleStratsText = "Deselect All";
            }
            else
            {
                foreach (CheckListItem<Strategy> item in Strategies)
                {
                    item.IsChecked = false;
                }
                ToggleStratsText = "Select All";
            }
        }

        private void ToggleTags()
        {
            if (ToggleTagsText == "Select All")
            {
                foreach (CheckListItem<Tag> item in Tags)
                {
                    item.IsChecked = true;
                }
                ToggleTagsText = "Deselect All";
            }
            else
            {
                foreach (CheckListItem<Tag> item in Tags)
                {
                    item.IsChecked = false;
                }
                ToggleTagsText = "Select All";
            }
        }

        private void ToggleInstruments()
        {
            if (ToggleInstrumentsText == "Select All")
            {
                foreach (CheckListItem<Instrument> item in Instruments)
                {
                    item.IsChecked = true;
                }
                ToggleInstrumentsText = "Deselect All";
            }
            else
            {
                foreach (CheckListItem<Instrument> item in Instruments)
                {
                    item.IsChecked = false;
                }
                ToggleInstrumentsText = "Select All";
            }
        }

        public override async Task Refresh()
        {
            //tags
            var selectedTags = Tags
                            .Where(x => x.IsChecked)
                            .Select(x => x.Item)
                            .ToList();
            Tags.Clear();

            foreach (var checkItem in Context
                .Tags
                .OrderBy(x => x.Name)
                .ToList()
                .Select(x => new CheckListItem<Tag>(x, selectedTags.Contains(x))))
            {
                Tags.Add(checkItem);
            }

            //strategies
            var selectedStrats = Strategies
                .Where(x => x.IsChecked)
                .Select(x => x.Item)
                .ToList();
            Strategies.Clear();

            foreach (var checkItem in Context
                .Strategies
                .OrderBy(x => x.Name)
                .ToList()
                .Select(x => new CheckListItem<Strategy>(x, selectedStrats.Contains(x))))
            {
                Strategies.Add(checkItem);
            }

            //Instruments
            if (Instruments.Count == 0)
            {
                //on first load we want all instruments selected, otherwise remember previous selection
                foreach (var checkItem in (await Context
                        .Instruments
                        .OrderBy(x => x.Symbol)
                        .ToListAsync().ConfigureAwait(true))
                    .Select(x => new CheckListItem<Instrument>(x, true)))
                {
                    Instruments.Add(checkItem);
                }
            }
            else
            {
                var selectedInstruments = Instruments
                                .Where(x => x.IsChecked)
                                .Select(x => x.Item)
                                .ToList();
                Instruments.Clear();

                foreach (var checkItem in Context
                    .Instruments
                    .OrderBy(x => x.Symbol)
                    .ToList()
                    .Select(x => new CheckListItem<Instrument>(x, selectedInstruments.Contains(x))))
                {
                    Instruments.Add(checkItem);
                }
            }

            //benchmarks
            Benchmarks.Clear();
            foreach (Benchmark b in Context.Benchmarks.OrderBy(x => x.Name))
            {
                Benchmarks.Add(b);
            }

            //backtest results from the external data source
            BacktestSeries.Clear();
            if(Datasourcer.ExternalDataSource.Connected)
            {
                BacktestSeries.AddRange(
                    await Datasourcer
                        .ExternalDataSource
                        .GetBacktestSeries().ConfigureAwait(true));
            }
        }

        private TimeSeries GetBacktestData()
        {
            return new TimeSeries(new System.Collections.Generic.List<QDMS.OHLCBar>());
        }

        private void GenReport()
        {
            //Load backtest result if it has been specified
            var backtestData = GetBacktestData();

            var selectedTags =
                Tags
                .Where(x => x.IsChecked)
                .Select(x => x.Item)
                .ToList();

            var selectedStrategies =
                Strategies
                .Where(x => x.IsChecked)
                .Select(x => x.Item)
                .ToList();

            var selectedInstruments = 
                Instruments
                .Where(x => x.IsChecked)
                .Select(x => x.Item)
                .ToList();

            var trades = TradeFiltering.Filter(selectedTags, selectedStrategies, selectedInstruments, Context, TradeFilterSettings);

            Parent.GenerateReportFromTrades.Execute(trades);
        }
    }
}