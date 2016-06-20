using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using Microsoft.SqlServer.Server;
using System.IO;

using ICEPDCommonModule;
using ICEPDDatabaseModule;

namespace RecordAttendance
{
    public class FingerPrintEntryRecorder
    {

        private static Boolean IsToday(DateTime today, DateTime dt)
        {
            return ( today.Date == dt.Date );
        }

        private static Boolean isEarlierDay(DateTime today, DateTime dt)
        {
            return (today.Date > dt.Date);
        }

        private static Boolean isLaterDay(DateTime today, DateTime dt)
        {
            return (today.Date < dt.Date);
        }

        private static CommonAttendanceDetails AttendanceDetails(String employeeCode, DateTime attendanceDate, DateTime attendanceTime)
        {
            CommonAttendanceDetails ad = new CommonAttendanceDetails();
            ad.EmployeeCode = employeeCode;
            ad.AttendanceDate = attendanceDate;
            ad.InOutTime = attendanceTime;
            return ad;
        }

        [SqlFunction]
        public static void RecordEntry(Int32 eid, Int32 tid, DateTime date, DateTime time)
        {
            SqlConnection conn = new SqlConnection();
            //conn.ConnectionString = "Data Source=ice-db;Initial Catalog=ICE_TEST;User ID=ICEDBUser; pwd = production; Max Pool Size=200;User Instance=false;Integrated Security= True;Enlist=False;";
            conn.ConnectionString = DBInfo.DBConnectionString;
            
            conn.Open();

            // if employee does not exist in database do nothing
            String EmployeeCode = eid.ToString();
            ICEDBEmployee emp = new ICEDBEmployee();
            if (!emp.EmployeeCodeExist(EmployeeCode))
                return;


            // if employee is not active, do not mark attendance 
            DataSet Employee = emp.LoadEmployeesByCode(EmployeeCode);
            Int32 EmployeeID = Convert.ToInt32(Employee.Tables["Employee"].Rows[0]["EmployeeID"]);
            DataSet EmployeeDetails = emp.LoadSelectedEmployee((int)EmployeeID);
            Boolean EmployeeStatus = Convert.ToBoolean(EmployeeDetails.Tables["Table"].Rows[0]["EmployeeStatus"]);

            if (!EmployeeStatus)
                return;




            // Get Last Entry for this user
            SqlCommand AttendanceDetailCommand = new SqlCommand();
            SqlDataAdapter AttendanceDetailAdapter = new SqlDataAdapter();
            DataSet AttendanceDetailData = new DataSet();
            AttendanceDetailCommand.CommandText = "Select InOutID, TrackDate, InOutTime, InOutStatus from AttendanceDetails where ADID = (Select IsNull(Max(ADID),0) from AttendanceDetails where EmployeeID = @EmployeeID)";
            AttendanceDetailCommand.Parameters.AddWithValue("@EmployeeID", (Int32)EmployeeID);
            AttendanceDetailAdapter.SelectCommand = AttendanceDetailCommand;
            AttendanceDetailCommand.Connection = conn;
            AttendanceDetailAdapter.Fill(AttendanceDetailData, "LastTransaction");

            DataRow lastEntry = AttendanceDetailData.Tables["LastTransaction"].Rows[0];
            String lastStatus = Convert.ToString(lastEntry["InOutStatus"]);
            DateTime lastDate = Convert.ToDateTime(lastEntry["TrackDate"]);
            DateTime lastTime = Convert.ToDateTime(lastEntry["InOutTime"]);
            lastTime = lastDate + lastTime.TimeOfDay;

            ICEDBAttendance att = new ICEDBAttendance();

            // if current entry is OUT (2)
            if (tid == 2)
            {
                if (lastStatus == "In")
                {
                    // if current entry is OUT and last Entry is IN (same day) ... mark attendance! 
                    if (IsToday(date, lastDate))
                    {
                        att.RecordAttendance(AttendanceDetails(EmployeeCode, date, time));
                    }
                    else // if current entry is OUT and last Entry is IN (previous day) ... mark out at 12am that day
                    {
                        DateTime inOutTime = lastDate + new TimeSpan(23, 59, 59);
                        att.RecordAttendance(AttendanceDetails(EmployeeCode, lastDate, inOutTime));
                    }
                }
                else
                {
                    if (IsToday(date, lastDate))
                    {
                        // if current entry is OUT and last entry is OUT on same day ... mark in one hour after last out or before now and out now
                        // mark in 
                        DateTime inOutTime = lastTime + new TimeSpan(1, 0, 0);
                        if (inOutTime > time)
                        {
                            inOutTime = time + new TimeSpan(0, -1, 0);
                        }
                        att.RecordAttendance(AttendanceDetails(EmployeeCode, date, inOutTime));

                        // mark out
                        att.RecordAttendance(AttendanceDetails(EmployeeCode, date, time));
                    }
                    else // if current entry is OUT and last entry is OUT on some previous day ... mark late today
                    {
                        // mark in 
                        DateTime inOutTime = date + new TimeSpan(11, 59, 0);
                        if (inOutTime > time)
                        {
                            inOutTime = time + new TimeSpan(0, -1, 0);
                        }
                        att.RecordAttendance(AttendanceDetails(EmployeeCode, date, inOutTime));

                        // mark out
                        att.RecordAttendance(AttendanceDetails(EmployeeCode, date, time));
                    }
                }
            }
            else // if current entry is IN
            {
                // if current entry is IN and last entry is OUT ... mark attendance!
                if (lastStatus == "Out")
                { 
                    att.RecordAttendance(AttendanceDetails(EmployeeCode, date, time));
                }
                else
                {

                    // if current entry is IN and last entry is IN on some previous day
                    // ... out 6 hours after in or at 1159 pm and then IN NOW
                    if (isEarlierDay(date, lastDate))
                    {
                        // mark out
                        DateTime inoutTime = lastTime + new TimeSpan(6, 0, 0);
                        if (isLaterDay(lastDate, inoutTime))
                        {
                            inoutTime = lastDate + new TimeSpan(11, 59, 59);
                        }
                        att.RecordAttendance(AttendanceDetails(EmployeeCode, lastDate, inoutTime));

                        // mark in
                        att.RecordAttendance(AttendanceDetails(EmployeeCode, date, time));
                    }
                    // if current entry is IN and last entry is IN on sameday ... ignore
                }
            }

            conn.Close();
            return;
        }
    }
}
