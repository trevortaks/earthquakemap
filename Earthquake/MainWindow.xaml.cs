﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Web;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using EarthQuake;
using System.Windows.Media.Animation;


namespace QuakeData
{
    
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string baseurl = "https://earthquake.usgs.gov/fdsnws/event/1/query?format=geojson&starttime=2019-02-12&minmagnitude=3";
        string url = "https://earthquake.usgs.gov/fdsnws/event/1/query?format=geojson&";
        RootObject root;

        public MainWindow()
        {
            InitializeComponent();
            initialiseData(baseurl);
        }

        public void initialiseData(string url) {
            Dictionary<System.Windows.Point, Ellipse> points = new Dictionary<System.Windows.Point, Ellipse>();

            try
            {
                root = getData(url);
            }
            catch (System.Net.WebException e)
            {
                lblStatus.Content = e.Message;
                string json = "{\"type\":\"FeatureCollection\",\"metadata\":{\"generated\":1549544968000,\"url\":\"https://earthquake.usgs.gov/fdsnws/event/1/query?format=geojson&starttime=2019-02-07&minmagnitude=5\",\"title\":\"USGS Earthquakes\",\"status\":200,\"api\":\"1.7.0\",\"count\":3},\"features\":[{\"type\":\"Feature\",\"properties\":{ \"mag\":5.4,\"place\":\"99km ENE of Tobelo, Indonesia\",\"time\":1549533788290,\"updated\":1549534764040,\"tz\":540,\"url\":\"https://earthquake.usgs.gov/earthquakes/eventpage/us2000jepg\",\"detail\":\"https://earthquake.usgs.gov/fdsnws/event/1/query?eventid=us2000jepg&format=geojson\",\"felt\":null,\"cdi\":null,\"mmi\":null,\"alert\":null,\"status\":\"reviewed\",\"tsunami\":1,\"sig\":449,\"net\":\"us\",\"code\":\"2000jepg\",\"ids\":\",us2000jepg,\",\"sources\":\",us,\",\"types\":\",geoserve,origin,phase-data,\",\"nst\":null,\"dmin\":2.002,\"rms\":1.45,\"gap\":106,\"magType\":\"mb\",\"type\":\"earthquake\",\"title\":\"M 5.4 - 99km ENE of Tobelo, Indonesia\"},\"geometry\":{\"type\":\"Point\",\"coordinates\":[128.7647,2.2151,33]},\"id\":\"us2000jepg\"},{\"type\":\"Feature\",\"properties\":{\"mag\":5.7,\"place\":\"129km NW of Kota Ternate, Indonesia\",\"time\":1549512929410,\"updated\":1549520267062,\"tz\":480,\"url\":\"https://earthquake.usgs.gov/earthquakes/eventpage/us2000jekx\",\"detail\":\"https://earthquake.usgs.gov/fdsnws/event/1/query?eventid=us2000jekx&format=geojson\",\"felt\":2,\"cdi\":4.5,\"mmi\":4.37,\"alert\":\"green\",\"status\":\"reviewed\",\"tsunami\":0,\"sig\":501,\"net\":\"us\",\"code\":\"2000jekx\",\"ids\":\",us2000jekx,\",\"sources\":\",us,\",\"types\":\",dyfi,geoserve,losspager,origin,phase-data,shakemap,\",\"nst\":null,\"dmin\":1.168,\"rms\":1.1,\"gap\":91,\"magType\":\"mww\",\"type\":\"earthquake\",\"title\":\"M 5.7 - 129km NW of Kota Ternate, Indonesia\"},\"geometry\":{\"type\":\"Point\",\"coordinates\":[126.4563,1.5093,35]},\"id\":\"us2000jekx\"},{\"type\":\"Feature\",\"properties\":{\"mag\":5.5,\"place\":\"43km NE of Auki, Solomon Islands\",\"time\":1549503694180,\"updated\":1549511014814,\"tz\":660,\"url\":\"https://earthquake.usgs.gov/earthquakes/eventpage/us2000jek8\",\"detail\":\"https://earthquake.usgs.gov/fdsnws/event/1/query?eventid=us2000jek8&format=geojson\",\"felt\":3,\"cdi\":2.7,\"mmi\":4.01,\"alert\":\"green\",\"status\":\"reviewed\",\"tsunami\":0,\"sig\":466,\"net\":\"us\",\"code\":\"2000jek8\",\"ids\":\",us2000jek8,\",\"sources\":\",us,\",\"types\":\",dyfi,geoserve,losspager,origin,phase-data,shakemap,\",\"nst\":null,\"dmin\":1.391,\"rms\":0.85,\"gap\":50,\"magType\":\"mb\",\"type\":\"earthquake\",\"title\":\"M 5.5 - 43km NE of Auki, Solomon Islands\"},\"geometry\":{\"type\":\"Point\",\"coordinates\":[160.9592,-8.4642,65.94]},\"id\":\"us2000jek8\"}],\"bbox\":[126.4563,-8.4642,33,160.9592,2.2151,65.94]}";
                root = JsonConvert.DeserializeObject<RootObject>(json);
            }
            finally
            {
                points = getQuake();
                Draw(points);
            }
        }

