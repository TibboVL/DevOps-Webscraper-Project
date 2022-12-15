using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Text.Json;
using File = System.IO.File;
using OpenQA.Selenium.DevTools.V106.HeadlessExperimental;
using System.Threading.Channels;

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

            WriteInColor("\n---------------------------------------------------------", "Red");
            WriteInColor("To be able to use the download functionality of this application\nyou must have YOUTUBEDLP AND FFMPEG installed and added to your path!!", "Red");
            WriteInColor("Youtubedlp: https://github.com/yt-dlp/yt-dlp", "Red");
            WriteInColor("ffmpeg: https://phoenixnap.com/kb/ffmpeg-windows", "Red");
            WriteInColor("\nDepending on your terminal you may see missing symbols,\nto fix this use the new windows Terminal", "Red");
            WriteInColor("---------------------------------------------------------\n", "Red");

            

            static void WriteInColor(string message = "No message provided", string color = "Grey", bool NewLine = true, bool Clear = false)
            {
                if (Clear)
                {
                    Console.Clear();
                }
                ConsoleColor[] consoleColors = (ConsoleColor[])ConsoleColor.GetValues(typeof(ConsoleColor));
                for (int i = 0; i < consoleColors.Length; i++) { 
                    if (color.Equals(consoleColors[i].ToString())) {
                        Console.ForegroundColor = consoleColors[i];
                        
                        if (NewLine)
                        {
                            Console.WriteLine(message);
                        } else
                        {
                            Console.Write(message);
                        }
                        Console.ResetColor();
                    }
                }
            }

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
                        case "6":
                            List<string> URLList = YoutubeSongList();
                            DownloadSongs(URLList, true);
                            break;
                        case "q" or "Q":
                            Environment.Exit(0);
                            break;
                        default:
                            Console.Clear();
                            WriteInColor("\"" + Selection + "\" is not a valid option!", "Red");
                            ShowMenu();
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
                    Console.WriteLine("3: Download a custom spotify playlist (max 700)");
                    Console.WriteLine("4: Scrapen van de basisdata van 5 recentste videos");
                    Console.WriteLine("5: Scrapen van 5 recentste jobs op ictjobs.be");
                    Console.WriteLine("6: Download a custom youtube platlist (max 700)");
                    Console.WriteLine("Q: Exit");
                    WriteInColor("\n(Repetition of same task can cause websites to temporary\nlimit/block traffic which causes the script issues!)", "Yellow");
                    Console.WriteLine("--------------------------------------------------------\n");

                    var response = Console.ReadLine();
                    Selections(response.ToString());
                }

                static ChromeDriver StartWebDriver(bool headless = true, string startAtURL = "", int loadDelay = 3000)
                {
                    var chromeOptions = new ChromeOptions();
                    if (headless)// get a headless chrome for increased speed
                    {
                        chromeOptions.AddArgument("headless");
                        chromeOptions.AddArgument("log-level=3");
                    }
                    var driver = new ChromeDriver(chromeOptions);
                    if (startAtURL != "") {
                        driver.Navigate().GoToUrl(startAtURL);
                        WriteInColor("Waiting for page to finish loading ", "Blue", false);
                        for (int i = 1; i <= loadDelay / 1000; i++)
                        {
                            WriteInColor(".", "Blue", i == loadDelay / 1000 );
                            Thread.Sleep(1000);
                        }
                    }
                    return driver;
                }

                static void SaveToJson(List<List<String>> ListOfLists, String KindOfData)
                {
                    DateTime now = DateTime.Now;
                    string partOfFilename = now.ToString().Replace("/", "-").Replace(":", ""); //ensure song names do not contain characters windows does not allow
                    string filename = KindOfData + " - " + partOfFilename;
                    string path = Directory.GetCurrentDirectory() + @"\Json\" + filename + ".json";
                    WriteInColor("\nSaving data to JSON as \"" + filename + "\"\n", "Blue");

                    if (KindOfData == "Youtube")
                    {
                        List<YouTube> _data = new();
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
                        List<Jobs> _JobData = new();
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
                    else if (KindOfData == "MNMSongs" || KindOfData == "SpotifySongs")
                    {
                        List<Songs> _SongData = new();
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
                    }else if (KindOfData == "YouTubeCustom")
                    {
                        List<YouTubePlaylist> _YoutubePlaylistData = new();
                        foreach (var List in ListOfLists)
                        {
                            _YoutubePlaylistData.Add(new YouTubePlaylist()
                            {
                                SongName = List[0],
                                URL = List[1],
                                channel = List[2],
                                vieuws = List[3],
                                upload = List[4]
                            });
                        }
                        string json = JsonSerializer.Serialize(_YoutubePlaylistData);
                        File.WriteAllText(path, json);
                    }
                }


                static void GetJobs()
                {
                    WriteInColor("Scrapen van 5 recentste jobs op ictjobs.be", "White", true, true);
                    WriteInColor("Please enter a search term here: ", "Magenta", false);
                    String SearchTerm = Console.ReadLine();
                    while (SearchTerm == "" || SearchTerm == " ")
                    {
                        WriteInColor("Please enter a search term here: ", "Magenta", false);
                        SearchTerm = Console.ReadLine();
                    }
                    WriteInColor("Getting 5 most recent jobs", "Green");

                    var driver = StartWebDriver(true, "https://www.ictjob.be/en/search-it-jobs?keywords=" + SearchTerm);
                    Thread.Sleep(3000); // wait for their dumb verification and resubmit the search
                    driver.Navigate().GoToUrl("https://www.ictjob.be/en/search-it-jobs?keywords=" + SearchTerm);
                    var ResultList = driver.FindElement(By.ClassName("search-result-list")).FindElements(By.ClassName("job-info"));
                    var DataList = new List<List<string>>();

                    for (int i = 0; i < 5; i++)
                    {
                        string JobTitle = ResultList[i].FindElement(By.TagName("h2")).Text;
                        string Company = ResultList[i].FindElement(By.ClassName("job-company")).Text;
                        string Location = ResultList[i].FindElement(By.ClassName("job-location")).Text;
                        string Keywords = ResultList[i].FindElement(By.ClassName("job-keywords")).Text;
                        string PageLink = ResultList[i].FindElement(By.TagName("a")).GetAttribute("href");

                        Console.WriteLine("------------------------------ " + (i + 1) + " ------------------------------");
                        Console.WriteLine("Title: " + JobTitle);
                        Console.WriteLine("Company: " + Company);
                        Console.WriteLine("Location: " + Location);
                        Console.WriteLine("Keywords: " + Keywords);
                        Console.WriteLine("PageLink: " + PageLink);

                        var ThisData = new List<string>
                        {
                            JobTitle,
                            Company,
                            Location,
                            Keywords,
                            PageLink
                        };
                        DataList.Add(ThisData);
                    }
                    SaveToJson(DataList, "Jobs");
                }

                static void GetBaseData()
                {
                    WriteInColor("Scrapen van de basisdata van 5 recentste videos", "White", true, true);
                    WriteInColor("Please enter a search term here: ", "Magenta", false);
                    string SearchTerm = Console.ReadLine();
                    while (SearchTerm == "" || SearchTerm == " ")
                    {
                        WriteInColor("Please enter a search term here: ", "Magenta", false);
                        SearchTerm = Console.ReadLine();
                    }

                    WriteInColor("Getting 5 most recent videos", "Green", true, true);
                    var driver = StartWebDriver(true,"https://www.youtube.com/results?search_query=" + SearchTerm + "&sp=CAI%253D");
                    var VideoContainer = driver.FindElement(By.Id("contents"));
                    var Videos = VideoContainer.FindElements(By.TagName("ytd-video-renderer"));

                    var DataList = new List<List<String>>();

                    for (int i = 0; i < 5; i++)
                    {
                        var meta = Videos[i].FindElement(By.Id("meta"));
                        string title = meta.FindElement(By.TagName("h3")).Text;

                        var metadata = meta.FindElement(By.Id("metadata-line")).FindElements(By.TagName("span"));
                        string vieuws = "Livestreams have no vieuws";
                        string upload = "Livestreams have no upload date";
                        var channel = Videos[i].FindElement(By.Id("channel-name")).FindElement(By.TagName("a")).GetAttribute("text");
                        if (metadata.Count < 1) //if there are no items in the metadate this is a livestream
                        {
                            string b = metadata[0].Text;
                            vieuws = b.Substring(0, b.Length - 6);
                            upload = metadata[1].Text;
                        }
                        else
                        {
                            Console.WriteLine("THIS IS A LIVESTREAM");
                            title += " 🔴 LIVE";
                        }
                        

                        Console.WriteLine("------------------------------ " + (i + 1) + " ------------------------------");
                        Console.WriteLine("Title: " + title);
                        Console.WriteLine("Vieuws: " + vieuws);
                        Console.WriteLine("Upload: " + upload);
                        Console.WriteLine("ChannelName: " + channel);

                        var ThisData = new List<string>
                        {
                            title,
                            vieuws,
                            upload,
                            channel
                        };
                        DataList.Add(ThisData);
                    }
                    SaveToJson(DataList, "Youtube");
                }

                static List<string> YoutubeSongList()
                {
                    WriteInColor("Please paste the playlist URL here: ", "Magenta", false, true);
                    String Playlist = Console.ReadLine();
                    while (Playlist == "" || Playlist == " " || !Playlist.Contains("https://www.youtube.com/playlist?list="))
                    {
                        WriteInColor("Please paste a valid PLAYLIST URL \"https://www.youtube.com/playlist?list=\" here:", "Magenta", false);
                        Playlist = Console.ReadLine();
                    }

                    var driver = StartWebDriver(true , "https://www.youtube.com/");

                    Console.WriteLine("Declinging youtube cookies...");
                    var Path = "/html/body/ytd-app/ytd-consent-bump-v2-lightbox/tp-yt-paper-dialog/div[4]/div[2]/div[6]/div[1]/ytd-button-renderer[1]/yt-button-shape/button/yt-touch-feedback-shape/div/div[2]";
                    driver.FindElement(By.XPath(Path)).Click();

                    driver.Navigate().GoToUrl(Playlist);
                    WriteInColor("Getting Youtube playlist", "Green", true, true);
                    
                    driver.ExecuteScript("document.body.style.zoom='1%'"); // zooming out so spotify loads all songs
                    Thread.Sleep(1000);

                    var AmountSongs = int.Parse(driver.FindElement(By.XPath("/html/body/ytd-app/div[1]/ytd-page-manager/ytd-browse/ytd-playlist-header-renderer/div/div[2]/div[1]/div/div[1]/div[1]/ytd-playlist-byline-renderer/div/yt-formatted-string[1]/span[1]")).Text.Replace(",", ""));
                    int Timeout = AmountSongs > 500 ? 10000 : (AmountSongs > 100 ? 5000 : 1000); // reduce wait time depending on platlist size
                    Console.WriteLine("Waiting " + Timeout / 1000 + " seconds for playlist to fully load");
                    Thread.Sleep(Timeout);

                    List<string> URLList = new(); // for download script
                    var DataList = new List<List<String>>(); // for json script


                    DateTime before = DateTime.Now;
                    int index = 1;
                    var contents = driver.FindElements(By.Id("contents"));
                    var SongList = contents[2].FindElements(By.Id("meta"));
                    foreach (var song in SongList)
                    {
                        string SongName = song.FindElement(By.Id("video-title")).GetAttribute("title");
                        string Link = song.FindElement(By.Id("video-title")).GetAttribute("href");
                        string ChannelName = song.FindElement(By.Id("channel-name")).Text;
                        var VideoInfo = song.FindElement(By.Id("video-info")).FindElements(By.TagName("span"));
                        string Views = VideoInfo[0].Text;
                        string Uploaded = VideoInfo[2].Text;
                        Console.WriteLine("------------------------------ " + (index) + " ------------------------------");
                        Console.WriteLine(SongName);
                        Console.WriteLine(Link);
                        Console.WriteLine(ChannelName);
                        Console.WriteLine(Views);
                        Console.WriteLine(Uploaded);

                        var ThisData = new List<string>
                        {
                            SongName,
                            Link,
                            ChannelName,
                            Views,
                            Uploaded
                        };
                        DataList.Add(ThisData);
                        string currentSong = SongName + "+++" + Link;
                        URLList.Add(currentSong);

                        index++;
                    }
                    TimeSpan span = DateTime.Now - before;
                    WriteInColor("\nTotal reading of playlist took " + span.Minutes + " minutes and " + span.Seconds + " seconds", "Blue");

                    SaveToJson(DataList, "YouTubeCustom");
                    driver.Close();
                    return URLList;
                }

                static List<string> SpotifySongList(bool Custom = false)
                {
                    var driver = StartWebDriver(true, "https://open.spotify.com/playlist/37i9dQZF1DWVmX5LMTOKPw") ;

                    if (!Custom)
                    {
                        WriteInColor("Getting list of Spotify top 50", "Green", true, true);
                    }
                    else
                    {
                        WriteInColor("Please paste the playlist URL here: ", "Magenta", false, true);
                        String Playlist = Console.ReadLine();
                        while (Playlist == "" || Playlist == " " || !Playlist.Contains("https://open.spotify.com/playlist/"))
                        {
                            WriteInColor("Please paste a valid PLAYLIST URL \"https://open.spotify.com/playlist/\" here:", "Magenta", false);
                            Playlist = Console.ReadLine();
                        }
                        driver.Navigate().GoToUrl(Playlist);
                        WriteInColor("Getting custom Spotify playlist", "Green", true, true);
                        //Thread.Sleep(5000);
                    }


                    driver.ExecuteScript("document.body.style.zoom='1%'"); // zooming out so spotify loads all songs
                    Thread.Sleep(7000);

                    var Path1 = "/html/body/div[3]/div/div[2]/div[3]/div[1]/div[2]/div[2]/div/div/div[2]/main/div/section/div[2]/div[3]/div/div[2]/div[2]";
                    var Path2 = "/html/body/div[4]/div/div[2]/div[3]/div[1]/div[2]/div[2]/div/div/div[2]/main/div/section/div[2]/div[3]/div/div[2]/div[2]";
                    var songlist = driver.FindElement(By.TagName("body"));
                    try
                    {
                        songlist = driver.FindElement(By.XPath(Path1));
                    } catch
                    {
                        WriteInColor("Fallback to alternative main pos in dom", "Blue"); // for when spotify randomly decides to add a fourth div
                        songlist = driver.FindElement(By.XPath(Path2));
                    }
                    
                    var songs = songlist.FindElements(By.TagName("div"));

                    List<string> CombinedList = new();
                    var DataList = new List<List<String>>();

                    // limit to max 700 as spotify for some reason does not like going further
                    for (int i = 0; i < songs.Count - 1 && i < 7700; i += 11)
                    {
                        var finalDiv = songs[i + 5];

                        var SongName = finalDiv.FindElement(By.TagName("a")).Text;
                        var Artist = finalDiv.FindElements(By.TagName("span"));
                        var Artist2 = Artist.Count > 1 ? Artist[2].Text : Artist[0].Text; // if there is an Explicit symbol take the part after it

                        var combined = Artist2 + " " + SongName;
                        Console.WriteLine(i / 11 + 1 + "\t" + combined);
                        CombinedList.Add(combined);
                        var ThisData = new List<string>
                        {
                            SongName,
                            Artist2
                        };
                        DataList.Add(ThisData);

                    }
                    SaveToJson(DataList, "SpotifySongs");
                    driver.Close();
                    return CombinedList;
                }


                static List<string> MNMSongList()
                {
                    var driver = StartWebDriver(true, "https://mnm.be/programmagids/ultratop50");
                    WriteInColor("Getting list of UltraTop50", "Green", true, true);

                    Console.WriteLine("Accepting cookies");
                    var iframes = driver.FindElements(By.TagName("iframe"));
                    driver.SwitchTo().Frame(iframes[2]);
                    var buttons = driver.FindElements(By.TagName("button"));
                    buttons[1].Click();

                    Thread.Sleep(2000);

                    Console.WriteLine("Searching latest UltraTop50");
                    var links = driver.FindElement(By.XPath("/html/body/div[1]/div/main/div/div[2]/div[2]/div/div/div[2]/a[1]"));
                    Console.WriteLine("URL: " + links.GetAttribute("href"));
                    string date = links.FindElement(By.TagName("p")).Text;
                    Console.WriteLine("Latest UltraTop 50: " + date);


                    Console.WriteLine("Getting song list");
                    var latestTop = links.GetAttribute("href");
                    driver.Navigate().GoToUrl(latestTop);
                    Console.WriteLine("Waiting for page to finish loading...");
                    Thread.Sleep(6000);

                    var songList = driver.FindElement(By.XPath("/html/body/div[1]/div[2]/div/div[2]/div[1]/div[2]/div/ul"));
                    var songs = songList.FindElements(By.ClassName("full"));


                    string pattern = @"(.*) - (.*)";
                    Regex rg = new(pattern);
                    List<string> SongsAsText = new();

                    var DataList = new List<List<String>>();

                    var Index = 0;
                    foreach (var song in songs)
                    {
                        var split = rg.Match(song.Text);
                        string currentSong = split.Groups[1].ToString() + " " + split.Groups[2].ToString();
                        SongsAsText.Add(currentSong);
                        Console.WriteLine(Index + 1 + "\t" + currentSong);

                        var ThisData = new List<string>
                        {
                            split.Groups[1].ToString(),
                            split.Groups[2].ToString()
                        };
                        DataList.Add(ThisData);
                        Index++;
                    }
                    SaveToJson(DataList, "MNMSongs");
                    driver.Close();
                    return SongsAsText;
                }


                static void DownloadSongs(List<string> SongsAsText, bool IsThisListAlreadyYoutubeURLs = false)
                {
                    Console.WriteLine("\n------------------------------");
                    Console.Write("1\t: Download this list");
                    WriteInColor(" !!CPU intensive!!", "Red");
                    Console.WriteLine("Any key\t: Return to menu");
                    Console.WriteLine("------------------------------\n");
                    var download = Console.ReadLine();
                    if (download != "1") ShowMenu();
                   

                    Console.WriteLine("\n------------------------------");
                    Console.WriteLine("1\t: Best audio quality");
                    Console.WriteLine("2\t: Compatibility (mp3)");
                    Console.WriteLine("Any key\t: Return to menu");
                    Console.WriteLine("------------------------------\n");
                    string AudioQuality = Console.ReadLine();


                    if (AudioQuality != "1" && AudioQuality != "2") ShowMenu();
                    string quality = AudioQuality == "1" ? "" : "--audio-format mp3 ";


                    var driver = StartWebDriver();
                    if (!IsThisListAlreadyYoutubeURLs)
                    {
                        driver.Navigate().GoToUrl("https://www.youtube.com/");
                        Console.WriteLine("Waiting for page to finish loading...");
                        Thread.Sleep(3000);


                        Console.WriteLine("Declinging youtube cookies...");
                        var Path = "/html/body/ytd-app/ytd-consent-bump-v2-lightbox/tp-yt-paper-dialog/div[4]/div[2]/div[6]/div[1]/ytd-button-renderer[1]/yt-button-shape/button/yt-touch-feedback-shape/div/div[2]";
                        driver.FindElement(By.XPath(Path)).Click();
                    }
                    else driver.Close();

                    int total = SongsAsText.Count;
                    WriteInColor("About to download " + total.ToString() + " songs", "Blue");
                    WriteInColor("Press Q at any time stop the download process!", "Red");
                    
                    DateTime before = DateTime.Now;
                    var HumanReadableTime = before.ToString("G").Replace("/", "-").Replace(":", "");
                    string path = Directory.GetCurrentDirectory() + @"\Downloads\" + HumanReadableTime;
                    int sleeptimer = IsThisListAlreadyYoutubeURLs ? 2000 : 1000;
                    int i = 0;

                    bool forceStoppedDownload = false;
                    bool normalStopped = false;
                    while (!forceStoppedDownload && !normalStopped)
                    {
                        foreach (var song in SongsAsText)
                        {
                            if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Q)
                            {
                                WriteInColor("\nYou pressed Q which ends the downloading process!", "Red", false);
                                forceStoppedDownload = true;
                                break;
                            }

                            Thread.Sleep(sleeptimer);

                            var songURL = IsThisListAlreadyYoutubeURLs ? song.Split("+++")[1] : "";
                            var filename = IsThisListAlreadyYoutubeURLs ? song.Split("+++")[0] : song;

                            if (!IsThisListAlreadyYoutubeURLs)
                            {
                                string search = song.Replace(" ", "+").Replace("&", "").Replace("/", ""); // replace characters that mess up the url
                                driver.Navigate().GoToUrl("https://www.youtube.com/results?search_query=" + search);
                                Thread.Sleep(500);

                                var youtubeList = driver.FindElement(By.Id("contents"));
                                var temp = youtubeList.FindElement(By.Id("thumbnail"));
                                songURL = temp.GetAttribute("href");
                            }

                            string strCmdText = "/C C:\\YoutubeDL\\yt-dlp.exe -x " + quality + "-P \"" + path + "\" -o \"" + (i + 1) + " - " + filename + ".%(ext)s\" --embed-thumbnail --embed-metadata " + songURL;
                            Console.WriteLine(i + 1 + "/" + total.ToString() + "\t" + filename);
                            var proc = new Process();
                            proc.StartInfo.FileName = "CMD.exe";
                            proc.StartInfo.Arguments = strCmdText;
                            proc.StartInfo.CreateNoWindow = true;
                            proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                            proc.Start();

                            i++;
                        }
                        normalStopped = true;
                    }

                    driver.Close();
                    TimeSpan span = DateTime.Now - before;
                    WriteInColor("\nSuccesfully downloaded " + i + "/" + total + " songs in " + span.Minutes + " minutes and " + span.Seconds + " seconds!", "Blue");
                    WriteInColor("(You may need to restart eplorer.exe for it to\nbe able to properly open the downloads directory)\n", "Blue");
                }
            }
            catch (Exception ex)
            {
                // if anything goes wrong let the user know and restart the app
                WriteInColor("\n-----------------------------------------------", "Red");
                WriteInColor("Oops! it seems something went wrong somewhere?!", "Red");
                WriteInColor("-----------------------------------------------\n", "Red");
                Console.WriteLine(ex);
                WriteInColor("\nreturning to menu in 15 seconds", "Yellow");
                for (int i = 15; i >= 0; i--)
                {
                    Console.Write("#");
                    Thread.Sleep(1000);
                }
                Main(args);
            }
        }
    }
}