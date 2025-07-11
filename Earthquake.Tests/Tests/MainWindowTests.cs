using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media.Imaging;
using Moq;
using Moq.Protected;
using QuakeData;
using EarthQuake;
using Xunit;

namespace Earthquake.Tests
{
    public class MainWindowTests
    {
        private MainWindow CreateWindow()
        {
            var window = new MainWindow();
            // initialize components are not called in tests
            window.imgMap = new Image { Width = 360, Height = 180, Margin = new Thickness(0) };
            window.dpStart = new DatePicker();
            window.dpEnd = new DatePicker();
            window.cbMagnitude = new ComboBox();
            return window;
        }

        [Fact]
        public void UrlBuilder_SameDay_GeneratesMinimalUrl()
        {
            var window = CreateWindow();
            window.dpStart.SelectedDate = new DateTime(2020, 1, 1);
            window.dpEnd.SelectedDate = new DateTime(2020, 1, 1);
            window.cbMagnitude.Text = "3";

            var url = window.urlBuilder();
            Assert.Contains("starttime=2020-01-01", url);
            Assert.DoesNotContain("endtime", url);
        }

        [Fact]
        public void UrlBuilder_Range_GeneratesFullUrl()
        {
            var window = CreateWindow();
            window.dpStart.SelectedDate = new DateTime(2020, 1, 1);
            window.dpEnd.SelectedDate = new DateTime(2020, 1, 2);
            window.cbMagnitude.Text = "2";

            var url = window.urlBuilder();
            Assert.Contains("starttime=2020-01-01", url);
            Assert.Contains("endtime=2020-01-02", url);
            Assert.Contains("minmagnitude=2", url);
        }

        [Fact]
        public void ToMercator_MapsCoordinates()
        {
            var window = CreateWindow();
            var point = window.ToMercator(new List<double> { 0, 0, 0 });
            Assert.Equal(180, point.X, 1);
            Assert.Equal(90, point.Y, 1);
        }

        [Fact]
        public void GetQuake_TransformsFeaturesToDictionary()
        {
            var window = CreateWindow();
            var root = new RootObject
            {
                features = new List<Feature>
                {
                    new Feature
                    {
                        geometry = new Geometry { coordinates = new List<double>{0,0,0} },
                        properties = new Properties { place = "x", mag = 1, felt = 2, tsunami = 0 }
                    }
                }
            };
            typeof(MainWindow).GetField("root", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                .SetValue(window, root);
            var dict = window.getQuake();
            Assert.Single(dict);
        }

        [Fact]
        public async Task GetDataAsync_ParsesJson()
        {
            var handler = new Mock<HttpMessageHandler>();
            handler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"features\":[],\"metadata\":{}}")
                });

            var client = new HttpClient(handler.Object);
            var window = CreateWindow();
            typeof(MainWindow).GetField("httpClient", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)!
                .SetValue(null, client);

            var result = await window.GetDataAsync("http://example");
            Assert.NotNull(result);
            Assert.Empty(result.features);
        }
    }
}
