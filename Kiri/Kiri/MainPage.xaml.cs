﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Kiri.Resources;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Devices.Geolocation;
using System.IO.IsolatedStorage;
using System.Windows.Media;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using Newtonsoft.Json;

using System.Windows.Controls.Primitives;
using System.ComponentModel;
using System.Threading;

using System.Device.Location; //ilangin kalo buat kelas city

namespace Kiri
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        private Protocol protocol;
        private LocationFinder lFinder;
        private HttpClient httpClient = new HttpClient();

        private String locationFrom, locationTo;
        private City c;
        private String myCity;

        private BackgroundWorker backgroundWorker;
        
        public MainPage()
        {
            InitializeComponent();
            this.lFinder = new LocationFinder();
            this.c = new City();
            this.cmbCurrFrom.ItemsSource = c.city;
            this.protocol = new Protocol();
            this.locationFrom = "";
            this.locationTo = "";
            ShowSplash();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            string locMapsFrom = "";
            string locMapsTo = "";
            if (NavigationContext.QueryString.TryGetValue("locMapsFrom", out locMapsFrom)) ;
            this.locationFrom = locMapsFrom+"";
            if (locationFrom!=null && !locationFrom.Equals(""))
            {
                fromBox.Text = "Maps";
            }
            if (NavigationContext.QueryString.TryGetValue("locMapsTo", out locMapsTo)) ;
            this.locationTo = locMapsTo+"";
            if (locationTo!=null && !locationTo.Equals(""))
            {
                toBox.Text = "Maps";
            }
            /*
             panelFrom.Visibility = Visibility.Collapsed;
             panelTo.Visibility = Visibility.Collapsed;
             */
        }

        private async void startRoute(object sender, RoutedEventArgs e)
        {
            
            String queryFrom = fromBox.Text;
            String queryTo = toBox.Text;
            string requestFrom = "";
            string requestTo = "";
            RootObjectSearchPlace from = null; //Untuk Asal
            RootObjectSearchPlace to = null; //Untuk Tujuan
            //Penting!
            //Dokumentasi zhttps://bitbucket.org/projectkiri/kiri_api/wiki/KIRI%20API%20v2%20Documentation
            //Contoh searchplace    zhttp://kiri.travel/handle.php?version=2&mode=searchplace&region=bdo&querystring=bip&apikey=97A7A1157A05ED6F
            //                      zhttp://kiri.travel/handle.php?version=2&mode=searchplace&region=bdo&querystring=pvj&apikey=97A7A1157A05ED6F
            //Contoh findroute      zhttp://kiri.travel/handle.php?version=2&mode=findroute&locale=id&&start=-6.90864,107.61108&finish=-6.88929,107.59574&presentation=mobile&apikey=97A7A1157A05ED6F
            //Contoh findroute      zhttp://kiri.travel/handle.php?version=2&mode=findroute&locale=id&&start=-6.87474,107.60491&finish=-6.88909,107.59614&presentation=mobile&apikey=97A7A1157A05ED6F    

            //Check form
            if (!queryFrom.Equals("") && !queryTo.Equals(""))
            {
                //Validate From
                Boolean routeStatus = true;
                progressFindPlace.IsIndeterminate = true;
                //Get place from query
                if ((!queryFrom.Equals("Here") || !queryFrom.Equals("Maps")) && locationFrom.Equals("")) //Check get location from GPS
                {
                    //Reference zhttps://msdn.microsoft.com/en-us/library/hh191443.aspx
                    Task<string> requestFromTask = httpClient.GetStringAsync(new Uri(protocol.getSearchPlace(queryFrom, myCity)));
                    requestFrom = await requestFromTask;
                    from = new RootObjectSearchPlace();
                    from = JsonConvert.DeserializeObject<RootObjectSearchPlace>(requestFrom); //Mengubah String menjadi objek
                    if (from.searchresult.Count() == 0)
                    {
                        MessageBox.Show("Pencarian untuk kata " + fromBox.Text + " tidak ditemukan");
                        routeStatus = false;
                    }
                }
                if ((!queryTo.Equals("Here") || !queryTo.Equals("Maps")) && locationTo.Equals("")) //Check get location from GPS
                {
                    Task<string> requestToTask = httpClient.GetStringAsync(new Uri(protocol.getSearchPlace(queryTo, myCity)));
                    requestTo = await requestToTask;
                    to = new RootObjectSearchPlace();
                    to = JsonConvert.DeserializeObject<RootObjectSearchPlace>(requestTo);//Mengubah String menjadi objek
                    if (to.searchresult.Count() == 0)
                    {
                        MessageBox.Show("Pencarian untuk kata " + toBox.Text + " tidak ditemukan");
                        routeStatus = false;
                    }
                }
                //Check Query
                if (routeStatus == true)
                {
                    this.findRoute();
                    if(locationFrom.Equals("")){
                        getListItem(from, "from"); //Show Listbox for location From
                    }
                    if (locationTo.Equals(""))
                    {
                        getListItem(to, "to");  //Show Listbox for location To
                    }
                }
                progressFindPlace.IsIndeterminate = false;
            }
            else {
                MessageBox.Show("Harap melengkapi tempat asal dan tempat tujuan");
            }

            //getListItem(from);
            //getListItem(to);
        }
        /*
        public RootObjectSearchPlace toObjectSearchPlace(string uri) {
            RootObjectSearchPlace sp = null;
            Task<string> request = httpClient.GetStringAsync(new Uri(uri));
            request.ContinueWith(delegate
            {
                Dispatcher.BeginInvoke(new Action(delegate
                {
                    sp = new RootObjectSearchPlace();
                    sp = JsonConvert.DeserializeObject<RootObjectSearchPlace>(request.Result);
                }));
            });
            return sp;
        }
        */
        private void changeMapFrom(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Map.xaml?fromMapFor=from", UriKind.Relative));    
        }
        private void changeMapTo(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Map.xaml?fromMapFor=to", UriKind.Relative));    
        }

        private void getHereFrom(object sender, RoutedEventArgs e)
        {
            locationFrom = lFinder.coorLat + "," + lFinder.coorLong;
            fromBox.Text = "Here";
        }

        private void getHereTo(object sender, RoutedEventArgs e)
        {
            locationTo = lFinder.coorLat + "," + lFinder.coorLong;
            toBox.Text = "Here";
        }

        /*reference: zhttps://msdn.microsoft.com/en-us/library/bb412179%28v=vs.110%29.aspx*/
        /*
        public static T Deserialize<T>(string json)
        {
            var obj = Activator.CreateInstance<T>();
            using (var ms = new MemoryStream(Encoding.Unicode.GetBytes(json)))
            {
                var serializer = new DataContractJsonSerializer(obj.GetType());
                obj = (T)serializer.ReadObject(ms);
                return obj;
            }
        }

        public static string Serialize<T>(T obj)
        {
            var serializer = new DataContractJsonSerializer(obj.GetType());
            using (var ms = new MemoryStream())
            {
                serializer.WriteObject(ms, obj);
                return Encoding.UTF8.GetString(ms.ToArray(),0,(int)ms.Length);
            }
        }
         */

        //zhttps://social.msdn.microsoft.com/forums/windowsapps/en-us/7db73c64-86f7-4c43-9fd4-faa03421ea21/popup-blocking-listbox-selection-changed
        
        private void getListItem(RootObjectSearchPlace request,String forRequest)
        {
            //ListBox listFrom = new ListBox();
            //ListBox listTo = new ListBox();
            Dispatcher.BeginInvoke(new Action(delegate
            {
                if (request.status.ToString().Equals("ok")) //check status
                {
                    if (request.searchresult.Count() == 1)
                    {
                        if (forRequest.Equals("from"))
                        {
                            this.locationFrom = request.searchresult[0].location.ToString();
                        }
                        else
                        {
                            this.locationTo = request.searchresult[0].location.ToString();
                        }
                    }
                    else
                    {
                        //LayoutRoot.Children.Add(getListItem(requestFrom));
                        if (forRequest.Equals("from"))
                        {
                            for (int c = 0; c < request.searchresult.Count; c++)
                            {
                                listPlaceFrom.Items.Add(request.searchresult[c].placename);
                            }
                            panelFrom.Visibility = Visibility.Visible;
                            listPlaceFrom.DataContext = request.searchresult;
                            listPlaceFrom.SelectionChanged += ListBoxSelectedPlace;
                        }
                        else
                        {
                            for (int c = 0; c < request.searchresult.Count; c++)
                            {
                                listPlaceTo.Items.Add(request.searchresult[c].placename);
                            }
                            if (panelFrom.Visibility == Visibility.Visible)
                            {
                                panelTo.Visibility = Visibility.Collapsed;
                            }
                            else {
                                panelTo.Visibility = Visibility.Visible;
                            }
                            listPlaceTo.DataContext = request.searchresult;
                            listPlaceTo.SelectionChanged += ListBoxSelectedPlace;
                        }

                        
                    }
                }
                else
                {
                    MessageBox.Show("Error!");
                }
            }));

        }

        private void ListBoxSelectedPlace(object sender, SelectionChangedEventArgs e)
        {
            if (panelFrom.Visibility.Equals(Visibility.Visible))
            { //Check visibility
                if (null != (sender as ListBox).SelectedItem)
                {
                    if ((sender as ListBox).SelectedIndex >= 0)
                    {
                        this.locationFrom = searchCoordinatePlace((List<Searchresult>)listPlaceFrom.DataContext, listPlaceFrom.SelectedItem.ToString());  //((sender as ListBox).SelectedItem as Searchresult).placename;
                    }
                }
                panelFrom.Children.Clear();
                panelFrom.Visibility = Visibility.Collapsed;
                panelTo.Visibility = Visibility.Visible;
            }
            else {
                if (null != (sender as ListBox).SelectedItem)
                {
                    if ((sender as ListBox).SelectedIndex >= 0)
                    {
                        this.locationTo = searchCoordinatePlace((List<Searchresult>)listPlaceTo.DataContext, listPlaceTo.SelectedItem.ToString()); //((sender as ListBox).SelectedItem as Searchresult).placename;
                    }
                }
                panelTo.Children.Clear();
                panelTo.Visibility = Visibility.Collapsed;
            }

            this.findRoute();
        }

        public string searchCoordinatePlace(List<Searchresult> listResult, string place) { //Pasti ketemu
            String coordinate = "0.0";
            for (int c = 0; c < listResult.Count; c++)
            {
                if (place.Equals(listResult[c].placename))
                {
                    coordinate = listResult[c].location;
                    c = listResult.Count;
                }
            }
            return coordinate;
        }

        public void findRoute()
        {
            if (!this.locationFrom.Equals("") && !this.locationTo.Equals(""))
            {
                NavigationService.Navigate(new Uri("/Route.xaml?start=" + locationFrom + "&nameFrom=" + fromBox.Text + "&finish=" + locationTo + "&nameTo=" + toBox.Text, UriKind.Relative));
            }
        }

        private void changeCity(object sender, SelectionChangedEventArgs e)
        {
            if (!cmbCurrFrom.SelectedItem.Equals(null))
            {
                switch (cmbCurrFrom.SelectedItem.ToString())
                {
                    case "Bandung":
                        this.myCity="bdo";
                        break;
                    case "Jakarta":
                        this.myCity="cgk";
                        break;
                    case "Malang":
                        this.myCity="mlg";
                        break;
                    case "Surabaya":
                        this.myCity="sub";
                        break;     
                    default:
                        this.myCity="bdo"; //Finder Auto
                        break;
                }
            }
        }

        public void showCity() {
            int indexCity = -1;
            while (indexCity == -1)
            {
                indexCity = c.getNearby(lFinder.coorLat, lFinder.coorLong);
            }
            this.myCity = c.cityCode[indexCity];
        }

        private void ShowSplash()
        {
            this.popup.IsOpen = true;
            StartLoadingData();
        }

        private void StartLoadingData()
        {
            backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += new DoWorkEventHandler(backgroundWorker_DoWork);
            backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorker_RunWorkerCompleted);
            backgroundWorker.RunWorkerAsync();
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            showCity();
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(() =>
            {
                this.cmbCurrFrom.SelectedIndex = c.getIndexFromCityCode(myCity);
                this.popup.IsOpen = false;
            });
        }


        // Sample code for building a localized ApplicationBar
        //private void BuildLocalizedApplicationBar()
        //{
        //    // Set the page's ApplicationBar to a new instance of ApplicationBar.
        //    ApplicationBar = new ApplicationBar();

        //    // Create a new button and set the text value to the localized string from AppResources.
        //    ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
        //    appBarButton.Text = AppResources.AppBarButtonText;
        //    ApplicationBar.Buttons.Add(appBarButton);

        //    // Create a new menu item with the localized string from AppResources.
        //    ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
        //    ApplicationBar.MenuItems.Add(appBarMenuItem);
        //}
    }
}