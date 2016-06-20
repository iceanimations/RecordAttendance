using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RecordAttendance;

namespace TestRecordAttendance
{
    class Program
    {
        static void Main(string[] args)
        {
            int eid = 96000540;
            int tid = 1;
            DateTime date = new DateTime(2016, 6, 13);
            DateTime time = new DateTime(2016, 6, 13, 14, 33, 0);

            FingerPrintEntryRecorder.RecordEntry(eid, tid, date, time);
        }
    }
}
