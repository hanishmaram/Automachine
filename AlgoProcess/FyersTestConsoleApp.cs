// See https://aka.ms/new-console-template for more information

using FyersAPI;

string access_token = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJhcGkuZnllcnMuaW4iLCJpYXQiOjE2NDc3MDUwODAsImV4cCI6MTY0NzczNjIyMCwibmJmIjoxNjQ3NzA1MDgwLCJhdWQiOlsieDowIiwieDoxIiwieDoyIiwiZDoxIiwiZDoyIiwieDoxIiwieDowIl0sInN1YiI6ImFjY2Vzc190b2tlbiIsImF0X2hhc2giOiJnQUFBQUFCaU5mdjRiTUluaUVrWlNpc2ctSW5fUy1mQVpMdUdpc3gzOVFNTWhIcUN5N1BlZGZOdkI2T2RheWVzQmR6c1VBVkd4RFNIVE5QQmFWQWkwZndPVkg2R25TSGdCY19mbUpqZ0xXazhZbVNEUHcwYzE5QT0iLCJkaXNwbGF5X25hbWUiOiJIQU5JU0ggTUFSQU0iLCJmeV9pZCI6IkRIMDAwNDgiLCJhcHBUeXBlIjoxMDAsInBvYV9mbGFnIjoiTiJ9.FA6DZ_lfT2NqkR51ZV01nI_0kDUCYfJTh9xsp3z4FiA";
const string appId = "";
const string appSecretId = "";
const string url = "http://127.0.0.1/?s=ok&code=200&auth_code=eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJhcGkubG9naW4uZnllcnMuaW4iLCJpYXQiOjE2NDc3MDUwMTEsImV4cCI6MTY0NzczNTAxMSwibmJmIjoxNjQ3NzA0NDExLCJhdWQiOlsieDowIiwieDoxIiwieDoyIiwiZDoxIiwiZDoyIiwieDoxIiwieDowIl0sInN1YiI6ImF1dGhfY29kZSIsImRpc3BsYXlfbmFtZSI6IkRIMDAwNDgiLCJub25jZSI6IiIsImFwcF9pZCI6IkhERk8zMkxNWU8iLCJ1dWlkIjoiYWEwNjM1ODUyZWQ5NDI3YjliYzAzNmY3MDM3YmY1MDgiLCJpcEFkZHIiOiIyMjMuMjI3LjEwMS4xNDEiLCJzY29wZSI6IiJ9.ozSIfROi5OAf7mT_GUx2LLbQjWWFf08gSPji5ORzzew&state=568";

Fyers fyers = new(appId);

await PlaceATMShortStraddleOrder();

//await GenerateAccessToken();

//Add_AccessToken_Header();
//await GetQuoteData();
//await PlaceOrder();






//var response = await Fyers.DownloadMasterAsync("NSE_CM");

//string line;
//List<MasterDataResponse> lstReponse = new();
//StreamReader reader = new StreamReader(response);

//while ((line = reader.ReadLine()) != null)
//{
//    var row= line.Split(',');
//    MasterDataResponse obj = new();

//    obj.Token = row[0];
//    obj.SymbolDetails = row[1];
//    obj.ExchangeInstrumentType = Convert.ToInt32(row[2]);
//    obj.MinimumLotSize = Convert.ToInt32(row[3]);
//    obj.TickSize = float.Parse(row[4]);
//    obj.Isin = row[5];
//    obj.TradingSession = row[6];
//    obj.LastUpdateDate = Convert.ToDateTime(row[7]);
//    obj.ExpiryDate = row[8];
//    obj.SymbolTicker = row[9];
//    obj.Exchange = Convert.ToInt32(row[10]);
//    obj.Segment = Convert.ToInt32(row[11]);
//    obj.ScriptCode = Convert.ToInt32(row[12]);
//    obj.ScriptName = row[13];
//    obj.UnderlyingScriptCode = Convert.ToInt32(row[14]);
//    obj.StrikePrice = float.Parse(row[15]);
//    obj.OptionType = row[16];

//    lstReponse.Add(obj);
//}

//MasterDataResponse bankNifty = lstReponse.Where(x => x.ScriptName.Equals("BANKNIFTY")).FirstOrDefault();

async Task PlaceOrder(string tradingSymbol)
{
    var response = await fyers.PlaceOrderAsync(new PlaceOrderPayload()
    {
        disclosedQty = 0,
        offlineOrder = "False",
        productType = ProductTypes.INTRADAY,
        qty = 25,
        takeProfit = 0.0m,
        side = OrderSide.Sell,
        stopLoss = 0.0m,
        stopPrice = 0.0m,
        symbol = tradingSymbol,
        type = (int)OrderType.Market,
        validity = OrderValidity.DAY
    });

    if (response != null && !string.IsNullOrEmpty(response.id))
        Console.WriteLine(response.id);
}

async Task PlaceOrders(string[] symbols)
{
    symbols.ToList<string>().ForEach(async x =>
    {
        await PlaceOrder(x);
    });
}


async Task<double> GetBNQuoteData() 
{
    var quouteData = await fyers.GetQuotesAsync(new string[] { "NSE:NIFTYBANK-INDEX" });

    Console.WriteLine(quouteData.d[0].v.lp);
    return quouteData.d[0].v.lp;
}


void Add_AccessToken_Header()
{
    fyers.AddHttpHeaders(new Dictionary<string, string>() { { "Authorization", $"{appId}:{access_token}" } });
}

async Task PlaceATMShortStraddleOrder()
{
    Add_AccessToken_Header();
    double ltpBN = await GetBNQuoteData();
    int strickPrice = GetATMStrikePrice(ltpBN);

    await PlaceOrders(GetATMSymbols(strickPrice));
}

async Task GenerateAccessToken()
{
    TokenPayload outToken;

    Fyers.IsValidLogin(url, appId, appSecretId,out outToken);
    TokenResponse res = await fyers.GenerateTokenAsync(outToken);
}

int GetATMStrikePrice(double ltpPrice)
{
    return Convert.ToInt32(Math.Round(ltpPrice/100,0)) * 100;
}

string[] GetATMSymbols(int strikePrice)
{
    string symbol = $"NSE:BANKNIFTY22324{strikePrice}";
    return new string[] { $"{symbol}CE", $"{symbol}PE" };
}

public class MasterDataResponse
{
    public string Token { get; set; }
    public string SymbolDetails { get; set; }
    public int ExchangeInstrumentType { get; set; }
    public int MinimumLotSize { get; set; }
    public float TickSize { get; set; }
    public string Isin { get; set; }
    public string TradingSession { get; set; }
    public DateTime LastUpdateDate { get; set; }
    public string ExpiryDate { get; set; }
    public string SymbolTicker { get; set; }
    public int Exchange { get; set; }
    public int Segment { get; set; }
    public int ScriptCode { get; set; }
    public string ScriptName { get; set; }
    public int UnderlyingScriptCode { get; set; }
    public float StrikePrice { get; set; }
    public string OptionType { get; set; }
}
