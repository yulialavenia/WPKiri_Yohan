﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace Kiri
{
    class Protocol
    {
        HttpClient httpClient = new HttpClient();
        private static String apiKey 
        {
            get 
            { 
                return "97A7A1157A05ED6F";    
            }
        }
        private static String hostname
        {
            get
            {
                return "http://kiri.travel/";
            }
        }
        private static String handle
        {
            get
            {
                return hostname+"handle.php?";
            }
        }
        private static String iconPath
        {
            get
            {
                return hostname + "images/means";
            }
        }
        private static String iconStart
        {
            get
            {
                return hostname + "images/stepicon-walkstart.png";
            }
        }
        private static String iconFinish
        {
            get
            {
                return hostname + "images/stepicon-finish.png";
            }
        }

        private static String version
        {
            get
            {
                return "2";
            }
        }

        private static String modeFind
        {
            get
            {
                return "searchplace";
            }
        }

        private static String modeRoute
        {
            get
            {
                return "findroute";
            }
        }
        private static String modeNearby
        {
            get
            {
                return "nearbytransport";
            }
        }
        private static String locale
        {
            get
            {
                return "locale";
            }
        }
        private static String start
        {
            get
            {
                return "start";
            }
        }
        private static String presentation
        {
            get
            {
                return "presentation";
            }
        }
        private static String region
        {
            get
            {
                return "region";
            }
        }
        private static String query { get; set; }


        public string getSearchPlace(string query)
        {
            
            String uri = handle + "version=" + version + "&mode=" + modeFind + "&region=" + "bdo" + "&querystring=" + query + "&apikey=" + apiKey;
            return uri;
        }

        public string getFindRoute(string start, string finish)
        {
            RootObjectFindRoute fr = new RootObjectFindRoute();
            String uri = handle + "version=" + version + "&mode=" + modeRoute + "&locale=" + "id" + "&start=" + start + "&finish=" + finish + "&presentation=" + "mobile" + "&apikey=" + apiKey;
            return uri;

        }

        public void getNearby(string loc) { 
        
        }
    }
}