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

ChromeOptions options = new();
options.AddArgument("--headless");
options.AddArgument("--disable-gpu");
options.AddArgument("--no-sandbox");

IWebDriver driver = new ChromeDriver(options);
driver.Navigate().GoToUrl("https://chartink.com/screener/copy-ema-daily-convergence-10-20-50-200-vinay-kumar-yadi-26");
var tableBody = driver.FindElement(By.XPath("//*[@id=\"DataTables_Table_0\"]/tbody"));
var rows = tableBody.FindElements(By.TagName("tr"));
List<dynamic> convergenceList = new List<dynamic>();
foreach (var item in rows)
{
    var tds = item.FindElements(By.TagName("td"));
    string name = tds[1].Text;
    string symbol = tds[2].Text;
    convergenceList.Add(new { Name = name, Symbol = symbol });
}
driver.Close();

string botToken = "1784206810:AAEdxWTwVpVbn3SsnusqT2OzrGCvHjQvbzE";

var botClient = new TelegramBotClient(botToken);

ChatId chatId = new ChatId("@StockUpdatesIndia");

StringBuilder sb=new StringBuilder();
sb.Append("Convergance List :: \n");
convergenceList.ForEach(stock => {
    sb.Append($"{stock.Name} - {stock.Symbol} \n");
});



Message message = await botClient.SendTextMessageAsync(
    chatId: chatId,
    text: sb.ToString()
    );
