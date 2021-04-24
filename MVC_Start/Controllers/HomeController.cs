using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MVC_Start.Models;
using Newtonsoft.Json;
using System.Net.Http;
using MVC_Start.DataAccess;

namespace MVC_Start.Controllers
{
    public class HomeController : Controller
    {

        public ApplicationDbContext dbContext;

        public HomeController(ApplicationDbContext context)
        {
            dbContext = context;
        }

        HttpClient httpClient;
        HttpClient httpClient2;



        static string BASE_URL = "https://api.data.gov/ed/collegescorecard/v1/";
        static string API_KEY = "9dbWmdA3TMdPpeGvydxxHox4QPt3cTtUSLrWEait";

        public IActionResult Index()
        {
            return View();
        }

        //public IActionResult IndexWithLayout()
        //{
        //    return View();
        //}

        //public IActionResult Contact()
        //{
        //    GuestContact contact = new GuestContact();

        //    contact.Name = "Manish Agrawal";
        //    contact.Email = "magrawal@usf.edu";
        //    contact.Phone = "813-974-6716";


        //    /* alternate syntax to initialize object 
        //    GuestContact contact2 = new GuestContact
        //    {
        //      Name = "Manish Agrawal",
        //      Email = "magrawal@usf.edu",
        //      Phone = "813-974-6716"
        //    };
        //    */

        //    //ViewData["Message"] = "Your contact page.";

        //    return View(contact);
        //}

        //[HttpPost]
        //public IActionResult Contact(GuestContact contact)
        //{
        //    return View(contact);
        //}

        /// <summary>
        /// Replicate the chart example in the JavaScript presentation
        /// 
        /// Typically LINQ and SQL return data as collections.
        /// Hence we start the example by creating collections representing the x-axis labels and the y-axis values
        /// However, chart.js expects data as a string, not as a collection.
        ///   Hence we join the elements in the collections into strings in the view model
        /// </summary>
        /// <returns>View that will display the chart</returns>
        //public ViewResult DemoChart()
        //{
        //    string[] ChartLabels = new string[] { "Africa", "Asia", "Europe", "Latin America", "North America" };
        //    int[] ChartData = new int[] { 2478, 5267, 734, 784, 433 };

        //    ChartModel Model = new ChartModel
        //    {
        //        ChartType = "bar",
        //        Labels = String.Join(",", ChartLabels.Select(d => "'" + d + "'")),
        //        Data = String.Join(",", ChartData.Select(d => d)),
        //        Title = "Predicted world population (millions) in 2050"
        //    };

        //    return View(Model);



        //}



        public IActionResult AboutUs()
        {
            return View();


        }

        public string replaceString(string responseString)
        {
            string responseStringModified = responseString;
            responseStringModified = responseStringModified.Replace("school.name", "schoolname");
            responseStringModified = responseStringModified.Replace("school.city", "schoolcity");
            responseStringModified = responseStringModified.Replace("school.state", "schoolstate");
            responseStringModified = responseStringModified.Replace("school.zip", "schoolzip");
            responseStringModified = responseStringModified.Replace("latest.programs.cip_4_digit", "data");

            return responseStringModified;
        }


        public IActionResult Searchuniv()
        {
            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Add("X-Api-Key", API_KEY);
            httpClient.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));



            string SCHOOL_DATA_API_PATH = BASE_URL + "schools?fields=id,ope6_id,school.name,school.city,school.state,school.zip&per_page=3000";

            string schooldata = "";


            Schools schools = null;


            httpClient.BaseAddress = new Uri(SCHOOL_DATA_API_PATH);

            try
            {
                HttpResponseMessage response = httpClient.GetAsync(SCHOOL_DATA_API_PATH).GetAwaiter().GetResult();

                if (response.IsSuccessStatusCode)
                {
                    schooldata = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    schooldata = replaceString(schooldata);
                }

     

                if (!schooldata.Equals(""))
                {

                    schools = JsonConvert.DeserializeObject<Schools>(schooldata);
                    schools.results.ToList().ForEach(s =>
                    {
                        var school = new SchoolData()
                        {
                            //id = s.id,
                            ope6_id = s.ope6_id,


                            schoolcity = s.schoolcity,
                            schoolname = s.schoolname,
                            schoolstate = s.schoolstate,
                            schoolzip = s.schoolzip


                        };

                        dbContext.Schools.Add(school);
                    });
                    dbContext.SaveChanges();


                }
            }
            catch (Exception e)
            {

                Console.WriteLine(e.Message);
            }

            return View(schools);
        }

        public IActionResult Programdetails()
        {
            httpClient2 = new HttpClient();
            httpClient2.DefaultRequestHeaders.Accept.Clear();
            httpClient2.DefaultRequestHeaders.Add("X-Api-Key", API_KEY);
            httpClient2.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            string Program_DATA_PARK_API_PATH = BASE_URL + "schools?fields=latest.programs.cip_4_digit.code,latest.programs.cip_4_digit.ope6_id,latest.programs.cip_4_digit.unit_id,latest.programs.cip_4_digit.title&per_page=3000";



            string programdata = "";

            Programs programs = null;



            httpClient2.BaseAddress = new Uri(Program_DATA_PARK_API_PATH);

            try
            {
                HttpResponseMessage response2 = httpClient2.GetAsync(Program_DATA_PARK_API_PATH).GetAwaiter().GetResult();

                if (response2.IsSuccessStatusCode)
                {
                    programdata = response2.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    programdata = replaceString(programdata);

                }

                if (!programdata.Equals(""))
                {

                    programs = JsonConvert.DeserializeObject<Programs>(programdata);
                    var i = 1;

                    programs.results.ToList().ForEach(r =>
                    {
                        r.data.ToList().ForEach(p =>
                        {
                            var program = new ProgramData()
                            {
                                //ID = i++,
                            code = p.code,
                            title = p.title,
                            unit_id = p.unit_id,
                            ope6_id = p.ope6_id,


                        };


                        dbContext.Programs.Add(program);
                    });
                });
                    dbContext.SaveChanges();



                }
            }
            catch (Exception e)
            {

                Console.WriteLine(e.Message);
            }

            return View(programs);
        }


        public IActionResult Update()
        {
            return View();
        }

        public IActionResult Delete()
        {
            return View();
        }

        public IActionResult Createnew()
        {
            return View();
        }

    }
}