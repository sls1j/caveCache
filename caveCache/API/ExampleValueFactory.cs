using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Linq;

namespace caveCache.API
{
    class ExampleValueFactory
    {
        const string _sessionId = "x2342lijs089j23";
        private object[] _examples;
        public ExampleValueFactory()
        {
            _examples = new object[]{
                new LoginRequest(){
                    RequestId = 1,
                    Email = "george@fred.com",
                    Password = "passwords are great"
                },
                new LoginResponse(){
                    RequestId = 1,
                    SessionId = _sessionId,
                    StatusDescription = "OK",
                    Status = (int)HttpStatusCode.OK
                },
                new CaveCreateRequest(){
                    RequestId = 2,
                    SessionId = _sessionId
                },
                new CaveCreateResponse(){
                    RequestId = 2,
                    SessionId = _sessionId,
                    CaveId = 1,
                    StatusDescription = "OK",
                    Status = (int)HttpStatusCode.OK
                },
                new CaveListByLocationRequestion(){
                    RequestId = 212,
                    SessionId = _sessionId,
                    Distance = 150,
                    Unit = "Emperial",
                    Latitude = -41.0m,
                    Longitude = -111.0m
                },
                new CaveListByLocationResponse(){
                    RequestId = 212,
                    SessionId = _sessionId,
                    StatusDescription = "OK",
                    Status = (int)HttpStatusCode.OK,
                    Caves = new CaveInfo[]{
                        new CaveInfo(){
                            CaveId = 1,
                            Description = "A simple cave",
                            LocationId = 1,
                            Locations = new Database.CaveLocation[]{
                                new Database.CaveLocation(){
                                LocationId = 1,
                                Latitude = 41.0m, Longitude = -111.0m,Altitude = 1234,
                                Accuracy = 3, AltitudeAccuracy = 19,
                                CaveId = 1, CaptureDate = DateTime.Now,
                                Notes = "Blah blah blah", Source = "My head", Unit = "Imperial"
                            }
                        },
                        CaveData = new Database.Data[0],
                        Media = new Database.Media[0],
                        Name = "Simple Cave"
                        }
                    }
                }
            };
        }


        public string GetExampleValue(Type messageType)
        {
            object example = _examples.FirstOrDefault(o => o.GetType() == messageType);

            if (null == example)
            {
                var con = messageType.GetConstructor(new Type[0]);
                example = con.Invoke(new object[0]);
            }
            return JsonConvert.SerializeObject(example, Formatting.Indented);
        }
    }
}
