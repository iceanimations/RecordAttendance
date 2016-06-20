using System;
using System.Collections.Generic;
using System.Text;

namespace ICEPDCommonModule
{
    [Serializable]
    public class CommonAttendanceDetails : CommonEmployeeAttendance
    {
        private Int32 mADID;
        public Int32 ADID
        {
            get { return mADID; }
            set { mADID = value; }
        }

        private DateTime mTrackDate;
        public DateTime TrackDate
        {
            get { return mTrackDate; }
            set { mTrackDate = value; }
        }

        private DateTime mInOutTime;
        public DateTime InOutTime
        {
            get { return mInOutTime; }
            set { mInOutTime = value; }
        }

        private String mInOutStatus;
        public String InOutStatus
        {
            get { return mInOutStatus; }
            set { mInOutStatus = value; }
        }        
    }
}
