
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;
using RAPPOR;
using RAPPOR.Helper;
using RAPPOR.Helper.ArrayDrawer;
using RAPPOR.Model;
using BloomFilter = RAPPOR.Model.BloomFilter;

namespace CrypTool.Plugins.RAPPOR.ViewModel
{
    /// <summary>
    /// Represents an instance of the Overview view model, handling the ui logic of the overview
    /// view.
    /// </summary>
    public class OverviewViewModel : ObservableObject, IViewModelBase
    {
        /// <summary>
        /// Name of the overview view model
        /// </summary>
        /**20211204private readonly string name;**/
        /// <summary>
        /// Instance of the rappor class.
        /// </summary>
        private readonly RAPPOR rappor;
        /// <summary>
        /// instance of the Array drawer.
        /// </summary>
        private ArrayDrawer arrayDrawer;
        /// <summary>
        /// Instance of the Bloom Filter.
        /// </summary>
        private readonly BloomFilter bloomFilter;
        /// <summary>
        /// Instance of the permanent randomized response for internal use.
        /// </summary>
        private readonly PermanentRandomizedResponse permanentRandomizedResponse;
        /// <summary>
        /// Initializes the OverviewModel and its parameter.
        /// </summary>
        /// <param name="rAPPOR">Instance of the RAPPOR class used in this component.</param>
        private readonly ArrayDrawerHeatMaps arrayDrawerHM;
        /// <summary>
        /// The drawe class for the overview
        /// </summary>
        private readonly ArrayDrawerRR arrayDrawerRR;

        /// <summary>
        /// Initializes the OverviewViewModel class object. 
        /// </summary>
        /// <param name="rAPPOR">The rappor instance which is being used.</param>
        public OverviewViewModel(RAPPOR rAPPOR)
        {
            arrayDrawerHM = new ArrayDrawerHeatMaps();
            arrayDrawerRR = new ArrayDrawerRR();
            arrayDrawer = new ArrayDrawer();
            rappor = rAPPOR;
            rappor.PropertyChanged += rappor_PropertyChanged;
            bloomFilter = rAPPOR.GetBloomFilter();
            rappor.PropertyChanged += bloomFilter_PropertyChanged;
            permanentRandomizedResponse = rAPPOR.GetPermanentRandomizedResponse();
            rappor.PropertyChanged += permanentRandomizedResponse_PropertyChanged;
            //name = "{Loc Overview}";//This parameter is not relevant
            setUp();
        }
        /// <summary>
        /// This method sets up the Overview view. It initializes the variables and draws a new 
        /// canvas for displaying the different parts of the overview.
        /// </summary>
        private void setUp()
        {
            Input = rappor.Input.Split(',').Length.ToString();
            OnPropertyChanged("InputDataSize");
            PRRArray = arrayDrawer.createArrayCanvas(rappor.GetPermanentRandomizedResponse().GetBoolArray());
            OnPropertyChanged("PRRArray");

            _iRRAreaHandler = new IRRAreaHandler();
            _iRRAreaHandler.Add(CrypTool.Plugins.RAPPOR.Properties.Resources.BF);
            _iRRAreaHandler.Add(CrypTool.Plugins.RAPPOR.Properties.Resources.PRR);
            OnPropertyChanged("IRRAreas");
        }
        #region Bindings
        /// <summary>
        /// The input string
        /// </summary>
        private string _Input;
        /// <summary>
        /// Getter and sett for the input string
        /// </summary>
        public string Input
        {
            get
            {
                if (string.IsNullOrEmpty(_Input))
                {
                    return "";
                }

                return _Input;
            }
            set
            {
                _Input = value;
                OnPropertyChanged("Input");
                rappor.RunRappor();
                UpdateBloomFilter();
            }

        }

        /// <summary>
        /// Propperty changer for the input.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The event arg object.</param>
        private void rappor_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Input")
            {
                Input = rappor.Input;
            }
        }
        /// <summary>
        /// The canvas of the overview view model.
        /// </summary>
        private Canvas _BloomFilterArray;

        /// <summary>
        /// Getter and setter for the bloom filter array.
        /// </summary>
        public Canvas BloomFilterArray
        {
            get
            {
                if (_BloomFilterArray == null)
                {
                    return null;
                }

                return _BloomFilterArray;
            }
            set
            {
                _BloomFilterArray = value;
                OnPropertyChanged("BloomFilterArray");
            }

        }

