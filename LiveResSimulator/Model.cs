using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveResSimulator
{
    [Serializable]
    public class Event
    {
        public Runner[] Runners { get; set; }
        public Result[] Results { get; set; }
        public RadioControl[] RadioControls { get; set; }

        public string CompName { get; set; }
        public string organizer { get; set; }
        public DateTime CompDate { get; set; }
        public bool Public { get; set; }
        public int CompId { get; set; }
    }
    [Serializable]
    public class Runner
    {
        public int dbId { get; set; }
        public string Name { get; set; }
        public string club { get; set; }
        public string Class { get; set; }
    }
    [Serializable]
    public class Result
    {
        public int dbId { get; set; }
        public int control { get; set; }
        public int time { get; set; }
        public int status { get; set; }
        public DateTime changed { get; set; }
    }
    [Serializable]
    public class RadioControl
    {
        public string classname { get; set; }
        public string name { get; set; }
        public int code { get; set; }
        public int corder { get; set; }
    }
}
