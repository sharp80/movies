using System;
using System.Data;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;

public class DbControl
{
    protected string m_cnString;
    protected DataSet m_dataSet;
    protected MySqlDataAdapter m_dr;
    protected MySqlDataAdapter m_dr1;
    protected MySqlConnection m_cn;
    public DbControl()
    {
        m_cnString = ConfigurationSettings.AppSettings["connectionstring"].ToString();
      //  Response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);
        string sqlString = "Select * from movies_details";

        m_cn = new MySqlConnection(m_cnString);
        m_dr = new MySqlDataAdapter(sqlString, m_cn);
        m_dataSet = new DataSet();
        m_dr.Fill(m_dataSet, "movies_details");

        sqlString = "Select * from movies_times";
        m_dr1 = new MySqlDataAdapter(sqlString, m_cn);
        m_dr1.Fill(m_dataSet, "movies_times");
    }

    public bool hasMovie(string movieId)
    {
        foreach (DataRow row in m_dataSet.Tables[0].Rows)
        {
            if (row["ID"].Equals(movieId))
            {
                return true;
            }
        }
        return false;
    }

    public void addMovieTimes(int movieId, List<DateTime> timesList, int locationId)
    {
        foreach (DateTime time in timesList)
        {
            bool alreadyHasTimeInDb = false;
            foreach (DataRow row in m_dataSet.Tables["movies_times"].Rows)
            {
                DateTime dbTime = DateTime.Parse(row["movieTime"].ToString());
                if (row["movieId"].Equals(movieId) && dbTime.Equals(time))
                {
                    alreadyHasTimeInDb = true ;
                    break;
                }
            }
            if (alreadyHasTimeInDb == false)
            {
                // Create the InsertCommand.
                MySqlCommand command = new MySqlCommand(
                    "INSERT INTO movies_times (movieId) " +
                    "VALUES (@movieId)", m_cn);
                command.Parameters.Add("@movieId", SqlDbType.Int);

                //  m_dr1.InsertCommand = command;

                DataRow newRow = m_dataSet.Tables[1].NewRow();
                newRow["movieId"] = movieId;
                newRow["movieTime"] = time;
                newRow["locationId"] = locationId;
                //  newRow.SetAdded();
                m_dataSet.Tables["movies_times"].Rows.Add(newRow);
                /*newRow.AcceptChanges();
                m_dataSet.Tables[1].AcceptChanges();
                m_dataSet.AcceptChanges();*/
                //  m_dr1.Update(m_dataSet,"movies_times");

                MySqlCommandBuilder projectBuilder = new MySqlCommandBuilder(m_dr1);
                //   DataSet newSet = m_dataSet.GetChanges(DataRowState.Added);
                m_dr1.Update(m_dataSet, "movies_times");
            }
        }
    }
    public bool addMovie(string movieId, string movieNameHebrew, string movieNameEnglish, string movieIcon)
    {
        string insertSqlString = "insert into movies_details(ID, MovieNameHebrew, MovieNameEnglish, Icon) value(" +
                                                    "'" + movieId + "'" +
                                                    ",'" + movieNameHebrew + "'" +
                                                    ",'" + movieNameEnglish + "'" +
                                                    ",'" + movieIcon + "'" +
                                                    ")";
        MySqlConnection cn = new MySqlConnection(m_cnString);
        bool res;
        try
        {
            cn.Open();
            MySqlCommand command = new MySqlCommand(insertSqlString, cn);
            command.ExecuteNonQuery();
            res= true;
        }
        catch (Exception _Exception)
        {
            //Response.Write(_Exception.ToString());
            //_Exception.ToString();
            res = false;
        }
        finally
        {
            cn.Close();
        }
        return res;
    }
}
