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
using System.Text.RegularExpressions;
using System.Text;
using System.Collections.Generic;
/*
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
*/
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

		string page = readPage1("http://movies.walla.co.il");
		//string filterArr[] = page.Split('dropdown_selection'); // all code before dropdown_selection is unneeded
		string[] pageLines = page.Split('\n');
		foreach (string line in pageLines)
		{
			string strToFind = "onclick=\"location.href='/movie/";
			int index = line.IndexOf(strToFind);
			string movieId ;
			if ( index > 0 ) {	
				movieId = line.Substring(index+strToFind.Length,4);
				Response.Write(movieId + "<BR>");
				
				string findCinemasUrl = @"http://movies.walla.co.il/?w=/@ajax.movie.projections&movie_id=" + movieId;
				//http://movies.walla.co.il/?w=/@ajax.movie.projection.time&movie_id=4673&cinema_id=38
				string cinemasPage = readPage1(findCinemasUrl);
				string[] cinemasPageLines = cinemasPage.Split('\n');
				List<string> cinemasList = new List<string>();
				List<string> cinemasIdList = new List<string>();
				
				foreach (string cinemasLine in cinemasPageLines) {
					string cinemasLineStrip = cinemasLine.ToLower();
					strToFind = "\"w3b\"";
					int indexStart = cinemasLineStrip.IndexOf(strToFind);
					int indexEnd = cinemasLineStrip.IndexOf("</a>");
					string cinemaLineSuf ;
					if ( indexEnd > 0 ){
						cinemaLineSuf = cinemasLineStrip.Substring(indexStart+strToFind.Length,indexEnd-(indexStart+strToFind.Length));
						//MatchCollection matches = Regex.Matches("[a-zA-Z0-9]*", cinemasLineStrip, RegexOptions.IgnoreCase);
						MatchCollection matches = Regex.Matches(cinemaLineSuf, "[a-zà-ú\\- ]*", RegexOptions.IgnoreCase);
						if ( indexStart > 0 && indexEnd > 0  ){
							foreach ( Match match1 in matches ){
								string cinema = match1.Value;
								if (cinema.Length > 1){
									cinemasList.Add(cinema);
									Response.Write(cinema);
								}
							}
						}
					}
					strToFind="/?w=/";
					indexStart = cinemasLineStrip.IndexOf(strToFind);
					indexEnd = cinemasLineStrip.IndexOf("/@cinema");
					if ( indexStart > 0 && indexEnd > 0 ){
						string cinemaId = cinemasLineStrip.Substring(indexStart+strToFind.Length,indexEnd-(indexStart+strToFind.Length));
						cinemasIdList.Add(cinemaId);
						Response.Write(cinemaId + "<BR>");
						
						string timesPage = readPage1(@"http://movies.walla.co.il/?w=/@ajax.movie.projection.time&movie_id="+movieId+"&cinema_id="+cinemaId);
						Response.Write(timesPage+"<BR>");
						/*string[] timesPageLines = timesPage.Split('\n');
						foreach( string timesLine in timesPageLines) {
							MatchCollection daysCollection = Regex.Matches(timesLine.Replace(" ",""), "[à-ú,]*", RegexOptions.IgnoreCase);
							foreach ( Match dayMatch in daysCollection ){
								string day = dayMatch.Value;
								Response.Write("Day: " + day + "<BR>");
							}
						}*/
					}
				}
			}
			strToFind = "</span>";
			index = line.IndexOf(strToFind);
			Match match = Regex.Match(line, "[à-ú]");
			if (  line.Replace("\t","").Replace(" ","").StartsWith(strToFind) && match.Success && index > 0 && line.Length > index+strToFind.Length) {	
				string name = line.Substring(index+strToFind.Length,line.Length-(index+strToFind.Length));
			//	Response.Write(name + "<BR>");
			//	Response.Write("found: " + line.Replace("<","") + "<br>");
			}
			
		}
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
	
	private string readPage1(string url)
	{
			//Create a Web-Request to an URL
		HttpWebRequest HWR_Request = (HttpWebRequest)WebRequest.Create(url);

		//Defined poperties for the Web-Request
		HWR_Request.Method = "POST";
		HWR_Request.MediaType = "HTTP/1.1";
		HWR_Request.ContentType = "text/xml";
		HWR_Request.UserAgent = "SNIP_CLIENT/1.0";

		//Defined data for the Web-Request
		byte[] b_Data = Encoding.ASCII.GetBytes("A string you would like to send");
		HWR_Request.ContentLength = b_Data.Length;

		//Attach data to the Web-Request
		Stream S_DataStream = HWR_Request.GetRequestStream();
		S_DataStream.Write(b_Data, 0, b_Data.Length);
		S_DataStream.Close();

		//Send Web-Request and receive a Web-Response
		HttpWebResponse HWR_Response = (HttpWebResponse)HWR_Request.GetResponse();

		//Translate data from the Web-Response to a string
		S_DataStream = HWR_Response.GetResponseStream();
		StreamReader SR_DataStream = new StreamReader(S_DataStream, Encoding.GetEncoding("windows-1255"));
		string s_ResponseString = SR_DataStream.ReadToEnd();
		return s_ResponseString;
	}
}

