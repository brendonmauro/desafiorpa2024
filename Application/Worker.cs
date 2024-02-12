using Classes.Models;
using Classes;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Domain.Models;
using DataAccess;
using Npgsql;

namespace DesafioProgram
{
    public class Worker : BackgroundService
    {
        private int _qtdThread = 2;
        private string _url = "https://10fastfingers.com/typing-test/portuguese";
        private string _connectionString = "Host=localhost;Port=5432;Database=desafiorpa;Username=postgres;Password=root;";

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    Task[] tasks = new Task[_qtdThread];
                    for (int i = 0; i < _qtdThread; i++)
                    {
                        tasks[i] = Task.Factory.StartNew(() =>
                        {
                            Thread.CurrentThread.Priority = ThreadPriority.Highest;
                            DoWork(i, stoppingToken);
                        });
                    }


                    Task.WaitAll(tasks);
                }
                catch (Exception e)
                {
                    SaveLog(e.Message);
                }

                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }

        private void DoWork(int idxThread, CancellationToken stoppingToken)
        {
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    ChromeOptions options = new ChromeOptions();
                    options.AddArguments("--disable-notifications");
                    options.AddArgument("--headless");

                    var driver = new ChromeDriver(options);
                    var isPageClean = false;

                    try
                    {
                        while (true)
                        {
                            NavigateToPage(driver);
                            if (!isPageClean) CleanPage(driver);
                            isPageClean = true;
                            ReadAndWrite(driver);
                            WaitForResult(driver);
                            SaveResult(ReadResult(driver));
                            RefreshPage(driver);
                        }
                    }
                    catch (Exception e)
                    {
                        throw new Exception(e.Message);
                    }
                    finally
                    {
                        driver.Quit();
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception($"{MethodBase.GetCurrentMethod()?.Name} -> Thread {idxThread} ->{e.Message}");
            }
        }

        private void NavigateToPage(ChromeDriver driver)
        {
            try
            {
                if (!driver.Url.Equals(_url))
                    driver.Navigate().GoToUrl(_url);
            }
            catch (Exception e)
            {
                var msg = "Site não responde no momento, continuaremos tentando, exceção: " + e.Message;
                throw new Exception(msg);
            }
        }

        private void CleanPage(ChromeDriver driver)
        {
            try
            {
                driver.FindElement(By.Id("closeIconHit")).Click();

                Utils utils = new();

                utils.RemoveAds("fs-sticky-footer", driver);
                utils.RemoveAds("CybotMultilevel", driver);

            }
            catch (Exception e)
            {
                var msg = "Erro ao tentar remover anúncios ou popups, detalhes: " + e.Message;
                throw new Exception(msg);
            }

        }

        private void ReadAndWrite(ChromeDriver driver)
        {
            string? word;
            IWebElement input;
            Thread.Sleep(3000);

            do
            {
                try
                {
                    word = driver.FindElement(By.ClassName("highlight")).Text;
                    input = driver.FindElement(By.Id("inputfield"));

                    input.SendKeys(word);
                    input.SendKeys(Keys.Space);
                }
                catch { word = string.Empty; }
            } while (!string.IsNullOrEmpty(word));
        }

        private void WaitForResult(ChromeDriver driver)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));
            var element = wait.Until(condition =>
            {
                try
                {
                    driver.SwitchTo().Alert().Dismiss();
                }
                catch (NoAlertPresentException) { }

                try
                {
                    var elementToBeDisplayed = driver.FindElement(By.Id("auswertung-result"));
                    var isZeroSeconds = driver.FindElement(By.Id("timer")).Text.Equals("0:00");
                    return elementToBeDisplayed.Displayed && isZeroSeconds;
                }
                catch (StaleElementReferenceException)
                {
                    return false;
                }
                catch (NoSuchElementException)
                {
                    return false;
                }
            });
        }

        private ResultModel ReadResult(ChromeDriver driver)
        {
            ResultModel resultado = new ResultModel();

            resultado.Wpm = Convert.ToInt32(driver.FindElement(By.CssSelector("td[id*=wpm] strong")).Text.Split(' ')[0]);
            var keystrokesCorrect = Convert.ToInt32(driver.FindElement(By.CssSelector("tr[id*=keystrokes] td[class*=value] small span[class*=correct]")).Text);
            var keystrokesWrong = Convert.ToInt32(driver.FindElement(By.CssSelector("tr[id*=keystrokes] td[class*=value] small span[class*=wrong]")).Text);
            resultado.Keystrokes = keystrokesCorrect - keystrokesWrong;
            resultado.Accuracy = Convert.ToInt32(driver.FindElement(By.CssSelector("tr[id*=accuracy] td[class*=value] strong")).Text.Split('%')[0]);
            resultado.CorrectWords = Convert.ToInt32(driver.FindElement(By.CssSelector("tr[id*=correct] strong")).Text);
            resultado.WrongWords = Convert.ToInt32(driver.FindElement(By.CssSelector("tr[id*=wrong] strong")).Text);

            return resultado;
        }

        private void SaveResult(ResultModel result)
        {
            NpgsqlConnection connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            ResultDB resultDb = new();
            resultDb.Insert(result, connection);
            connection.Close();
        }

        private void SaveLog(string text)
        {
            LogModel log = new();
            log.Text = text;
            log.Date = DateTime.Now;

            NpgsqlConnection connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            LogDb logDb = new();
            logDb.Insert(log, connection);
            connection.Close();
        }

        private void RefreshPage(ChromeDriver driver)
        {
            try
            {
                try
                {
                    driver.SwitchTo().Alert().Dismiss();
                }
                catch (NoAlertPresentException) { }


                var refreshButton = driver.FindElement(By.Id("reload-btn"));
                Thread.Sleep(3000);
                refreshButton.Click();
            }
            catch (Exception e)
            {
                var msg = "Erro ao tentar atualizar página, detalhes: " + e.Message;
                throw new Exception(msg);
            }
        }
    }
}
