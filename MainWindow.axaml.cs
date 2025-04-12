using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using Avalonia.Controls;
using Avalonia.Interactivity;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Newtonsoft.Json;

namespace Module4;

public partial class MainWindow : Window
{
    private string _dataFromApi = "";
    public MainWindow()
    {
        InitializeComponent();
    }
    
    private async void GetDataFromApi_OnClick(object? sender, RoutedEventArgs e)
    {
        var httpClient = new HttpClient();
        const string url = "identityCard";
        try
        {
            var response = await httpClient.GetStringAsync($"http://127.0.0.1:4444/TransferSimulator/{url}");
            var data = JsonConvert.DeserializeObject<Dictionary<string,string>>(response);
            _dataFromApi = data!["value"];
            DataFromApiTextBlock.Text = _dataFromApi;
        }
        catch (Exception ex)
        {
            DataFromApiTextBlock.Text = ex.Message;
        }
    }

    private void WriteInDoc_OnClick(object? sender, RoutedEventArgs e)
    {
        const string regex = @"[0-9]{2} [0-9]{2} [0-9]{6}";
        var validationResult = Regex.IsMatch(_dataFromApi, regex);
        TestResultTextBlock.Text = validationResult ? "не содержит запрещенные символы" : "содержит запрещенные символы";
        try
        {
            using var doc = WordprocessingDocument.Open("ТестКейс.docx", true); 
            var document = doc.MainDocumentPart!.Document;

            if (document.Descendants<Text>().FirstOrDefault(t => t.Text.Contains("Result 1")) != null)
                ReplaceText("Result 1", validationResult, document);
            else if(document.Descendants<Text>().FirstOrDefault(t => t.Text.Contains("Result 2")) != null)
                ReplaceText("Result 2", validationResult, document);
        }
        catch (Exception ex)
        {
            TestResultTextBlock.Text = ex.Message;
        }
    }

    private static void ReplaceText(string replaceText, bool validationResult, Document document)
    {
        foreach (var text in document.Descendants<Text>())
        {
            if(text.Text.Contains(replaceText))
                text.Text = text.Text.Replace(replaceText, validationResult ? "Успешно" : "Не успешно");
            document.Save();
        }
    }
}