        /// <summary>
        /// Propperty changer for this class.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The event arg object.</param>
        private void bloomFilter_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "BloomFilterArray")
            {
                BloomFilterArray = bloomFilter.BloomFilterArray;
            }
        }


        /// <summary>
        /// This clas is used to update the bloom filter
        /// </summary>
        private void UpdateBloomFilter()
        {
            if (rappor.Presentation != null)
            {

                rappor.Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    arrayDrawer = new ArrayDrawer();
                }, null);
            }
            UpdatePRRArray();
        }
        #endregion
        /// <summary>
        /// Binding canvas variable.
        /// </summary>
        private Canvas _PRRArray;
        /// <summary>
        /// Getter and setter for the binding cavas variable.
        /// </summary>
        public Canvas PRRArray
        {
            get
            {
                if (_PRRArray == null)
                {
                    return null;
                }

                return _PRRArray;
            }
            set
            {
                _PRRArray = value;
                OnPropertyChanged("PRRArray");
            }

        }
        /// <summary>
        /// Property changed parameter of the binding permanent randomized response variable.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The text composition event arg.</param>
        private void permanentRandomizedResponse_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "PRRArray")
            {
                PRRArray = permanentRandomizedResponse.PRRArray;
            }
        }

        /// <summary>
        /// This method is used to update and redraw the canvas containing the arrays.
        /// </summary>
        private void UpdatePRRArray()
        {
            if (rappor.Presentation != null)
            {

                rappor.Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    rappor.RunRappor();
                    bool[][] boolArrays = new bool[(rappor.GetRAPPORSettings().GetAmountOfInstantaneousRandomizedResponses() + 2)][];
                    boolArrays[0] = rappor.GetBloomFilter().GetBoolArray();
                    boolArrays[1] = rappor.GetPermanentRandomizedResponse().GetBoolArray();
                    for (int i = 0; i < rappor.GetRAPPORSettings().GetAmountOfInstantaneousRandomizedResponses(); i++)
                    {
                        boolArrays[i + 2] = rappor.GetInstantaneousRandomizedResponse()[i].GetBoolArray();
                    }
                    PRRArray = arrayDrawer.createArrayCanvas(boolArrays);
                    PRRArray.Children.Add(arrayDrawer.CreateCross(0, 82, 8, 8, "#f2f2f2", true));
                    PRRArray.Children.Add(arrayDrawer.CreateCross(650, 82, 8, 8, "#f2f2f2", false));
                    PRRArray.Children.Add(arrayDrawer.CreateCross(650, 82, 8, 8, "#f2f2f2", false));
                    PRRArray.Children.Add(arrayDrawer.CreateLine(0, 650, 82, 82, 5, "#f2f2f2"));
                    //PRRArray.Children.Add(arrayDrawerRR.AddStrokedLine(660, 0, 660, ((rappor.GetRAPPORSettings().GetAmountOfInstantaneousRandomizedResponses() + 2) * 100),5,"#808080");
                    _iRRAreaHandler.Remove();
                    _iRRAreaHandler.Add("");
                    _iRRAreaHandler.Add(CrypTool.Plugins.RAPPOR.Properties.Resources.BF);
                    _iRRAreaHandler.Add(CrypTool.Plugins.RAPPOR.Properties.Resources.PRR);
                    for (int i = 0; i < rappor.GetRAPPORSettings().GetAmountOfInstantaneousRandomizedResponses(); i++)
                    {
                        _iRRAreaHandler.Add((i + 1) + ". " + CrypTool.Plugins.RAPPOR.Properties.Resources.IRR);
                    }
                }, null);
                OnPropertyChanged("PRRArray");
            }
        }
        /// <summary>
        /// This class is used to update the prr array
        /// </summary>
        public void DrawCanvas()
        {
            UpdatePRRArray();
        }

        /// <summary>
        /// Binding variable for the irr area handler.
        /// </summary>
        private IRRAreaHandler _iRRAreaHandler;
        /// <summary>
        /// This class is used to handle the irr area.
        /// </summary>
        public class IRRAreaHandler
        {
            public IRRAreaHandler()
            {
                IRRAreas = new ObservableCollection<string>();
            }

            //public List<IRRArea> IRRAreas { get; private set; }
            public ObservableCollection<string> IRRAreas = new ObservableCollection<string>();

            public void Add(string name)
            {
                IRRAreas.Add(name);
            }
            public void Remove()
            {
                IRRAreas.Clear();
            }
        }
        /// <summary>
        /// Observable collection of the irr areas.
        /// </summary>
        public ObservableCollection<string> IRRAreas => _iRRAreaHandler.IRRAreas;



    }
}