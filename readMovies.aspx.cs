using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using MySql.Data.MySqlClient;
using System.Data.SqlClient;

using System.Data.Odbc;
using System.Net.Mail;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Configuration;
using System.Net;


public class Pr
{
    public string dt { get; set; }
    public string tm { get; set; }
    public int sb { get; set; }
    public string pc { get; set; }
    public int at { get; set; }
    public int vt { get; set; }
    public bool? td { get; set; }
    public int? db { get; set; }
}

public class Fe
{
    public string dc { get; set; }
    public string fn { get; set; }
    public List<Pr> pr { get; set; }
}

public class Site
{
    public string sn { get; set; }
    public string tu { get; set; }
    public int vt { get; set; }
    public List<Fe> fe { get; set; }
    public int si { get; set; }
}

public class RootObject
{
    public List<Site> sites { get; set; }
    public List<string> venueTypes { get; set; }
}

public partial class readMovies : System.Web.UI.Page
{
    protected string cnString ;
    
	private bool CanSetWager( string WagerId )
	{
		string sqlString = "Select WagerState from Wagers where Id="+WagerId;
			
		MySqlConnection cn = new MySqlConnection(cnString);
		MySqlDataAdapter dr = new MySqlDataAdapter();
  	    MySqlCommandBuilder cb = new MySqlCommandBuilder(dr);
		dr.SelectCommand = new MySqlCommand(sqlString, cn);
		DataSet ds = new DataSet();
		dr.Fill(ds, "table" );
		if (ds.Tables["table"].Rows.Count == 0)
			return false;
		
		string wagerState = ds.Tables[0].Rows[0]["WagerState"].ToString();
		if ( wagerState == "beforeGame" )
		{
			return true;
		}
		return false;	
	}
	
	private void Page_Load(object sender, System.EventArgs e)
	{
		cnString  = ConfigurationSettings.AppSettings["connectionstring"].ToString();
		Response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);

		string json = readPage("http://www.cinema-city.co.il/presentationsJSON");
		RootObject deserialized = 
            JsonConvert.DeserializeObject<RootObject>(json);
		for (int ind = 0 ; ind < deserialized.sites.Count ; ind++)
		{
			Response.Write( deserialized.sites[ind].sn + "<BR>");
			for (int film_ind = 0 ; film_ind < deserialized.sites[ind].fe.Count ; film_ind++)
			{
				Response.Write( deserialized.sites[ind].fe[film_ind].fn + "<BR>");
			}
			Response.Write("<BR>");
		}
		/*int UserId = deserialized.uId;
		string sqlString = "Select * from UsersWagers where UserId="+UserId;
			
		MySqlConnection cn = new MySqlConnection(cnString);
		MySqlDataAdapter dr = new MySqlDataAdapter();
  	    MySqlCommandBuilder cb = new MySqlCommandBuilder(dr);
		dr.SelectCommand = new MySqlCommand(sqlString, cn);
		DataSet ds = new DataSet();
		dr.Fill(ds, "table");
		
		bool canWage = CanSetWager( deserialized.wagers[0].WagerId.ToString() );
		if ( canWage == false)
			return;
		
		for (int ind = 0 ; ind < deserialized.wagers.Count ; ind++)
		{
			bool foundWager = false;
			DataRow newRow = null;
			foreach (  DataRow row in ds.Tables[0].Rows )
			{
				if ( deserialized.wagers[ind].WagerId == Convert.ToInt32(row["WagerId"]))
				{
					foundWager = true;
					newRow = row;
					Response.Write("update<BR>");
					break;
				}
			}

			if (!foundWager )
			{// If wager is not aleady in the DB adding new wager
				newRow = ds.Tables[0].NewRow();
				Response.Write("new<BR>");
			}

			newRow["Wager1"]  = deserialized.wagers[ind].WagedEntry1;
			newRow["Wager2"]  = deserialized.wagers[ind].WagedEntry2;
			newRow["WagerId"] = deserialized.wagers[ind].WagerId;
			newRow["UserId"] = UserId;
			Response.Write( newRow["Wager1"] + " " + newRow["Wager2"] + " " + newRow["WagerId"] + "<BR>" );			
			Response.Write("UserId " + UserId+ "<BR>");
			if (!foundWager )
				ds.Tables[0].Rows.Add( newRow );
		}

		
		
		//dr.InsertCommand = new MySqlCommand( "insert into UsersWagers (Wager1 , Wager1 , WagerId , UserId)"+
		//									"VALUES (@Wager1 , @Wager1 , @WagerId , @UserId)", cn);
		dr.Update(ds,"table");


		/*
		//string jsonString = JsonConvert.SerializeObject(ds.Tables[0]);
		//Response.Write(jsonString);
		if ( ds.Tables[0].Rows.Count == 0 )
		{
			string insertSqlString = "insert into users(extId) value(" +
													"'" + _UId + "'"+
													")";

			try
			{
				cn.Open();
				MySqlCommand command = new MySqlCommand(insertSqlString, cn);
				command.ExecuteNonQuery();				
				// get generated user id from DB
				dr.Fill(ds);				
			}
			catch (Exception _Exception)
			{
				//Response.Write(_Exception.ToString());
				//_Exception.ToString();
			}
			finally
			{
				cn.Close();
			}
		}
		
		string jsonString = JsonConvert.SerializeObject(ds.Tables[0]);
		Response.Write(jsonString);			
		*/
	}
	
	private string readPage(string url)
	{
		WebRequest request = WebRequest.Create(url);
		request.Timeout = 5000; // msec
		request.Credentials = CredentialCache.DefaultCredentials;
		WebResponse response = request.GetResponse();

		try
		{
			Stream dataStream = response.GetResponseStream ();
			StreamReader reader = new StreamReader (dataStream);
			
			// Read the content.
			string responseFromServer = reader.ReadToEnd ();
			reader.Close();
			dataStream.Flush();
			dataStream.Close();
			response.Close();
			//Response.Write(responseFromServer);
			return responseFromServer;
		}
		catch (WebException we) 
		{
			Response.Write(we);
		}
		catch( Exception x )
		{
			Response.Write(x);	
		}
		return null;
	}
}

