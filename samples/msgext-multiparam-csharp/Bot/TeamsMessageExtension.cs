﻿using AdaptiveCards.Templating;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using MsgExtMultiParamCSharp.Models;
using Newtonsoft.Json;
using System.Diagnostics;

namespace MsgExtMultiParamCSharp.Bot
{
    public class TeamsMessageExtension : TeamsActivityHandler
    {
        protected override Task<MessagingExtensionResponse> OnTeamsMessagingExtensionQueryAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionQuery query, CancellationToken cancellationToken)
        {
            var stockIndex = query?.Parameters?[0]?.Value as string ?? string.Empty;
            var numberOfStocks = query?.Parameters?[1]?.Value as string ?? string.Empty;
            var pB = query?.Parameters?[2]?.Value as string ?? string.Empty;
            var pE = query?.Parameters?[3]?.Value as string ?? string.Empty;

            // Basic: Find stocks in NASDAQ Stocks
            // [
            //   { name: 'StockIndex', value: 'NASDAQ' },
            //   { name: 'NumberofStocks', value: '' },
            //   { name: 'P/B', value: '' },
            //   { name: 'P/E', value: '' }
            // ]

            // Advanced: Find top 10 stocks in NASDAQ Stocks with P/B < 2 and P/E < 30
            // [
            //   { name: 'StockIndex', value: '' },
            //   { name: 'NumberofStocks', value: 'Top:10' },
            //   { name: 'P/B', value: '<2' },
            //   { name: 'P/E', value: '<30' }
            // ]

            Debug.WriteLine($"🔍 StockIndex: '{stockIndex}' | NumberofStocks: '{numberOfStocks}' | P/B: '{pB}' | P/E: '{pE}'");

            var stockData = File.ReadAllText("stock.data.json");
            var stockDataList = JsonConvert.DeserializeObject<List<StockData>>(stockData);

            var adaptiveCardJson = File.ReadAllText("stock.json");
            var template = new AdaptiveCardTemplate(adaptiveCardJson);

            var attachments = new List<MessagingExtensionAttachment>();

            foreach (var stock in stockDataList)
            {
                string resultCard = template.Expand(stockData);

                var previewCard = new HeroCard
                {
                    Title = stock.CompanyName,
                    Subtitle = stock.Symbol,
                }.ToAttachment();

                var attachment = new MessagingExtensionAttachment
                {
                    Content = resultCard,
                    ContentType = "application/vnd.microsoft.card.adaptive",
                    Preview = previewCard,
                };

                attachments.Add(attachment);
            }

            return Task.FromResult(new MessagingExtensionResponse
            {
                ComposeExtension = new MessagingExtensionResult
                {
                    Type = "result",
                    AttachmentLayout = "list",
                    Attachments = attachments,
                },
            });
        }
    }
}