        public Dictionary<System.Windows.Point, Ellipse> getQuake()
        {
            Dictionary<System.Windows.Point, Ellipse> points = new Dictionary<System.Windows.Point, Ellipse>();
                
            foreach (Feature feature in root.features)
            {
                System.Windows.Point point = ToMercator(feature.geometry.coordinates);

                Ellipse ellipse = new Ellipse();
                string info = "Location : " + feature.properties.place + "\n"
                    + "Magnitude : " + feature.properties.mag + "\n"
                    + "Felt by : " + feature.properties.felt + "\n"
                    + "Tsunami Alert : " + feature.properties.tsunami;

                ellipse.ToolTip = info;

                if (feature.properties.alert != null)
                {
                    string color = feature.properties.alert;
                    BrushConverter bc = new BrushConverter();
                    Brush brush = (Brush)bc.ConvertFrom(color);
                    ellipse.Fill = brush;
                }
                else
                {
                    ellipse.Fill = Brushes.Gray;
                }

                points.Add(point, ellipse);   
            }
            return points;
        }

        public RootObject getData(string urll) {
            RootObject rot = new RootObject();

            using (WebClient wc = new WebClient())
            {
                var json = wc.DownloadString(url);

                 rot = JsonConvert.DeserializeObject<RootObject>(json);
            }

            return rot;
        }

        private System.Windows.Point ToMercator(List<double> coordinates)
        {
            double lon0 = imgMap.Margin.Top;
            double lat0 = imgMap.Margin.Left;

            System.Windows.Point mercator = new System.Windows.Point();

            double lat = coordinates[0];
            double lon = coordinates[1];

            double scale_x = imgMap.Width/360;
            double scale_y = imgMap.Height/180;

            mercator.Y = ((90 - lon) * scale_y);
            mercator.X = ((180 + lat) * scale_x);

            return mercator;
        }

        private void Draw(Dictionary<System.Windows.Point, Ellipse> quakeList) {
            //Ellipse ellipse = new Ellipse();
            //ellipse.Fill = Brushes.Green;

            foreach( System.Windows.Point mercator in quakeList.Keys){
                Ellipse ellipse = new Ellipse();
                ellipse = quakeList[mercator];
                ellipse.Width = 10;
                ellipse.Height = ellipse.Width;
                ellipse.StrokeThickness = 2;
                ellipse.MouseEnter += ellipse_MouseEnter;
                ellipse.MouseDown += ellipse_MouseDown; 

                canvas.Children.Add(ellipse);

                Canvas.SetLeft(ellipse, mercator.X);
                Canvas.SetTop(ellipse, mercator.Y);
            }
        }

        private void ellipse_MouseEnter(object sender, MouseEventArgs e)
        {
            
        }

        private void ellipse_MouseDown(object sender, MouseButtonEventArgs e)
        {
         
            MessageBox.Show(root.features[0].properties.place);
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            DateTime start =(DateTime) dpStart.SelectedDate;
            DateTime end = (DateTime)dpEnd.SelectedDate;
            string magnitude =(String) cbMagnitude.Text;

            string full_url = url + "startime=" + start.ToShortDateString() 
                + "&endtime=" + end.ToShortDateString()
                + "&minmagnitude=" + magnitude;

            lblStatus.Content = full_url;

            initialiseData(full_url);

        }
        

    }
}
