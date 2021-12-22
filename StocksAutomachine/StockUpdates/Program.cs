// Selenium.WebDriver.ChromeDriver --> https://github.com/jsakamoto/nupkg-selenium-webdriver-chromedriver/
// Telegram.Bot --> https://github.com/TelegramBots/Telegram.Bot
// Quick start guide --> https://telegrambots.github.io/book/1/quickstart.html

// Convergence scanner Updates --> https://chartink.com/screener/copy-ema-daily-convergence-10-20-50-200-vinay-kumar-yadi-26


// Install packages Selenium.WebDriver, Selenium.WebDriver.ChromeDriver, Telegram.Bot


using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;


String[] inputArgs = Environment.GetCommandLineArgs();
string argType = inputArgs[1];

(string url, string processName) = argType switch {
    "CONVERGANCE"=> ("https://chartink.com/screener/copy-ema-daily-convergence-10-20-50-200-vinay-kumar-yadi-26","Convergance"),
    "HIGH-VOLUME-50-EMA" => ("https://chartink.com/screener/copy-50-ema-crossover-with-high-volumes-3","High Volume 50 EMA Crossover"),
    _=>("","")
};

ChromeOptions options = new();
options.AddArgument("--headless");
options.AddArgument("--disable-gpu");
options.AddArgument("--no-sandbox");

IWebDriver driver = new ChromeDriver(options);
driver.Navigate().GoToUrl(url);
var tableBody = driver.FindElement(By.XPath("//*[@id=\"DataTables_Table_0\"]/tbody"));
var rows = tableBody.FindElements(By.TagName("tr"));
List<StockItem> convergenceList = new();
foreach (var item in rows)
{
    var tds = item.FindElements(By.TagName("td"));
    string name = tds[1].Text;
    string symbol = tds[2].Text;
    convergenceList.Add(new(name, symbol));
}
driver.Close();

StringBuilder sb = new();
sb.Append($"{processName} List :: \n");
convergenceList.ForEach(stock =>
{
    sb.Append($"{stock.Name} - {stock.Symbol} \n");
});

string botToken = "";
var botClient = new TelegramBotClient(botToken);
ChatId chatId = new("@StockUpdatesIndia");

Message message = await botClient.SendTextMessageAsync(
    chatId: chatId,
    text: sb.ToString()
    );


record StockItem(string Name, string Symbol);
