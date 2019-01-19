using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace caveCache.MongoDb
{
  public class Cave
  {
    public ObjectId Id { get; set; }
    public bool Saved { get; set; }
    public DateTime CreatedDate { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DateDeleted { get; set; }
    public List<ObjectId> DataTemplates { get; set; }
    public List<Data> Data { get; set; }
    public List<CaveLocation> Locations { get; set; }
    public List<CaveNote> Notes { get; set; }
    public List<CaveSurvey> Surveys { get; set; }

    public Cave()
    {
    }
  }

  public class CaveUser
  {
    public ObjectId Id { get; set; }
    public ObjectId UserId { get; set; }
    public ObjectId CaveId { get; set; }
    public string Note { get; set; }
  }

  public class CaveLocation
  {
    public bool IsActive { get; set; }
    public DateTime? CaptureDate { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public decimal? Altitude { get; set; }
    public decimal? Accuracy { get; set; }
    public decimal? AltitudeAccuracy { get; set; }
    public string Unit { get; set; }
    public string Source { get; set; }
    public string Notes { get; set; }

    public CaveLocation Clone()
    {
      var cloned = base.MemberwiseClone() as CaveLocation;
      return cloned;
    }
  }

  public class CaveNote
  {
    public DateTime CreatedDate { get; set; }
    public string Note { get; set; }
    public ObjectId UserId { get; set; }
    public string Summary { get; set; }

    public CaveNote Clone()
    {
      return MemberwiseClone() as CaveNote;
    }
  }

  public class CaveSurvey
  {
    public string Name { get; set; }
    public List<SurveyNote> Notes { get; set; }
    public DateTime SurveyDate { get; set; }
    public List<SurveyPoint> SurveyPoints { get; set; }
  }

  public class SurveyNote
  {
    public DateTime CreationTime { get; set; }
    public int UserId { get; set; }
    public string Notes { get; set; }
  }

  public class SurveyPoint
  {
    public string Name { get; set; }
    public string ToName { get; set; }
    public double ToDistance { get; set; }
    public double ToHeading { get; set; }
    public double ToInclination { get; set; }
    public string BackName { get; set; }
    public double BackDistance { get; set; }
    public double BackHeading { get; set; }
    public double BackInclination { get; set; }
    public double UpDistance { get; set; }
    public double LeftDistance { get; set; }
    public double DownDistance { get; set; }
    public double RightDistance { get; set; }
  }


  public static class Units
  {
    public const string Emprial = "Emperial";
    public const string Metric = "Metric";
  }
}
