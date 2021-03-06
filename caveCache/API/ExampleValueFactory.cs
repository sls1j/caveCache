﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Linq;
using MongoDB.Bson;

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
                    Cave = new CaveInfo(){
                        Name = "CC 34",
                        CaveId = ObjectId.GenerateNewId(),
                        Description = "New Cave"                        
                    },
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
                            CaveId = ObjectId.GenerateNewId(),
                            Description = "A simple cave",
                            Locations = new MongoDb.CaveLocation[]{
                                new MongoDb.CaveLocation(){
                                Latitude = 41.0m, Longitude = -111.0m,Altitude = 1234,
                                Accuracy = 3, AltitudeAccuracy = 19,                                
                                Notes = "Blah blah blah", Source = "My head", Unit = "Imperial"
                            }
                        },
                        CaveData = new MongoDb.Data[0],
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
