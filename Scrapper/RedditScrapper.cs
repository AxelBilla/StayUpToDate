using System.Globalization;

public class RedditScrapper
{
    public static async Task Main(string[] args)
    {
        foreach (string arg in args)
        {
            string[] content = arg.Split("=");
            switch (content[0])
            {
                case "-id":
                    Reddit.SetID(content[1]);
                    break;
                case "-secret":
                    Reddit.SetSecret(content[1]);
                    break;
            }
        }
        
        string[] target_subs = File.ReadAllLines("./targets.txt");
        if (target_subs.Length == 0) throw new Exception("No target set in ./targets.txt");
        foreach (string target_sub in target_subs)
        {
            if (target_sub.Trim() == string.Empty) continue;
            string name = "";
            string type = "new";
            List<string> options = new List<string>();
            
            string[] items = target_sub.Split("'");
            for (int i = 0; i < items.Length; i++)
            {

                if (i % 2 != 0) continue;
                switch (items[i].Trim().ToLower())
                {
                    case "-name=":
                        name = items[i + 1];
                        break;
                    case "-type=":
                        type = items[i + 1];
                        break;
                    case "-options=":
                        string[] option_list = items[i + 1].Split("&");
                        foreach (string option in option_list)
                        {
                            options.Add(option);
                        }
                        break;
                }
            }
            
            targets.Add(new Reddit.ScrapeTarget(name, type, options));
        }

        
        DateTime today = DateTime.Now.ToUniversalTime();
        CultureInfo zone = new CultureInfo("fr-FR");
        string day = today.ToString("d", zone);
        day = day.Replace("/", "-");
        
        Console.WriteLine($"[Reddit Scrapper: Keep me up-to-date!]\n\nStarting to scrape data for {day}...\n");
        App.Create.Folder(App.path+day);
        foreach(Reddit.ScrapeTarget target in targets)
        {
            string folder_path = day + "/" + target.name;
            string file_name = today.ToString("HH:m:s", zone);
            
            App.Create.Folder(App.path+folder_path);
            string res = await Reddit.Scrape(target);
            App.Create.JSON(folder_path, file_name, res);
            Console.WriteLine($"[SUCCESS]: {target.name} was scrapped! ({App.path+folder_path}/{file_name}.json)");
        }

    }

    private static List<Reddit.ScrapeTarget> targets = new List<Reddit.ScrapeTarget>()
    {
    };
}

public static class App
{
    public static string path = "./files/";
    public static class Create
    {
        public static void JSON(string file_path, string name, string content = "")
        {
            string new_path = App.Create.File(path+file_path, name, "json");
            if (content != "") App.Write.File(new_path, content);
        }

        public static string File(string path, string name, string extension="")
        {
            string file_path = path + "/" + name + (extension!="" ? "."+extension : "");

            if (!System.IO.File.Exists(file_path)) System.IO.File.Create(file_path).Close();
            else Console.WriteLine($"[{name}]: DATA ALREADY SCRAPPED");

            return file_path;
        }

        public static void Folder(string path)
        {
            if(!Directory.Exists(path)) Directory.CreateDirectory(path);
        }
    }

    public static class Write
    {
        public static void File(string path, string content)
        {
            StreamWriter file = System.IO.File.CreateText(path);
            file.Write(content);
            file.Close();
        }
    }
    
}

public static class Reddit
{
    public static string client_id = "";
    public static string secret = "";

    public static void SetID(string id)
    {
        Reddit.client_id = id;
    }

    public static void SetSecret(string secret)
    {
        Reddit.secret = secret;
    }
    
    private static SocketsHttpHandler http_socket_handler = new SocketsHttpHandler
    {
        PooledConnectionLifetime = TimeSpan.FromMinutes(15)
    };

    private static HttpClient reddit_client = new HttpClient(http_socket_handler);

    
    public static async Task<string> Scrape(string sub, string sort)
    {
        return await Reddit.Scrape($"https://www.reddit.com/r/{sub}/{sort}/.json");
    }

    public static async Task<string> Scrape(Reddit.ScrapeTarget target)
    {
        if (target.options != default) return await Scrape(target.name, target.type, target.options);
        else return await Scrape(target.name, target.type);
    }
    
    public static async Task<string> Scrape(string sub, string type, List<string> options)
    {
        string url = $"https://old.reddit.com/r/{sub}/{type}/.json?";
        foreach (string option in options)
        {
            url += "&" + option;
        }

        url += "&restrict_sr=on";

        return await Reddit.Scrape(url);
    }

    public static async Task<string> Scrape(string url)
    {
        if (client_id == "" || secret == "") throw new Exception("Missing Credentials");
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, (url));

        request.Headers.Add("User-Agent", "User agent");
        request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes($"{client_id}:${secret}")));

        HttpResponseMessage response = await reddit_client.SendAsync(request);
        return await response.Content.ReadAsStringAsync();
    }

    public class ScrapeTarget
    {
        public string name;
        public string type;
        public List<string> options;

        public ScrapeTarget(string name, string type = "new", List<string> options = default)
        {
            this.name = name;
            this.type = type;
            this.options = options;
        }
    }
}