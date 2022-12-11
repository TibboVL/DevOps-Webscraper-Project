using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Text.Json;



namespace DevOps_Project_Webscraper_Tibbo_Van_Leemput
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            String GlobalPath = Directory.GetCurrentDirectory();

            if (!Directory.Exists(GlobalPath + @"\Downloads"))
            {
                Directory.CreateDirectory(GlobalPath + @"\Downloads");
            }

            if (!Directory.Exists(GlobalPath + @"\Json"))
            {
                Directory.CreateDirectory(GlobalPath + @"\Json");
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n---------------------------------------------------------");
            Console.WriteLine("To be able to use the download functionality of this application\nyou must have YOUTUBEDLP AND FFMPEG installed and added to your path!!");
            Console.WriteLine("Youtubedlp: https://github.com/yt-dlp/yt-dlp");
            Console.WriteLine("ffmpeg: https://phoenixnap.com/kb/ffmpeg-windows");
            Console.WriteLine("---------------------------------------------------------\n");
            Console.ResetColor();
            Thread.Sleep(1000);


            try
            {
                ShowMenu();

                static void Selections(string Selection)
                {
                    switch (Selection)
                    {
                        case "1":
                            List<string> MSongList = MNMSongList();
                            DownloadSongs(MSongList);
                            break;
                        case "2":
                            List<string> SongList = SpotifySongList();
                            DownloadSongs(SongList);
                            break;
                        case "3":
                            List<string> CSSongList = SpotifySongList(true);
                            DownloadSongs(CSSongList);
                            break;
                        case "4":
                            GetBaseData();
                            break;
                        case "5":
                            GetJobs();
                            break;
                        case "q" or "Q":
                            Environment.Exit(0);
                            break;
                        default:
                            ShowMenu();
                            Console.WriteLine("Invalid option!");
                            break;
                    }
                    ShowMenu();
                }

                static void ShowMenu()
                {
                    Console.WriteLine("\n--------------------------------------------------------");
                    Console.WriteLine("Welcome to my Music downloader");
                    Console.WriteLine("Please choose one of the options below\n");
                    Console.WriteLine("1: Download MNM UltraTop50");
                    Console.WriteLine("2: Download Spotify Top 50");
                    Console.WriteLine("3: Download a custom spotify playlist (max 700 songs)");
                    Console.WriteLine("4: Scrapen van de basisdata van 5 recentste videos");
                    Console.WriteLine("5: Scrapen van 5 recentste jobs op ictjobs.be");
                    Console.WriteLine("Q: Exit");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("\n(Repetition of same task can cause websites to temporary\nlimit/block traffic which causes the script issues!)");
                    Console.ResetColor();
                    Console.WriteLine("--------------------------------------------------------\n");

                    var response = Console.ReadLine();
                    Console.Clear();
                    Selections(response.ToString());
                }


                static void SaveToJson(List<List<String>> ListOfLists, String KindOfData)
                {

                    DateTime now = DateTime.Now;
                    var partOfFilename = now.ToString().Replace("/", "").Replace(":", "").Replace(" ", "");

                    string path = Directory.GetCurrentDirectory() + @"\Json\" + KindOfData + partOfFilename + ".json";
                    //var path = @"C:\Users\Tibbo\source\repos\DevOps Project Webscraper Tibbo Van Leemput\DevOps Project Webscraper Tibbo Van Leemput\Json\" + KindOfData + partOfFilename + ".json";



                    Console.WriteLine("\nSaving data to JSON as " + KindOfData + partOfFilename + "\n");

                    if (KindOfData == "Youtube")
                    {

                        List<YouTube> _data = new List<YouTube>();

                        foreach (var List in ListOfLists)
                        {
                            _data.Add(new YouTube()
                            {
                                title = List[0],
                                vieuws = List[1],
                                upload = List[2],
                                channel = List[3]
                            });
                        }

                        string json = JsonSerializer.Serialize(_data);
                        File.WriteAllText(path, json);
                    }
                    else if (KindOfData == "Jobs")
                    {

                        List<Jobs> _JobData = new List<Jobs>();

                        foreach (var List in ListOfLists)
                        {
                            _JobData.Add(new Jobs()
                            {
                                JobTitle = List[0],
                                Company = List[1],
                                Location = List[2],
                                Keywords = List[3],
                                PageLink = List[4]
                            });
                        }

                        string json = JsonSerializer.Serialize(_JobData);
                        File.WriteAllText(path, json);
                    }
                    else if (KindOfData == "Songs")
                    {

                        List<Songs> _SongData = new List<Songs>();

                        foreach (var List in ListOfLists)
                        {
                            _SongData.Add(new Songs()
                            {
                                SongName = List[0],
                                Artist = List[1],
                            });
                        }

                        string json = JsonSerializer.Serialize(_SongData);
                        File.WriteAllText(path, json);
                    }



                }

                static void GetJobs()
                {
                    var chromeOptions = new ChromeOptions();
                    chromeOptions.AddArgument("headless");
                    chromeOptions.AddArgument("log-level=3");
                    var driver = new ChromeDriver(chromeOptions);
                    //var driver = new ChromeDriver();

                    Console.Clear();
                    Console.WriteLine("Scrapen van 5 recentste jobs op ictjobs.be");
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.Write("Please enter a search term here: ");
                    String SearchTerm = Console.ReadLine();
                    while (SearchTerm == null)
                    {
                        SearchTerm = Console.ReadLine();
                    }

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Getting 5 most recent jobs");
                    Console.ResetColor();



                    driver.Navigate().GoToUrl("https://www.ictjob.be/en/search-it-jobs?keywords=" + SearchTerm);

                    var ResultList = driver.FindElement(By.ClassName("search-result-list")).FindElements(By.ClassName("job-info"));
                    var DataList = new List<List<String>>();

                    for (int i = 0; i < 5; i++)
                    {
                        String JobTitle = ResultList[i].FindElement(By.TagName("h2")).Text;
                        String Company = ResultList[i].FindElement(By.ClassName("job-company")).Text;
                        String Location = ResultList[i].FindElement(By.ClassName("job-location")).Text;
                        String Keywords = ResultList[i].FindElement(By.ClassName("job-keywords")).Text;
                        String PageLink = ResultList[i].FindElement(By.TagName("a")).GetAttribute("href");

                        var ThisData = new List<string>();
                        ThisData.Add(JobTitle);
                        ThisData.Add(Company);
                        ThisData.Add(Location);
                        ThisData.Add(Keywords);
                        ThisData.Add(PageLink);

                        DataList.Add(ThisData);
                    }

                    SaveToJson(DataList, "Jobs");

                }

                static void GetBaseData()
                {
                    var chromeOptions = new ChromeOptions();
                    chromeOptions.AddArgument("headless");
                    chromeOptions.AddArgument("log-level=3");
                    var driver = new ChromeDriver(chromeOptions);
                    //var driver = new ChromeDriver();

                    Console.Clear();
                    Console.WriteLine("Scrapen van de basisdata van 5 recentste videos");

                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.Write("Please enter a search term here: ");
                    String SearchTerm = Console.ReadLine();
                    while (SearchTerm == null)
                    {
                        SearchTerm = Console.ReadLine();
                    }

                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Getting 5 most recent videos");
                    Console.ResetColor();

                    driver.Navigate().GoToUrl("https://www.youtube.com/results?search_query=" + SearchTerm + "&sp=CAI%253D");

                    Console.WriteLine("Waiting for page to finish loading...");
                    Thread.Sleep(2000);

                    var VideoContainer = driver.FindElement(By.Id("contents"));
                    var Videos = VideoContainer.FindElements(By.TagName("ytd-video-renderer"));

                    var DataList = new List<List<String>>();


                    for (int i = 0; i < 5; i++)
                    {
                        var meta = Videos[i].FindElement(By.Id("meta"));
                        string title = meta.FindElement(By.TagName("h3")).Text;

                        var metadata = meta.FindElement(By.Id("metadata-line")).FindElements(By.TagName("span"));
                        var vieuws = "Livestreams have no vieuws";
                        string upload = "Livestreams have no upload date";

                        if (metadata.Count != 0) //if there are no items in the metadate this is a livestream
                        {
                            var b = metadata[0].Text;
                            vieuws = b.Substring(0, b.Length - 6);
                            upload = metadata[1].Text;

                        }
                        else
                        {
                            Console.WriteLine("THIS IS A LIVESTREAM");
                            title += " ---LIVE---";
                        }


                        var channel = Videos[i].FindElement(By.Id("channel-name")).FindElement(By.TagName("a")).GetAttribute("text");


                        int a = i + 1;
                        Console.WriteLine("------------------------------" + a + "------------------------------");
                        Console.WriteLine("Title: " + title);
                        Console.WriteLine("Vieuws: " + vieuws);
                        Console.WriteLine("Upload: " + upload);
                        Console.WriteLine("ChannelName: " + channel);

                        var ThisData = new List<string>();
                        ThisData.Add(title);
                        ThisData.Add(vieuws);
                        ThisData.Add(upload);
                        ThisData.Add(channel);


                        DataList.Add(ThisData);
                    }

                    SaveToJson(DataList, "Youtube");


                }

                static List<string> SpotifySongList(bool Custom = false)
                {
                    var chromeOptions = new ChromeOptions();
                    chromeOptions.AddArgument("headless");
                    chromeOptions.AddArgument("log-level=3");
                    var driver = new ChromeDriver(chromeOptions);
                    //var driver = new ChromeDriver();

                    Console.Clear();
                    if (!Custom)
                    {
                        driver.Navigate().GoToUrl("https://open.spotify.com/playlist/37i9dQZF1DWVmX5LMTOKPw");

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Getting list of Spotify top 50");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        Console.Write("Please paste the playlist URL here: ");
                        String Playlist = Console.ReadLine();
                        Console.WriteLine(Playlist);
                        while (Playlist == null || Playlist == "")
                        {
                            Console.Write("Please paste the playlist URL here: ");
                            Playlist = Console.ReadLine();
                        }
                        driver.Navigate().GoToUrl(Playlist);

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Getting custom Spotify playlist");
                        Console.ResetColor();
                        Thread.Sleep(5000);
                    }


                    Thread.Sleep(7000);

                    var Path = "/html/body/div[3]/div/div[2]/div[3]/div[1]/div[2]/div[2]/div/div/div[2]/main/div/section/div[2]/div[3]/div/div[2]/div[2]";
                    var songlist = driver.FindElement(By.XPath(Path));

                    driver.ExecuteScript("document.body.style.zoom='1%'"); // zooming out so spotify loads all songs

                    Thread.Sleep(3000);



                    var songs = songlist.FindElements(By.TagName("div"));
                    List<string> CombinedList = new List<string>();

                    var DataList = new List<List<String>>();


                    // limit to max 700 as spotify for some reason does not like going further
                    for (int i = 0; i < songs.Count - 1 && i < 7700; i += 11)
                    {
                        var finalDiv = songs[i + 5];

                        var SongName = finalDiv.FindElement(By.TagName("a")).Text;
                        var Artist = finalDiv.FindElements(By.TagName("span"));

                        var Artist2 = Artist[0].Text;
                        if (Artist.Count > 1)
                        {
                            Artist2 = Artist[2].Text;
                        }


                        var combined = Artist2 + " " + SongName;
                        Console.WriteLine(i / 11 + 1 + "\t" + combined);
                        CombinedList.Add(combined);

                        var ThisData = new List<string>();
                        ThisData.Add(SongName);
                        ThisData.Add(Artist2);


                        DataList.Add(ThisData);

                    }
                    SaveToJson(DataList, "Songs");

                    driver.Close();
                    return CombinedList;
                }


                static List<string> MNMSongList()
                {
                    var chromeOptions = new ChromeOptions();
                    chromeOptions.AddArgument("headless");
                    chromeOptions.AddArgument("log-level=3");
                    var driver = new ChromeDriver(chromeOptions);
                    //var driver = new ChromeDriver();

                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Getting list of UltraTop50");
                    Console.ResetColor();

                    driver.Navigate().GoToUrl("https://mnm.be/programmagids/ultratop50");
                    Console.WriteLine("Waiting for page to finish loading...");
                    Thread.Sleep(2000);

                    Console.WriteLine("Accepting cookies");
                    var iframes = driver.FindElements(By.TagName("iframe"));
                    driver.SwitchTo().Frame(iframes[2]);
                    var buttons = driver.FindElements(By.TagName("button"));
                    buttons[1].Click();

                    Thread.Sleep(2000);

                    Console.WriteLine("Searching latest UltraTop50");
                    var links = driver.FindElement(By.XPath("/html/body/div[1]/div/main/div/div[2]/div[2]/div/div/div[2]/a[1]"));
                    Console.WriteLine(links.GetAttribute("href"));

                    var date = links.FindElement(By.TagName("p"));
                    Console.WriteLine("Latest UltraTop 50: " + date.Text);


                    Console.WriteLine("Getting song list");
                    var latestTop = links.GetAttribute("href");
                    driver.Navigate().GoToUrl(latestTop);
                    Console.WriteLine("Waiting for page to finish loading...");
                    Thread.Sleep(5000);

                    var songList = driver.FindElement(By.XPath("/html/body/div[1]/div[2]/div/div[2]/div[1]/div[2]/div/ul"));
                    var songs = songList.FindElements(By.ClassName("full"));


                    string pattern = @"(.*) - (.*)";
                    Regex rg = new Regex(pattern);
                    List<string> SongsAsText = new List<string>();

                    var DataList = new List<List<String>>();

                    var Index = 0;
                    foreach (var song in songs)
                    {
                        var split = rg.Match(song.Text);
                        var currentSong = split.Groups[1].ToString() + " " + split.Groups[2].ToString();
                        SongsAsText.Add(currentSong);
                        Console.WriteLine(Index + 1 + "\t" + currentSong);

                        Index++;

                        var ThisData = new List<string>();
                        ThisData.Add(split.Groups[1].ToString());
                        ThisData.Add(split.Groups[2].ToString());


                        DataList.Add(ThisData);
                    }
                    SaveToJson(DataList, "Songs");
                    driver.Close();
                    return SongsAsText;
                }


                static void DownloadSongs(List<string> SongsAsText)
                {
                    Console.WriteLine("\n------------------------------");
                    Console.Write("1\t: Download this list");
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(" !!CPU intensive!!");
                    Console.ResetColor();
                    Console.WriteLine("Any key\t: Return to menu");
                    Console.WriteLine("------------------------------\n");
                    var download = Console.ReadLine();

                    if (download != "1")
                    {
                        ShowMenu();
                        return;
                    }


                    Console.WriteLine("\n------------------------------");
                    Console.WriteLine("1\t: Best audio quality");
                    Console.WriteLine("2\t: Compatibility (mp3)");
                    Console.WriteLine("Any key\t: Return to menu");
                    Console.WriteLine("------------------------------\n");
                    var AudioQuality = Console.ReadLine();
                    bool bestQuality = false;

                    if (AudioQuality == "1")
                    {
                        bestQuality = true;
                    }
                    else if (AudioQuality != "2")
                    {
                        ShowMenu();
                        return;
                    }

                    // get a headless chrome for increased speed
                    var chromeOptions = new ChromeOptions();
                    chromeOptions.AddArgument("headless");
                    chromeOptions.AddArgument("log-level=3");
                    var driver = new ChromeDriver(chromeOptions);
                    //var driver = new ChromeDriver();

                    driver.Navigate().GoToUrl("https://www.youtube.com/");
                    Console.WriteLine("Waiting for page to finish loading...");
                    Thread.Sleep(3000);


                    Console.WriteLine("Declinging youtube cookies...");
                    var Path = "/html/body/ytd-app/ytd-consent-bump-v2-lightbox/tp-yt-paper-dialog/div[4]/div[2]/div[6]/div[1]/ytd-button-renderer[1]/yt-button-shape/button/yt-touch-feedback-shape/div/div[2]";
                    driver.FindElement(By.XPath(Path)).Click();

                    var i = 0;
                    int total = SongsAsText.Count;
                    Console.WriteLine("About to download " + total.ToString() + " songs");



                    foreach (var song in SongsAsText)
                    {
                        var search = song.Replace(" ", "+").Replace("&", "").Replace("/", "");

                        driver.Navigate().GoToUrl("https://www.youtube.com/results?search_query=" + search);

                        Thread.Sleep(500);


                        var youtubeList = driver.FindElement(By.Id("contents"));
                        var temp = youtubeList.FindElement(By.Id("thumbnail"));
                        var songURL = temp.GetAttribute("href");

                        var path = Directory.GetCurrentDirectory() + @"\Downloads";
                        
                        //var path = @"C:\Users\Tibbo\source\repos\DevOps Project Webscraper Tibbo Van Leemput\DevOps Project Webscraper Tibbo Van Leemput\Downloads";
                        string strCmdText = "/C C:\\YoutubeDL\\yt-dlp.exe -x --audio-format mp3 -P \"" + path + "\" -o \"" + song + ".%(ext)s\" --embed-thumbnail --embed-metadata " + songURL;

                        if (bestQuality)
                        {
                            strCmdText = "/C C:\\YoutubeDL\\yt-dlp.exe -x -P \"" + path + "\" -o \"" + song + ".%(ext)s\" --embed-thumbnail --embed-metadata " + songURL;
                        }


                        Console.WriteLine(i + 1 + "/" + total.ToString() + "\t" + song);
                        var proc = new Process();
                        proc.StartInfo.FileName = "CMD.exe";
                        proc.StartInfo.Arguments = strCmdText;
                        proc.StartInfo.CreateNoWindow = true;
                        proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                        proc.Start();

                        //Process.Start("CMD.exe", strCmdText);


                        i++;

                    }

                    driver.Close();
                    Console.WriteLine("Succesfully downloaded all songs!");


                }


            }
            catch
            {
                // if anything goes wrong let the user know and restart the app
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n-----------------------------------------------");
                Console.WriteLine("Oops! it seems something went wrong somewhere?!");
                Console.WriteLine("-----------------------------------------------\n");
                Console.ResetColor();

                Console.WriteLine("Taking you back to the menu in 15 seconds");

                for (int i = 15; i >= 0; i--)
                {
                    Console.Write(".");
                    Thread.Sleep(1000);
                }

                Main(args);

            }

        }
    }
}