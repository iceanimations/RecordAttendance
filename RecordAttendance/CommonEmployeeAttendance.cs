using System;
using System.Collections.Generic;
using System.Text;

namespace ICEPDCommonModule
{
    [Serializable]
    public class CommonEmployeeAttendance
    {
        private DateTime mAttendanceDate;
        public DateTime AttendanceDate
        {
            get { return mAttendanceDate; }
            set { mAttendanceDate = value; }
        }

        private String mEmployeeCode;
        public String EmployeeCode
        {
            get { return mEmployeeCode; }
            set { mEmployeeCode = value; }
        }

        private String mAttendanceStatus;
        public String AttendanceStatus
        {
            get { return mAttendanceStatus; }
            set { mAttendanceStatus = value; }
        }

        private String mDescription;
        public String Description
        {
            get { return mDescription; }
            set { mDescription = value; }
        }
    }
}
